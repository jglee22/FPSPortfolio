using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MenuManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public Image fadeImage;
    public Button resumeButton;
    public Button lobbyButton;
    public Button quitButton;
    public TMP_FontAsset uiFont;
    public string mainSceneName = "Main";
    public string lobbySceneName = "Lobby";
    public float gameOverDelay = 0.75f;
    public string gameOverTitle = "GAME OVER";
    public string missionClearTitle = "MISSION CLEAR";
    public string retryButtonLabel = "다시 시작";
    public string lobbyButtonLabel = "로비 이동";
    public string newBestLabel = "신기록";
    public string currentScoreFormat = "현재 점수 : {0}";
    public string highScoreFormat = "최고 점수 : {0}";
    public string comboRecordFormat = "최고 콤보 : {0}  /  기록 {1}";
    public string clearTimeFormat = "클리어 : {0}  /  최단 클리어 : {1}";
    public string surviveTimeFormat = "생존 시간 : {0}";

    bool isPaused;
    bool isGameOver;
    bool isLeavingScene;
    static MenuManager instance;
    GameObject gameOverPanel;
    TextMeshProUGUI resultTitleText;
    TextMeshProUGUI gameOverScoreText;
    TextMeshProUGUI gameOverHighScoreText;
    TextMeshProUGUI gameOverComboText;
    TextMeshProUGUI gameOverTimeText;
    TextMeshProUGUI newBestText;

    public static bool IsInputBlocked
    {
        get
        {
            if (instance == null)
                return false;
            return instance.isPaused || instance.isGameOver || instance.isLeavingScene;
        }
    }

    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    void Start()
    {
        resumeButton.onClick.AddListener(ResumeGame);
        lobbyButton.onClick.AddListener(GoToLobby);
        quitButton.onClick.AddListener(QuitGame);
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
        CreateGameOverPanel();
        SetMenuCanvasVisible(false);
    }

    void Update()
    {
        if (isGameOver || isLeavingScene || WaveRewardUI.IsOpen)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isGameOver || WaveRewardUI.IsOpen)
            return;

        SetMenuCanvasVisible(true);
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Time.timeScale = 0f;
        SetCursorVisible(true);
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (isGameOver)
            return;

        SetMenuCanvasVisible(false);
        Time.timeScale = 1f;
        SetCursorVisible(false);
        isPaused = false;
    }

    public void ShowGameOver()
    {
        ShowResult(gameOverTitle, false);
    }

    public void ShowMissionClear()
    {
        ShowResult(missionClearTitle, true);
    }

    void ShowResult(string title, bool cleared)
    {
        if (isGameOver)
            return;

        isGameOver = true;
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.StopRun(cleared);
        CombatHitFeedback.PlayUi(cleared ? 1.12f : 0.7f);
        if (resultTitleText != null)
            resultTitleText.text = title;
        StartCoroutine(ShowGameOverRoutine());
    }

    public void GoToLobby()
    {
        if (WaveRewardUI.IsOpen)
            return;

        LoadSceneWithFade(lobbySceneName);
    }

    public void RetryGame()
    {
        LoadSceneWithFade(mainSceneName);
    }

    public void QuitGame()
    {
        if (isLeavingScene)
            return;

        isLeavingScene = true;
        Time.timeScale = 1f;
        fadeImage.gameObject.SetActive(true);
        fadeImage.transform.SetAsLastSibling();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.DOFade(1f, 1f).OnComplete(() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });
    }

    IEnumerator ShowGameOverRoutine()
    {
        yield return new WaitForSecondsRealtime(gameOverDelay);

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        RefreshGameOverScore();
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        SetMenuCanvasVisible(true);
        Time.timeScale = 0f;
        SetCursorVisible(true);
    }

    void LoadSceneWithFade(string sceneName)
    {
        if (isLeavingScene)
            return;

        isLeavingScene = true;
        Time.timeScale = 1f;
        fadeImage.gameObject.SetActive(true);
        fadeImage.transform.SetAsLastSibling();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.DOFade(1f, 1f).OnComplete(() =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }

    void RefreshGameOverScore()
    {
        int score = 0;
        int highScore = 0;
        int combo = 0;
        int bestCombo = 0;
        float runTime = 0f;
        float bestClearTime = 0f;
        bool missionCleared = false;
        bool isNewBest = false;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.PersistHighScore();
            score = ScoreManager.Instance.score;
            highScore = ScoreManager.Instance.highScore;
            combo = ScoreManager.Instance.BestComboThisRun;
            bestCombo = ScoreManager.Instance.BestCombo;
            runTime = ScoreManager.Instance.RunTime;
            bestClearTime = ScoreManager.Instance.BestClearTime;
            missionCleared = ScoreManager.Instance.MissionCleared;
            isNewBest = ScoreManager.Instance.HasAnyNewRecord;
        }

        if (gameOverScoreText != null)
            gameOverScoreText.text = string.Format(currentScoreFormat, score);
        if (gameOverHighScoreText != null)
            gameOverHighScoreText.text = string.Format(highScoreFormat, highScore);
        if (gameOverComboText != null)
            gameOverComboText.text = string.Format(comboRecordFormat, combo, bestCombo);
        if (gameOverTimeText != null)
        {
            string timeText = ScoreSave.FormatTime(runTime);
            if (missionCleared)
                gameOverTimeText.text = string.Format(clearTimeFormat, timeText, ScoreSave.FormatClearRecord(bestClearTime));
            else
                gameOverTimeText.text = string.Format(surviveTimeFormat, timeText);
        }
        if (newBestText != null)
            newBestText.gameObject.SetActive(isNewBest);
    }

    void CreateGameOverPanel()
    {
        if (uiFont == null && ScoreManager.Instance != null && ScoreManager.Instance.scoreText != null)
            uiFont = ScoreManager.Instance.scoreText.font;

        gameOverPanel = CreateUiObject("GameOverPanel", transform);
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        StretchFull(panelRect);
        Image overlay = gameOverPanel.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.78f);
        overlay.raycastTarget = true;
        gameOverPanel.SetActive(false);

        resultTitleText = CreateLabel("Title", gameOverPanel.transform, gameOverTitle, 92f, new Vector2(0f, 240f), new Vector2(900f, 120f));
        gameOverScoreText = CreateLabel("Score", gameOverPanel.transform, string.Format(currentScoreFormat, 0), 42f, new Vector2(0f, 110f), new Vector2(800f, 60f));
        gameOverHighScoreText = CreateLabel("HighScore", gameOverPanel.transform, string.Format(highScoreFormat, 0), 32f, new Vector2(0f, 58f), new Vector2(800f, 44f));
        gameOverComboText = CreateLabel("Combo", gameOverPanel.transform, string.Format(comboRecordFormat, 0, 0), 32f, new Vector2(0f, 14f), new Vector2(800f, 44f));
        gameOverTimeText = CreateLabel("Time", gameOverPanel.transform, string.Format(surviveTimeFormat, "-"), 32f, new Vector2(0f, -30f), new Vector2(800f, 44f));
        newBestText = CreateLabel("NewBest", gameOverPanel.transform, newBestLabel, 32f, new Vector2(0f, -78f), new Vector2(400f, 40f));
        newBestText.color = new Color(1f, 0.78f, 0.25f, 1f);
        newBestText.gameObject.SetActive(false);

        Button retry = CreateMenuButton("RetryButton", gameOverPanel.transform, retryButtonLabel, new Vector2(0f, -175f));
        retry.onClick.AddListener(RetryGame);

        Button lobby = CreateMenuButton("LobbyButton", gameOverPanel.transform, lobbyButtonLabel, new Vector2(0f, -280f));
        lobby.onClick.AddListener(GoToLobby);
    }

    TextMeshProUGUI CreateLabel(string name, Transform parent, string text, float fontSize, Vector2 position, Vector2 size)
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
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
        return label;
    }

    Button CreateMenuButton(string name, Transform parent, string label, Vector2 position)
    {
        GameObject buttonObject = CreateUiObject(name, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(320f, 80f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.12f, 0.12f, 0.12f, 0.92f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        colors.pressedColor = new Color(0.08f, 0.08f, 0.08f, 1f);
        button.colors = colors;

        CreateLabel(name + "Label", buttonObject.transform, label, 34f, Vector2.zero, Vector2.zero);
        RectTransform labelRect = buttonObject.transform.GetChild(0).GetComponent<RectTransform>();
        StretchFull(labelRect);
        return button;
    }

    GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject uiObject = new GameObject(name, typeof(RectTransform));
        uiObject.layer = gameObject.layer;
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

    void SetMenuCanvasVisible(bool visible)
    {
        CanvasGroup canvasGroup = GetComponentInParent<CanvasGroup>();
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    void SetCursorVisible(bool visible)
    {
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = visible;
    }
}
