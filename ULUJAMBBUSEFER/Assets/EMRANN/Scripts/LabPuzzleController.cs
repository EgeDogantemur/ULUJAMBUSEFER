using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public class LabPuzzleController : MonoBehaviour
{
    [Header("UI Ayarları")]
    public GameObject puzzleUI;
    public Button[] buttons;
    public float buttonPressDelay = 0.5f;

    [Header("Sahne Geçiş Ayarları")]
    public string timelineSceneName = "TimelineScene"; // Timeline'ın olduğu sahne adı
    public string nextLevelName = "Level1"; // Sonraki level adı
    public float levelLoadDelay = 6f; // Timeline bittikten sonra level geçişi için bekleme süresi

    [Header("Etkileşim Ayarları")]
    public float interactionDistance = 3f;
    public LayerMask interactionLayer;

    private int currentButtonIndex = 0;
    private bool isPuzzleActive = false;
    private bool isPuzzleCompleted = false;
    private bool isPlayerInRange = false;

    void Start()
    {
        if (interactionLayer == 0)
        {
            Debug.LogWarning("Interaction Layer ayarlanmamış!");
        }

        if (puzzleUI != null)
        {
            puzzleUI.SetActive(false);
        }
        else
        {
            Debug.LogError("Puzzle UI atanmamış!");
        }

        if (buttons == null || buttons.Length == 0)
        {
            Debug.LogError("Butonlar atanmamış!");
        }
        else
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null)
                {
                    Debug.LogError($"Buton {i} atanmamış!");
                    continue;
                }
                int buttonIndex = i;
                buttons[i].onClick.AddListener(() => OnButtonClick(buttonIndex));
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && isPlayerInRange && !isPuzzleActive && !isPuzzleCompleted)
        {
            StartPuzzle();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            
            if (puzzleUI != null && puzzleUI.activeSelf)
            {
                puzzleUI.SetActive(false);
                isPuzzleActive = false;
            }
        }
    }

    private void StartPuzzle()
    {
        isPuzzleActive = true;
        currentButtonIndex = 0;
        
        if (puzzleUI != null)
        {
            puzzleUI.SetActive(true);
        }
    }

    private void OnButtonClick(int buttonIndex)
    {
        if (!isPuzzleActive || isPuzzleCompleted) return;

        if (buttonIndex == currentButtonIndex)
        {
            StartCoroutine(ButtonPressEffect(buttons[buttonIndex]));
            currentButtonIndex++;

            if (currentButtonIndex >= buttons.Length)
            {
                CompletePuzzle();
            }
        }
        else
        {
            ResetPuzzle();
        }
    }

    private IEnumerator ButtonPressEffect(Button button)
    {
        Color originalColor = button.image.color;
        button.image.color = Color.green;
        
        yield return new WaitForSeconds(buttonPressDelay);
        
        button.image.color = originalColor;
    }

    private void ResetPuzzle()
    {
        currentButtonIndex = 0;
        
        foreach (Button button in buttons)
        {
            button.image.color = Color.white;
        }
    }

    private void CompletePuzzle()
    {
        isPuzzleCompleted = true;
        isPuzzleActive = false;

        if (puzzleUI != null)
        {
            puzzleUI.SetActive(false);
        }

        Debug.Log("Puzzle tamamlandı, Timeline sahnesine geçiliyor...");
        SceneManager.LoadScene("Level1");
    }

    // Timeline sahnesinde çağrılacak fonksiyon
    public static void OnTimelineCompleted()
    {
        Debug.Log("Timeline tamamlandı, Level1'e geçiliyor...");
        SceneManager.LoadScene("Level1");
    }

    void OnDrawGizmos()
    {
        // Scene görünümünde raycast'i görselleştir
        if (Camera.main != null)
        {
            Gizmos.color = Color.yellow;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Gizmos.DrawRay(ray.origin, ray.direction * interactionDistance);
        }
    }
} 