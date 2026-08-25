using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgradeItem", menuName = "Item/WeaponUpgradeItem")]
public class WeaponUpgradeItem : ScriptableObject
{
    public string displayName;
    public string weaponType;
    public bool increaseDamage;
    public bool increaseAmmo;
    public bool increaseReloadSpeed;
    public bool increaseFireRate;
    public int amount;
    public float multiplier = 1f;

    public string GetDescription()
    {
        if (increaseDamage)
            return "공격력 +" + amount;
        if (increaseAmmo)
            return "탄창 +" + amount;
        if (increaseFireRate)
            return "발사 간격 " + FormatReductionPercent(multiplier) + "% 감소";
        if (increaseReloadSpeed)
            return "재장전 시간 " + FormatReductionPercent(multiplier) + "% 감소";
        return string.Empty;
    }

    static int FormatReductionPercent(float value)
    {
        return Mathf.RoundToInt((1f - value) * 100f);
    }

    public void Apply(Gun gun)
    {
        if (gun == null)
            return;

        if (!string.IsNullOrEmpty(weaponType) && gun.gunName != weaponType)
            return;

        if (increaseDamage)
            gun.IncreaseDamage(amount);
        if (increaseAmmo)
            gun.IncreaseMaxAmmo(amount);
        if (increaseReloadSpeed)
            gun.MultiplyReloadTime(multiplier);
        if (increaseFireRate)
            gun.MultiplyFireRate(multiplier);
    }

    public void ApplyToLoadout(GunController gunController)
    {
        if (gunController == null || gunController.guns == null)
            return;

        for (int i = 0; i < gunController.guns.Count; i++)
            Apply(gunController.guns[i]);
    }
}
