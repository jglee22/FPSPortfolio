using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Gun : MonoBehaviour
{
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
            Shoot();
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

    // UI 업데이트
    void UpdateUI()
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
                    crosshairImage.color = targetColor; // 적 조준 시 색상 변경
                else
                    crosshairImage.color = normalColor;
            }
        }
        else
        {
            crosshairImage.color = normalColor;
        }

        // 반동에 따른 크로스헤어 크기 조절
        currentSpread = Mathf.Lerp(currentSpread, 0f, Time.deltaTime * 10f);
        crosshairImage.rectTransform.sizeDelta = new Vector2(50 + currentSpread, 50 + currentSpread);
    }

    // 발사 기능
    void Shoot()
    {
        if (currentAmmo > 0 && !isReloading)
        {
            // 이펙트 및 소리
            if(muzzleFlash != null)
            muzzleFlash?.Play();
            gunAudioSource.PlayOneShot(gunShotSound);

            // 탄약 감소
            currentAmmo--;
            ApplyRecoil();

            // 발사 방향 계산 (카메라 기준 → 총구 위치 보정)
            Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Vector3 targetPoint = ray.GetPoint(range);
            Vector3 direction = (targetPoint - gunBarrel.position).normalized;

            RaycastHit hit;
            if (Physics.Raycast(gunBarrel.position, direction, out hit, range))
            {
                Debug.Log(hit.transform.name); // 맞춘 오브젝트 로그 출력

                // 적 타격 처리
                EnemyAI target = hit.transform.GetComponent<EnemyAI>();
                if (target != null)
                {
                    target.EnemyTakeDamage(damage);
                }
            }
        }
    }

    // 반동 적용
    void ApplyRecoil()
    {
        gunRecoil.ApplyRecoil();
        weaponRecoil.ApplyRecoil();
        currentSpread += recoilSpread; // 반동 시 크로스헤어 확장
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
}