using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float duration = 0.2f;      // ��鸲 ���� �ð�
    public float magnitude = 0.3f;    // ��鸲 ����

    private Transform camTransform;
    private Vector3 originalPosition;

    void Awake()
    {
        // ī�޶� Transform ����
        camTransform = Camera.main.transform;
        originalPosition = camTransform.localPosition;
    }

    public void Shake(float durationOverride = -1f, float magnitudeOverride = -1f)
    {
        // Ŀ���� ���� �ð� �� ���� ����
        float shakeDuration = durationOverride > 0 ? durationOverride : duration;
        float shakeMagnitude = magnitudeOverride > 0 ? magnitudeOverride : magnitude;

        // ��鸲 ����
        StartCoroutine(ShakeCoroutine(shakeDuration, shakeMagnitude));
    }

    IEnumerator ShakeCoroutine(float shakeDuration, float shakeMagnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            // ���� ��ġ ����
            Vector3 randomOffset = Random.insideUnitSphere * shakeMagnitude;
            camTransform.localPosition = originalPosition + randomOffset;

            elapsed += Time.deltaTime;
            yield return null; // �� ������ ���
        }

        // ���� ��ġ�� ����
        camTransform.localPosition = originalPosition;
    }
}
