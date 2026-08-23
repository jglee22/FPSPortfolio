using UnityEngine;

public class CameraRig : MonoBehaviour
{
    public float minPitch = -90f;
    public float maxPitch = 90f;

    private float pitch;
    private Vector3 recoilEuler;
    private float leanAngle;
    private Vector3 shakeOffset;
    private float heightOffset;
    private Vector3 restLocalPosition;
    private bool hasRestPosition;

    void Awake()
    {
        CacheRestPosition();
    }

    public void AddPitch(float delta)
    {
        pitch = Mathf.Clamp(pitch + delta, minPitch, maxPitch);
    }

    public void SetRecoil(Vector3 euler)
    {
        recoilEuler = euler;
    }

    public void SetLean(float angle)
    {
        leanAngle = angle;
    }

    public void SetShakeOffset(Vector3 offset)
    {
        shakeOffset = offset;
    }

    public void SetHeightOffset(float yOffset)
    {
        heightOffset = yOffset;
    }

    void LateUpdate()
    {
        CacheRestPosition();
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f)
            * Quaternion.Euler(recoilEuler)
            * Quaternion.Euler(0f, 0f, leanAngle);
        transform.localPosition = restLocalPosition + shakeOffset + Vector3.up * heightOffset;
    }

    public static CameraRig GetOrCreate(Transform cameraTransform)
    {
        if (cameraTransform == null)
            return null;

        CameraRig rig = cameraTransform.GetComponent<CameraRig>();
        if (rig == null)
            rig = cameraTransform.gameObject.AddComponent<CameraRig>();

        return rig;
    }

    void CacheRestPosition()
    {
        if (hasRestPosition)
            return;

        restLocalPosition = transform.localPosition;
        hasRestPosition = true;
    }
}
