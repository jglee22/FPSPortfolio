using UnityEngine;

public class CombatHitFeedback : MonoBehaviour
{
    public static CombatHitFeedback Instance { get; private set; }

    public Material bloodParticleMaterial;
    public Color bloodColor = new Color(0.72f, 0.08f, 0.06f, 1f);
    public int bloodParticleCount = 28;
    public float bloodSpeed = 2.6f;
    public float bloodSize = 0.14f;
    public float bloodLifetime = 0.45f;
    public float bloodSpawnOffset = 0.1f;
    public AudioClip hitSound;
    public AudioClip deathSound;
    [Range(0f, 1f)] public float hitSoundVolume = 0.2f;
    [Range(0f, 1f)] public float deathSoundVolume = 0.28f;
    public Color hitFlashColor = new Color(0.55f, 0.08f, 0.08f, 1f);
    public float hitFlashDuration = 0.08f;
    public float hitStunDuration = 0.08f;

    AudioSource feedbackAudio;
    ParticleSystem bloodEmitter;

    void Awake()
    {
        Instance = this;
        feedbackAudio = GetComponent<AudioSource>();
        if (feedbackAudio == null)
            feedbackAudio = gameObject.AddComponent<AudioSource>();
        feedbackAudio.playOnAwake = false;
        feedbackAudio.spatialBlend = 0f;
        CreateBloodEmitter();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void PlayBodyFeedback(EnemyAI enemy, Vector3 hitPoint, Vector3 hitNormal, bool killed)
    {
        if (Instance == null || enemy == null)
            return;

        Instance.SpawnBlood(hitPoint, hitNormal);
        enemy.PlayHitFlash(Instance.hitFlashColor, Instance.hitFlashDuration);
        if (!killed)
            enemy.PlayHitStun(Instance.hitStunDuration);
    }

    public static void PlayHudFeedback(bool killed)
    {
        if (Instance == null)
            return;

        Instance.PlayClip(Instance.hitSound, Instance.hitSoundVolume);
    }

    public static void PlayDeathSound(Vector3 position)
    {
        if (Instance == null || Instance.deathSound == null)
            return;

        AudioSource.PlayClipAtPoint(Instance.deathSound, position, Instance.deathSoundVolume);
    }

    void SpawnBlood(Vector3 point, Vector3 normal)
    {
        if (bloodEmitter == null)
            CreateBloodEmitter();
        if (bloodEmitter == null)
            return;

        Vector3 spawnPoint = point;
        if (normal.sqrMagnitude > 0.001f)
            spawnPoint += normal.normalized * bloodSpawnOffset;

        Quaternion rotation = normal.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(normal)
            : Quaternion.identity;
        bloodEmitter.transform.SetPositionAndRotation(spawnPoint, rotation);

        if (!bloodEmitter.isPlaying)
            bloodEmitter.Play(true);

        bloodEmitter.Emit(bloodParticleCount);
    }

    void CreateBloodEmitter()
    {
        GameObject emitterObject = new GameObject("BloodHitEmitter");
        emitterObject.transform.SetParent(transform, false);

        bloodEmitter = emitterObject.AddComponent<ParticleSystem>();
        bloodEmitter.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = bloodEmitter.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.2f;
        main.startDelay = 0f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(bloodLifetime * 0.5f, bloodLifetime);
        main.startSpeed = new ParticleSystem.MinMaxCurve(bloodSpeed * 0.4f, bloodSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(bloodSize * 0.45f, bloodSize);
        main.startColor = bloodColor;
        main.gravityModifier = 1.6f;
        main.maxParticles = 256;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Local;

        ParticleSystem.EmissionModule emission = bloodEmitter.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.burstCount = 0;

        ParticleSystem.ShapeModule shape = bloodEmitter.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 30f;
        shape.radius = 0.03f;
        shape.radiusThickness = 1f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = bloodEmitter.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.85f, 0.35f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = gradient;

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = bloodEmitter.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.35f));

        ParticleSystemRenderer renderer = emitterObject.GetComponent<ParticleSystemRenderer>();
        renderer.enabled = true;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.sharedMaterial = GetBloodMaterial();
    }

    Material GetBloodMaterial()
    {
        if (bloodParticleMaterial != null)
            return bloodParticleMaterial;

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");
        if (shader == null)
            return null;

        Material material = new Material(shader);
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0f);
        material.SetFloat("_Cull", 0f);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = 3000;
        material.SetColor("_BaseColor", Color.white);
        return material;
    }

    void PlayClip(AudioClip clip, float volume)
    {
        if (clip == null || feedbackAudio == null)
            return;

        feedbackAudio.pitch = Random.Range(0.92f, 1.08f);
        feedbackAudio.PlayOneShot(clip, volume);
    }
}
