using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class EnemyCounterManager : MonoBehaviour
{
    public static EnemyCounterManager Instance; // 싱글톤 인스턴스

    public int enemyCount = 0;                  // 현재 적 수
    public TextMeshProUGUI enemyCountText;      // UI 표시용 텍스트

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateEnemyCountUI();
    }

    public void AddEnemy()
    {
        enemyCount++;            // 적 수 증가
        UpdateEnemyCountUI();    // UI 업데이트
    }

    public void RemoveEnemy()
    {
        enemyCount--;            // 적 수 감소
        UpdateEnemyCountUI();    // UI 업데이트
    }

    void UpdateEnemyCountUI()
    {
        if (enemyCountText != null)
        {
            enemyCountText.text = "Enemies: " + enemyCount;
        }
    }
}
