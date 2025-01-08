using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 무기 타입 정의
public enum GunType
{
    AssaultRifle,
    Shotgun,
    SniperRifle,
    Pistol
}

public class Gun : MonoBehaviour
{
    // 무기 타입 설정
    public GunType gunType; // 무기 타입 선택
    public string gunName;         // 총기 이름 (Rifle 또는 Shotgun)

    // 탄약 및 재장전 설정
    public int maxAmmo = 30;
    public int currentAmmo;
    public float reloadTime = 2f;
    private bool isReloading = false;

    // 발사 설정
    public float fireRate = 0.1f; // 연사 속도
    private float nextTimeToFire = 0f;
    private bool isAutoFire = true; // 발사 모드 (연사/단발)

    // 사격 관련 설정
    public float range = 100f;
    public int damage = 10;

    // 샷건 전용 변수
    public int pellets = 1;              // 샷건 탄환 개수
    public float spreadAngle = 0f;       // 샷건 확산 각도
    
    private bool isShotgunCooldown = false; // 샷건 쿨다운 상태
    public float shotgunCooldownTime = 1.0f; // 샷건 딜레이 시간

    // 총구 위치와 카메라 설정
    public Transform gunBarrel;      // 총구 위치
    public Camera fpsCamera;         // 카메라

    // 이펙트 및 사운드
    public ParticleSystem muzzleFlash;
    public AudioSource gunAudioSource;
    public AudioClip gunShotSound;
    public AudioClip reloadSound;

    // UI
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI fireModeText;
    public Image crosshairImage;
    public Color normalColor = Color.white;
    public Color targetColor = Color.red;

    // 반동 및 크로스헤어 설정
    private GunRecoil gunRecoil;
    private WeaponRecoil weaponRecoil;
    public float recoilSpread = 10f; // 반동 크기
    private float currentSpread = 0f;

    void Start()
    {
        // 무기 타입별 설정 적용
        ApplyGunSettings();

        // 초기 설정
        currentAmmo = maxAmmo;
        gunRecoil = FindObjectOfType<GunRecoil>();
        weaponRecoil = FindObjectOfType<WeaponRecoil>();
        UpdateUI();
    }
   
    void Update()
    {  
        // 재장전 중이면 조작 금지
        if (isReloading) return;

        UpdateUI(); // UI 업데이트
        // 발사 모드 전환 (B 키)
        if (Input.GetKeyDown(KeyCode.B))
        {
            if(gunType != GunType.AssaultRifle)
            isAutoFire = !isAutoFire;
        }

        // R 키 입력 또는 탄약 소진 시 재장전
        if (Input.GetKeyDown(KeyCode.R) || currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        // 단발 모드
        if (!isAutoFire && Input.GetButtonDown("Fire1") && currentAmmo > 0)
        {
            if (gunType == GunType.Shotgun && isShotgunCooldown) return; // 샷건 쿨다운 체크
            Shoot();

            if (gunType == GunType.Shotgun) // 샷건일 경우 쿨다운 시작
            {
                StartCoroutine(ShotgunCooldown());
            }
        }
        // 연사 모드
        else if (isAutoFire && Input.GetButton("Fire1") && currentAmmo > 0 && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }

        // 크로스헤어 업데이트
        UpdateCrosshair();
    }

    // 무기 타입별 설정 적용
    void ApplyGunSettings()
    {
        switch (gunType)
        {
            case GunType.AssaultRifle:
                maxAmmo = 30;
                reloadTime = 2f;
                fireRate = 0.1f;
                isAutoFire = true;
                damage = 10;
                recoilSpread = 10f;
                break;

            case GunType.Shotgun:
                maxAmmo = 8;
                reloadTime = 3f;
                fireRate = 1f;
                isAutoFire = false;
                damage = 15;
                recoilSpread = 20f;
                pellets = 8;
                spreadAngle = 10f;
                break;

            case GunType.SniperRifle:
                maxAmmo = 5;
                reloadTime = 2.5f;
                fireRate = 1.5f;
                isAutoFire = false;
                damage = 50;
                recoilSpread = 25f;
                break;

            case GunType.Pistol:
                maxAmmo = 15;
                reloadTime = 1.5f;
                fireRate = 0.3f;
                isAutoFire = false;
                damage = 8;
                recoilSpread = 5f;
                break;
        }
    }

    // UI 업데이트
    public void UpdateUI()
    {
        ammoText.text = currentAmmo + " / " + maxAmmo;
        fireModeText.text = isAutoFire ? "AUTO" : "SINGLE";
    }

    // 크로스헤어 업데이트
    void UpdateCrosshair()
    {
        // 크로스헤어 색상 변경 (적 조준 시 빨간색)
        Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {
            if (hit.collider != null)
            {
                if (hit.transform.CompareTag("Enemy"))
                    crosshairImage.color = targetColor;
                else
                    crosshairImage.color = normalColor;
            }
        }
        else
        {
            crosshairImage.color = normalColor;
        }
    }

    // 발사 기능
    void Shoot()
    {
        ApplyRecoil();
        if (gunAudioSource != null)
        {
            if (gunShotSound != null)
            {
                gunAudioSource.clip = gunShotSound;
                gunAudioSource.Play();
            }
        }
        if (muzzleFlash != null)
            Instantiate(muzzleFlash, gunBarrel);
        currentAmmo--;
        if (gunType == GunType.Shotgun)
        {
            for (int i = 0; i < pellets; i++)
            {
                FirePellet();
            }
        }
        else
        {
            FireSingleShot();
        }
    }

    void FireSingleShot()
    {
        Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            EnemyAI target = hit.transform.GetComponent<EnemyAI>();
            if (target != null)
            {
                target.EnemyTakeDamage(damage);
            }
        }
    }

    void FirePellet()
    {
        Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 shootDirection = ray.direction;
        shootDirection.x += Random.Range(-spreadAngle, spreadAngle) / 100f;
        shootDirection.y += Random.Range(-spreadAngle, spreadAngle) / 100f;

        if (Physics.Raycast(fpsCamera.transform.position, shootDirection, out RaycastHit hit, range))
        {
            EnemyAI target = hit.transform.GetComponent<EnemyAI>();
            if (target != null)
            {
                target.EnemyTakeDamage(damage);
            }
        }
    }
    // 반동 적용
    void ApplyRecoil()
    {
        gunRecoil.ApplyRecoil();
        weaponRecoil.ApplyRecoil();
        currentSpread += recoilSpread;
    }

    // 재장전 기능
    IEnumerator Reload()
    {
        if (currentAmmo == maxAmmo) yield break;

        isReloading = true;
        gunAudioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        UpdateUI();
    }
    public void CancelReload()
    {
        StopCoroutine(Reload()); // 진행 중인 코루틴 중단
        isReloading = false;     // 재장전 상태 초기화
    }

    IEnumerator ShotgunCooldown()
    {
        isShotgunCooldown = true; // 쿨다운 시작
        yield return new WaitForSeconds(shotgunCooldownTime); // 딜레이 적용
        isShotgunCooldown = false; // 쿨다운 해제
    }
    public void CancelShotgunCooldown()
    {
        StopCoroutine(ShotgunCooldown()); // 진행 중인 코루틴 중단
        isShotgunCooldown = false;        // 쿨다운 상태 해제
    }

    public void IncreaseDamage(int amount)
    {
        damage += amount;
        Debug.Log($"{gunName} 데미지 증가: {damage}");
    }

    public void IncreaseMaxAmmo(int amount)
    {
        maxAmmo += amount;
        Debug.Log($"{gunName} 최대 탄수 증가: {maxAmmo}");
    }
}
