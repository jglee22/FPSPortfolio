using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float explosionDelay = 3f;    // ���� ���� �ð�
    public float explosionRadius = 5f;  // ���� ����
    public float explosionForce = 700f; // ���� ��
    public int damage = 50;             // ������

    public GameObject explosionEffect;  // ���� ����Ʈ ������

    private bool hasExploded = false;

    void Start()
    {
        // ���� �ð� �� ����
        Invoke(nameof(Explode), explosionDelay);
    }

    void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;

        // ���� ȿ�� ����
        Instantiate(explosionEffect, transform.position, Quaternion.identity);

        // �ֺ� ��ü�� ���� ��� �� ������ ����
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            // ���� ��� ����
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            // �� ������ ó��
            EnemyAI enemy = nearbyObject.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.EnemyTakeDamage(damage);
            }
        }

        // ���� �� ������Ʈ ����
        Destroy(gameObject, 0.1f);
    }

    void OnDrawGizmosSelected()
    {
        // ���� ���� �ð�ȭ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
