using UnityEngine;
using System.Collections;

public class PushableObject : MonoBehaviour
{
    [Header("İtme Ayarları")]
    [SerializeField] private float pushForce = 5f;

    [Header("Tutma Ayarları")]
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float holdHeight = 1f;
    [SerializeField] private float grabStability = 5f;

    private bool isBeingHeld = false;
    private Transform holdPoint;
    private Rigidbody rb;
    private bool wasKinematic;
    private float holdStartTime;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        wasKinematic = rb.isKinematic;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (isBeingHeld && holdPoint != null)
        {
            float holdTime = Time.time - holdStartTime;
            float strength = Mathf.Min(1.0f, holdTime * grabStability);

            // Hedef pozisyonu hesapla
            targetPosition = holdPoint.position;
            targetPosition.y = holdPoint.position.y + holdHeight;

            // Yumuşak hareket
            Vector3 currentPos = rb.position;
            Vector3 newPos = Vector3.Lerp(currentPos, targetPosition, strength * Time.fixedDeltaTime * smoothSpeed);
            rb.MovePosition(newPos);

            // Yumuşak rotasyon
            targetRotation = holdPoint.rotation;
            rb.MoveRotation(Quaternion.Lerp(rb.rotation, targetRotation, strength * Time.fixedDeltaTime * rotationSpeed));
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isBeingHeld)
        {
            Vector3 moveInput = Vector3.zero;

            if (InputSystemWrapper.Instance != null)
            {
                moveInput = InputSystemWrapper.Instance.GetMovementInput();
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

    public void HoldObject(Transform grabPoint)
    {
        if (isBeingHeld) return;

        isBeingHeld = true;
        holdPoint = grabPoint;
        holdStartTime = Time.time;
        
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

    public void ReleaseObject()
    {
        if (!isBeingHeld) return;

        isBeingHeld = false;
        holdPoint = null;
        
        rb.isKinematic = wasKinematic;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
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
} 