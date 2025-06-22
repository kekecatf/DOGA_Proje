using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int killedEnemyCount = 0;
    public float gameTime = 0f;

    private GameObject gameSceneRootObject;


    public static GameManager Instance { get; private set; }

    public string gameSceneName = "GameScene";
    public string miniGameSceneName = "MiniOyun";
    public string gameOverSceneName = "OyunSonu";

    private bool miniGameTriggered = false;
    public bool isGameOver = false;
    private bool hasRespawned = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // GameManager sahneler arası kalsın
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!isGameOver && SceneManager.GetActiveScene().name == "GameScene")
        {
            gameTime += Time.unscaledDeltaTime;
        }
    }

    // Mini oyunu kaybedersen çağrılır
    public void GameOverAfterMiniGame()
    {
        Time.timeScale = 1f;
        miniGameTriggered = false;

        // MiniOyun sahnesini kaldır ve GameOver sahnesine geç
        SceneManager.UnloadSceneAsync(miniGameSceneName);
        SceneManager.LoadScene(gameOverSceneName);
    }

    // GameScene UI ve inputlarını aç/kapat
    public void SetGameSceneActive(bool active)
    {
        if (gameSceneRootObject == null)
        {
            gameSceneRootObject = GameObject.Find("[SCENE_ROOT]");
        }

        if (gameSceneRootObject != null)
        {
            // Adım 1: Ana kök objeyi her zamanki gibi aktif/pasif yap.
            gameSceneRootObject.SetActive(active);
            Debug.Log($"[GameManager] GameScene'in kök objesi ([SCENE_ROOT]) '{(active ? "AKTİF" : "PASİF")}' hale getirildi.");

            // --- YENİ EKLENEN "UYANDIRMA" KODU ---
            // Adım 2: Eğer sahneyi AKTİF hale getiriyorsak, Zeplin'in de aktif olduğundan emin ol.
            if (active)
            {
                // Kök objenin altındaki Zeplin'i bul.
                // transform.Find, pasif objeleri de bulabilir.
                Transform zeplinTransform = gameSceneRootObject.transform.Find("Zeplin");

                if (zeplinTransform != null)
                {
                    // Zeplin'in GameObject'ini kesin olarak aktif et.
                    zeplinTransform.gameObject.SetActive(true);
                    Debug.Log("<color=cyan>[GameManager] Zeplin objesi 'elle' aktif edildi.</color>");
                }
                else
                {
                    Debug.LogWarning("[GameManager] [SCENE_ROOT] altında 'Zeplin' objesi bulunamadı.");
                }
            }
        }
        else
        {
            Debug.LogError("[GameManager] HATA: Sahnede '[SCENE_ROOT]' adında bir kök obje bulunamadı!");
        }
    }

    public void TriggerMiniGame()
    {
        Debug.Log("<color=orange>--- [GameManager] Mini oyuna GEÇİŞ tetiklendi. ---</color>");
        if (miniGameTriggered) return;
        if (PlayerData.Instance != null && PlayerData.Instance.revivedOnce)
        {
            // Mini oyun hakkı zaten kullanıldıysa, direkt oyun sonu
            GameOver();
            return;
        }
        miniGameTriggered = true;

        // Kayıt al
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.SaveValues();
        }

        Time.timeScale = 1f;
        // GameScene UI ve inputlarını devre dışı bırak
        SetGameSceneActive(false);
        if (!IsSceneOpen(miniGameSceneName))
            SceneManager.LoadScene(miniGameSceneName, LoadSceneMode.Additive);
    }

    // Sahne açık mı kontrolü
    private bool IsSceneOpen(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == sceneName)
                return true;
        }
        return false;
    }

    
    

    public void OnMiniGameResult(bool wonMiniGame)
    {
        miniGameTriggered = false;

        if (wonMiniGame)
        {
            Debug.Log("Mini oyun kazanıldı. 'MiniOyun' sahnesi UNLOAD ediliyor...");

            // --- DEĞİŞİKLİK BURADA ---
            // Unload işlemi bittiğinde çalışacak olan kod bloğunu (.completed +=) kullanıyoruz.
            // Bu, zamanlama sorununu çözer.
            SceneManager.UnloadSceneAsync(miniGameSceneName).completed += (asyncOperation) =>
            {
                // BU KOD, SAHNE TAMAMEN KALDIRILDIKTAN SONRA ÇALIŞIR.

                Debug.Log("'MiniOyun' sahnesi başarıyla kaldırıldı. GameScene'deki mantık devam ediyor.");

                // Şimdi, sahne durumu stabilken, GameScene'i tekrar aktif hale getiriyoruz.
                SetGameSceneActive(true);

                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.RestoreLevelFromPlayerData();
                }
                else
                {
                    Debug.LogError("Dönüşte LevelManager.Instance bulunamadı!");
                }
            };
        }
        else
        {
            Debug.Log("Mini oyun kaybedildi. Oyun Bitti.");
            GameOver();
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        // Bir sonraki oyun için checkpoint verilerini sıfırla ki temiz başlasın.
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.isPlayerRespawned = false;
            PlayerData.Instance.revivedOnce = false;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(gameOverSceneName);
    }

    public void EnemyKilled(EnemyType enemyType)
    {
        killedEnemyCount++;
        Debug.Log($"Öldürülen düşman: {enemyType}, Toplam: {killedEnemyCount}");
    }

    public void TryInitializePlayerData()
    {
        if (FindObjectOfType<PlayerData>() == null)
        {
            Debug.LogWarning("PlayerData bulunamadı!");
            return;
        }

        PlayerData playerData = FindObjectOfType<PlayerData>();
        playerData.metalPara = 0;
    }

    public bool HasRespawned()
    {
        return hasRespawned;
    }

    public void ResetRespawnFlag()
    {
        hasRespawned = false;
    }
}