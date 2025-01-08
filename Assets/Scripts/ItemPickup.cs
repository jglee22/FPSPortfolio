using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public WeaponUpgradeItem upgradeItem; // 아이템 데이터
    public AudioClip pickupSound; // 획득 효과음 (옵션)

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyUpgrade(other.GetComponent<GunController>()); // 플레이어 무기 컨트롤러 전달

            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            Destroy(gameObject); // 아이템 제거
        }
    }

    void ApplyUpgrade(GunController gunController)
    {
        // 현재 장착 중인 무기 가져오기
        Gun currentGun = gunController.GetCurrentGun();

        if (currentGun.gunName == upgradeItem.weaponType)
        {
            if (upgradeItem.increaseDamage)
                currentGun.IncreaseDamage(upgradeItem.amount); // 데미지 증가

            if (upgradeItem.increaseAmmo)
                currentGun.IncreaseMaxAmmo(upgradeItem.amount); // 최대 탄수 증가
        }
    }
}
