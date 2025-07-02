using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Utility script to automatically setup missing UI system components
/// Run this from the Tools menu or attach it to a GameObject in the scene
/// </summary>
public class UISystemAutoSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [Tooltip("Run setup automatically on Start")]
    public bool autoSetupOnStart = true;
    
    [Header("Debug")]
    public bool debugMode = true;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupUISystem();
        }
    }
    
    /// <summary>
    /// Main setup method - can be called from Tools menu or manually
    /// </summary>
    [ContextMenu("Setup UI System")]
    public void SetupUISystem()
    {
        if (debugMode)
        {
            Debug.Log("UISystemAutoSetup: Starting UI system setup...");
        }
        
        // 1. Setup CategoryMenuManager
        SetupCategoryMenuManager();
        
        // 2. Fix missing script references on buttons
        FixMissingScriptReferences();
        
        // 3. Setup UIMenuController content frames
        SetupUIMenuControllerFrames();
        
        // 4. Setup ButtonSelectionManager
        SetupButtonSelectionManager();
        
        if (debugMode)
        {
            Debug.Log("UISystemAutoSetup: UI system setup completed!");
        }
    }
    
    private void SetupCategoryMenuManager()
    {
        if (debugMode)
        {
            Debug.Log("UISystemAutoSetup: Setting up CategoryMenuManager...");
        }
        
        // Check if CategoryMenuManager already exists
        CategoryMenuManager existingManager = FindObjectOfType<CategoryMenuManager>();
        
        if (existingManager != null)
        {
            if (debugMode)
            {
                Debug.Log($"UISystemAutoSetup: CategoryMenuManager already exists on {existingManager.gameObject.name}");
            }
            return;
        }
        
        // Try to find the Test GameObject
        GameObject testObject = GameObject.Find("Test");
        
        if (testObject == null)
        {
            // Try to find Content objects that might be the right place
            GameObject[] contentObjects = GameObject.FindObjectsOfType<GameObject>();
            
            foreach (GameObject obj in contentObjects)
            {
                if (obj.name == "Content" && obj.transform.childCount > 0)
                {
                    // Check if this Content object has category-like children
                    bool hasCategories = false;
                    foreach (Transform child in obj.transform)
                    {
                        if (child.name.ToLower().Contains("frame") || 
                            child.name.ToLower().Contains("category") ||
                            child.name.ToLower().Contains("module"))
                        {
                            hasCategories = true;
                            break;
                        }
                    }
                    
                    if (hasCategories)
                    {
                        testObject = obj;
                        if (debugMode)
                        {
                            Debug.Log($"UISystemAutoSetup: Found suitable Content object: {obj.name}");
                        }
                        break;
                    }
                }
            }
        }
        
        if (testObject == null)
        {
            if (debugMode)
            {
                Debug.LogWarning("UISystemAutoSetup: Could not find suitable object for CategoryMenuManager. Creating placeholder...");
            }
            
            // Create a placeholder GameObject
            testObject = new GameObject("Test");
            // Try to put it in a logical place in the hierarchy
            Canvas mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas != null)
            {
                testObject.transform.SetParent(mainCanvas.transform);
            }
        }
        
        // Add CategoryMenuManager component
        CategoryMenuManager manager = testObject.GetComponent<CategoryMenuManager>();
        if (manager == null)
        {
            manager = testObject.AddComponent<CategoryMenuManager>();
            if (debugMode)
            {
                Debug.Log($"UISystemAutoSetup: Added CategoryMenuManager to {testObject.name}");
            }
        }
    }
    
    private void FixMissingScriptReferences()
    {
        if (debugMode)
        {
            Debug.Log("UISystemAutoSetup: Checking for missing script references...");
        }
        
        // Find all buttons in the scene
        Button[] allButtons = FindObjectsOfType<Button>(true);
        
        foreach (Button button in allButtons)
        {
            // Check for missing components
            Component[] components = button.GetComponents<Component>();
            
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    if (debugMode)
                    {
                        Debug.LogWarning($"UISystemAutoSetup: Found missing script on button '{button.name}' - this needs manual fixing");
                    }
                }
            }
            
            // Check if button has proper handlers
            if (button.onClick.GetPersistentEventCount() == 0)
            {
                // Check if this button should have a specific handler based on its name
                if (button.name.ToLower().Contains("modules"))
                {
                    ModulesButtonHandler moduleHandler = button.GetComponent<ModulesButtonHandler>();
                    if (moduleHandler == null)
                    {
                        moduleHandler = button.gameObject.AddComponent<ModulesButtonHandler>();
                        if (debugMode)
                        {
                            Debug.Log($"UISystemAutoSetup: Added ModulesButtonHandler to {button.name}");
                        }
                    }
                }
                else if (button.name.ToLower().Contains("btn_"))
                {
                    // This might need UIMenuController setup
                    UIMenuController uiController = FindObjectOfType<UIMenuController>();
                    if (uiController != null)
                    {
                        if (debugMode)
                        {
                            Debug.Log($"UISystemAutoSetup: Button {button.name} might need UIMenuController setup");
                        }
                    }
                }
            }
        }
    }
    
    private void SetupUIMenuControllerFrames()
    {
        if (debugMode)
        {
            Debug.Log("UISystemAutoSetup: Setting up UIMenuController content frames...");
        }
        
        UIMenuController uiController = FindObjectOfType<UIMenuController>();
        
        if (uiController == null)
        {
            if (debugMode)
            {
                Debug.LogWarning("UISystemAutoSetup: No UIMenuController found in scene");
            }
            return;
        }
        
        // Find buttons that match the pattern and create content frames if needed
        Button[] allButtons = FindObjectsOfType<Button>(true);
        
        foreach (Button button in allButtons)
        {
            if (button.name.StartsWith("Btn_"))
            {
                string expectedFrameName = "Informatie inhoud frame";
                
                // Look for existing content frame
                Transform contentFrame = FindContentFrameForButton(button, expectedFrameName);
                
                if (contentFrame == null)
                {
                    // Create content frame if it doesn't exist
                    CreateContentFrameForButton(button, expectedFrameName);
                }
            }
        }
    }
    
    private Transform FindContentFrameForButton(Button button, string frameName)
    {
        // Search in parent hierarchy
        Transform current = button.transform.parent;
        
        while (current != null)
        {
            Transform found = current.Find(frameName);
            if (found != null)
            {
                return found;
            }
            current = current.parent;
        }
        
        return null;
    }
    
    private void CreateContentFrameForButton(Button button, string frameName)
    {
        if (debugMode)
        {
            Debug.Log($"UISystemAutoSetup: Creating content frame '{frameName}' for button {button.name}");
        }
        
        // Create content frame as sibling of button
        GameObject contentFrame = new GameObject(frameName);
        contentFrame.transform.SetParent(button.transform.parent);
        
        // Add RectTransform and basic UI components
        RectTransform rectTransform = contentFrame.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Add CanvasGroup for better control
        CanvasGroup canvasGroup = contentFrame.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        // Start inactive
        contentFrame.SetActive(false);
    }
    
    private void SetupButtonSelectionManager()
    {
        if (debugMode)
        {
            Debug.Log("UISystemAutoSetup: Setting up ButtonSelectionManager...");
        }
        
        ButtonSelectionManager selectionManager = FindObjectOfType<ButtonSelectionManager>();
        
        if (selectionManager == null)
        {
            if (debugMode)
            {
                Debug.LogWarning("UISystemAutoSetup: No ButtonSelectionManager found in scene");
            }
            return;
        }
        
        // The ButtonSelectionManager should automatically setup its references
        // Just ensure it has proper screen references
        if (debugMode)
        {
            Debug.Log("UISystemAutoSetup: ButtonSelectionManager exists, should auto-configure");
        }
    }
    
    /// <summary>
    /// Helper method to run setup from code
    /// </summary>
    public static void RunSetupFromCode()
    {
        // Find existing setup component or create one
        UISystemAutoSetup setup = FindObjectOfType<UISystemAutoSetup>();
        
        if (setup == null)
        {
            GameObject setupObject = new GameObject("UI System Auto Setup");
            setup = setupObject.AddComponent<UISystemAutoSetup>();
            setup.debugMode = true;
        }
        
        setup.SetupUISystem();
        
        Debug.Log("UI System Auto Setup completed!");
    }
} 