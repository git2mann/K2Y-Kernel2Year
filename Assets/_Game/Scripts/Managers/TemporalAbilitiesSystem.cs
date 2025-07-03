using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Audio-Safe Y2K Temporal Abilities System - No FMOD conflicts
/// Works with any Unity project that has a Player tagged GameObject
/// </summary>
public class TemporalAbilitiesSystem : MonoBehaviour
{
    [Header("Rewind Abilities")]
    [SerializeField] private KeyCode rewindKey = KeyCode.R;
    [SerializeField] private KeyCode quickRewindKey = KeyCode.Q;
    [SerializeField] private float rewindCooldown = 3f;
    [SerializeField] private int maxRewinds = 3;
    
    [Header("Temporal Anchor")]
    [SerializeField] private KeyCode setAnchorKey = KeyCode.X;
    [SerializeField] private KeyCode snapToAnchorKey = KeyCode.Z;
    [SerializeField] private GameObject anchorMarkerPrefab;
    
    [Header("Time Echo")]
    [SerializeField] private KeyCode timeEchoKey = KeyCode.E;
    [SerializeField] private float echoVisualizationTime = 5f;
    [SerializeField] private GameObject echoTrailPrefab;
    [SerializeField] private Color echoTrailColor = Color.yellow;
    
    [Header("Temporal Burst")]
    [SerializeField] private KeyCode burstKey = KeyCode.F;
    [SerializeField] private float burstDuration = 1f;
    [SerializeField] private float burstTimeMultiplier = 3f;
    [SerializeField] private ParticleSystem burstEffect;
    
    [Header("Ghost Projection")]
    [SerializeField] private KeyCode projectionKey = KeyCode.G;
    [SerializeField] private float projectionDuration = 3f;
    [SerializeField] private bool ghostCanActivateSwitches = true;
    [SerializeField] private LayerMask projectionInteractionLayers = 1;
    
    [Header("Y2K System Resources")]
    [SerializeField] private int systemMemory = 100;
    [SerializeField] private int rewindCost = 30;
    [SerializeField] private int anchorCost = 20;
    [SerializeField] private int echoCost = 15;
    [SerializeField] private int burstCost = 25;
    [SerializeField] private int projectionCost = 40;
    [SerializeField] private float memoryRegenerationRate = 10f;
    
    [Header("Audio Settings")]
    [SerializeField] private bool enableAudio = false; // DISABLED by default to avoid FMOD conflicts
    [SerializeField] private bool useFMOD = false;
    [SerializeField] private AudioSource existingAudioSource; // Use existing AudioSource instead of creating new one
    [SerializeField] private AudioClip[] rewindSounds;
    [SerializeField] private AudioClip[] errorSounds;
    [SerializeField] private AudioClip[] y2kSystemSounds;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool enableScreenEffects = true;
    [SerializeField] private Color rewindScreenTint = new Color(0.3f, 0.3f, 1f, 0.3f);
    
    // Private variables
    private Transform playerTransform;
    private List<TemporalSnapshot> timelineHistory = new List<TemporalSnapshot>();
    private Vector3? anchorPosition = null;
    private int currentSystemMemory;
    private float lastRewindTime;
    private bool isPerformingTemporalAction = false;
    private int remainingRewinds;
    
    // Ghost projection
    private GameObject projectedGhost;
    private bool isProjectionActive = false;
    
    // UI elements
    private Canvas temporalUI;
    private UnityEngine.UI.Text memoryDisplay;
    private UnityEngine.UI.Text rewindCountDisplay;
    private UnityEngine.UI.Text systemStatusDisplay;
    private UnityEngine.UI.Image memoryBarFill;
    
    [System.Serializable]
    public class TemporalSnapshot
    {
        public Vector3 position;
        public Vector3 velocity;
        public float timestamp;
        public bool facingRight;
        public int playerState;
        
        public TemporalSnapshot(Vector3 pos, Vector3 vel, float time, bool facing, int state)
        {
            position = pos;
            velocity = vel;
            timestamp = time;
            facingRight = facing;
            playerState = state;
        }
    }
    
    void Start()
    {
        InitializeComponents();
        CreateY2KStyleUI();
        
        currentSystemMemory = systemMemory;
        remainingRewinds = maxRewinds;
        
        if (playerTransform != null)
        {
            StartCoroutine(RecordTimeline());
        }
        
        StartCoroutine(Y2KGlitchEffects());
        
        // Audio warning
        if (!enableAudio)
        {
            Debug.Log("Y2K Temporal Abilities: Audio disabled to prevent FMOD conflicts. Enable in inspector if needed.");
        }
    }
    
    void InitializeComponents()
    {
        // Find player components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("No Player tagged GameObject found! Please tag your player GameObject with 'Player'.");
        }
    }
    
    void CreateY2KStyleUI()
    {
        // Create main UI canvas
        GameObject uiObj = new GameObject("K2Y_TemporalUI");
        temporalUI = uiObj.AddComponent<Canvas>();
        temporalUI.renderMode = RenderMode.ScreenSpaceOverlay;
        temporalUI.sortingOrder = 100;
        
        CreateUIPanel();
    }
    
    void CreateUIPanel()
    {
        // Y2K-style terminal background panel
        GameObject panelObj = new GameObject("Y2K_SystemPanel");
        panelObj.transform.SetParent(temporalUI.transform, false);
        UnityEngine.UI.Image panelImage = panelObj.AddComponent<UnityEngine.UI.Image>();
        panelImage.color = new Color(0f, 0f, 0.2f, 0.8f);
        
        RectTransform panelRect = panelImage.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.65f, 0.7f);
        panelRect.anchorMax = new Vector2(0.98f, 0.98f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        CreateMemoryDisplay(panelObj);
        CreateRewindDisplay(panelObj);
        CreateSystemStatusDisplay(panelObj);
    }
    
    void CreateMemoryDisplay(GameObject parent)
    {
        // Memory text
        GameObject memoryObj = new GameObject("MemoryDisplay");
        memoryObj.transform.SetParent(parent.transform, false);
        memoryDisplay = memoryObj.AddComponent<UnityEngine.UI.Text>();
        memoryDisplay.font = GetSafeFont();
        memoryDisplay.fontSize = 12;
        memoryDisplay.color = Color.cyan;
        memoryDisplay.text = $"RAM: {currentSystemMemory}MB";
        
        RectTransform memoryRect = memoryDisplay.GetComponent<RectTransform>();
        memoryRect.anchorMin = new Vector2(0.05f, 0.7f);
        memoryRect.anchorMax = new Vector2(0.95f, 0.9f);
        memoryRect.offsetMin = Vector2.zero;
        memoryRect.offsetMax = Vector2.zero;
        
        // Memory bar background
        GameObject barBgObj = new GameObject("MemoryBarBG");
        barBgObj.transform.SetParent(parent.transform, false);
        UnityEngine.UI.Image barBg = barBgObj.AddComponent<UnityEngine.UI.Image>();
        barBg.color = Color.black;
        
        RectTransform barBgRect = barBg.GetComponent<RectTransform>();
        barBgRect.anchorMin = new Vector2(0.05f, 0.5f);
        barBgRect.anchorMax = new Vector2(0.95f, 0.65f);
        barBgRect.offsetMin = Vector2.zero;
        barBgRect.offsetMax = Vector2.zero;
        
        // Memory bar fill
        GameObject barFillObj = new GameObject("MemoryBarFill");
        barFillObj.transform.SetParent(barBgObj.transform, false);
        memoryBarFill = barFillObj.AddComponent<UnityEngine.UI.Image>();
        memoryBarFill.color = Color.green;
        memoryBarFill.type = UnityEngine.UI.Image.Type.Filled;
        memoryBarFill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        
        RectTransform barFillRect = memoryBarFill.GetComponent<RectTransform>();
        barFillRect.anchorMin = Vector2.zero;
        barFillRect.anchorMax = Vector2.one;
        barFillRect.offsetMin = Vector2.zero;
        barFillRect.offsetMax = Vector2.zero;
    }
    
    void CreateRewindDisplay(GameObject parent)
    {
        GameObject rewindObj = new GameObject("RewindDisplay");
        rewindObj.transform.SetParent(parent.transform, false);
        rewindCountDisplay = rewindObj.AddComponent<UnityEngine.UI.Text>();
        rewindCountDisplay.font = GetSafeFont();
        rewindCountDisplay.fontSize = 12;
        rewindCountDisplay.color = Color.yellow;
        rewindCountDisplay.text = $"CYCLES: {remainingRewinds}";
        
        RectTransform rewindRect = rewindCountDisplay.GetComponent<RectTransform>();
        rewindRect.anchorMin = new Vector2(0.05f, 0.3f);
        rewindRect.anchorMax = new Vector2(0.95f, 0.45f);
        rewindRect.offsetMin = Vector2.zero;
        rewindRect.offsetMax = Vector2.zero;
    }
    
    void CreateSystemStatusDisplay(GameObject parent)
    {
        GameObject statusObj = new GameObject("SystemStatus");
        statusObj.transform.SetParent(parent.transform, false);
        systemStatusDisplay = statusObj.AddComponent<UnityEngine.UI.Text>();
        systemStatusDisplay.font = GetSafeFont();
        systemStatusDisplay.fontSize = 10;
        systemStatusDisplay.color = Color.green;
        systemStatusDisplay.text = "KERNEL: NORMAL";
        
        RectTransform statusRect = systemStatusDisplay.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.05f, 0.05f);
        statusRect.anchorMax = new Vector2(0.95f, 0.25f);
        statusRect.offsetMin = Vector2.zero;
        statusRect.offsetMax = Vector2.zero;
    }
    
    Font GetSafeFont()
    {
        try
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch
        {
            try
            {
                return Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            catch
            {
                Debug.LogWarning("No built-in font found! Using default font.");
                return Resources.GetBuiltinResource<Font>("ui");
            }
        }
    }
    
    IEnumerator Y2KGlitchEffects()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(8f, 20f));
            
            if (memoryDisplay != null && Random.value < 0.2f)
            {
                Color originalColor = memoryDisplay.color;
                string originalText = memoryDisplay.text;
                
                memoryDisplay.color = Color.red;
                memoryDisplay.text = "ERROR: LEAK DETECTED";
                
                yield return new WaitForSeconds(0.3f);
                
                memoryDisplay.color = originalColor;
                memoryDisplay.text = originalText;
            }
        }
    }
    
    IEnumerator RecordTimeline()
    {
        while (true)
        {
            if (playerTransform != null)
            {
                Rigidbody2D playerRB = playerTransform.GetComponent<Rigidbody2D>();
                SpriteRenderer playerSR = playerTransform.GetComponent<SpriteRenderer>();
                
                Vector3 velocity = playerRB?.linearVelocity ?? Vector3.zero;
                bool facingRight = playerSR ? !playerSR.flipX : true;
                int state = GetPlayerState(playerRB);
                
                TemporalSnapshot snapshot = new TemporalSnapshot(
                    playerTransform.position,
                    velocity,
                    Time.unscaledTime,
                    facingRight,
                    state
                );
                
                timelineHistory.Add(snapshot);
                
                // Limit history size
                while (timelineHistory.Count > 600)
                {
                    timelineHistory.RemoveAt(0);
                }
            }
            
            yield return new WaitForFixedUpdate();
        }
    }
    
    int GetPlayerState(Rigidbody2D playerRB)
    {
        if (playerRB == null) return 0;
        
        float velocityY = playerRB.linearVelocity.y;
        float velocityX = Mathf.Abs(playerRB.linearVelocity.x);
        
        if (velocityY > 1f) return 1; // Jumping
        if (velocityY < -1f) return 2; // Falling
        if (velocityX > 0.1f) return 4; // Moving
        return 0; // Idle
    }
    
    void Update()
    {
        HandleTemporalInput();
        UpdateUI();
        RegenerateSystemMemory();
    }
    
    void HandleTemporalInput()
    {
        if (isPerformingTemporalAction) return;
        
        if (Input.GetKeyDown(rewindKey))
        {
            PerformTimeRewind(3f);
        }
        else if (Input.GetKeyDown(quickRewindKey))
        {
            PerformTimeRewind(1f);
        }
        else if (Input.GetKeyDown(setAnchorKey))
        {
            SetTemporalAnchor();
        }
        else if (Input.GetKeyDown(snapToAnchorKey))
        {
            SnapToAnchor();
        }
        else if (Input.GetKeyDown(timeEchoKey))
        {
            ShowTimeEcho();
        }
        else if (Input.GetKeyDown(burstKey))
        {
            PerformTemporalBurst();
        }
        else if (Input.GetKeyDown(projectionKey))
        {
            ToggleGhostProjection();
        }
    }
    
    void PerformTimeRewind(float rewindSeconds)
    {
        if (!CanUseAbility(rewindCost) || remainingRewinds <= 0)
        {
            PlayY2KErrorSound();
            return;
        }
        
        if (Time.unscaledTime - lastRewindTime < rewindCooldown)
        {
            PlayY2KErrorSound();
            return;
        }
        
        TemporalSnapshot targetSnapshot = GetSnapshotAtTime(Time.unscaledTime - rewindSeconds);
        if (targetSnapshot == null)
        {
            PlayY2KErrorSound();
            return;
        }
        
        StartCoroutine(ExecuteRewind(targetSnapshot));
    }
    
    IEnumerator ExecuteRewind(TemporalSnapshot target)
    {
        isPerformingTemporalAction = true;
        
        StartCoroutine(Y2KRewindScreenEffect());
        PlayY2KSystemSound();
        
        // Teleport to past position
        playerTransform.position = target.position;
        
        // Restore velocity and state
        Rigidbody2D playerRB = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRB != null)
        {
            playerRB.linearVelocity = target.velocity;
        }
        
        // Restore facing direction
        SpriteRenderer playerSR = playerTransform.GetComponent<SpriteRenderer>();
        if (playerSR != null)
        {
            playerSR.flipX = !target.facingRight;
        }
        
        // Consume resources
        ConsumeSystemMemory(rewindCost);
        remainingRewinds--;
        lastRewindTime = Time.unscaledTime;
        
        yield return new WaitForSeconds(0.2f);
        isPerformingTemporalAction = false;
        
        Debug.Log($"Y2K Temporal Rewind: {Time.unscaledTime - target.timestamp:F1} seconds ago");
    }
    
    void SetTemporalAnchor()
    {
        if (!CanUseAbility(anchorCost))
        {
            PlayY2KErrorSound();
            return;
        }
        
        anchorPosition = playerTransform.position;
        ConsumeSystemMemory(anchorCost);
        
        // Visual marker
        if (anchorMarkerPrefab != null)
        {
            GameObject marker = Instantiate(anchorMarkerPrefab, anchorPosition.Value, Quaternion.identity);
            StartCoroutine(Y2KAnchorPulse(marker));
        }
        else
        {
            // Create simple marker if no prefab assigned
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.transform.position = anchorPosition.Value;
            marker.transform.localScale = Vector3.one * 0.5f;
            marker.GetComponent<Renderer>().material.color = Color.cyan;
            StartCoroutine(Y2KAnchorPulse(marker));
        }
        
        PlayY2KSystemSound();
        Debug.Log("Y2K Temporal Anchor Set");
    }
    
    void SnapToAnchor()
    {
        if (anchorPosition == null)
        {
            PlayY2KErrorSound();
            return;
        }
        
        playerTransform.position = anchorPosition.Value;
        
        Rigidbody2D playerRB = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRB != null)
        {
            playerRB.linearVelocity = Vector2.zero;
        }
        
        PlayY2KSystemSound();
        Debug.Log("Y2K System Restore Complete");
    }
    
    void ShowTimeEcho()
    {
        if (!CanUseAbility(echoCost))
        {
            PlayY2KErrorSound();
            return;
        }
        
        StartCoroutine(Y2KTimeEchoVisualization());
        ConsumeSystemMemory(echoCost);
    }
    
    IEnumerator Y2KTimeEchoVisualization()
    {
        List<GameObject> echoMarkers = new List<GameObject>();
        
        float currentTime = Time.unscaledTime;
        for (float t = 0; t < echoVisualizationTime; t += 0.2f)
        {
            TemporalSnapshot snapshot = GetSnapshotAtTime(currentTime - t);
            if (snapshot != null)
            {
                GameObject marker;
                
                if (echoTrailPrefab != null)
                {
                    marker = Instantiate(echoTrailPrefab, snapshot.position, Quaternion.identity);
                }
                else
                {
                    // Create simple marker if no prefab assigned
                    marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    marker.transform.position = snapshot.position;
                    marker.transform.localScale = Vector3.one * 0.2f;
                    Destroy(marker.GetComponent<Collider>());
                }
                
                Renderer renderer = marker.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color color = echoTrailColor;
                    color.a = 1f - (t / echoVisualizationTime);
                    renderer.material.color = color;
                }
                
                echoMarkers.Add(marker);
            }
        }
        
        PlayY2KSystemSound();
        yield return new WaitForSeconds(3f);
        
        foreach (GameObject marker in echoMarkers)
        {
            if (marker != null) 
            {
                StartCoroutine(GlitchDestroy(marker));
            }
        }
    }
    
    void PerformTemporalBurst()
    {
        if (!CanUseAbility(burstCost))
        {
            PlayY2KErrorSound();
            return;
        }
        
        StartCoroutine(ExecuteY2KTemporalBurst());
        ConsumeSystemMemory(burstCost);
    }
    
    IEnumerator ExecuteY2KTemporalBurst()
    {
        // Simple time acceleration effect
        float originalTimeScale = Time.timeScale;
        Time.timeScale = burstTimeMultiplier;
        
        if (burstEffect != null)
        {
            burstEffect.Play();
        }
        
        PlayY2KSystemSound();
        yield return new WaitForSecondsRealtime(burstDuration);
        
        Time.timeScale = originalTimeScale;
        Debug.Log("Y2K Temporal Burst Complete");
    }
    
    void ToggleGhostProjection()
    {
        if (isProjectionActive)
        {
            EndGhostProjection();
        }
        else
        {
            StartGhostProjection();
        }
    }
    
    void StartGhostProjection()
    {
        if (!CanUseAbility(projectionCost))
        {
            PlayY2KErrorSound();
            return;
        }
        
        projectedGhost = new GameObject("Y2K_Ghost_Process");
        SpriteRenderer playerSR = playerTransform.GetComponent<SpriteRenderer>();
        if (playerSR != null)
        {
            SpriteRenderer ghostSR = projectedGhost.AddComponent<SpriteRenderer>();
            ghostSR.sprite = playerSR.sprite;
            ghostSR.color = new Color(0f, 1f, 1f, 0.6f);
            ghostSR.sortingOrder = playerSR.sortingOrder + 1;
        }
        
        projectedGhost.transform.position = playerTransform.position;
        projectedGhost.transform.localScale = playerTransform.localScale * 1.1f;
        
        if (ghostCanActivateSwitches)
        {
            BoxCollider2D ghostCollider = projectedGhost.AddComponent<BoxCollider2D>();
            ghostCollider.isTrigger = true;
            
            GhostSafeInteraction interaction = projectedGhost.AddComponent<GhostSafeInteraction>();
            interaction.Initialize(this);
        }
        
        isProjectionActive = true;
        ConsumeSystemMemory(projectionCost);
        
        StartCoroutine(Y2KProjectionLifetime());
        Debug.Log("Y2K Ghost Process Spawned");
    }
    
    IEnumerator Y2KProjectionLifetime()
    {
        yield return new WaitForSeconds(projectionDuration);
        EndGhostProjection();
    }
    
    void EndGhostProjection()
    {
        if (projectedGhost != null)
        {
            StartCoroutine(GlitchDestroy(projectedGhost));
            projectedGhost = null;
        }
        isProjectionActive = false;
    }
    
    IEnumerator GlitchDestroy(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            for (int i = 0; i < 5; i++)
            {
                renderer.enabled = !renderer.enabled;
                yield return new WaitForSeconds(0.05f);
            }
        }
        Destroy(obj);
    }
    
    TemporalSnapshot GetSnapshotAtTime(float targetTime)
    {
        TemporalSnapshot closest = null;
        float closestTimeDiff = float.MaxValue;
        
        foreach (TemporalSnapshot snapshot in timelineHistory)
        {
            float timeDiff = Mathf.Abs(snapshot.timestamp - targetTime);
            if (timeDiff < closestTimeDiff)
            {
                closest = snapshot;
                closestTimeDiff = timeDiff;
            }
        }
        
        return closest;
    }
    
    bool CanUseAbility(int cost)
    {
        return currentSystemMemory >= cost;
    }
    
    void ConsumeSystemMemory(int amount)
    {
        currentSystemMemory = Mathf.Max(0, currentSystemMemory - amount);
    }
    
    void RegenerateSystemMemory()
    {
        if (currentSystemMemory < systemMemory)
        {
            currentSystemMemory += Mathf.RoundToInt(Time.deltaTime * memoryRegenerationRate);
            currentSystemMemory = Mathf.Min(systemMemory, currentSystemMemory);
        }
    }
    
    void UpdateUI()
    {
        UpdateMemoryDisplay();
        UpdateRewindDisplay();
    }
    
    void UpdateMemoryDisplay()
    {
        if (memoryDisplay != null)
        {
            int percentage = Mathf.RoundToInt((float)currentSystemMemory / systemMemory * 100);
            memoryDisplay.text = $"RAM: {percentage}%";
            
            if (percentage > 60) memoryDisplay.color = Color.cyan;
            else if (percentage > 30) memoryDisplay.color = Color.yellow;
            else memoryDisplay.color = Color.red;
        }
        
        if (memoryBarFill != null)
        {
            float fillAmount = (float)currentSystemMemory / systemMemory;
            memoryBarFill.fillAmount = fillAmount;
            
            if (fillAmount > 0.6f) memoryBarFill.color = Color.green;
            else if (fillAmount > 0.3f) memoryBarFill.color = Color.yellow;
            else memoryBarFill.color = Color.red;
        }
    }
    
    void UpdateRewindDisplay()
    {
        if (rewindCountDisplay != null)
        {
            rewindCountDisplay.text = $"CYCLES: {remainingRewinds}";
            rewindCountDisplay.color = remainingRewinds > 0 ? Color.yellow : Color.red;
        }
    }
    
    IEnumerator Y2KRewindScreenEffect()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null && enableScreenEffects)
        {
            Color originalColor = mainCam.backgroundColor;
            
            for (int i = 0; i < 3; i++)
            {
                mainCam.backgroundColor = Color.Lerp(originalColor, rewindScreenTint, 0.7f);
                yield return new WaitForSeconds(0.05f);
                mainCam.backgroundColor = originalColor;
                yield return new WaitForSeconds(0.05f);
            }
            
            mainCam.backgroundColor = Color.Lerp(originalColor, rewindScreenTint, 0.5f);
            yield return new WaitForSeconds(0.2f);
            mainCam.backgroundColor = originalColor;
        }
    }
    
    IEnumerator Y2KAnchorPulse(GameObject marker)
    {
        Vector3 originalScale = marker.transform.localScale;
        Renderer renderer = marker.GetComponent<Renderer>();
        
        while (marker != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 3f) * 0.2f;
            marker.transform.localScale = originalScale * pulse;
            
            if (renderer != null)
            {
                float colorCycle = (Mathf.Sin(Time.time * 2f) + 1f) * 0.5f;
                renderer.material.color = Color.Lerp(Color.cyan, Color.yellow, colorCycle);
            }
            
            yield return null;
        }
    }
    
    // SAFE AUDIO METHODS - No FMOD conflicts
    void PlayY2KSystemSound()
    {
        if (!enableAudio) return;
        
        if (useFMOD)
        {
            PlayFMODSoundSafely("Y2K_System_Sound");
        }
        else if (existingAudioSource != null)
        {
            PlayAudioClipSafely(GetSystemSound());
        }
        else
        {
            Debug.Log("Y2K System Sound: *BEEP*"); // Console feedback when audio disabled
        }
    }
    
    void PlayY2KErrorSound()
    {
        if (!enableAudio)
        {
            Debug.Log("Y2K Error Sound: *ERROR BEEP*"); // Console feedback when audio disabled
            StartCoroutine(Y2KErrorFlash());
            return;
        }
        
        if (useFMOD)
        {
            PlayFMODSoundSafely("Y2K_Error_Sound");
        }
        else if (existingAudioSource != null)
        {
            PlayAudioClipSafely(GetErrorSound());
        }
        
        StartCoroutine(Y2KErrorFlash());
    }
    
    AudioClip GetSystemSound()
    {
        if (y2kSystemSounds.Length > 0)
        {
            return y2kSystemSounds[Random.Range(0, y2kSystemSounds.Length)];
        }
        else if (rewindSounds.Length > 0)
        {
            return rewindSounds[Random.Range(0, rewindSounds.Length)];
        }
        return null;
    }
    
    AudioClip GetErrorSound()
    {
        if (errorSounds.Length > 0)
        {
            return errorSounds[Random.Range(0, errorSounds.Length)];
        }
        return null;
    }
    
    void PlayAudioClipSafely(AudioClip clip)
    {
        try
        {
            if (clip != null && existingAudioSource != null)
            {
                existingAudioSource.PlayOneShot(clip);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Y2K Audio Error: {e.Message}");
        }
    }
    
    void PlayFMODSoundSafely(string eventName)
    {
        try
        {
            // Safe FMOD integration using reflection
            System.Type runtimeManagerType = System.Type.GetType("FMODUnity.RuntimeManager");
            if (runtimeManagerType != null)
            {
                var playOneShotMethod = runtimeManagerType.GetMethod("PlayOneShot", new System.Type[] { typeof(string) });
                if (playOneShotMethod != null)
                {
                    playOneShotMethod.Invoke(null, new object[] { $"event:/{eventName}" });
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Y2K FMOD Error: {e.Message}");
        }
    }
    
    IEnumerator Y2KErrorFlash()
    {
        if (memoryDisplay != null)
        {
            Color originalColor = memoryDisplay.color;
            
            for (int i = 0; i < 3; i++)
            {
                memoryDisplay.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                memoryDisplay.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    
    // Ghost interaction handling
    public void OnGhostTriggerEnter(Collider2D other)
    {
        if (!ghostCanActivateSwitches || !isProjectionActive) return;
        
        if (IsSwitchObject(other.gameObject))
        {
            ActivateSwitch(other.gameObject);
        }
    }
    
    bool IsSwitchObject(GameObject obj)
    {
        // Safe switch detection using multiple methods
        string[] switchTags = { "Switch", "Button", "Lever", "Activator" };
        
        // Method 1: Check tags safely
        foreach (string tag in switchTags)
        {
            if (HasTag(obj, tag))
            {
                return true;
            }
        }
        
        // Method 2: Check layer
        if (obj.layer == LayerMask.NameToLayer("Interactive"))
        {
            return true;
        }
        
        // Method 3: Check for switch components (safe checking)
        if (HasComponentSafely(obj, "SwitchController") ||
            HasComponentSafely(obj, "Button") ||
            HasComponentSafely(obj, "Lever") ||
            HasComponentSafely(obj, "Activator"))
        {
            return true;
        }
        
        // Method 4: Check name contains switch-related keywords
        string objName = obj.name.ToLower();
        if (objName.Contains("switch") || objName.Contains("button") || 
            objName.Contains("lever") || objName.Contains("activator"))
        {
            return true;
        }
        
        return false;
    }
    
    bool HasComponentSafely(GameObject obj, string componentName)
    {
        try
        {
            Component component = obj.GetComponent(componentName);
            return component != null;
        }
        catch
        {
            return false;
        }
    }
    
    bool HasTag(GameObject obj, string tagName)
    {
        try
        {
            return obj.CompareTag(tagName);
        }
        catch (UnityException)
        {
            return false;
        }
    }
    
    void ActivateSwitch(GameObject switchObj)
    {
        // Try different activation methods
        switchObj.SendMessage("Activate", SendMessageOptions.DontRequireReceiver);
        switchObj.SendMessage("OnActivate", SendMessageOptions.DontRequireReceiver);
        switchObj.SendMessage("Toggle", SendMessageOptions.DontRequireReceiver);
        switchObj.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
        switchObj.SendMessage("Press", SendMessageOptions.DontRequireReceiver);
        
        // Try animator trigger
        Animator animator = switchObj.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Activate");
            animator.SetBool("IsActivated", true);
        }
        
        // Try to activate any switch-like components safely
        ActivateComponentSafely(switchObj, "SwitchController");
        ActivateComponentSafely(switchObj, "Button");
        ActivateComponentSafely(switchObj, "Lever");
        
        Debug.Log($"Y2K Ghost activated: {switchObj.name}");
    }
    
    void ActivateComponentSafely(GameObject obj, string componentName)
    {
        try
        {
            Component component = obj.GetComponent(componentName);
            if (component != null)
            {
                // Try common activation method names
                component.SendMessage("Activate", SendMessageOptions.DontRequireReceiver);
                component.SendMessage("OnActivate", SendMessageOptions.DontRequireReceiver);
                component.SendMessage("Toggle", SendMessageOptions.DontRequireReceiver);
            }
        }
        catch
        {
            // Silently fail if component doesn't exist or methods aren't available
        }
    }
    
    // Public interface for external systems
    public bool HasMemoryForAbility(int cost)
    {
        return CanUseAbility(cost);
    }
    
    public void RestoreMemory(int amount)
    {
        currentSystemMemory = Mathf.Min(systemMemory, currentSystemMemory + amount);
    }
    
    public void AddRewindCharges(int charges)
    {
        remainingRewinds = Mathf.Min(maxRewinds, remainingRewinds + charges);
    }
    
    public int GetCurrentMemory()
    {
        return currentSystemMemory;
    }
    
    public int GetRemainingRewinds()
    {
        return remainingRewinds;
    }
    
    public bool IsPerformingTemporalAction()
    {
        return isPerformingTemporalAction;
    }
    
    // Methods for external triggers
    public void TriggerRewind(float seconds = 3f)
    {
        if (!isPerformingTemporalAction)
        {
            PerformTimeRewind(seconds);
        }
    }
    
    public void TriggerAnchorSet()
    {
        if (!isPerformingTemporalAction)
        {
            SetTemporalAnchor();
        }
    }
    
    public void TriggerAnchorSnap()
    {
        if (!isPerformingTemporalAction)
        {
            SnapToAnchor();
        }
    }
    
    public bool HasAnchor()
    {
        return anchorPosition.HasValue;
    }
    
    // Ability unlock system
    public void UnlockY2KAbility(string abilityName)
    {
        StartCoroutine(Y2KAbilityUnlockSequence(abilityName));
    }
    
    IEnumerator Y2KAbilityUnlockSequence(string abilityName)
    {
        GameObject unlockObj = new GameObject("Y2K_Unlock_Notification");
        unlockObj.transform.SetParent(temporalUI.transform, false);
        
        UnityEngine.UI.Text unlockText = unlockObj.AddComponent<UnityEngine.UI.Text>();
        unlockText.font = GetSafeFont();
        unlockText.fontSize = 14;
        unlockText.color = Color.green;
        unlockText.text = $"SYSTEM UPDATE: {abilityName.ToUpper()} LOADED";
        unlockText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform unlockRect = unlockText.GetComponent<RectTransform>();
        unlockRect.anchorMin = new Vector2(0.2f, 0.4f);
        unlockRect.anchorMax = new Vector2(0.8f, 0.6f);
        unlockRect.offsetMin = Vector2.zero;
        unlockRect.offsetMax = Vector2.zero;
        
        PlayY2KSystemSound();
        
        // Typing effect
        string fullText = unlockText.text;
        unlockText.text = "";
        
        foreach (char c in fullText)
        {
            unlockText.text += c;
            yield return new WaitForSeconds(0.05f);
        }
        
        yield return new WaitForSeconds(3f);
        StartCoroutine(GlitchDestroy(unlockObj));
    }
    
    // Reset system for new levels
    public void ResetAbilities()
    {
        currentSystemMemory = systemMemory;
        remainingRewinds = maxRewinds;
        anchorPosition = null;
        isPerformingTemporalAction = false;
        
        if (projectedGhost != null)
        {
            EndGhostProjection();
        }
        
        Debug.Log("Y2K System Reset - All abilities restored");
    }
    
    // Save/Load system
    [System.Serializable]
    public class Y2KTemporalSaveData
    {
        public int currentMemory;
        public int remainingRewinds;
        public bool hasAnchor;
        public Vector3 anchorPos;
        
        public Y2KTemporalSaveData(TemporalAbilitiesSystem system)
        {
            currentMemory = system.currentSystemMemory;
            remainingRewinds = system.remainingRewinds;
            hasAnchor = system.anchorPosition.HasValue;
            anchorPos = system.anchorPosition ?? Vector3.zero;
        }
    }
    
    public Y2KTemporalSaveData GetSaveData()
    {
        return new Y2KTemporalSaveData(this);
    }
    
    public void LoadSaveData(Y2KTemporalSaveData data)
    {
        currentSystemMemory = data.currentMemory;
        remainingRewinds = data.remainingRewinds;
        anchorPosition = data.hasAnchor ? data.anchorPos : null;
    }
    
    void OnDestroy()
    {
        // Clean up
        if (temporalUI != null) Destroy(temporalUI.gameObject);
        if (projectedGhost != null) Destroy(projectedGhost);
        
        Debug.Log("Y2K Temporal Abilities System shutting down");
    }
    
    // Context menu methods for testing
    [ContextMenu("Test Y2K Rewind")]
    public void TestRewind()
    {
        if (Application.isPlaying)
        {
            TriggerRewind(2f);
        }
    }
    
    [ContextMenu("Test Memory Drain")]
    public void TestMemoryDrain()
    {
        if (Application.isPlaying)
        {
            ConsumeSystemMemory(50);
        }
    }
    
    [ContextMenu("Restore Full Memory")]
    public void TestMemoryRestore()
    {
        if (Application.isPlaying)
        {
            currentSystemMemory = systemMemory;
        }
    }
    
    [ContextMenu("Enable Audio")]
    public void EnableAudio()
    {
        enableAudio = true;
        Debug.Log("Y2K Audio enabled - ensure you have an AudioSource assigned to avoid FMOD conflicts");
    }
    
    [ContextMenu("Test All Abilities")]
    public void TestAllAbilities()
    {
        if (Application.isPlaying)
        {
            Debug.Log("=== Y2K Temporal Abilities Test ===");
            Debug.Log($"Player found: {playerTransform != null}");
            Debug.Log($"Current memory: {currentSystemMemory}/{systemMemory}");
            Debug.Log($"Remaining rewinds: {remainingRewinds}/{maxRewinds}");
            Debug.Log($"Has anchor: {anchorPosition.HasValue}");
            Debug.Log($"Ghost active: {isProjectionActive}");
            Debug.Log($"Audio enabled: {enableAudio}");
            Debug.Log($"Using FMOD: {useFMOD}");
            Debug.Log("=== Test Complete ===");
        }
    }
}

/// <summary>
/// Safe interaction component for ghost objects
/// </summary>
public class GhostSafeInteraction : MonoBehaviour
{
    private TemporalAbilitiesSystem parentSystem;
    
    public void Initialize(TemporalAbilitiesSystem parent)
    {
        parentSystem = parent;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (parentSystem != null)
        {
            parentSystem.OnGhostTriggerEnter(other);
        }
    }
}