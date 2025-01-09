using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Vector3 originalScale;
    public float scaleFactor = 1.2f; // 커질 크기 비율
    public float animationSpeed = 0.2f; // 애니메이션 속도

    private Coroutine currentCoroutine; // 현재 실행 중인 Coroutine 참조
    void Start()
    {
        originalScale = transform.localScale; // 초기 크기 저장
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartScaling(originalScale * scaleFactor); // 크기 키우기
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartScaling(originalScale); // 원래 크기로 돌아가기
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        StartScaling(originalScale); // 클릭 시 원래 크기로 복원
    }
    private void StartScaling(Vector3 targetScale)
    {
        // 이전 Coroutine 중지
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        // 새로운 Coroutine 시작
        currentCoroutine = StartCoroutine(ScaleTo(targetScale));
    }
    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        Vector3 currentScale = transform.localScale;
        float progress = 0;

        while (progress < 1)
        {
            progress += Time.unscaledDeltaTime / animationSpeed;
            transform.localScale = Vector3.Lerp(currentScale, targetScale, progress);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    private void OnDisable()
    {
        // 오브젝트 비활성화 시 Coroutine 중지
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        // 비활성화 시 크기를 원래 크기로 복원
        transform.localScale = originalScale;
    }
}
