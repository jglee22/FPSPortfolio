using UnityEngine;

public class Leaning : MonoBehaviour
{
    public Transform cameraTransform;
    public float leanAngle = 15f;
    public float leanSpeed = 5f;

    private float currentLean = 0f;
    private float targetLean = 0f;
    private CameraRig cameraRig;
    private PlayerHealth playerHealth;

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        cameraRig = CameraRig.GetOrCreate(cameraTransform);
    }

    void Update()
    {
        if (playerHealth != null && playerHealth.isPlayerDie)
            targetLean = 0f;
        else if (MenuManager.IsInputBlocked || WaveRewardUI.IsOpen)
            targetLean = 0f;
        else if (Input.GetKey(KeyCode.Q))
            targetLean = leanAngle;
        else if (Input.GetKey(KeyCode.E))
            targetLean = -leanAngle;
        else
            targetLean = 0f;

        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);

        if (cameraRig != null)
            cameraRig.SetLean(currentLean);
    }
}
