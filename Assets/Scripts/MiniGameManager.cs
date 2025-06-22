using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MiniGameManager : MonoBehaviour
{
    // Oyunun tamamlanması için gerekli puan
    public int pointsToWin = 100;

    // Mevcut puan
    private int currentPoints = 0;

    // Singleton yapısı
    public static MiniGameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log($"MiniGame başladı! Kazanmak için {pointsToWin} puan topla.");

        // PlayerData var mı kontrol et (zorunlu değil ama debug için yararlı)
        if (PlayerData.Instance == null)
        {
            Debug.LogWarning("PlayerData bulunamadı! Zeplin sahneye dönünce yaratılmalı!");
        }
    }

    // Puan eklemek için kullanılacak method (butonlar veya diğer oyun mekanikleri tarafından çağrılabilir)
    public void AddPoints(int points)
    {
        currentPoints += points;
        Debug.Log("Puan kazanıldı! Mevcut puan: " + currentPoints + " / " + pointsToWin);

        // Kazanma şartı sağlandı mı kontrol et
        if (currentPoints >= pointsToWin)
        {
            WinMiniGame();
        }
    }

    // Mini oyunu kazanma durumu
    public void WinMiniGame()
    {
        if (PlayerData.Instance != null)
        {
            // --- EN ÖNEMLİ SATIR ---
            // Bu, Zeplin'e yeniden doğması gerektiğini söyler.
            PlayerData.Instance.isPlayerRespawned = true;

            // Canlanma hakkının kullanıldığını işaretle.
            PlayerData.Instance.revivedOnce = true;

            Debug.Log("<color=cyan>[MiniGameManager] WinMiniGame çağrıldı. isPlayerRespawned = true yapıldı.</color>");
        }

        // Sonucu GameManager'a bildir.
        if (GameManager.Instance != null)
            GameManager.Instance.OnMiniGameResult(true);
        else
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("MiniOyun");
    }


    // Mini oyunu kaybetme durumu
    public void LoseMiniGame()
{
    if (GameManager.Instance != null)
        GameManager.Instance.OnMiniGameResult(false);
    else
    {
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("MiniOyun");
        UnityEngine.SceneManagement.SceneManager.LoadScene("OyunSonu");
    }
}

    // Test için: UI butonlarına bağlanabilecek metodlar
    public void TestAddPoints()
    {
        AddPoints(10);
    }

    public void TestWinGame()
    {
        WinMiniGame();
    }

    public void TestLoseGame()
    {
        LoseMiniGame();
    }
}

