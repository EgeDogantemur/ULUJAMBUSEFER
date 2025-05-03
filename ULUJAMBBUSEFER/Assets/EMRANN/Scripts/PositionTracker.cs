using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionTracker : MonoBehaviour
{
    private List<Vector3> recordedPositions = new List<Vector3>();
    private const float RECORD_INTERVAL = 0.1f;
    private const int MAX_POSITIONS = 100; // 10 saniye * 10 kayıt/saniye

    private void Start()
    {
        StartCoroutine(RecordPositionCoroutine());
    }

    private IEnumerator RecordPositionCoroutine()
    {
        while (true)
        {
            RecordPosition();
            yield return new WaitForSeconds(RECORD_INTERVAL);
        }
    }

    private void RecordPosition()
    {
        recordedPositions.Add(transform.position);
        
        // Eğer maksimum kayıt sayısını aştıysak, en eski kaydı sil
        if (recordedPositions.Count > MAX_POSITIONS)
        {
            recordedPositions.RemoveAt(0);
        }
    }

    public List<Vector3> GetRecordedPositions()
    {
        // Liste kopyasını döndür
        return new List<Vector3>(recordedPositions);
    }

    public void ResetPositions()
    {
        recordedPositions.Clear();
    }

    private void OnDrawGizmos()
    {
        if (recordedPositions.Count == 0) return;

        // En eski ve en yeni pozisyonlar için renkler
        Color startColor = new Color(1f, 0f, 0f, 0.3f); // Kırmızı (eski)
        Color endColor = new Color(0f, 1f, 0f, 1f);     // Yeşil (yeni)

        // Her pozisyon için gizmo çiz
        for (int i = 0; i < recordedPositions.Count; i++)
        {
            // Pozisyonun yaşına göre renk hesapla (0 = en eski, 1 = en yeni)
            float age = (float)i / (recordedPositions.Count - 1);
            Color positionColor = Color.Lerp(startColor, endColor, age);

            // Gizmo rengini ayarla
            Gizmos.color = positionColor;

            // Küçük bir küre çiz
            Gizmos.DrawSphere(recordedPositions[i], 0.2f);

            // Eğer son pozisyon değilse, bir sonraki pozisyonla çizgi çiz
            if (i < recordedPositions.Count - 1)
            {
                Gizmos.DrawLine(recordedPositions[i], recordedPositions[i + 1]);
            }
        }
    }
} 