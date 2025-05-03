using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ButtonAction
{
    public GameObject targetObject;
    public enum ActionType { Move, Bridge }
    public ActionType actionType;

    // Hareket ayarları
    public Vector3 moveDirection;
    public float moveDistance;
    public float moveSpeed;
    public bool resetOnRelease = true;
}

public class ButtonController : MonoBehaviour
{
    public List<ButtonAction> actions = new List<ButtonAction>();
    private bool isPressed = false;
    private Vector3 originalPosition;
    private Vector3 pressedPosition;

    private void Start()
    {
        originalPosition = transform.position;
        pressedPosition = originalPosition - new Vector3(0, 0.1f, 0);
        
        // Bridge objelerini başlangıçta gizle
        foreach (ButtonAction action in actions)
        {
            if (action.actionType == ButtonAction.ActionType.Bridge && action.targetObject != null)
            {
                HideBridgeObject(action.targetObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed && other.CompareTag("Pushable"))
        {
            isPressed = true;
            ExecuteActions(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isPressed && other.CompareTag("Pushable"))
        {
            isPressed = false;
            ExecuteActions(false);
        }
    }

    private void ExecuteActions(bool isPressing)
    {
        foreach (ButtonAction action in actions)
        {
            if (action.targetObject == null) continue;

            switch (action.actionType)
            {
                case ButtonAction.ActionType.Move:
                    StartCoroutine(MoveObject(action, isPressing));
                    break;

                case ButtonAction.ActionType.Bridge:
                    if (isPressing)
                    {
                        ShowBridgeObject(action.targetObject);
                    }
                    break;
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

    private void HideBridgeObject(GameObject bridgeObject)
    {
        // Mesh Renderer'ı kapat
        MeshRenderer renderer = bridgeObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }

        // Collider'ı kapat
        Collider collider = bridgeObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    private void ShowBridgeObject(GameObject bridgeObject)
    {
        // Mesh Renderer'ı aç
        MeshRenderer renderer = bridgeObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
        }

        // Collider'ı aç
        Collider collider = bridgeObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }
} 