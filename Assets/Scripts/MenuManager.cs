using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
public class MenuManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // 일시정지 메뉴
    public Image fadeImage;        // 페이드 효과용 이미지
    public Button resumeButton;    // 일시 정지 해제 버튼
    public Button lobbyButton;     // 로비씬 이동 버튼
    public Button quitButton;      // 종료 버튼
    private bool isPaused = false; // 게임이 일시정지 상태인지 확인

    // Start is called before the first frame update
    void Start()
    {
        resumeButton.onClick.AddListener(ResumeGame);
        lobbyButton.onClick.AddListener(GoToLobby);
        quitButton.onClick.AddListener(QuitGame);
    }

    // Update is called once per frame
    void Update()
    {
        // ESC 키로 일시정지 토글
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
        // 메뉴 활성화 및 게임 정지
        //pauseMenuUI.SetActive(true);
        if (GetComponentInParent<CanvasGroup>() != null)
        {
            CanvasGroup canvasGroup = GetComponentInParent<CanvasGroup>();

            canvasGroup.alpha = 1f; // 버튼 표시
            canvasGroup.interactable = true; // 클릭 가능
            canvasGroup.blocksRaycasts = true; // UI 클릭 가능
        }
        Time.timeScale = 0f; // 일시 정지
        Cursor.lockState = CursorLockMode.None; // 커서 잠금 해제
        Cursor.visible = true; // 커서 활성화
        isPaused = true;
    }

    public void ResumeGame()
    {
        // 메뉴 비활성화 및 게임 재개
        //pauseMenuUI.SetActive(false);
        if (GetComponentInParent<CanvasGroup>() != null)
        {
            CanvasGroup canvasGroup = GetComponentInParent<CanvasGroup>();

            canvasGroup.alpha = 0f; // 버튼 숨김
            canvasGroup.interactable = false; // 클릭 불가
            canvasGroup.blocksRaycasts = false; // UI 클릭 차단
        }
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked; // 커서 잠금 설정
        Cursor.visible = false;
        isPaused = false;
    }

    public void GoToLobby()
    {
        Time.timeScale = 1f; // 시간 재개
        // 페이드 아웃 후 로비 씬으로 이동
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 0); // 투명하게 시작
        fadeImage.DOFade(1, 1f).OnComplete(() =>
        {
            SceneManager.LoadScene("Lobby"); // 로비 씬 이동
        });
    }

    public void QuitGame()
    {
        // 게임 종료
        Debug.Log("게임 종료");
#if UNITY_EDITOR || UNITY_STANDALONE //Editor와 PC 빌드일때의 플랫폼
        Time.timeScale = 1f; // 시간 재개
        // 페이드 아웃 후 로비 씬으로 이동
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 0); // 투명하게 시작
        fadeImage.DOFade(1, 1f).OnComplete(() =>
        {
            UnityEditor.EditorApplication.isPlaying = false;
        });
#else   //Android / IOS 등 Editor와 PC 빌드가 아닌 플랫폼
        Time.timeScale = 1f; // 시간 재개
        // 페이드 아웃 후 종료
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 0); // 투명하게 시작
        fadeImage.DOFade(1, 1f).OnComplete(() =>
        {
            Application.Quit();
        });
#endif
    }
}
