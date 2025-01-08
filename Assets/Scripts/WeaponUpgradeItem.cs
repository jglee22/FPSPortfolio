using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgradeItem", menuName = "Item/WeaponUpgradeItem")]
public class WeaponUpgradeItem : ScriptableObject
{
    public string weaponType; // "Rifle" 또는 "Shotgun"
    public bool increaseDamage; // 데미지 증가 여부
    public bool increaseAmmo;   // 최대 탄수 증가 여부
    public int amount;          // 증가량
}
