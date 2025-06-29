using UnityEngine;
using System.Collections;

public class ReversePlatform : MonoBehaviour, IReverseTimeObject
{
    [Header("Platform Behavior")]
    public float destructionTime = 4f;
    public float reconstructionSpeed = 2f;
    public Vector3 brokenOffset = Vector3.down * 2f;
    
    [Header("Visual Effects")]
    public Color normalColor = Color.white;
    public Color damagedColor = Color.orange;
    public Color brokenColor = Color.red;
    
    private Vector3 originalPosition;
    private bool isBroken = false;
    private bool isReconstructing = false;
    private float destructionTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D platformCollider;
    
    void Start()
    {
        originalPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<BoxCollider2D>();
        
        // Start destruction sequence
        StartCoroutine(InitialDestruction());
        
        if (ReverseTimeManager.Instance != null)
        {
            ReverseTimeManager.Instance.RegisterObject(this);
        }
    }
    
    IEnumerator InitialDestruction()
    {
        // Gradual damage
        for (float t = 0; t < destructionTime; t += 0.5f)
        {
            float damagePercent = t / destructionTime;
            spriteRenderer.color = Color.Lerp(normalColor, damagedColor, damagePercent);
            
            // Shake effect
            transform.position = originalPosition + Random.insideUnitSphere * 0.1f * damagePercent;
            
            yield return new WaitForSeconds(0.5f);
        }
        
        // Final break
        BreakPlatform();
    }
    
    void BreakPlatform()
    {
        Debug.Log("Platform broken - will reconstruct in reverse time");
        
        isBroken = true;
        destructionTimer = 0f;
        
        // Move to broken position
        transform.position = originalPosition + brokenOffset;
        spriteRenderer.color = brokenColor;
        
        // Disable collision
        platformCollider.enabled = false;
    }
    
    public void OnReverseTimeUpdate(float deltaTime)
    {
        if (isBroken && !isReconstructing)
        {
            destructionTimer += Time.deltaTime;
            
            if (destructionTimer >= 2f) // Wait before starting reconstruction
            {
                StartReconstruction();
            }
        }
        else if (isReconstructing)
        {
            UpdateReconstruction();
        }
    }
    
    void StartReconstruction()
    {
        isReconstructing = true;
        Debug.Log("Platform reconstructing in reverse time!");
    }
    
    void UpdateReconstruction()
    {
        // Move back to original position
        transform.position = Vector3.MoveTowards(
            transform.position,
            originalPosition,
            reconstructionSpeed * Time.deltaTime
        );
        
        // Gradually restore color
        float distance = Vector3.Distance(transform.position, originalPosition);
        float progress = 1f - (distance / brokenOffset.magnitude);
        
        spriteRenderer.color = Color.Lerp(brokenColor, normalColor, progress);
        
        // When fully reconstructed
        if (distance < 0.1f)
        {
            CompleteReconstruction();
        }
    }
    
    void CompleteReconstruction()
    {
        Debug.Log("Platform fully reconstructed!");
        
        transform.position = originalPosition;
        spriteRenderer.color = normalColor;
        platformCollider.enabled = true;
        
        // Reset for next cycle
        isBroken = false;
        isReconstructing = false;
        destructionTimer = 0f;
        
        // Start destruction again
        StartCoroutine(InitialDestruction());
    }
    
    void OnDestroy()
    {
        if (ReverseTimeManager.Instance != null)
        {
            ReverseTimeManager.Instance.UnregisterObject(this);
        }
    }
}