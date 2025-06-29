using UnityEngine;

public class SimpleTemporalUI : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private bool showUI = true;
    [SerializeField] private Vector2 screenPosition = new Vector2(20, 50);
    [SerializeField] private int fontSize = 24;
    
    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color reverseColor = Color.red;
    [SerializeField] private Color forwardColor = Color.cyan;
    
    private GUIStyle textStyle;
    private bool stylesInitialized = false;
    
    void Start()
    {
        // Force this GameObject to stay alive
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("SimpleTemporalUI started!");
    }
    
    void InitializeStyles()
    {
        if (stylesInitialized) return;
        
        textStyle = new GUIStyle();
        textStyle.fontSize = fontSize;
        textStyle.fontStyle = FontStyle.Bold;
        textStyle.normal.textColor = normalColor;
        
        stylesInitialized = true;
    }
    
    void OnGUI()
    {
        if (!showUI) return;
        
        InitializeStyles();
        
        // Get time info
        string timeText = "TIME: LOADING...";
        Color currentColor = normalColor;
        
        if (TemporalController.Instance != null)
        {
            float timeDirection = TemporalController.Instance.GetTimeDirection();
            string directionText = TemporalController.Instance.GetTimeDirectionText();
            
            if (timeDirection < -0.1f)
            {
                directionText = "REVERSE TIME";
                currentColor = reverseColor;
            }
            else if (timeDirection > 0.1f)
            {
                directionText = "NORMAL TIME";
                currentColor = normalColor;
            }
            else
            {
                directionText = "TRANSITIONING";
                currentColor = Color.yellow;
            }
            
            // Simple, clean display
            timeText = directionText + "\n" +
                      "Direction: " + timeDirection.ToString("F2") + "\n" +
                      "Physics: PROTECTED" + "\n" +
                      "Unity TimeScale: " + Time.timeScale.ToString("F2");
                      
            // Add movement hint
            if (Mathf.Abs(timeDirection - 1f) < 0.1f)
            {
                timeText += "\nMove RIGHT for reverse time";
            }
        }
        else
        {
            timeText = "NO TEMPORAL CONTROLLER\nMove RIGHT = Reverse\nMove LEFT = Normal";
            currentColor = Color.gray;
        }
        
        // Update text color
        textStyle.normal.textColor = currentColor;
        
        // Draw the text
        GUI.Label(new Rect(screenPosition.x, screenPosition.y, 300, 120), timeText, textStyle);
        
        // Optional: Draw a background box
        GUI.Box(new Rect(screenPosition.x - 5, screenPosition.y - 5, 250, 100), "");
    }
    
    // Public methods for easy testing
    [ContextMenu("Toggle UI")]
    public void ToggleUI()
    {
        showUI = !showUI;
        Debug.Log($"Temporal UI: {(showUI ? "ON" : "OFF")}");
    }
    
    [ContextMenu("Test Colors")]
    public void TestColors()
    {
        Debug.Log($"Normal: {normalColor}, Reverse: {reverseColor}, Forward: {forwardColor}");
    }
    
    void Update()
    {
        // Optional: Toggle with a key
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleUI();
        }
    }
}