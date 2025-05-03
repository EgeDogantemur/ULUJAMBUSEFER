using UnityEngine;
using System.Collections.Generic;

public class ButtonController : MonoBehaviour
{
    [System.Serializable]
    public class ButtonAction
    {
        public Transform targetObject; // Etkileşime girecek nesne
        public enum ActionType { Move, Scale }
        public ActionType actionType = ActionType.Move;
        
        // Hareket ayarları
        public Vector3 movementDirection = Vector3.up;
        public float movementDistance = 2f;
        public float movementSpeed = 2f;
        
        // Scale ayarları
        public bool useScaleInstead = false;
        public Vector3 scaleDirection = Vector3.up;
        public float scaleAmount = 2f;
        public float scaleSpeed = 1f;
        public bool canScaleToZero = false;
        
        // Pozisyon ayarları
        public bool moveWithScale = true;
        public bool invertPosition = false;
        
        // Özel ayarlar
        public bool reverseOnRelease = true; // Buton bırakıldığında tersine dönsün mü?
        public float delay = 0f; // İşlemin başlaması için gecikme süresi
    }

    public List<ButtonAction> buttonActions = new List<ButtonAction>();
    private bool isPressed = false;
    private Dictionary<Transform, Vector3> startPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Vector3> startScales = new Dictionary<Transform, Vector3>();

    void Start()
    {
        // Başlangıç pozisyonlarını ve scale'lerini kaydet
        foreach (var action in buttonActions)
        {
            if (action.targetObject != null)
            {
                startPositions[action.targetObject] = action.targetObject.position;
                startScales[action.targetObject] = action.targetObject.localScale;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pushable") && !isPressed)
        {
            isPressed = true;
            foreach (var action in buttonActions)
            {
                if (action.targetObject != null)
                {
                    if (action.delay > 0)
                    {
                        StartCoroutine(DelayedAction(action, false));
                    }
                    else
                    {
                        StartAction(action, false);
                    }
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Pushable") && isPressed)
        {
            isPressed = false;
            foreach (var action in buttonActions)
            {
                if (action.targetObject != null && action.reverseOnRelease)
                {
                    if (action.delay > 0)
                    {
                        StartCoroutine(DelayedAction(action, true));
                    }
                    else
                    {
                        StartAction(action, true);
                    }
                }
            }
        }
    }

    private System.Collections.IEnumerator DelayedAction(ButtonAction action, bool reverse)
    {
        yield return new WaitForSeconds(action.delay);
        StartAction(action, reverse);
    }

    private void StartAction(ButtonAction action, bool reverse)
    {
        if (action.actionType == ButtonAction.ActionType.Move)
        {
            StartCoroutine(MoveObject(action, reverse));
        }
        else if (action.actionType == ButtonAction.ActionType.Scale)
        {
            StartCoroutine(ScaleObject(action, reverse));
        }
    }

    private System.Collections.IEnumerator MoveObject(ButtonAction action, bool reverse)
    {
        Vector3 startPos = action.targetObject.position;
        Vector3 endPos;
        
        if (reverse)
        {
            endPos = startPositions[action.targetObject];
        }
        else
        {
            endPos = startPositions[action.targetObject] + (action.movementDirection.normalized * action.movementDistance);
        }

        float journeyLength = Vector3.Distance(startPos, endPos);
        float startTime = Time.time;

        while (Vector3.Distance(action.targetObject.position, endPos) > 0.01f)
        {
            float distanceCovered = (Time.time - startTime) * action.movementSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;
            
            action.targetObject.position = Vector3.Lerp(startPos, endPos, fractionOfJourney);
            yield return null;
        }

        action.targetObject.position = endPos;
    }

    private System.Collections.IEnumerator ScaleObject(ButtonAction action, bool reverse)
    {
        Vector3 startScale = action.targetObject.localScale;
        Vector3 startPos = action.targetObject.position;
        Vector3 endScale;
        Vector3 endPos;
        
        if (reverse)
        {
            endScale = startScales[action.targetObject];
            endPos = startPositions[action.targetObject];
        }
        else
        {
            if (action.canScaleToZero)
            {
                endScale = action.scaleDirection * action.scaleAmount;
            }
            else
            {
                endScale = startScales[action.targetObject] + (action.scaleDirection * action.scaleAmount);
            }
            
            if (action.moveWithScale)
            {
                float scaleChange = (endScale.y - startScale.y) * 0.5f;
                endPos = startPos + new Vector3(0, action.invertPosition ? -scaleChange : scaleChange, 0);
            }
            else
            {
                endPos = startPos;
            }
        }

        float journeyLength = Vector3.Distance(startScale, endScale);
        float startTime = Time.time;

        while (Vector3.Distance(action.targetObject.localScale, endScale) > 0.01f)
        {
            float distanceCovered = (Time.time - startTime) * action.scaleSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;
            
            action.targetObject.localScale = Vector3.Lerp(startScale, endScale, fractionOfJourney);
            if (action.moveWithScale)
            {
                action.targetObject.position = Vector3.Lerp(startPos, endPos, fractionOfJourney);
            }
            
            yield return null;
        }

        action.targetObject.localScale = endScale;
        if (action.moveWithScale)
        {
            action.targetObject.position = endPos;
        }
    }
} 