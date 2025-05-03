using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform CloneMarkerPrefab;
    public float rewindSpeed = 10f;
    public float jumpForce = 5f;
    public float cloneLifetime = 5f; // Klonun yaşam süresi
    public float rewindWindow = 3f; // Geri dönme penceresi
    
    private List<(Vector3 position, float timestamp)> clonePositions = new List<(Vector3, float)>();
    private List<GameObject> activeMarkers = new List<GameObject>();
    private bool isRewinding = false;
    private PositionTracker positionTracker;
    private bool hasActiveClone = false;
    private Rigidbody rb;
    private bool isGrounded;
    private Collider playerCollider;
    private bool wasKinematic;
    private bool useGravity;
    private float cloneCreationTime;

    void Start()
    {
        positionTracker = GetComponent<PositionTracker>();
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
    }

    void Update()
    {
        if (isRewinding) return;

        // Klon süresini kontrol et
        if (hasActiveClone && Time.time - cloneCreationTime > cloneLifetime)
        {
            // Klon süresi doldu
            foreach (GameObject marker in activeMarkers)
            {
                if (marker != null)
                {
                    Destroy(marker);
                }
            }
            activeMarkers.Clear();
            clonePositions.Clear();
            hasActiveClone = false;
        }

        // Karakter hareketi
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 movement = new Vector3(horizontal, 0f, vertical);
        transform.Translate(movement * moveSpeed * Time.deltaTime);

        // Zıplama kontrolü
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        // Q tuşu ile klon oluşturma veya dönme
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!hasActiveClone)
            {
                CreateCloneMarker();
            }
            else if (Time.time - cloneCreationTime <= rewindWindow)
            {
                // Sadece geri dönme penceresi içindeyse geri dönebilir
                StartRewind();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Yere değdiğimizi kontrol et
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // Yerden ayrıldığımızı kontrol et
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    private void CreateCloneMarker()
    {
        if (CloneMarkerPrefab != null && !hasActiveClone)
        {
            // Önceki konum geçmişini sıfırla
            positionTracker.ResetPositions();
            
            GameObject marker = Instantiate(CloneMarkerPrefab, transform.position, Quaternion.identity).gameObject;
            clonePositions.Add((transform.position, Time.time));
            activeMarkers.Add(marker);
            hasActiveClone = true;
            cloneCreationTime = Time.time;
        }
    }

    private void StartRewind()
    {
        if (!isRewinding && hasActiveClone)
        {
            // Tüm aktif işaretleyicileri yok et
            foreach (GameObject marker in activeMarkers)
            {
                if (marker != null)
                {
                    Destroy(marker);
                }
            }
            activeMarkers.Clear();
            
            // Rigidbody ve Collider'ı devre dışı bırak
            if (rb != null)
            {
                wasKinematic = rb.isKinematic;
                useGravity = rb.useGravity;
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            if (playerCollider != null)
            {
                playerCollider.enabled = false;
            }
            
            StartCoroutine(RewindCoroutine());
        }
    }

    private IEnumerator RewindCoroutine()
    {
        isRewinding = true;
        List<Vector3> positions = positionTracker.GetRecordedPositions();
        
        // Pozisyonları ters çevir (en yeniden en eskiye)
        positions.Reverse();

        foreach (Vector3 targetPosition in positions)
        {
            // Hedef pozisyona doğru hareket et
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    rewindSpeed * Time.deltaTime
                );
                yield return null;
            }

            // Klon pozisyonlarından birine ulaşıldı mı kontrol et
            foreach (var clonePos in clonePositions)
            {
                if (Vector3.Distance(transform.position, clonePos.position) < 0.1f)
                {
                    // Rigidbody ve Collider'ı tekrar aktif et
                    if (rb != null)
                    {
                        rb.isKinematic = wasKinematic;
                        rb.useGravity = useGravity;
                    }
                    
                    if (playerCollider != null)
                    {
                        playerCollider.enabled = true;
                    }
                    
                    isRewinding = false;
                    hasActiveClone = false;
                    clonePositions.Clear();
                    yield break;
                }
            }
        }

        // Eğer klona ulaşılamadıysa, Rigidbody ve Collider'ı tekrar aktif et
        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
            rb.useGravity = useGravity;
        }
        
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }
        
        isRewinding = false;
        hasActiveClone = false;
        clonePositions.Clear();
    }

    public List<(Vector3 position, float timestamp)> GetClonePositions()
    {
        return new List<(Vector3, float)>(clonePositions);
    }
}