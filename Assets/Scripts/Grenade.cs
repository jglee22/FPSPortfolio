using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float explosionDelay = 3f;    // 폭발 지연 시간
    public float explosionRadius = 5f;  // 폭발 범위
    public float explosionForce = 700f; // 폭발 힘
    public int damage = 50;             // 데미지

    public GameObject explosionEffect;  // 폭발 이펙트 프리팹

    private bool hasExploded = false;

    void Start()
    {
        // 일정 시간 후 폭발
        Invoke(nameof(Explode), explosionDelay);
    }

    void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;

        // 폭발 효과 생성
        Instantiate(explosionEffect, transform.position, Quaternion.identity);

        // 주변 객체에 물리 충격 및 데미지 적용
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            // 물리 충격 적용
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            // 적 데미지 처리
            EnemyAI enemy = nearbyObject.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.EnemyTakeDamage(damage);
            }
        }

        // 폭발 후 오브젝트 제거
        Destroy(gameObject, 0.1f);
    }

    void OnDrawGizmosSelected()
    {
        // 폭발 범위 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
