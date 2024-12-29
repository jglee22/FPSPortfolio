using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPoolManager : MonoBehaviour
{
    [System.Serializable] // 적 종류별 데이터 관리
    public class EnemyType
    {
        public string name;            // 적 이름
        public GameObject prefab;      // 적 프리팹
        public int poolSize = 10;      // 초기 풀 크기
    }

    public EnemyType[] enemyTypes;     // 적 종류 배열
    public Transform[] spawnPoints;    // 스폰 위치 배열

    public int waveNumber = 1;
    public int enemiesPerWave = 5;
    private int enemiesSpawned = 0;

    private Dictionary<string, Queue<GameObject>> enemyPools; // 적 풀 관리 딕셔너리

    void Start()
    {
        // 딕셔너리 초기화
        enemyPools = new Dictionary<string, Queue<GameObject>>();

        // 적 종류별 풀 생성
        foreach (EnemyType enemyType in enemyTypes)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < enemyType.poolSize; i++)
            {
                GameObject enemy = Instantiate(enemyType.prefab);
                enemy.SetActive(false); // 초기에는 비활성화
                pool.Enqueue(enemy);
            }
            enemyPools[enemyType.name] = pool; // 딕셔너리에 추가
        }
        SpawnWave();
        // 주기적 스폰 테스트
        InvokeRepeating("SpawnEnemy", 1f, 1f); // 1초 후 시작, 1초 간격
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

            // 적 초기화 (풀에서 꺼낼 때)
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.enemyType = selectedEnemy.name; // 타입 지정
                enemyAI.EnemyTakeDamage(0); // 체력 초기화 (또는 별도 초기화 메서드 호출)
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
        enemiesPerWave += 2; // 웨이브마다 적 증가
    }

    // 풀로 반환
    public void ReturnToPool(GameObject enemy, string type)
    {
        enemy.SetActive(false);
        enemyPools[type].Enqueue(enemy); // 해당 타입 풀로 반환
    }
}
