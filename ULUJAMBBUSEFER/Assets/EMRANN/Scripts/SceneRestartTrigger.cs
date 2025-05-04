using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneRestartTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Karakter trigger'a değdi, sahne yeniden yükleniyor...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
} 