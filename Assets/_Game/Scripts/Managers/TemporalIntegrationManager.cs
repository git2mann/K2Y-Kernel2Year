using UnityEngine;
using System.Collections.Generic;

public class TemporalIntegrationManager : MonoBehaviour
{
    [Header("Time-Responsive Objects")]
    [SerializeField] private List<TemporalUnexplosionEffect> unexplosionEffects = new List<TemporalUnexplosionEffect>();
    [SerializeField] private bool autoFindTemporalObjects = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color reverseTimeColor = Color.red;
    [SerializeField] private Color forwardTimeColor = Color.blue;
    [SerializeField] private Color normalTimeColor = Color.white;
    
    [Header("Player Effects")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private bool enablePlayerTimeEffects = true;
    
    private float currentTimeScale = 1f;
    private Camera mainCamera;
    
    void Start()
    {
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }
        
        if (playerController != null && playerSpriteRenderer == null)
        {
            playerSpriteRenderer = playerController.GetComponent<SpriteRenderer>();
        }
        
        mainCamera = Camera.main ?? FindFirstObjectByType<Camera>();
        
        if (autoFindTemporalObjects)
        {
            FindAllTemporalObjects();
        }
        
        if (TemporalController.Instance != null)
        {
            TemporalController.Instance.OnTimeScaleChanged += OnTimeScaleChanged;
            TemporalController.Instance.OnTimeReversed += OnTimeReversed;
            TemporalController.Instance.OnTimeForward += OnTimeForward;
        }
    }
    
    void OnDestroy()
    {
        if (TemporalController.Instance != null)
        {
            TemporalController.Instance.OnTimeScaleChanged -= OnTimeScaleChanged;
            TemporalController.Instance.OnTimeReversed -= OnTimeReversed;
            TemporalController.Instance.OnTimeForward -= OnTimeForward;
        }
    }
    
    void FindAllTemporalObjects()
    {
        TemporalUnexplosionEffect[] allEffects = FindObjectsByType<TemporalUnexplosionEffect>(FindObjectsSortMode.None);
        unexplosionEffects.AddRange(allEffects);
        Debug.Log($"Found {allEffects.Length} temporal unexplosion effects in scene");
    }
    
    void OnTimeScaleChanged(float timeScale)
    {
        currentTimeScale = timeScale;
        
        if (enablePlayerTimeEffects)
        {
            UpdatePlayerTimeEffects();
        }
    }
    
    void OnTimeReversed()
    {
        Debug.Log("Time flow reversed! Objects will unexplode.");
        StartCoroutine(TimeDirectionFlash(reverseTimeColor));
    }
    
    void OnTimeForward()
    {
        Debug.Log("Time flow restored! Objects will explode normally.");
        StartCoroutine(TimeDirectionFlash(forwardTimeColor));
    }
    
    void UpdatePlayerTimeEffects()
    {
        if (playerSpriteRenderer == null) return;
        
        Color targetColor = normalTimeColor;
        
        if (Mathf.Abs(currentTimeScale) > 0.5f)
        {
            if (currentTimeScale < 0)
            {
                float intensity = Mathf.Abs(currentTimeScale) / 2f;
                targetColor = Color.Lerp(normalTimeColor, reverseTimeColor, intensity * 0.3f);
            }
            else
            {
                float intensity = currentTimeScale / 2f;
                targetColor = Color.Lerp(normalTimeColor, forwardTimeColor, intensity * 0.3f);
            }
        }
        
        playerSpriteRenderer.color = Color.Lerp(playerSpriteRenderer.color, targetColor, Time.unscaledDeltaTime * 5f);
    }
    
    System.Collections.IEnumerator TimeDirectionFlash(Color flashColor)
    {
        if (mainCamera == null) yield break;
        
        Color originalColor = mainCamera.backgroundColor;
        
        mainCamera.backgroundColor = Color.Lerp(originalColor, flashColor, 0.5f);
        yield return new WaitForSecondsRealtime(0.1f);
        
        float fadeTime = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeTime;
            mainCamera.backgroundColor = Color.Lerp(Color.Lerp(originalColor, flashColor, 0.5f), originalColor, t);
            yield return null;
        }
        
        mainCamera.backgroundColor = originalColor;
    }
    
    [ContextMenu("Test Time Reversal")]
    public void TestTimeReversal()
    {
        if (TemporalController.Instance != null)
        {
            TemporalController.Instance.SetTimeScaleOverride(-1.5f, 3f);
        }
    }
}