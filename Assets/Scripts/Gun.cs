using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ���� Ÿ�� ����
public enum GunType
{
    AssaultRifle,
    Shotgun,
    SniperRifle,
    Pistol
}

public class Gun : MonoBehaviour
{
    // ���� Ÿ�� ����
    public GunType gunType; // ���� Ÿ�� ����
    public string gunName;         // �ѱ� �̸� (Rifle �Ǵ� Shotgun)

    // ź�� �� ������ ����
    public int maxAmmo = 30;
    public int currentAmmo;
    public float reloadTime = 2f;
    private bool isReloading = false;

    // �߻� ����
    public float fireRate = 0.1f; // ���� �ӵ�
    private float nextTimeToFire = 0f;
    private bool isAutoFire = true; // �߻� ��� (����/�ܹ�)

    // ��� ���� ����
    public float range = 100f;
    public int damage = 10;

    // ���� ���� ����
    public int pellets = 1;              // ���� źȯ ����
    public float spreadAngle = 0f;       // ���� Ȯ�� ����
    
    private bool isShotgunCooldown = false; // ���� ��ٿ� ����
    public float shotgunCooldownTime = 1.0f; // ���� ������ �ð�

    // �ѱ� ��ġ�� ī�޶� ����
    public Transform gunBarrel;      // �ѱ� ��ġ
    public Camera fpsCamera;         // ī�޶�

    // ����Ʈ �� ����
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

    // �ݵ� �� ũ�ν���� ����
    private GunRecoil gunRecoil;
    private WeaponRecoil weaponRecoil;
    public float recoilSpread = 10f; // �ݵ� ũ��
    private float currentSpread = 0f;

    void Start()
    {
        // ���� Ÿ�Ժ� ���� ����
        ApplyGunSettings();

        // �ʱ� ����
        currentAmmo = maxAmmo;
        gunRecoil = FindObjectOfType<GunRecoil>();
        weaponRecoil = FindObjectOfType<WeaponRecoil>();
        UpdateUI();
    }
   
    void Update()
    {  
        // ������ ���̸� ���� ����
        if (isReloading) return;

        UpdateUI(); // UI ������Ʈ
        // �߻� ��� ��ȯ (B Ű)
        if (Input.GetKeyDown(KeyCode.B))
        {
            if(gunType != GunType.AssaultRifle)
            isAutoFire = !isAutoFire;
        }

        // R Ű �Է� �Ǵ� ź�� ���� �� ������
        if (Input.GetKeyDown(KeyCode.R) || currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        // �ܹ� ���
        if (!isAutoFire && Input.GetButtonDown("Fire1") && currentAmmo > 0)
        {
            if (gunType == GunType.Shotgun && isShotgunCooldown) return; // ���� ��ٿ� üũ
            Shoot();

            if (gunType == GunType.Shotgun) // ������ ��� ��ٿ� ����
            {
                StartCoroutine(ShotgunCooldown());
            }
        }
        // ���� ���
        else if (isAutoFire && Input.GetButton("Fire1") && currentAmmo > 0 && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }

        // ũ�ν���� ������Ʈ
        UpdateCrosshair();
    }

    // ���� Ÿ�Ժ� ���� ����
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

    // UI ������Ʈ
    public void UpdateUI()
    {
        ammoText.text = currentAmmo + " / " + maxAmmo;
        fireModeText.text = isAutoFire ? "AUTO" : "SINGLE";
    }

    // ũ�ν���� ������Ʈ
    void UpdateCrosshair()
    {
        // ũ�ν���� ���� ���� (�� ���� �� ������)
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

    // �߻� ���
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
    // �ݵ� ����
    void ApplyRecoil()
    {
        gunRecoil.ApplyRecoil();
        weaponRecoil.ApplyRecoil();
        currentSpread += recoilSpread;
    }

    // ������ ���
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
        StopCoroutine(Reload()); // ���� ���� �ڷ�ƾ �ߴ�
        isReloading = false;     // ������ ���� �ʱ�ȭ
    }

    IEnumerator ShotgunCooldown()
    {
        isShotgunCooldown = true; // ��ٿ� ����
        yield return new WaitForSeconds(shotgunCooldownTime); // ������ ����
        isShotgunCooldown = false; // ��ٿ� ����
    }
    public void CancelShotgunCooldown()
    {
        StopCoroutine(ShotgunCooldown()); // ���� ���� �ڷ�ƾ �ߴ�
        isShotgunCooldown = false;        // ��ٿ� ���� ����
    }

    public void IncreaseDamage(int amount)
    {
        damage += amount;
        Debug.Log($"{gunName} ������ ����: {damage}");
    }

    public void IncreaseMaxAmmo(int amount)
    {
        maxAmmo += amount;
        Debug.Log($"{gunName} �ִ� ź�� ����: {maxAmmo}");
    }
}
