using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AdvancedTimeGhost : MonoBehaviour
{
    [Header("Multiple Ghost System")]
    [SerializeField] private int maxGhosts = 3; // Show multiple temporal echoes
    [SerializeField] private float ghostSpacing = 1f; // Time between each ghost
    [SerializeField] private bool showGhostTrail = true; // Connect ghosts with lines
    
    [Header("Interactive Ghost Mechanics")]
    [SerializeField] private bool ghostCanTriggerSwitches = true; // Ghost activates platforms
    [SerializeField] private bool ghostBlocksProjectiles = true; // Ghost shields player
    [SerializeField] private LayerMask ghostInteractionLayers = 1;
    
    [Header("Temporal Abilities")]
    [SerializeField] private bool enableTimeRewind = true; // Player can snap to ghost position
    [SerializeField] private KeyCode rewindKey = KeyCode.R;
    [SerializeField] private bool enableGhostSwap = true; // Swap player and ghost positions
    [SerializeField] private KeyCode swapKey = KeyCode.T;
    
    [Header("Predictive Ghost")]
    [SerializeField] private bool showFutureGhost = true; // Show where player will be
    [SerializeField] private Color futureGhostColor = Color.yellow;
    [SerializeField] private float futurePredictionTime = 2f;
    
    [Header("Y2K Digital Effects")]
    [SerializeField] private bool enableDataTrail = true; // Leave digital breadcrumbs
    [SerializeField] private GameObject dataFragmentPrefab;
    [SerializeField] private bool enableGhostEcho = true; // Sound echoes from ghost position
    [SerializeField] private AudioSource ghostAudioSource;
    
    [Header("Advanced Visuals")]
    [SerializeField] private bool enableScanlines = true; // CRT monitor effect
    [SerializeField] private bool enablePixelDistortion = true; // Digital corruption
    [SerializeField] private Material ghostMaterial; // Custom shader for effects
    
    // Private variables
    private Transform playerTransform;
    private List<GameObject> ghostObjects = new List<GameObject>();
    private List<List<Vector3>> ghostPositionHistories = new List<List<Vector3>>();
    private GameObject futureGhost;
    private LineRenderer trailRenderer;
    private List<GameObject> dataFragments = new List<GameObject>();
    
    // Ghost interaction components
    private Dictionary<GameObject, Collider2D> ghostColliders = new Dictionary<GameObject, Collider2D>();
    
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null) return;
        
        CreateMultipleGhosts();
        CreateFutureGhost();
        CreateTrailRenderer();
        SetupGhostAudio();
        
        // Subscribe to temporal events
        if (TemporalController.Instance != null)
        {
            TemporalController.Instance.OnTimeScaleChanged += OnTimeDirectionChanged;
        }
    }
    
    void CreateMultipleGhosts()
    {
        SpriteRenderer playerSR = playerTransform.GetComponent<SpriteRenderer>();
        if (playerSR == null) return;
        
        for (int i = 0; i < maxGhosts; i++)
        {
            // Create ghost object
            GameObject ghost = new GameObject($"TimeGhost_{i}");
            
            // Setup sprite renderer
            SpriteRenderer ghostSR = ghost.AddComponent<SpriteRenderer>();
            ghostSR.sprite = playerSR.sprite;
            ghostSR.sortingOrder = playerSR.sortingOrder - (i + 1);
            
            // Apply custom material if available
            if (ghostMaterial != null)
            {
                ghostSR.material = ghostMaterial;
            }
            
            // Setup scaling and color
            float scaleMultiplier = 1f - (i * 0.1f); // Each ghost slightly smaller
            ghost.transform.localScale = Vector3.one * scaleMultiplier;
            
            Color ghostColor = Color.white;
            ghostColor.a = 0.3f - (i * 0.05f); // Each ghost more transparent
            ghostSR.color = ghostColor;
            
            // Setup interaction collider if needed
            if (ghostCanTriggerSwitches || ghostBlocksProjectiles)
            {
                Collider2D ghostCollider = ghost.AddComponent<BoxCollider2D>();
                ghostCollider.isTrigger = true;
                ghostColliders[ghost] = ghostCollider;
                
                // Add ghost interaction script
                GhostInteraction interaction = ghost.AddComponent<GhostInteraction>();
                interaction.Initialize(this, i);
            }
            
            ghostObjects.Add(ghost);
            ghostPositionHistories.Add(new List<Vector3>());
            
            ghost.SetActive(false); // Start hidden
        }
    }
    
    void CreateFutureGhost()
    {
        if (!showFutureGhost) return;
        
        SpriteRenderer playerSR = playerTransform.GetComponent<SpriteRenderer>();
        if (playerSR == null) return;
        
        futureGhost = new GameObject("FutureGhost");
        SpriteRenderer futureSR = futureGhost.AddComponent<SpriteRenderer>();
        futureSR.sprite = playerSR.sprite;
        futureSR.sortingOrder = playerSR.sortingOrder - 10;
        
        Color futureColor = futureGhostColor;
        futureColor.a = 0.4f;
        futureSR.color = futureColor;
        
        futureGhost.transform.localScale = Vector3.one * 0.9f;
        futureGhost.SetActive(false);
    }
    
    void CreateTrailRenderer()
    {
        if (!showGhostTrail) return;
        
        GameObject trailObj = new GameObject("GhostTrail");
        trailRenderer = trailObj.AddComponent<LineRenderer>();
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        // Fixed LineRenderer API for newer Unity versions
        trailRenderer.startColor = new Color(0.5f, 0.8f, 1f, 0.3f);
        trailRenderer.endColor = new Color(0.5f, 0.8f, 1f, 0.1f);
        trailRenderer.startWidth = 0.1f;
        trailRenderer.endWidth = 0.05f;
        
        trailRenderer.positionCount = 0;
        trailRenderer.useWorldSpace = true;
    }
    
    void SetupGhostAudio()
    {
        if (enableGhostEcho && ghostAudioSource == null)
        {
            GameObject audioObj = new GameObject("GhostAudio");
            ghostAudioSource = audioObj.AddComponent<AudioSource>();
            ghostAudioSource.volume = 0.3f;
            ghostAudioSource.pitch = 0.8f; // Lower pitch for ghostly effect
        }
    }
    
    void Update()
    {
        if (playerTransform == null) return;
        
        UpdateGhostPositions();
        UpdateFutureGhost();
        UpdateGhostTrail();
        UpdateDataFragments();
        HandleTemporalAbilities();
        
        if (enableScanlines || enablePixelDistortion)
        {
            UpdateVisualEffects();
        }
    }
    
    void UpdateGhostPositions()
    {
        // Record current player position
        for (int i = 0; i < maxGhosts; i++)
        {
            List<Vector3> history = ghostPositionHistories[i];
            
            // Add current position every frame
            history.Add(playerTransform.position);
            
            // Calculate how far back this ghost should be
            float timeOffset = (i + 1) * ghostSpacing;
            int positionsBack = Mathf.RoundToInt(timeOffset / Time.deltaTime);
            
            // Remove old positions
            while (history.Count > positionsBack + 50)
            {
                history.RemoveAt(0);
            }
            
            // Position ghost at historical location
            if (history.Count > positionsBack)
            {
                int targetIndex = history.Count - 1 - positionsBack;
                targetIndex = Mathf.Clamp(targetIndex, 0, history.Count - 1);
                
                ghostObjects[i].transform.position = history[targetIndex];
                ghostObjects[i].SetActive(true);
            }
        }
        
        // Update ghost colors based on time direction
        float timeDirection = TemporalController.Instance?.GetTimeDirection() ?? 1f;
        Color baseColor = timeDirection < 0 ? Color.cyan : Color.white;
        
        for (int i = 0; i < ghostObjects.Count; i++)
        {
            SpriteRenderer sr = ghostObjects[i].GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color ghostColor = baseColor;
                ghostColor.a = 0.3f - (i * 0.05f);
                sr.color = Color.Lerp(sr.color, ghostColor, Time.deltaTime * 3f);
            }
        }
    }
    
    void UpdateFutureGhost()
    {
        if (!showFutureGhost || futureGhost == null) return;
        
        // Predict future position based on current velocity
        Rigidbody2D playerRB = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRB != null)
        {
            Vector3 predictedPosition = playerTransform.position + 
                                      (Vector3)playerRB.linearVelocity * futurePredictionTime;
            
            futureGhost.transform.position = predictedPosition;
            futureGhost.SetActive(true);
        }
    }
    
    void UpdateGhostTrail()
    {
        if (!showGhostTrail || trailRenderer == null) return;
        
        List<Vector3> trailPositions = new List<Vector3>();
        trailPositions.Add(playerTransform.position);
        
        foreach (GameObject ghost in ghostObjects)
        {
            if (ghost.activeSelf)
            {
                trailPositions.Add(ghost.transform.position);
            }
        }
        
        trailRenderer.positionCount = trailPositions.Count;
        trailRenderer.SetPositions(trailPositions.ToArray());
    }
    
    void UpdateDataFragments()
    {
        if (!enableDataTrail || dataFragmentPrefab == null) return;
        
        // Spawn data fragments occasionally at ghost positions
        if (Random.Range(0f, 1f) < 0.02f) // 2% chance per frame
        {
            foreach (GameObject ghost in ghostObjects)
            {
                if (ghost.activeSelf)
                {
                    SpawnDataFragment(ghost.transform.position);
                }
            }
        }
        
        // Clean up old fragments
        for (int i = dataFragments.Count - 1; i >= 0; i--)
        {
            if (dataFragments[i] == null)
            {
                dataFragments.RemoveAt(i);
            }
        }
    }
    
    void SpawnDataFragment(Vector3 position)
    {
        if (dataFragments.Count > 20) return; // Limit fragments
        
        GameObject fragment = Instantiate(dataFragmentPrefab, position, Quaternion.identity);
        dataFragments.Add(fragment);
        
        // Auto-destroy after time
        StartCoroutine(DestroyFragmentAfterTime(fragment, 3f));
    }
    
    IEnumerator DestroyFragmentAfterTime(GameObject fragment, float time)
    {
        yield return new WaitForSeconds(time);
        if (fragment != null)
        {
            dataFragments.Remove(fragment);
            Destroy(fragment);
        }
    }
    
    void HandleTemporalAbilities()
    {
        // Time Rewind: Snap player to oldest ghost position
        if (enableTimeRewind && Input.GetKeyDown(rewindKey))
        {
            PerformTimeRewind();
        }
        
        // Ghost Swap: Exchange player and ghost positions
        if (enableGhostSwap && Input.GetKeyDown(swapKey))
        {
            PerformGhostSwap();
        }
    }
    
    void PerformTimeRewind()
    {
        if (ghostObjects.Count == 0 || !ghostObjects[maxGhosts - 1].activeSelf) return;
        
        Vector3 rewindPosition = ghostObjects[maxGhosts - 1].transform.position;
        playerTransform.position = rewindPosition;
        
        // Visual effect
        StartCoroutine(RewindEffect());
        
        Debug.Log("Time rewind activated!");
    }
    
    void PerformGhostSwap()
    {
        if (ghostObjects.Count == 0 || !ghostObjects[0].activeSelf) return;
        
        Vector3 playerPos = playerTransform.position;
        Vector3 ghostPos = ghostObjects[0].transform.position;
        
        playerTransform.position = ghostPos;
        
        // Clear ghost histories to prevent weird behavior
        foreach (List<Vector3> history in ghostPositionHistories)
        {
            history.Clear();
        }
        
        StartCoroutine(SwapEffect());
        
        Debug.Log("Ghost swap activated!");
    }
    
    IEnumerator RewindEffect()
    {
        // Brief screen flash or particle effect
        yield return new WaitForSeconds(0.1f);
    }
    
    IEnumerator SwapEffect()
    {
        // Brief teleport effect
        yield return new WaitForSeconds(0.1f);
    }
    
    void UpdateVisualEffects()
    {
        // Apply scanlines or pixel distortion to all ghosts
        foreach (GameObject ghost in ghostObjects)
        {
            if (ghost.activeSelf)
            {
                ApplyRetroEffects(ghost);
            }
        }
    }
    
    void ApplyRetroEffects(GameObject ghost)
    {
        // Add CRT-style effects, pixel distortion, etc.
        // This would work with custom shaders
    }
    
    void OnTimeDirectionChanged(float timeDirection)
    {
        // Ghosts could behave differently in reverse time
        if (timeDirection < 0)
        {
            // Reverse time: ghosts move more clearly, show future positions
        }
        else
        {
            // Normal time: ghosts show past positions
        }
    }
    
    // Public methods for ghost interactions
    public void OnGhostTriggerEnter(int ghostIndex, Collider2D other)
    {
        if (ghostCanTriggerSwitches && other.CompareTag("Switch"))
        {
            // Ghost activates switch
            ISwitchable switchable = other.GetComponent<ISwitchable>();
            switchable?.OnActivate();
        }
    }
    
    void OnDestroy()
    {
        if (TemporalController.Instance != null)
        {
            TemporalController.Instance.OnTimeScaleChanged -= OnTimeDirectionChanged;
        }
        
        foreach (GameObject ghost in ghostObjects)
        {
            if (ghost != null) Destroy(ghost);
        }
        
        if (futureGhost != null) Destroy(futureGhost);
        if (trailRenderer != null) Destroy(trailRenderer.gameObject);
    }
}

// Helper class for ghost interactions
public class GhostInteraction : MonoBehaviour
{
    private AdvancedTimeGhost parentGhost;
    private int ghostIndex;
    
    public void Initialize(AdvancedTimeGhost parent, int index)
    {
        parentGhost = parent;
        ghostIndex = index;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        parentGhost?.OnGhostTriggerEnter(ghostIndex, other);
    }
}

// Interface for switchable objects
public interface ISwitchable
{
    void OnActivate();
}