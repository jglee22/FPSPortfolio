using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

[System.Serializable]
public class ScoreData
{
    public int highScore; // �ְ��� ����
}

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance; // �̱��� �ν��Ͻ�
    public int score = 0;                // ���� ����
    public int highScore = 0;    // �ְ���
    public TextMeshProUGUI scoreText;    // ���� ����
    public TextMeshProUGUI highScoreText; // �ְ� ����

    private string savePath;     // ���� ���

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
        // ���� ��� ����
        savePath = Path.Combine(Application.persistentDataPath, "scoreData.json");

        // ����� ������ �ε�
        LoadScore();

        // �ְ��� UI ǥ��
        Debug.Log($"�ְ� ���� : {highScore}");
        UpdateScoreUI();
    }

    public void AddScore(int amount)
    {
        score += amount; // ���� ����
        if (score > highScore)
        {
            highScore = score;
        }
        UpdateScoreUI(); // UI ������Ʈ
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "���� ����: " + score;
            highScoreText.text = $"�ְ� ���� : {highScore}";

            if (highScore <= score)
                highScoreText.text = $"�ְ� ���� : {highScore}";
        }
        SaveScore();
    }

    void SaveScore()
    {
        // ������ ����
        ScoreData data = new ScoreData();
        data.highScore = highScore;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"���� ���� �Ϸ�: {savePath}");
    }

    void LoadScore()
    {
        // ������ �����ϸ� ������ �ҷ�����
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            ScoreData data = JsonUtility.FromJson<ScoreData>(json);

            // �ְ��� �ҷ�����
            highScore = data.highScore;
            highScoreText.text = $"�ְ� ���� : {highScore}";
        }
        else
        {
            Debug.Log("����� ������ �����ϴ�. �ʱ�ȭ�մϴ�.");
            highScore = 0;
        }
    }

    private void OnApplicationQuit()
    {
        SaveScore();
    }
}
