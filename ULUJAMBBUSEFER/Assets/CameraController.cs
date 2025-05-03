using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // Takip edilecek karakter
    public float height = 10f; // Kameranın yüksekliği
    public float distance = 10f; // Kameranın karaktere olan uzaklığı
    public float rotationSpeed = 5f; // Kamera dönüş hızı
    public float orthographicSize = 5f; // Ortographic kamera boyutu

    private Camera mainCamera;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = orthographicSize;
        
        // Kamerayı başlangıç pozisyonuna yerleştir
        UpdateCameraPosition();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Kamerayı güncelle
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        // Kameranın pozisyonunu hesapla
        Vector3 targetPosition = target.position;
        Vector3 cameraPosition = new Vector3(
            targetPosition.x - distance,
            targetPosition.y + height,
            targetPosition.z - distance
        );

        // Kamerayı pozisyonlaştır
        transform.position = cameraPosition;

        // Kamerayı hedefe doğru döndür
        Vector3 direction = target.position - transform.position;
        Quaternion desiredRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);

        // 2.5D perspektifi korumak için X ve Z rotasyonlarını kilitle
        Vector3 eulerAngles = transform.eulerAngles;
        eulerAngles.x = 45f; // Sabit 45 derece yukarı bakış açısı
        eulerAngles.z = 0f; // Z rotasyonunu sıfırla
        transform.eulerAngles = eulerAngles;
    }
} 