using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
public class PickupMessageManager : MonoBehaviour
{
    public static PickupMessageManager Instance; // 싱글톤 패턴 적용

    public GameObject pickupMessagePrefab; // 메시지 프리팹
    public Transform pickupMessagePanel; // UI 패널 (메시지를 담는 부모)

    private Queue<string> messageQueue = new Queue<string>(); // 메시지 저장 큐
    private bool isDisplayingMessage = false; // 현재 메시지 출력 여부

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

            // 1️⃣ 메시지 프리팹 생성
            GameObject newMessage = Instantiate(pickupMessagePrefab, pickupMessagePanel);
            TextMeshProUGUI messageText = newMessage.GetComponentInChildren<TextMeshProUGUI>();

            messageText.text = message;
            newMessage.transform.localPosition = new Vector3(0, 200, 0); // 초기 위치 (화면 위)

            // 2️⃣ DoTween으로 위에서 아래로 이동 애니메이션
            newMessage.transform.DOLocalMoveY(-288, 0.5f).SetEase(Ease.OutBounce);

            // 3️⃣ 일정 시간 유지
            yield return new WaitForSeconds(2f);

            // 4️⃣ 위로 이동하면서 사라지는 애니메이션
            newMessage.transform.DOLocalMoveY(200, 0.5f).SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    Destroy(newMessage);
                });

            yield return new WaitForSeconds(0.5f); // 메시지가 완전히 사라진 후 다음 메시지 출력
        }

        isDisplayingMessage = false;
    }
}
