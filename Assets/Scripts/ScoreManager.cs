using UnityEngine;
using TMPro;
using System.IO;

[System.Serializable]
public class ScoreData
{
    public int highScore;
}

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public int score = 0;
    public int highScore = 0;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    private string savePath;
    private int loadedHighScore;

    public bool HasNewHighScore => score > loadedHighScore;

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
        savePath = Path.Combine(Application.persistentDataPath, "scoreData.json");
        LoadScore();
        UpdateScoreUI();
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (score > highScore)
        {
            highScore = score;
            SaveScore();
        }
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "점수 : " + score;
        if (highScoreText != null)
            highScoreText.text = "최고 점수 : " + highScore;
    }

    public void PersistHighScore()
    {
        SaveScore();
    }

    void SaveScore()
    {
        if (string.IsNullOrEmpty(savePath))
            return;

        ScoreData data = new ScoreData();
        data.highScore = highScore;
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    void LoadScore()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            ScoreData data = JsonUtility.FromJson<ScoreData>(json);
            highScore = data.highScore;
            loadedHighScore = highScore;
        }
        else
        {
            highScore = 0;
            loadedHighScore = 0;
        }
    }

    private void OnApplicationQuit()
    {
        SaveScore();
    }
}
