using UnityEngine;
using UnityEngine.UI;

namespace SoulGames.EasyGridBuilderPro
{
    /// <summary>
    /// Simple Category Button - A lightweight script for custom category buttons
    /// Just attach to a button and assign a category to filter objects in the default panel
    /// 
    /// This is a simpler alternative to CustomCategoryButtonHandler for basic use cases
    /// </summary>
    [AddComponentMenu("Easy Grid Builder Pro/Grid UI/Simple Category Button", 1)]
    public class SimpleCategoryButton : MonoBehaviour
    {
        [Header("Category Settings")]
        [Tooltip("The category to show when this button is clicked")]
        public BuildableObjectUICategorySO targetCategory;
        
        [Header("Panel Reference")]
        [Tooltip("Target panel to show/hide. If null, uses the default buildable objects panel")]
        public GameObject targetPanel;
        
        private Button buttonComponent;
        private GridManager gridManager;
        private GridUIManager gridUIManager;
        
        private void Start()
        {
            buttonComponent = GetComponent<Button>();
            if (buttonComponent == null)
            {
                Debug.LogError($"SimpleCategoryButton requires a Button component on {gameObject.name}");
                enabled = false;
                return;
            }
            
            buttonComponent.onClick.AddListener(OnButtonClicked);
            
            // Find GridUIManager in scene
            gridUIManager = FindObjectOfType<GridUIManager>();
            gridManager = GridManager.Instance;
        }
        
        private void OnDestroy()
        {
            if (buttonComponent != null)
            {
                buttonComponent.onClick.RemoveListener(OnButtonClicked);
            }
        }
        
        public void OnButtonClicked()
        {
            // Set build mode if not already active
            if (gridManager && gridManager.GetActiveEasyGridBuilderPro())
            {
                gridManager.GetActiveEasyGridBuilderPro().SetActiveGridMode(GridMode.BuildMode);
            }
            
            // Show/hide target panel
            if (targetPanel)
            {
                bool isActive = targetPanel.activeSelf;
                targetPanel.SetActive(!isActive);
            }
            
            // If a specific category is set, you could implement category filtering here
            // For now, this just toggles the panel - extend as needed
        }
        
        /// <summary>
        /// Set the target category for this button
        /// </summary>
        public void SetTargetCategory(BuildableObjectUICategorySO category)
        {
            targetCategory = category;
        }
        
        /// <summary>
        /// Set the target panel for this button
        /// </summary>
        public void SetTargetPanel(GameObject panel)
        {
            targetPanel = panel;
        }
    }
} 