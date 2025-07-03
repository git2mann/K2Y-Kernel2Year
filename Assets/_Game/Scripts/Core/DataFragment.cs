using System;
using UnityEngine;
using System.Collections;

public class DataFragment : MonoBehaviour, IItem
{
    public static event Action<int> OnFragmentCollect;
    public int worth = 5;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player touched fragment");
            Collect();
        }
    }

    public void Collect()
    {
        OnFragmentCollect?.Invoke(worth);
        StartCoroutine(AnimateAndDestroy());
    }

    private IEnumerator AnimateAndDestroy()
    {
        float duration = 0.5f;
        float timer = 0f;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Vector3 originalScale = transform.localScale;

        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // Fade out
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                sr.color = c;
            }

            // Shrink
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            yield return null;
        }

        // Wait for sound to finish if playing
        if (audioSource != null && audioSource.clip != null)
        {
            yield return new WaitForSeconds(audioSource.clip.length - duration);
        }

        Destroy(gameObject);
    }
}
