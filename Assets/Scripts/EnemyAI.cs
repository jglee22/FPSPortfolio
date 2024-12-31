using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.AI;


public class EnemyAI : MonoBehaviour
{
    // �÷��̾� �� �⺻ ����
    public Transform player; // �÷��̾� ��ġ
    private PlayerHealth playerHealth; // �÷��̾� ü�� ����
    public float attackRange = 2.0f; // ���� ����
    public float moveSpeed = 3.5f;  // �̵� �ӵ�
    public int health = 100;        // ü��

    // ���� ����
    public Transform leftAttackPoint;   // �޼� ���� ����
    public Transform rightAttackPoint;  // ������ ���� ����
    public float attackRadius = 1.5f;   // ���� �ݰ�
    public LayerMask playerLayer;       // �÷��̾� ���̾� ����
    public int attackDamage = 10;       // ���� ������

    public float viewAngle = 60f; // �þ߰�(�� ����)
    public float viewDistance = 10f; // �þ� �Ÿ�
    public LayerMask obstacleMask; // ��ֹ� ���̾�

    // ���� ����
    private NavMeshAgent agent;
    private Animator animator;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isRotatingAfterAttack = false;

    // �÷��̾� ���� �߰�
    private bool isPlayerDead = false; // �÷��̾� ��� ����

    // �߰�: �� Ÿ�� �� Ǯ �Ŵ��� ����
    private EnemyPoolManager poolManager;
    public string enemyType; // EnemyPoolManager�� ���ǵ� Ÿ�� �̸�

    // ȸ�� ���� ����
    private Vector3 targetPosition; // ���� �� ������ �÷��̾� ��ġ
    private Quaternion lookRotation; // ���� ���� ���� ȸ�� ��
    public float rotationSpeed = 1f;

    public event System.Action OnDeath; // ��� �̺�Ʈ


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = FindObjectOfType<PlayerMovement>().transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        agent.speed = moveSpeed;

        // Ǯ �Ŵ��� ����
        poolManager = FindObjectOfType<EnemyPoolManager>();

        // ���̺� ��� ��ȭ ����
        int waveNumber = poolManager.waveNumber; // ���� ���̺� ��������
        health += waveNumber * 10;              // ���̺�� ü�� ����
        moveSpeed += waveNumber * 0.1f;         // ���̺�� �ӵ� ����
        attackDamage += waveNumber * 2;         // ���̺�� ���ݷ� ����
        agent.speed = moveSpeed;                // �̵� �ӵ� ������Ʈ
    }

    void Update()
    {
        if (isDead) return; // ��� �� ���� ����

        float distance = Vector3.Distance(transform.position, player.position);

        // **�÷��̾� ��� ���� üũ**
        if (playerHealth.currentHealth <= 0)
        {
            isPlayerDead = true; // �÷��̾� ��� ó��
        }

        // �÷��̾ �׾����� ���� ����
        if (isDead || isPlayerDead) // �� �Ǵ� �÷��̾� ��� ��
        {
            agent.isStopped = true;
            animator.SetBool("isMoving", false);
            animator.SetBool("isAttacking", false);
            if(!animator.GetBool("isIdle"))
            animator.SetBool("isIdle", true);

            Debug.Log("Player Dead ");
            return;
        }
        // ���� ���߿��� ȸ�� ����, ���� ��ȯ ����
        if (isAttacking)
        {
            transform.rotation = lookRotation; // ���� ���� ȸ�� ����
            return;
        }

        // ���� ���� �� ȸ�� ����
        if (isRotatingAfterAttack)
        {
            RotateTowardsPlayer(); // �ֽ� ��ġ�� ȸ�� ����
            return;
        }

        // ü�� üũ
        if (health <= 0)
        {
            EnemyDie();
            return;
        }

        // ���� ���� �˻�
        if (distance <= attackRange)
        {
            Attack(); // ���� ����
        }
        else
        {
            MoveToPlayer(); // �̵� ����
        }
    }

    void Attack()
    {
        if (!isAttacking) // ������ ����
        {
            isAttacking = true; // ���� ���� ����
            agent.isStopped = true; // �̵� ����

            // ���� �� �÷��̾� ��ġ ����
            targetPosition = player.position; // ���� ���� �� ��ġ ����

            Vector3 direction =  (targetPosition - transform.position).normalized; // ���� ���
            direction.y = 0;

            //lookRotation = Quaternion.LookRotation((targetPosition - transform.position).normalized);
            lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            // ���� ���� ����
            //transform.rotation = lookRotation;

            // �ִϸ��̼� ����
            animator.SetBool("isAttacking", true);
            animator.SetBool("isMoving", false);

            // �ִϸ��̼� ���� �� ó��
            StartCoroutine(WaitForAttackAnimation());
        }
    }

    // ���� �ִϸ��̼� ������� ���
    IEnumerator WaitForAttackAnimation()
    {
        // ���� �ִϸ��̼� ���� ���� ��������
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;

        // �ִϸ��̼� ������� ���
        yield return new WaitForSeconds(animationLength);

        // ���� ���� �� ȸ�� ���� Ȱ��ȭ
        isRotatingAfterAttack = true;

        // ���� ���� �ʱ�ȭ
        ResetAttack();
    }

    void ResetAttack()
    {
        isAttacking = false; // ���� �ʱ�ȭ
        animator.SetBool("isAttacking", false);
        agent.isStopped = false; // �̵� �簳
    }

    // �ִϸ��̼� ���� �� ȸ�� ó��
    void RotateTowardsTarget()
    {
        // ���� ���� ����
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        // �ε巯�� ȸ�� ó��
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

        // ȸ�� �Ϸ� �� �̵����� ��ȯ
        if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
        {
            isRotatingAfterAttack = false; // ȸ�� ����
        }
    }
    // ���� ���� (�ִϸ��̼� �̺�Ʈ���� ȣ��)
    public void HitCheckLeft()
    {
        CheckHit(leftAttackPoint); // �޼� Ÿ��
        Debug.Log($"enemy left attack");
    }

    public void HitCheckRight()
    {
        CheckHit(rightAttackPoint); // ������ Ÿ��
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
                playerHealth.TakeDamage(attackDamage); // ������ ����
                Debug.Log("�÷��̾� �ǰ�!");
            }
        }
    }
    // �ε巯�� ȸ�� ó��
    IEnumerator SmoothRotate(Quaternion targetRotation)
    {
        float rotateSpeed = 5f; // ȸ�� �ӵ�
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
            yield return null; // ���� ������ ���
        }
    }

    void MoveToPlayer()
    {
        isAttacking = false; // ���� ���� ����
        agent.isStopped = false; // �̵� �簳
        agent.SetDestination(player.position);
        Vector3 nextPos = agent.steeringTarget - transform.position;

        if (nextPos != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(nextPos.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
        // �̵� �߿��� ���� ������Ʈ
        //RotateTowardsPlayer();

        animator.SetBool("isMoving", true);
        animator.SetBool("isAttacking", false);
    }

    // ��� ó��
    void EnemyDie()
    {
        if (isDead) return;

        isDead = true;
        agent.isStopped = true;

        animator.SetBool("isMoving", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isDead", true);

        StartCoroutine(ReturnToPoolAfterDeath());

        // �̺�Ʈ ȣ��
        OnDeath?.Invoke();
    }

    IEnumerator ReturnToPoolAfterDeath()
    {
        yield return new WaitForSeconds(2f); // ��� �ִϸ��̼� ���̸�ŭ ���

        // ���� �ʱ�ȭ
        ResetEnemy();
        //yield return new WaitForSeconds(1f);
        // Ǯ�� ��ȯ
        poolManager.ReturnToPool(gameObject, enemyType);
    }

    // ���� �ʱ�ȭ
    void ResetEnemy()
    {
        isDead = false;
        health = 50; // ü�� �ʱ�ȭ

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
