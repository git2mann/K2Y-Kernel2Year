using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public GameObject loadingCanvas;
    public CanvasGroup canvasGroup;
    public Image glitchFlashImage;
    public float fadeDuration = 1f;
    public string sceneToLoad = "LevelOne";

    public void LoadLevelOne()
    {
        StartCoroutine(FadeInAndLoad());
    }

    IEnumerator FadeInAndLoad()
    {
        loadingCanvas.SetActive(true);

        // Fade in the canvas
        float t = 0f;
        canvasGroup.alpha = 0f;
        while (t < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Wait for glitch effect to build tension
        yield return new WaitForSecondsRealtime(0.4f);

        // Perform final glitch flash
        yield return StartCoroutine(FlashGlitch());

        // Load scene
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneToLoad);
        while (!load.isDone)
        {
            yield return null;
        }
    }

    IEnumerator FlashGlitch()
    {
        float flashIn = 0.05f;
        float flashOut = 0.07f;

        // Flash in
        glitchFlashImage.color = new Color(1, 0.8f, 0.8f, 1);
        yield return new WaitForSecondsRealtime(flashIn);

        // Flash out
        glitchFlashImage.color = new Color(1, 1, 1, 0);
        yield return new WaitForSecondsRealtime(flashOut);
    }
}
