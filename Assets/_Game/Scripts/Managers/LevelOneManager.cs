using UnityEngine;
using UnityEngine.UI;

public class LevelOneManager : MonoBehaviour
{
    [Header("Level Components")]
    public ReverseExplosionCore explosionCore;
    public Transform player;
    public Transform explosionCenter;
    
    [Header("UI Elements")]
    public Text objectiveText;
    public Text progressText;
    public GameObject levelCompleteUI;
    
    [Header("Level State")]
    public bool levelCompleted = false;
    
    private float startDistance;
    private float currentProgress = 0f;
    
    void Start()
    {
        SetupLevel();
        UpdateUI();
    }
    
    void SetupLevel()
    {
        // Find components if not assigned
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;
            
        if (explosionCore == null)
            explosionCore = FindObjectOfType<ReverseExplosionCore>();
            
        if (explosionCenter == null)
            explosionCenter = explosionCore?.explosionCenter;
        
        // Calculate starting distance for progress tracking
        if (player != null && explosionCenter != null)
        {
            startDistance = Vector3.Distance(player.position, explosionCenter.position);
        }
        
        Debug.Log("Level One: Reverse Explosion Challenge initialized");
        Debug.Log("Objective: Reach the explosion center as it contracts backward in time");
    }
    
    void Update()
    {
        if (!levelCompleted)
        {
            UpdateProgress();
            UpdateUI();
        }
    }
    
    void UpdateProgress()
    {
        if (player == null || explosionCenter == null) return;
        
        float currentDistance = Vector3.Distance(player.position, explosionCenter.position);
        currentProgress = 1f - (currentDistance / startDistance);
        currentProgress = Mathf.Clamp01(currentProgress);
    }
    
    void UpdateUI()
    {
        if (objectiveText != null)
        {
            objectiveText.text = "Navigate to the explosion center as time flows backward";
        }
        
        if (progressText != null)
        {
            progressText.text = $"Progress: {(currentProgress * 100f):F0}%";
        }
    }
    
    public void OnExplosionCenterReached()
    {
        levelCompleted = true;
        Debug.Log("Level One completed! Player successfully reached explosion center!");
        
        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(true);
        }
        
        if (objectiveText != null)
        {
            objectiveText.text = "SUCCESS! Explosion center reached!";
        }
        
        if (progressText != null)
        {
            progressText.text = "Level Complete - 100%";
        }
    }
    
    public void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    public void NextLevel()
    {
        // Load next level or return to menu
        Debug.Log("Loading next level...");
        // SceneManager.LoadScene("Level2");
    }
}