using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaning : MonoBehaviour
{
    public Transform cameraTransform;  // FPS 카메라 참조
    public float leanAngle = 15f;      // 기울일 각도
    public float leanSpeed = 5f;      // 부드러운 전환 속도

    private float currentLean = 0f;    // 현재 기울기 상태
    private float targetLean = 0f;     // 목표 기울기 상태

    void Update()
    {
        // 입력 처리
        if (Input.GetKey(KeyCode.Q))      // 왼쪽 기울이기
        {
            targetLean = leanAngle;
        }
        else if (Input.GetKey(KeyCode.E)) // 오른쪽 기울이기
        {
            targetLean = -leanAngle;
        }
        else
        {
            targetLean = 0f; // 초기화
        }

        if(currentLean != targetLean)
        {
            // 부드러운 회전 전환
            currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);

            // 기존 회전값에 기울기 추가 (Y축 회전 유지)
            Quaternion baseRotation = Quaternion.Euler(cameraTransform.localRotation.eulerAngles.x, cameraTransform.localRotation.eulerAngles.y, 0);
            Quaternion leanRotation = Quaternion.Euler(0, 0, currentLean);
            cameraTransform.localRotation = baseRotation * leanRotation; // 기존 회전 + 기울기 적용
        }
    }
}
