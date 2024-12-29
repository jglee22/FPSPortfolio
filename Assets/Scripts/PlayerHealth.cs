using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;      // 최대 체력
    public int currentHealth;       // 현재 체력
    public HealthBar healthBar;     // 체력 바 (UI 표시)

    public bool isPlayerDie = false;
    void Start()
    {
        currentHealth = maxHealth;   // 시작 시 최대 체력으로 설정
        healthBar.SetMaxHealth(maxHealth); // 체력바 초기화
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage; // 체력 감소
        healthBar.SetHealth(currentHealth); // 체력 UI 업데이트
        Debug.Log($"player hp : {currentHealth}");
        if (currentHealth <= 0)
        {
            Die(); // 체력이 0 이하일 때 사망 처리
        }
    }

    void Die()
    {
        Debug.Log("플레이어 사망!");
        isPlayerDie = true;
        // 게임 종료 처리 (필요 시 추가 구현)
    }
}
