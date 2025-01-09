using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Vector3 originalScale;
    public float scaleFactor = 1.2f; // Ŀ�� ũ�� ����
    public float animationSpeed = 0.2f; // �ִϸ��̼� �ӵ�

    private Coroutine currentCoroutine; // ���� ���� ���� Coroutine ����
    void Start()
    {
        originalScale = transform.localScale; // �ʱ� ũ�� ����
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartScaling(originalScale * scaleFactor); // ũ�� Ű���
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartScaling(originalScale); // ���� ũ��� ���ư���
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        StartScaling(originalScale); // Ŭ�� �� ���� ũ��� ����
    }
    private void StartScaling(Vector3 targetScale)
    {
        // ���� Coroutine ����
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        // ���ο� Coroutine ����
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
        // ������Ʈ ��Ȱ��ȭ �� Coroutine ����
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        // ��Ȱ��ȭ �� ũ�⸦ ���� ũ��� ����
        transform.localScale = originalScale;
    }
}
