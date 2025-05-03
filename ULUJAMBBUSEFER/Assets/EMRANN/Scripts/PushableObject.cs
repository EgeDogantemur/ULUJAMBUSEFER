using UnityEngine;
using System.Collections;

public class PushableObject : MonoBehaviour
{
    [SerializeField] private float pushForce = 5f;
    [SerializeField] private float holdDistance = 1f;
    [SerializeField] private float smoothSpeed = 20f;
    [SerializeField] private float interactionCooldown = 0.5f;
    [SerializeField] private float maxSpeed = 10f; // Maksimum takip hızı
    [SerializeField] private float grabStability = 5f; // Tutma anında objenin ne kadar sert tutulacağı

    private bool isBeingHeld = false;
    private Transform playerTransform;
    private Transform holdPoint;
    private Rigidbody rb;
    private bool canInteract = true;
    private bool isPlayerInTrigger = false;
    private PlayerController playerController;
    private bool wasKinematic;
    private float holdStartTime;
    
    // Input System referansı
    private InputSystemWrapper inputManager;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        wasKinematic = rb.isKinematic;
        
        // Input Manager referansını al
        inputManager = InputSystemWrapper.Instance;
        if (inputManager == null)
        {
            Debug.LogError("InputSystemWrapper bulunamadı! Lütfen sahnede bir InputSystemWrapper olduğundan emin olun.");
        }
    }

    private void Update()
    {
        if (inputManager != null)
        {
            // F tuşuna basıldığında tutma işlemini başlat
            if (!isBeingHeld && isPlayerInTrigger && inputManager.GetHoldPressed() && canInteract)
            {
                HoldObject(playerTransform);
            }
            // F tuşu bırakıldığında nesneyi bırak
            else if (isBeingHeld && !inputManager.GetHoldHeld())
            {
                ReleaseObject();
                StartCoroutine(InteractionCooldown());
            }
        }
        else
        {
            // Eski Input sistemi ile yedek kontroller
            if (!isBeingHeld && isPlayerInTrigger && Input.GetKeyDown(KeyCode.F) && canInteract)
            {
                HoldObject(playerTransform);
            }
            else if (isBeingHeld && !Input.GetKey(KeyCode.F))
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
            // Tutma noktasına doğru hareket et
            Vector3 targetPosition = holdPoint.position;
            
            // Tutma süresine bağlı olarak gücü değiştir
            float holdTime = Time.time - holdStartTime;
            float strength = Mathf.Min(1.0f, holdTime * grabStability); // Zaman geçtikçe daha sağlam tutma
            
            // Rigidbody'yi doğrudan hareket ettir
            rb.MovePosition(Vector3.Lerp(rb.position, targetPosition, strength * Time.fixedDeltaTime * smoothSpeed));
            
            // Rotasyonu güncelle
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, holdPoint.rotation, strength * Time.fixedDeltaTime * smoothSpeed));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            playerTransform = other.transform;
            playerController = other.GetComponent<PlayerController>();
            
            Debug.Log("PushableObject: Oyuncu objeye yaklaştı. Tutmak için F tuşunu basılı tutun.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Eğer obje hala tutuluyorsa, bırak
            if (isBeingHeld && playerController != null)
            {
                ReleaseObject();
            }
            
            isPlayerInTrigger = false;
            playerTransform = null;
            playerController = null;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isBeingHeld)
        {
            if (inputManager != null)
            {
                Vector3 moveInput = inputManager.GetMovementInput();
                if (moveInput.magnitude > 0)
                {
                    Vector3 pushDirection = new Vector3(moveInput.x, 0, moveInput.z).normalized;
                    rb.AddForce(pushDirection * pushForce, ForceMode.Force);
                }
            }
            else
            {
                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");

                if (horizontal != 0 || vertical != 0)
                {
                    Vector3 pushDirection = new Vector3(horizontal, 0, vertical).normalized;
                    rb.AddForce(pushDirection * pushForce, ForceMode.Force);
                }
            }
        }
    }

    private void HoldObject(Transform player)
    {
        isBeingHeld = true;
        playerTransform = player;
        holdStartTime = Time.time;
        
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Eğer daha önce farklı bir playerController referansımız varsa, onu temizleyelim
            if (this.playerController != null && this.playerController != playerController)
            {
                this.playerController.SetHoldingObject(false);
            }
            
            this.playerController = playerController;
            holdPoint = playerController.GetNearestHoldPoint(transform.position);
            playerController.SetHoldingObject(true);
            Debug.Log("PushableObject: Obje tutuldu, zıplama devre dışı bırakıldı");
        }
        
        // Rigidbody ayarları
        wasKinematic = rb.isKinematic;
        rb.isKinematic = true;  // Tamamen kinematik kontrole geçirdik
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // Daha güçlü bir tutma etkisi için çarpışmayı devre dışı bırak
        foreach (Collider col in GetComponents<Collider>())
        {
            if (!col.isTrigger)
            {
                col.enabled = false;
            }
        }

        // Obje tutma sesi çal
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play("ObjectPickup");
        }
    }

    private void ReleaseObject()
    {
        // Öncelikle isBeingHeld'i false yap ki tekrarlı çağrılmasın
        isBeingHeld = false;
        holdPoint = null;
        
        // Zıplama özelliğini aktif et
        PlayerController tempController = playerController;
        if (tempController != null)
        {
            tempController.SetHoldingObject(false);
            Debug.Log("PushableObject: Obje bırakıldı, zıplama aktif edildi");
        }
        
        // Rigidbody ayarlarını geri al
        rb.isKinematic = wasKinematic;
        rb.useGravity = true;
        
        // Çarpışmaları yeniden etkinleştir
        foreach (Collider col in GetComponents<Collider>())
        {
            if (!col.isTrigger)
            {
                col.enabled = true;
            }
        }
        
        // Bırakıldığında hızı oyuncunun mevcut hareket yönüne ayarla
        if (inputManager != null && playerTransform != null)
        {
            Vector3 playerVelocity = inputManager.GetMovementInput() * 2f; // Biraz hız ekleyelim
            if (playerVelocity.magnitude > 0.1f)
            {
                rb.linearVelocity = new Vector3(playerVelocity.x, 0, playerVelocity.z);
            }
        }
        
        // Obje bırakma sesi çal
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play("ObjectDrop");
        }
    }

    private System.Collections.IEnumerator InteractionCooldown()
    {
        canInteract = false;
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }
} 