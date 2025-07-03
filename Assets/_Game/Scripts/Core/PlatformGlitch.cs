using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GlitchEffectGroup : MonoBehaviour
{
    public List<SpriteRenderer> renderers = new List<SpriteRenderer>();
    public float flickerSpeed = 0.1f;
    public float scaleAmount = 0.05f;
    public float colorShiftAmount = 0.2f;

    Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;

        if (renderers.Count == 0)
            renderers.AddRange(GetComponentsInChildren<SpriteRenderer>());

        StartCoroutine(GlitchLoop());
    }

    IEnumerator GlitchLoop()
    {
        while (true)
        {
            float flicker = Random.Range(-scaleAmount, scaleAmount);
            transform.localScale = originalScale + new Vector3(flicker, flicker, 0f);

            foreach (var sr in renderers)
            {
                if (sr == null) continue;

                Color baseColor = sr.color;
                float r = Mathf.Clamp01(baseColor.r + Random.Range(-colorShiftAmount, colorShiftAmount));
                float g = Mathf.Clamp01(baseColor.g + Random.Range(-colorShiftAmount, colorShiftAmount));
                float b = Mathf.Clamp01(baseColor.b + Random.Range(-colorShiftAmount, colorShiftAmount));
                sr.color = new Color(r, g, b, baseColor.a);
            }

            yield return new WaitForSeconds(flickerSpeed);

            transform.localScale = originalScale;

            foreach (var sr in renderers)
            {
                if (sr == null) continue;
                sr.color = Color.white;
            }

            yield return new WaitForSeconds(flickerSpeed);
        }
    }
}
