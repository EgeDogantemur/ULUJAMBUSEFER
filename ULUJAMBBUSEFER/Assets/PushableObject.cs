using UnityEngine;

public class PushableObject : MonoBehaviour
{
    public float pushForce = 5f; // İtme kuvveti
    private Rigidbody rb;
    private bool isBeingPushed = false;
    private Vector3 pushDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Dönmeyi engelle
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Karakterin yönünü al
            Vector3 playerDirection = collision.gameObject.transform.forward;
            
            // Karakterin yönüne göre itme yönünü belirle
            pushDirection = new Vector3(playerDirection.x, 0f, playerDirection.z).normalized;
            
            // İtme kuvvetini uygula
            rb.AddForce(pushDirection * pushForce, ForceMode.Force);
            isBeingPushed = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isBeingPushed = false;
        }
    }
} 