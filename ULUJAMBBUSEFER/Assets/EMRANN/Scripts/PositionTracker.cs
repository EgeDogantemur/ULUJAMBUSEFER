using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionTracker : MonoBehaviour
{
    private List<(Vector3 position, Quaternion rotation)> recordedPositions = new List<(Vector3, Quaternion)>();
    private const float RECORD_INTERVAL = 0.05f; // Daha sık kayıt
    private const int MAX_POSITIONS = 200; // Daha fazla pozisyon kaydı
    private bool isRecording = false;
    private Coroutine recordingCoroutine;

    private void Start()
    {
        StartRecording();
    }

    public void StartRecording()
    {
        if (!isRecording)
        {
            isRecording = true;
            if (recordingCoroutine != null)
            {
                StopCoroutine(recordingCoroutine);
            }
            recordingCoroutine = StartCoroutine(RecordPositionCoroutine());
            Debug.Log("Pozisyon kaydı başlatıldı");
        }
    }

    public void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            if (recordingCoroutine != null)
            {
                StopCoroutine(recordingCoroutine);
                recordingCoroutine = null;
            }
            Debug.Log("Pozisyon kaydı durduruldu");
        }
    }

    private IEnumerator RecordPositionCoroutine()
    {
        while (isRecording)
        {
            RecordPosition();
            yield return new WaitForSeconds(RECORD_INTERVAL);
        }
    }

    private void RecordPosition()
    {
        if (!isRecording) return;

        recordedPositions.Add((transform.position, transform.rotation));
        
        if (recordedPositions.Count > MAX_POSITIONS)
        {
            recordedPositions.RemoveAt(0);
        }
    }

    public List<(Vector3 position, Quaternion rotation)> GetRecordedPositions()
    {
        return new List<(Vector3, Quaternion)>(recordedPositions);
    }

    public void ResetPositions()
    {
        recordedPositions.Clear();
        Debug.Log("Pozisyon kaydı sıfırlandı");
    }

    private void OnDrawGizmos()
    {
        if (recordedPositions.Count == 0) return;

        Color startColor = new Color(1f, 0f, 0f, 0.3f);
        Color endColor = new Color(0f, 1f, 0f, 1f);

        for (int i = 0; i < recordedPositions.Count; i++)
        {
            float age = (float)i / (recordedPositions.Count - 1);
            Color positionColor = Color.Lerp(startColor, endColor, age);
            Gizmos.color = positionColor;

            // Pozisyon ve rotasyonu göster
            Vector3 position = recordedPositions[i].position;
            Quaternion rotation = recordedPositions[i].rotation;
            
            // Küçük bir küp çiz
            Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one * 0.2f);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

            // Pozisyonlar arası çizgi çiz
            if (i < recordedPositions.Count - 1)
            {
                Gizmos.DrawLine(position, recordedPositions[i + 1].position);
            }
        }
        
        // Gizmos matrisini sıfırla
        Gizmos.matrix = Matrix4x4.identity;
    }

    private void OnDisable()
    {
        StopRecording();
    }
} 