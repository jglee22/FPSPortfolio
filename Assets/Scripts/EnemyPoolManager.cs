using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class EnemyPoolManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemyType
    {
        public string name;
        public GameObject prefab;
        public int poolSize = 10;
        public bool isBoss = false;
    }

    public Transform enemyContainer;
    public EnemyType[] enemyTypes;
    public Transform[] spawnPoints;

    public int waveNumber = 1;
    public int enemiesPerWave = 5;
    public int bossWaveInterval = 3;
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
    private int startWaveNumber;
    private int enemiesAlive = 0;
    private PlayerHealth playerHealth;
    private Dictionary<string, Queue<GameObject>> enemyPools;
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
        playerHealth = FindObjectOfType<PlayerHealth>();
        CreateWaveBanner();
        CreateBossHealthBar();
        ApplyCanvasHudOutlines();

        foreach (EnemyType enemyType in enemyTypes)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < enemyType.poolSize; i++)
            {
                GameObject enemy = Instantiate(enemyType.prefab);
                enemy.SetActive(false);
                pool.Enqueue(enemy);
            }
            enemyPools[enemyType.name] = pool;
        }

        SpawnWave();
        UpdateWaveUI();
        isWaveActive = true;
        ShowWaveStartBanner();
    }

    void Update()
    {
        if (spawnTestBoss)
        {
            spawnTestBoss = false;
            SpawnTestBoss();
        }

        if (IsPlayerDead())
            return;

        if (isWaveActive && enemiesAlive <= 0 && nextWaveRoutine == null)
            nextWaveRoutine = StartCoroutine(StartNextWave());
    }

    IEnumerator StartNextWave()
    {
        isWaveActive = false;
        int clearedWave = waveNumber;
        bool nextIsBoss = IsBossWave(clearedWave + 1);

        ShowBanner(string.Format(waveClearFormat, clearedWave), waveBannerColor, waveClearBannerDuration);

        if (nextIsBoss && waveClearBannerDuration < waveDelay)
        {
            yield return WaitUnlessDead(waveClearBannerDuration);
            if (IsPlayerDead())
            {
                nextWaveRoutine = null;
                yield break;
            }

            ShowBanner(bossWarningText, bossBannerColor, waveDelay - waveClearBannerDuration);
            yield return WaitUnlessDead(waveDelay - waveClearBannerDuration);
        }
        else
        {
            yield return WaitUnlessDead(waveDelay);
        }

        if (IsPlayerDead())
        {
            nextWaveRoutine = null;
            yield break;
        }

        waveNumber++;
        enemiesPerWave += 2;

        foreach (EnemyType enemyType in enemyTypes)
            enemyType.poolSize += 5;

        SpawnWave();
        isWaveActive = true;
        UpdateWaveUI();
        ShowWaveStartBanner();
        nextWaveRoutine = null;
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

    void SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            bool spawnBoss = IsBossWave(waveNumber) && i == enemiesPerWave - 1;
            SpawnEnemy(spawnBoss);
        }

        enemiesAlive = enemiesPerWave;
    }

    void SpawnEnemy(bool isBoss)
    {
        EnemyType selectedEnemy;

        if (isBoss)
        {
            selectedEnemy = System.Array.Find(enemyTypes, e => e.isBoss);
            if (selectedEnemy == null)
                selectedEnemy = enemyTypes[0];
        }
        else
        {
            int enemyTypeIndex = Random.Range(0, enemyTypes.Length);
            selectedEnemy = enemyTypes[enemyTypeIndex];
            int attempts = 0;
            while (selectedEnemy.isBoss && attempts < enemyTypes.Length)
            {
                enemyTypeIndex = (enemyTypeIndex + 1) % enemyTypes.Length;
                selectedEnemy = enemyTypes[enemyTypeIndex];
                attempts++;
            }
        }

        if (enemyPools[selectedEnemy.name].Count == 0)
            ExpandPool(selectedEnemy);

        int spawnIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[spawnIndex];

        GameObject enemy = enemyPools[selectedEnemy.name].Dequeue();
        enemy.transform.position = spawnPoint.position;
        enemy.transform.rotation = spawnPoint.rotation;
        enemy.SetActive(true);

        if (EnemyCounterManager.Instance != null)
            EnemyCounterManager.Instance.AddEnemy();

        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.enemyType = selectedEnemy.name;
            enemyAI.InitializeForSpawn(waveNumber, isBoss);
            enemyAI.OnDeath -= EnemyDied;
            enemyAI.OnDeath += EnemyDied;

            if (isBoss)
                ShowBossHealth(enemyAI);
        }
    }

    void ExpandPool(EnemyType enemyType)
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject enemy = Instantiate(enemyType.prefab, enemyContainer);
            enemy.SetActive(false);
            enemyPools[enemyType.name].Enqueue(enemy);
        }
    }

    void UpdateWaveUI()
    {
        if (waveText == null)
            return;

        waveText.text = string.Format(waveStartFormat, waveNumber);
    }

    void ShowWaveStartBanner()
    {
        if (IsBossWave(waveNumber))
            ShowBanner(bossWaveStartText, bossBannerColor, waveStartBannerDuration);
        else
            ShowBanner(string.Format(waveStartFormat, waveNumber), waveBannerColor, waveStartBannerDuration);
    }

    bool IsBossWave(int wave)
    {
        if (spawnBossOnStart && wave == startWaveNumber)
            return true;

        return bossWaveInterval > 0 && wave > 0 && wave % bossWaveInterval == 0;
    }

    [ContextMenu("Spawn Test Boss")]
    public void SpawnTestBoss()
    {
        if (enemyPools == null || enemyTypes == null || enemyTypes.Length == 0)
            return;

        if (spawnPoints == null || spawnPoints.Length == 0)
            return;

        if (IsPlayerDead())
            return;

        SpawnEnemy(true);
        enemiesAlive++;
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
