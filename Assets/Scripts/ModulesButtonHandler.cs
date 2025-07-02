using UnityEngine;
using UnityEngine.UI;

public class ModulesButtonHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CategoryMenuManager categoryManager;
    [SerializeField] private GameObject targetMenuObject;
    [SerializeField] private GameObject contentParent;
    
    private Button button;
    
    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnModulesButtonClick);
        }
        
        // Auto-find references if not set
        if (contentParent == null)
        {
            contentParent = GameObject.Find("Content");
        }
        
        if (targetMenuObject == null && contentParent != null)
        {
            // Look for Menu object in Content
            Transform menuTransform = contentParent.transform.Find("Menu");
            if (menuTransform != null)
            {
                targetMenuObject = menuTransform.gameObject;
            }
        }
        
        // Vind de CategoryMenuManager automatisch als deze niet is ingesteld (voor fallback)
        if (categoryManager == null)
        {
            categoryManager = FindObjectOfType<CategoryMenuManager>();
        }
        
        Debug.Log($"ModulesButtonHandler: Setup complete. TargetMenu: {(targetMenuObject ? targetMenuObject.name : "None")}, Content: {(contentParent ? contentParent.name : "None")}");
    }
    
    void OnModulesButtonClick()
    {
        Debug.Log("Modules button clicked!");
        
        if (targetMenuObject != null && contentParent != null)
        {
            // First, deactivate all children in Content
            foreach (Transform child in contentParent.transform)
            {
                if (child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(false);
                    Debug.Log($"ModulesButtonHandler: Deactivated {child.name}");
                }
            }
            
            // Then activate the Menu object
            targetMenuObject.SetActive(true);
            Debug.Log($"ModulesButtonHandler: Activated {targetMenuObject.name}");
            
            // Register with SimpleBackButton for navigation
            if (System.Type.GetType("SimpleBackButton") != null)
            {
                try
                {
                    var method = System.Type.GetType("SimpleBackButton").GetMethod("RegisterScreenChange", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    method?.Invoke(null, new object[] { targetMenuObject, null });
                    Debug.Log($"ModulesButtonHandler: Registered {targetMenuObject.name} with SimpleBackButton");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"ModulesButtonHandler: Could not register with SimpleBackButton: {e.Message}");
                }
            }
        }
        else if (categoryManager != null)
        {
            // Fallback to old behavior if Menu object not found
            Debug.LogWarning("ModulesButtonHandler: Menu object not found, falling back to CategoryMenuManager");
            categoryManager.ShowFirstCategory();
            Debug.Log($"Showing first category: {categoryManager.CurrentCategoryName}");
        }
        else
        {
            Debug.LogError("ModulesButtonHandler: Neither Menu object nor CategoryMenuManager found!");
        }
    }
    
    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnModulesButtonClick);
        }
    }
}
