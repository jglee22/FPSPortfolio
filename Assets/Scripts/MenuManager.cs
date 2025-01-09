using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
public class MenuManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // �Ͻ����� �޴�
    public Image fadeImage;        // ���̵� ȿ���� �̹���
    public Button resumeButton;    // �Ͻ� ���� ���� ��ư
    public Button lobbyButton;     // �κ�� �̵� ��ư
    public Button quitButton;      // ���� ��ư
    private bool isPaused = false; // ������ �Ͻ����� �������� Ȯ��

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
        // ESC Ű�� �Ͻ����� ���
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
        // �޴� Ȱ��ȭ �� ���� ����
        //pauseMenuUI.SetActive(true);
        if (GetComponentInParent<CanvasGroup>() != null)
        {
            CanvasGroup canvasGroup = GetComponentInParent<CanvasGroup>();

            canvasGroup.alpha = 1f; // ��ư ǥ��
            canvasGroup.interactable = true; // Ŭ�� ����
            canvasGroup.blocksRaycasts = true; // UI Ŭ�� ����
        }
        Time.timeScale = 0f; // �Ͻ� ����
        Cursor.lockState = CursorLockMode.None; // Ŀ�� ��� ����
        Cursor.visible = true; // Ŀ�� Ȱ��ȭ
        isPaused = true;
    }

    public void ResumeGame()
    {
        // �޴� ��Ȱ��ȭ �� ���� �簳
        //pauseMenuUI.SetActive(false);
        if (GetComponentInParent<CanvasGroup>() != null)
        {
            CanvasGroup canvasGroup = GetComponentInParent<CanvasGroup>();

            canvasGroup.alpha = 0f; // ��ư ����
            canvasGroup.interactable = false; // Ŭ�� �Ұ�
            canvasGroup.blocksRaycasts = false; // UI Ŭ�� ����
        }
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked; // Ŀ�� ��� ����
        Cursor.visible = false;
        isPaused = false;
    }

    public void GoToLobby()
    {
        Time.timeScale = 1f; // �ð� �簳
        // ���̵� �ƿ� �� �κ� ������ �̵�
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 0); // �����ϰ� ����
        fadeImage.DOFade(1, 1f).OnComplete(() =>
        {
            SceneManager.LoadScene("Lobby"); // �κ� �� �̵�
        });
    }

    public void QuitGame()
    {
        // ���� ����
        Debug.Log("���� ����");
#if UNITY_EDITOR || UNITY_STANDALONE //Editor�� PC �����϶��� �÷���
        Time.timeScale = 1f; // �ð� �簳
        // ���̵� �ƿ� �� �κ� ������ �̵�
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 0); // �����ϰ� ����
        fadeImage.DOFade(1, 1f).OnComplete(() =>
        {
            UnityEditor.EditorApplication.isPlaying = false;
        });
#else   //Android / IOS �� Editor�� PC ���尡 �ƴ� �÷���
        Time.timeScale = 1f; // �ð� �簳
        // ���̵� �ƿ� �� ����
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 0); // �����ϰ� ����
        fadeImage.DOFade(1, 1f).OnComplete(() =>
        {
            Application.Quit();
        });
#endif
    }
}
