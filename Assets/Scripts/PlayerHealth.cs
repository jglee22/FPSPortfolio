using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public HealthBar healthBar;
    public TextMeshProUGUI healthText;
    public bool isPlayerDie = false;

    CameraShake cameraShake;
    public float duration, magnitude;

    public DamageOverlay damageOverlay;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        cameraShake = Camera.main.GetComponent<CameraShake>();
        healthText.text = currentHealth + " / " + maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isPlayerDie)
            return;

        damageOverlay.ShowDamageEffect();

        cameraShake.Shake(duration, magnitude);

        currentHealth = Mathf.Max(0, currentHealth - damage);
        healthBar.SetHealth(currentHealth);
        healthText.text = currentHealth + " / " + maxHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isPlayerDie || amount <= 0)
            return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        healthBar.SetHealth(currentHealth);
        healthText.text = currentHealth + " / " + maxHealth;
    }

    public bool NeedsHealth()
    {
        return currentHealth < maxHealth;
    }

    void Die()
    {
        if (isPlayerDie)
            return;

        isPlayerDie = true;

        MenuManager menuManager = FindObjectOfType<MenuManager>();
        if (menuManager != null)
            menuManager.ShowGameOver();
    }
}
