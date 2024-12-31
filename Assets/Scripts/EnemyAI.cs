using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.AI;


public class EnemyAI : MonoBehaviour
{
    // 플레이어 및 기본 설정
    public Transform player; // 플레이어 위치
    private PlayerHealth playerHealth; // 플레이어 체력 관리
    public float attackRange = 2.0f; // 공격 범위
    public float moveSpeed = 3.5f;  // 이동 속도
    public int health = 100;        // 체력

    // 공격 설정
    public Transform leftAttackPoint;   // 왼손 공격 지점
    public Transform rightAttackPoint;  // 오른손 공격 지점
    public float attackRadius = 1.5f;   // 공격 반경
    public LayerMask playerLayer;       // 플레이어 레이어 설정
    public int attackDamage = 10;       // 공격 데미지

    public float viewAngle = 60f; // 시야각(도 단위)
    public float viewDistance = 10f; // 시야 거리
    public LayerMask obstacleMask; // 장애물 레이어

    // 상태 관리
    private NavMeshAgent agent;
    private Animator animator;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isRotatingAfterAttack = false;

    // 플레이어 상태 추가
    private bool isPlayerDead = false; // 플레이어 사망 여부

    // 추가: 적 타입 및 풀 매니저 참조
    private EnemyPoolManager poolManager;
    public string enemyType; // EnemyPoolManager에 정의된 타입 이름

    // 회전 관련 변수
    private Vector3 targetPosition; // 공격 시 고정할 플레이어 위치
    private Quaternion lookRotation; // 공격 방향 고정 회전 값
    public float rotationSpeed = 1f;

    public event System.Action OnDeath; // 사망 이벤트


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = FindObjectOfType<PlayerMovement>().transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        agent.speed = moveSpeed;

        // 풀 매니저 참조
        poolManager = FindObjectOfType<EnemyPoolManager>();

        // 웨이브 기반 강화 적용
        int waveNumber = poolManager.waveNumber; // 현재 웨이브 가져오기
        health += waveNumber * 10;              // 웨이브당 체력 증가
        moveSpeed += waveNumber * 0.1f;         // 웨이브당 속도 증가
        attackDamage += waveNumber * 2;         // 웨이브당 공격력 증가
        agent.speed = moveSpeed;                // 이동 속도 업데이트
    }

    void Update()
    {
        if (isDead) return; // 사망 시 동작 중지

        float distance = Vector3.Distance(transform.position, player.position);

        // **플레이어 사망 상태 체크**
        if (playerHealth.currentHealth <= 0)
        {
            isPlayerDead = true; // 플레이어 사망 처리
        }

        // 플레이어가 죽었으면 동작 멈춤
        if (isDead || isPlayerDead) // 적 또는 플레이어 사망 시
        {
            agent.isStopped = true;
            animator.SetBool("isMoving", false);
            animator.SetBool("isAttacking", false);
            if(!animator.GetBool("isIdle"))
            animator.SetBool("isIdle", true);

            Debug.Log("Player Dead ");
            return;
        }
        // 공격 도중에는 회전 고정, 상태 전환 차단
        if (isAttacking)
        {
            transform.rotation = lookRotation; // 공격 도중 회전 고정
            return;
        }

        // 공격 종료 후 회전 보정
        if (isRotatingAfterAttack)
        {
            RotateTowardsPlayer(); // 최신 위치로 회전 보정
            return;
        }

        // 체력 체크
        if (health <= 0)
        {
            EnemyDie();
            return;
        }

        // 공격 범위 검사
        if (distance <= attackRange)
        {
            Attack(); // 공격 실행
        }
        else
        {
            MoveToPlayer(); // 이동 실행
        }
    }

    void Attack()
    {
        if (!isAttacking) // 재진입 방지
        {
            isAttacking = true; // 공격 상태 고정
            agent.isStopped = true; // 이동 정지

            // 공격 시 플레이어 위치 고정
            targetPosition = player.position; // 공격 시작 시 위치 저장

            Vector3 direction =  (targetPosition - transform.position).normalized; // 방향 계산
            direction.y = 0;

            //lookRotation = Quaternion.LookRotation((targetPosition - transform.position).normalized);
            lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            // 공격 방향 고정
            //transform.rotation = lookRotation;

            // 애니메이션 실행
            animator.SetBool("isAttacking", true);
            animator.SetBool("isMoving", false);

            // 애니메이션 종료 후 처리
            StartCoroutine(WaitForAttackAnimation());
        }
    }

    // 공격 애니메이션 종료까지 대기
    IEnumerator WaitForAttackAnimation()
    {
        // 현재 애니메이션 상태 정보 가져오기
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;

        // 애니메이션 종료까지 대기
        yield return new WaitForSeconds(animationLength);

        // 공격 종료 후 회전 보정 활성화
        isRotatingAfterAttack = true;

        // 공격 상태 초기화
        ResetAttack();
    }

    void ResetAttack()
    {
        isAttacking = false; // 상태 초기화
        animator.SetBool("isAttacking", false);
        agent.isStopped = false; // 이동 재개
    }

    // 애니메이션 종료 후 회전 처리
    void RotateTowardsTarget()
    {
        // 공격 방향 유지
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        // 부드러운 회전 처리
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

        // 회전 완료 후 이동으로 전환
        if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
        {
            isRotatingAfterAttack = false; // 회전 종료
        }
    }
    // 공격 판정 (애니메이션 이벤트에서 호출)
    public void HitCheckLeft()
    {
        CheckHit(leftAttackPoint); // 왼손 타격
        Debug.Log($"enemy left attack");
    }

    public void HitCheckRight()
    {
        CheckHit(rightAttackPoint); // 오른손 타격
        Debug.Log($"enemy right attack");
    }

    void CheckHit(Transform attackPoint)
    {
        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRadius, playerLayer);
        foreach (Collider player in hitPlayers)
        {
            Debug.Log($"player : {player}");
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {   
                playerHealth.TakeDamage(attackDamage); // 데미지 적용
                Debug.Log("플레이어 피격!");
            }
        }
    }
    // 부드러운 회전 처리
    IEnumerator SmoothRotate(Quaternion targetRotation)
    {
        float rotateSpeed = 5f; // 회전 속도
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
            yield return null; // 다음 프레임 대기
        }
    }

    void MoveToPlayer()
    {
        isAttacking = false; // 공격 상태 해제
        agent.isStopped = false; // 이동 재개
        agent.SetDestination(player.position);
        Vector3 nextPos = agent.steeringTarget - transform.position;

        if (nextPos != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(nextPos.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
        // 이동 중에도 방향 업데이트
        //RotateTowardsPlayer();

        animator.SetBool("isMoving", true);
        animator.SetBool("isAttacking", false);
    }

    // 사망 처리
    void EnemyDie()
    {
        if (isDead) return;

        isDead = true;
        agent.isStopped = true;

        animator.SetBool("isMoving", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isDead", true);

        StartCoroutine(ReturnToPoolAfterDeath());

        // 이벤트 호출
        OnDeath?.Invoke();
    }

    IEnumerator ReturnToPoolAfterDeath()
    {
        yield return new WaitForSeconds(2f); // 사망 애니메이션 길이만큼 대기

        // 상태 초기화
        ResetEnemy();
        //yield return new WaitForSeconds(1f);
        // 풀로 반환
        poolManager.ReturnToPool(gameObject, enemyType);
    }

    // 상태 초기화
    void ResetEnemy()
    {
        isDead = false;
        health = 50; // 체력 초기화

        agent.isStopped = false;
        animator.SetBool("isDead", false);
        animator.SetBool("isMoving", false);
        animator.SetBool("isAttacking", false);
    }

    public void EnemyTakeDamage(int damage)
    {
        if(isDead) return;
        health -= damage;
        Debug.Log($"Enemy HP : {health}");
    }

    void OnDrawGizmosSelected()
    {
        if (leftAttackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(leftAttackPoint.position, attackRadius);
        }

        if (rightAttackPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(rightAttackPoint.position, attackRadius);
        }
    }

 
}
