using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GlitchDeathEffect : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public float flickerDuration = 0.5f;
    public float flickerInterval = 0.05f;

    private Rigidbody2D rb; // 🟢 FIX: Declare rb here

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>(); // 🟢 Get the Rigidbody2D early
    }

    public void GlitchOutAndDie()
    {
        // Freeze movement
        var moveScript = GetComponent<PlayerMovement>();
        if (moveScript != null)
        {
            moveScript.canMove = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; // freeze physics too
        }

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
