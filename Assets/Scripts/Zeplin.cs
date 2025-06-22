using UnityEngine;
using UnityEngine.UI;

public class Zeplin : MonoBehaviour
{
    [Header("Hedefleme Ayarları")]
    public float targetingRange = 10f;
    private Transform currentTarget;

    [Header("UI Elemanları")]
    public Slider healthSlider;
    public Text healthText;
    public RectTransform fillRectTransform;

    [Header("Efektler")]
    public GameObject damageEffect;
    public GameObject destroyEffect;
    public GameObject activationEffect;

    [Header("Silah Ayarları")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    private float nextAutoFireTime = 0f;
    private float minAutoFireDelay = 2.5f;
    private float maxAutoFireDelay = 5f;

    [Header("Hasar Kontrolü")]
    public bool useInvincibility = false;
    public float invincibilityTime = 0.2f;
    private float invincibilityTimer = 0f;
    private bool isInvincible = false;

    [Header("Kontrol Geçiş Efektleri")]
    public float activationHighlightDuration = 1.5f;
    private float activationHighlightTimer = 0f;
    private bool isActivationHighlightActive = false;

    [Header("Zeplin Sağlık Ayarı")]
    private SpriteRenderer spriteRenderer;
    private PlayerData playerData;
    private int maxHealth = 100;
    private bool isFacingLeft = false;
    private bool isControlActive = false;
    private Vector3 originalFirePointLocalPos;

    
    private bool miniGameTriggered = false;
    void Start()
    {
        Debug.Log("--- Zeplin.Start() başlıyor ---");
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerData = PlayerData.Instance;

        if (playerData == null)
        {
            Debug.LogError("FATAL HATA: PlayerData.Instance bulunamadı! Zeplin script'i devre dışı bırakılıyor.");
            this.enabled = false;
            return;
        }

        maxHealth = playerData.zeplinMaxSaglik;
        if (maxHealth <= 0)
        {
            Debug.LogWarning($"maxHealth geçersiz ({maxHealth}). Varsayılan olarak 100 ayarlanıyor.");
            maxHealth = 100;
        }
        Debug.Log($"Max Sağlık '{maxHealth}' olarak ayarlandı.");

        if (PlayerData.Instance.isPlayerRespawned)
        {
            Debug.Log("<color=green>Yeniden Doğma durumu algılandı (isPlayerRespawned = true).</color>");

            int respawnHealth = Mathf.RoundToInt(maxHealth * 0.25f);
            playerData.zeplinSaglik = respawnHealth;

            Debug.Log($"Can %25 olarak hesaplandı: {respawnHealth}. playerData.zeplinSaglik şimdi '{playerData.zeplinSaglik}'.");

            PlayerData.Instance.isPlayerRespawned = false;
            Debug.Log("isPlayerRespawned bayrağı 'false' yapıldı.");

            ActivateZeplinControl();
        }
        else
        {
            Debug.Log("<color=yellow>Normal oyun başlangıcı algılandı.</color>");

            playerData.zeplinSaglik = maxHealth;
            Debug.Log($"Can max değere ayarlandı: {playerData.zeplinSaglik}");
        }

        Debug.Log($"--- Zeplin.Start() tamamlandı. Son Can Değeri: {playerData.zeplinSaglik} ---");
        UpdateUI();

        // Diğer başlangıç ayarları...
        if (firePoint != null)
        {
            originalFirePointLocalPos = firePoint.localPosition;
        }
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }
    }

    void Update()
    {
        if (Player.isDead && !isControlActive)
        {
            ActivateZeplinControl();
        }

        if (!isControlActive) return;

        HandleInvincibility();
        HandleActivationHighlight();
        HandleAutoFire();
    }

    public void ActivateZeplinControl()
    {
        if (isControlActive) return;

        isControlActive = true;

        if (activationEffect != null)
        {
            Instantiate(activationEffect, transform.position, Quaternion.identity);
        }

        isActivationHighlightActive = true;
        activationHighlightTimer = activationHighlightDuration;

        // Otomatik ateş başlat
        ScheduleNextAutoFire();
    }

    private void HandleAutoFire()
    {
        if (Time.time >= nextAutoFireTime)
        {
            FireMinigun();
            ScheduleNextAutoFire();
        }
    }

    private void ScheduleNextAutoFire()
    {
        float randomDelay = Random.Range(minAutoFireDelay, maxAutoFireDelay);
        nextAutoFireTime = Time.time + randomDelay;
    }

    private void HandleInvincibility()
    {
        if (!useInvincibility || !isInvincible) return;

        invincibilityTimer -= Time.deltaTime;
        spriteRenderer.enabled = Time.time % 0.2f < 0.1f;

        if (invincibilityTimer <= 0)
        {
            isInvincible = false;
            spriteRenderer.enabled = true;
        }
    }

    private void HandleActivationHighlight()
    {
        if (!isActivationHighlightActive) return;

        activationHighlightTimer -= Time.deltaTime;
        float pulse = Mathf.PingPong(Time.time * 4, 1.0f);
        spriteRenderer.color = Color.Lerp(Color.white, Color.yellow, pulse * 0.5f);

        if (activationHighlightTimer <= 0)
        {
            isActivationHighlightActive = false;
            spriteRenderer.color = Color.white;
        }
    }

    public void FireMinigun()
    {
        if (!isControlActive || bulletPrefab == null) return;

        Transform spawnPoint = firePoint != null ? firePoint : transform;
        GameObject bullet = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);

        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.isZeplinBullet = true;
            bulletComponent.zeplinSource = this.transform;
            bulletComponent.SetDirection(isFacingLeft);
            bulletComponent.damage = playerData.zeplinMinigunDamage;
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayMinigunSound();
    }

    public void TakeDamage(int damage)
    {
        // 1. KORUMA: Eğer dokunulmazsak veya canımız zaten sıfırlanmış ve mini oyun tetiklenmişse, çık.
        if ((useInvincibility && isInvincible) || (playerData.zeplinSaglik <= 0 && miniGameTriggered))
        {
            return;
        }

        // 2. HASAR UYGULAMA
        Debug.Log($"Zeplin canı (hasardan önce): {playerData.zeplinSaglik}, Alınan Hasar: {damage}");
        playerData.zeplinSaglik -= damage;

        // Hasar efektleri ve geçici dokunulmazlık
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }
        if (useInvincibility)
        {
            isInvincible = true;
            invincibilityTimer = invincibilityTime;
        }

        // 3. CAN SIFIRIN ALTINA DÜŞTÜYSE, 0'a SABİTLE
        if (playerData.zeplinSaglik < 0)
        {
            playerData.zeplinSaglik = 0;
        }

        // UI'ı her zaman güncelle
        UpdateUI();

        // 4. ÖLÜM KONTROLÜ
        // Can 0 veya daha az ise VE mini oyun daha önce tetiklenmediyse...
        if (playerData.zeplinSaglik <= 0 && !miniGameTriggered)
        {
            // Ölüm sürecini başlattığımızı işaretle.
            miniGameTriggered = true;
            Debug.Log("Zeplin'in canı sıfırlandı. Ölüm süreci başlatılıyor...");

            // Canlanma hakkımız var mı diye kontrol et.
            if (PlayerData.Instance != null && !PlayerData.Instance.revivedOnce)
            {
                // Canlanma hakkı VAR: Checkpoint al ve mini oyunu tetikle.
                if (LevelManager.Instance != null)
                {
                    PlayerData.Instance.savedLevel = LevelManager.Instance.currentLevel;
                    Debug.Log($"Checkpoint alındı. Kaydedilen Level: {PlayerData.Instance.savedLevel}");
                }
                GameManager.Instance.TriggerMiniGame();
            }
            else
            {
                // Canlanma hakkı YOK: Direkt oyun sonu.
                Debug.Log("Canlanma hakkı zaten kullanıldı veya bulunamadı. Oyun Bitti.");
                GameManager.Instance.GameOver();
            }
        }
    }

    void Die()
    {
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }

       
        Destroy(gameObject);
    }

    void UpdateUI()
    {
        if (playerData == null) return;

        float healthValue = playerData.zeplinSaglik;
        float healthPercent = (maxHealth > 0) ? (healthValue / maxHealth) : 0;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = healthValue;
        }
        if (fillRectTransform != null)
        {
            fillRectTransform.localScale = new Vector3(healthPercent, 1, 1);
        }
        if (healthText != null)
        {
            healthText.text = $"{healthValue} / {maxHealth}";
        }
    }

    private void ProcessCollision(GameObject other)
    {
        if (other.CompareTag("Enemy"))
        {
            int damage = other.GetComponent<Enemy>()?.GetDamageAmount() ?? 10;
            Debug.Log("Zeplin canı (önce): " + playerData.zeplinSaglik);
            TakeDamage(damage);
            Destroy(other);
        }
        else if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null && bullet.isEnemyBullet)
            {
                TakeDamage(bullet.damage);
                Destroy(other);
            }
        }
        else if (other.CompareTag("Rocket"))
        {
            RocketProjectile rocket = other.GetComponent<RocketProjectile>();
            if (rocket != null && rocket.isEnemyRocket)
            {
                TakeDamage(rocket.damage);
                Destroy(other);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollision(collision.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ProcessCollision(other.gameObject);
    }
}