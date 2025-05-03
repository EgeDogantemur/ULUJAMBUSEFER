using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI Referansları")]
    public GameObject pauseMenuUI;
    public Slider volumeSlider;
    public Button muteButton;
    public Button resumeButton;
    public Button restartButton;
    public Button exitButton;

    private bool isPaused = false;
    private InputSystemWrapper inputManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Input Manager referansını al
        inputManager = InputSystemWrapper.Instance;
        if (inputManager == null)
        {
            Debug.LogError("InputSystemWrapper bulunamadı!");
        }

        SetupUIReferences();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ana menüde pause menu'yü devre dışı bırak
        if (scene.name == "MainMenu")
        {
            enabled = false;
            return;
        }
        
        enabled = true;
        
        // Referansları sıfırla
        pauseMenuUI = null;
        volumeSlider = null;
        muteButton = null;
        resumeButton = null;
        restartButton = null;
        exitButton = null;

        // Yeni referansları bul
        SetupUIReferences();
        
        // Oyun başlangıcında pause menu'yü kapat
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;
        }
    }

    private void SetupUIReferences()
    {
        // Canvas'ı bul
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas bulunamadı!");
            return;
        }

        // Pause Menu UI'ı bul
        if (pauseMenuUI == null)
        {
            Transform pauseMenuTransform = canvas.transform.Find("PauseMenu");
            if (pauseMenuTransform != null)
            {
                pauseMenuUI = pauseMenuTransform.gameObject;
                Debug.Log("PauseMenu bulundu: " + pauseMenuUI.name);
            }
            else
            {
                Debug.LogError("PauseMenu bulunamadı!");
                return;
            }
        }

        // Butonları ve slider'ı bul
        if (pauseMenuUI != null)
        {
            // Resume Button
            Transform resumeButtonTransform = pauseMenuUI.transform.Find("ResumeButton");
            if (resumeButtonTransform != null)
            {
                resumeButton = resumeButtonTransform.GetComponent<Button>();
                if (resumeButton != null)
                {
                    resumeButton.onClick.RemoveAllListeners();
                    resumeButton.onClick.AddListener(Resume);
                    Debug.Log("ResumeButton bulundu ve ayarlandı");
                }
            }

            // Restart Button
            Transform restartButtonTransform = pauseMenuUI.transform.Find("RestartButton");
            if (restartButtonTransform != null)
            {
                restartButton = restartButtonTransform.GetComponent<Button>();
                if (restartButton != null)
                {
                    restartButton.onClick.RemoveAllListeners();
                    restartButton.onClick.AddListener(Restart);
                    Debug.Log("RestartButton bulundu ve ayarlandı");
                }
            }

            // Exit Button
            Transform exitButtonTransform = pauseMenuUI.transform.Find("ExitButton");
            if (exitButtonTransform != null)
            {
                exitButton = exitButtonTransform.GetComponent<Button>();
                if (exitButton != null)
                {
                    exitButton.onClick.RemoveAllListeners();
                    exitButton.onClick.AddListener(ExitToMainMenu);
                    Debug.Log("ExitButton bulundu ve ayarlandı");
                }
            }

            // Volume Slider
            Transform volumeSliderTransform = pauseMenuUI.transform.Find("VolumeSlider");
            if (volumeSliderTransform != null)
            {
                volumeSlider = volumeSliderTransform.GetComponent<Slider>();
                if (volumeSlider != null)
                {
                    volumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
                    volumeSlider.onValueChanged.RemoveAllListeners();
                    volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
                    Debug.Log("VolumeSlider bulundu ve ayarlandı");
                }
            }

            // Mute Button
            Transform muteButtonTransform = pauseMenuUI.transform.Find("MuteButton");
            if (muteButtonTransform != null)
            {
                muteButton = muteButtonTransform.GetComponent<Button>();
                if (muteButton != null)
                {
                    UpdateMuteButtonText();
                    muteButton.onClick.RemoveAllListeners();
                    muteButton.onClick.AddListener(ToggleMute);
                    Debug.Log("MuteButton bulundu ve ayarlandı");
                }
            }
        }
    }

    private void Update()
    {
        if (inputManager != null && inputManager.GetPausePressed())
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        if (pauseMenuUI != null)
        {
            isPaused = false;
            Time.timeScale = 1f;
            pauseMenuUI.SetActive(false);
            
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.Play("ButtonPress");
            }
        }
    }

    public void Pause()
    {
        if (pauseMenuUI != null)
        {
            isPaused = true;
            Time.timeScale = 0f;
            pauseMenuUI.SetActive(true);
            
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.Play("ButtonPress");
            }
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
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
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play("ButtonPress");
        }
    }

    public void ExitToMainMenu()
    {
        Debug.Log("ExitToMainMenu çağrıldı");
        Time.timeScale = 1f;
        FadeController fade = FindObjectOfType<FadeController>();
        if (fade != null)
        {
            fade.gameObject.SetActive(true);
            fade.FadeOutAndLoadMainMenu();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play("ButtonPress");
        }
    }

    private void OnVolumeChanged(float value)
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetVolume(value);
            PlayerPrefs.SetFloat("MusicVolume", value);
            PlayerPrefs.Save();
        }
    }

    private void ToggleMute()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.ToggleMusic();
            UpdateMuteButtonText();
            
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.Play("ButtonPress");
            }
        }
    }

    private void UpdateMuteButtonText()
    {
        if (muteButton != null && MusicManager.Instance != null)
        {
            Text buttonText = muteButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = MusicManager.Instance.IsMuted() ? "Ses Aç" : "Ses Kapat";
            }
        }
    }
} 