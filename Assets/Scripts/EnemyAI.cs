using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    private PlayerHealth playerHealth;
    public float attackRange = 2.0f;
    public float moveSpeed = 3.5f;
    public int health = 100;

    public Transform leftAttackPoint;
    public Transform rightAttackPoint;
    public float attackRadius = 1.5f;
    public LayerMask playerLayer;
    public int attackDamage = 10;

    public float viewAngle = 60f;
    public float viewDistance = 10f;
    public LayerMask obstacleMask;

    public GameObject[] dropItems;
    public float dropChance = 1f;
    public float dropHeight = 1f;

    [SerializeField] int healthPerWave = 10;
    [SerializeField] float speedPerWave = 0.1f;
    [SerializeField] int damagePerWave = 2;
    [SerializeField] int bossHealthPerWave = 20;
    [SerializeField] int bossDamagePerWave = 5;
    [SerializeField] float bossSpeedBonus = 0.5f;

    private NavMeshAgent agent;
    private Animator animator;
    private CapsuleCollider capsuleCollider;
    private Collider[] bodyColliders;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isRotatingAfterAttack = false;
    private bool isPlayerDead = false;
    private Coroutine attackRoutine;
    private Coroutine hitStunRoutine;
    private Coroutine flashRoutine;
    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private EnemyPoolManager poolManager;
    public string enemyType;

    private Vector3 targetPosition;
    private Quaternion lookRotation;
    public float rotationSpeed = 1f;

    private int baseHealth;
    private int maxHealth;
    private float baseMoveSpeed;
    private int baseAttackDamage;
    private bool hasCachedBaseStats;
    private bool spawnedAsBoss;

    public event System.Action OnDeath;
    public event System.Action<int, int> OnHealthChanged;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => health;
    public bool IsBoss => spawnedAsBoss;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        bodyColliders = GetComponentsInChildren<Collider>(true);
        CacheBaseStats();
    }

    void Start()
    {
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            player = playerMovement.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        poolManager = FindObjectOfType<EnemyPoolManager>();
    }

    public void InitializeForSpawn(EnemyData enemyData, WaveData waveData)
    {
        CacheBaseStats();
        ResetCombatState();

        float healthMul = waveData != null ? waveData.healthMultiplier : 1f;
        float damageMul = waveData != null ? waveData.damageMultiplier : 1f;
        float speedMul = waveData != null ? waveData.speedMultiplier : 1f;

        int sourceHealth = enemyData != null ? enemyData.maxHealth : baseHealth;
        int sourceDamage = enemyData != null ? enemyData.attackDamage : baseAttackDamage;
        float sourceSpeed = enemyData != null ? enemyData.moveSpeed : baseMoveSpeed;

        health = Mathf.Max(1, Mathf.RoundToInt(sourceHealth * healthMul));
        attackDamage = Mathf.Max(1, Mathf.RoundToInt(sourceDamage * damageMul));
        moveSpeed = sourceSpeed * speedMul;
        spawnedAsBoss = enemyData != null && enemyData.isBoss;
        maxHealth = health;
        OnHealthChanged?.Invoke(health, maxHealth);

        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            agent.speed = moveSpeed;
            agent.ResetPath();
        }

        SetBodyCollision(true);

        if (animator != null)
        {
            animator.SetBool("isDead", false);
            animator.SetBool("isMoving", false);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isIdle", false);
        }
    }

    void Update()
    {
        if (isDead || player == null)
            return;

        if (playerHealth != null && playerHealth.currentHealth <= 0)
            isPlayerDead = true;

        if (isPlayerDead)
        {
            if (agent != null)
                agent.isStopped = true;
            if (animator != null)
            {
                animator.SetBool("isMoving", false);
                animator.SetBool("isAttacking", false);
                if (!animator.GetBool("isIdle"))
                    animator.SetBool("isIdle", true);
            }
            return;
        }

        if (isAttacking)
        {
            transform.rotation = lookRotation;
            return;
        }

        if (isRotatingAfterAttack)
        {
            RotateTowardsPlayer();
            return;
        }

        if (health <= 0)
        {
            EnemyDie();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= attackRange)
            Attack();
        else
            MoveToPlayer();
    }

    void Attack()
    {
        if (isAttacking)
            return;

        isAttacking = true;
        if (agent != null)
            agent.isStopped = true;

        targetPosition = player.position;
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

        if (animator != null)
        {
            animator.SetBool("isAttacking", true);
            animator.SetBool("isMoving", false);
        }

        attackRoutine = StartCoroutine(WaitForAttackAnimation());
    }

    IEnumerator WaitForAttackAnimation()
    {
        float animationLength = 1f;
        if (animator != null)
            animationLength = animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(animationLength);

        isRotatingAfterAttack = true;
        ResetAttack();
        attackRoutine = null;
    }

    void ResetAttack()
    {
        isAttacking = false;
        if (animator != null)
            animator.SetBool("isAttacking", false);
        if (agent != null)
            agent.isStopped = false;
    }

    void RotateTowardsTarget()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            isRotatingAfterAttack = false;
    }

    public void HitCheckLeft()
    {
        CheckHit(leftAttackPoint);
    }

    public void HitCheckRight()
    {
        CheckHit(rightAttackPoint);
    }

    void CheckHit(Transform attackPoint)
    {
        if (attackPoint == null)
            return;

        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRadius, playerLayer);
        foreach (Collider hitCollider in hitPlayers)
        {
            PlayerHealth targetHealth = hitCollider.GetComponent<PlayerHealth>();
            if (targetHealth != null)
                targetHealth.TakeDamage(attackDamage);
        }
    }

    IEnumerator SmoothRotate(Quaternion targetRotation)
    {
        float rotateSpeed = 5f;
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
            yield return null;
        }
    }

    void MoveToPlayer()
    {
        isAttacking = false;
        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            Vector3 nextPos = agent.steeringTarget - transform.position;

            if (nextPos != Vector3.zero)
            {
                Quaternion moveLookRotation = Quaternion.LookRotation(nextPos.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, moveLookRotation, Time.deltaTime * 10f);
            }
        }

        if (animator != null)
        {
            animator.SetBool("isMoving", true);
            animator.SetBool("isAttacking", false);
        }
    }

    void EnemyDie()
    {
        if (isDead)
            return;

        isDead = true;
        DropItem();

        int score = spawnedAsBoss ? 300 : 100;
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(score);

        StopHitStun();
        CombatHitFeedback.PlayDeathSound(transform.position);

        SetBodyCollision(false);
        if (agent != null)
        {
            agent.ResetPath();
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isDead", true);
        }

        StartCoroutine(ReturnToPoolAfterDeath());
        OnDeath?.Invoke();
    }

    IEnumerator ReturnToPoolAfterDeath()
    {
        yield return new WaitForSeconds(2f);
        ResetCombatState();

        if (poolManager != null)
            poolManager.ReturnToPool(gameObject, enemyType);
        else
            gameObject.SetActive(false);
    }

    void ResetCombatState()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        StopHitStun();
        StopHitFlash();

        isDead = false;
        isAttacking = false;
        isRotatingAfterAttack = false;
        isPlayerDead = false;
        spawnedAsBoss = false;
        health = baseHealth;
        maxHealth = baseHealth;
        moveSpeed = baseMoveSpeed;
        attackDamage = baseAttackDamage;
    }

    void SetBodyCollision(bool enabled)
    {
        if (bodyColliders == null)
            bodyColliders = GetComponentsInChildren<Collider>(true);

        for (int i = 0; i < bodyColliders.Length; i++)
        {
            if (bodyColliders[i] != null)
                bodyColliders[i].enabled = enabled;
        }
    }

    public bool EnemyTakeDamage(int damageAmount)
    {
        return EnemyTakeDamage(damageAmount, transform.position + Vector3.up, Vector3.up);
    }

    public bool EnemyTakeDamage(int damageAmount, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (isDead)
            return false;

        health -= damageAmount;
        if (health < 0)
            health = 0;

        OnHealthChanged?.Invoke(health, maxHealth);

        bool killed = health <= 0;
        CombatHitFeedback.PlayBodyFeedback(this, hitPoint, hitNormal, killed);
        return killed;
    }

    public void PlayHitFlash(Color flashColor, float duration)
    {
        if (duration <= 0f)
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(HitFlashRoutine(flashColor, duration));
    }

    public void PlayHitStun(float duration)
    {
        if (isDead || duration <= 0f || agent == null || !agent.enabled)
            return;

        if (hitStunRoutine != null)
            StopCoroutine(hitStunRoutine);

        hitStunRoutine = StartCoroutine(HitStunRoutine(duration));
    }

    IEnumerator HitStunRoutine(float duration)
    {
        agent.speed = 0f;
        yield return new WaitForSeconds(duration);
        hitStunRoutine = null;
        if (!isDead && agent != null && agent.enabled)
            agent.speed = moveSpeed;
    }

    void StopHitStun()
    {
        if (hitStunRoutine != null)
        {
            StopCoroutine(hitStunRoutine);
            hitStunRoutine = null;
        }

        if (agent != null && agent.enabled)
            agent.speed = moveSpeed;
    }

    void StopHitFlash()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        ClearHitFlash();
    }

    IEnumerator HitFlashRoutine(Color flashColor, float duration)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - (elapsed / duration);
            Color emission = flashColor * (2f * t);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                    continue;

                renderers[i].GetPropertyBlock(block);
                block.SetColor(EmissionColorId, emission);
                renderers[i].SetPropertyBlock(block);
            }

            yield return null;
        }

        flashRoutine = null;
        ClearHitFlash();
    }

    void ClearHitFlash()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].SetPropertyBlock(null);
        }
    }

    void DropItem()
    {
        if (dropItems == null || dropItems.Length == 0)
            return;

        if (Random.value < dropChance)
        {
            int randomIndex = Random.Range(0, dropItems.Length);
            Instantiate(dropItems[randomIndex], transform.position + Vector3.up * dropHeight, Quaternion.identity);
        }
    }

    void CacheBaseStats()
    {
        if (hasCachedBaseStats)
            return;

        baseHealth = health;
        baseMoveSpeed = moveSpeed;
        baseAttackDamage = attackDamage;
        hasCachedBaseStats = true;
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
