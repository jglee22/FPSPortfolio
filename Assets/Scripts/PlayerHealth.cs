using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;      // �ִ� ü��
    public int currentHealth;       // ���� ü��
    public HealthBar healthBar;     // ü�� �� (UI ǥ��)

    public bool isPlayerDie = false;

    CameraShake cameraShake;
    public float duration, magnitude;

    public DamageOverlay damageOverlay;

    void Start()
    {
        currentHealth = maxHealth;   // ���� �� �ִ� ü������ ����
        healthBar.SetMaxHealth(maxHealth); // ü�¹� �ʱ�ȭ
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
        Debug.Log("�÷��̾� ���!");
        isPlayerDie = true;
        // ���� ���� ó�� (�ʿ� �� �߰� ����)
    }
}
