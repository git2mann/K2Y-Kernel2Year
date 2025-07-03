using UnityEngine;

public class SimpleParticleSpawner : MonoBehaviour
{
    [Header("Particle Settings")]
    public GameObject particlePrefab;
    public int particleCount = 6;
    public float kickForce = 3f;
    public float upwardBias = 0.5f;
    
    public void SpawnJumpParticles()
    {
        Debug.Log("Spawning jump particles from ground impact!");
        
        if (particlePrefab == null)
        {
            Debug.LogError("No particle prefab assigned!");
            return;
        }
        
        // Find the ground position (where player's feet are)
        Vector3 groundPos = transform.position + Vector3.down * 0.6f;
        
        for (int i = 0; i < particleCount; i++)
        {
            // Create particle at ground level
            GameObject particle = Instantiate(particlePrefab);
            
            // Small random spread around ground point
            Vector3 spawnPos = groundPos + (Vector3)Random.insideUnitCircle * 0.2f;
            spawnPos.z = 0;
            particle.transform.position = spawnPos;
            
            // Kick particles outward and slightly up (like dust being kicked up)
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) + upwardBias);
            direction = direction.normalized;
            
            Rigidbody2D rb = particle.GetComponent<Rigidbody2D>();
            rb.AddForce(direction * kickForce, ForceMode2D.Impulse);
            
            // Make particles smaller over time and disappear
            StartCoroutine(FadeParticle(particle));
        }
    }

    public void SpawnDoubleJumpParticles()
    {
        Debug.Log("Spawning double jump particles in mid-air!");
        if (particlePrefab == null)
        {
            Debug.LogError("No particle prefab assigned!");
            return;
        }

        // Spawn particles around player position (mid-air for double jump)
        Vector3 playerPos = transform.position;
        
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = Instantiate(particlePrefab);
            
            // Small random spread around player
            Vector3 spawnPos = playerPos + (Vector3)Random.insideUnitCircle * 0.3f;
            spawnPos.z = 0;
            particle.transform.position = spawnPos;

            // For double jump, particles burst outward in all directions
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            direction = direction.normalized;

            Rigidbody2D rb = particle.GetComponent<Rigidbody2D>();
            rb.AddForce(direction * kickForce * 1.2f, ForceMode2D.Impulse); // Slightly more force for double jump

            StartCoroutine(FadeParticle(particle));
        }
    }


    
    // Original FadeParticle method (was missing!)
    System.Collections.IEnumerator FadeParticle(GameObject particle)
    {
        SpriteRenderer sr = particle.GetComponent<SpriteRenderer>();
        Vector3 originalScale = particle.transform.localScale;
        Color originalColor = sr.color;
        float lifetime = 1.5f;
        float elapsed = 0f;
        
        while (elapsed < lifetime && particle != null)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / lifetime;
            
            // Shrink particle over time
            particle.transform.localScale = originalScale * (1f - progress);
            
            // Fade out
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - progress);
            
            yield return null;
        }
        
        if (particle != null)
        {
            Destroy(particle);
        }
    }

}