using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;      // �ִ� ü��
    public int currentHealth;       // ���� ü��
    public HealthBar healthBar;     // ü�� �� (UI ǥ��)

    public bool isPlayerDie = false;
    void Start()
    {
        currentHealth = maxHealth;   // ���� �� �ִ� ü������ ����
        healthBar.SetMaxHealth(maxHealth); // ü�¹� �ʱ�ȭ
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage; // ü�� ����
        healthBar.SetHealth(currentHealth); // ü�� UI ������Ʈ
        Debug.Log($"player hp : {currentHealth}");
        if (currentHealth <= 0)
        {
            Die(); // ü���� 0 ������ �� ��� ó��
        }
    }

    void Die()
    {
        Debug.Log("�÷��̾� ���!");
        isPlayerDie = true;
        // ���� ���� ó�� (�ʿ� �� �߰� ����)
    }
}
