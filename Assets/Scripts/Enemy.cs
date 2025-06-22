using UnityEngine;
using System.Collections;

public enum EnemyType
{
    Kamikaze,   // Hedefe doğru gidip çarparak hasar verir.
    Minigun     // Uzaktan ateş eder.
}

// Tüm hasar alabilen objeler için standart bir arayüz.
public interface IDamageable
{
    void TakeDamage(int damageAmount);
}

/// <summary>
/// Düşmanların genel davranışlarını, hareketini, saldırısını ve ölümünü yönetir.
/// Düşman tipi (EnemyType) Inspector'dan ayarlanarak davranışları değiştirilebilir.
/// </summary>
public class Enemy : MonoBehaviour, IDamageable
{
    
    private enum TargetType { Player, Zeplin }
    private TargetType chosenTarget;


    [Header("Düşman Tipi")]
    public EnemyType enemyType = EnemyType.Kamikaze;

    [Header("Temel Ayarlar")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public int scoreValue = 25;
    public GameObject deathEffect;

    [Header("Ateş Ayarları (Minigun için)")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    public float attackRange = 8f;
    public float safeDistance = 6f;
    public float minAttackDistance = 6f;
    public float initialStabilizationTime = 1.5f;

    // Dahili değişkenler
    protected Transform targetTransform;
    protected PlayerData playerData;
    protected SpriteRenderer spriteRenderer;
    protected int currentHealth;
    protected int damage;
    public float nextFireTime = 0f;
    private float spawnTime;
    private bool isDying = false;

    protected virtual void Start()
    {
        playerData = FindObjectOfType<PlayerData>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (playerData == null)
        {
            Debug.LogError("PlayerData objesi sahnede bulunamadı! Düşmanlar düzgün çalışmayabilir.");
            this.enabled = false;
            return;
        }

        spawnTime = Time.time;
        InitializeEnemyStats();

        
        ChooseTarget(); // Yeni metot çağrısı
    }

    private void ChooseTarget()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        Zeplin zeplinObj = FindObjectOfType<Zeplin>();

        if (playerObj == null && zeplinObj == null)
        {
            targetTransform = null;
            return;
        }

        // Rastgele hedef seç (%50 şans)
        bool targetPlayer = Random.value < 0.5f;

        if (targetPlayer && playerObj != null && !Player.isDead)
        {
            targetTransform = playerObj.transform;
        }
        else if (zeplinObj != null)
        {
            targetTransform = zeplinObj.transform;
        }
        else if (playerObj != null && !Player.isDead)
        {
            targetTransform = playerObj.transform; // yedek: biri yoksa diğeri seçilsin
        }
    }


    protected virtual void Update()
    {
        if (isDying) return;

        if (transform.position.y < -100f)
        {
            Destroy(gameObject);
            return;
        }

        UpdateTarget();

        if (targetTransform == null) return;

        HandleBehavior();
        UpdateSpriteFlipping();
    }

    private void InitializeEnemyStats()
    {
        int baseHealth = playerData.CalculateEnemyHealth();
        moveSpeed = Random.Range(moveSpeed * 0.8f, moveSpeed * 1.2f);

        switch (enemyType)
        {
            case EnemyType.Kamikaze:
                currentHealth = LevelManager.Instance != null ? LevelManager.Instance.GetKamikazeHealth() : baseHealth / 2;
                damage = playerData.CalculateKamikazeDamage();
                moveSpeed *= 1.3f;
                scoreValue = (int)(playerData.CalculateEnemyScoreValue() * 0.8f);
                GetComponent<Collider2D>().isTrigger = true;
                break;

            case EnemyType.Minigun:
                currentHealth = baseHealth;
                damage = playerData.CalculateMinigunDamage();
                fireRate = LevelManager.Instance != null ? LevelManager.Instance.GetMinigunFireRate() : playerData.CalculateMinigunFireRate();
                moveSpeed *= 0.8f;
                scoreValue = (int)(playerData.CalculateEnemyScoreValue() * 1.2f);
                break;
        }

        chosenTarget = (Random.value < 0.5f) ? TargetType.Player : TargetType.Zeplin;
    }





    private void UpdateTarget()
    {
        if (chosenTarget == TargetType.Zeplin)
        {
            Zeplin zeplin = FindObjectOfType<Zeplin>();
            targetTransform = (zeplin != null) ? zeplin.transform : null;
        }
        else if (chosenTarget == TargetType.Player)
        {
            if (!Player.isDead)
            {
                GameObject player = GameObject.FindWithTag("Player");
                targetTransform = (player != null) ? player.transform : null;
            }
            else
            {
                targetTransform = null;
            }
        }
    }


    private void HandleBehavior()
    {
        switch (enemyType)
        {
            case EnemyType.Kamikaze:
                MoveTowardsTarget();
                break;
            case EnemyType.Minigun:
                RangedEnemyBehavior();
                break;
        }
    }

    protected virtual void MoveTowardsTarget()
    {
        if (targetTransform == null) return;

        Vector2 direction = (targetTransform.position - transform.position).normalized;
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;

        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void RangedEnemyBehavior()
    {
        if (targetTransform == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, targetTransform.position);
        Vector2 lookDirection = (targetTransform.position - transform.position).normalized;

        // Düşmanın hedefe doğru dönmesi
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Hareket davranışı
        if (distanceToTarget > attackRange)
        {
            // Hedefe yaklaş
            transform.position += (Vector3)lookDirection * moveSpeed * Time.deltaTime;
        }
        else if (distanceToTarget < safeDistance)
        {
            // Hedeften uzaklaş
            transform.position -= (Vector3)lookDirection * moveSpeed * Time.deltaTime;
        }
        // else: 6 ile 8 arasındaysa dur

        // Ateş etme kontrolü
        bool canFire = Time.time >= nextFireTime && (Time.time - spawnTime) >= initialStabilizationTime;
        bool inRange = distanceToTarget >= safeDistance && distanceToTarget <= attackRange;

        if (canFire && inRange)
        {
            FireMinigun();
            float fireRateMultiplier = (targetTransform.GetComponent<Zeplin>() != null) ? 0.7f : 1.0f;
            nextFireTime = Time.time + (1f / (fireRate * fireRateMultiplier));
        }
    }




    private void FireMinigun()
    {
        if (bulletPrefab == null || firePoint == null) return;

        Quaternion spreadRotation = Quaternion.Euler(0, 0, Random.Range(-3f, 3f));
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, transform.rotation * spreadRotation);

        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.isEnemyBullet = true;
            bulletComponent.damage = (targetTransform.GetComponent<Zeplin>() != null) ? (int)(damage * 0.75f) : damage;
        }
    }

    public virtual void TakeDamage(int damageAmount)
    {
        if (isDying) return;
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDying) return;
        isDying = true;
        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayExplosionSound();
        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
        if (spriteRenderer != null) spriteRenderer.enabled = false;

        if (GameManager.Instance != null) GameManager.Instance.EnemyKilled(enemyType);
        if (playerData != null) playerData.metalPara += scoreValue;
        if (ItemDropManager.Instance != null) ItemDropManager.Instance.TryDropItem(transform.position);

        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    private void UpdateSpriteFlipping()
    {
        if (spriteRenderer != null)
        {
            float zRotation = transform.eulerAngles.z;
            spriteRenderer.flipY = (zRotation > 90 && zRotation < 270);
        }
    }

    private void ProcessCollision(GameObject otherObject)
    {
        if (isDying) return;

        if (enemyType == EnemyType.Kamikaze)
        {
            IDamageable target = otherObject.GetComponent<IDamageable>();
            if (target != null && (otherObject.CompareTag("Player") || otherObject.CompareTag("Zeplin")))
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayKamikazeSound();
                target.TakeDamage(damage);
                Die();
                return;
            }
        }

        if (otherObject.CompareTag("Bullet"))
        {
            Bullet bullet = otherObject.GetComponent<Bullet>();
            if (bullet != null && !bullet.isEnemyBullet)
            {
                TakeDamage(bullet.damage);
                Destroy(otherObject);
            }
        }
        else if (otherObject.CompareTag("Rocket"))
        {
            RocketProjectile rocket = otherObject.GetComponent<RocketProjectile>();
            if (rocket != null && !rocket.isEnemyRocket)
            {
                TakeDamage(rocket.damage);
                rocket.HandleHit(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ProcessCollision(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollision(collision.gameObject);
    }

    public int GetDamageAmount() => damage;
    public int GetCurrentHealth() => currentHealth;
}