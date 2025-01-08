using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

[System.Serializable]
public class ScoreData
{
    public int highScore; // 최고점 저장
}

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance; // 싱글톤 인스턴스
    public int score = 0;                // 현재 점수
    public int highScore = 0;    // 최고점
    public TextMeshProUGUI scoreText;    // 현재 점수
    public TextMeshProUGUI highScoreText; // 최고 점수

    private string savePath;     // 저장 경로

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
        // 저장 경로 설정
        savePath = Path.Combine(Application.persistentDataPath, "scoreData.json");

        // 저장된 데이터 로드
        LoadScore();

        // 최고점 UI 표시
        Debug.Log($"최고 점수 : {highScore}");
        UpdateScoreUI();
    }

    public void AddScore(int amount)
    {
        score += amount; // 점수 증가
        if (score > highScore)
        {
            highScore = score;
        }
        UpdateScoreUI(); // UI 업데이트
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "현재 점수: " + score;
            highScoreText.text = $"최고 점수 : {highScore}";

            if (highScore <= score)
                highScoreText.text = $"최고 점수 : {highScore}";
        }
        SaveScore();
    }

    void SaveScore()
    {
        // 데이터 저장
        ScoreData data = new ScoreData();
        data.highScore = highScore;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"점수 저장 완료: {savePath}");
    }

    void LoadScore()
    {
        // 파일이 존재하면 데이터 불러오기
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            ScoreData data = JsonUtility.FromJson<ScoreData>(json);

            // 최고점 불러오기
            highScore = data.highScore;
            highScoreText.text = $"최고 점수 : {highScore}";
        }
        else
        {
            Debug.Log("저장된 점수가 없습니다. 초기화합니다.");
            highScore = 0;
        }
    }

    private void OnApplicationQuit()
    {
        SaveScore();
    }
}
