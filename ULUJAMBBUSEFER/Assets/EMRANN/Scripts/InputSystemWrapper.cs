using UnityEngine;

// InputSystem içinde sorun yaşıyoruz, bu yüzden geçici olarak klasik Input sistemini kullanacak bir wrapper oluşturuyoruz
public class InputSystemWrapper : MonoBehaviour
{
    public static InputSystemWrapper Instance { get; private set; }

    private Vector3 movementInput;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool interactPressed;
    private bool interactHeld;
    private bool holdPressed; // F tuşu ile tutma (basma anı)
    private bool holdHeld;    // F tuşu ile tutma (basılı tutma)
    private bool clonePressed;
    private bool pausePressed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Hareket inputu
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        movementInput = new Vector3(horizontal, 0f, vertical).normalized;

        // Zıplama inputu
        jumpPressed = Input.GetButtonDown("Jump");
        jumpHeld = Input.GetKey(KeyCode.Space);

        // Etkileşim inputu (E tuşu)
        interactPressed = Input.GetKeyDown(KeyCode.E);
        interactHeld = Input.GetKey(KeyCode.E);
        
        // Tutma inputu (F tuşu)
        holdPressed = Input.GetKeyDown(KeyCode.F);
        holdHeld = Input.GetKey(KeyCode.F);

        // Klon inputu
        clonePressed = Input.GetKeyDown(KeyCode.Q);

        // Pause inputu
        pausePressed = Input.GetKeyDown(KeyCode.Escape);
    }

    // Getter metodları
    public Vector3 GetMovementInput()
    {
        return movementInput;
    }

    public bool GetJumpPressed()
    {
        return jumpPressed;
    }

    public bool GetJumpHeld()
    {
        return jumpHeld;
    }

    public bool GetInteractPressed()
    {
        return interactPressed;
    }

    public bool GetInteractHeld()
    {
        return interactHeld;
    }
    
    public bool GetHoldPressed()
    {
        return holdPressed;
    }
    
    public bool GetHoldHeld()
    {
        return holdHeld;
    }

    public bool GetClonePressed()
    {
        return clonePressed;
    }

    public bool GetPausePressed()
    {
        return pausePressed;
    }
} 