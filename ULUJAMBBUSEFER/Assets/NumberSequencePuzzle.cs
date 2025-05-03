using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class NumberSequencePuzzle : MonoBehaviour
{
    public GameObject puzzleUI; // UI panel
    public Button[] numberButtons; // 1-10 arasý butonlar
    public int[] correctSequence = { 1, 2, 3, 4, 5, 6 }; // Doðru sýra
    public PlayableDirector sequencer; // Çalýþtýrýlacak timeline
    public Transform player;
    public Transform interactionTarget; // UI’yi baþlatacak nesne
    public float interactionDistance = 3f;

    private int currentStep = 0;
    private bool puzzleActive = false;

    void Start()
    {
        puzzleUI.SetActive(false);

        foreach (Button btn in numberButtons)
        {
            btn.onClick.AddListener(() => OnButtonClicked(btn));
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            float distance = Vector3.Distance(player.position, interactionTarget.position);
            if (distance <= interactionDistance)
            {
                puzzleUI.SetActive(true);
                puzzleActive = true;
                ResetPuzzle();
            }
        }
    }

    void OnButtonClicked(Button btn)
    {
        if (!puzzleActive) return;

        int number = int.Parse(btn.GetComponentInChildren<Text>().text);

        if (number == correctSequence[currentStep])
        {
            currentStep++;
            if (currentStep == correctSequence.Length)
            {
                Debug.Log("Correct Sequence!");
                sequencer.Play();
                puzzleUI.SetActive(false);
                puzzleActive = false;
            }
        }
        else
        {
            Debug.Log("Wrong Sequence. Resetting...");
            ResetPuzzle();
        }
    }

    void ResetPuzzle()
    {
        currentStep = 0;
    }
}
