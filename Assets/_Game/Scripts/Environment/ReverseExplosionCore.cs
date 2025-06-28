using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ReverseExplosionCore : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float maxExplosionRadius = 15f;
    public float explosionSpeed = 2f;
    public Transform playerTransform;
    public Transform explosionCenter;
    
    [Header("Damage Zones")]
    public float safeRadius = 2f;
    public float warningRadius = 3f;
    
    [Header("Visual Effects")]
    public Color explosionColor = Color.red;
    public Color warningColor = Color.orange;
    public Color safeColor = Color.green;
    public int flameParticleCount = 40;
    
    [Header("Audio")]
    public AudioSource explosionAudioSource;
    public AudioClip reverseExplosionSound;
    
    private List<ExplosionFlame> flames = new List<ExplosionFlame>();
    private float currentExplosionRadius;
    private bool hasReachedCenter = false;
    
    void Start()
    {
        // Find player if not assigned
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindWithTag("Player")?.transform;
        }
        
        // Set explosion center if not assigned
        if (explosionCenter == null)
        {
            explosionCenter = transform;
        }
        
        // Start with FULL explosion visible
        currentExplosionRadius = maxExplosionRadius;
        
        // Create explosion flames immediately
        CreateExplosionFlames();
        
        // Set up audio
        SetupAudio();
        
        Debug.Log("Reverse Explosion initialized at full size. Walk toward center to make it contract!");
    }
    
    void Update()
    {
        if (playerTransform != null && !hasReachedCenter)
        {
            UpdateExplosionRadius();
            UpdateFlameVisibility();
            CheckPlayerSafety();
        }
    }
    
    void SetupAudio()
    {
        if (explosionAudioSource == null)
        {
            explosionAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (reverseExplosionSound != null)
        {
            explosionAudioSource.clip = reverseExplosionSound;
            explosionAudioSource.loop = true;
            explosionAudioSource.volume = 0.3f;
            explosionAudioSource.Play();
        }
    }
    
    void CreateExplosionFlames()
    {
        Debug.Log($"Creating {flameParticleCount} explosion flames");
        
        // Clear any existing flames
        foreach (var flame in flames)
        {
            if (flame.gameObject != null)
                DestroyImmediate(flame.gameObject);
        }
        flames.Clear();
        
        // Create flame particles in concentric circles
        for (int i = 0; i < flameParticleCount; i++)
        {
            CreateFlameParticle(i);
        }
        
        Debug.Log($"Created {flames.Count} flames around explosion center");
    }
    
    void CreateFlameParticle(int index)
    {
        GameObject flameGO = new GameObject($"Flame_{index}");
        flameGO.transform.SetParent(transform);
        
        // Create flames in rings around the center
        float ring = Mathf.Floor((float)index / 8f); // 8 flames per ring
        float angleInRing = (index % 8) * 45f; // 45 degrees apart
        float distance = safeRadius + (ring * 1.5f); // Rings 1.5 units apart
        
        // Ensure we don't exceed max radius
        distance = Mathf.Min(distance, maxExplosionRadius - 0.5f);
        
        float angleRad = angleInRing * Mathf.Deg2Rad;
        Vector3 flamePosition = explosionCenter.position + new Vector3(
            Mathf.Cos(angleRad) * distance,
            Mathf.Sin(angleRad) * distance,
            0
        );
        
        flameGO.transform.position = flamePosition;
        
        // Set up flame visual
        SpriteRenderer flameRenderer = flameGO.AddComponent<SpriteRenderer>();
        flameRenderer.sprite = Resources.GetBuiltinResource<Sprite>("Knob");
        flameRenderer.color = explosionColor;
        flameRenderer.sortingOrder = 10; // Render on top
        
        // Scale flame
        float scale = Random.Range(0.4f, 0.8f);
        flameGO.transform.localScale = Vector3.one * scale;
        
        // Create flame data
        ExplosionFlame flame = new ExplosionFlame
        {
            gameObject = flameGO,
            renderer = flameRenderer,
            originalPosition = flamePosition,
            distanceFromCenter = distance,
            isActive = true // Start all flames active
        };
        
        flames.Add(flame);
        
        // Start flame animation
        StartCoroutine(AnimateFlame(flame));
        
        Debug.Log($"Created flame {index} at distance {distance} from center");
    }
    
    void UpdateExplosionRadius()
    {
        if (explosionCenter == null || playerTransform == null) return;
        
        // Calculate distance from player to explosion center
        float playerDistance = Vector3.Distance(playerTransform.position, explosionCenter.position);
        
        // KEY FIX: As player gets CLOSER, explosion gets SMALLER
        // When player is far away (at maxExplosionRadius), explosion is at full size
        // When player is close (at safeRadius), explosion is at minimum size
        
        float normalizedDistance = Mathf.Clamp01(playerDistance / maxExplosionRadius);
        float targetRadius = Mathf.Lerp(safeRadius, maxExplosionRadius, normalizedDistance);
        
        // Smooth the transition
        currentExplosionRadius = Mathf.Lerp(currentExplosionRadius, targetRadius, explosionSpeed * Time.deltaTime);
        
        // Debug info
        if (Time.frameCount % 60 == 0) // Every second
        {
            Debug.Log($"Player distance: {playerDistance:F1}, Target radius: {targetRadius:F1}, Current radius: {currentExplosionRadius:F1}");
        }
    }
    
    void UpdateFlameVisibility()
    {
        int activeCount = 0;
        int totalCount = 0;
        
        foreach (ExplosionFlame flame in flames)
        {
            if (flame.gameObject == null) continue;
            totalCount++;
            
            // Flame is visible if it's within the current explosion radius
            bool shouldBeActive = flame.distanceFromCenter <= currentExplosionRadius;
            
            if (flame.isActive != shouldBeActive)
            {
                flame.isActive = shouldBeActive;
                flame.renderer.enabled = shouldBeActive;
                
                if (shouldBeActive)
                {
                    // Flame becoming active - add flicker effect
                    flame.renderer.color = explosionColor;
                }
            }
            
            if (flame.isActive)
            {
                activeCount++;
                UpdateFlameColor(flame);
            }
        }
        
        // Debug every few frames
        if (Time.frameCount % 120 == 0) // Every 2 seconds
        {
            Debug.Log($"Flames active: {activeCount}/{totalCount}, Explosion radius: {currentExplosionRadius:F1}");
        }
    }
    
    void UpdateFlameColor(ExplosionFlame flame)
    {
        if (!flame.isActive) return;
        
        float distanceFromPlayer = Vector3.Distance(flame.gameObject.transform.position, playerTransform.position);
        
        Color flameColor;
        if (distanceFromPlayer < warningRadius)
        {
            // Close to player - flash white for danger
            float flash = Mathf.Sin(Time.time * 15f) * 0.5f + 0.5f;
            flameColor = Color.Lerp(explosionColor, Color.white, flash);
        }
        else
        {
            // Normal explosion color with slight flicker
            float flicker = Mathf.Sin(Time.time * 8f + flame.distanceFromCenter) * 0.1f + 0.9f;
            flameColor = explosionColor * flicker;
        }
        
        flame.renderer.color = flameColor;
    }
    
    void CheckPlayerSafety()
    {
        float distanceToCenter = Vector3.Distance(playerTransform.position, explosionCenter.position);
        
        // Check if player is touching active flames
        bool inDanger = false;
        foreach (var flame in flames)
        {
            if (!flame.isActive) continue;
            
            float distanceToFlame = Vector3.Distance(playerTransform.position, flame.gameObject.transform.position);
            if (distanceToFlame < 0.8f) // Close enough to be "touching" flame
            {
                inDanger = true;
                break;
            }
        }
        
        if (inDanger)
        {
            DamagePlayer();
        }
        
        // Check if player reached the safe center
        if (distanceToCenter <= safeRadius)
        {
            OnPlayerReachedCenter();
        }
    }
    
    void DamagePlayer()
    {
        Debug.Log("Player burned by explosion flames! Resetting position.");
        
        // Reset player to a safe distance
        Vector3 direction = (playerTransform.position - explosionCenter.position).normalized;
        Vector3 safePosition = explosionCenter.position + direction * (maxExplosionRadius + 1f);
        safePosition.y = Mathf.Max(safePosition.y, -2f); // Keep above ground level
        
        playerTransform.position = safePosition;
        
        // Visual damage effect
        StartCoroutine(DamageFlash());
    }
    
    IEnumerator DamageFlash()
    {
        Camera mainCam = Camera.main;
        Color originalColor = mainCam.backgroundColor;
        
        mainCam.backgroundColor = Color.red;
        yield return new WaitForSeconds(0.15f);
        mainCam.backgroundColor = originalColor;
    }
    
    void OnPlayerReachedCenter()
    {
        if (hasReachedCenter) return;
        
        hasReachedCenter = true;
        Debug.Log("SUCCESS! Player reached the explosion center!");
        
        StartCoroutine(LevelCompleteSequence());
    }
    
    IEnumerator LevelCompleteSequence()
    {
        // Disable all flames
        foreach (var flame in flames)
        {
            if (flame.gameObject != null)
            {
                flame.renderer.enabled = false;
            }
        }
        
        // Success flash
        Camera mainCam = Camera.main;
        Color originalColor = mainCam.backgroundColor;
        
        mainCam.backgroundColor = Color.green;
        yield return new WaitForSeconds(0.3f);
        mainCam.backgroundColor = originalColor;
        
        // Notify level manager
        LevelOneManager levelManager = FindObjectOfType<LevelOneManager>();
        if (levelManager != null)
        {
            levelManager.OnExplosionCenterReached();
        }
    }
    
    IEnumerator AnimateFlame(ExplosionFlame flame)
    {
        Vector3 originalScale = flame.gameObject.transform.localScale;
        
        while (flame.gameObject != null)
        {
            if (flame.isActive)
            {
                // Flickering scale animation
                float flicker = 1f + Mathf.Sin(Time.time * Random.Range(10f, 20f)) * 0.2f;
                flame.gameObject.transform.localScale = originalScale * flicker;
                
                // Slight position wobble
                Vector3 wobble = Random.insideUnitCircle * 0.05f;
                flame.gameObject.transform.position = flame.originalPosition + wobble;
            }
            
            yield return new WaitForSeconds(0.05f);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (explosionCenter == null) return;
        
        // Draw current explosion radius
        Gizmos.color = explosionColor;
        Gizmos.DrawWireSphere(explosionCenter.position, currentExplosionRadius);
        
        // Draw safe zone
        Gizmos.color = safeColor;
        Gizmos.DrawWireSphere(explosionCenter.position, safeRadius);
        
        // Draw max explosion radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(explosionCenter.position, maxExplosionRadius);
    }
}

[System.Serializable]
public class ExplosionFlame
{
    public GameObject gameObject;
    public SpriteRenderer renderer;
    public Vector3 originalPosition;
    public float distanceFromCenter;
    public bool isActive;
}