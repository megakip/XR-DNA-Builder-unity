using UnityEngine;
using UnityEngine.UI;

namespace SoulGames.EasyGridBuilderPro
{
    /// <summary>
    /// Custom UI Setup Utility - Helper methods for setting up custom UI elements
    /// This provides convenient methods to quickly setup custom panels and buttons
    /// </summary>
    [AddComponentMenu("Easy Grid Builder Pro/Grid UI/Custom UI Setup Utility", 2)]
    public class CustomUISetupUtility : MonoBehaviour
    {
        [Header("Quick Setup Templates")]
        [Tooltip("Template button for creating buildable object buttons")]
        public RectTransform buildableButtonTemplate;
        
        [Tooltip("Template panel for creating external panels")]
        public RectTransform panelTemplate;
        
        /// <summary>
        /// Creates a new external panel for buildable objects
        /// </summary>
        /// <param name="parentCanvas">Canvas to create the panel under</param>
        /// <param name="panelName">Name for the new panel</param>
        /// <param name="position">Position on screen</param>
        /// <returns>The created panel RectTransform</returns>
        public RectTransform CreateExternalPanel(Canvas parentCanvas, string panelName = "Custom Buildables Panel", UIPanelPosition position = UIPanelPosition.TopLeft)
        {
            GameObject panelObject = new GameObject(panelName);
            RectTransform panelRect = panelObject.AddComponent<RectTransform>();
            panelObject.AddComponent<CanvasGroup>();
            
            // Add background image
            Image background = panelObject.AddComponent<Image>();
            background.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // Add grid layout for buttons
            GridLayoutGroup gridLayout = panelObject.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(64, 64);
            gridLayout.spacing = new Vector2(5, 5);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperLeft;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3;
            
            // Set parent and position
            panelRect.SetParent(parentCanvas.transform, false);
            SetPanelPosition(panelRect, position, new Vector2(16, 16));
            
            // Set size
            panelRect.sizeDelta = new Vector2(200, 150);
            
            panelObject.SetActive(false);
            
            return panelRect;
        }
        
        /// <summary>
        /// Sets up a button with the CustomCategoryButtonHandler component
        /// </summary>
        /// <param name="button">Button to setup</param>
        /// <param name="externalPanel">Panel to control</param>
        /// <param name="category">Category to filter (optional)</param>
        /// <param name="buttonTemplate">Template for buildable buttons (optional)</param>
        /// <returns>The added CustomCategoryButtonHandler component</returns>
        public CustomCategoryButtonHandler SetupCustomCategoryButton(Button button, RectTransform externalPanel, BuildableObjectUICategorySO category = null, RectTransform buttonTemplate = null)
        {
            CustomCategoryButtonHandler handler = button.GetComponent<CustomCategoryButtonHandler>();
            if (handler == null)
            {
                handler = button.gameObject.AddComponent<CustomCategoryButtonHandler>();
            }
            
            handler.externalPanel = externalPanel;
            handler.buildableObjectCategory = category;
            handler.buildableButtonTemplate = buttonTemplate ?? buildableButtonTemplate;
            
            return handler;
        }
        
        /// <summary>
        /// Creates a complete custom category button setup
        /// </summary>
        /// <param name="parentCanvas">Canvas to create elements under</param>
        /// <param name="buttonPosition">Position for the button</param>
        /// <param name="panelPosition">Position for the panel</param>
        /// <param name="category">Category to filter</param>
        /// <param name="buttonName">Name for the button</param>
        /// <param name="panelName">Name for the panel</param>
        /// <returns>Tuple containing the button and panel</returns>
        public (Button button, RectTransform panel) CreateCompleteCustomCategorySetup(
            Canvas parentCanvas, 
            UIPanelPosition buttonPosition = UIPanelPosition.BottomLeft,
            UIPanelPosition panelPosition = UIPanelPosition.TopLeft,
            BuildableObjectUICategorySO category = null,
            string buttonName = "Custom Category Button",
            string panelName = "Custom Buildables Panel")
        {
            // Create button
            GameObject buttonObject = new GameObject(buttonName);
            RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
            buttonRect.SetParent(parentCanvas.transform, false);
            
            // Setup button component
            Button button = buttonObject.AddComponent<Button>();
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.3f, 0.8f, 0.8f);
            
            // Position button
            SetPanelPosition(buttonRect, buttonPosition, new Vector2(16, 16));
            buttonRect.sizeDelta = new Vector2(80, 40);
            
            // Add button text
            GameObject textObject = new GameObject("Text");
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.SetParent(buttonRect, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            Text buttonText = textObject.AddComponent<Text>();
            buttonText.text = category ? category.categoryName : "Custom";
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 12;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            
            // Create panel
            RectTransform panel = CreateExternalPanel(parentCanvas, panelName, panelPosition);
            
            // Setup handler
            SetupCustomCategoryButton(button, panel, category);
            
            return (button, panel);
        }
        
        /// <summary>
        /// Helper method to set panel position based on UIPanelPosition enum
        /// </summary>
        public void SetPanelPosition(RectTransform rectTransform, UIPanelPosition position, Vector2 offset)
        {
            Vector2 anchorMin, anchorMax, pivot;
            
            switch (position)
            {
                case UIPanelPosition.TopLeft:
                    anchorMin = anchorMax = new Vector2(0, 1);
                    pivot = new Vector2(0, 1);
                    break;
                case UIPanelPosition.TopCenter:
                    anchorMin = anchorMax = new Vector2(0.5f, 1);
                    pivot = new Vector2(0.5f, 1);
                    break;
                case UIPanelPosition.TopRight:
                    anchorMin = anchorMax = new Vector2(1, 1);
                    pivot = new Vector2(1, 1);
                    break;
                case UIPanelPosition.MiddleLeft:
                    anchorMin = anchorMax = new Vector2(0, 0.5f);
                    pivot = new Vector2(0, 0.5f);
                    break;
                case UIPanelPosition.MiddleCenter:
                    anchorMin = anchorMax = new Vector2(0.5f, 0.5f);
                    pivot = new Vector2(0.5f, 0.5f);
                    break;
                case UIPanelPosition.MiddleRight:
                    anchorMin = anchorMax = new Vector2(1, 0.5f);
                    pivot = new Vector2(1, 0.5f);
                    break;
                case UIPanelPosition.BottomLeft:
                    anchorMin = anchorMax = new Vector2(0, 0);
                    pivot = new Vector2(0, 0);
                    break;
                case UIPanelPosition.BottomCenter:
                    anchorMin = anchorMax = new Vector2(0.5f, 0);
                    pivot = new Vector2(0.5f, 0);
                    break;
                case UIPanelPosition.BottomRight:
                    anchorMin = anchorMax = new Vector2(1, 0);
                    pivot = new Vector2(1, 0);
                    break;
                default:
                    return; // Don't change for custom
            }
            
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = offset;
        }
        
        #if UNITY_EDITOR
        [Header("Editor Tools")]
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private UIPanelPosition testButtonPosition = UIPanelPosition.BottomLeft;
        [SerializeField] private UIPanelPosition testPanelPosition = UIPanelPosition.TopLeft;
        [SerializeField] private BuildableObjectUICategorySO testCategory;
        
        [ContextMenu("Create Test Setup")]
        private void CreateTestSetup()
        {
            if (targetCanvas == null)
            {
                targetCanvas = FindObjectOfType<Canvas>();
            }
            
            if (targetCanvas == null)
            {
                Debug.LogError("No Canvas found! Please assign a target canvas.");
                return;
            }
            
            CreateCompleteCustomCategorySetup(targetCanvas, testButtonPosition, testPanelPosition, testCategory, "Test Button", "Test Panel");
            Debug.Log("Test setup created!");
        }
        #endif
    }
} 