// MiniGamePlayer.cs - DOĞRU HALİ

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class MiniGamePlayer : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float horizontalSpeed = 5f;

    // MiniGameManager referansını tutmak için bir değişken
    private MiniGameManager miniGameManager;

    void Start()
    {
        // Oyun başladığında, sahnede bulunan MiniGameManager'ı bul ve referansını al.
        miniGameManager = FindObjectOfType<MiniGameManager>();

        // Eğer sahnede MiniGameManager yoksa, hata ver.
        if (miniGameManager == null)
        {
            Debug.LogError("MiniGamePlayer: Sahnede MiniGameManager bulunamadı!");
        }
    }

    void Update()
    {
        // Aşağıya doğru sabit hareket
        transform.Translate(Vector2.down * moveSpeed * Time.deltaTime);

        // Sağ-sol hareket
        float horizontalInput = 0f;
        if (Keyboard.current.leftArrowKey.isPressed)
            horizontalInput = -1f;
        else if (Keyboard.current.rightArrowKey.isPressed)
            horizontalInput = 1f;

        transform.Translate(Vector2.right * horizontalInput * horizontalSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Eğer MiniGameManager bulunamadıysa, hiçbir şey yapma.
        if (miniGameManager == null) return;

        // Zemin'e değdiysen, KAZANDIN demektir.
        if (other.CompareTag("Zemin"))
        {
            // Oyuncuyu ve hareketini durdur.
            enabled = false; // Bu script'i (Update'i) devre dışı bırakır.

            // MiniGameManager'a "KAZANDIN" sinyalini gönder.
            // Gerisini o halledecek.
            miniGameManager.WinMiniGame();
        }
        // Engel'e değdiysen, KAYBETTİN demektir.
        else if (other.CompareTag("Engel"))
        {
            // Oyuncuyu ve hareketini durdur.
            enabled = false;

            // MiniGameManager'a "KAYBETTİN" sinyalini gönder.
            // Gerisini o halledecek.
            miniGameManager.LoseMiniGame();
        }
    }

    // Artık bu Coroutine'lere ihtiyacımız yok, çünkü
    // bu mantık GameManager ve UI script'leri tarafından yönetilecek.
    // Bunları silebilirsiniz.
    /*
    IEnumerator KazandinVeDevamEt() { ... }
    IEnumerator KaybettinVeBitisEkrani() { ... }
    */
}