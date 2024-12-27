using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    public Transform weaponTransform;  // 무기 오브젝트
    public float recoilAmount = 0.1f;  // 기본 반동 크기
    public float recoilSpeed = 8f;     // 회복 속도
    public float maxRecoil = 0.3f;     // 최대 반동 거리

    private Vector3 originalPosition;  // 원래 위치
    private Vector3 recoilPosition;    // 반동 위치

    void Start()
    {
        originalPosition = weaponTransform.localPosition;
    }

    void Update()
    {
        // 반동 위치에서 원래 위치로 회복
        weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, originalPosition, Time.deltaTime * recoilSpeed);
    }

    public void ApplyRecoil()
    {
        // 무기 반동 누적 처리
        recoilPosition = weaponTransform.localPosition - Vector3.forward * recoilAmount;
        recoilPosition.z = Mathf.Clamp(recoilPosition.z, -maxRecoil, 0); // 최대 반동 제한
        weaponTransform.localPosition = recoilPosition;
    }
}
