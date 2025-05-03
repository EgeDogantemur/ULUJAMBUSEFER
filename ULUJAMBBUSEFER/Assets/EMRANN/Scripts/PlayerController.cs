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
    public Transform[] holdPoints = new Transform[4]; // 4 farklı tutma noktası
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;
    
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
    private bool isHoldingObject = false;
    
    // Input System referansı
    private InputSystemWrapper inputManager;

    // Animator referansı
    private Animator animator;
    private bool isJumping = false;

    // Hızlanma/yavaşlama için değişkenler
    public float acceleration = 8f;
    public float deceleration = 10f;
    public float maxRunSpeed = 1f; // Animator için max scale
    private float currentSpeed = 0f;

    void Start()
    {
        positionTracker = GetComponent<PositionTracker>();
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
        
        // Input Manager referansını al
        inputManager = InputSystemWrapper.Instance;
        if (inputManager == null)
        {
            Debug.LogError("InputSystemWrapper bulunamadı! Lütfen sahnede bir InputSystemWrapper olduğundan emin olun.");
        }

        // Animator referansını al
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("PlayerController: Animator bulunamadı!");
        }

        // Eğer holdPoints boşsa otomatik oluştur
        if (holdPoints[0] == null)
        {
            CreateHoldPoints();
        }
    }

    private void CreateHoldPoints()
    {
        // Ön, arka, sağ ve sol tutma noktaları
        Vector3[] positions = new Vector3[]
        {
            new Vector3(0, 0, 1f),  // Ön
            new Vector3(0, 0, -1f), // Arka
            new Vector3(1f, 0, 0),  // Sağ
            new Vector3(-1f, 0, 0)  // Sol
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject holdPointObj = new GameObject($"HoldPoint_{i}");
            holdPointObj.transform.SetParent(transform);
            holdPointObj.transform.localPosition = positions[i];
            holdPointObj.transform.localRotation = Quaternion.identity;
            holdPoints[i] = holdPointObj.transform;
        }
    }

    // En yakın tutma noktasını bul
    public Transform GetNearestHoldPoint(Vector3 objectPosition)
    {
        Transform nearestPoint = holdPoints[0];
        float minDistance = Vector3.Distance(objectPosition, holdPoints[0].position);

        for (int i = 1; i < holdPoints.Length; i++)
        {
            float distance = Vector3.Distance(objectPosition, holdPoints[i].position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPoint = holdPoints[i];
            }
        }

        return nearestPoint;
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

        // Input System ile karakter hareketi
        Vector3 movement = Vector3.zero;
        float targetSpeed = 0f;
        if (inputManager != null)
        {
            movement = inputManager.GetMovementInput();
            targetSpeed = new Vector3(movement.x, 0, movement.z).magnitude;
            transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);

            // Zıplama kontrolü - eğer obje tutuluyorsa zıplayamaz
            if (inputManager.GetJumpPressed() && IsGrounded() && !isHoldingObject)
            {
                Jump();
            }
            else if (inputManager.GetJumpPressed() && isHoldingObject)
            {
                Debug.Log("PlayerController: Zıplama tuşuna basıldı ama obje tutuluyor");
            }

            // Q tuşu ile klon oluşturma veya dönme
            if (inputManager.GetClonePressed())
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
        else
        {
            // Eski Input sistemi ile yedek kontroller
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            movement = new Vector3(horizontal, 0f, vertical);
            targetSpeed = movement.magnitude;
            transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);

            // Zıplama kontrolü - eğer obje tutuluyorsa zıplayamaz
            if (Input.GetKeyDown(KeyCode.Space) && IsGrounded() && !isHoldingObject)
            {
                Jump();
            }
            else if (Input.GetKeyDown(KeyCode.Space) && isHoldingObject)
            {
                Debug.Log("PlayerController: Zıplama tuşuna basıldı ama obje tutuluyor");
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

        // Hızlanma/yavaşlama (smooth)
        if (targetSpeed > currentSpeed)
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        else
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, deceleration * Time.deltaTime);

        // ANİMASYON: Koşma/Idle
        if (animator != null)
        {
            animator.SetFloat("Speed", currentSpeed / maxRunSpeed); // 0-1 arası scale
            animator.SetBool("IsGrounded", IsGrounded());
        }

        // HAREKET YÖNÜNE DÖNME
        Vector3 lookDir = new Vector3(movement.x, 0, movement.z);
        if (lookDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    private void Jump()
    {
        if (!isHoldingObject)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            isJumping = true;
            Debug.Log("PlayerController: Zıplama gerçekleşti");
            
            // Zıplama sesi çal
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.Play("Jump");
            }
            // ANİMASYON: Zıplama
            if (animator != null)
            {
                animator.SetTrigger("Trigger");
                animator.SetBool("Jump", true);
                animator.SetBool("IsGround", false);
            }
        }
        else
        {
            Debug.Log("PlayerController: Obje tutuluyor, zıplama engellendi");
        }
    }

    private bool IsGrounded()
    {
        // Karakterin altından bir ışın gönder
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f; // Biraz yukarıdan başlat
        bool grounded = Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance + 0.1f, groundLayer);
        
        // Debug için ışını görselleştir
        Debug.DrawRay(rayStart, Vector3.down * (groundCheckDistance + 0.1f), grounded ? Color.green : Color.red);
        
        // ANİMASYON: Yere inince zıplama animasyonunu kapat
        if (animator != null && grounded && isJumping)
        {
            animator.SetBool("Jump", false);
            animator.SetBool("IsGround", true);
            isJumping = false;
        }
        return grounded;
    }

    private void CreateCloneMarker()
    {
        if (CloneMarkerPrefab != null && !hasActiveClone)
        {
            // Zaman kontrolü
            if (TimerManager.Instance != null && TimerManager.Instance.CanCreateClone())
            {
                TimerManager.Instance.DeductTimeForClone();

                // Önceki konum geçmişini sıfırla
                positionTracker.ResetPositions();
                
                GameObject marker = Instantiate(CloneMarkerPrefab, transform.position, Quaternion.identity).gameObject;
                clonePositions.Add((transform.position, Time.time));
                activeMarkers.Add(marker);
                hasActiveClone = true;
                cloneCreationTime = Time.time;
                
                // Klon oluşturma sesi çal
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.Play("CloneCreate");
                }
            }
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
            
            // Geri sarma sesi çal
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.Play("CloneRewind");
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

    // Obje tutma durumunu güncelle
    public void SetHoldingObject(bool holding)
    {
        isHoldingObject = holding;
        Debug.Log($"PlayerController: isHoldingObject = {isHoldingObject}");
    }

    // ANİMASYON EVENTLERİ
    public void FootL()
    {
        // Adım sesi veya efekt eklemek için buraya kod ekleyebilirsin
        // if (SoundManager.Instance != null) SoundManager.Instance.Play("Footstep");
    }

    public void FootR()
    {
        // Adım sesi veya efekt eklemek için buraya kod ekleyebilirsin
        // if (SoundManager.Instance != null) SoundManager.Instance.Play("Footstep");
    }
}