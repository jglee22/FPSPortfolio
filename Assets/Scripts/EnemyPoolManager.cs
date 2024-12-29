using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPoolManager : MonoBehaviour
{
    [System.Serializable] // �� ������ ������ ����
    public class EnemyType
    {
        public string name;            // �� �̸�
        public GameObject prefab;      // �� ������
        public int poolSize = 10;      // �ʱ� Ǯ ũ��
    }

    public EnemyType[] enemyTypes;     // �� ���� �迭
    public Transform[] spawnPoints;    // ���� ��ġ �迭

    public int waveNumber = 1;
    public int enemiesPerWave = 5;
    private int enemiesSpawned = 0;

    private Dictionary<string, Queue<GameObject>> enemyPools; // �� Ǯ ���� ��ųʸ�

    void Start()
    {
        // ��ųʸ� �ʱ�ȭ
        enemyPools = new Dictionary<string, Queue<GameObject>>();

        // �� ������ Ǯ ����
        foreach (EnemyType enemyType in enemyTypes)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < enemyType.poolSize; i++)
            {
                GameObject enemy = Instantiate(enemyType.prefab);
                enemy.SetActive(false); // �ʱ⿡�� ��Ȱ��ȭ
                pool.Enqueue(enemy);
            }
            enemyPools[enemyType.name] = pool; // ��ųʸ��� �߰�
        }
        SpawnWave();
        // �ֱ��� ���� �׽�Ʈ
        InvokeRepeating("SpawnEnemy", 1f, 1f); // 1�� �� ����, 1�� ����
    }

    void SpawnEnemy()
    {
        int enemyTypeIndex = Random.Range(0, enemyTypes.Length);
        EnemyType selectedEnemy = enemyTypes[enemyTypeIndex];

        int spawnIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[spawnIndex];

        if (enemyPools[selectedEnemy.name].Count > 0)
        {
            GameObject enemy = enemyPools[selectedEnemy.name].Dequeue();
            enemy.transform.position = spawnPoint.position;
            enemy.transform.rotation = spawnPoint.rotation;
            enemy.SetActive(true);

            // �� �ʱ�ȭ (Ǯ���� ���� ��)
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.enemyType = selectedEnemy.name; // Ÿ�� ����
                enemyAI.EnemyTakeDamage(0); // ü�� �ʱ�ȭ (�Ǵ� ���� �ʱ�ȭ �޼��� ȣ��)
            }
        }
    }
    void SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy();
            enemiesSpawned++;
        }

        waveNumber++;
        enemiesPerWave += 2; // ���̺긶�� �� ����
    }

    // Ǯ�� ��ȯ
    public void ReturnToPool(GameObject enemy, string type)
    {
        enemy.SetActive(false);
        enemyPools[type].Enqueue(enemy); // �ش� Ÿ�� Ǯ�� ��ȯ
    }
}
