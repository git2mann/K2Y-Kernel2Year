using UnityEngine;
using System.Collections;

public class TemporalAbilities : MonoBehaviour
{
    [Header("Ability Settings")]
    public float systemRestoreDuration = 3f;
    public float temporalDebugRadius = 5f;
    public float temporalDebugDuration = 2f;
    
    [Header("Cooldowns")]
    public float systemRestoreCooldown = 5f;
    public float temporalDebugCooldown = 3f;
    
    [Header("Visual Effects")]
    public Color systemRestoreColor = Color.cyan;
    public Color temporalDebugColor = Color.yellow;
    
    private bool canUseSystemRestore = true;
    private bool canUseTemporalDebug = true;
    private Camera playerCamera;
    
    void Start()
    {
        playerCamera = Camera.main;
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        // Q Key - System Restore (freezes reverse time temporarily)
        if (Input.GetKeyDown(KeyCode.Q) && canUseSystemRestore)
        {
            StartCoroutine(SystemRestore());
        }
        
        // E Key - Temporal Debug (pauses reverse objects in radius)
        if (Input.GetKeyDown(KeyCode.E) && canUseTemporalDebug)
        {
            StartCoroutine(TemporalDebug());
        }
    }
    
    IEnumerator SystemRestore()
    {
        canUseSystemRestore = false;
        
        Debug.Log("SYSTEM RESTORE ACTIVATED!");
        
        // Stop all reverse time
        if (ReverseTimeManager.Instance != null)
        {
            ReverseTimeManager.Instance.SetReverseTimeActive(false);
        }
        
        // Visual effect
        StartCoroutine(SystemRestoreVisualEffect());
        
        // Wait for duration
        yield return new WaitForSeconds(systemRestoreDuration);
        
        // Reactivate reverse time
        if (ReverseTimeManager.Instance != null)
        {
            ReverseTimeManager.Instance.SetReverseTimeActive(true);
        }
        
        Debug.Log("System restore ended - reverse time resumed");
        
        // Cooldown
        yield return new WaitForSeconds(systemRestoreCooldown);
        canUseSystemRestore = true;
        
        Debug.Log("System restore ready");
    }
    
    IEnumerator TemporalDebug()
    {
        canUseTemporalDebug = false;
        
        Debug.Log("TEMPORAL DEBUG ACTIVATED!");
        
        // Find all reverse objects in radius
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, temporalDebugRadius);
        
        // Temporarily pause reverse objects in range
        foreach (var collider in objectsInRange)
        {
            IReverseTimeObject reverseObj = collider.GetComponent<IReverseTimeObject>();
            if (reverseObj != null && !ReferenceEquals(reverseObj, this))
            {
                StartCoroutine(PauseReverseObject(collider.gameObject));
            }
        }
        
        // Visual effect
        StartCoroutine(TemporalDebugVisualEffect());
        
        // Cooldown
        yield return new WaitForSeconds(temporalDebugCooldown);
        canUseTemporalDebug = true;
        
        Debug.Log("Temporal debug ready");
    }
    
    IEnumerator PauseReverseObject(GameObject obj)
    {
        IReverseTimeObject reverseObj = obj.GetComponent<IReverseTimeObject>();
        
        // Unregister from reverse time system
        if (ReverseTimeManager.Instance != null)
        {
            ReverseTimeManager.Instance.UnregisterObject(reverseObj);
        }
        
        // Visual indicator
        SpriteRenderer sprite = obj.GetComponent<SpriteRenderer>();
        Color originalColor = sprite.color;
        sprite.color = temporalDebugColor;
        
        Debug.Log("Paused reverse object: " + obj.name);
        
        // Wait for debug duration
        yield return new WaitForSeconds(temporalDebugDuration);
        
        // Restore color
        sprite.color = originalColor;
        
        // Re-register with reverse time system
        if (ReverseTimeManager.Instance != null)
        {
            ReverseTimeManager.Instance.RegisterObject(reverseObj);
        }
        
        Debug.Log("Resumed reverse object: " + obj.name);
    }
    
    IEnumerator SystemRestoreVisualEffect()
    {
        // Create visual indicator
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effect.name = "SystemRestoreEffect";
        effect.transform.position = transform.position;
        effect.transform.localScale = Vector3.one * 2f;
        
        // Configure visual
        Renderer effectRenderer = effect.GetComponent<Renderer>();
        effectRenderer.material.color = new Color(systemRestoreColor.r, systemRestoreColor.g, systemRestoreColor.b, 0.5f);
        
        // Remove collider
        Destroy(effect.GetComponent<Collider>());
        
        // Animate
        float elapsed = 0f;
        while (elapsed < systemRestoreDuration)
        {
            float pulse = Mathf.Sin(elapsed * 10f) * 0.2f + 1f;
            effect.transform.localScale = Vector3.one * (2f * pulse);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Destroy(effect);
    }
    
    IEnumerator TemporalDebugVisualEffect()
    {
        // Create visual indicator
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effect.name = "TemporalDebugEffect";
        effect.transform.position = transform.position;
        effect.transform.localScale = Vector3.one * temporalDebugRadius * 2f;
        
        // Configure visual
        Renderer effectRenderer = effect.GetComponent<Renderer>();
        effectRenderer.material.color = new Color(temporalDebugColor.r, temporalDebugColor.g, temporalDebugColor.b, 0.3f);
        
        // Remove collider
        Destroy(effect.GetComponent<Collider>());
        
        // Fade out
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            float alpha = 0.3f * (1f - elapsed);
            effectRenderer.material.color = new Color(temporalDebugColor.r, temporalDebugColor.g, temporalDebugColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Destroy(effect);
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw temporal debug radius
        Gizmos.color = temporalDebugColor;
        Gizmos.DrawWireSphere(transform.position, temporalDebugRadius);
    }
}