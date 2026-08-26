using UnityEngine;
using TMPro;
using System.IO;

[System.Serializable]
public class ScoreData
{
    public int highScore;
    public int bestCombo;
    public float bestClearTime;
}

public static class ScoreSave
{
    public const string FileName = "scoreData.json";

    public static string GetPath()
    {
        return Path.Combine(Application.persistentDataPath, FileName);
    }

    public static ScoreData Load()
    {
        string path = GetPath();
        if (!File.Exists(path))
            return new ScoreData();

        string json = File.ReadAllText(path);
        ScoreData data = JsonUtility.FromJson<ScoreData>(json);
        return data != null ? data : new ScoreData();
    }

    public static void Save(ScoreData data)
    {
        if (data == null)
            return;

        File.WriteAllText(GetPath(), JsonUtility.ToJson(data, true));
    }

    public static string FormatTime(float seconds)
    {
        if (seconds < 0f)
            seconds = 0f;

        int minutes = Mathf.FloorToInt(seconds / 60f);
        float remainder = seconds - minutes * 60f;
        return string.Format("{0}:{1:00.0}", minutes, remainder);
    }

    public static string FormatClearRecord(float seconds)
    {
        if (seconds <= 0f)
            return "-";

        return FormatTime(seconds);
    }
}

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public int score = 0;
    public int highScore = 0;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI runTimeText;
    public float comboWindow = 2.5f;
    public int maxMultiplier = 5;
    public Color comboColor = new Color(1f, 0.82f, 0.22f, 1f);
    public Vector2 comboTextOffset = new Vector2(0f, -44f);
    public Vector2 runTimeTextOffset = new Vector2(0f, -88f);
    public string scoreFormat = "점수 : {0}";
    public string highScoreFormat = "최고 점수 : {0}";
    public string comboFormat = "{0} COMBO";
    public string comboCappedFormat = "{0} COMBO  x{1}";
    public string runTimeFormat = "{0}";

    private string savePath;
    private int loadedHighScore;
    private int loadedBestCombo;
    private float loadedBestClearTime;
    private int combo;
    private int bestCombo;
    private int bestComboThisRun;
    private float lastKillTime = -999f;
    private float runStartTime;
    private float finalRunTime;
    private float bestClearTime;
    private bool runStopped;
    private bool missionCleared;

    public int Combo => combo;
    public int BestComboThisRun => bestComboThisRun;
    public int BestCombo => bestCombo;
    public float RunTime => runStopped ? finalRunTime : Mathf.Max(0f, Time.time - runStartTime);
    public float BestClearTime => bestClearTime;
    public bool MissionCleared => missionCleared;
    public bool HasNewHighScore => score > loadedHighScore;
    public bool HasNewBestCombo => bestComboThisRun > loadedBestCombo;
    public bool HasNewBestClear { get; private set; }
    public bool HasAnyNewRecord => HasNewHighScore || HasNewBestCombo || HasNewBestClear;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        savePath = ScoreSave.GetPath();
        LoadScore();
        EnsureHudLabels();
        runStartTime = Time.time;
        UpdateScoreUI();
        UpdateHud();
    }

    void Update()
    {
        if (combo > 0 && Time.time - lastKillTime > comboWindow)
            combo = 0;

        UpdateHud();
    }

    public void RegisterKill(int baseScore)
    {
        if (Time.time - lastKillTime > comboWindow)
            combo = 0;

        combo++;
        lastKillTime = Time.time;

        if (combo > bestComboThisRun)
            bestComboThisRun = combo;
        if (bestComboThisRun > bestCombo)
            bestCombo = bestComboThisRun;

        int multiplier = Mathf.Min(combo, maxMultiplier);
        AddScore(baseScore * multiplier);
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (score > highScore)
            highScore = score;
        UpdateScoreUI();
    }

    public void StopRun(bool cleared)
    {
        if (runStopped)
            return;

        runStopped = true;
        missionCleared = cleared;
        finalRunTime = Mathf.Max(0f, Time.time - runStartTime);
        HasNewBestClear = cleared && (loadedBestClearTime <= 0f || finalRunTime < loadedBestClearTime);
        if (HasNewBestClear)
            bestClearTime = finalRunTime;
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = string.Format(scoreFormat, score);
        if (highScoreText != null)
            highScoreText.text = string.Format(highScoreFormat, highScore);
    }

    void UpdateHud()
    {
        if (comboText != null)
        {
            bool showCombo = combo >= 2;
            comboText.gameObject.SetActive(showCombo);
            if (showCombo)
            {
                if (combo > maxMultiplier)
                    comboText.text = string.Format(comboCappedFormat, combo, maxMultiplier);
                else
                    comboText.text = string.Format(comboFormat, combo);
            }
        }

        if (runTimeText != null)
            runTimeText.text = string.Format(runTimeFormat, ScoreSave.FormatTime(RunTime));
    }

    public void PersistHighScore()
    {
        SaveScore();
    }

    void EnsureHudLabels()
    {
        if (comboText == null)
            comboText = CreateHudLabel("ComboText", comboTextOffset, comboColor);
        if (runTimeText == null)
            runTimeText = CreateHudLabel("RunTimeText", runTimeTextOffset, Color.white);
    }

    TextMeshProUGUI CreateHudLabel(string name, Vector2 offset, Color color)
    {
        if (scoreText == null)
            return null;

        RectTransform source = scoreText.rectTransform;
        GameObject labelObject = new GameObject(name, typeof(RectTransform));
        labelObject.layer = scoreText.gameObject.layer;
        labelObject.transform.SetParent(source.parent, false);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = source.anchorMin;
        rect.anchorMax = source.anchorMax;
        rect.pivot = source.pivot;
        rect.sizeDelta = source.sizeDelta;
        rect.anchoredPosition = source.anchoredPosition + offset;

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.font = scoreText.font;
        label.fontSize = scoreText.fontSize;
        label.alignment = scoreText.alignment;
        label.color = color;
        label.raycastTarget = false;
        label.enableWordWrapping = false;
        return label;
    }

    void SaveScore()
    {
        if (string.IsNullOrEmpty(savePath))
            savePath = ScoreSave.GetPath();

        ScoreData data = new ScoreData();
        data.highScore = highScore;
        data.bestCombo = bestCombo;
        data.bestClearTime = bestClearTime;
        ScoreSave.Save(data);
    }

    void LoadScore()
    {
        ScoreData data = ScoreSave.Load();
        highScore = data.highScore;
        bestCombo = data.bestCombo;
        bestClearTime = data.bestClearTime;
        loadedHighScore = highScore;
        loadedBestCombo = bestCombo;
        loadedBestClearTime = bestClearTime;
    }

    private void OnApplicationQuit()
    {
        SaveScore();
    }
}
