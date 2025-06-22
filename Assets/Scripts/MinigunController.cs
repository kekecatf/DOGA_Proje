using UnityEngine;
using UnityEngine.UI;

public class MinigunController : MonoBehaviour
{
    [Header("UI")]
    public Image chargeBarImage;

    [Header("Audio")]
    public AudioSource spinUpSound;
    public AudioSource fireSound;
    public AudioSource coolDownSound;

    [Header("Minigun")]
    public float chargeTime = 1f;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float fireRate = 0.1f;

    private float chargeProgress = 0f;
    private bool isFiring = false;
    private bool isCharging = false;
    private float fireCooldown = 0f;

    public void StartFiring()
    {
        isCharging = true;
        spinUpSound.Play();
        chargeBarImage.gameObject.SetActive(true);
    }

    public void StopFiring()
    {
        isCharging = false;
        isFiring = false;
        spinUpSound.Stop();
        fireSound.Stop();
        coolDownSound.Play();
    }

    void Update()
    {
        if (isCharging)
        {
            chargeProgress += Time.deltaTime / chargeTime;
            chargeBarImage.fillAmount = chargeProgress;

            if (chargeProgress >= 1f && !isFiring)
            {
                isFiring = true;
                fireSound.Play();
            }
        }
        else
        {
            if (chargeProgress > 0f)
            {
                chargeProgress -= Time.deltaTime;
                chargeBarImage.fillAmount = chargeProgress;
            }
            else
            {
                chargeBarImage.gameObject.SetActive(false);
            }
        }

        if (isFiring && Time.time >= fireCooldown)
        {
            FireBullet();
            fireCooldown = Time.time + fireRate;
        }
    }

    void FireBullet()
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}
