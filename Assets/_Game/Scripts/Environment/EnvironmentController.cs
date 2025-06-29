using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    [Header("Environment Effects")]
    public bool showReverseTimeIndicator = true;
    public Color reverseTimeColor = new Color(0.8f, 0.3f, 0.8f, 0.3f);
    public float pulseSpeed = 2f;
    
    private Camera mainCamera;
    private bool reverseTimeActive = true;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // Add screen overlay for reverse time indication
        if (showReverseTimeIndicator)
        {
            CreateReverseTimeOverlay();
        }
    }
    
    void CreateReverseTimeOverlay()
    {
        // Create canvas for overlay
        GameObject canvasGO = new GameObject("ReverseTimeOverlay");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        // Create overlay image
        GameObject overlayGO = new GameObject("OverlayImage");
        overlayGO.transform.SetParent(canvasGO.transform);
        
        UnityEngine.UI.Image overlay = overlayGO.AddComponent<UnityEngine.UI.Image>();
        overlay.color = reverseTimeColor;
        
        // Make it full screen
        RectTransform rect = overlayGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Add pulsing effect
        overlayGO.AddComponent<ReverseTimePulse>();
    }
    
    void Update()
    {
        // Monitor reverse time state
        if (ReverseTimeManager.Instance != null)
        {
            bool newState = ReverseTimeManager.Instance.reverseTimeActive;
            if (newState != reverseTimeActive)
            {
                reverseTimeActive = newState;
                OnReverseTimeStateChanged(reverseTimeActive);
            }
        }
    }
    
    void OnReverseTimeStateChanged(bool isActive)
    {
        if (isActive)
        {
            Debug.Log("REVERSE TIME ACTIVE - Environment responding");
            // Could add screen shake, color shifts, etc.
        }
        else
        {
            Debug.Log("REVERSE TIME PAUSED - Environment frozen");
        }
    }
}

// Component for the pulsing overlay effect
public class ReverseTimePulse : MonoBehaviour
{
    private UnityEngine.UI.Image image;
    private Color baseColor;
    
    void Start()
    {
        image = GetComponent<UnityEngine.UI.Image>();
        baseColor = image.color;
    }
    
    void Update()
    {
        if (ReverseTimeManager.Instance != null && ReverseTimeManager.Instance.reverseTimeActive)
        {
            float pulse = Mathf.Sin(Time.time * 3f) * 0.1f + 0.3f;
            image.color = new Color(baseColor.r, baseColor.g, baseColor.b, pulse);
        }
        else
        {
            image.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
        }
    }
}