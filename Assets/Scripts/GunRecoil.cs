using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    public Transform cameraTransform;
    public float recoilAmount = 1.5f;
    public float recoilSpeed = 5f;
    public float recoilRiseTime = 0.05f;
    public float maxRecoilX = 10f;
    public float maxRecoilY = 5f;
    [Range(0f, 1f)] public float recoilYawScale = 0.4f;
    [Range(0.1f, 1f)] public float firingRecoveryScale = 0.4f;

    private Vector3 currentRotation;
    private Vector3 targetRotation;
    private Vector3 recoilVelocity;
    private PlayerHealth playerHealth;
    private CameraRig cameraRig;
    private bool isFiring;

    void Awake()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
    }

    void Update()
    {
        if (playerHealth != null && playerHealth.isPlayerDie)
            return;

        float recovery = isFiring ? recoilSpeed * firingRecoveryScale : recoilSpeed;
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, recovery * Time.deltaTime);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref recoilVelocity, Mathf.Max(0.01f, recoilRiseTime));

        CameraRig rig = GetRig();
        if (rig != null)
            rig.SetRecoil(currentRotation);
    }

    public void ApplyRecoil()
    {
        if (playerHealth != null && playerHealth.isPlayerDie)
            return;

        float yawRange = recoilAmount * recoilYawScale;
        targetRotation += new Vector3(
            -Mathf.Abs(recoilAmount),
            Random.Range(-yawRange, yawRange),
            0f
        );

        targetRotation.x = Mathf.Clamp(targetRotation.x, -maxRecoilX, maxRecoilX);
        targetRotation.y = Mathf.Clamp(targetRotation.y, -maxRecoilY, maxRecoilY);
    }

    public void SetFiringState(bool firing)
    {
        isFiring = firing;
    }

    CameraRig GetRig()
    {
        if (cameraRig == null)
        {
            Transform cam = cameraTransform;
            if (cam == null && Camera.main != null)
                cam = Camera.main.transform;
            cameraRig = CameraRig.GetOrCreate(cam);
        }

        return cameraRig;
    }
}
