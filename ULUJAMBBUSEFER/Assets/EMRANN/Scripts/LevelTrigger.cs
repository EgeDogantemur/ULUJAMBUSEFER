using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Sadece Player tag'ine sahip objeler için çalış
        if (other.CompareTag("Player"))
        {
            // Mevcut sahnenin index'ini al
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            
            // Bir sonraki sahneyi yükle
            int nextSceneIndex = currentSceneIndex + 1;
            
            // Eğer sonraki sahne varsa yükle
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                FadeController fade = FindObjectOfType<FadeController>();
                if (fade != null)
                {
                    fade.gameObject.SetActive(true);
                    fade.FadeOutAndLoadNextLevel();
                }
                else
                {
                    SceneManager.LoadScene(nextSceneIndex);
                }
            }
            else
            {
                Debug.Log("Bu son level!");
                // Oyunu bitir veya ana menüye dön
            }
        }
    }
} 