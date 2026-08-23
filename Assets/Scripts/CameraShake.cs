using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float duration = 0.2f;
    public float magnitude = 0.3f;

    public void Shake(float durationOverride = -1f, float magnitudeOverride = -1f)
    {
        float shakeDuration = durationOverride > 0 ? durationOverride : duration;
        float shakeMagnitude = magnitudeOverride > 0 ? magnitudeOverride : magnitude;
        StartCoroutine(ShakeCoroutine(shakeDuration, shakeMagnitude));
    }

    IEnumerator ShakeCoroutine(float shakeDuration, float shakeMagnitude)
    {
        CameraRig rig = GetRig();
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            if (rig != null)
                rig.SetShakeOffset(Random.insideUnitSphere * shakeMagnitude);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (rig != null)
            rig.SetShakeOffset(Vector3.zero);
    }

    CameraRig GetRig()
    {
        Transform cam = transform;
        if (Camera.main != null)
            cam = Camera.main.transform;
        return CameraRig.GetOrCreate(cam);
    }
}
