using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;      // 최대 체력
    public int currentHealth;       // 현재 체력
    public HealthBar healthBar;     // 체력 바 (UI 표시)
    public TextMeshProUGUI healthText; //체력 수치
    public bool isPlayerDie = false;

    CameraShake cameraShake;
    public float duration, magnitude;   // 피격시 화면 진동 효과 지속시간,강도

    public DamageOverlay damageOverlay; //피격시 화면 붉게 보이게 하는 효과

    void Start()
    {
        currentHealth = maxHealth;   // 시작 시 최대 체력으로 설정
        healthBar.SetMaxHealth(maxHealth); // 체력바 초기화
        cameraShake = Camera.main.GetComponent<CameraShake>();
        healthText.text = currentHealth + " / " + maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isPlayerDie)
            return;

        damageOverlay.ShowDamageEffect();

        cameraShake.Shake(duration, magnitude);

        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
        
        healthText.text = currentHealth + " / " + maxHealth;

        if (currentHealth < 0)
            healthText.text = 0 + " / " + maxHealth;

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
