using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1f;
    public bool startWithFadeIn = true;

    private bool isFading = false;
    private float currentAlpha = 0f;

    private void Awake()
    {
        if (fadeImage == null)
        {
            fadeImage = GetComponent<Image>();
            if (fadeImage == null)
            {
                Debug.LogError("FadeController: FadeImage bulunamadı!");
                return;
            }
        }

        Debug.Log("FadeController başlatıldı. FadeImage: " + (fadeImage != null));
        
        // Başlangıçta fade in
        if (startWithFadeIn)
        {
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
            currentAlpha = 1f;
            FadeIn();
        }
    }

    public bool IsFading()
    {
        return isFading;
    }

    public float GetCurrentAlpha()
    {
        return currentAlpha;
    }

    public void FadeOutAndRestart()
    {
        if (!isFading)
        {
            StartCoroutine(FadeOutCoroutine(true));
        }
    }

    public void FadeOutAndLoadGame()
    {
        if (!isFading)
        {
            StartCoroutine(FadeOutCoroutine(false));
        }
    }

    public void FadeOutAndLoadMainMenu()
    {
        if (!isFading)
        {
            StartCoroutine(FadeOutCoroutine(false, "MainMenu"));
        }
    }

    public void FadeOutAndLoadNextLevel()
    {
        if (!isFading)
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                StartCoroutine(FadeOutCoroutine(false, nextSceneIndex));
            }
            else
            {
                Debug.Log("Bu son level!");
                StartCoroutine(FadeOutCoroutine(false, "MainMenu"));
            }
        }
    }

    IEnumerator FadeOutCoroutine(bool restart, string sceneName = "Lab")
    {
        Debug.Log("FadeOut başladı");
        isFading = true;
        float t = 0;
        Color c = fadeImage.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, t / fadeDuration);
            currentAlpha = c.a;
            fadeImage.color = c;
            yield return null;
        }
        c.a = 1f;
        currentAlpha = 1f;
        fadeImage.color = c;
        isFading = false;

        Debug.Log("FadeOut tamamlandı, sahne yükleniyor");
        if (restart)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        else
            SceneManager.LoadScene(sceneName);
    }

    IEnumerator FadeOutCoroutine(bool restart, int sceneIndex)
    {
        isFading = true;
        float t = 0;
        Color c = fadeImage.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, t / fadeDuration);
            currentAlpha = c.a;
            fadeImage.color = c;
            yield return null;
        }
        c.a = 1f;
        currentAlpha = 1f;
        fadeImage.color = c;
        isFading = false;

        SceneManager.LoadScene(sceneIndex);
    }

    public void FadeIn()
    {
        if (!isFading)
        {
            StartCoroutine(FadeInCoroutine());
        }
    }

    IEnumerator FadeInCoroutine()
    {
        isFading = true;
        float t = 0;
        Color c = fadeImage.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1, 0, t / fadeDuration);
            currentAlpha = c.a;
            fadeImage.color = c;
            yield return null;
        }
        c.a = 0f;
        currentAlpha = 0f;
        fadeImage.color = c;
        isFading = false;
        
        // Fade tamamlandıktan sonra görünmez yap
        fadeImage.gameObject.SetActive(false);
        Debug.Log("FadeIn tamamlandı");
    }
} 