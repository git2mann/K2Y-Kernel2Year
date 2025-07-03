using UnityEngine;
using System.Collections;

public class TemporalUnexplosionEffect : MonoBehaviour
{
    [Header("Kenney Smoke Particle Setup")]
    [SerializeField] private Sprite[] smokeParticleSprites;
    [SerializeField] private GameObject unexplodedObject;
    [SerializeField] private GameObject explodedState;
    
    [Header("Unexplosion Settings")]
    [SerializeField] private int particleCount = 15;
    [SerializeField] private float particleSpeed = 3f;
    [SerializeField] private float unexplosionRadius = 3f;
    [SerializeField] private float particleLifetime = 1.2f;
    [SerializeField] private float explosionSpeed = 1.0f; // NEW: tweakable explosion speed (1 = normal, <1 = slower, >1 = faster)
    
    [Header("Temporal Behavior")]
    [SerializeField] private bool respondToTimeDirection = true;
    [SerializeField] private float timeDirectionThreshold = -0.5f;
    
    [Header("Y2K Glitch Effects")]
    [SerializeField] private bool enableGlitchEffect = true;
    [SerializeField] private Color glitchColor = Color.red;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip unexplosionSound;
    [SerializeField] private AudioClip explosionSound;
    
    private bool isCurrentlyExploded = true;
    private bool isAnimating = false;
    private Coroutine currentAnimation;
    private System.Collections.Generic.List<GameObject> activeParticles = new System.Collections.Generic.List<GameObject>();
    
    void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (TemporalController.Instance != null && respondToTimeDirection)
        {
            TemporalController.Instance.OnTimeScaleChanged += OnTimeScaleChanged;
        }
        
        SetExplodedState(true);
    }
    
    void OnDestroy()
    {
        if (TemporalController.Instance != null)
        {
            TemporalController.Instance.OnTimeScaleChanged -= OnTimeScaleChanged;
        }
    }
    
    void OnTimeScaleChanged(float timeScale)
    {
        if (!respondToTimeDirection) return;
        
        if (timeScale <= timeDirectionThreshold && isCurrentlyExploded && !isAnimating)
        {
            TriggerUnexplosion();
        }
        else if (timeScale > timeDirectionThreshold && !isCurrentlyExploded && !isAnimating)
        {
            TriggerExplosion();
        }
    }
    
    [ContextMenu("Test Unexplosion")]
    public void TriggerUnexplosion()
    {
        if (isAnimating) return;
        
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        
        currentAnimation = StartCoroutine(UnexplosionSequence());
    }
    
    [ContextMenu("Test Explosion")]
    public void TriggerExplosion()
    {
        if (isAnimating) return;
        
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        
        currentAnimation = StartCoroutine(ExplosionSequence());
    }
    
    IEnumerator UnexplosionSequence()
    {
        isAnimating = true;
        
        Debug.Log("Starting unexplosion sequence!");
        
        SetExplodedState(true);
        CreateTemporalParticles(true);
        
        if (unexplosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(unexplosionSound);
        }
        
        yield return new WaitForSeconds(particleLifetime * 0.8f);
        
        SetExplodedState(false);
        
        if (enableGlitchEffect)
        {
            yield return StartCoroutine(GlitchRestoration());
        }
        
        isAnimating = false;
        isCurrentlyExploded = false;
        
        Debug.Log("Unexplosion complete!");
    }
    
    IEnumerator ExplosionSequence()
    {
        isAnimating = true;

        Debug.Log("Starting explosion sequence!");

        SetExplodedState(false);
        CreateTemporalParticles(false);

        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        // Wait for explosionSpeed-scaled time before hiding object
        yield return new WaitForSeconds(0.2f * (1f / Mathf.Max(0.01f, explosionSpeed)));
        SetExplodedState(true);

        // Wait for particles to finish, scaled by explosionSpeed
        yield return new WaitForSeconds(particleLifetime * 0.8f * (1f / Mathf.Max(0.01f, explosionSpeed)));

        isAnimating = false;
        isCurrentlyExploded = true;

        Debug.Log("Explosion complete!");
    }
    
    void CreateTemporalParticles(bool moveInward)
    {
        if (smokeParticleSprites == null || smokeParticleSprites.Length == 0)
        {
            Debug.LogWarning("No Kenney smoke sprites assigned!");
            return;
        }
        
        ClearActiveParticles();
        
        for (int i = 0; i < particleCount; i++)
        {
            GameObject smokeParticle = new GameObject("TemporalParticle_" + i);
            smokeParticle.transform.parent = transform;
            
            SpriteRenderer sr = smokeParticle.AddComponent<SpriteRenderer>();
            sr.sprite = smokeParticleSprites[Random.Range(0, smokeParticleSprites.Length)];
            sr.sortingOrder = 10;
            
            Rigidbody2D rb = smokeParticle.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.5f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            
            float angle = (i / (float)particleCount) * 360f * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * unexplosionRadius,
                Mathf.Sin(angle) * unexplosionRadius,
                0f
            );
            
            if (moveInward)
            {
                smokeParticle.transform.position = transform.position + offset;
                Vector2 inwardDirection = -offset.normalized;
                rb.linearVelocity = inwardDirection * particleSpeed;
            }
            else
            {
                smokeParticle.transform.position = transform.position;
                Vector2 outwardDirection = offset.normalized;
                rb.linearVelocity = outwardDirection * particleSpeed;
            }
            
            activeParticles.Add(smokeParticle);
            StartCoroutine(AnimateTemporalParticle(smokeParticle, sr, rb, moveInward, moveInward ? 1f : explosionSpeed));
        }
    }
    
    IEnumerator AnimateTemporalParticle(GameObject particle, SpriteRenderer sr, Rigidbody2D rb, bool moveInward, float speedMultiplier = 1f)
    {
        Vector3 originalScale = particle.transform.localScale;
        Color originalColor = sr.color;
        float elapsed = 0f;
        float lifetime = particleLifetime * (1f / Mathf.Max(0.01f, speedMultiplier));

        while (elapsed < lifetime && particle != null)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / lifetime;
            
            float scale = moveInward ? Mathf.Lerp(1f, 0.2f, progress) : Mathf.Lerp(0.2f, 1f, progress);
            particle.transform.localScale = originalScale * scale;
            
            float alpha;
            if (progress < 0.3f)
            {
                alpha = Mathf.Lerp(0f, 1f, progress / 0.3f);
            }
            else
            {
                alpha = Mathf.Lerp(1f, 0f, (progress - 0.3f) / 0.7f);
            }
            
            Color currentColor = originalColor;
            if (enableGlitchEffect && Random.Range(0f, 1f) < 0.05f)
            {
                currentColor = glitchColor;
            }
            
            currentColor.a = alpha;
            sr.color = currentColor;
            
            rb.linearDamping = Mathf.Lerp(0f, 5f, progress);
            
            yield return null;
        }
        
        if (particle != null)
        {
            activeParticles.Remove(particle);
            Destroy(particle);
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
    
    void ClearActiveParticles()
    {
        foreach (GameObject particle in activeParticles)
        {
            if (particle != null)
            {
                Destroy(particle);
            }
        }
        activeParticles.Clear();
    }
    
    IEnumerator GlitchRestoration()
    {
        SpriteRenderer objectRenderer = unexplodedObject.GetComponent<SpriteRenderer>();
        Color originalColor = objectRenderer != null ? objectRenderer.color : Color.white;
        
        for (int i = 0; i < 8; i++)
        {
            if (objectRenderer != null)
            {
                objectRenderer.color = (i % 2 == 0) ? glitchColor : originalColor;
            }
            yield return new WaitForSeconds(0.05f);
        }
        
        if (objectRenderer != null)
        {
            objectRenderer.color = originalColor;
        }
    }
    
    public bool IsExploded() => isCurrentlyExploded;
    public void SetTemporalResponse(bool enabled) => respondToTimeDirection = enabled;
}
