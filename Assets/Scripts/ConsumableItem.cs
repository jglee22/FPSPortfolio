using UnityEngine;

public enum ConsumableType
{
    Ammo,
    Health,
    Grenade
}

[CreateAssetMenu(fileName = "NewConsumableItem", menuName = "Item/ConsumableItem")]
public class ConsumableItem : ScriptableObject
{
    public string displayName;
    public ConsumableType type;
    public int amount;

    public bool CanApply(GameObject player)
    {
        if (player == null)
            return false;

        switch (type)
        {
            case ConsumableType.Ammo:
                Gun gun = GetCurrentGun(player);
                return gun != null && gun.NeedsAmmo();
            case ConsumableType.Health:
                PlayerHealth playerHealth = player.GetComponentInParent<PlayerHealth>();
                return playerHealth != null && playerHealth.NeedsHealth();
            case ConsumableType.Grenade:
                return true;
            default:
                return false;
        }
    }

    public void Apply(GameObject player)
    {
        if (player == null)
            return;

        switch (type)
        {
            case ConsumableType.Ammo:
                ApplyAmmo(player);
                break;
            case ConsumableType.Health:
                ApplyHealth(player);
                break;
            case ConsumableType.Grenade:
                ApplyGrenade(player);
                break;
        }
    }

    void ApplyAmmo(GameObject player)
    {
        Gun gun = GetCurrentGun(player);
        if (gun != null)
            gun.RestoreAmmo();
    }

    Gun GetCurrentGun(GameObject player)
    {
        GunController gunController = player.GetComponentInParent<GunController>();
        if (gunController == null)
            return null;

        return gunController.GetCurrentGun();
    }

    void ApplyHealth(GameObject player)
    {
        PlayerHealth playerHealth = player.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.Heal(amount);
    }

    void ApplyGrenade(GameObject player)
    {
        GrenadeThrower grenadeThrower = player.GetComponentInParent<GrenadeThrower>();
        if (grenadeThrower != null)
            grenadeThrower.AddGrenades(amount);
    }
}
