using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform CloneMarkerPrefab;
    public float rewindSpeed = 10f;
    public float jumpForce = 5f;
    public float cloneLifetime = 5f;
    public float rewindWindow = 3f;
    public Transform[] holdPoints = new Transform[4];
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;
    public float acceleration = 8f;
    public float deceleration = 10f;
    public float maxRunSpeed = 1f;

    [Header("Tutma Sistemi")]
    public Transform grabPoint;
    public float grabRange = 2f;
    public LayerMask pushableLayer;
    private PushableObject heldObject;

    [Header("Ses Ayarları")]
    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;
    public AudioClip CloneCreateSound;
    public AudioClip RewindSound;
    [Range(0, 1)] public float CloneSoundVolume = 0.5f;

    private List<(Vector3 position, Quaternion rotation, float timestamp)> clonePositions = new List<(Vector3, Quaternion, float)>();
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
    private float currentSpeed = 0f;
    private bool isJumping = false;
    private InputSystemWrapper inputManager;
    private Animator animator;

    // Animasyon ID'leri
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDMotionSpeed;
    private float _animationBlend;
    private bool _hasAnimator;

    private AudioSource cloneAudioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
        positionTracker = GetComponent<PositionTracker>();
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("Animator bileşeni bulunamadı! Lütfen karakterinize bir Animator ekleyin.");
        }
        else
        {
            _hasAnimator = true;
            AssignAnimationIDs();
        }

        inputManager = InputSystemWrapper.Instance;
        if (inputManager == null)
        {
            Debug.LogError("InputSystemWrapper bulunamadı!");
        }

        // Tutma noktalarını kontrol et ve oluştur
        if (holdPoints != null && holdPoints.Length > 0)
        {
            if (holdPoints[0] == null)
            {
                CreateHoldPoints();
            }
        }
        else
        {
            holdPoints = new Transform[4];
            CreateHoldPoints();
        }

        if (grabPoint == null)
        {
            Debug.LogError("GrabPoint referansı eksik! Lütfen Inspector'dan atayın.");
        }

        // Ses kaynağı oluştur
        cloneAudioSource = gameObject.AddComponent<AudioSource>();
        cloneAudioSource.playOnAwake = false;
        cloneAudioSource.volume = CloneSoundVolume;
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void CreateHoldPoints()
    {
        // Tutma noktaları sadece X ve Z ekseninde
        Vector3[] positions = new Vector3[]
        {
            new Vector3(0, 0, 1f),    // Ön
            new Vector3(0, 0, -1f),   // Arka
            new Vector3(1f, 0, 0),    // Sağ
            new Vector3(-1f, 0, 0)    // Sol
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
        if (holdPoints == null || holdPoints.Length == 0)
        {
            Debug.LogWarning("Hold points dizisi boş!");
            return null;
        }

        Transform nearestPoint = holdPoints[0];
        float minDistance = float.MaxValue;
        Vector3 playerForward = transform.forward;

        foreach (Transform point in holdPoints)
        {
            if (point == null) continue;
            
            // Sadece X ve Z eksenindeki mesafeyi hesapla
            Vector3 pointPos = new Vector3(point.position.x, 0, point.position.z);
            Vector3 objPos = new Vector3(objectPosition.x, 0, objectPosition.z);
            float distance = Vector3.Distance(objPos, pointPos);
            
            Vector3 directionToPoint = (pointPos - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
            float dotProduct = Vector3.Dot(playerForward, directionToPoint);
            
            // Oyuncunun baktığı yöne yakın noktaları tercih et
            if (dotProduct > 0.5f && distance < minDistance)
            {
                minDistance = distance;
                nearestPoint = point;
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
            DestroyClone();
        }

        HandleMovement();
        HandleCloneAndRewind();
        HandleGrab();
    }

    private void HandleMovement()
    {
        if (inputManager == null) return;

        Vector3 movement = inputManager.GetMovementInput();
        Debug.Log($"Ham movement değeri: {movement}");

        // Hareket yönüne yumuşak dönüş
        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Yumuşak hızlanma/yavaşlama
        float targetSpeed = movement.magnitude;
        Debug.Log($"Target speed: {targetSpeed}");

        if (targetSpeed > currentSpeed)
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        else
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, deceleration * Time.deltaTime);

        // Yumuşak hareket
        Vector3 moveDirection = transform.forward * currentSpeed;
        Vector3 newPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;
        transform.position = newPosition;

        // Animasyon kontrolü
        if (_hasAnimator)
        {
            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * acceleration);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            animator.SetFloat(_animIDSpeed, _animationBlend);
            animator.SetFloat(_animIDMotionSpeed, 1f);
            animator.SetBool(_animIDGrounded, IsGrounded());
        }

        // Zıplama
        if (inputManager.GetJumpPressed() && IsGrounded() && !isHoldingObject)
        {
            Jump();
        }
    }

    private void HandleCloneAndRewind()
    {
        if (inputManager.GetClonePressed())
        {
            Debug.Log("Q tuşuna basıldı");
            
            if (!hasActiveClone)
            {
                Debug.Log("Klon oluşturulmaya çalışılıyor...");
                if (CloneMarkerPrefab == null)
                {
                    Debug.LogError("Clone Prefab referansı eksik! Lütfen Inspector'dan atayın.");
                    return;
                }
                CreateCloneMarker();
            }
            else if (Time.time - cloneCreationTime <= rewindWindow)
            {
                Debug.Log("Geri sarma başlatılıyor...");
                StartRewind();
            }
        }
    }

    private void HandleGrab()
    {
        if (inputManager == null) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isHoldingObject)
            {
                TryGrabObject();
            }
            else
            {
                ReleaseObject();
            }
        }
    }

    private void TryGrabObject()
    {
        Collider[] colliders = Physics.OverlapSphere(grabPoint.position, grabRange, pushableLayer);
        
        foreach (Collider col in colliders)
        {
            PushableObject pushable = col.GetComponent<PushableObject>();
            if (pushable != null)
            {
                pushable.HoldObject(grabPoint);
                heldObject = pushable;
                isHoldingObject = true;
                break;
            }
        }
    }

    private void ReleaseObject()
    {
        if (heldObject != null)
        {
            heldObject.ReleaseObject();
            heldObject = null;
            isHoldingObject = false;
        }
    }

    private void Jump()
    {
        if (rb != null && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJumping = true;
            
            if (_hasAnimator)
            {
                animator.SetBool(_animIDJump, true);
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.Play("Jump");
            }
        }
    }

    private bool IsGrounded()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        bool grounded = Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance + 0.1f, groundLayer);
        
        if (grounded && isJumping)
        {
            isJumping = false;
            if (_hasAnimator)
            {
                animator.SetBool(_animIDJump, false);
            }
        }
        
        return grounded;
    }

    private void CreateCloneMarker()
    {
        if (CloneMarkerPrefab == null)
        {
            Debug.LogError("Clone Prefab bulunamadı!");
            return;
        }

        if (!TimerManager.Instance)
        {
            Debug.LogError("TimerManager bulunamadı!");
            return;
        }

        if (!TimerManager.Instance.CanCreateClone())
        {
            Debug.Log("Yeterli süre yok!");
            return;
        }

        Debug.Log("Klon oluşturuluyor...");
        TimerManager.Instance.DeductTimeForClone();

        // Klon oluşturma sesi
        if (cloneAudioSource != null && CloneCreateSound != null)
        {
            cloneAudioSource.clip = CloneCreateSound;
            cloneAudioSource.Play();
        }

        // Önceki pozisyon geçmişini sıfırla
        if (positionTracker != null)
        {
            positionTracker.ResetPositions();
            positionTracker.StartRecording();
        }

        try
        {
            // Marker oluştur
            GameObject marker = Instantiate(CloneMarkerPrefab.gameObject, transform.position, transform.rotation);
            if (marker != null)
            {
                Debug.Log("Klon başarıyla oluşturuldu!");
                activeMarkers.Add(marker);
                hasActiveClone = true;
                cloneCreationTime = Time.time;

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.Play("CloneCreate");
                }
            }
            else
            {
                Debug.LogError("Klon oluşturulamadı!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Klon oluşturulurken hata: {e.Message}");
        }
    }

    private void StartRewind()
    {
        if (!hasActiveClone || isRewinding) return;

        // Rewind sesi
        if (cloneAudioSource != null && RewindSound != null)
        {
            cloneAudioSource.clip = RewindSound;
            cloneAudioSource.loop = true;
            cloneAudioSource.Play();
        }

        // Fizik özelliklerini kaydet ve devre dışı bırak
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

        if (positionTracker != null)
        {
            positionTracker.StopRecording();
        }

        isRewinding = true;
        StartCoroutine(RewindCoroutine());

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play("CloneRewind");
        }
    }

    private IEnumerator RewindCoroutine()
    {
        if (positionTracker == null)
        {
            ResetRewind();
            yield break;
        }

        var positions = positionTracker.GetRecordedPositions();
        positions.Reverse(); // En son pozisyondan başlayarak geri git

        foreach (var posData in positions)
        {
            float startTime = Time.time;
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            Vector3 targetPos = posData.position;
            Quaternion targetRot = posData.rotation;

            float journeyLength = Vector3.Distance(startPos, targetPos);
            float distanceCovered = 0;

            while (distanceCovered < journeyLength)
            {
                float fractionOfJourney = distanceCovered / journeyLength;
                transform.position = Vector3.Lerp(startPos, targetPos, fractionOfJourney);
                transform.rotation = Quaternion.Lerp(startRot, targetRot, fractionOfJourney);

                distanceCovered = (Time.time - startTime) * rewindSpeed;
                yield return null;
            }

            // Son pozisyonu tam olarak ayarla
            transform.position = targetPos;
            transform.rotation = targetRot;
        }

        ResetRewind();
    }

    private void ResetRewind()
    {
        // Rewind sesini durdur
        if (cloneAudioSource != null)
        {
            cloneAudioSource.Stop();
            cloneAudioSource.loop = false;
        }

        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
            rb.useGravity = useGravity;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        isRewinding = false;
        DestroyClone();
    }

    private void DestroyClone()
    {
        foreach (GameObject marker in activeMarkers)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }
        
        activeMarkers.Clear();
        hasActiveClone = false;

        if (positionTracker != null)
        {
            positionTracker.ResetPositions();
        }
    }

    public void SetHoldingObject(bool holding)
    {
        isHoldingObject = holding;
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(playerCollider.bounds.center), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(playerCollider.bounds.center), FootstepAudioVolume);
        }
    }
}