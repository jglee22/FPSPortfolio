using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class EnemyCounterManager : MonoBehaviour
{
    public static EnemyCounterManager Instance; // �̱��� �ν��Ͻ�

    public int enemyCount = 0;                  // ���� �� ��
    public TextMeshProUGUI enemyCountText;      // UI ǥ�ÿ� �ؽ�Ʈ

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
        enemyCount++;            // �� �� ����
        UpdateEnemyCountUI();    // UI ������Ʈ
    }

    public void RemoveEnemy()
    {
        enemyCount--;            // �� �� ����
        UpdateEnemyCountUI();    // UI ������Ʈ
    }

    void UpdateEnemyCountUI()
    {
        if (enemyCountText != null)
        {
            enemyCountText.text = "Enemies: " + enemyCount;
        }
    }
}
