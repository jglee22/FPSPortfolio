using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeThrower : MonoBehaviour
{
    public GameObject grenadePrefab; // 수류탄 프리팹
    public Transform throwPoint;     // 던지는 위치
    public float throwForce = 20f;   // 던지는 힘

    public int grenadeCount = 3;     // 수류탄 개수
    public float throwCooldown = 1f; // 투척 쿨다운
    private bool isCooldown = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && grenadeCount > 0 && !isCooldown)
        {
            ThrowGrenade(); // 수류탄 던지기
            grenadeCount--; // 수류탄 개수 감소
            StartCoroutine(ThrowCooldown());
        }
    }

    void ThrowGrenade()
    {
        // 수류탄 생성
        GameObject grenade = Instantiate(grenadePrefab, throwPoint.position, throwPoint.rotation);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();

        // 힘 적용
        rb.AddForce(throwPoint.forward * throwForce, ForceMode.VelocityChange);
    }

    IEnumerator ThrowCooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(throwCooldown);
        isCooldown = false;
    }
}
