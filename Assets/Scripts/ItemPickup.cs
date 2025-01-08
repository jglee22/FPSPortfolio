using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public WeaponUpgradeItem upgradeItem; // ������ ������
    public AudioClip pickupSound; // ȹ�� ȿ���� (�ɼ�)

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyUpgrade(other.GetComponent<GunController>()); // �÷��̾� ���� ��Ʈ�ѷ� ����

            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            Destroy(gameObject); // ������ ����
        }
    }

    void ApplyUpgrade(GunController gunController)
    {
        // ���� ���� ���� ���� ��������
        Gun currentGun = gunController.GetCurrentGun();

        if (currentGun.gunName == upgradeItem.weaponType)
        {
            if (upgradeItem.increaseDamage)
                currentGun.IncreaseDamage(upgradeItem.amount); // ������ ����

            if (upgradeItem.increaseAmmo)
                currentGun.IncreaseMaxAmmo(upgradeItem.amount); // �ִ� ź�� ����
        }
    }
}
