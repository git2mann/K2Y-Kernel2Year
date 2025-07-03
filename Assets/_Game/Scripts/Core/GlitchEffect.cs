using UnityEngine;
using System.Collections;

public class GlitchEffect : MonoBehaviour
{
    public SpriteRenderer sr;
    public float flickerSpeed = 0.1f;
    public float scaleAmount = 0.2f;
    public float colorShiftAmount = 0.2f;

    void Start()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        StartCoroutine(GlitchLoop());
    }

    IEnumerator GlitchLoop()
    {
        Vector3 originalScale = transform.localScale;
        Color originalColor = sr.color;

        while (true)
        {
            // Random flicker scale
            float flicker = Random.Range(-scaleAmount, scaleAmount);
            transform.localScale = originalScale + new Vector3(flicker, flicker, 0f);

            // Random color shift
            float r = Mathf.Clamp01(originalColor.r + Random.Range(-colorShiftAmount, colorShiftAmount));
            float g = Mathf.Clamp01(originalColor.g + Random.Range(-colorShiftAmount, colorShiftAmount));
            float b = Mathf.Clamp01(originalColor.b + Random.Range(-colorShiftAmount, colorShiftAmount));
            sr.color = new Color(r, g, b, originalColor.a);

            yield return new WaitForSeconds(flickerSpeed);

            // Reset between flickers
            transform.localScale = originalScale;
            sr.color = originalColor;
            yield return new WaitForSeconds(flickerSpeed);
        }
    }
}
