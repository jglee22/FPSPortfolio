using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using TMPro;

public class GrenadeThrower : MonoBehaviour
{
    public GameObject grenadePrefab;
    public Transform throwPoint;
    public float throwForce = 20f;

    public TextMeshProUGUI grenadeCountText;

    [FormerlySerializedAs("maxGrenadeCount")]
    public int startingGrenadeCount = 3;
    public int currentGrenadeCount;
    public float throwCooldown = 1f;
    private bool isCooldown = false;
    private PlayerHealth playerHealth;

    private void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        currentGrenadeCount = startingGrenadeCount;
        grenadeCountText.text = " x " + currentGrenadeCount.ToString();
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

        currentGrenadeCount += amount;
        if (grenadeCountText != null)
            grenadeCountText.text = " x " + currentGrenadeCount.ToString();
    }
}
