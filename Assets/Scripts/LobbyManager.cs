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
    public string lobbyTitle = "WAVEBREAK";
    public string playButtonLabel = "PLAY";
    public string highScoreLabel = "최고 점수";
    public string bestComboLabel = "최고 콤보";
    public string bestClearLabel = "최단 클리어";
    public string mainSceneName = "Main";
    public Vector2 playButtonSize = new Vector2(360f, 96f);
    public Vector2 playButtonPosition = new Vector2(0f, -130f);

    TMP_FontAsset uiFont;
    RecordRow scoreRow;
    RecordRow comboRow;
    RecordRow clearRow;

    class RecordRow
    {
        public TextMeshProUGUI label;
        public TextMeshProUGUI value;
    }

    void Start()
    {
        CacheFont();
        BuildLayout();
        RefreshRecords();
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

    void CacheFont()
    {
        if (highScoreText != null)
            uiFont = highScoreText.font;
    }

    void BuildLayout()
    {
        Transform panel = startButton != null ? startButton.transform.parent : null;
        if (panel == null && highScoreText != null)
            panel = highScoreText.transform.parent;
        if (panel == null)
            return;

        if (highScoreText != null)
            highScoreText.gameObject.SetActive(false);

        CreateLabel("GameTitle", panel, lobbyTitle, 58f, new Vector2(0f, 220f), new Vector2(900f, 80f), Color.white, TextAlignmentOptions.Center).characterSpacing = 4f;

        const float rowY = 88f;
        const float rowGap = 44f;
        scoreRow = CreateRecordRow("ScoreRow", panel, new Vector2(0f, rowY));
        comboRow = CreateRecordRow("ComboRow", panel, new Vector2(0f, rowY - rowGap));
        clearRow = CreateRecordRow("ClearRow", panel, new Vector2(0f, rowY - rowGap * 2f));

        RectTransform buttonRect = startButton.GetComponent<RectTransform>();
        buttonRect.anchoredPosition = playButtonPosition;
        buttonRect.sizeDelta = playButtonSize;

        TextMeshProUGUI playLabel = startButton.GetComponentInChildren<TextMeshProUGUI>();
        if (playLabel != null)
        {
            if (uiFont != null)
                playLabel.font = uiFont;
            playLabel.text = playButtonLabel;
            playLabel.fontSize = 36f;
            playLabel.alignment = TextAlignmentOptions.Center;
        }

        if (fadeImage != null)
            fadeImage.transform.SetAsLastSibling();
        if (loadingPanel != null)
            loadingPanel.transform.SetAsLastSibling();
    }

    void RefreshRecords()
    {
        ScoreData data = ScoreSave.Load();
        if (data == null)
            data = new ScoreData();

        SetRecordRow(scoreRow, highScoreLabel, data.highScore.ToString());
        SetRecordRow(comboRow, bestComboLabel, data.bestCombo.ToString());
        SetRecordRow(clearRow, bestClearLabel, ScoreSave.FormatClearRecord(data.bestClearTime));
    }

    void SetRecordRow(RecordRow row, string label, string value)
    {
        if (row == null)
            return;
        if (row.label != null)
            row.label.text = label;
        if (row.value != null)
            row.value.text = value;
    }

    RecordRow CreateRecordRow(string name, Transform parent, Vector2 position)
    {
        GameObject rowObject = CreateUiObject(name, parent);
        RectTransform rowRect = rowObject.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 0.5f);
        rowRect.anchorMax = new Vector2(0.5f, 0.5f);
        rowRect.pivot = new Vector2(0.5f, 0.5f);
        rowRect.anchoredPosition = position;
        rowRect.sizeDelta = new Vector2(480f, 40f);

        TextMeshProUGUI label = CreateLabel("Label", rowObject.transform, string.Empty, 32f, Vector2.zero, Vector2.zero, new Color(0.72f, 0.72f, 0.72f, 1f), TextAlignmentOptions.MidlineLeft);
        StretchFull(label.rectTransform);

        TextMeshProUGUI value = CreateLabel("Value", rowObject.transform, string.Empty, 32f, Vector2.zero, Vector2.zero, Color.white, TextAlignmentOptions.MidlineRight);
        StretchFull(value.rectTransform);

        RecordRow row = new RecordRow();
        row.label = label;
        row.value = value;
        return row;
    }

    TextMeshProUGUI CreateLabel(string name, Transform parent, string text, float fontSize, Vector2 position, Vector2 size, Color color, TextAlignmentOptions alignment)
    {
        GameObject labelObject = CreateUiObject(name, parent);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        if (uiFont != null)
            label.font = uiFont;
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = color;
        label.raycastTarget = false;
        return label;
    }

    GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject uiObject = new GameObject(name, typeof(RectTransform));
        uiObject.layer = parent.gameObject.layer;
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
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
        fadeImage.transform.SetAsLastSibling();
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.DOFade(1, 1f).OnComplete(() => onComplete.Invoke());
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        loadingPanel.transform.SetAsLastSibling();
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
