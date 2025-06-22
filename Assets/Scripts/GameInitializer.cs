// GameInitializer.cs

using UnityEngine;
using System.Collections;

public class GameInitializer : MonoBehaviour
{
    private static GameInitializer _instance;

    // --- YENİ EKLENEN SATIR ---
    // Bu statik bayrak, oyunun tüm oturumu boyunca sadece BİR KEZ başlatılmasını garantiler.
    private static bool hasBeenInitialized = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // --- YENİ EKLENEN KONTROL ---
            // Yöneticileri sadece daha önce hiç başlatılmadıysa başlat.
            if (!hasBeenInitialized)
            {
                InitializeManagers();

                // Başlatma işlemi bittikten sonra bayrağı 'true' yap.
                // Bu sayede bu blok bir daha asla çalışmaz.
                hasBeenInitialized = true;
            }
        }
        else
        {
            // Eğer bu obje bir kopyaysa, hiçbir şey yapmadan yok et.
            Destroy(gameObject);
        }
    }

    // Bu fonksiyona veya diğerlerine HİÇBİR DEĞİŞİKLİK YAPMIYORUZ.
    // Hepsi olduğu gibi kalacak.
    private void InitializeManagers()
    {
        Debug.LogWarning("<color=green>!!!!!! [GameInitializer] OYUN YÖNETİCİLERİ İLK KEZ BAŞLATILIYOR. !!!!!!</color>");

        // PlayerData oluştur ve default değerleri yükle
        PlayerData playerData = CheckOrCreateManager<PlayerData>("PlayerData");
        if (playerData == null)
        {
            Debug.LogError("GameInitializer: PlayerData oluşturulamadı!");
            return;
        }

        StartCoroutine(DelayedInitialization());
    }

    // ... BU NOKTADAN SONRAKİ TÜM KODUNUZ OLDUĞU GİBİ KALACAK ...
    // ... DelayedInitialization(), CheckOrCreateManager() vs. hepsi aynı ...
    private IEnumerator DelayedInitialization()
    {
        yield return null;

        GameManager gameManager = CheckOrCreateManager<GameManager>("GameManager");
        if (gameManager == null)
        {
            Debug.LogError("GameInitializer: GameManager oluşturulamadı!");
            yield break;
        }

        if (PlayerData.Instance == null)
        {
            Debug.LogWarning("GameInitializer: PlayerData singleton bulunamadı, GameManager üzerinden initialize ediliyor...");
            gameManager.GetComponentInChildren<GameManager>().TryInitializePlayerData();
        }

        EnemySpawner enemySpawner = CheckOrCreateManager<EnemySpawner>("EnemySpawner");

        CreateItemDropManager();

        if (PlayerData.Instance != null && PlayerData.Instance.isPlayerRespawned)
        {
            CheckOrCreatePlayerRespawner();
        }

        Debug.Log("GameInitializer: Tüm oyun yöneticileri başarıyla başlatıldı.");
    }

    private void CreateItemDropManager()
    {
        ItemDropManager dropManager = FindObjectOfType<ItemDropManager>();
        if (dropManager == null)
        {
            GameObject managerObj = new GameObject("ItemDropManager");
            dropManager = managerObj.AddComponent<ItemDropManager>();

            GameObject itemPrefab = Resources.Load<GameObject>("ItemPrefab");
            if (itemPrefab != null)
            {
                dropManager.itemPrefab = itemPrefab;
                Debug.Log("GameInitializer: ItemDropManager oluşturuldu ve prefab atandı.");
            }
            else
            {
                Debug.LogWarning("GameInitializer: ItemPrefab bulunamadı, item düşme devre dışı.");
            }

            DontDestroyOnLoad(managerObj);
        }
        else
        {
            Debug.Log("GameInitializer: ItemDropManager zaten var.");
        }
    }

    private void CheckOrCreatePlayerRespawner()
    {
        PlayerRespawner respawner = FindObjectOfType<PlayerRespawner>();
        if (respawner == null)
        {
            GameObject respawnerObj = new GameObject("PlayerRespawner");
            respawner = respawnerObj.AddComponent<PlayerRespawner>();

            respawner.playerPrefab = Resources.Load<GameObject>("Prefabs/Player");

            if (respawner.playerPrefab == null)
            {
                respawner.playerPrefab = Resources.Load<GameObject>("Player");

                if (respawner.playerPrefab == null)
                {
                    GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
                    if (existingPlayer != null)
                    {
                        Debug.LogWarning("GameInitializer: Prefab bulunamadı, sahnedeki Player kullanılacak.");
                        respawner.playerPrefab = existingPlayer;
                    }
                    else
                    {
                        Debug.LogError("GameInitializer: Player prefabı hiç bulunamadı!");
                    }
                }
            }

            if (respawner.playerPrefab != null)
            {
                Debug.Log($"GameInitializer: Player prefabı yüklendi: {respawner.playerPrefab.name}");
            }

            GameObject respawnPointObj = GameObject.FindGameObjectWithTag("RespawnPoint");
            if (respawnPointObj != null)
            {
                respawner.respawnPoint = respawnPointObj.transform;
                Debug.Log($"GameInitializer: RespawnPoint bulundu: {respawnPointObj.name}");
            }
            else
            {
                Debug.LogWarning("GameInitializer: RespawnPoint bulunamadı, varsayılan pozisyon kullanılacak.");
            }

            DontDestroyOnLoad(respawnerObj);
            Debug.Log("GameInitializer: PlayerRespawner oluşturuldu.");
        }
    }

    private T CheckOrCreateManager<T>(string managerName) where T : MonoBehaviour
    {
        T manager = FindObjectOfType<T>();
        if (manager == null)
        {
            GameObject managerObj = new GameObject(managerName);
            manager = managerObj.AddComponent<T>();

            if (typeof(T) == typeof(PlayerData))
            {
                Debug.Log("GameInitializer: PlayerData oluşturuldu, Inspector’dan ayarlanabilir.");
            }

            DontDestroyOnLoad(managerObj);
            Debug.Log($"GameInitializer: {managerName} oluşturuldu.");
        }

        return manager;
    }

    public static void Initialize()
    {
        if (_instance == null)
        {
            GameObject initializerObj = new GameObject("GameInitializer");
            initializerObj.AddComponent<GameInitializer>();
            Debug.Log("GameInitializer: Başlatıcı oluşturuldu.");
        }
    }
}