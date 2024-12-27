using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public int maxAmmo = 30;       // 탄창당 최대 탄약 수
    public int currentAmmo;       // 현재 탄약 수
    public float reloadTime = 2f; // 재장전 시간
    public bool isReloading = false; // 재장전 여부

    public Camera fpsCamera;
    public float range = 100f;    // 사거리
    public int damage = 10;   // 데미지

    public ParticleSystem muzzleFlash; // 총구 효과
    public AudioSource gunShotSound;   // 총소리
    public AudioSource reloadSound;    // 재장전 소리

    private GunRecoil gunRecoil;
    private WeaponRecoil weaponRecoil;

    void Start()
    {
        currentAmmo = maxAmmo; // 시작 시 탄약을 가득 채움
        gunRecoil = FindObjectOfType<GunRecoil>();
        weaponRecoil = FindObjectOfType<WeaponRecoil>();
    }

    void Update()
    {
        // 재장전 중이면 동작 중지
        if (isReloading) return;

        // R 키 입력 또는 탄약 소진 시 재장전
        if (Input.GetKeyDown(KeyCode.R) || currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        // 마우스 왼쪽 버튼 클릭 시 발사
        if (Input.GetButtonDown("Fire1") && currentAmmo > 0)
        {
            Shoot();
        }
    }

    // 발사 기능
    void Shoot()
    {
        if(muzzleFlash != null)
        muzzleFlash.Play();              // 총구 이펙트 실행

        if(gunShotSound != null)
        gunShotSound.Play();             // 총소리 재생

        currentAmmo--;                   // 탄약 감소

        gunRecoil.ApplyRecoil();
        weaponRecoil.ApplyRecoil();

        RaycastHit hit;
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name); // 맞춘 오브젝트 로그 출력

            // 데미지 처리 예제
            EnemyAI target = hit.transform.GetComponent<EnemyAI>();
            if (target != null)
            {
                target.TakeDamage(damage); // 데미지 적용
            }
        }
    }

    // 재장전 기능
    IEnumerator Reload()
    {
        if (currentAmmo == maxAmmo) yield break; // 탄약이 가득 차 있으면 재장전 무시

        isReloading = true;                       // 재장전 중 상태 설정
        Debug.Log("재장전 중...");

        if(reloadSound != null)
        reloadSound.Play();                        // 재장전 소리 재생

        yield return new WaitForSeconds(reloadTime); // 재장전 시간 대기

        currentAmmo = maxAmmo;                     // 탄약을 가득 채움
        isReloading = false;                        // 재장전 완료 상태 설정
        Debug.Log("재장전 완료!");
    }
}