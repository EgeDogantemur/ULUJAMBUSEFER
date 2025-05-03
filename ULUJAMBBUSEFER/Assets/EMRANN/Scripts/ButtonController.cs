using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class ButtonAction
{
    public GameObject targetObject;
    public enum ActionType { Move, Scale, Bridge }
    public ActionType actionType;

    // Hareket ayarları
    public Vector3 moveDirection;
    public float moveDistance;
    public float moveSpeed;
    public bool resetOnRelease = true; // Buton bırakıldığında eski haline dönsün mü?
    public bool singleUse = false; // Sadece bir kere kullanılsın mı? (resetOnRelease false ise geçerli)

    // Ölçek ayarları
    public Vector3 scaleAxis;
    public float scaleAmount;
    public float scaleSpeed;
    public bool resetScaleOnRelease = true; // Buton bırakıldığında eski boyutuna dönsün mü?
    public bool singleUseScale = false; // Sadece bir kere ölçeklensin mi? (resetScaleOnRelease false ise geçerli)
}

public class ButtonController : MonoBehaviour
{
    public List<ButtonAction> actions = new List<ButtonAction>();
    private bool isPressed = false;
    public float pressDepth = 0.1f;
    public float pressSpeed = 1f;
    private Vector3 originalPosition;
    private Vector3 pressedPosition;
    private Dictionary<ButtonAction, bool> actionUsed = new Dictionary<ButtonAction, bool>();

    private void Start()
    {
        originalPosition = transform.position;
        pressedPosition = originalPosition - new Vector3(0, pressDepth, 0);
        
        // Her aksiyon için kullanım durumunu takip et
        foreach (var action in actions)
        {
            actionUsed[action] = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed && other.CompareTag("Pushable"))
        {
            isPressed = true;
            StartCoroutine(PressButton());
            ExecuteActions(true);
            
            // Buton basma sesi çal
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.Play("ButtonPress");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isPressed && other.CompareTag("Pushable"))
        {
            isPressed = false;
            StartCoroutine(ReleaseButton());
            ExecuteActions(false);
            
            // Buton bırakma sesi çal (aynı ses efekti kullanılabilir)
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.Play("ButtonPress");
            }
        }
    }

    private IEnumerator PressButton()
    {
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            transform.position = Vector3.Lerp(originalPosition, pressedPosition, elapsedTime);
            elapsedTime += Time.deltaTime * pressSpeed;
            yield return null;
        }
        transform.position = pressedPosition;
    }

    private IEnumerator ReleaseButton()
    {
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            transform.position = Vector3.Lerp(pressedPosition, originalPosition, elapsedTime);
            elapsedTime += Time.deltaTime * pressSpeed;
            yield return null;
        }
        transform.position = originalPosition;
    }

    private void ExecuteActions(bool isPressing)
    {
        foreach (ButtonAction action in actions)
        {
            if (action.targetObject == null) continue;

            // Eğer singleUse true ise ve daha önce kullanıldıysa, atla
            if ((action.singleUse && !action.resetOnRelease && actionUsed[action]) ||
                (action.singleUseScale && !action.resetScaleOnRelease && actionUsed[action]))
            {
                continue;
            }

            switch (action.actionType)
            {
                case ButtonAction.ActionType.Move:
                    StartCoroutine(MoveObject(action, isPressing));
                    break;

                case ButtonAction.ActionType.Scale:
                    StartCoroutine(ScaleObject(action, isPressing));
                    break;

                case ButtonAction.ActionType.Bridge:
                    if (isPressing)
                    {
                        DestructibleBridge bridge = action.targetObject.GetComponent<DestructibleBridge>();
                        if (bridge != null)
                        {
                            bridge.RestoreBridge();
                        }
                    }
                    break;
            }

            // Eğer basıldıysa ve singleUse true ise, kullanıldı olarak işaretle
            if (isPressing)
            {
                if (action.singleUse && !action.resetOnRelease)
                {
                    actionUsed[action] = true;
                }
                if (action.singleUseScale && !action.resetScaleOnRelease)
                {
                    actionUsed[action] = true;
                }
            }
        }
    }

    private IEnumerator MoveObject(ButtonAction action, bool moveUp)
    {
        Vector3 startPos = action.targetObject.transform.position;
        Vector3 targetPos;
        
        if (moveUp)
        {
            targetPos = startPos + action.moveDirection.normalized * action.moveDistance;
        }
        else
        {
            if (action.resetOnRelease)
            {
                targetPos = startPos - action.moveDirection.normalized * action.moveDistance;
            }
            else
            {
                yield break;
            }
        }

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * action.moveSpeed;
            action.targetObject.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
    }

    private IEnumerator ScaleObject(ButtonAction action, bool scaleUp)
    {
        Vector3 startScale = action.targetObject.transform.localScale;
        Vector3 targetScale;
        
        if (scaleUp)
        {
            targetScale = startScale + Vector3.Scale(action.scaleAxis, new Vector3(action.scaleAmount, action.scaleAmount, action.scaleAmount));
        }
        else
        {
            if (action.resetScaleOnRelease)
            {
                targetScale = startScale - Vector3.Scale(action.scaleAxis, new Vector3(action.scaleAmount, action.scaleAmount, action.scaleAmount));
            }
            else
            {
                yield break;
            }
        }

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * action.scaleSpeed;
            action.targetObject.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
    }
} 