using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreenFader : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 1f;
    public string sceneToLoad = "Seans Level";

    public void StartFadeAndLoad()
    {
        gameObject.SetActive(true); // Ensure the canvas is enabled
        StartCoroutine(FadeInAndLoad());
    }

    IEnumerator FadeInAndLoad()
    {
        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;

        // Optional: pause after fade
        yield return new WaitForSecondsRealtime(0.3f);

        // Load the scene
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneToLoad);
        while (!load.isDone)
        {
            yield return null;
        }
    }
}
