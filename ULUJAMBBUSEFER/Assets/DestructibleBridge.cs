using UnityEngine;

public class DestructibleBridge : MonoBehaviour
{
    public float destroyDelay = 1f; // Yok olma gecikmesi
    public float fadeOutDuration = 1f; // Solma süresi
    public bool destroyOnExit = true; // Çıkışta yok olma
    
    private bool hasBeenTriggered = false;
    private Material[] originalMaterials;
    private Renderer[] renderers;
    private Collider[] bridgeColliders;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        bridgeColliders = GetComponents<Collider>();
        
        // Orijinal materyalleri kaydet
        originalMaterials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenTriggered)
        {
            hasBeenTriggered = true;
            if (!destroyOnExit)
            {
                StartCoroutine(DestroyBridge());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && hasBeenTriggered && destroyOnExit)
        {
            StartCoroutine(DestroyBridge());
        }
    }

    private System.Collections.IEnumerator DestroyBridge()
    {
        // Yok olma gecikmesi
        yield return new WaitForSeconds(destroyDelay);

        // Tüm collider'ları devre dışı bırak
        foreach (Collider col in bridgeColliders)
        {
            col.enabled = false;
        }

        // Solma efekti
        float elapsedTime = 0f;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);

            for (int i = 0; i < renderers.Length; i++)
            {
                Color color = renderers[i].material.color;
                color.a = alpha;
                renderers[i].material.color = color;
            }

            yield return null;
        }

        // Köprüyü yok et
        Destroy(gameObject);
    }
} 