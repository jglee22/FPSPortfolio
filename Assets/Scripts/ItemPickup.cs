using TMPro;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ConsumableItem consumableItem;
    public AudioClip pickupSound;
    public float hoverHeight = 0.12f;
    public float hoverSpeed = 2.4f;
    public float spinSpeed = 50f;
    public Color markerColor = new Color(1f, 0.82f, 0.22f, 0.95f);
    public float beamHeight = 1.6f;
    public float beamWidth = 0.07f;
    public float lightRange = 3.5f;
    public float lightIntensity = 1.6f;
    public TMP_FontAsset labelFont;
    public float labelHeight = 0.7f;
    public float labelFontSize = 1.4f;

    Vector3 basePosition;
    Transform labelTransform;

    void Start()
    {
        ApplyVisualColor();
        CreateDropMarker();
        CreateDropLabel();
        IgnoreGunRaycasts();
        basePosition = transform.position;
    }

    void IgnoreGunRaycasts()
    {
        int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        if (ignoreRaycastLayer < 0)
            return;

        Transform[] transforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
            transforms[i].gameObject.layer = ignoreRaycastLayer;
    }

    void Update()
    {
        transform.position = basePosition + Vector3.up * (Mathf.Sin(Time.time * hoverSpeed) * hoverHeight);
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        FaceLabelToCamera();
    }

    void FaceLabelToCamera()
    {
        if (labelTransform == null || Camera.main == null)
            return;

        Vector3 cameraPosition = Camera.main.transform.position;
        labelTransform.LookAt(labelTransform.position + (labelTransform.position - cameraPosition));
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null && playerHealth.isPlayerDie)
            return;

        if (consumableItem == null)
            return;

        consumableItem.Apply(other.gameObject);

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        string message = GetPickupMessage();
        if (PickupMessageManager.Instance != null && !string.IsNullOrEmpty(message))
            PickupMessageManager.Instance.EnqueuePickupMessage(message);

        Destroy(gameObject);
    }

    void CreateDropMarker()
    {
        GameObject marker = new GameObject("DropMarker");
        marker.transform.SetParent(transform, false);

        LineRenderer beam = marker.AddComponent<LineRenderer>();
        beam.useWorldSpace = false;
        beam.positionCount = 2;
        beam.SetPosition(0, Vector3.zero);
        beam.SetPosition(1, Vector3.up * beamHeight);
        beam.startWidth = beamWidth;
        beam.endWidth = 0.01f;
        beam.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        beam.receiveShadows = false;
        beam.material = CreateMarkerMaterial();
        beam.startColor = markerColor;
        beam.endColor = new Color(markerColor.r, markerColor.g, markerColor.b, 0f);

        Light light = marker.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = markerColor;
        light.range = lightRange;
        light.intensity = lightIntensity;
        light.shadows = LightShadows.None;
    }

    void CreateDropLabel()
    {
        string labelText = GetPickupMessage();
        if (string.IsNullOrEmpty(labelText) || labelFont == null)
            return;

        GameObject labelObject = new GameObject("DropLabel");
        labelTransform = labelObject.transform;
        labelTransform.SetParent(transform, false);

        float parentScale = Mathf.Max(0.001f, transform.lossyScale.y);
        labelTransform.localPosition = Vector3.up * (labelHeight / parentScale);
        labelTransform.localScale = Vector3.one * (1f / parentScale);

        TextMeshPro text = labelObject.AddComponent<TextMeshPro>();
        text.font = labelFont;
        text.text = labelText;
        text.fontSize = labelFontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.outlineWidth = 0.3f;
        text.outlineColor = Color.black;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        text.rectTransform.sizeDelta = new Vector2(labelFontSize * 12f, labelFontSize * 2f);
    }

    Material CreateMarkerMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        Material material = new Material(shader);
        material.color = markerColor;
        return material;
    }

    void ApplyVisualColor()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            return;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");
        if (shader == null)
            return;

        Material material = new Material(shader);
        material.color = markerColor;
        meshRenderer.material = material;
    }

    string GetPickupMessage()
    {
        if (consumableItem != null && !string.IsNullOrEmpty(consumableItem.displayName))
            return consumableItem.displayName;

        return consumableItem != null ? consumableItem.type.ToString() : string.Empty;
    }
}
