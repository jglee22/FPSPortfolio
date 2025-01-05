using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class EnemyPoolManager : MonoBehaviour
{
    [System.Serializable] // 적 종류별 데이터 관리
    public class EnemyType
    {
        public string name;            // 적 이름
        public GameObject prefab;      // 적 프리팹
        public int poolSize = 10;      // 초기 풀 크기
        public bool isBoss = false;
    }

    public Transform enemyContainer;
    public EnemyType[] enemyTypes;     // 적 종류 배열
    public Transform[] spawnPoints;    // 스폰 위치 배열

    public int waveNumber = 1;
    public int enemiesPerWave = 5;
    private int enemiesSpawned = 0;


    public TextMeshProUGUI waveText;        // 웨이브 상태 표시 UI
    public float waveDelay = 5f; // 웨이브 간 대기 시간
    private bool isWaveActive = false; // 웨이브 진행 여부 확인
    private int enemiesAlive = 0; // 현재 살아있는 적 수

    private Dictionary<string, Queue<GameObject>> enemyPools; // 적 풀 관리 딕셔너리

    public bool isTestBoss = false;

    void Start()
    {
        // 초기화
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
        SpawnWave();         // 웨이브 시작
        UpdateWaveUI();      // UI 업데이트
        isWaveActive = true; // 웨이브 활성화 추가
    }
    void Update()
    {
        // 적이 모두 처치되었고, 웨이브가 활성 상태이면 다음 웨이브 시작
        if (isWaveActive && enemiesAlive <= 0)
        {
            StartCoroutine(StartNextWave());
        }
    }

    IEnumerator StartNextWave()
    {
        isWaveActive = false; // 웨이브 종료 처리
        yield return new WaitForSeconds(waveDelay); // 대기 시간 추가

        waveNumber++; // 웨이브 수 증가
        enemiesPerWave += 2; // 웨이브마다 적 수 증가

        // **적 강화 적용**
        foreach (EnemyType enemyType in enemyTypes)
        {
            enemyType.poolSize += 5; // 웨이브마다 풀 크기 확장
        }

        SpawnWave(); // 새로운 웨이브 시작
        isWaveActive = true; // 웨이브 활성화
        UpdateWaveUI(); // UI 업데이트
    }
    void SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            if (waveNumber % 3 == 0 && i == enemiesPerWave - 1) // 3웨이브마다 마지막 적을 보스로 변경
            {
                SpawnEnemy(true); // 보스 소환
            }
            else
            {
                SpawnEnemy(false); // 일반 적 소환
            }
        }
        enemiesAlive = enemiesPerWave; // 살아 있는 적 수 초기화
    }

    void SpawnEnemy(bool isBoss)
    {
        EnemyType selectedEnemy;

        if (isBoss) // 보스 소환
        {
            selectedEnemy = System.Array.Find(enemyTypes, e => e.isBoss); // 보스 타입 찾기
        }
        else
        {
            int enemyTypeIndex = Random.Range(0, enemyTypes.Length);
            selectedEnemy = enemyTypes[enemyTypeIndex];
            while (selectedEnemy.isBoss) // 일반 적 중 선택
            {
                enemyTypeIndex = Random.Range(0, enemyTypes.Length);
                selectedEnemy = enemyTypes[enemyTypeIndex];
            }
        }

        // 풀 크기 동적 확장
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

        // 적 초기화 및 사망 시 콜백 연결
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.enemyType = selectedEnemy.name;
            enemyAI.EnemyTakeDamage(0);

            // 기존 이벤트 제거 후 새 이벤트 연결
            enemyAI.OnDeath -= EnemyDied;
            enemyAI.OnDeath += EnemyDied;

            // **보스 강화 설정 추가**
            if (isBoss)
            {
                enemyAI.health += waveNumber * 20;     // 더 높은 체력
                enemyAI.attackDamage += waveNumber * 5; // 더 높은 공격력
                enemyAI.moveSpeed += 0.5f;            // 더 빠른 속도
            }
        }
    }

    // 풀 확장 함수 추가
    void ExpandPool(EnemyType enemyType)
    {
        for (int i = 0; i < 5; i++) // 부족 시 5개씩 추가
        {
            GameObject enemy = Instantiate(enemyType.prefab, enemyContainer);
            enemy.SetActive(false);
            enemyPools[enemyType.name].Enqueue(enemy);
        }
        Debug.Log($"{enemyType.name} 풀 확장! 새 크기: {enemyPools[enemyType.name].Count}");
    }

    void UpdateWaveUI()
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {waveNumber}";
            Debug.Log($"웨이브 업데이트: {waveNumber}");
        }
    }
    void EnemyDied()
    {
        enemiesAlive--;
        Debug.Log($"적 사망. 남은 적: {enemiesAlive}");
    }
    // 풀로 반환
    public void ReturnToPool(GameObject enemy, string type)
    {
        enemy.SetActive(false);
        enemyPools[type].Enqueue(enemy);
        Debug.Log($"{type} 반환. 현재 풀 크기: {enemyPools[type].Count}");
    }
}
