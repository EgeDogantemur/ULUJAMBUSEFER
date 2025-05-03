using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class PlayerButtonAction
{
    public GameObject targetObject;
    public enum ActionType { Move, Scale, Bridge }
    public ActionType actionType;

    // Hareket ayarları
    public Vector3 moveDirection;
    public float moveDistance;
    public float moveSpeed;
    public bool resetOnRelease = true;
    public bool singleUse = false;

    // Ölçek ayarları
    public Vector3 scaleAxis;
    public float scaleAmount;
    public float scaleSpeed;
    public bool resetScaleOnRelease = true;
    public bool singleUseScale = false;
}

public class PlayerButtonController : MonoBehaviour
{
    public List<PlayerButtonAction> actions = new List<PlayerButtonAction>();
    private bool isPressed = false;
    public float pressDepth = 0.1f;
    public float pressSpeed = 1f;
    private Vector3 originalPosition;
    private Vector3 pressedPosition;
    private Dictionary<PlayerButtonAction, bool> actionUsed = new Dictionary<PlayerButtonAction, bool>();

    private void Start()
    {
        originalPosition = transform.position;
        pressedPosition = originalPosition - new Vector3(0, pressDepth, 0);
        
        foreach (var action in actions)
        {
            // Hedef objenin Pushable tag'ine sahip olup olmadığını kontrol et
            if (action.targetObject != null && !action.targetObject.CompareTag("Pushable"))
            {
                Debug.LogWarning($"Uyarı: {action.targetObject.name} objesi Pushable tag'ine sahip değil! Bu obje butondan etkilenmeyecek.");
            }
            actionUsed[action] = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed && other.CompareTag("Player"))
        {
            isPressed = true;
            StartCoroutine(PressButton());
            ExecuteActions(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isPressed && other.CompareTag("Player"))
        {
            isPressed = false;
            StartCoroutine(ReleaseButton());
            ExecuteActions(false);
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
        foreach (PlayerButtonAction action in actions)
        {
            if (action.targetObject == null) continue;

            // Hedef objenin Pushable tag'ine sahip olup olmadığını kontrol et
            if (!action.targetObject.CompareTag("Pushable"))
            {
                continue;
            }

            // Tek kullanımlık kontrolleri
            if ((action.singleUse && !action.resetOnRelease && actionUsed[action]) ||
                (action.singleUseScale && !action.resetScaleOnRelease && actionUsed[action]))
            {
                continue;
            }

            switch (action.actionType)
            {
                case PlayerButtonAction.ActionType.Move:
                    StartCoroutine(MoveObject(action, isPressing));
                    break;

                case PlayerButtonAction.ActionType.Scale:
                    StartCoroutine(ScaleObject(action, isPressing));
                    break;

                case PlayerButtonAction.ActionType.Bridge:
                    if (isPressing)
                    {
                        SimpleDestructibleBridge bridge = action.targetObject.GetComponent<SimpleDestructibleBridge>();
                        if (bridge != null)
                        {
                            bridge.gameObject.SetActive(true);
                        }
                    }
                    break;
            }

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

    private IEnumerator MoveObject(PlayerButtonAction action, bool moveUp)
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

    private IEnumerator ScaleObject(PlayerButtonAction action, bool scaleUp)
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