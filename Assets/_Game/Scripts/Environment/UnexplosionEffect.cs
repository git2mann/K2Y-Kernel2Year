using UnityEngine;
using System.Collections;

public class UnexplosionEffect : MonoBehaviour
{
    [Header("Kenney Smoke Particle Setup")]
    [SerializeField] private Sprite[] smokeParticleSprites; // Individual smoke sprites from Kenney pack
    [SerializeField] private float frameRate = 15f; // Animation speed
    [SerializeField] private GameObject unexplodedObject; // What appears after unexplosion
    
    [Header("Unexplosion Settings")]
    [SerializeField] private int particleCount = 25; // How many smoke particles to spawn
    [SerializeField] private float particleSpeed = 3f;
    [SerializeField] private float unexplosionRadius = 3f; // How far particles start from center
    [SerializeField] private float particleLifetime = 1.5f;
    
    [Header("Y2K Glitch Effects")]
    [SerializeField] private bool enableGlitchEffect = true;
    [SerializeField] private Color glitchColor = Color.red;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip unexplosionSound; // Reverse explosion sound
    
    private bool isUnexploding = false;
    
    void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    [ContextMenu("Test Unexplosion")]
    public void TriggerUnexplosion()
    {
        if (!isUnexploding)
        {
            StartCoroutine(UnexplosionSequence());
        }
    }
    
    IEnumerator UnexplosionSequence()
    {
        isUnexploding = true;
        
        Debug.Log("Starting unexplosion sequence with Kenney smoke particles!");
        
        // Start with invisible object (post-explosion state)
        if (unexplodedObject != null)
        {
            unexplodedObject.SetActive(false);
        }
        
        // Create the inward-moving smoke particles
        CreateKenneySmokeUnexplosion();
        
        // Play unexplosion sound
        if (unexplosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(unexplosionSound);
        }
        
        // Wait for particles to converge, then reveal the restored object
        yield return new WaitForSeconds(particleLifetime * 0.8f);
        
        // Show the restored object with Y2K glitch effect
        if (unexplodedObject != null)
        {
            if (enableGlitchEffect)
            {
                StartCoroutine(GlitchRestoration());
            }
            else
            {
                unexplodedObject.SetActive(true);
            }
        }
        
        isUnexploding = false;
        
        Debug.Log("Unexplosion complete! Object restored from digital ashes.");
    }
    
    void CreateKenneySmokeUnexplosion()
    {
        if (smokeParticleSprites == null || smokeParticleSprites.Length == 0)
        {
            Debug.LogWarning("No Kenney smoke sprites assigned! Please assign smoke particle sprites from the pack.");
            return;
        }
        
        for (int i = 0; i < particleCount; i++)
        {
            // Create individual smoke particle GameObject
            GameObject smokeParticle = new GameObject("SmokeParticle_" + i);
            smokeParticle.transform.parent = transform;
            
            // Add sprite renderer with random smoke sprite from Kenney pack
            SpriteRenderer sr = smokeParticle.AddComponent<SpriteRenderer>();
            sr.sprite = smokeParticleSprites[Random.Range(0, smokeParticleSprites.Length)];
            sr.sortingOrder = 10; // Render on top
            
            // Add rigidbody for physics movement
            Rigidbody2D rb = smokeParticle.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // No gravity for smoke
            
            // Position particles in expanding circle (like explosion debris)
            float angle = (i / (float)particleCount) * 360f * Mathf.Deg2Rad;
            Vector3 startPos = transform.position + new Vector3(
                Mathf.Cos(angle) * unexplosionRadius,
                Mathf.Sin(angle) * unexplosionRadius,
                0f
            );
            smokeParticle.transform.position = startPos;
            
            // Add some random variation to positions
            smokeParticle.transform.position += new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f),
                0f
            );
            
            // Make particles move INWARD toward explosion center (unexplosion!)
            Vector2 inwardDirection = (transform.position - smokeParticle.transform.position).normalized;
            rb.AddForce(inwardDirection * particleSpeed, ForceMode2D.Impulse);
            
            // Add slight random rotation for more natural look
            rb.AddTorque(Random.Range(-50f, 50f));
            
            // Start the particle animation and cleanup
            StartCoroutine(AnimateKenneySmoke(smokeParticle, sr, rb));
        }
    }
    
    IEnumerator AnimateKenneySmoke(GameObject particle, SpriteRenderer sr, Rigidbody2D rb)
    {
        Vector3 originalScale = particle.transform.localScale;
        Color originalColor = sr.color;
        float elapsed = 0f;
        
        // Optional: Cycle through different smoke sprites for animation
        float spriteChangeInterval = 0.1f;
        float lastSpriteChange = 0f;
        
        while (elapsed < particleLifetime && particle != null)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / particleLifetime;
            
            // Particles get SMALLER as they move inward (converging)
            float scale = Mathf.Lerp(1f, 0.2f, progress);
            particle.transform.localScale = originalScale * scale;
            
            // Fade in at start, then fade out near end (unexplosion visibility)
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
            Destroy(particle);
        }
    }
    
    IEnumerator GlitchRestoration()
    {
        // Y2K-style glitch effect when object reappears
        SpriteRenderer objectRenderer = unexplodedObject.GetComponent<SpriteRenderer>();
        Color originalColor = objectRenderer != null ? objectRenderer.color : Color.white;
        
        unexplodedObject.SetActive(true);
        
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
        if (!isUnexploding)
        {
            StartCoroutine(UnexplosionSequence());
        }
    }
}