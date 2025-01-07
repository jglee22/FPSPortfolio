using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaning : MonoBehaviour
{
    public Transform cameraTransform;  // FPS ī�޶� ����
    public float leanAngle = 15f;      // ����� ����
    public float leanSpeed = 5f;      // �ε巯�� ��ȯ �ӵ�

    private float currentLean = 0f;    // ���� ���� ����
    private float targetLean = 0f;     // ��ǥ ���� ����

    void Update()
    {
        // �Է� ó��
        if (Input.GetKey(KeyCode.Q))      // ���� ����̱�
        {
            targetLean = leanAngle;
        }
        else if (Input.GetKey(KeyCode.E)) // ������ ����̱�
        {
            targetLean = -leanAngle;
        }
        else
        {
            targetLean = 0f; // �ʱ�ȭ
        }

        if(currentLean != targetLean)
        {
            // �ε巯�� ȸ�� ��ȯ
            currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);

            // ���� ȸ������ ���� �߰� (Y�� ȸ�� ����)
            Quaternion baseRotation = Quaternion.Euler(cameraTransform.localRotation.eulerAngles.x, cameraTransform.localRotation.eulerAngles.y, 0);
            Quaternion leanRotation = Quaternion.Euler(0, 0, currentLean);
            cameraTransform.localRotation = baseRotation * leanRotation; // ���� ȸ�� + ���� ����
        }
    }
}
