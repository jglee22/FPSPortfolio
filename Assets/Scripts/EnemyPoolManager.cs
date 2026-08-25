using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class EnemyPoolManager : MonoBehaviour
{
    public Transform enemyContainer;
    public WaveData[] waves;
    public Transform[] spawnPoints;
    public int defaultPoolSize = 8;

    public int waveNumber = 1;
    public TextMeshProUGUI waveText;
    public float waveDelay = 5f;
    public float waveClearBannerDuration = 1.8f;
    public float waveStartBannerDuration = 1.8f;
    public float waveBannerFontSize = 88f;
    public float waveBannerPunchScale = 1.12f;
    public string waveClearFormat = "WAVE {0} CLEAR";
    public string waveStartFormat = "WAVE {0}";
    public string bossWarningText = "WARNING";
    public string bossWaveStartText = "BOSS WAVE";
    public string missionClearText = "MISSION CLEAR";
    public Color waveBannerColor = Color.white;
    public Color bossBannerColor = new Color(1f, 0.28f, 0.22f, 1f);
    public string bossHealthLabel = "BOSS";
    public string bossHealthFormat = "{0} / {1}";
    public Vector2 bossHealthBarOffset = new Vector2(0f, -24f);
    public Vector2 bossHealthBarSize = new Vector2(1000f, 84f);
    public float bossHealthFillHeight = 42f;
    public float bossHealthLabelSize = 36f;
    public float bossHealthValueSize = 36f;
    public Color bossHealthBackColor = new Color(0.08f, 0.08f, 0.08f, 0.82f);
    public float hudTextOutlineWidth = 0.25f;
    public Color hudTextOutlineColor = new Color(0f, 0f, 0f, 0.9f);

    [Header("Test")]
    public bool spawnBossOnStart = false;
    public bool spawnTestBoss = false;

    private bool isWaveActive = false;
    private bool isSpawning = false;
    private bool isRunComplete = false;
    private int waveIndex = 0;
    private int startWaveNumber;
    private int enemiesAlive = 0;
    private PlayerHealth playerHealth;
    private Dictionary<string, Queue<GameObject>> enemyPools;
    private Dictionary<string, EnemyData> enemyCatalog;
    private TextMeshProUGUI bannerText;
    private CanvasGroup bannerGroup;
    private Tween bannerTween;
    private Coroutine nextWaveRoutine;
    private EnemyAI currentBoss;
    private GameObject bossHealthRoot;
    private Image bossHealthFill;
    private TextMeshProUGUI bossHealthText;
    private TextMeshProUGUI bossHealthNameText;

    void Start()
    {
        startWaveNumber = waveNumber;
        enemyPools = new Dictionary<string, Queue<GameObject>>();
        enemyCatalog = new Dictionary<string, EnemyData>();
        playerHealth = FindObjectOfType<PlayerHealth>();
        CreateWaveBanner();
        CreateBossHealthBar();
        ApplyCanvasHudOutlines();

        if (waves == null || waves.Length == 0)
        {
            Debug.LogError("EnemyPoolManager: waves 배열이 비어 있습니다.");
            return;
        }

        BuildPoolsFromWaves();
        waveIndex = spawnBossOnStart ? FindBossWaveIndex() : 0;
        waveNumber = waveIndex + 1;
        StartCoroutine(SpawnCurrentWave());
    }

    void Update()
    {
        if (spawnTestBoss)
        {
            spawnTestBoss = false;
            SpawnTestBoss();
        }

        if (IsPlayerDead() || isRunComplete)
            return;

        if (isWaveActive && !isSpawning && enemiesAlive <= 0 && nextWaveRoutine == null)
            nextWaveRoutine = StartCoroutine(StartNextWave());
    }

    IEnumerator StartNextWave()
    {
        isWaveActive = false;
        int clearedWave = waveNumber;
        bool isLastWave = waveIndex >= waves.Length - 1;

        if (isLastWave)
        {
            ShowBanner(missionClearText, waveBannerColor, waveClearBannerDuration);
            yield return new WaitForSeconds(waveClearBannerDuration);

            MenuManager menuManager = FindObjectOfType<MenuManager>();
            if (menuManager != null)
                menuManager.ShowMissionClear();

            isRunComplete = true;
            nextWaveRoutine = null;
            yield break;
        }

        WaveData nextWave = waves[waveIndex + 1];
        bool nextIsBoss = nextWave != null && nextWave.isBossWave;
        float delay = GetWaveDelay(nextWave);

        ShowBanner(string.Format(waveClearFormat, clearedWave), waveBannerColor, waveClearBannerDuration);
        yield return WaitUnlessDead(waveClearBannerDuration);
        if (IsPlayerDead())
        {
            nextWaveRoutine = null;
            yield break;
        }

        yield return ShowWaveReward();
        if (IsPlayerDead())
        {
            nextWaveRoutine = null;
            yield break;
        }

        if (nextIsBoss)
        {
            ShowBanner(bossWarningText, bossBannerColor, delay);
            yield return WaitUnlessDead(delay);
        }
        else
        {
            yield return WaitUnlessDead(delay);
        }

        if (IsPlayerDead())
        {
            nextWaveRoutine = null;
            yield break;
        }

        waveIndex++;
        waveNumber = waveIndex + 1;
        yield return SpawnCurrentWave();
        nextWaveRoutine = null;
    }

    IEnumerator ShowWaveReward()
    {
        WaveRewardUI rewardUI = GetComponent<WaveRewardUI>();
        if (rewardUI == null)
            yield break;

        yield return rewardUI.ShowChoices();
    }

    IEnumerator WaitUnlessDead(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (IsPlayerDead())
                yield break;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator SpawnCurrentWave()
    {
        WaveData wave = GetCurrentWave();
        isWaveActive = true;
        isSpawning = true;
        UpdateWaveUI();
        ShowWaveStartBanner();

        if (wave == null || wave.enemies == null)
        {
            Debug.LogError("EnemyPoolManager: 현재 WaveData가 비어 있습니다.");
            isSpawning = false;
            yield break;
        }

        float interval = Mathf.Max(0f, wave.spawnInterval);
        bool spawnedAny = false;

        for (int i = 0; i < wave.enemies.Length; i++)
        {
            WaveEnemyEntry entry = wave.enemies[i];
            if (entry == null || entry.enemyData == null)
                continue;

            for (int n = 0; n < entry.count; n++)
            {
                if (IsPlayerDead())
                {
                    isSpawning = false;
                    yield break;
                }

                if (spawnedAny && interval > 0f)
                    yield return WaitUnlessDead(interval);

                if (IsPlayerDead())
                {
                    isSpawning = false;
                    yield break;
                }

                SpawnEnemy(entry.enemyData, wave);
                spawnedAny = true;
            }
        }

        isSpawning = false;
    }

    void SpawnEnemy(EnemyData enemyData, WaveData waveData)
    {
        if (enemyData == null || enemyData.prefab == null)
        {
            Debug.LogError("EnemyPoolManager: EnemyData 또는 prefab이 없습니다.");
            return;
        }

        string key = GetPoolKey(enemyData);
        if (!enemyPools.ContainsKey(key))
            CreatePool(enemyData);

        if (enemyPools[key].Count == 0)
            ExpandPool(enemyData);

        int spawnIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[spawnIndex];

        GameObject enemy = enemyPools[key].Dequeue();
        enemy.transform.position = spawnPoint.position;
        enemy.transform.rotation = spawnPoint.rotation;
        enemy.SetActive(true);
        enemiesAlive++;

        if (EnemyCounterManager.Instance != null)
            EnemyCounterManager.Instance.AddEnemy();

        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.enemyType = key;
            enemyAI.InitializeForSpawn(enemyData, waveData);
            enemyAI.OnDeath -= EnemyDied;
            enemyAI.OnDeath += EnemyDied;

            if (enemyData.isBoss)
                ShowBossHealth(enemyAI);
        }
    }

    void BuildPoolsFromWaves()
    {
        for (int i = 0; i < waves.Length; i++)
        {
            WaveData wave = waves[i];
            if (wave == null || wave.enemies == null)
                continue;

            for (int e = 0; e < wave.enemies.Length; e++)
            {
                EnemyData enemyData = wave.enemies[e] != null ? wave.enemies[e].enemyData : null;
                if (enemyData == null)
                    continue;

                string key = GetPoolKey(enemyData);
                if (enemyCatalog.ContainsKey(key))
                    continue;

                enemyCatalog[key] = enemyData;
                CreatePool(enemyData);
            }
        }
    }

    void CreatePool(EnemyData enemyData)
    {
        string key = GetPoolKey(enemyData);
        if (enemyPools.ContainsKey(key))
            return;

        Queue<GameObject> pool = new Queue<GameObject>();
        int poolSize = Mathf.Max(1, defaultPoolSize);
        for (int i = 0; i < poolSize; i++)
            pool.Enqueue(CreatePooledEnemy(enemyData));

        enemyPools[key] = pool;
    }

    void ExpandPool(EnemyData enemyData)
    {
        string key = GetPoolKey(enemyData);
        for (int i = 0; i < 5; i++)
            enemyPools[key].Enqueue(CreatePooledEnemy(enemyData));
    }

    GameObject CreatePooledEnemy(EnemyData enemyData)
    {
        GameObject enemy = Instantiate(enemyData.prefab, enemyContainer);
        enemy.SetActive(false);
        return enemy;
    }

    string GetPoolKey(EnemyData enemyData)
    {
        if (enemyData == null)
            return string.Empty;

        return string.IsNullOrEmpty(enemyData.enemyName) ? enemyData.name : enemyData.enemyName;
    }

    WaveData GetCurrentWave()
    {
        if (waves == null || waveIndex < 0 || waveIndex >= waves.Length)
            return null;

        return waves[waveIndex];
    }

    float GetWaveDelay(WaveData wave)
    {
        if (wave != null && wave.waveDelay > 0f)
            return wave.waveDelay;

        return waveDelay;
    }

    int FindBossWaveIndex()
    {
        for (int i = 0; i < waves.Length; i++)
        {
            if (waves[i] != null && waves[i].isBossWave)
                return i;
        }

        return Mathf.Max(0, waves.Length - 1);
    }

    void UpdateWaveUI()
    {
        if (waveText == null)
            return;

        waveText.text = string.Format(waveStartFormat, waveNumber);
    }

    void ShowWaveStartBanner()
    {
        WaveData wave = GetCurrentWave();
        if (wave != null && wave.isBossWave)
            ShowBanner(bossWaveStartText, bossBannerColor, waveStartBannerDuration);
        else
            ShowBanner(string.Format(waveStartFormat, waveNumber), waveBannerColor, waveStartBannerDuration);
    }

    [ContextMenu("Spawn Test Boss")]
    public void SpawnTestBoss()
    {
        if (enemyCatalog == null || spawnPoints == null || spawnPoints.Length == 0)
            return;

        if (IsPlayerDead())
            return;

        EnemyData bossData = null;
        foreach (KeyValuePair<string, EnemyData> pair in enemyCatalog)
        {
            if (pair.Value != null && pair.Value.isBoss)
            {
                bossData = pair.Value;
                break;
            }
        }

        if (bossData == null)
            return;

        SpawnEnemy(bossData, GetCurrentWave());
    }

    bool IsPlayerDead()
    {
        return playerHealth != null && playerHealth.isPlayerDie;
    }

    void EnemyDied()
    {
        enemiesAlive--;
        if (EnemyCounterManager.Instance != null)
            EnemyCounterManager.Instance.RemoveEnemy();
    }

    public void ReturnToPool(GameObject enemy, string type)
    {
        enemy.SetActive(false);
        enemyPools[type].Enqueue(enemy);
    }

    void ShowBanner(string message, Color color, float duration)
    {
        if (bannerText == null || bannerGroup == null)
            return;

        bannerTween?.Kill();
        bannerText.text = message;
        bannerText.color = color;
        bannerGroup.alpha = 0f;
        bannerText.transform.localScale = Vector3.one * 0.86f;

        float hold = Mathf.Max(0.35f, duration - 0.7f);
        bannerTween = DOTween.Sequence()
            .Append(bannerGroup.DOFade(1f, 0.18f))
            .Join(bannerText.transform.DOScale(waveBannerPunchScale, 0.22f).SetEase(Ease.OutBack))
            .Append(bannerText.transform.DOScale(1f, 0.12f).SetEase(Ease.OutQuad))
            .AppendInterval(hold)
            .Append(bannerGroup.DOFade(0f, 0.4f))
            .Join(bannerText.transform.DOScale(1.08f, 0.4f).SetEase(Ease.InQuad));
    }

    void CreateWaveBanner()
    {
        if (waveText == null)
            return;

        Transform canvasTransform = waveText.canvas != null ? waveText.canvas.transform : waveText.transform.parent;
        GameObject bannerObject = new GameObject("WaveBanner");
        bannerObject.transform.SetParent(canvasTransform, false);
        bannerObject.transform.SetAsLastSibling();

        RectTransform rect = bannerObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(1400f, 180f);

        bannerGroup = bannerObject.AddComponent<CanvasGroup>();
        bannerGroup.alpha = 0f;
        bannerGroup.blocksRaycasts = false;
        bannerGroup.interactable = false;

        bannerText = bannerObject.AddComponent<TextMeshProUGUI>();
        bannerText.font = waveText.font;
        bannerText.fontSize = waveBannerFontSize;
        bannerText.alignment = TextAlignmentOptions.Center;
        bannerText.raycastTarget = false;
        bannerText.fontStyle = FontStyles.Bold;
        ApplyHudOutline(bannerText);
    }

    void CreateBossHealthBar()
    {
        if (waveText == null)
            return;

        Transform canvasTransform = waveText.canvas != null ? waveText.canvas.transform : waveText.transform.parent;
        bossHealthRoot = new GameObject("BossHealth");
        bossHealthRoot.layer = waveText.gameObject.layer;
        bossHealthRoot.transform.SetParent(canvasTransform, false);
        bossHealthRoot.transform.SetAsLastSibling();

        RectTransform rootRect = bossHealthRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.anchoredPosition = bossHealthBarOffset;
        rootRect.sizeDelta = bossHealthBarSize;

        CanvasGroup group = bossHealthRoot.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        bossHealthNameText = CreateBossLabel("Label", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 40f), bossHealthLabelSize, TextAlignmentOptions.Bottom);
        bossHealthNameText.text = bossHealthLabel;
        bossHealthNameText.color = bossBannerColor;

        GameObject barObject = CreateBossChild("Bar");
        RectTransform barRect = barObject.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 0f);
        barRect.anchorMax = new Vector2(1f, 0f);
        barRect.pivot = new Vector2(0.5f, 0f);
        barRect.anchoredPosition = Vector2.zero;
        barRect.sizeDelta = new Vector2(0f, bossHealthFillHeight);

        Image barBackground = barObject.AddComponent<Image>();
        barBackground.color = bossHealthBackColor;
        barBackground.raycastTarget = false;

        GameObject fillObject = CreateBossChild("Fill", barObject.transform);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);

        bossHealthFill = fillObject.AddComponent<Image>();
        bossHealthFill.color = bossBannerColor;
        bossHealthFill.raycastTarget = false;

        bossHealthText = CreateBossLabel("Value", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, bossHealthValueSize, TextAlignmentOptions.Center, barObject.transform);

        bossHealthRoot.SetActive(false);
    }

    GameObject CreateBossChild(string name, Transform parent = null)
    {
        GameObject child = new GameObject(name, typeof(RectTransform));
        child.layer = bossHealthRoot.layer;
        child.transform.SetParent(parent != null ? parent : bossHealthRoot.transform, false);
        return child;
    }

    TextMeshProUGUI CreateBossLabel(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment, Transform parent = null)
    {
        GameObject labelObject = CreateBossChild(name, parent);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        if (waveText != null)
            label.font = waveText.font;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = Color.white;
        label.raycastTarget = false;
        label.enableWordWrapping = false;
        ApplyHudOutline(label);
        return label;
    }

    void ShowBossHealth(EnemyAI boss)
    {
        if (boss == null || bossHealthRoot == null || !boss.IsBoss)
            return;

        HideBossHealthBindings();
        currentBoss = boss;
        currentBoss.OnHealthChanged += UpdateBossHealthBar;
        currentBoss.OnDeath += OnBossDied;

        bossHealthRoot.SetActive(true);
        if (bossHealthNameText != null)
            bossHealthNameText.text = bossHealthLabel;

        UpdateBossHealthBar(currentBoss.CurrentHealth, currentBoss.MaxHealth);
    }

    void UpdateBossHealthBar(int current, int max)
    {
        float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
        if (bossHealthFill != null)
        {
            bossHealthFill.gameObject.SetActive(ratio > 0f);
            RectTransform fillRect = bossHealthFill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(ratio, 1f);
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
        }

        if (bossHealthText != null)
            bossHealthText.text = string.Format(bossHealthFormat, current, max);
    }

    void OnBossDied()
    {
        HideBossHealth();
    }

    void HideBossHealth()
    {
        HideBossHealthBindings();

        if (bossHealthRoot != null)
            bossHealthRoot.SetActive(false);
    }

    void HideBossHealthBindings()
    {
        if (currentBoss != null)
        {
            currentBoss.OnHealthChanged -= UpdateBossHealthBar;
            currentBoss.OnDeath -= OnBossDied;
            currentBoss = null;
        }
    }

    void ApplyCanvasHudOutlines()
    {
        if (waveText == null || waveText.canvas == null)
            return;

        TextMeshProUGUI[] labels = waveText.canvas.GetComponentsInChildren<TextMeshProUGUI>(false);
        for (int i = 0; i < labels.Length; i++)
            ApplyHudOutline(labels[i]);
    }

    void ApplyHudOutline(TextMeshProUGUI label)
    {
        if (label == null || label.font == null || label.fontSharedMaterial == null)
            return;

        label.outlineWidth = hudTextOutlineWidth;
        label.outlineColor = hudTextOutlineColor;
        label.fontMaterial.EnableKeyword("OUTLINE_ON");
        label.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, hudTextOutlineWidth);
        label.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, hudTextOutlineColor);
    }

    void OnDestroy()
    {
        bannerTween?.Kill();
        HideBossHealthBindings();
    }
}
