using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GrenadeThrower : MonoBehaviour
{
    public GameObject grenadePrefab; // ����ź ������
    public Transform throwPoint;     // ������ ��ġ
    public float throwForce = 20f;   // ������ ��

    public TextMeshProUGUI grenadeCountText;

    public int maxGrenadeCount = 3;
    public int currentGrenadeCount;     // ����ź ����
    public float throwCooldown = 1f; // ��ô ��ٿ�
    private bool isCooldown = false;

    private void Start()
    {
        currentGrenadeCount = maxGrenadeCount;
        grenadeCountText.text = " x " + maxGrenadeCount.ToString();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && currentGrenadeCount > 0 && !isCooldown)
        {
            ThrowGrenade(); // ����ź ������
            currentGrenadeCount--; // ����ź ���� ����
            grenadeCountText.text = " x " + currentGrenadeCount.ToString();
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
