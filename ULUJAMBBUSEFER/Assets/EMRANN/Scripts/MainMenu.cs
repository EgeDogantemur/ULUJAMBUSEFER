using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Button startButton;
    public Button quitButton;
    public FadeController fadeController;

    private void Start()
    {
        // Butonları null kontrolü yaparak bağla
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);
        }
        else
        {
            Debug.LogError("Start Button bulunamadı!");
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
        }
        else
        {
            Debug.LogError("Quit Button bulunamadı!");
        }

        // Başlangıçta fade in
        if (fadeController != null)
        {
            fadeController.FadeIn();
        }
        else
        {
            Debug.LogError("FadeController bulunamadı!");
        }
    }

    public void StartGame()
    {
        Debug.Log("StartGame çağrıldı");
        if (fadeController != null)
        {
            fadeController.gameObject.SetActive(true);
            fadeController.FadeOutAndLoadGame();
        }
        else
        {
            SceneManager.LoadScene("Lab");
        }
    }

    public void QuitGame()
    {
        Debug.Log("QuitGame çağrıldı");
        #if UNITY_EDITOR
            Debug.Log("Unity Editor'dan çıkılıyor");
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Debug.Log("Uygulamadan çıkılıyor");
            Application.Quit();
        #endif
    }
} 