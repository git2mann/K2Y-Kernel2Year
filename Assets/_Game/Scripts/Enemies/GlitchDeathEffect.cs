using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GlitchDeathEffect : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public float flickerDuration = 0.5f;
    public float flickerInterval = 0.05f;
    public AudioClip deathSound; // Drag in your sound here

    private AudioSource audioSource;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
    }

    public void GlitchOutAndDie()
    {
        // Disable movement
        var moveScript = GetComponent<PlayerMovement>();
        if (moveScript != null)
        {
            moveScript.canMove = false;
        }

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Play death sound
        if (audioSource && deathSound)
            audioSource.PlayOneShot(deathSound);

        StartCoroutine(FlickerThenRestart());
    }

    private IEnumerator FlickerThenRestart()
    {
        float timer = 0f;

        while (timer < flickerDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flickerInterval);
            timer += flickerInterval;
        }

        spriteRenderer.enabled = false;

        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
