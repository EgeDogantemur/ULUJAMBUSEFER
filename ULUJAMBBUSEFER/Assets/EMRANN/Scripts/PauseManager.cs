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

        SetupUIReferences();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            // Referansları sıfırla
            pauseMenuUI = null;
            volumeSlider = null;
            muteButton = null;
            resumeButton = null;
            restartButton = null;
            exitButton = null;

            // Yeni referansları bul
            SetupUIReferences();
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
            }
        }

        // Butonları ve slider'ı bul
        if (pauseMenuUI != null)
        {
            // Resume Button
            if (resumeButton == null)
            {
                Transform resumeButtonTransform = pauseMenuUI.transform.Find("ResumeButton");
                if (resumeButtonTransform != null)
                {
                    resumeButton = resumeButtonTransform.GetComponent<Button>();
                    Debug.Log("ResumeButton bulundu: " + resumeButton.name);
                }
                else
                {
                    Debug.LogError("ResumeButton bulunamadı!");
                }
            }

            // Restart Button
            if (restartButton == null)
            {
                Transform restartButtonTransform = pauseMenuUI.transform.Find("RestartButton");
                if (restartButtonTransform != null)
                {
                    restartButton = restartButtonTransform.GetComponent<Button>();
                    Debug.Log("RestartButton bulundu: " + restartButton.name);
                }
                else
                {
                    Debug.LogError("RestartButton bulunamadı!");
                }
            }

            // Exit Button
            if (exitButton == null)
            {
                Transform exitButtonTransform = pauseMenuUI.transform.Find("ExitButton");
                if (exitButtonTransform != null)
                {
                    exitButton = exitButtonTransform.GetComponent<Button>();
                    Debug.Log("ExitButton bulundu: " + exitButton.name);
                }
                else
                {
                    Debug.LogError("ExitButton bulunamadı!");
                }
            }

            // Volume Slider
            if (volumeSlider == null)
            {
                Transform volumeSliderTransform = pauseMenuUI.transform.Find("VolumeSlider");
                if (volumeSliderTransform != null)
                {
                    volumeSlider = volumeSliderTransform.GetComponent<Slider>();
                    Debug.Log("VolumeSlider bulundu: " + volumeSlider.name);
                }
                else
                {
                    Debug.LogError("VolumeSlider bulunamadı!");
                }
            }

            // Mute Button
            if (muteButton == null)
            {
                Transform muteButtonTransform = pauseMenuUI.transform.Find("MuteButton");
                if (muteButtonTransform != null)
                {
                    muteButton = muteButtonTransform.GetComponent<Button>();
                    Debug.Log("MuteButton bulundu: " + muteButton.name);
                }
                else
                {
                    Debug.LogError("MuteButton bulunamadı!");
                }
            }
        }

        // Butonları bağla
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(Resume);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(Restart);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(ExitToMainMenu);
        }

        // Ses ayarlarını yükle
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (muteButton != null)
        {
            UpdateMuteButtonText();
            muteButton.onClick.RemoveAllListeners();
            muteButton.onClick.AddListener(ToggleMute);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
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