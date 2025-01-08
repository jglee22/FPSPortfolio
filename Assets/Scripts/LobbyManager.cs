using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // �� ��ȯ�� ���� ���ӽ����̽�
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro ���
using DG.Tweening;

public class LobbyManager : MonoBehaviour
{
    public TMP_Text highScoreText; // �ְ��� ǥ�� Text
    public Image fadeImage;        // ���̵� ȿ���� UI �̹���
    public GameObject loadingPanel; // �ε� �г�
    public Image loadingImage;    // �ε� �̹���
    public Button startButton;   // Play ��ư
    private string savePath;       // ���� ���

    void Start()
    {
        // ���� ��� ����
        savePath = System.IO.Path.Combine(Application.persistentDataPath, "scoreData.json");

        // �ְ��� �ҷ����� �� ǥ��
        int highScore = LoadHighScore();
        highScoreText.text = $"�ְ� ���� : {highScore}";

        startButton.onClick.AddListener(StartGame);
        // ���� �� ���̵� ��
        FadeIn();
    }

    public void StartGame()
    {
        // ���̵� �ƿ� �� �� ��ȯ
        FadeOut(() =>
        {
            StartCoroutine(LoadSceneAsync("Main")); // ���� ������ �̵�
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

        // ����� ������ ������ 0 ��ȯ
        return 0;
    }
    void FadeIn()
    {
        // ���� �� ���̵� �� ȿ��
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 1); // ������ ���� ������
        fadeImage.DOFade(0, 1f).OnComplete(() =>
        {
            fadeImage.gameObject.SetActive(false); // ���̵� �Ϸ� �� ��Ȱ��ȭ
        });
    }

    void FadeOut(System.Action onComplete)
    {
        // �� ��ȯ �� ���̵� �ƿ� ȿ��
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 0); // ������ ���� ����
        fadeImage.DOFade(1, 1f).OnComplete(() => onComplete.Invoke());
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        // �ε� �г� Ȱ��ȭ
        loadingPanel.SetActive(true);

        // �ε� ������ ȸ�� ����
        loadingImage.transform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart) // ���� ȸ��
            .SetEase(Ease.Linear);

        // �񵿱� �� �ε�
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            // �ε� ���� ǥ�� (�ʿ� �� �߰�)
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            Debug.Log($"�ε� ��: {progress * 100}%");

            if (operation.progress >= 0.9f)
            {
                // �ε� �Ϸ� �� �� ��ȯ
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}