using UnityEngine;
using UnityEngine.Playables;

public class TimelineSceneController : MonoBehaviour
{
    public PlayableDirector timelineDirector;
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;

    void Start()
    {
        if (timelineDirector == null)
        {
            Debug.LogError("Timeline Director atanmamış!");
            return;
        }

        // Timeline'ı başlat
        timelineDirector.stopped += OnTimelineStopped;
        timelineDirector.Play();
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        Debug.Log("Timeline tamamlandı, Level1'e geçiliyor...");
        LabPuzzleController.OnTimelineCompleted();
    }

    void OnDestroy()
    {
        if (timelineDirector != null)
        {
            timelineDirector.stopped -= OnTimelineStopped;
        }
    }
} 