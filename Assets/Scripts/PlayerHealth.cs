using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;      // 최대 체력
    public int currentHealth;       // 현재 체력
    public HealthBar healthBar;     // 체력 바 (UI 표시)

    public bool isPlayerDie = false;

    CameraShake cameraShake;
    public float duration, magnitude;

    public DamageOverlay damageOverlay;

    void Start()
    {
        currentHealth = maxHealth;   // 시작 시 최대 체력으로 설정
        healthBar.SetMaxHealth(maxHealth); // 체력바 초기화
        cameraShake = Camera.main.GetComponent<CameraShake>();
    }

    public void TakeDamage(int damage)
    {
        damageOverlay.ShowDamageEffect();

        cameraShake.Shake(duration, magnitude);

        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("플레이어 사망!");
        isPlayerDie = true;
        // 게임 종료 처리 (필요 시 추가 구현)
    }
}
