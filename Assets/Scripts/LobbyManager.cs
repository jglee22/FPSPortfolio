using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LobbyManager : MonoBehaviour
{
    public TMP_Text highScoreText;
    public Image fadeImage;
    public GameObject loadingPanel;
    public Image loadingImage;
    public Button startButton;
    public string highScoreFormat = "최고 점수 : {0}";
    public string bestComboFormat = "최고 콤보 : x{0}";
    public string bestClearFormat = "최단 클리어 : {0}";
    public string mainSceneName = "Main";

    void Start()
    {
        if (highScoreText != null)
            highScoreText.text = FormatRecords(ScoreSave.Load());

        startButton.onClick.AddListener(StartGame);
        FadeIn();
    }

    public void StartGame()
    {
        FadeOut(() =>
        {
            StartCoroutine(LoadSceneAsync(mainSceneName));
        });
    }

    string FormatRecords(ScoreData data)
    {
        if (data == null)
            data = new ScoreData();

        return string.Format(highScoreFormat, data.highScore)
            + "\n" + string.Format(bestComboFormat, data.bestCombo)
            + "\n" + string.Format(bestClearFormat, ScoreSave.FormatClearRecord(data.bestClearTime));
    }

    void FadeIn()
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 1);
        fadeImage.DOFade(0, 1f).OnComplete(() =>
        {
            fadeImage.gameObject.SetActive(false);
        });
    }

    void FadeOut(System.Action onComplete)
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.DOFade(1, 1f).OnComplete(() => onComplete.Invoke());
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        loadingPanel.SetActive(true);

        loadingImage.transform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            if (operation.progress >= 0.9f)
                operation.allowSceneActivation = true;

            yield return null;
        }
    }
}