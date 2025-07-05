using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("_Game/Scripts/Managers/LevelOneManager")]
public class LevelOneManager : MonoBehaviour
{
    [Header("Level Components")]
    public Transform player;
    public Transform goalTarget; // Simple goal instead of explosion center
    
    [Header("UI Elements")]
    public Text objectiveText;
    public Text progressText;
    public GameObject levelCompleteUI;
    
    [Header("Level State")]
    public bool levelCompleted = false;
    public float goalRadius = 2f;
    
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
            
        if (goalTarget == null)
        {
            // Create a simple goal target
            GameObject goal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            goal.name = "Goal";
            goal.transform.position = new Vector3(15, 1, 0); // End of level
            goal.transform.localScale = Vector3.one * 0.5f;
            goal.GetComponent<Renderer>().material.color = Color.green;
            Destroy(goal.GetComponent<Collider>()); // Remove collider
            goalTarget = goal.transform;
        }
        
        // Calculate starting distance for progress tracking
        if (player != null && goalTarget != null)
        {
            startDistance = Vector3.Distance(player.position, goalTarget.position);
        }
        
        Debug.Log("Level One: Simple platforming challenge initialized");
        Debug.Log("Objective: Reach the green goal sphere");
    }
    
    void Update()
    {
        if (!levelCompleted)
        {
            UpdateProgress();
            UpdateUI();
            CheckGoalReached();
        }
    }
    
    void UpdateProgress()
    {
        if (player == null || goalTarget == null) return;
        
        float currentDistance = Vector3.Distance(player.position, goalTarget.position);
        currentProgress = 1f - (currentDistance / startDistance);
        currentProgress = Mathf.Clamp01(currentProgress);
    }
    
    void UpdateUI()
    {
        if (objectiveText != null)
        {
            objectiveText.text = "Navigate to the green goal sphere";
        }
        
        if (progressText != null)
        {
            progressText.text = $"Progress: {(currentProgress * 100f):F0}%";
        }
    }
    
    void CheckGoalReached()
    {
        if (player == null || goalTarget == null) return;
        
        float distance = Vector3.Distance(player.position, goalTarget.position);
        if (distance <= goalRadius)
        {
            CompleteLevel();
        }
    }
    
    void CompleteLevel()
    {
        if (levelCompleted) return;
        
        levelCompleted = true;
        Debug.Log("Level One completed! Player reached the goal!");
        
        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(true);
        }
        
        if (objectiveText != null)
        {
            objectiveText.text = "SUCCESS! Goal reached!";
        }
        
        if (progressText != null)
        {
            progressText.text = "Level Complete - 100%";
        }
        
        // Goal reached effect
        if (goalTarget != null)
        {
            StartCoroutine(GoalReachedEffect());
        }
    }
    
    System.Collections.IEnumerator GoalReachedEffect()
    {
        // Simple success effect
        Camera mainCam = Camera.main;
        Color originalColor = mainCam.backgroundColor;
        
        // Flash green
        mainCam.backgroundColor = Color.green;
        yield return new WaitForSeconds(0.3f);
        mainCam.backgroundColor = originalColor;
        
        // Make goal pulse
        Vector3 originalScale = goalTarget.localScale;
        for (int i = 0; i < 5; i++)
        {
            goalTarget.localScale = originalScale * 1.5f;
            yield return new WaitForSeconds(0.1f);
            goalTarget.localScale = originalScale;
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    public void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    public void NextLevel()
    {
        Debug.Log("Loading next level...");
        // SceneManager.LoadScene("Level2");
    }
}