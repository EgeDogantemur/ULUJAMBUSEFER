using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    public static GameInitializer Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeManagers()
    {
        // Gerekli yöneticileri oluştur
        CreateManagerIfNotExists<InputSystemWrapper>("InputSystemWrapper");
        CreateManagerIfNotExists<SoundManager>("SoundManager");
    }

    private T CreateManagerIfNotExists<T>(string name) where T : Component
    {
        // Manager zaten varsa bul
        T existingManager = FindObjectOfType<T>();
        if (existingManager != null)
        {
            return existingManager;
        }

        // Yoksa yeni oluştur
        GameObject managerObject = new GameObject(name);
        managerObject.transform.SetParent(transform);
        T manager = managerObject.AddComponent<T>();
        Debug.Log($"{name} oluşturuldu.");
        return manager;
    }

    // Oyun başladığında çağrılacak
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnGameStart()
    {
        // Sadece oyun ilk başladığında çalışacak
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // İlk sahne yüklenmeden önce GameInitializer'ı oluştur
        GameObject initializer = new GameObject("GameInitializer");
        initializer.AddComponent<GameInitializer>();
        Debug.Log("Oyun başlatıldı, GameInitializer oluşturuldu.");
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Her sahne yüklendiğinde çağrılır
        if (Instance != null)
        {
            Instance.InitializeManagers();
        }
    }
} 