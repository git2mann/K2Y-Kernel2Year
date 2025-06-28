// ============================================
// Enhanced Scripts/Enemies/SimpleReverseEnemy.cs (REPLACE EXISTING)
// ============================================
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SimpleReverseEnemy : MonoBehaviour, IReverseTimeObject
{
    [Header("Reverse Movement Settings")]
    public float moveSpeed = 3f;
    public float recordingDuration = 6f;
    
    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color forwardColor = Color.red;
    public Color reverseColor = Color.magenta;
    public Color pausedColor = Color.cyan;
    
    [Header("Action Effects")]
    public GameObject jumpEffectPrefab;
    public GameObject landEffectPrefab;
    
    // Recording system
    private List<EnemyAction> actionSequence = new List<EnemyAction>();
    private bool isRecordingForward = true;
    private float recordingTimer = 0f;
    private int playbackIndex = 0;
    
    // Visual effects
    private TrailRenderer trail;
    private GameObject currentEffect;
    private TextMesh stateText;
    
    // Action recording
    private Vector3 lastPosition;
    // private bool wasGrounded = true;
    
    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        lastPosition = transform.position;
        
        SetupVisualEffects();
        StartForwardRecording();
        
        if (ReverseTimeManager.Instance != null)
        {
            ReverseTimeManager.Instance.RegisterObject(this);
        }
    }
    
    void SetupVisualEffects()
    {
        // Enhanced trail
        trail = gameObject.AddComponent<TrailRenderer>();
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = Color.yellow;
        trail.endColor = Color.orange;
        trail.time = 3f;
        trail.startWidth = 0.2f;
        trail.endWidth = 0.05f;
        
        // State indicator text
        GameObject textObj = new GameObject("StateText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = Vector3.up * 1.5f;
        
        stateText = textObj.AddComponent<TextMesh>();
        stateText.text = "RECORDING";
        stateText.fontSize = 20;
        stateText.color = Color.white;
        stateText.anchor = TextAnchor.MiddleCenter;
        stateText.characterSize = 0.1f;
    }
    
    void StartForwardRecording()
    {
        isRecordingForward = true;
        recordingTimer = 0f;
        actionSequence.Clear();
        
        spriteRenderer.color = forwardColor;
        stateText.text = "RECORDING";
        stateText.color = Color.white;
        
        trail.startColor = Color.yellow;
        trail.endColor = Color.orange;
        
        Debug.Log("=== ENEMY RECORDING FORWARD ACTIONS ===");
    }
    
    void Update()
    {
        if (isRecordingForward)
        {
            RecordForwardActions();
        }
        
        UpdateVisualEffects();
    }
    
    void RecordForwardActions()
    {
        recordingTimer += Time.deltaTime;
        float progress = recordingTimer / recordingDuration;
        
        // Complex sequence of actions that will be obvious when reversed
        if (progress < 0.2f) // Walk right and jump
        {
            PerformAction("WALK_RIGHT_JUMP", Vector3.right * moveSpeed * Time.deltaTime);
            if (progress > 0.1f) PerformAction("JUMP", Vector3.up * 3f);
        }
        else if (progress < 0.4f) // Land and walk left
        {
            PerformAction("WALK_LEFT", Vector3.left * moveSpeed * Time.deltaTime);
        }
        else if (progress < 0.6f) // Stop and "attack" (visual effect)
        {
            PerformAction("ATTACK", Vector3.zero);
            if (Mathf.FloorToInt(progress * 100) % 10 == 0) CreateAttackEffect();
        }
        else if (progress < 0.8f) // Jump backwards
        {
            PerformAction("JUMP_BACK", Vector3.left * moveSpeed * 0.5f * Time.deltaTime + Vector3.up * 2f * Time.deltaTime);
        }
        else // Return to start
        {
            Vector3 directionHome = (lastPosition - transform.position).normalized;
            PerformAction("RETURN_HOME", directionHome * moveSpeed * Time.deltaTime);
        }
        
        // Record every frame
        actionSequence.Add(new EnemyAction
        {
            position = transform.position,
            facing = !spriteRenderer.flipX,
            action = "MOVE",
            time = recordingTimer
        });
        
        if (recordingTimer >= recordingDuration)
        {
            StartReversePlayback();
        }
    }
    
    void PerformAction(string actionType, Vector3 movement)
    {
        transform.position += movement;
        
        // Update facing direction
        if (movement.x > 0) spriteRenderer.flipX = false;
        else if (movement.x < 0) spriteRenderer.flipX = true;
        
        // Visual feedback for specific actions
        switch (actionType)
        {
            case "JUMP":
                CreateJumpEffect();
                break;
            case "ATTACK":
                // Pulse red for attack
                StartCoroutine(PulseColor(Color.yellow, 0.2f));
                break;
        }
    }
    
    void CreateJumpEffect()
    {
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effect.transform.position = transform.position + Vector3.down * 0.5f;
        effect.transform.localScale = Vector3.one * 0.3f;
        effect.GetComponent<Renderer>().material.color = Color.yellow;
        Destroy(effect.GetComponent<Collider>());
        Destroy(effect, 1f);
    }
    
    void CreateAttackEffect()
    {
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Cube);
        effect.transform.position = transform.position + Vector3.right * (spriteRenderer.flipX ? -1 : 1);
        effect.transform.localScale = Vector3.one * 0.2f;
        effect.GetComponent<Renderer>().material.color = Color.red;
        Destroy(effect.GetComponent<Collider>());
        Destroy(effect, 0.5f);
    }
    
    IEnumerator PulseColor(Color pulseColor, float duration)
    {
        Color original = spriteRenderer.color;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            spriteRenderer.color = Color.Lerp(original, pulseColor, Mathf.Sin(elapsed * 10f));
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        spriteRenderer.color = original;
    }
    
    void StartReversePlayback()
    {
        isRecordingForward = false;
        playbackIndex = actionSequence.Count - 1;
        
        spriteRenderer.color = reverseColor;
        stateText.text = "REVERSING";
        stateText.color = Color.magenta;
        
        trail.startColor = Color.magenta;
        trail.endColor = Color.red;
        
        Debug.Log("=== ENEMY NOW REVERSING ALL ACTIONS ===");
        Debug.Log($"Reversing {actionSequence.Count} recorded actions");
    }
    
    public void OnReverseTimeUpdate(float deltaTime)
    {
        if (!isRecordingForward && actionSequence.Count > 0)
        {
            PlayReverseActions();
        }
    }
    
    void PlayReverseActions()
    {
        if (playbackIndex >= 0 && playbackIndex < actionSequence.Count)
        {
            EnemyAction action = actionSequence[playbackIndex];
            
            // Move toward recorded position
            transform.position = Vector3.MoveTowards(
                transform.position, 
                action.position, 
                moveSpeed * 1.5f * Time.deltaTime
            );
            
            // Reverse facing direction
            spriteRenderer.flipX = !action.facing;
            
            // Create reverse effects at key moments
            if (playbackIndex % 20 == 0) // Every 20 frames
            {
                CreateReverseEffect();
            }
            
            // Move to next reverse frame when close enough
            if (Vector3.Distance(transform.position, action.position) < 0.1f)
            {
                playbackIndex--;
                
                if (playbackIndex < 0)
                {
                    CompleteReverseSequence();
                }
            }
        }
    }
    
    void CreateReverseEffect()
    {
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effect.transform.position = transform.position;
        effect.transform.localScale = Vector3.one * 0.4f;
        
        Renderer renderer = effect.GetComponent<Renderer>();
        renderer.material.color = new Color(1f, 0f, 1f, 0.6f); // Transparent magenta
        
        Destroy(effect.GetComponent<Collider>());
        
        // Animate the effect
        StartCoroutine(AnimateReverseEffect(effect));
    }
    
    IEnumerator AnimateReverseEffect(GameObject effect)
    {
        float elapsed = 0f;
        Vector3 startScale = effect.transform.localScale;
        
        while (elapsed < 0.5f)
        {
            float progress = elapsed / 0.5f;
            effect.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            
            Renderer renderer = effect.GetComponent<Renderer>();
            Color color = renderer.material.color;
            renderer.material.color = new Color(color.r, color.g, color.b, 1f - progress);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Destroy(effect);
    }
    
    void CompleteReverseSequence()
    {
        Debug.Log("=== REVERSE SEQUENCE COMPLETE - RESTARTING ===");
        
        // Brief pause to show completion
        StartCoroutine(PauseAndRestart());
    }
    
    IEnumerator PauseAndRestart()
    {
        stateText.text = "COMPLETE";
        stateText.color = Color.green;
        spriteRenderer.color = Color.green;
        
        yield return new WaitForSeconds(1f);
        
        StartForwardRecording();
    }
    
    void UpdateVisualEffects()
    {
        // Update state text position to always face camera
        if (stateText != null)
        {
            stateText.transform.rotation = Quaternion.identity;
        }
        
        // Add slight glow effect based on state
        if (isRecordingForward)
        {
            float glow = Mathf.Sin(Time.time * 4f) * 0.3f + 0.7f;
            spriteRenderer.color = Color.Lerp(forwardColor, Color.white, glow * 0.3f);
        }
    }
    
    void OnDestroy()
    {
        if (ReverseTimeManager.Instance != null)
        {
            ReverseTimeManager.Instance.UnregisterObject(this);
        }
    }
}

// Data structure for recording actions
[System.Serializable]
public class EnemyAction
{
    public Vector3 position;
    public bool facing; // true = right, false = left
    public string action;
    public float time;
}