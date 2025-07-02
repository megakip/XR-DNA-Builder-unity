using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple component to automatically fix UI system issues
/// Just add this to any GameObject in your scene and it will auto-fix the problems
/// </summary>
[AddComponentMenu("UI System/Auto Fix UI Issues")]
public class UISystemAutoFix : MonoBehaviour
{
    [Header("Fix Settings")]
    [Tooltip("Automatically fix issues when this component starts")]
    public bool autoFixOnStart = true;
    
    [Tooltip("Run fixes periodically (useful for runtime issues)")]
    public bool periodicChecks = false;
    
    [Tooltip("Interval for periodic checks (in seconds)")]
    public float checkInterval = 5f;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    private float lastCheckTime;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            FixUIIssues();
        }
    }
    
    void Update()
    {
        if (periodicChecks && Time.time - lastCheckTime > checkInterval)
        {
            FixUIIssues();
            lastCheckTime = Time.time;
        }
    }
    
    [ContextMenu("Fix UI Issues Now")]
    public void FixUIIssues()
    {
        if (showDebugLogs)
        {
            Debug.Log("UISystemAutoFix: Starting automatic fix...");
        }
        
        FixCategoryMenuManager();
        FixMissingButtonHandlers();
        
        if (showDebugLogs)
        {
            Debug.Log("UISystemAutoFix: Auto-fix completed!");
        }
    }
    
    private void FixCategoryMenuManager()
    {
        // Check if CategoryMenuManager exists
        CategoryMenuManager manager = FindObjectOfType<CategoryMenuManager>();
        
        if (manager == null)
        {
            // Try to find a suitable GameObject to add it to
            GameObject targetObject = GameObject.Find("Test");
            
            if (targetObject == null)
            {
                // Look for Content objects
                GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == "Content" && obj.transform.childCount > 0)
                    {
                        targetObject = obj;
                        break;
                    }
                }
            }
            
            if (targetObject == null)
            {
                // Create a new GameObject for the manager
                targetObject = new GameObject("CategoryMenuManager");
                
                // Try to put it in a sensible location
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    targetObject.transform.SetParent(canvas.transform);
                }
            }
            
            // Add the CategoryMenuManager component
            manager = targetObject.AddComponent<CategoryMenuManager>();
            
            if (showDebugLogs)
            {
                Debug.Log($"UISystemAutoFix: Added CategoryMenuManager to {targetObject.name}");
            }
        }
        else if (showDebugLogs)
        {
            Debug.Log($"UISystemAutoFix: CategoryMenuManager already exists on {manager.gameObject.name}");
        }
    }
    
    private void FixMissingButtonHandlers()
    {
        // Find buttons that might need specific handlers
        Button[] allButtons = FindObjectsOfType<Button>(true);
        
        foreach (Button button in allButtons)
        {
            // Check for modules button
            if (button.name.ToLower().Contains("modules") || button.name.Contains("Btn_Modules"))
            {
                ModulesButtonHandler handler = button.GetComponent<ModulesButtonHandler>();
                if (handler == null)
                {
                    handler = button.gameObject.AddComponent<ModulesButtonHandler>();
                    if (showDebugLogs)
                    {
                        Debug.Log($"UISystemAutoFix: Added ModulesButtonHandler to {button.name}");
                    }
                }
            }
            
            // Check for other UI buttons that need content frames
            if (button.name.StartsWith("Btn_"))
            {
                // Check if there's a UIMenuController to handle this
                UIMenuController controller = FindObjectOfType<UIMenuController>();
                if (controller != null)
                {
                    // The UIMenuController should handle the content frame setup
                    if (showDebugLogs)
                    {
                        Debug.Log($"UISystemAutoFix: UIMenuController found for button {button.name}");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Public method to manually trigger fixes
    /// </summary>
    public void TriggerFix()
    {
        FixUIIssues();
    }
} 