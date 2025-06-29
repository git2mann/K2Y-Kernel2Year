using UnityEngine;
using System.Collections;

public class TemporalController : MonoBehaviour
{
    [Header("Time Direction Settings")]
    [SerializeField] private float timeDirection = 1f; // 1 = normal, -1 = reverse
    [SerializeField] private float directionSmoothness = 3f; // How quickly direction changes
    
    [Header("Player Movement Detection")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float movementThreshold = 0.5f; // Minimum speed to trigger time change
    [SerializeField] private bool useInputInsteadOfVelocity = true; // Changed to TRUE - use input by default
    
    [Header("COMPLETE ISOLATION")]
    [SerializeField] private bool completelyIsolated = true; // NEVER touch anything physics-related
    
    [Header("Smooth Transitions")]
    [SerializeField] private float returnToNormalDelay = 0.2f; // Time before returning to normal
    [SerializeField] private bool requireContinuousMovement = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showConsoleDebug = false;
    
    private Vector3 lastPlayerPosition;
    private float targetTimeDirection = 1f; // What direction we want to be
    private float currentPlayerVelocityX;
    private float timeSinceLastMovement = 0f;
    
    private string debugText = "";
    
    public static TemporalController Instance { get; private set; }
    
    public System.Action<float> OnTimeScaleChanged; // Keep for compatibility
    public System.Action OnTimeReversed;
    public System.Action OnTimeForward;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
        
        if (playerTransform != null)
        {
            lastPlayerPosition = playerTransform.position;
        }
        
        // Start with normal time
        timeDirection = 1f;
        targetTimeDirection = 1f;
        
        // GUARANTEE: Never touch Time.timeScale
        if (completelyIsolated)
        {
            Time.timeScale = 1f;
            Debug.Log("TemporalController: COMPLETELY ISOLATED MODE - Will never affect physics!");
        }
    }
    
    void Update()
    {
        // ABSOLUTE GUARANTEE: Never change Time.timeScale
        if (completelyIsolated)
        {
            if (Time.timeScale != 1f)
            {
                Debug.LogWarning($"TimeScale was {Time.timeScale}, forcing back to 1.0!");
                Time.timeScale = 1f; // Force it to stay normal every frame
            }
        }
        
        if (playerTransform == null) return;
        
        UpdatePlayerMovement();
        UpdateTimeDirection();
        // REMOVED: ApplyTimeEffects(); - No physics effects at all
        
        if (showDebugInfo)
        {
            UpdateDebugInfo();
        }
    }
    
    void UpdatePlayerMovement()
    {
        if (useInputInsteadOfVelocity)
        {
            // Use raw input for more responsive feel - NEVER touches PlayerController
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            currentPlayerVelocityX = horizontalInput * 6f; // Simulate velocity from input
        }
        else
        {
            // DANGEROUS: This accesses PlayerController and causes the stopping bug!
            PlayerController playerController = playerTransform.GetComponent<PlayerController>();
            
            if (playerController != null)
            {
                // THIS LINE CAUSES THE BUG - accessing CurrentSpeed every frame
                currentPlayerVelocityX = playerController.CurrentSpeed;
            }
            else
            {
                // Fallback: calculate from position - for reading only
                Vector3 currentPosition = playerTransform.position;
                Vector3 movement = currentPosition - lastPlayerPosition;
                currentPlayerVelocityX = movement.x / Time.unscaledDeltaTime; // Use unscaled time
                lastPlayerPosition = currentPosition;
            }
        }
        
        // Track time since movement
        if (Mathf.Abs(currentPlayerVelocityX) > movementThreshold)
        {
            timeSinceLastMovement = 0f;
        }
        else
        {
            timeSinceLastMovement += Time.unscaledDeltaTime; // Use unscaled time
        }
        
        // Determine target time direction based on movement
        if (Mathf.Abs(currentPlayerVelocityX) > movementThreshold)
        {
            if (currentPlayerVelocityX > 0)
            {
                // Moving RIGHT = REVERSE TIME
                targetTimeDirection = -1f;
            }
            else
            {
                // Moving LEFT = NORMAL TIME  
                targetTimeDirection = 1f;
            }
        }
        else
        {
            // Not moving significantly
            if (requireContinuousMovement && timeSinceLastMovement > returnToNormalDelay)
            {
                // Return to normal time when stopped
                targetTimeDirection = 1f;
            }
        }
    }
    
    void UpdateTimeDirection()
    {
        float previousTimeDirection = timeDirection;
        
        // Smoothly change direction using unscaled time (not affected by Time.timeScale)
        timeDirection = Mathf.Lerp(timeDirection, targetTimeDirection, Time.unscaledDeltaTime * directionSmoothness);
        
        // Trigger events when crossing zero (direction change)
        if (previousTimeDirection >= 0 && timeDirection < 0)
        {
            OnTimeReversed?.Invoke();
            if (showConsoleDebug) Debug.Log("Time REVERSED! (Moving forward)");
        }
        else if (previousTimeDirection <= 0 && timeDirection > 0)
        {
            OnTimeForward?.Invoke();
            if (showConsoleDebug) Debug.Log("Time NORMAL! (Moving backward/stopped)");
        }
        
        // Trigger change event for visual systems (using time direction as "scale")
        OnTimeScaleChanged?.Invoke(timeDirection);
    }
    
    void UpdateDebugInfo()
    {
        string directionText = timeDirection < -0.1f ? "REVERSE" : "NORMAL";
        debugText = "Direction: " + directionText + System.Environment.NewLine + 
                   "Value: " + timeDirection.ToString("F2") + System.Environment.NewLine +
                   "Player Vel: " + currentPlayerVelocityX.ToString("F2") + System.Environment.NewLine +
                   "Input Mode: " + (useInputInsteadOfVelocity ? "INPUT" : "VELOCITY") + System.Environment.NewLine +
                   "ISOLATED: " + (completelyIsolated ? "YES" : "NO") + System.Environment.NewLine +
                   "Unity TimeScale: " + Time.timeScale.ToString("F2");
        
        if (showConsoleDebug && Time.frameCount % 60 == 0)
        {
            Debug.Log($"Temporal Debug - Direction: {directionText}, Value: {timeDirection:F2}, Player Vel: {currentPlayerVelocityX:F2}");
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugInfo || playerTransform == null || !Application.isPlaying) return;
        
        #if UNITY_EDITOR
        Vector3 debugPos = playerTransform.position + Vector3.up * 2f;
        
        UnityEditor.Handles.color = timeDirection < -0.1f ? Color.red : Color.white;
        UnityEditor.Handles.DrawWireCube(debugPos, Vector3.one * 0.2f);
        
        GUIStyle style = new GUIStyle();
        style.normal.textColor = timeDirection < -0.1f ? Color.red : Color.white;
        style.fontSize = 12;
        style.fontStyle = FontStyle.Bold;
        
        UnityEditor.Handles.Label(debugPos, debugText, style);
        #endif
    }
    
    // Public methods for compatibility with existing scripts
    public float GetTimeScale() => timeDirection; // Return direction as "scale"
    public float GetTimeDirection() => timeDirection;
    public bool IsTimeReversed() => timeDirection < -0.1f;
    public bool IsTimeNormal() => timeDirection > 0.1f;
    
    // Get clean direction for UI display
    public string GetTimeDirectionText()
    {
        if (timeDirection < -0.1f) return "REVERSE";
        if (timeDirection > 0.1f) return "NORMAL";
        return "TRANSITIONING";
    }
    
    public void SetTimeDirectionOverride(float overrideDirection, float duration = 0f)
    {
        StartCoroutine(TimeDirectionOverrideCoroutine(overrideDirection, duration));
    }
    
    // Legacy compatibility method
    public void SetTimeScaleOverride(float overrideScale, float duration = 0f)
    {
        // Convert scale to direction for compatibility
        float direction = overrideScale < 0 ? -1f : 1f;
        SetTimeDirectionOverride(direction, duration);
    }
    
    IEnumerator TimeDirectionOverrideCoroutine(float overrideDirection, float duration)
    {
        float originalSmoothness = directionSmoothness;
        directionSmoothness = 0f; // Disable player control
        targetTimeDirection = overrideDirection;
        
        if (duration > 0f)
        {
            yield return new WaitForSecondsRealtime(duration);
            directionSmoothness = originalSmoothness; // Restore player control
        }
    }
    
    public void RestorePlayerControl()
    {
        StopAllCoroutines();
        directionSmoothness = directionSmoothness == 0f ? 3f : directionSmoothness;
        
        // GUARANTEE: Always keep physics normal
        Time.timeScale = 1f;
    }
    
    [ContextMenu("Test Reverse Time")]
    public void TestReverseTime()
    {
        SetTimeDirectionOverride(-1f, 3f);
    }
    
    [ContextMenu("Test Normal Time")]
    public void TestNormalTime()
    {
        SetTimeDirectionOverride(1f, 3f);
    }
    
    [ContextMenu("Toggle Input Mode")]
    public void ToggleInputMode()
    {
        useInputInsteadOfVelocity = !useInputInsteadOfVelocity;
        Debug.Log($"Input Mode: {(useInputInsteadOfVelocity ? "INPUT" : "VELOCITY")}");
    }
    
    [ContextMenu("Force Physics Reset")]
    public void ForcePhysicsReset()
    {
        Time.timeScale = 1f;
        Debug.Log("Forced Time.timeScale back to 1.0!");
    }
}