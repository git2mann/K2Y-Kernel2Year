using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ReverseExplosion : MonoBehaviour, IReverseTimeObject
{
    [Header("Explosion Settings")]
    public int debrisCount = 8;
    public float explosionForce = 5f;
    public float explosionRadius = 3f;
    public GameObject debrisPrefab;
    
    [Header("Reverse Settings")]
    public float timeUntilReverse = 3f;
    public Color explodedColor = Color.red;
    public Color reassemblingColor = Color.orange;
    public Color normalColor = Color.white;
    
    private List<GameObject> debrisObjects = new List<GameObject>();
    private Vector3 originalPosition;
    private bool hasExploded = false;
    private bool isReassembling = false;
    private SpriteRenderer mainRenderer;
    private BoxCollider2D mainCollider;
    private float explosionTimer = 0f;
    
    void Start()
    {
        originalPosition = transform.position;
        mainRenderer = GetComponent<SpriteRenderer>();
        mainCollider = GetComponent<BoxCollider2D>();
        
        // Create initial explosion
        StartCoroutine(InitialExplosion());
        
        if (ReverseTimeManager.Instance != null)
        {
            ReverseTimeManager.Instance.RegisterObject(this);
        }
    }
    
    IEnumerator InitialExplosion()
    {
        yield return new WaitForSeconds(1f); // Brief delay before explosion
        
        Debug.Log("EXPLOSION! Creating debris that will reassemble in reverse time...");
        
        // Hide main object
        mainRenderer.enabled = false;
        mainCollider.enabled = false;
        
        // Create debris pieces
        for (int i = 0; i < debrisCount; i++)
        {
            GameObject debris = CreateDebrisPiece(i);
            debrisObjects.Add(debris);
        }
        
        hasExploded = true;
        explosionTimer = 0f;
    }
    
    GameObject CreateDebrisPiece(int index)
    {
        // Create debris piece
        GameObject debris = new GameObject($"Debris_{index}");
        debris.transform.position = transform.position;
        
        // Add visual
        SpriteRenderer sr = debris.AddComponent<SpriteRenderer>();
        sr.sprite = mainRenderer.sprite;
        sr.color = explodedColor;
        sr.sortingOrder = 1;
        
        // Scale down debris
        debris.transform.localScale = Vector3.one * 0.3f;
        
        // Add physics for initial explosion
        Rigidbody2D rb = debris.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0.5f;
        
        // Add explosion force in random direction
        Vector2 explosionDirection = Random.insideUnitCircle.normalized;
        rb.AddForce(explosionDirection * explosionForce, ForceMode2D.Impulse);
        
        // Add rotation for visual effect
        rb.angularVelocity = Random.Range(-360f, 360f);
        
        // Add trail for better visualization
        TrailRenderer trail = debris.AddComponent<TrailRenderer>();
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = Color.red;
        trail.endColor = Color.yellow;
        trail.time = 1f;
        trail.startWidth = 0.05f;
        trail.endWidth = 0.01f;
        
        return debris;
    }
    
    public void OnReverseTimeUpdate(float deltaTime)
    {
        if (hasExploded && !isReassembling)
        {
            explosionTimer += Time.deltaTime;
            
            if (explosionTimer >= timeUntilReverse)
            {
                StartReassembly();
            }
        }
        else if (isReassembling)
        {
            UpdateReassembly();
        }
    }
    
    void StartReassembly()
    {
        isReassembling = true;
        Debug.Log("Starting reverse explosion - debris reassembling!");
        
        // Change debris colors to show reverse mode
        foreach (GameObject debris in debrisObjects)
        {
            if (debris != null)
            {
                SpriteRenderer sr = debris.GetComponent<SpriteRenderer>();
                sr.color = reassemblingColor;
                
                // Remove physics - we'll control movement manually
                Rigidbody2D rb = debris.GetComponent<Rigidbody2D>();
                if (rb != null) Destroy(rb);
                
                // Change trail color for reverse
                TrailRenderer trail = debris.GetComponent<TrailRenderer>();
                if (trail != null)
                {
                    trail.startColor = Color.cyan;
                    trail.endColor = Color.blue;
                }
            }
        }
    }
    
    void UpdateReassembly()
    {
        bool allReassembled = true;
        float reassemblySpeed = 4f;
        
        foreach (GameObject debris in debrisObjects)
        {
            if (debris != null)
            {
                // Move debris back to original position
                debris.transform.position = Vector3.MoveTowards(
                    debris.transform.position, 
                    originalPosition, 
                    reassemblySpeed * Time.deltaTime
                );
                
                // Slow down rotation
                debris.transform.Rotate(0, 0, -90f * Time.deltaTime);
                
                // Scale back up as it gets closer
                float distance = Vector3.Distance(debris.transform.position, originalPosition);
                if (distance > 0.5f)
                {
                    allReassembled = false;
                }
                else
                {
                    // Fade as it gets close
                    SpriteRenderer sr = debris.GetComponent<SpriteRenderer>();
                    float alpha = Mathf.Lerp(0f, 1f, distance / 0.5f);
                    sr.color = new Color(reassemblingColor.r, reassemblingColor.g, reassemblingColor.b, alpha);
                }
            }
        }
        
        if (allReassembled)
        {
            CompleteReassembly();
        }
    }
    
    void CompleteReassembly()
    {
        Debug.Log("Explosion reversed - object fully reassembled!");
        
        // Destroy all debris
        foreach (GameObject debris in debrisObjects)
        {
            if (debris != null) Destroy(debris);
        }
        debrisObjects.Clear();
        
        // Restore main object
        mainRenderer.enabled = true;
        mainRenderer.color = normalColor;
        mainCollider.enabled = true;
        
        // Reset for next cycle
        hasExploded = false;
        isReassembling = false;
        explosionTimer = 0f;
        
        // Wait and explode again
        StartCoroutine(InitialExplosion());
    }
    
    void OnDestroy()
    {
        if (ReverseTimeManager.Instance != null)
        {
            ReverseTimeManager.Instance.UnregisterObject(this);
        }
    }
}