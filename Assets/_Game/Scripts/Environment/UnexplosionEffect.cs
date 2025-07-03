using UnityEngine;
using System.Collections;

public class UnexplosionEffect : MonoBehaviour
{
    [Header("Kenney Smoke Particle Setup")]
    [SerializeField] private Sprite[] smokeParticleSprites; // Individual smoke sprites from Kenney pack
    [SerializeField] private GameObject unexplodedObject; // What appears after unexplosion
    [SerializeField] private GameObject explodedState; // What shows when exploded (optional)
    
    [Header("Unexplosion Settings")]
    [SerializeField] private int particleCount = 15; // Reduced from 25 for better performance
    [SerializeField] private float particleSpeed = 3f;
    [SerializeField] private float unexplosionRadius = 3f; // How far particles start from center
    [SerializeField] private float particleLifetime = 1.2f; // Slightly shorter
    [SerializeField] private bool smoothMovement = true; // New: Use smooth interpolation
    
    [Header("Temporal Behavior")]
    [SerializeField] private bool respondToTimeDirection = true; // Auto-trigger based on time
    [SerializeField] private float timeDirectionThreshold = -0.5f; // How reversed time needs to be
    [SerializeField] private bool startExploded = true; // Start in exploded state
    
    [Header("Y2K Glitch Effects")]
    [SerializeField] private bool enableGlitchEffect = true;
    [SerializeField] private Color glitchColor = Color.red;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip unexplosionSound; // Reverse explosion sound
    [SerializeField] private AudioClip explosionSound; // Forward explosion sound
    
    private bool isUnexploding = false;
    private bool isCurrentlyExploded = true; // Track current state
    private bool isAnimating = false;
    
    void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Subscribe to temporal events
        if (TemporalController.Instance != null && respondToTimeDirection)
        {
            TemporalController.Instance.OnTimeScaleChanged += OnTimeDirectionChanged;
            Debug.Log($"Connected to TemporalController: {gameObject.name}");
        }
        else if (respondToTimeDirection)
        {
            Debug.LogWarning($"TemporalController not found for {gameObject.name}! Make sure it exists in the scene.");
        }
        
        // Set initial state
        SetExplodedState(startExploded);
        isCurrentlyExploded = startExploded;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (TemporalController.Instance != null)
        {
            TemporalController.Instance.OnTimeScaleChanged -= OnTimeDirectionChanged;
        }
    }
    
    void OnTimeDirectionChanged(float timeDirection)
    {
        if (!respondToTimeDirection || isAnimating) return;
        
        // When time goes reverse, trigger unexplosion
        if (timeDirection <= timeDirectionThreshold && isCurrentlyExploded)
        {
            TriggerUnexplosion();
        }
        // When time goes forward, trigger explosion
        else if (timeDirection > timeDirectionThreshold && !isCurrentlyExploded)
        {
            TriggerExplosion();
        }
    }
    
    [ContextMenu("Test Unexplosion")]
    public void TriggerUnexplosion()
    {
        if (!isAnimating)
        {
            StartCoroutine(UnexplosionSequence());
        }
    }
    
    [ContextMenu("Test Explosion")]
    public void TriggerExplosion()
    {
        if (!isAnimating)
        {
            StartCoroutine(ExplosionSequence());
        }
    }
    
    IEnumerator UnexplosionSequence()
    {
        isAnimating = true;
        isUnexploding = true;
        
        Debug.Log($"Starting unexplosion sequence with Kenney smoke particles! ({gameObject.name})");
        
        // Start with exploded state
        SetExplodedState(true);
        
        // Create the inward-moving smoke particles
        CreateKenneySmokeUnexplosion(true); // true = inward movement
        
        // Play unexplosion sound
        if (unexplosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(unexplosionSound);
        }
        
        // Wait for particles to converge, then reveal the restored object
        yield return new WaitForSeconds(particleLifetime * 0.8f);
        
        // Show the restored object with Y2K glitch effect
        SetExplodedState(false);
        
        if (enableGlitchEffect)
        {
            StartCoroutine(GlitchRestoration());
        }
        
        isCurrentlyExploded = false;
        isUnexploding = false;
        isAnimating = false;
        
        Debug.Log($"Unexplosion complete! Object restored from digital ashes. ({gameObject.name})");
    }
    
    IEnumerator ExplosionSequence()
    {
        isAnimating = true;
        
        Debug.Log($"Starting explosion sequence! ({gameObject.name})");
        
        // Start with whole object
        SetExplodedState(false);
        
        // Brief moment to see the object
        yield return new WaitForSeconds(0.1f);
        
        // Create outward-moving particles
        CreateKenneySmokeUnexplosion(false); // false = outward movement
        
        // Play explosion sound
        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
        
        // Hide object and show exploded state
        yield return new WaitForSeconds(0.2f);
        SetExplodedState(true);
        
        // Wait for particles to finish
        yield return new WaitForSeconds(particleLifetime * 0.6f);
        
        isCurrentlyExploded = true;
        isAnimating = false;
        
        Debug.Log($"Explosion complete! ({gameObject.name})");
    }
    
    void CreateKenneySmokeUnexplosion(bool moveInward)
    {
        if (smokeParticleSprites == null || smokeParticleSprites.Length == 0)
        {
            Debug.LogWarning("No Kenney smoke sprites assigned! Please assign smoke particle sprites from the pack.");
            return;
        }
        
        for (int i = 0; i < particleCount; i++)
        {
            // Create individual smoke particle GameObject
            GameObject smokeParticle = new GameObject($"SmokeParticle_{i}_{(moveInward ? "In" : "Out")}");
            smokeParticle.transform.parent = transform;
            
            // Add sprite renderer with random smoke sprite from Kenney pack
            SpriteRenderer sr = smokeParticle.AddComponent<SpriteRenderer>();
            sr.sprite = smokeParticleSprites[Random.Range(0, smokeParticleSprites.Length)];
            sr.sortingOrder = 10; // Render on top
            
            // Add rigidbody for physics movement
            Rigidbody2D rb = smokeParticle.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // No gravity for smoke
            rb.linearDamping = 0f; // Start with no drag
            rb.angularDamping = 0.5f; // Some rotational resistance
            
            // Configure for smoother movement
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            
            // Position particles in expanding circle (like explosion debris)
            float angle = (i / (float)particleCount) * 360f * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * unexplosionRadius,
                Mathf.Sin(angle) * unexplosionRadius,
                0f
            );
            
            Vector3 startPos;
            Vector2 direction;
            
            if (moveInward)
            {
                // Unexplosion: start far, move to center
                startPos = transform.position + offset;
                smokeParticle.transform.position = startPos;
                
                // Add some random variation to positions
                smokeParticle.transform.position += new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f),
                    0f
                );
                
                // Make particles move INWARD toward explosion center (unexplosion!)
                direction = (transform.position - smokeParticle.transform.position).normalized;
            }
            else
            {
                // Explosion: start at center, move outward
                startPos = transform.position;
                smokeParticle.transform.position = startPos;
                
                // Make particles move OUTWARD from center
                direction = offset.normalized;
            }
            
            if (smoothMovement)
            {
                // Use smooth force application for less stuttery movement
                rb.AddForce(direction * particleSpeed, ForceMode2D.Impulse);
            }
            else
            {
                // Direct velocity for more immediate response
                rb.linearVelocity = direction * particleSpeed;
            }
            
            // Add slight random rotation for more natural look (reduced for smoothness)
            rb.AddTorque(Random.Range(-20f, 20f));
            
            // Start the particle animation and cleanup
            StartCoroutine(AnimateKenneySmoke(smokeParticle, sr, rb, moveInward));
        }
    }
    
    IEnumerator AnimateKenneySmoke(GameObject particle, SpriteRenderer sr, Rigidbody2D rb, bool moveInward)
    {
        Vector3 originalScale = particle.transform.localScale;
        Color originalColor = sr.color;
        float elapsed = 0f;
        
        // Optional: Cycle through different smoke sprites for animation
        float spriteChangeInterval = 0.1f;
        float lastSpriteChange = 0f;
        
        while (elapsed < particleLifetime && particle != null)
        {
            elapsed += Time.deltaTime; // Use regular deltaTime (not affected by our time direction)
            float progress = elapsed / particleLifetime;
            
            // Scale behavior depends on direction
            float scale;
            if (moveInward)
            {
                // Particles get SMALLER as they move inward (converging)
                scale = Mathf.Lerp(1f, 0.2f, progress);
            }
            else
            {
                // Particles get LARGER as they move outward (dispersing)
                scale = Mathf.Lerp(0.2f, 1f, progress);
            }
            particle.transform.localScale = originalScale * scale;
            
            // Fade in at start, then fade out near end
            float alpha;
            if (progress < 0.3f)
            {
                alpha = Mathf.Lerp(0f, 1f, progress / 0.3f); // Fade in
            }
            else
            {
                alpha = Mathf.Lerp(1f, 0f, (progress - 0.3f) / 0.7f); // Fade out
            }
            
            // Y2K glitch effect: occasionally flash red
            Color currentColor = originalColor;
            if (enableGlitchEffect && Random.Range(0f, 1f) < 0.05f) // 5% chance per frame
            {
                currentColor = glitchColor;
            }
            
            currentColor.a = alpha;
            sr.color = currentColor;
            
            // Change smoke sprite occasionally for variety
            if (elapsed - lastSpriteChange > spriteChangeInterval && smokeParticleSprites.Length > 1)
            {
                sr.sprite = smokeParticleSprites[Random.Range(0, smokeParticleSprites.Length)];
                lastSpriteChange = elapsed;
            }
            
            // Slow down particles as they approach center (more realistic)
            rb.linearDamping = Mathf.Lerp(0f, 5f, progress);
            
            yield return null;
        }
        
        if (particle != null)
        {
            // Use different destroy method based on play mode
            #if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Destroy(particle);
            }
            else
            {
                DestroyImmediate(particle);
            }
            #else
            Destroy(particle);
            #endif
        }
    }
    
    void SetExplodedState(bool exploded)
    {
        if (unexplodedObject != null)
        {
            unexplodedObject.SetActive(!exploded);
        }
        
        if (explodedState != null)
        {
            explodedState.SetActive(exploded);
        }
    }
    
    IEnumerator GlitchRestoration()
    {
        // Y2K-style glitch effect when object reappears
        SpriteRenderer objectRenderer = unexplodedObject?.GetComponent<SpriteRenderer>();
        Color originalColor = objectRenderer != null ? objectRenderer.color : Color.white;
        
        // Flash between normal and glitch colors
        for (int i = 0; i < 8; i++)
        {
            if (objectRenderer != null)
            {
                objectRenderer.color = (i % 2 == 0) ? glitchColor : originalColor;
            }
            yield return new WaitForSeconds(0.05f);
        }
        
        // Restore original color
        if (objectRenderer != null)
        {
            objectRenderer.color = originalColor;
        }
        
        Debug.Log("Object restored with Y2K glitch effect!");
    }
    
    // Method to trigger from temporal zones or other scripts
    public void TriggerFromTemporalZone()
    {
        if (!isUnexploding && !isAnimating)
        {
            StartCoroutine(UnexplosionSequence());
        }
    }
    
    // Public methods for checking state
    public bool IsExploded() => isCurrentlyExploded;
    public bool IsAnimating() => isAnimating;
    
    // Method to manually set temporal response
    public void SetTemporalResponse(bool enabled)
    {
        respondToTimeDirection = enabled;
    }
}