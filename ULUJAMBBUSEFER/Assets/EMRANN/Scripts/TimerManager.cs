using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    [Header("Zaman Ayarları")]
    public float levelTime = 60f; // Level süresi
    public float cloneTimeCost = 10f; // Klon başına kaybedilen süre

    [Header("UI Referansları")]
    public Text timerText; // Inspector'dan atayacağız

    private float currentTime;
    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sadece oyun sahnesinde timer'ı başlat
        if (scene.name == "GameScene")
        {
            // TimerText'i doğru şekilde bul
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Text[] texts = canvas.GetComponentsInChildren<Text>();
                foreach (Text text in texts)
                {
                    if (text.gameObject.name == "TimerText")
                    {
                        timerText = text;
                        break;
                    }
                }
            }

            if (timerText == null)
            {
                Debug.LogError("TimerText bulunamadı! Lütfen Canvas içinde 'TimerText' adında bir Text objesi oluşturun.");
            }

            ResetTimer();
        }
    }

    private void Update()
    {
        if (isGameOver) return;

        currentTime -= Time.deltaTime;
        UpdateTimerDisplay();

        if (currentTime <= 0)
        {
            GameOver();
        }
    }

    public void ResetTimer()
    {
        currentTime = levelTime;
        isGameOver = false;
        UpdateTimerDisplay();
    }

    public bool CanCreateClone()
    {
        return currentTime >= cloneTimeCost;
    }

    public void DeductTimeForClone()
    {
        if (CanCreateClone())
        {
            currentTime -= cloneTimeCost;
            UpdateTimerDisplay();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int seconds = Mathf.FloorToInt(currentTime);
            timerText.text = seconds.ToString();
        }
    }

    private void GameOver()
    {
        isGameOver = true;
        FadeController fade = FindObjectOfType<FadeController>();
        if (fade != null)
        {
            fade.gameObject.SetActive(true);
            fade.FadeOutAndRestart();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
} 