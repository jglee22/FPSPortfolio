using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    public Transform weaponTransform;
    public float recoilAmount = 0.1f;
    public float recoilSpeed = 8f;
    public float recoilFollowSpeed = 16f;
    public float maxRecoil = 0.3f;
    public Vector3 recoilKickEuler = new Vector3(-6f, 0f, 2f);

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 recoilRestPosition;
    private Quaternion recoilRestRotation = Quaternion.identity;
    private Vector3 targetOffset;
    private Quaternion targetRotation = Quaternion.identity;
    private Vector3 appliedOffset;
    private Quaternion appliedRotation = Quaternion.identity;
    private Transform recoilTarget;
    private PlayerHealth playerHealth;
    private bool recoilPaused;
    private bool hasRestPose;
    private bool hasRecoilRest;
    private float bobTime;
    private Vector3 bobOffset;
    public float walkBobAmount = 0.007f;
    public float sprintBobAmount = 0.012f;
    public float walkBobSpeed = 9f;
    public float sprintBobSpeed = 13f;

    void Awake()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        if (weaponTransform == null)
            weaponTransform = transform;
        ResolveRecoilTarget();
    }

    void Start()
    {
        CacheRestPose();
    }

    void OnEnable()
    {
        CacheRestPose();
        ResolveRecoilTarget();
        ClearKick();
    }

    void OnDisable()
    {
        ApplyRestToView();
    }

    void LateUpdate()
    {
        if (recoilPaused)
            return;

        ResolveRecoilTarget();
        if (recoilTarget == null)
            return;

        targetOffset = Vector3.Lerp(targetOffset, Vector3.zero, Time.deltaTime * recoilSpeed);
        targetRotation = Quaternion.Slerp(targetRotation, Quaternion.identity, Time.deltaTime * recoilSpeed);

        float follow = Time.deltaTime * recoilFollowSpeed;
        appliedOffset = Vector3.Lerp(appliedOffset, targetOffset, follow);
        appliedRotation = Quaternion.Slerp(appliedRotation, targetRotation, follow);

        UpdateBob();

        recoilTarget.localPosition = recoilRestPosition + appliedOffset + bobOffset;
        recoilTarget.localRotation = recoilRestRotation * appliedRotation;
    }

    void UpdateBob()
    {
        PlayerMovement movement = playerHealth != null ? playerHealth.GetComponent<PlayerMovement>() : null;
        bool sprinting = movement != null && movement.IsSprinting;
        float moveSpeed = movement != null ? movement.PlanarSpeed : 0f;
        bool bobbing = !recoilPaused && moveSpeed > 0.4f && !MenuManager.IsInputBlocked && !WaveRewardUI.IsOpen;

        Vector3 targetBob = Vector3.zero;
        if (bobbing)
        {
            float amount = sprinting ? sprintBobAmount : walkBobAmount;
            float speed = sprinting ? sprintBobSpeed : walkBobSpeed;
            bobTime += Time.deltaTime * speed;
            targetBob = new Vector3(Mathf.Cos(bobTime * 0.5f) * amount * 0.35f, Mathf.Sin(bobTime) * amount, 0f);
        }
        else
        {
            bobTime = 0f;
        }

        bobOffset = Vector3.Lerp(bobOffset, targetBob, Time.deltaTime * 10f);
    }

    public void BindTransform(Transform target)
    {
        weaponTransform = target;
        hasRestPose = false;
        CacheRestPose();
    }

    public void ApplyRecoil()
    {
        if (recoilTarget == null)
            ResolveRecoilTarget();

        if (recoilTarget == null || recoilPaused)
            return;

        if (playerHealth != null && playerHealth.isPlayerDie)
            return;

        CacheRestPose();

        targetOffset = Vector3.back * recoilAmount;
        targetOffset.z = Mathf.Clamp(targetOffset.z, -maxRecoil, 0f);

        float yaw = Random.Range(-Mathf.Abs(recoilKickEuler.y), Mathf.Abs(recoilKickEuler.y));
        float roll = Random.Range(-Mathf.Abs(recoilKickEuler.z), Mathf.Abs(recoilKickEuler.z));
        targetRotation = Quaternion.Euler(recoilKickEuler.x, yaw, roll);
    }

    public void SetReloadLock(bool locked)
    {
        recoilPaused = locked;
        if (locked)
            ApplyRestToView();
        else
            ClearKick();
    }

    public Vector3 RestLocalPosition
    {
        get
        {
            CacheRestPose();
            return originalPosition;
        }
    }

    public Quaternion RestLocalRotation
    {
        get
        {
            CacheRestPose();
            return originalRotation;
        }
    }

    void ResolveRecoilTarget()
    {
        if (recoilTarget != null && hasRecoilRest)
            return;

        FPSViewModel viewModel = GetComponent<FPSViewModel>();
        if (viewModel != null && viewModel.characterInstance != null)
            recoilTarget = viewModel.characterInstance.transform;
        else
            recoilTarget = transform.parent != null ? transform.parent : transform;

        if (recoilTarget == null || hasRecoilRest)
            return;

        recoilRestPosition = recoilTarget.localPosition;
        recoilRestRotation = recoilTarget.localRotation;
        hasRecoilRest = true;
    }

    void CacheRestPose()
    {
        if (hasRestPose || weaponTransform == null)
            return;

        originalPosition = weaponTransform.localPosition;
        originalRotation = weaponTransform.localRotation;
        hasRestPose = true;
    }

    void ClearKick()
    {
        targetOffset = Vector3.zero;
        targetRotation = Quaternion.identity;
        appliedOffset = Vector3.zero;
        appliedRotation = Quaternion.identity;
        bobOffset = Vector3.zero;
        bobTime = 0f;
    }

    void ApplyRestToView()
    {
        ClearKick();
        if (recoilTarget == null || !hasRecoilRest)
            return;

        recoilTarget.localPosition = recoilRestPosition;
        recoilTarget.localRotation = recoilRestRotation;
    }
}
