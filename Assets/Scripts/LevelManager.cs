using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Settings")]

    [HideInInspector]
    public int currentLevel = 1;
    public int maxLevel = 5;

    [Header("Stat Modifiers")]
    public int kamikazeHealthAtLevel3 = 3;
    public float minigunFireRateAtLevel3 = 3f;

    private int previousLevelForDebug = -1;
    private void Awake()
    {
        Debug.Log($"<color=cyan>[LevelManager] Awake çağrıldı. Başlangıç Level: {currentLevel}</color>");
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);


    }

    void Update()
    {
        if (currentLevel != previousLevelForDebug)
        {
            Debug.LogWarning($"<color=magenta>[LevelManager] SEVİYE DEĞİŞTİ! Eski: {previousLevelForDebug}, Yeni: {currentLevel}</color>");
            previousLevelForDebug = currentLevel;
        }
    }

    // MiniOyun sonrası additive modda GameScene açıkken seviye restore için çağrılır
    public void RestoreLevelFromPlayerData()
{
    if (PlayerData.Instance != null && PlayerData.Instance.isPlayerRespawned)
    {
            Debug.Log($"<color=yellow>[LevelManager] Checkpoint'ten YÜKLENDİ. Yeni Level: {currentLevel}</color>");
            currentLevel = PlayerData.Instance.savedLevel;
        Debug.Log("[LevelManager] RestoreLevelFromPlayerData: currentLevel=" + currentLevel);
        PlayerData.Instance.isPlayerRespawned = false;
    }
}

    public int GetEnemyCount()
    {
        switch (currentLevel)
        {
            case 1: return 6;
            case 2: return 8;
            case 3: return 10;
            case 4: return 12;
            case 5: return 4; // Bossla birlikte 4 düman
            default: return 6;
        }
    }

    public int GetKamikazeHealth()
    {
        return currentLevel >= 3 ? kamikazeHealthAtLevel3 : 1;
    }

    public float GetMinigunFireRate()
    {
        if (currentLevel >= 3)
            return 3f;
        return 1f;
    }

    public bool IsBossLevel()
    {
        return currentLevel == maxLevel;
    }

    public void NextLevel()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            Debug.Log("Yeni Level: " + currentLevel);
        }
        else
        {
            Debug.Log("Son seviye ulaşıldı. Oyun bitmiş olabilir.");
            // İstersen buraya event/final sahne çağırsı ekleyebilirsin.
        }
    }

    public void ResetLevel() // Eğer böyle bir fonksiyonunuz varsa
    {
        currentLevel = 1;
        Debug.LogWarning($"<color=red>[LevelManager] ResetLevel çağrıldı! Level 1'e sıfırlandı.</color>");
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }
}
