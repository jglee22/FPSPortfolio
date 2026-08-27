using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public enum GunType
{
    AssaultRifle,
    Shotgun,
    SniperRifle,
    Pistol
}

public class Gun : MonoBehaviour
{
    public GunType gunType;
    public string gunName;

    public int maxAmmo = 30;
    public bool lockMaxAmmo;
    public int currentAmmo;
    public float reloadTime = 2f;
    private bool isReloading = false;
    private Coroutine reloadRoutine;
    private Coroutine shotgunCooldownRoutine;

    public float fireRate = 0.1f;
    public float minFireRate = 0.05f;
    public float minReloadTime = 0.5f;
    public float minShotgunCooldownTime = 0.35f;
    private float nextTimeToFire = 0f;
    [SerializeField] private bool isAutoFire = true;

    public float range = 100f;
    public int damage = 10;

    public int pellets = 1;
    public float spreadAngle = 0f;

    private bool isShotgunCooldown = false;
    public float shotgunCooldownTime = 1.0f;

    public Transform gunBarrel;
    public Camera fpsCamera;

    public ParticleSystem muzzleFlash;
    public AudioSource gunAudioSource;
    public AudioClip gunShotSound;
    public AudioClip reloadSound;
    public AudioClip emptyClickSound;
    public Vector3 reloadMoveOffset = new Vector3(0f, -0.04f, 0.05f);
    public Vector3 reloadTiltEuler = new Vector3(8f, 0f, -6f);
    [Range(0.05f, 0.5f)] public float reloadDipRatio = 0.2f;
    [Range(0.05f, 0.5f)] public float reloadRecoverRatio = 0.22f;
    public Transform magazinePart;
    public string magazinePartName;
    public Vector3 magazineEjectOffset = new Vector3(0f, -0.08f, 0f);
    public Transform pumpPart;
    public string pumpPartName;
    public Vector3 pumpPullOffset = new Vector3(0f, 0f, -0.12f);
    public GameObject reloadShellPrefab;
    public Vector3 reloadShellStartLocalOffset = new Vector3(0.03f, -0.12f, -0.04f);
    public Vector3 reloadShellEndLocalOffset = new Vector3(0f, -0.03f, -0.08f);
    public Vector3 reloadShellLocalEuler = new Vector3(0f, 0f, 0f);
    public float reloadShellScale = 1f;

    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI fireModeText;
    public Image crosshairImage;
    public Color normalColor = Color.white;
    public Color targetColor = Color.red;

    public float muzzleFlashScale = 1f;
    public Vector3 muzzleFlashLocalOffset = Vector3.zero;
    public float firePitchMin = 0.97f;
    public float firePitchMax = 1.04f;
    [Range(0f, 1f)] public float fireVolume = 1f;

    private GunRecoil gunRecoil;
    private WeaponRecoil weaponRecoil;
    private FPSViewModel viewModel;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private Tween reloadTween;
    private Vector3 magazineRestPosition;
    private Quaternion magazineRestRotation;
    private Vector3 pumpRestPosition;
    private Quaternion pumpRestRotation;
    private bool hasMagazineRestPose;
    private bool hasPumpRestPose;
    private GameObject spawnedReloadShell;
    private ParticleSystem muzzleFlashInstance;
    public float recoilSpread = 10f;

    void Awake()
    {
        viewModel = GetComponent<FPSViewModel>();
    }

    void OnEnable()
    {
        if (viewModel != null)
            viewModel.Build();
    }

    void Start()
    {
        currentAmmo = maxAmmo;
        playerHealth = FindObjectOfType<PlayerHealth>();
        playerMovement = FindObjectOfType<PlayerMovement>();
        gunRecoil = GetComponent<GunRecoil>();
        weaponRecoil = GetComponent<WeaponRecoil>();
        ResolveReloadParts();
        EnsureMuzzleFlash();
        UpdateUI();
    }

    void Update()
    {
        if (playerHealth != null && playerHealth.isPlayerDie)
        {
            if (isReloading)
                CancelReload();
            SetRecoilFiring(false);
            return;
        }

        if (WaveRewardUI.IsOpen)
        {
            if (isReloading)
                CompleteReloadNow();
            SetRecoilFiring(false);
            return;
        }

        if (MenuManager.IsInputBlocked)
        {
            SetRecoilFiring(false);
            return;
        }

        UpdateCrosshair();

        if (isReloading)
            return;

        if (viewModel != null && !Input.GetMouseButton(0))
            viewModel.UpdateLocomotion(playerMovement != null && playerMovement.IsSprinting);

        UpdateUI();

        if (Input.GetKeyDown(KeyCode.B) && gunType == GunType.Shotgun)
            isAutoFire = !isAutoFire;

        if (!IsPointerOverUI() && Input.GetMouseButtonDown(0) && currentAmmo <= 0)
            PlayEmptyClick();

        if ((Input.GetKeyDown(KeyCode.R) || currentAmmo <= 0) && currentAmmo < maxAmmo && reloadRoutine == null)
        {
            reloadRoutine = StartCoroutine(Reload());
            return;
        }

        if (!IsPointerOverUI())
        {
            if (!isAutoFire && Input.GetMouseButtonDown(0) && currentAmmo > 0)
            {
                if (CanFireShotgun())
                {
                    Shoot();
                    StartShotgunCooldownIfNeeded();
                    SetRecoilFiring(true);
                }
            }
            else if (isAutoFire && Input.GetMouseButton(0) && currentAmmo > 0)
            {
                if (gunType == GunType.Shotgun)
                {
                    if (CanFireShotgun())
                    {
                        Shoot();
                        StartShotgunCooldownIfNeeded();
                        SetRecoilFiring(true);
                    }
                }
                else if (Time.time >= nextTimeToFire)
                {
                    nextTimeToFire = Time.time + fireRate;
                    Shoot();
                    SetRecoilFiring(true);
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
            SetRecoilFiring(false);
    }

    public void UpdateUI()
    {
        if (ammoText != null)
            ammoText.text = currentAmmo + " / " + maxAmmo;
        if (fireModeText != null)
            fireModeText.text = isAutoFire ? "AUTO" : "SINGLE";
    }

    void UpdateCrosshair()
    {
        if (crosshairImage == null || fpsCamera == null)
            return;

        Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, range) && hit.transform.CompareTag("Enemy"))
            crosshairImage.color = targetColor;
        else
            crosshairImage.color = normalColor;
    }

    void Shoot()
    {
        if (isReloading || currentAmmo <= 0)
            return;

        ApplyRecoil();

        if (viewModel != null)
            viewModel.PlayFire(!isAutoFire);

        PlayFireSound();
        PlayMuzzleFlash();
        PlayShotgunKick();

        currentAmmo--;

        if (gunType == GunType.Shotgun)
        {
            bool anyHit = false;
            bool killed = false;
            for (int i = 0; i < pellets; i++)
            {
                if (FirePellet(out bool pelletKilled))
                {
                    anyHit = true;
                    if (pelletKilled)
                        killed = true;
                }
            }

            if (anyHit)
                CombatHitFeedback.PlayHudFeedback(killed);
        }
        else if (FireSingleShot(out bool killed))
        {
            CombatHitFeedback.PlayHudFeedback(killed);
        }
    }

    bool FireSingleShot(out bool killed)
    {
        killed = false;
        if (fpsCamera == null)
            return false;

        Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, range))
            return false;

        return ApplyHit(hit, out killed);
    }

    bool FirePellet(out bool killed)
    {
        killed = false;
        if (fpsCamera == null)
            return false;

        Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 shootDirection = ray.direction;
        shootDirection.x += Random.Range(-spreadAngle, spreadAngle) / 100f;
        shootDirection.y += Random.Range(-spreadAngle, spreadAngle) / 100f;

        if (!Physics.Raycast(fpsCamera.transform.position, shootDirection, out RaycastHit hit, range))
            return false;

        return ApplyHit(hit, out killed);
    }

    bool ApplyHit(RaycastHit hit, out bool killed)
    {
        killed = false;
        EnemyAI target = hit.collider.GetComponentInParent<EnemyAI>();
        if (target == null)
            return false;

        killed = target.EnemyTakeDamage(damage, hit.point, hit.normal);
        return true;
    }

    void PlayFireSound()
    {
        if (gunAudioSource == null || gunShotSound == null)
            return;

        float previousPitch = gunAudioSource.pitch;
        gunAudioSource.pitch = Random.Range(firePitchMin, firePitchMax);
        gunAudioSource.PlayOneShot(gunShotSound, fireVolume);
        gunAudioSource.pitch = previousPitch;
    }

    void PlayEmptyClick()
    {
        if (gunAudioSource == null || emptyClickSound == null)
            return;

        gunAudioSource.PlayOneShot(emptyClickSound, 0.45f);
    }

    void PlayShotgunKick()
    {
        if (gunType != GunType.Shotgun || fpsCamera == null)
            return;

        CameraShake shake = fpsCamera.GetComponent<CameraShake>();
        if (shake != null)
            shake.Shake(0.07f, 0.11f);
    }

    void EnsureMuzzleFlash()
    {
        if (muzzleFlashInstance != null || muzzleFlash == null)
            return;

        if (muzzleFlash.gameObject.scene.IsValid())
        {
            muzzleFlashInstance = muzzleFlash;
        }
        else if (gunBarrel != null)
        {
            muzzleFlashInstance = Instantiate(muzzleFlash, gunBarrel);
            muzzleFlashInstance.transform.localPosition = muzzleFlashLocalOffset;
            muzzleFlashInstance.transform.localRotation = Quaternion.identity;
            muzzleFlashInstance.transform.localScale = Vector3.one * muzzleFlashScale;
        }

        if (muzzleFlashInstance == null)
            return;

        ParticleSystem.MainModule main = muzzleFlashInstance.main;
        main.playOnAwake = false;
        muzzleFlashInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void PlayMuzzleFlash()
    {
        EnsureMuzzleFlash();
        if (muzzleFlashInstance == null)
            return;

        muzzleFlashInstance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        muzzleFlashInstance.Play(true);
    }

    void ApplyRecoil()
    {
        if (gunRecoil != null)
            gunRecoil.ApplyRecoil();
        if (weaponRecoil != null)
            weaponRecoil.ApplyRecoil();
    }

    IEnumerator Reload()
    {
        isReloading = true;
        SetRecoilFiring(false);

        float wait = reloadTime;
        if (viewModel != null)
            wait = viewModel.PlayReload(ShouldUseTacticalReload(), reloadTime);
        else
            PlayReloadMotion();

        if (gunAudioSource != null && reloadSound != null)
        {
            gunAudioSource.clip = reloadSound;
            gunAudioSource.Play();
        }

        float elapsed = 0f;
        while (isReloading && elapsed < wait)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (isReloading)
            ReloadComplete();
        else
            reloadRoutine = null;
    }

    public void ReloadComplete()
    {
        if (!isReloading)
            return;

        currentAmmo = maxAmmo;
        isReloading = false;
        reloadRoutine = null;
        if (viewModel != null)
            viewModel.PlayIdle();
        UpdateUI();
    }

    void CompleteReloadNow()
    {
        if (!isReloading)
            return;

        StopReloadSound();
        if (reloadTween != null && reloadTween.IsActive())
            reloadTween.Kill();
        ReloadComplete();
    }

    bool ShouldUseTacticalReload()
    {
        return gunType == GunType.Shotgun && currentAmmo > 0 && currentAmmo < maxAmmo;
    }

    bool CanFireShotgun()
    {
        if (gunType != GunType.Shotgun)
            return true;
        return !isShotgunCooldown;
    }

    void StartShotgunCooldownIfNeeded()
    {
        if (gunType != GunType.Shotgun || shotgunCooldownRoutine != null)
            return;

        shotgunCooldownRoutine = StartCoroutine(ShotgunCooldown());
    }

    void PlayReloadMotion()
    {
        Transform target = GetReloadTarget();
        if (target == null || reloadTime <= 0f)
            return;

        if (reloadTween != null && reloadTween.IsActive())
            reloadTween.Kill();

        ResolveReloadParts();

        if (weaponRecoil != null)
            weaponRecoil.SetReloadLock(true);

        Vector3 restPos = weaponRecoil != null ? weaponRecoil.RestLocalPosition : target.localPosition;
        Quaternion restRot = weaponRecoil != null ? weaponRecoil.RestLocalRotation : target.localRotation;
        Quaternion reloadRot = restRot * Quaternion.Euler(reloadTiltEuler);
        float duration = reloadTime;
        bool hasMagazine = magazinePart != null && gunType != GunType.Shotgun;
        bool hasPump = pumpPart != null;
        bool hasShellInsert = reloadShellPrefab != null;

        float dipDuration = duration * Mathf.Clamp(reloadDipRatio, 0.05f, 0.5f);
        float recoverDuration = duration * Mathf.Clamp(reloadRecoverRatio, 0.05f, 0.5f);
        float recoverStart = Mathf.Max(dipDuration, duration - recoverDuration);

        Sequence sequence = DOTween.Sequence();
        sequence.SetTarget(target);
        sequence.Insert(0f, target.DOLocalMove(restPos + reloadMoveOffset, dipDuration).SetEase(Ease.OutQuad));
        sequence.Insert(0f, target.DOLocalRotateQuaternion(reloadRot, dipDuration).SetEase(Ease.OutQuad));
        sequence.Insert(recoverStart, target.DOLocalMove(restPos, recoverDuration).SetEase(Ease.InOutQuad));
        sequence.Insert(recoverStart, target.DOLocalRotateQuaternion(restRot, recoverDuration).SetEase(Ease.InOutQuad));

        if (hasMagazine)
        {
            float magOutDuration = duration * 0.12f;
            float magInStart = duration * 0.2f;
            float magInDuration = duration * 0.14f;
            sequence.Insert(0f, magazinePart.DOLocalMove(magazineRestPosition + magazineEjectOffset, magOutDuration).SetEase(Ease.OutQuad));
            sequence.Insert(magInStart, magazinePart.DOLocalMove(magazineRestPosition, magInDuration).SetEase(Ease.InQuad));
        }

        if (hasShellInsert)
        {
            ClearSpawnedReloadShell();
            spawnedReloadShell = Instantiate(reloadShellPrefab, target);
            spawnedReloadShell.transform.localPosition = reloadShellStartLocalOffset;
            spawnedReloadShell.transform.localRotation = Quaternion.Euler(reloadShellLocalEuler);
            spawnedReloadShell.transform.localScale = Vector3.one * reloadShellScale;

            float shellMoveDuration = duration * 0.22f;
            sequence.Insert(duration * 0.08f, spawnedReloadShell.transform.DOLocalMove(reloadShellEndLocalOffset, shellMoveDuration).SetEase(Ease.InQuad));
            sequence.InsertCallback(duration * 0.32f, ClearSpawnedReloadShell);
        }

        if (hasPump)
        {
            float pumpStart = hasShellInsert ? duration * 0.38f : duration * 0.28f;
            sequence.Insert(pumpStart, pumpPart.DOLocalMove(pumpRestPosition + pumpPullOffset, duration * 0.14f).SetEase(Ease.OutQuad));
            sequence.Insert(pumpStart + duration * 0.16f, pumpPart.DOLocalMove(pumpRestPosition, duration * 0.14f).SetEase(Ease.InQuad));
        }

        sequence.OnKill(() =>
        {
            reloadTween = null;
            RestoreReloadVisuals(target, restPos, restRot);
        });
        reloadTween = sequence;
    }

    void ResolveReloadParts()
    {
        Transform searchRoot = transform;
        if (magazinePart == null)
            magazinePart = FindDescendant(searchRoot, magazinePartName);
        if (pumpPart == null)
            pumpPart = FindDescendant(searchRoot, pumpPartName);

        if (magazinePart != null && !hasMagazineRestPose)
        {
            magazineRestPosition = magazinePart.localPosition;
            magazineRestRotation = magazinePart.localRotation;
            hasMagazineRestPose = true;
        }

        if (pumpPart != null && !hasPumpRestPose)
        {
            pumpRestPosition = pumpPart.localPosition;
            pumpRestRotation = pumpPart.localRotation;
            hasPumpRestPose = true;
        }
    }

    void RestoreReloadVisuals(Transform body, Vector3 restPos, Quaternion restRot)
    {
        if (weaponRecoil != null)
            weaponRecoil.SetReloadLock(false);
        else if (body != null)
        {
            body.localPosition = restPos;
            body.localRotation = restRot;
        }

        if (magazinePart != null && hasMagazineRestPose)
        {
            magazinePart.localPosition = magazineRestPosition;
            magazinePart.localRotation = magazineRestRotation;
        }

        if (pumpPart != null && hasPumpRestPose)
        {
            pumpPart.localPosition = pumpRestPosition;
            pumpPart.localRotation = pumpRestRotation;
        }

        ClearSpawnedReloadShell();
    }

    void ClearSpawnedReloadShell()
    {
        if (spawnedReloadShell == null)
            return;

        Destroy(spawnedReloadShell);
        spawnedReloadShell = null;
    }

    static Transform FindDescendant(Transform root, string partName)
    {
        if (root == null || string.IsNullOrEmpty(partName))
            return null;

        if (root.name == partName)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDescendant(root.GetChild(i), partName);
            if (found != null)
                return found;
        }

        return null;
    }

    Transform GetReloadTarget()
    {
        if (weaponRecoil != null && weaponRecoil.weaponTransform != null)
            return weaponRecoil.weaponTransform;
        return transform;
    }

    public void CancelReload()
    {
        bool wasReloading = isReloading || reloadRoutine != null;
        if (reloadRoutine != null)
        {
            StopCoroutine(reloadRoutine);
            reloadRoutine = null;
        }
        isReloading = false;

        if (wasReloading)
            StopReloadSound();

        if (viewModel != null)
            viewModel.PlayIdle();

        if (reloadTween != null && reloadTween.IsActive())
            reloadTween.Kill();
        else
            RestoreReloadVisuals(
                GetReloadTarget(),
                weaponRecoil != null ? weaponRecoil.RestLocalPosition : transform.localPosition,
                weaponRecoil != null ? weaponRecoil.RestLocalRotation : transform.localRotation
            );
    }

    void StopReloadSound()
    {
        if (gunAudioSource != null)
            gunAudioSource.Stop();
    }

    IEnumerator ShotgunCooldown()
    {
        isShotgunCooldown = true;
        yield return new WaitForSeconds(shotgunCooldownTime);
        isShotgunCooldown = false;
        shotgunCooldownRoutine = null;
    }

    public void CancelShotgunCooldown()
    {
        if (shotgunCooldownRoutine != null)
        {
            StopCoroutine(shotgunCooldownRoutine);
            shotgunCooldownRoutine = null;
        }
        isShotgunCooldown = false;
    }

    public void IncreaseDamage(int amount)
    {
        damage += amount;
    }

    public void IncreaseMaxAmmo(int amount)
    {
        if (lockMaxAmmo)
            return;

        maxAmmo += amount;
        UpdateUI();
    }

    public void RestoreAmmo()
    {
        if (isReloading)
            CancelReload();

        currentAmmo = maxAmmo;
        UpdateUI();
    }

    public void MultiplyReloadTime(float multiplier)
    {
        if (multiplier <= 0f)
            return;

        reloadTime = Mathf.Max(minReloadTime, reloadTime * multiplier);
    }

    public void MultiplyFireRate(float multiplier)
    {
        if (multiplier <= 0f)
            return;

        fireRate = Mathf.Max(minFireRate, fireRate * multiplier);
        shotgunCooldownTime = Mathf.Max(minShotgunCooldownTime, shotgunCooldownTime * multiplier);
    }

    void SetRecoilFiring(bool firing)
    {
        if (gunRecoil != null)
            gunRecoil.SetFiringState(firing);
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            return true;

        return false;
    }
}
