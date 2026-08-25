using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public List<Gun> guns;
    private int currentGunIndex = 0;
    private Gun currentGun;
    private PlayerHealth playerHealth;

    void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        EquipGun(currentGunIndex);
    }

    void Update()
    {
        HandleWeaponSwitch();
    }

    void HandleWeaponSwitch()
    {
        if (playerHealth != null && playerHealth.isPlayerDie)
            return;

        if (WaveRewardUI.IsOpen)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            EquipGun(0);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            EquipGun(1);
    }

    void EquipGun(int index)
    {
        if (index < 0 || index >= guns.Count)
            return;

        if (currentGun != null)
        {
            if (currentGun.gunType == GunType.Shotgun)
                currentGun.CancelShotgunCooldown();

            currentGun.CancelReload();
            currentGun.gameObject.SetActive(false);
        }

        currentGunIndex = index;
        currentGun = guns[currentGunIndex];
        currentGun.gameObject.SetActive(true);
        currentGun.UpdateUI();

        GunRecoil gunRecoil = currentGun.GetComponent<GunRecoil>();
        if (gunRecoil != null && Camera.main != null)
            gunRecoil.cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public Gun GetCurrentGun()
    {
        return currentGun;
    }
}
