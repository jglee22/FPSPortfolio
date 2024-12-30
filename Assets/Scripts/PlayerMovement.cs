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

    public float jumpHeight = 3f;  // 점프 높이
    private Vector3 velocity;      // 캐릭터의 속도
    public bool isGrounded;        // 바닥에 닿아 있는지 여부

    public Transform groundCheck; // 바닥 체크 위치
    public float groundDistance = 0.4f; // 체크 반경
    public LayerMask groundMask; // 바닥으로 인식할 레이어

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
            velocity.y = -2f; // 약간의 중력 유지
        }

        if (Input.GetButtonDown("Jump") && isGrounded) // 점프 키(스페이스바)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 마우스 회전
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : speed;

        // X축 회전 누적값 조절 및 제한
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);


        // 이동
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // 중력 적용
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
