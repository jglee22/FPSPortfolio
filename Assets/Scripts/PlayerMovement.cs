using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float crouchSpeed = 6f;
    public float speed = 12f;
    public float sprintSpeed = 18f;
    public float gravity = -9.8f;
    public float mouseSensitivity = 100f;
    public float standHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchCameraHeight = 0.9f;
    public float crouchTransitionSpeed = 12f;

    public float jumpHeight = 3f;
    private Vector3 velocity;
    public bool isGrounded;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public Transform playerCamera;

    private bool isCrouching = false;
    private float currentHeight;
    private float standCameraY;
    private float currentCameraY;
    private PlayerHealth playerHealth;
    private CameraRig cameraRig;

    void Awake()
    {
        cameraRig = CameraRig.GetOrCreate(playerCamera);
    }

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        currentHeight = standHeight;
        standCameraY = playerCamera != null ? playerCamera.localPosition.y : crouchCameraHeight;
        currentCameraY = standCameraY;
        ApplyCapsuleHeight(currentHeight);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (playerHealth != null && playerHealth.isPlayerDie)
            return;

        if (MenuManager.IsInputBlocked || WaveRewardUI.IsOpen)
            return;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        if (cameraRig != null)
            cameraRig.AddPitch(-mouseY);

        transform.Rotate(Vector3.up * mouseX);

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (!isCrouching || CanStand())
                isCrouching = !isCrouching;
        }

        UpdateCrouch();

        float currentSpeed = GetCurrentSpeed();
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public bool IsSprinting
    {
        get
        {
            if (isCrouching || !isGrounded)
                return false;
            if (MenuManager.IsInputBlocked || WaveRewardUI.IsOpen)
                return false;
            if (!Input.GetKey(KeyCode.LeftShift))
                return false;

            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");
            return (x * x + z * z) > 0.01f;
        }
    }

    public float PlanarSpeed
    {
        get
        {
            if (MenuManager.IsInputBlocked || WaveRewardUI.IsOpen)
                return 0f;

            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");
            if ((x * x + z * z) < 0.01f)
                return 0f;

            return GetCurrentSpeed();
        }
    }

    float GetCurrentSpeed()
    {
        if (isCrouching)
            return crouchSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
            return sprintSpeed;

        return speed;
    }

    void UpdateCrouch()
    {
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        float targetCameraY = isCrouching ? crouchCameraHeight : standCameraY;
        float t = 1f - Mathf.Exp(-crouchTransitionSpeed * Time.deltaTime);

        currentHeight = Mathf.Lerp(currentHeight, targetHeight, t);
        currentCameraY = Mathf.Lerp(currentCameraY, targetCameraY, t);

        if (Mathf.Abs(currentHeight - targetHeight) < 0.001f)
            currentHeight = targetHeight;
        if (Mathf.Abs(currentCameraY - targetCameraY) < 0.001f)
            currentCameraY = targetCameraY;

        ApplyCapsuleHeight(currentHeight);

        if (cameraRig != null)
            cameraRig.SetHeightOffset(currentCameraY - standCameraY);
    }

    void ApplyCapsuleHeight(float height)
    {
        if (controller == null)
            return;

        controller.height = height;
        Vector3 center = controller.center;
        center.y = height * 0.5f;
        controller.center = center;
    }

    bool CanStand()
    {
        if (controller == null)
            return true;

        float radius = controller.radius * 0.9f;
        Vector3 origin = transform.position + Vector3.up * (radius + controller.skinWidth);
        float checkDistance = standHeight - crouchHeight;
        if (checkDistance <= 0f)
            return true;

        return !Physics.SphereCast(origin, radius, Vector3.up, out _, checkDistance, groundMask);
    }
}
