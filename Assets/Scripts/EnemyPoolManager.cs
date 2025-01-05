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
        public bool isBoss = false;
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

    public bool isTestBoss = false;

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

        // **�� ��ȭ ����**
        foreach (EnemyType enemyType in enemyTypes)
        {
            enemyType.poolSize += 5; // ���̺긶�� Ǯ ũ�� Ȯ��
        }

        SpawnWave(); // ���ο� ���̺� ����
        isWaveActive = true; // ���̺� Ȱ��ȭ
        UpdateWaveUI(); // UI ������Ʈ
    }
    void SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            if (waveNumber % 3 == 0 && i == enemiesPerWave - 1) // 3���̺긶�� ������ ���� ������ ����
            {
                SpawnEnemy(true); // ���� ��ȯ
            }
            else
            {
                SpawnEnemy(false); // �Ϲ� �� ��ȯ
            }
        }
        enemiesAlive = enemiesPerWave; // ��� �ִ� �� �� �ʱ�ȭ
    }

    void SpawnEnemy(bool isBoss)
    {
        EnemyType selectedEnemy;

        if (isBoss) // ���� ��ȯ
        {
            selectedEnemy = System.Array.Find(enemyTypes, e => e.isBoss); // ���� Ÿ�� ã��
        }
        else
        {
            int enemyTypeIndex = Random.Range(0, enemyTypes.Length);
            selectedEnemy = enemyTypes[enemyTypeIndex];
            while (selectedEnemy.isBoss) // �Ϲ� �� �� ����
            {
                enemyTypeIndex = Random.Range(0, enemyTypes.Length);
                selectedEnemy = enemyTypes[enemyTypeIndex];
            }
        }

        // Ǯ ũ�� ���� Ȯ��
        if (enemyPools[selectedEnemy.name].Count == 0)
        {
            ExpandPool(selectedEnemy);
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

            // **���� ��ȭ ���� �߰�**
            if (isBoss)
            {
                enemyAI.health += waveNumber * 20;     // �� ���� ü��
                enemyAI.attackDamage += waveNumber * 5; // �� ���� ���ݷ�
                enemyAI.moveSpeed += 0.5f;            // �� ���� �ӵ�
            }
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
