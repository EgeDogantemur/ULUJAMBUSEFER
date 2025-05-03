using UnityEngine;

public class ScaleController : MonoBehaviour
{
    [System.Serializable]
    public class AxisSettings
    {
        public bool enabled = false;
        public float scaleAmount = 2f;
        public float scaleSpeed = 1f;
        public bool moveWithScale = true; // Scale ile birlikte pozisyon da değişsin mi?
        public bool invertPosition = false; // Pozisyon değişimini tersine çevir
        public bool canScaleToZero = false; // Scale 0'a düşebilir mi?
    }

    public AxisSettings xAxis = new AxisSettings();
    public AxisSettings yAxis = new AxisSettings();
    public AxisSettings zAxis = new AxisSettings();
    
    private Vector3 startScale;
    private Vector3 startPosition;
    private bool isScaling = false;

    void Start()
    {
        startScale = transform.localScale;
        startPosition = transform.position;
    }

    public void StartScaling()
    {
        if (!isScaling)
        {
            StartCoroutine(ScaleObject());
        }
    }

    public void ResetScale()
    {
        if (!isScaling)
        {
            StartCoroutine(ScaleObject(true));
        }
    }

    private System.Collections.IEnumerator ScaleObject(bool reset = false)
    {
        isScaling = true;
        
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.position;
        
        // Hedef scale ve pozisyonu hesapla
        Vector3 targetScale = CalculateTargetScale(reset);
        Vector3 targetPosition = CalculateTargetPosition(reset, startScale, targetScale);
        
        float journeyLength = Vector3.Distance(startScale, targetScale);
        float startTime = Time.time;

        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            float distanceCovered = (Time.time - startTime) * GetMaxScaleSpeed();
            float fractionOfJourney = distanceCovered / journeyLength;
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, fractionOfJourney);
            transform.position = Vector3.Lerp(startPos, targetPosition, fractionOfJourney);
            
            yield return null;
        }

        transform.localScale = targetScale;
        transform.position = targetPosition;
        isScaling = false;
    }

    private Vector3 CalculateTargetScale(bool reset)
    {
        Vector3 target = startScale;
        
        if (!reset)
        {
            if (xAxis.enabled)
            {
                if (xAxis.canScaleToZero)
                    target.x = xAxis.scaleAmount;
                else
                    target.x = startScale.x + xAxis.scaleAmount;
            }
            
            if (yAxis.enabled)
            {
                if (yAxis.canScaleToZero)
                    target.y = yAxis.scaleAmount;
                else
                    target.y = startScale.y + yAxis.scaleAmount;
            }
            
            if (zAxis.enabled)
            {
                if (zAxis.canScaleToZero)
                    target.z = zAxis.scaleAmount;
                else
                    target.z = startScale.z + zAxis.scaleAmount;
            }
        }
        
        return target;
    }

    private Vector3 CalculateTargetPosition(bool reset, Vector3 startScale, Vector3 targetScale)
    {
        Vector3 target = startPosition;
        
        if (!reset)
        {
            if (xAxis.enabled && xAxis.moveWithScale)
            {
                float xChange = (targetScale.x - startScale.x) * 0.5f;
                target.x += xAxis.invertPosition ? -xChange : xChange;
            }
            
            if (yAxis.enabled && yAxis.moveWithScale)
            {
                float yChange = (targetScale.y - startScale.y) * 0.5f;
                target.y += yAxis.invertPosition ? -yChange : yChange;
            }
            
            if (zAxis.enabled && zAxis.moveWithScale)
            {
                float zChange = (targetScale.z - startScale.z) * 0.5f;
                target.z += zAxis.invertPosition ? -zChange : zChange;
            }
        }
        
        return target;
    }

    private float GetMaxScaleSpeed()
    {
        float maxSpeed = 0f;
        if (xAxis.enabled) maxSpeed = Mathf.Max(maxSpeed, xAxis.scaleSpeed);
        if (yAxis.enabled) maxSpeed = Mathf.Max(maxSpeed, yAxis.scaleSpeed);
        if (zAxis.enabled) maxSpeed = Mathf.Max(maxSpeed, zAxis.scaleSpeed);
        return maxSpeed;
    }

    // Inspector'da değerler değiştiğinde çağrılır
    void OnValidate()
    {
        // Pozitif hız sağla
        xAxis.scaleSpeed = Mathf.Max(0.01f, xAxis.scaleSpeed);
        yAxis.scaleSpeed = Mathf.Max(0.01f, yAxis.scaleSpeed);
        zAxis.scaleSpeed = Mathf.Max(0.01f, zAxis.scaleSpeed);
    }
}