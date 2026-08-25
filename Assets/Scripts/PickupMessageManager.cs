using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
public class PickupMessageManager : MonoBehaviour
{
    public static PickupMessageManager Instance; // 싱글톤 패턴 적용

    public GameObject pickupMessagePrefab;
    public Transform pickupMessagePanel;
    public float appearOffsetY = 28f;
    public float showDuration = 1.4f;
    public float fadeDuration = 0.18f;
    public float backgroundAlpha = 0.72f;

    private Queue<string> messageQueue = new Queue<string>();
    private bool isDisplayingMessage = false;

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

    // ✅ 아이템 픽업 시 메시지를 Queue에 추가
    public void EnqueuePickupMessage(string message)
    {
        messageQueue.Enqueue(message);

        // 현재 메시지가 출력 중이 아니라면 새로운 메시지 표시 시작
        if (!isDisplayingMessage)
        {
            StartCoroutine(DisplayMessages());
        }
    }

    // ✅ Queue에 저장된 메시지를 하나씩 순서대로 출력
    private IEnumerator DisplayMessages()
    {
        isDisplayingMessage = true;

        while (messageQueue.Count > 0)
        {
            string message = messageQueue.Dequeue();

            GameObject newMessage = Instantiate(pickupMessagePrefab, pickupMessagePanel);
            PrepareMessage(newMessage, message);

            RectTransform messageRect = newMessage.GetComponent<RectTransform>();
            messageRect.anchoredPosition = new Vector2(0f, appearOffsetY);

            CanvasGroup canvasGroup = newMessage.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = newMessage.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            canvasGroup.DOFade(1f, fadeDuration);
            messageRect.DOAnchorPosY(0f, fadeDuration).SetEase(Ease.OutQuad);

            yield return new WaitForSeconds(showDuration);

            canvasGroup.DOFade(0f, fadeDuration);
            messageRect.DOAnchorPosY(appearOffsetY, fadeDuration).SetEase(Ease.InQuad)
                .OnComplete(() => Destroy(newMessage));

            yield return new WaitForSeconds(fadeDuration);
        }

        isDisplayingMessage = false;
    }

    void PrepareMessage(GameObject messageObject, string message)
    {
        TextMeshProUGUI messageText = messageObject.GetComponentInChildren<TextMeshProUGUI>();
        if (messageText != null)
        {
            messageText.text = message;
            messageText.raycastTarget = false;
        }

        Image[] images = messageObject.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            images[i].raycastTarget = false;
            Color color = images[i].color;
            color.a = backgroundAlpha;
            images[i].color = color;
        }
    }
}
