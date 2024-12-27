using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.AI;
using static EnemyPoolManager;
public class EnemyAI : MonoBehaviour
{
    public Transform player; // �÷��̾� ��ġ
    public float attackRange = 2.0f; // ���� ����
    public float moveSpeed = 3.5f;  // �̵� �ӵ�
    public int health = 100;        // ü��

    public float viewAngle = 60f; // �þ߰�(�� ����)
    public float viewDistance = 10f; // �þ� �Ÿ�
    public LayerMask obstacleMask; // ��ֹ� ���̾�

    private NavMeshAgent agent;
    private Animator animator;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isRotatingAfterAttack = false;
    // �߰�: �� Ÿ�� �� Ǯ �Ŵ��� ����
    private EnemyPoolManager poolManager;

    private Vector3 targetPosition; // ���� �� ������ �÷��̾� ��ġ
    private Quaternion lookRotation; // ���� ���� ���� ȸ�� ��

    public string enemyType; // EnemyPoolManager�� ���ǵ� Ÿ�� �̸�

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = FindObjectOfType<PlayerMovement>().transform;
        agent.speed = moveSpeed;

        // Ǯ �Ŵ��� ����
        poolManager = FindObjectOfType<EnemyPoolManager>();
    }

    void Update()
    {
        if (isDead) return; // ��� �� ���� ����

        float distance = Vector3.Distance(transform.position, player.position);

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
            Die();
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
            lookRotation = Quaternion.LookRotation((targetPosition - transform.position).normalized);

            // ���� ���� ����
            transform.rotation = lookRotation;

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

        // �̵� �߿��� ���� ������Ʈ
        RotateTowardsPlayer();

        animator.SetBool("isMoving", true);
        animator.SetBool("isAttacking", false);
    }

    // ��� ó��
    void Die()
    {
        if (isDead) return;

        isDead = true;
        agent.isStopped = true;

        animator.SetBool("isMoving", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isDead", true);

        // ��� �ִϸ��̼� ��� �� Ǯ�� ��ȯ
        StartCoroutine(ReturnToPoolAfterDeath());
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
        health = 100; // ü�� �ʱ�ȭ

        agent.isStopped = false;
        animator.SetBool("isDead", false);
        animator.SetBool("isMoving", false);
        animator.SetBool("isAttacking", false);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
    }
}
