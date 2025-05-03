using UnityEngine;
using System.Collections;

public class PushableObject : MonoBehaviour
{
    [Header("İtme Ayarları")]
    [SerializeField] private float pushForce = 5f;

    [Header("Tutma Ayarları")]
    [SerializeField] private float holdDistance = 1f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float interactionCooldown = 0.5f;
    [SerializeField] private float grabStability = 5f;

    private bool isBeingHeld = false;
    private Transform playerTransform;
    private Transform holdPoint;
    private Rigidbody rb;
    private bool canInteract = true;
    private bool isPlayerInTrigger = false;
    private PlayerController playerController;
    private bool wasKinematic;
    private float holdStartTime;
    private InputSystemWrapper inputManager;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        wasKinematic = rb.isKinematic;
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
        
        inputManager = InputSystemWrapper.Instance;
        if (inputManager == null)
        {
            Debug.LogError("InputSystemWrapper bulunamadı!");
        }
    }

    private void Update()
    {
        if (inputManager == null) return;

        // E tuşuna basıldığında tutma/bırakma
        if (Input.GetKeyDown(KeyCode.E) && isPlayerInTrigger && canInteract)
        {
            if (!isBeingHeld)
            {
                HoldObject(playerTransform);
            }
            else
            {
                ReleaseObject();
                StartCoroutine(InteractionCooldown());
            }
        }
    }

    private void FixedUpdate()
    {
        if (isBeingHeld && holdPoint != null)
        {
            // Tutma noktasına doğru hareket
            Vector3 targetPosition = holdPoint.position;
            float holdTime = Time.time - holdStartTime;
            float strength = Mathf.Min(1.0f, holdTime * grabStability);
            
            // Sadece X ve Z eksenlerinde hareket
            Vector3 currentPos = transform.position;
            Vector3 targetPos = new Vector3(targetPosition.x, currentPos.y, targetPosition.z);
            
            rb.MovePosition(Vector3.Lerp(currentPos, targetPos, strength * Time.fixedDeltaTime * smoothSpeed));
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, holdPoint.rotation, strength * Time.fixedDeltaTime * smoothSpeed));
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isBeingHeld)
        {
            // İtme mantığı
            Vector3 moveInput = Vector3.zero;

            if (inputManager != null)
            {
                moveInput = inputManager.GetMovementInput();
            }
            else
            {
                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");
                moveInput = new Vector3(horizontal, 0, vertical);
            }

            if (moveInput.magnitude > 0)
            {
                Vector3 pushDirection = new Vector3(moveInput.x, 0, moveInput.z).normalized;
                rb.AddForce(pushDirection * pushForce, ForceMode.Force);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            playerTransform = other.transform;
            playerController = other.GetComponent<PlayerController>();
            Debug.Log("Kutuyu tutmak için E tuşuna basın");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isBeingHeld)
            {
                ReleaseObject();
            }
            
            isPlayerInTrigger = false;
            playerTransform = null;
            playerController = null;
        }
    }

    private void HoldObject(Transform player)
    {
        if (!canInteract) return;

        isBeingHeld = true;
        playerTransform = player;
        holdStartTime = Time.time;
        
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            if (this.playerController != null && this.playerController != playerController)
            {
                this.playerController.SetHoldingObject(false);
            }
            
            this.playerController = playerController;
            holdPoint = playerController.GetNearestHoldPoint(transform.position);
            playerController.SetHoldingObject(true);
        }
        
        wasKinematic = rb.isKinematic;
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        foreach (Collider col in GetComponents<Collider>())
        {
            if (!col.isTrigger)
            {
                col.enabled = false;
            }
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play("ObjectPickup");
        }
    }

    private void ReleaseObject()
    {
        if (!isBeingHeld) return;

        isBeingHeld = false;
        holdPoint = null;
        
        if (playerController != null)
        {
            playerController.SetHoldingObject(false);
        }
        
        rb.isKinematic = wasKinematic;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        transform.rotation = originalRotation;
        
        foreach (Collider col in GetComponents<Collider>())
        {
            if (!col.isTrigger)
            {
                col.enabled = true;
            }
        }
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play("ObjectDrop");
        }
    }

    private IEnumerator InteractionCooldown()
    {
        canInteract = false;
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }
} 