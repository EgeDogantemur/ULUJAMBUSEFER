using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    [Header("Zaman Ayarları")]
    public float levelTime = 120f; // 2 dakika başlangıç süresi
    public float cloneTimeCost = 5f; // Klon maliyetini düşürdüm

    [Header("UI Referansları")]
    public Text timerText;

    private float currentTime;
    private bool isGameOver = false;
    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Başlangıç değerlerini ayarla
            currentTime = levelTime;
            UpdateTimerDisplay();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindTimerText();
        ResetTimer();
        Debug.Log($"Timer başlatıldı: {currentTime} saniye");
    }

    private void FindTimerText()
    {
        if (timerText == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Text[] texts = canvas.GetComponentsInChildren<Text>(true);
                foreach (Text text in texts)
                {
                    if (text.gameObject.name == "TimerText")
                    {
                        timerText = text;
                        Debug.Log("TimerText bulundu!");
                        break;
                    }
                }
            }

            if (timerText == null)
            {
                Debug.LogError("TimerText bulunamadı! Canvas içinde 'TimerText' adında bir Text komponenti olduğundan emin olun.");
                
                // Geçici text oluştur
                GameObject tempTextObj = new GameObject("TimerText");
                tempTextObj.transform.SetParent(canvas.transform);
                timerText = tempTextObj.AddComponent<Text>();
                timerText.rectTransform.anchoredPosition = new Vector2(100, -50);
                timerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                timerText.fontSize = 36;
                timerText.color = Color.white;
                Debug.Log("Geçici TimerText oluşturuldu!");
            }
        }
    }

    private void Update()
    {
        if (isGameOver || isPaused) return;

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
        isPaused = false;
        UpdateTimerDisplay();
        Debug.Log($"Timer sıfırlandı: {currentTime} saniye");
    }

    public bool CanCreateClone()
    {
        bool canCreate = currentTime >= cloneTimeCost;
        if (!canCreate)
        {
            Debug.Log($"Klon oluşturmak için yeterli süre yok. Mevcut süre: {currentTime}, Gereken süre: {cloneTimeCost}");
        }
        return canCreate;
    }

    public void DeductTimeForClone()
    {
        if (CanCreateClone())
        {
            currentTime -= cloneTimeCost;
            UpdateTimerDisplay();
            Debug.Log($"Klon için süre düşüldü. Kalan süre: {currentTime}");
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void PauseTimer()
    {
        isPaused = true;
    }

    public void ResumeTimer()
    {
        isPaused = false;
    }

    private void GameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        FadeController fade = FindObjectOfType<FadeController>();
        
        if (fade != null)
        {
            fade.gameObject.SetActive(true);
            fade.FadeOutAndRestart();
        }
        else
        {
            StartCoroutine(DelayedRestart());
        }
    }

    private System.Collections.IEnumerator DelayedRestart()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public bool IsPaused()
    {
        return isPaused;
    }
} 