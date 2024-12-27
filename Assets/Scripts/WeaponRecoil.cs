using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    public Transform weaponTransform;  // ���� ������Ʈ
    public float recoilAmount = 0.1f;  // �⺻ �ݵ� ũ��
    public float recoilSpeed = 8f;     // ȸ�� �ӵ�
    public float maxRecoil = 0.3f;     // �ִ� �ݵ� �Ÿ�

    private Vector3 originalPosition;  // ���� ��ġ
    private Vector3 recoilPosition;    // �ݵ� ��ġ

    void Start()
    {
        originalPosition = weaponTransform.localPosition;
    }

    void Update()
    {
        // �ݵ� ��ġ���� ���� ��ġ�� ȸ��
        weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, originalPosition, Time.deltaTime * recoilSpeed);
    }

    public void ApplyRecoil()
    {
        // ���� �ݵ� ���� ó��
        recoilPosition = weaponTransform.localPosition - Vector3.forward * recoilAmount;
        recoilPosition.z = Mathf.Clamp(recoilPosition.z, -maxRecoil, 0); // �ִ� �ݵ� ����
        weaponTransform.localPosition = recoilPosition;
    }
}
