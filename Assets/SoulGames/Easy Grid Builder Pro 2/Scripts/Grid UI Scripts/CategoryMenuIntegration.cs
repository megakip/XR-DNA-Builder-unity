using UnityEngine;
using UnityEngine.UI;

namespace SoulGames.EasyGridBuilderPro
{
    /// <summary>
    /// Integration helper for CategoryMenuManager and Custom Category Button Handler
    /// Place this on the same GameObject as CategoryMenuManager to enable integration
    /// </summary>
    [AddComponentMenu("Easy Grid Builder Pro/Grid UI/Category Menu Integration", 4)]
    public class CategoryMenuIntegration : MonoBehaviour
    {
        [Header("Integration Settings")]
        [Tooltip("Close all custom category panels when CategoryMenuManager changes categories")]
        public bool closeCustomPanelsOnCategoryChange = true;
        
        [Header("Debug")]
        public bool debugMode = true;
        
        private CategoryMenuManager categoryMenuManager;
        private int lastCategoryIndex = -1;
        
        private void Start()
        {
            categoryMenuManager = GetComponent<CategoryMenuManager>();
            if (categoryMenuManager == null)
            {
                categoryMenuManager = FindObjectOfType<CategoryMenuManager>();
            }
            
            if (categoryMenuManager == null)
            {
                Debug.LogWarning("CategoryMenuIntegration: No CategoryMenuManager found!");
                enabled = false;
                return;
            }
            
            if (debugMode)
            {
                Debug.Log($"CategoryMenuIntegration: Found CategoryMenuManager on {categoryMenuManager.gameObject.name}");
            }
            
            lastCategoryIndex = categoryMenuManager.CurrentCategoryIndex;
            
            // Also listen for PageNavigator clicks to close custom panels
            ListenForPageNavigationButtons();
        }
        
        private void ListenForPageNavigationButtons()
        {
            // Find all PageNavigator scripts and add listeners
            var pageNavigators = FindObjectsOfType<PageNavigator>();
            
            foreach (var navigator in pageNavigators)
            {
                var button = navigator.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => {
                        if (debugMode)
                        {
                            Debug.Log("CategoryMenuIntegration: PageNavigator clicked - closing custom panels");
                        }
                        CloseAllCustomCategoryPanels();
                    });
                }
            }
            
            if (debugMode && pageNavigators.Length > 0)
            {
                Debug.Log($"CategoryMenuIntegration: Added listeners to {pageNavigators.Length} PageNavigator buttons");
            }
        }
        
        private void Update()
        {
            if (categoryMenuManager == null) return;
            
            // Check if category changed
            int currentCategoryIndex = categoryMenuManager.CurrentCategoryIndex;
            if (currentCategoryIndex != lastCategoryIndex)
            {
                OnCategoryChanged(lastCategoryIndex, currentCategoryIndex);
                lastCategoryIndex = currentCategoryIndex;
            }
        }
        
        private void OnCategoryChanged(int oldIndex, int newIndex)
        {
            if (debugMode)
            {
                Debug.Log($"CategoryMenuIntegration: Category changed from {oldIndex} to {newIndex}");
            }
            
            if (closeCustomPanelsOnCategoryChange)
            {
                CloseAllCustomCategoryPanels();
            }
        }
        
        private void CloseAllCustomCategoryPanels()
        {
            // Find all CustomCategoryButtonHandler instances and close their panels
            CustomCategoryButtonHandler[] handlers = FindObjectsOfType<CustomCategoryButtonHandler>();
            
            foreach (var handler in handlers)
            {
                if (handler.IsPanelOpen)
                {
                    if (debugMode)
                    {
                        Debug.Log($"CategoryMenuIntegration: Closing custom panel from {handler.gameObject.name}");
                    }
                    handler.ClosePanel();
                }
            }
        }
        
        /// <summary>
        /// Manually trigger closure of all custom panels
        /// </summary>
        public void CloseAllCustomPanels()
        {
            CloseAllCustomCategoryPanels();
        }
        
        /// <summary>
        /// Get current category info
        /// </summary>
        public string GetCurrentCategoryInfo()
        {
            if (categoryMenuManager == null) return "No CategoryMenuManager";
            
            return $"Current: {categoryMenuManager.CurrentCategoryName} (Index: {categoryMenuManager.CurrentCategoryIndex})";
        }
    }
} 