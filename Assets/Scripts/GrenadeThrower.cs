using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeThrower : MonoBehaviour
{
    public GameObject grenadePrefab; // ����ź ������
    public Transform throwPoint;     // ������ ��ġ
    public float throwForce = 20f;   // ������ ��

    public int grenadeCount = 3;     // ����ź ����
    public float throwCooldown = 1f; // ��ô ��ٿ�
    private bool isCooldown = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && grenadeCount > 0 && !isCooldown)
        {
            ThrowGrenade(); // ����ź ������
            grenadeCount--; // ����ź ���� ����
            StartCoroutine(ThrowCooldown());
        }
    }

    void ThrowGrenade()
    {
        // ����ź ����
        GameObject grenade = Instantiate(grenadePrefab, throwPoint.position, throwPoint.rotation);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();

        // �� ����
        rb.AddForce(throwPoint.forward * throwForce, ForceMode.VelocityChange);
    }

    IEnumerator ThrowCooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(throwCooldown);
        isCooldown = false;
    }
}
