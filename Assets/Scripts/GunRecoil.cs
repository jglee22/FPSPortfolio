using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    public Transform cameraTransform;  // FPS 카메라
    public float recoilAmount = 1.5f;  // 기본 반동 크기
    public float recoilSpeed = 5f;     // 반동 회복 속도
    public float maxRecoilX = 10f;     // X축 최대 반동 (상하)
    public float maxRecoilY = 5f;      // Y축 최대 반동 (좌우)

    private Vector3 currentRotation;   // 현재 회전 값
    private Vector3 targetRotation;    // 목표 회전 값

    void Update()
    {
        // 반동 회복 처리
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, recoilSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, recoilSpeed * Time.deltaTime);

        // 기존 회전값에 반동 추가
        Quaternion originalRotation = cameraTransform.localRotation; // 기존 회전값 저장
        Quaternion recoilRotation = Quaternion.Euler(currentRotation); // 반동 회전값 생성
        cameraTransform.localRotation = originalRotation * recoilRotation; // 기존 회전에 반동값 곱하기
    }

    public void ApplyRecoil()
    {
        // 반동 누적
        targetRotation += new Vector3(
            Random.Range(-recoilAmount, recoilAmount),   // 상하 반동
            Random.Range(-recoilAmount / 2, recoilAmount / 2), // 좌우 반동
            0
        );

        // 최대 반동 제한 적용
        targetRotation.x = Mathf.Clamp(targetRotation.x, -maxRecoilX, maxRecoilX);
        targetRotation.y = Mathf.Clamp(targetRotation.y, -maxRecoilY, maxRecoilY);
    }
}
