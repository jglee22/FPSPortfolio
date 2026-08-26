using System.Collections;
using UnityEngine;
using TMPro;

public class GrenadeThrower : MonoBehaviour
{
    public GameObject grenadePrefab;
    public Transform throwPoint;
    public float throwForce = 20f;

    public TextMeshProUGUI grenadeCountText;

    public int maxGrenadeCount = 3;
    public int currentGrenadeCount;
    public float throwCooldown = 1f;
    private bool isCooldown = false;
    private PlayerHealth playerHealth;

    private void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        currentGrenadeCount = maxGrenadeCount;
        grenadeCountText.text = " x " + maxGrenadeCount.ToString();
    }

    void Update()
    {
        if (playerHealth != null && playerHealth.isPlayerDie)
            return;

        if (MenuManager.IsInputBlocked || WaveRewardUI.IsOpen)
            return;

        if (Input.GetKeyDown(KeyCode.G) && currentGrenadeCount > 0 && !isCooldown)
        {
            ThrowGrenade();
            currentGrenadeCount--;
            grenadeCountText.text = " x " + currentGrenadeCount.ToString();
            StartCoroutine(ThrowCooldown());
        }
    }

    void ThrowGrenade()
    {
        GameObject grenade = Instantiate(grenadePrefab, throwPoint.position, throwPoint.rotation);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce(throwPoint.forward * throwForce, ForceMode.VelocityChange);
    }

    IEnumerator ThrowCooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(throwCooldown);
        isCooldown = false;
    }

    public void AddGrenades(int amount)
    {
        if (amount <= 0)
            return;

        currentGrenadeCount = Mathf.Min(maxGrenadeCount, currentGrenadeCount + amount);
        if (grenadeCountText != null)
            grenadeCountText.text = " x " + currentGrenadeCount.ToString();
    }
}
