using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float duration = 0.2f;      // 흔들림 지속 시간
    public float magnitude = 0.3f;    // 흔들림 강도

    private Transform camTransform;
    private Vector3 originalPosition;

    void Awake()
    {
        // 카메라 Transform 참조
        camTransform = Camera.main.transform;
        originalPosition = camTransform.localPosition;
    }

    public void Shake(float durationOverride = -1f, float magnitudeOverride = -1f)
    {
        // 커스텀 지속 시간 및 강도 적용
        float shakeDuration = durationOverride > 0 ? durationOverride : duration;
        float shakeMagnitude = magnitudeOverride > 0 ? magnitudeOverride : magnitude;

        // 흔들림 시작
        StartCoroutine(ShakeCoroutine(shakeDuration, shakeMagnitude));
    }

    IEnumerator ShakeCoroutine(float shakeDuration, float shakeMagnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            // 랜덤 위치 적용
            Vector3 randomOffset = Random.insideUnitSphere * shakeMagnitude;
            camTransform.localPosition = originalPosition + randomOffset;

            elapsed += Time.deltaTime;
            yield return null; // 한 프레임 대기
        }

        // 원래 위치로 복귀
        camTransform.localPosition = originalPosition;
    }
}
