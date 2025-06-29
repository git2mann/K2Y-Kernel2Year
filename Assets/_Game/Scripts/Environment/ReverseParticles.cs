using UnityEngine;

public class ReverseParticles : MonoBehaviour, IReverseTimeObject
{
    [Header("Particle Settings")]
    public int particleCount = 20;
    public float spawnRadius = 2f;
    public float moveSpeed = 1f;
    public Color startColor = Color.yellow;
    public Color endColor = Color.red;
    
    private GameObject[] particles;
    private Vector3[] targetPositions;
    private bool particlesActive = true;
    
    void Start()
    {
        CreateParticles();
        
        if (ReverseTimeManager.Instance != null)
        {
            ReverseTimeManager.Instance.RegisterObject(this);
        }
    }
    
    void CreateParticles()
    {
        particles = new GameObject[particleCount];
        targetPositions = new Vector3[particleCount];
        
        for (int i = 0; i < particleCount; i++)
        {
            // Create particle
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.name = $"ReverseParticle_{i}";
            particle.transform.localScale = Vector3.one * 0.1f;
            
            // Position randomly around center
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPos.z = 0; // Keep in 2D plane
            particle.transform.position = randomPos;
            
            // Set target as center
            targetPositions[i] = transform.position;
            
            // Visual setup
            Renderer renderer = particle.GetComponent<Renderer>();
            renderer.material.color = Color.Lerp(startColor, endColor, Random.Range(0f, 1f));
            
            // Remove collider
            Destroy(particle.GetComponent<Collider>());
            
            particles[i] = particle;
        }
        
        Debug.Log("Created reverse particles - they will converge on center in reverse time");
    }
    
    public void OnReverseTimeUpdate(float deltaTime)
    {
        if (!particlesActive) return;
        
        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i] != null)
            {
                // Move particles toward center (reverse of spreading out)
                particles[i].transform.position = Vector3.MoveTowards(
                    particles[i].transform.position,
                    targetPositions[i],
                    moveSpeed * Time.deltaTime
                );
                
                // Fade in as they get closer (reverse of fading out)
                float distance = Vector3.Distance(particles[i].transform.position, targetPositions[i]);
                float alpha = 1f - (distance / spawnRadius);
                
                Renderer renderer = particles[i].GetComponent<Renderer>();
                Color currentColor = renderer.material.color;
                renderer.material.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                
                // When very close, reset position
                if (distance < 0.1f)
                {
                    Vector3 newPos = transform.position + Random.insideUnitSphere * spawnRadius;
                    newPos.z = 0;
                    particles[i].transform.position = newPos;
                }
            }
        }
    }
    
    void OnDestroy()
    {
        if (ReverseTimeManager.Instance != null)
        {
            ReverseTimeManager.Instance.UnregisterObject(this);
        }
        
        // Clean up particles
        if (particles != null)
        {
            foreach (GameObject particle in particles)
            {
                if (particle != null) Destroy(particle);
            }
        }
    }
}