using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    // 무기 목록 및 현재 무기
    public List<Gun> guns;             // 장착 가능한 무기 리스트
    private int currentGunIndex = 0;   // 현재 선택된 무기 인덱스
    private Gun currentGun;            // 현재 장착된 무기

    void Start()
    {
        // 첫 번째 무기 장착
        EquipGun(currentGunIndex);
    }

    void Update()
    {
        HandleWeaponSwitch(); // 무기 전환 처리
    }

    // 무기 전환 처리
    void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EquipGun(0); // 1번 무기 선택 (예: 라이플)
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            EquipGun(1); // 2번 무기 선택 (예: 샷건)
        }
    }

    // 무기 장착
    void EquipGun(int index)
    {
        if (index < 0 || index >= guns.Count) return;

        if (currentGun != null)
        {
            // 기존 무기 비활성화 및 쿨다운 초기화
            if (currentGun.gunType == GunType.Shotgun) // 이전 무기가 샷건일 경우
            {
                currentGun.CancelShotgunCooldown();   // 쿨다운 초기화 호출
            }
            currentGun.CancelReload();
            currentGun.gameObject.SetActive(false);
        }

        currentGunIndex = index;
        currentGun = guns[currentGunIndex];
        currentGun.gameObject.SetActive(true);

        currentGun.UpdateUI();

        // 카메라 회전 초기화 보완
        GunRecoil gunRecoil = currentGun.GetComponent<GunRecoil>();
        if (gunRecoil != null)
        {
            gunRecoil.cameraTransform = Camera.main.transform; // 카메라 동기화
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 현재 무기 반환
    public Gun GetCurrentGun()
    {
        return currentGun;
    }
}
