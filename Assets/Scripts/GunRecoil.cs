using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    public Transform cameraTransform;  // FPS ī�޶�
    public float recoilAmount = 1.5f;  // �⺻ �ݵ� ũ��
    public float recoilSpeed = 5f;     // �ݵ� ȸ�� �ӵ�
    public float maxRecoilX = 10f;     // X�� �ִ� �ݵ� (����)
    public float maxRecoilY = 5f;      // Y�� �ִ� �ݵ� (�¿�)

    private Vector3 currentRotation;   // ���� ȸ�� ��
    private Vector3 targetRotation;    // ��ǥ ȸ�� ��

    void Update()
    {
        // �ݵ� ȸ�� ó��
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, recoilSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, recoilSpeed * Time.deltaTime);

        // ���� ȸ������ �ݵ� �߰�
        Quaternion originalRotation = cameraTransform.localRotation; // ���� ȸ���� ����
        Quaternion recoilRotation = Quaternion.Euler(currentRotation); // �ݵ� ȸ���� ����
        cameraTransform.localRotation = originalRotation * recoilRotation; // ���� ȸ���� �ݵ��� ���ϱ�
    }

    public void ApplyRecoil()
    {
        // �ݵ� ����
        targetRotation += new Vector3(
            Random.Range(-recoilAmount, recoilAmount),   // ���� �ݵ�
            Random.Range(-recoilAmount / 2, recoilAmount / 2), // �¿� �ݵ�
            0
        );

        // �ִ� �ݵ� ���� ����
        targetRotation.x = Mathf.Clamp(targetRotation.x, -maxRecoilX, maxRecoilX);
        targetRotation.y = Mathf.Clamp(targetRotation.y, -maxRecoilY, maxRecoilY);
    }
}
