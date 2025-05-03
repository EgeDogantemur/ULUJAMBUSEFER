using UnityEngine;
using System.Collections;

public class SimpleDestructibleBridge : MonoBehaviour
{
    [Header("Ayarlar")]
    public float respawnDelay = 3f; // Yeniden oluşma süresi

    private Renderer rend;
    private Collider col;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(DestroyAndRespawn());
        }
    }

    private IEnumerator DestroyAndRespawn()
    {
        // Köprüyü görünmez ve etkisiz hale getir
        rend.enabled = false;
        col.enabled = false;

        // Belirtilen süre kadar bekle
        yield return new WaitForSeconds(respawnDelay);

        // Köprüyü tekrar görünür ve aktif hale getir
        rend.enabled = true;
        col.enabled = true;
    }
}
