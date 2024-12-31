using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    // ���� ��� �� ���� ����
    public List<Gun> guns;             // ���� ������ ���� ����Ʈ
    private int currentGunIndex = 0;   // ���� ���õ� ���� �ε���
    private Gun currentGun;            // ���� ������ ����

    void Start()
    {
        // ù ��° ���� ����
        EquipGun(currentGunIndex);
    }

    void Update()
    {
        HandleWeaponSwitch(); // ���� ��ȯ ó��
    }

    // ���� ��ȯ ó��
    void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EquipGun(0); // 1�� ���� ���� (��: ������)
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            EquipGun(1); // 2�� ���� ���� (��: ����)
        }
    }

    // ���� ����
    void EquipGun(int index)
    {
        if (index < 0 || index >= guns.Count) return;

        if (currentGun != null)
        {
            // ���� ���� ��Ȱ��ȭ �� ��ٿ� �ʱ�ȭ
            if (currentGun.gunType == GunType.Shotgun) // ���� ���Ⱑ ������ ���
            {
                currentGun.CancelShotgunCooldown();   // ��ٿ� �ʱ�ȭ ȣ��
            }
            currentGun.CancelReload();
            currentGun.gameObject.SetActive(false);
        }

        currentGunIndex = index;
        currentGun = guns[currentGunIndex];
        currentGun.gameObject.SetActive(true);

        currentGun.UpdateUI();

        // ī�޶� ȸ�� �ʱ�ȭ ����
        GunRecoil gunRecoil = currentGun.GetComponent<GunRecoil>();
        if (gunRecoil != null)
        {
            gunRecoil.cameraTransform = Camera.main.transform; // ī�޶� ����ȭ
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ���� ���� ��ȯ
    public Gun GetCurrentGun()
    {
        return currentGun;
    }
}
