using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;      // �ִ� ü��
    public int currentHealth;       // ���� ü��
    public HealthBar healthBar;     // ü�� �� (UI ǥ��)
    public TextMeshProUGUI healthText; //ü�� ��ġ
    public bool isPlayerDie = false;

    CameraShake cameraShake;
    public float duration, magnitude;   // �ǰݽ� ȭ�� ���� ȿ�� ���ӽð�,����

    public DamageOverlay damageOverlay; //�ǰݽ� ȭ�� �Ӱ� ���̰� �ϴ� ȿ��

    void Start()
    {
        currentHealth = maxHealth;   // ���� �� �ִ� ü������ ����
        healthBar.SetMaxHealth(maxHealth); // ü�¹� �ʱ�ȭ
        cameraShake = Camera.main.GetComponent<CameraShake>();
        healthText.text = currentHealth + " / " + maxHealth;
    }

    public void TakeDamage(int damage)
    {
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

    void Die()
    {
        Debug.Log("�÷��̾� ���!");
        isPlayerDie = true;
        // ���� ���� ó�� (�ʿ� �� �߰� ����)
    }
}
