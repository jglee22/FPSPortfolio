using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public int maxAmmo = 30;       // źâ�� �ִ� ź�� ��
    public int currentAmmo;       // ���� ź�� ��
    public float reloadTime = 2f; // ������ �ð�
    public bool isReloading = false; // ������ ����

    public Camera fpsCamera;
    public float range = 100f;    // ��Ÿ�
    public int damage = 10;   // ������

    public ParticleSystem muzzleFlash; // �ѱ� ȿ��
    public AudioSource gunShotSound;   // �ѼҸ�
    public AudioSource reloadSound;    // ������ �Ҹ�

    private GunRecoil gunRecoil;
    private WeaponRecoil weaponRecoil;

    void Start()
    {
        currentAmmo = maxAmmo; // ���� �� ź���� ���� ä��
        gunRecoil = FindObjectOfType<GunRecoil>();
        weaponRecoil = FindObjectOfType<WeaponRecoil>();
    }

    void Update()
    {
        // ������ ���̸� ���� ����
        if (isReloading) return;

        // R Ű �Է� �Ǵ� ź�� ���� �� ������
        if (Input.GetKeyDown(KeyCode.R) || currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        // ���콺 ���� ��ư Ŭ�� �� �߻�
        if (Input.GetButtonDown("Fire1") && currentAmmo > 0)
        {
            Shoot();
        }
    }

    // �߻� ���
    void Shoot()
    {
        if(muzzleFlash != null)
        muzzleFlash.Play();              // �ѱ� ����Ʈ ����

        if(gunShotSound != null)
        gunShotSound.Play();             // �ѼҸ� ���

        currentAmmo--;                   // ź�� ����

        gunRecoil.ApplyRecoil();
        weaponRecoil.ApplyRecoil();

        RaycastHit hit;
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name); // ���� ������Ʈ �α� ���

            // ������ ó�� ����
            EnemyAI target = hit.transform.GetComponent<EnemyAI>();
            if (target != null)
            {
                target.TakeDamage(damage); // ������ ����
            }
        }
    }

    // ������ ���
    IEnumerator Reload()
    {
        if (currentAmmo == maxAmmo) yield break; // ź���� ���� �� ������ ������ ����

        isReloading = true;                       // ������ �� ���� ����
        Debug.Log("������ ��...");

        if(reloadSound != null)
        reloadSound.Play();                        // ������ �Ҹ� ���

        yield return new WaitForSeconds(reloadTime); // ������ �ð� ���

        currentAmmo = maxAmmo;                     // ź���� ���� ä��
        isReloading = false;                        // ������ �Ϸ� ���� ����
        Debug.Log("������ �Ϸ�!");
    }
}