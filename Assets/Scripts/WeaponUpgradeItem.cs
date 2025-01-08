using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgradeItem", menuName = "Item/WeaponUpgradeItem")]
public class WeaponUpgradeItem : ScriptableObject
{
    public string weaponType; // "Rifle" �Ǵ� "Shotgun"
    public bool increaseDamage; // ������ ���� ����
    public bool increaseAmmo;   // �ִ� ź�� ���� ����
    public int amount;          // ������
}
