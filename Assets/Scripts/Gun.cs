using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Gun : MonoBehaviour
{
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
            Shoot();
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

    // UI ������Ʈ
    void UpdateUI()
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
                    crosshairImage.color = targetColor; // �� ���� �� ���� ����
                else
                    crosshairImage.color = normalColor;
            }
        }
        else
        {
            crosshairImage.color = normalColor;
        }

        // �ݵ��� ���� ũ�ν���� ũ�� ����
        currentSpread = Mathf.Lerp(currentSpread, 0f, Time.deltaTime * 10f);
        crosshairImage.rectTransform.sizeDelta = new Vector2(50 + currentSpread, 50 + currentSpread);
    }

    // �߻� ���
    void Shoot()
    {
        if (currentAmmo > 0 && !isReloading)
        {
            // ����Ʈ �� �Ҹ�
            if(muzzleFlash != null)
            muzzleFlash?.Play();
            gunAudioSource.PlayOneShot(gunShotSound);

            // ź�� ����
            currentAmmo--;
            ApplyRecoil();

            // �߻� ���� ��� (ī�޶� ���� �� �ѱ� ��ġ ����)
            Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Vector3 targetPoint = ray.GetPoint(range);
            Vector3 direction = (targetPoint - gunBarrel.position).normalized;

            RaycastHit hit;
            if (Physics.Raycast(gunBarrel.position, direction, out hit, range))
            {
                Debug.Log(hit.transform.name); // ���� ������Ʈ �α� ���

                // �� Ÿ�� ó��
                EnemyAI target = hit.transform.GetComponent<EnemyAI>();
                if (target != null)
                {
                    target.EnemyTakeDamage(damage);
                }
            }
        }
    }

    // �ݵ� ����
    void ApplyRecoil()
    {
        gunRecoil.ApplyRecoil();
        weaponRecoil.ApplyRecoil();
        currentSpread += recoilSpread; // �ݵ� �� ũ�ν���� Ȯ��
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
}