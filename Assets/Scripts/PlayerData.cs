using UnityEngine;
using UnityEngine.UI;

public class PlayerData : MonoBehaviour
{
    private const int DEFAULT_METAL_PARA = 500;
    private const int DEFAULT_ZEPLIN_SAGLIK = 300;
    private const int DEFAULT_ANA_GEMI_SAGLIK = 100;

    public int metalPara = 500;

    [Header("Zeplin Ayarları")]
    public int zeplinSaglik=300;
    public int zeplinMaxSaglik = 300;
    public int zeplinSaglikLevel = 0;

    public int zeplinMinigunDamage = 10;
    public int zeplinMinigunLevel = 0;
    public float zeplinMinigunCooldown = 1.0f;
    public int zeplinMinigunCount = 1;

    public int zeplinRoketDamage = 60;
    public int zeplinRoketLevel = 0;
    public int zeplinRoketCount = 1;
    public float zeplinRoketDelay = 2.0f;

    [Header("Oyuncu Ayarları")]
    public int anaGemiSaglik = 100;
    public int anaGemiSaglikLevel = 0;

    public int anaGemiMinigunDamage = 3;
    public int anaGemiMinigunLevel = 0;
    public float anaGemiMinigunCooldown = 0.4f;
    public int anaGemiMinigunCount = 1;

    public int anaGemiRoketDamage = 60;
    public int anaGemiRoketLevel = 0;
    public int anaGemiRoketCount = 1;
    public float anaGemiRoketDelay = 2.0f;
    public float anaGemiRoketSpeed = 10.0f;

    [Header("Düşman Ayarları")]
    public int enemyBaseHealth = 50;
    public int enemyBaseDamage = 10;
    public int enemyBaseScoreValue = 25;
    public float enemyDifficultyMultiplier = 1.0f;

    [Header("Düşman Hasar Ayarları")]
    public int enemyKamikazeDamage = 10;
    public int enemyMinigunDamage = 30;
    public int enemyRocketDamage = 20;

    public float enemyKamikazeDamageMultiplier = 0.8f;
    public float enemyMinigunDamageMultiplier = 0.3f;
    public float enemyRocketDamageMultiplier = 1.5f;

    [Header("Düşman Atış Hızı Ayarları")]
    public float enemyMinigunFireRate = 2.0f;
    public float enemyRocketFireRate = 1.0f;
    public float enemyFireRateMultiplier = 1.0f;

    public static PlayerData Instance { get; private set; }

    [Header("Oyun Mekanikleri")]
    public bool isPlayerRespawned = false;
    public int savedLevel = 1;
    public bool revivedOnce = false;
   


    private void Awake()
    {
        Debug.Log($"PlayerData Awake çağrıldı. Obje Adı: {gameObject.name}, Instance ID: {GetInstanceID()}");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"---> YENİ PlayerData Instance oluşturuldu ve kalıcı yapıldı.");
        }
        else
        {
            Debug.LogWarning($"<--- Zaten bir PlayerData var. Bu kopya ({gameObject.name}) yok ediliyor.");
            Destroy(gameObject);
        }
    }

    

    

    public void SaveValues()
    {
        if (zeplinSaglik <= 0)
        {
            Debug.LogWarning($"Negatif zeplinSaglik ({zeplinSaglik}) kaydediliyor.");
            
        }

    }

    public void ResetAllData()
    {
        metalPara = DEFAULT_METAL_PARA;

        zeplinSaglik = DEFAULT_ZEPLIN_SAGLIK;
        zeplinSaglikLevel = 0;
        zeplinMinigunDamage = 10;
        zeplinMinigunLevel = 0;
        zeplinMinigunCooldown = 1.0f;
        zeplinMinigunCount = 1;
        zeplinRoketDamage = 60;
        zeplinRoketLevel = 0;
        zeplinRoketCount = 1;
        zeplinRoketDelay = 2.0f;

        anaGemiSaglik = DEFAULT_ANA_GEMI_SAGLIK;
        anaGemiSaglikLevel = 0;
        anaGemiMinigunDamage = 5;
        anaGemiMinigunLevel = 0;
        anaGemiMinigunCooldown = 0.4f;
        anaGemiMinigunCount = 1;
        anaGemiRoketDamage = 60;
        anaGemiRoketLevel = 0;
        anaGemiRoketCount = 1;
        anaGemiRoketDelay = 2.0f;
        anaGemiRoketSpeed = 10.0f;

        enemyDifficultyMultiplier = 1.0f;
        enemyFireRateMultiplier = 1.0f;

        enemyKamikazeDamage = 10;
        enemyMinigunDamage = 3;
        enemyRocketDamage = 20;
        enemyKamikazeDamageMultiplier = 0.8f;
        enemyMinigunDamageMultiplier = 0.3f;
        enemyRocketDamageMultiplier = 1.5f;

        isPlayerRespawned = false;
        revivedOnce = false;

        SaveValues();
        Debug.Log("Tüm PlayerData değerleri sıfırlandı.");
    }

    public void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Tüm PlayerPrefs silindi.");
        zeplinSaglik = DEFAULT_ZEPLIN_SAGLIK;
        PlayerPrefs.SetInt("zeplinSaglik", DEFAULT_ZEPLIN_SAGLIK);
        PlayerPrefs.Save();
    }

    void Start()
    {
        UpdateEnemyDifficulty();
        
    }

    void Update() { }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            Debug.Log("Inspector değişikliği algılandı, kaydediliyor.");
            SaveValues();
        }
    }

    private void OnDestroy()
    {
        SaveValues();
        Debug.Log("Nesne yok edilirken değerler kaydedildi.");
    }

    public void UpdateEnemyDifficulty()
    {
        int playerLevel = Mathf.Max(anaGemiMinigunLevel, anaGemiRoketLevel);
        enemyDifficultyMultiplier = 1f + (playerLevel * 0.2f);
        enemyFireRateMultiplier = 1f + (playerLevel * 0.1f);
        Debug.Log("Düşman zorluğu güncellendi.");
    }

    public int CalculateEnemyHealth() => Mathf.RoundToInt(enemyBaseHealth * enemyDifficultyMultiplier);
    public int CalculateEnemyDamage() => Mathf.RoundToInt(enemyBaseDamage * enemyDifficultyMultiplier);
    public int CalculateKamikazeDamage() => Mathf.RoundToInt(enemyKamikazeDamage * enemyDifficultyMultiplier * enemyKamikazeDamageMultiplier);
    public int CalculateMinigunDamage() => Mathf.RoundToInt(enemyMinigunDamage * enemyDifficultyMultiplier * enemyMinigunDamageMultiplier);
    public int CalculateRocketDamage() => Mathf.RoundToInt(enemyRocketDamage * enemyDifficultyMultiplier * enemyRocketDamageMultiplier);
    public float CalculateMinigunFireRate() => enemyMinigunFireRate * enemyFireRateMultiplier;
    public float CalculateRocketFireRate() => enemyRocketFireRate * enemyFireRateMultiplier;
    public int CalculateEnemyScoreValue() => Mathf.RoundToInt(enemyBaseScoreValue * enemyDifficultyMultiplier);
}
