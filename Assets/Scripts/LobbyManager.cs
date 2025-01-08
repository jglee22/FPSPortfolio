using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // 씬 전환을 위한 네임스페이스
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용
using DG.Tweening;

public class LobbyManager : MonoBehaviour
{
    public TMP_Text highScoreText; // 최고점 표시 Text
    public Image fadeImage;        // 페이드 효과용 UI 이미지
    public GameObject loadingPanel; // 로딩 패널
    public Image loadingImage;    // 로딩 이미지
    public Button startButton;   // Play 버튼
    private string savePath;       // 저장 경로

    void Start()
    {
        // 저장 경로 설정
        savePath = System.IO.Path.Combine(Application.persistentDataPath, "scoreData.json");

        // 최고점 불러오기 및 표시
        int highScore = LoadHighScore();
        highScoreText.text = $"최고 점수 : {highScore}";

        startButton.onClick.AddListener(StartGame);
        // 시작 시 페이드 인
        FadeIn();
    }

    public void StartGame()
    {
        // 페이드 아웃 후 씬 전환
        FadeOut(() =>
        {
            StartCoroutine(LoadSceneAsync("Main")); // 메인 씬으로 이동
        });
    }

    int LoadHighScore()
    {
        if (System.IO.File.Exists(savePath))
        {
            string json = System.IO.File.ReadAllText(savePath);
            ScoreData data = JsonUtility.FromJson<ScoreData>(json);
            return data.highScore;
        }

        // 저장된 점수가 없으면 0 반환
        return 0;
    }
    void FadeIn()
    {
        // 시작 시 페이드 인 효과
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 1); // 검은색 완전 불투명
        fadeImage.DOFade(0, 1f).OnComplete(() =>
        {
            fadeImage.gameObject.SetActive(false); // 페이드 완료 후 비활성화
        });
    }

    void FadeOut(System.Action onComplete)
    {
        // 씬 전환 전 페이드 아웃 효과
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 0); // 검은색 완전 투명
        fadeImage.DOFade(1, 1f).OnComplete(() => onComplete.Invoke());
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        // 로딩 패널 활성화
        loadingPanel.SetActive(true);

        // 로딩 아이콘 회전 시작
        loadingImage.transform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart) // 무한 회전
            .SetEase(Ease.Linear);

        // 비동기 씬 로드
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            // 로딩 상태 표시 (필요 시 추가)
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            Debug.Log($"로딩 중: {progress * 100}%");

            if (operation.progress >= 0.9f)
            {
                // 로딩 완료 후 씬 전환
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}