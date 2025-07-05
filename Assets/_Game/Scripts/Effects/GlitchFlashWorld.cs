using UnityEngine;

public class GlitchFlashWorld : MonoBehaviour
{
    public GameObject glitchPlane;
    public float flashInterval = 30f;
    public float flashDuration = 0.2f;
    private float timer;

    void Start()
    {
        // Hide glitch plane at the beginning
        glitchPlane.SetActive(false);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= flashInterval)
        {
            StartCoroutine(Flash());
            timer = -2f;
        }
    }

    System.Collections.IEnumerator Flash()
    {
        glitchPlane.SetActive(true);
        yield return new WaitForSeconds(flashDuration);
        glitchPlane.SetActive(false);
    }
}
