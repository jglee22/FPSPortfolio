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
        GunController gunController = player.GetComponentInParent<GunController>();
        if (gunController == null)
            return;

        Gun gun = gunController.GetCurrentGun();
        if (gun != null)
            gun.RestoreAmmo();
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
