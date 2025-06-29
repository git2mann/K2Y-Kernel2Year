using UnityEngine;
using System.Collections;

public class ReversePhysicsObject : MonoBehaviour, IReverseTimeObject
{
    [Header("Reverse Physics Settings")]
    public bool isDebris = true;
    public Vector3 targetPosition;
    public float reassemblySpeed = 2f;
    public float reassemblyTime = 3f;
    
    [Header("Visual Settings")]
    public Color debrisColor = Color.yellow;
    public Color reassembledColor = Color.white;
    
    private Vector3 originalPosition;
    private bool isReassembling = false;
    private bool isReassembled = false;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        originalPosition = transform.position;
        
        // If no target position set, create a default one
        if (targetPosition == Vector3.zero)
        {
            targetPosition = originalPosition + Vector3.up * 2f + Vector3.right * Random.Range(-2f, 2f);
        }
        
        // Set initial state
        if (isDebris)
        {
            SetDebrisState();
        }
        else
        {
            SetReassembledState();
        }
        
        // Register with reverse time system
        if (ReverseTimeManager.Instance != null)
        {
            ReverseTimeManager.Instance.RegisterObject(this);
            Debug.Log("Reverse physics object registered: " + gameObject.name);
        }
    }
    
    void OnDestroy()
    {
        if (ReverseTimeManager.Instance != null)
        {
            ReverseTimeManager.Instance.UnregisterObject(this);
        }
    }
    
    public void OnReverseTimeUpdate(float deltaTime)
    {
        if (isDebris && !isReassembling && !isReassembled)
        {
            // Move debris toward target position (reassembly point)
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, 
                reassemblySpeed * Time.deltaTime);
            
            // Check if reached target
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                StartReassembly();
            }
        }
    }
    
    void SetDebrisState()
    {
        isReassembled = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = debrisColor;
        }
        
        // Debris doesn't block player
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
        
        Debug.Log("Object set to debris state: " + gameObject.name);
    }
    
    void SetReassembledState()
    {
        isReassembled = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = reassembledColor;
        }
        
        // Reassembled object blocks player
        if (boxCollider != null)
        {
            boxCollider.isTrigger = false;
        }
        
        Debug.Log("Object set to reassembled state: " + gameObject.name);
    }
    
    void StartReassembly()
    {
        if (!isReassembling)
        {
            Debug.Log("Starting reassembly: " + gameObject.name);
            StartCoroutine(ReassemblySequence());
        }
    }
    
    IEnumerator ReassemblySequence()
    {
        isReassembling = true;
        
        // Visual reassembly effect
        for (int i = 0; i < 10; i++)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = i % 2 == 0 ? Color.white : debrisColor;
            }
            yield return new WaitForSeconds(0.1f);
        }
        
        // Fully reassemble
        SetReassembledState();
        isReassembling = false;
        isDebris = false;
        
        Debug.Log("Reassembly complete: " + gameObject.name);
    }
    
    // Call this to manually trigger explosion (for testing)
    [ContextMenu("Explode into Debris")]
    public void ExplodeIntoDebris()
    {
        transform.position = originalPosition;
        isDebris = true;
        isReassembled = false;
        isReassembling = false;
        SetDebrisState();
        Debug.Log("Object exploded into debris: " + gameObject.name);
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw target position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPosition, 0.3f);
        
        // Draw line to target
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, targetPosition);
    }
}