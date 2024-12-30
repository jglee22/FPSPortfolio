using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float crouchSpeed = 6f;
    public float speed = 12f;
    public float sprintSpeed = 18f;
    public float gravity = -9.8f;
    public float mouseSensitivity = 100f;

    public float jumpHeight = 3f;  // ���� ����
    private Vector3 velocity;      // ĳ������ �ӵ�
    public bool isGrounded;        // �ٴڿ� ��� �ִ��� ����

    public Transform groundCheck; // �ٴ� üũ ��ġ
    public float groundDistance = 0.4f; // üũ �ݰ�
    public LayerMask groundMask; // �ٴ����� �ν��� ���̾�

    public Transform playerCamera;

    float xRotation = 0f;
    private bool isCrouching = false;

    private PlayerHealth playerHealth;

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    void Update()
    {
        if (playerHealth != null)
        {
            if (playerHealth.isPlayerDie)
                return;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // �ణ�� �߷� ����
        }

        if (Input.GetButtonDown("Jump") && isGrounded) // ���� Ű(�����̽���)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // ���콺 ȸ��
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : speed;

        // X�� ȸ�� ������ ���� �� ����
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);


        // �̵�
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // �߷� ����
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
            controller.height = isCrouching ? 1.0f : 2.0f;
            speed = isCrouching ? crouchSpeed : 5f;
        }
    }
}
