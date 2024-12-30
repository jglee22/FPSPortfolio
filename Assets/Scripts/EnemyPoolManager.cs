using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class EnemyPoolManager : MonoBehaviour
{
    [System.Serializable] // �� ������ ������ ����
    public class EnemyType
    {
        public string name;            // �� �̸�
        public GameObject prefab;      // �� ������
        public int poolSize = 10;      // �ʱ� Ǯ ũ��
    }

    public Transform enemyContainer;
    public EnemyType[] enemyTypes;     // �� ���� �迭
    public Transform[] spawnPoints;    // ���� ��ġ �迭

    public int waveNumber = 1;
    public int enemiesPerWave = 5;
    private int enemiesSpawned = 0;


    public TextMeshProUGUI waveText;        // ���̺� ���� ǥ�� UI
    public float waveDelay = 5f; // ���̺� �� ��� �ð�
    private bool isWaveActive = false; // ���̺� ���� ���� Ȯ��
    private int enemiesAlive = 0; // ���� ����ִ� �� ��

    private Dictionary<string, Queue<GameObject>> enemyPools; // �� Ǯ ���� ��ųʸ�


    void Start()
    {
        // �ʱ�ȭ
        enemyPools = new Dictionary<string, Queue<GameObject>>();

        foreach (EnemyType enemyType in enemyTypes)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < enemyType.poolSize; i++)
            {
                GameObject enemy = Instantiate(enemyType.prefab);
                enemy.SetActive(false);
                pool.Enqueue(enemy);
            }
            enemyPools[enemyType.name] = pool;
        }
        SpawnWave();         // ���̺� ����
        UpdateWaveUI();      // UI ������Ʈ
        isWaveActive = true; // ���̺� Ȱ��ȭ �߰�
    }
    void Update()
    {
        // ���� ��� óġ�Ǿ���, ���̺갡 Ȱ�� �����̸� ���� ���̺� ����
        if (isWaveActive && enemiesAlive <= 0)
        {
            StartCoroutine(StartNextWave());
        }
    }

    IEnumerator StartNextWave()
    {
        isWaveActive = false; // ���̺� ���� ó��
        yield return new WaitForSeconds(waveDelay); // ��� �ð� �߰�

        waveNumber++; // ���̺� �� ����
        enemiesPerWave += 2; // ���̺긶�� �� �� ����
        SpawnWave(); // ���ο� ���̺� ����

        isWaveActive = true; // ���̺� Ȱ��ȭ
        UpdateWaveUI(); // UI ������Ʈ
    }
    void SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy();
        }

        enemiesAlive = enemiesPerWave; // ��� �ִ� �� �� �ʱ�ȭ
    }

    void SpawnEnemy()
    {
        int enemyTypeIndex = Random.Range(0, enemyTypes.Length);
        EnemyType selectedEnemy = enemyTypes[enemyTypeIndex];

        // Ǯ ũ�� ���� Ȯ��
        if (enemyPools[selectedEnemy.name].Count == 0) // Ǯ�� ���� ���� ���
        {
            ExpandPool(selectedEnemy); // Ǯ Ȯ��
        }

        int spawnIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[spawnIndex];

        GameObject enemy = enemyPools[selectedEnemy.name].Dequeue();
        enemy.transform.position = spawnPoint.position;
        enemy.transform.rotation = spawnPoint.rotation;
        enemy.SetActive(true);

        // �� �ʱ�ȭ �� ��� �� �ݹ� ����
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.enemyType = selectedEnemy.name;
            enemyAI.EnemyTakeDamage(0);

            // ���� �̺�Ʈ ���� �� �� �̺�Ʈ ����
            enemyAI.OnDeath -= EnemyDied;
            enemyAI.OnDeath += EnemyDied;
        }
    }

    // Ǯ Ȯ�� �Լ� �߰�
    void ExpandPool(EnemyType enemyType)
    {
        for (int i = 0; i < 5; i++) // ���� �� 5���� �߰�
        {
            GameObject enemy = Instantiate(enemyType.prefab, enemyContainer);
            enemy.SetActive(false);
            enemyPools[enemyType.name].Enqueue(enemy);
        }
        Debug.Log($"{enemyType.name} Ǯ Ȯ��! �� ũ��: {enemyPools[enemyType.name].Count}");
    }

    void UpdateWaveUI()
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {waveNumber}";
            Debug.Log($"���̺� ������Ʈ: {waveNumber}");
        }
    }
    void EnemyDied()
    {
        enemiesAlive--;
        Debug.Log($"�� ���. ���� ��: {enemiesAlive}");
    }
    // Ǯ�� ��ȯ
    public void ReturnToPool(GameObject enemy, string type)
    {
        enemy.SetActive(false);
        enemyPools[type].Enqueue(enemy);
        Debug.Log($"{type} ��ȯ. ���� Ǯ ũ��: {enemyPools[type].Count}");
    }
}
