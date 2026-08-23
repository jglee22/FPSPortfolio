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
    private Vector3 kickPosition;
    private Quaternion kickRotation;
    private PlayerHealth playerHealth;
    private bool recoilPaused;
    private bool hasRestPose;

    void Awake()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
    }

    void Start()
    {
        CacheRestPose();
    }

    void OnEnable()
    {
        CacheRestPose();
    }

    void Update()
    {
        if (recoilPaused || weaponTransform == null)
            return;

        kickPosition = Vector3.Lerp(kickPosition, originalPosition, Time.deltaTime * recoilSpeed);
        kickRotation = Quaternion.Slerp(kickRotation, originalRotation, Time.deltaTime * recoilSpeed);

        float follow = Time.deltaTime * recoilFollowSpeed;
        weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, kickPosition, follow);
        weaponTransform.localRotation = Quaternion.Slerp(weaponTransform.localRotation, kickRotation, follow);
    }

    public void ApplyRecoil()
    {
        if (weaponTransform == null)
            return;

        if (recoilPaused)
            return;

        if (playerHealth != null && playerHealth.isPlayerDie)
            return;

        CacheRestPose();

        kickPosition = originalPosition - Vector3.forward * recoilAmount;
        kickPosition.z = Mathf.Clamp(kickPosition.z, originalPosition.z - maxRecoil, originalPosition.z);

        float yaw = Random.Range(-Mathf.Abs(recoilKickEuler.y), Mathf.Abs(recoilKickEuler.y));
        float roll = Random.Range(-Mathf.Abs(recoilKickEuler.z), Mathf.Abs(recoilKickEuler.z));
        kickRotation = originalRotation * Quaternion.Euler(recoilKickEuler.x, yaw, roll);
    }

    public void SetFiringState(bool firing)
    {
    }

    public void SetReloadLock(bool locked)
    {
        recoilPaused = locked;
        if (!locked)
            RestoreRestPose();
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

    void CacheRestPose()
    {
        if (hasRestPose || weaponTransform == null)
            return;

        originalPosition = weaponTransform.localPosition;
        originalRotation = weaponTransform.localRotation;
        kickPosition = originalPosition;
        kickRotation = originalRotation;
        hasRestPose = true;
    }

    void RestoreRestPose()
    {
        if (weaponTransform == null)
            return;

        CacheRestPose();
        kickPosition = originalPosition;
        kickRotation = originalRotation;
        weaponTransform.localPosition = originalPosition;
        weaponTransform.localRotation = originalRotation;
    }
}
