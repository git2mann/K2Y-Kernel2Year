using UnityEngine;
using UnityEngine.UI;

public class GlitchFlashEffect : MonoBehaviour
{
    public Image glitchImage;
    public float flashInterval = 2f;
    public float flashDuration = 0.1f;
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= flashInterval)
        {
            StartCoroutine(Flash());
            timer = 0f;
        }
    }

    System.Collections.IEnumerator Flash()
    {
        // Make visible
        glitchImage.color = new Color(1f, 1f, 1f, 1f);  // Full opacity
        yield return new WaitForSeconds(flashDuration);
        // Make transparent
        glitchImage.color = new Color(1f, 1f, 1f, 0f);  // Invisible
    }
}
