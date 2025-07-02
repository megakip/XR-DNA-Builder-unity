using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SoulGames.EasyGridBuilderPro
{
    /// <summary>
    /// Custom Category Button Handler - Allows buttons to show/hide external panels with buildable objects
    /// This component can be attached to any Button to make it control an external buildables panel
    /// 
    /// Usage:
    /// 1. Attach this script to a Button GameObject
    /// 2. Assign the externalPanel (where buildable objects should appear)
    /// 3. Set the buildableObjectCategory to filter which objects to show
    /// 4. Configure the button template for spawning buildable buttons
    /// 
    /// Author: Easy Grid Builder Pro 2 - Custom Extension
    /// </summary>
    [AddComponentMenu("Easy Grid Builder Pro/Grid UI/Custom Category Button Handler", 0)]
    public class CustomCategoryButtonHandler : MonoBehaviour
    {
        [Header("Panel Configuration")]
        [Tooltip("The external panel where buildable object buttons will be spawned")]
        public RectTransform externalPanel;
        
        [Tooltip("Template button to use for creating buildable object buttons")]
        public RectTransform buildableButtonTemplate;
        
        [Tooltip("Specific category to show. If null, shows all objects from the active grid")]
        public BuildableObjectUICategorySO buildableObjectCategory;
        
        [Header("Behavior Settings")]
        [Tooltip("Close other external panels when this one opens")]
        public bool closeOtherPanelsOnOpen = true;
        
        [Tooltip("Always close the current active screen when opening this panel")]
        public bool alwaysCloseCurrentScreen = true;
        
        [Tooltip("Close this panel when clicking the button again")]
        public bool allowToggle = true;
        
        [Tooltip("Auto-close panel when switching grid modes")]
        public bool autoCloseOnGridModeChange = true;
        
        [Tooltip("Automatically enter build mode when selecting an object")]
        public bool autoEnterBuildMode = true;
        
        [Tooltip("Automatically exit build mode when clicking outside UI panels")]
        public bool autoExitBuildMode = true;
        
        [Tooltip("Keep build mode active after placing objects")]
        public bool keepBuildModeAfterPlacement = true;
        
        [Tooltip("Enter build mode immediately when opening this category panel")]
        public bool enterBuildModeOnPanelOpen = true;
        
        [Header("Animation Settings")]
        [Tooltip("Use fade in/out animation")]
        public bool useFadeAnimation = true;
        
        [Tooltip("Animation duration")]
        public float animationDuration = 0.3f;
        
        // Private variables
        private Button buttonComponent;
        private CanvasGroup panelCanvasGroup;
        private bool isPanelOpen = false;
        private List<RectTransform> spawnedButtons = new List<RectTransform>();
        private GridManager gridManager;
        private EasyGridBuilderPro activeEasyGridBuilderPro;
        
        // Static list to track all custom handlers for closing other panels
        private static List<CustomCategoryButtonHandler> allHandlers = new List<CustomCategoryButtonHandler>();
        
        private void Awake()
        {
            Debug.Log($"CustomCategoryButtonHandler: Awake called on {gameObject.name}");
            
            buttonComponent = GetComponent<Button>();
            if (buttonComponent == null)
            {
                Debug.LogError($"CustomCategoryButtonHandler requires a Button component on {gameObject.name}");
                enabled = false;
                return;
            }
            else
            {
                Debug.Log($"CustomCategoryButtonHandler: Button component found on {gameObject.name}");
            }
            
            // Setup canvas group for animations
            if (externalPanel && useFadeAnimation)
            {
                panelCanvasGroup = externalPanel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = externalPanel.gameObject.AddComponent<CanvasGroup>();
                }
            }
            
            // Register this handler
            allHandlers.Add(this);
            Debug.Log($"CustomCategoryButtonHandler: Registered handler for {gameObject.name}. Total handlers: {allHandlers.Count}");
        }
        
        private void Start()
        {
            Debug.Log($"CustomCategoryButtonHandler: Start called on {gameObject.name}");
            
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(OnButtonClicked);
                Debug.Log($"CustomCategoryButtonHandler: onClick listener added to {gameObject.name}");
            }
            else
            {
                Debug.LogError($"CustomCategoryButtonHandler: No button component found on {gameObject.name}");
            }
            
            // Start monitoring for screen changes
            StartCoroutine(MonitorScreenChanges());
            
            // Start monitoring for panel state changes (to handle back button)
            StartCoroutine(MonitorPanelState());
            
            // Get grid manager
            gridManager = GridManager.Instance;
            if (gridManager != null)
            {
                gridManager.OnActiveEasyGridBuilderProChanged += OnActiveEasyGridBuilderProChanged;
                gridManager.OnActiveGridModeChanged += OnActiveGridModeChanged;
                activeEasyGridBuilderPro = gridManager.GetActiveEasyGridBuilderPro();
            }
            
            // Ensure template button is hidden
            if (buildableButtonTemplate)
            {
                buildableButtonTemplate.gameObject.SetActive(false);
            }
            
            // Initially close panel
            if (externalPanel)
            {
                ClosePanel(false);
            }
        }
        
        /// <summary>
        /// Update method for immediate panel state checking
        /// </summary>
        private void Update()
        {
            // Check for external panel closure every frame for maximum responsiveness
            if (isPanelOpen && externalPanel && !externalPanel.gameObject.activeInHierarchy)
            {
                Debug.Log("CustomCategoryButtonHandler: Update detected panel was closed externally - immediate cleanup");
                OnPanelClosedExternally();
            }
        }
        
        private void OnDestroy()
        {
            if (buttonComponent != null)
            {
                buttonComponent.onClick.RemoveListener(OnButtonClicked);
            }
            
            if (gridManager != null)
            {
                gridManager.OnActiveEasyGridBuilderProChanged -= OnActiveEasyGridBuilderProChanged;
                gridManager.OnActiveGridModeChanged -= OnActiveGridModeChanged;
            }
            
            // Unregister this handler
            allHandlers.Remove(this);
        }
        
        private void OnActiveEasyGridBuilderProChanged(EasyGridBuilderPro newActiveGrid)
        {
            activeEasyGridBuilderPro = newActiveGrid;
            if (isPanelOpen)
            {
                PopulateBuildableObjects();
            }
        }
        
        private void OnActiveGridModeChanged(EasyGridBuilderPro easyGridBuilderPro, GridMode gridMode)
        {
            // If build mode is turned off and our panel is open, close it
            if (autoCloseOnGridModeChange && gridMode != GridMode.BuildMode && isPanelOpen)
            {
                Debug.Log("CustomCategoryButtonHandler: Grid mode changed to non-build mode - closing panel");
                ClosePanel();
            }
            
            // Check for panel state mismatch, but delay the check to avoid timing issues
            // This prevents false positives when the panel is just opening and entering build mode
            if (gridMode == GridMode.BuildMode && isPanelOpen && externalPanel)
            {
                // Only start coroutine if this GameObject is active
                if (gameObject.activeInHierarchy)
                {
                    StartCoroutine(DelayedPanelStateCheck());
                }
                else
                {
                    // If GameObject is inactive, do immediate check instead
                    if (!externalPanel.gameObject.activeInHierarchy)
                    {
                        Debug.Log("CustomCategoryButtonHandler: Immediate check detected panel state mismatch - cleaning up");
                        OnPanelClosedExternally();
                    }
                }
            }
        }
        
        private System.Collections.IEnumerator DelayedPanelStateCheck()
        {
            // Wait a frame to ensure the panel has time to fully activate
            yield return null;
            
            // Only check for mismatch if we're still supposed to be open and in build mode
            if (isPanelOpen && activeEasyGridBuilderPro && activeEasyGridBuilderPro.GetActiveGridMode() == GridMode.BuildMode)
            {
                if (externalPanel && !externalPanel.gameObject.activeInHierarchy)
                {
                    Debug.Log("CustomCategoryButtonHandler: Delayed check detected panel state mismatch - cleaning up");
                    OnPanelClosedExternally();
                }
            }
        }
        
        public void OnButtonClicked()
        {
            Debug.Log($"CustomCategoryButtonHandler: OnButtonClicked called on {gameObject.name}!");
            
            if (!externalPanel)
            {
                Debug.LogWarning($"External panel not assigned on {gameObject.name}");
                return;
            }
            
            // Check for state mismatch - if we think panel is open but it's actually closed
            if (isPanelOpen && !externalPanel.gameObject.activeInHierarchy)
            {
                Debug.Log($"CustomCategoryButtonHandler: Detected state mismatch - panel was closed externally, cleaning up");
                OnPanelClosedExternally();
                // Now open the panel as this was probably intended to be an open action
                OpenPanel();
                return;
            }
            
            // If this panel is already open, don't close it - just keep it open
            // This prevents the menu from disappearing when clicking the same category button
            if (isPanelOpen)
            {
                Debug.Log($"CustomCategoryButtonHandler: Panel {gameObject.name} is already open - keeping it open");
                return;
            }
            
            // Open this panel (which will close other category panels automatically)
            Debug.Log($"CustomCategoryButtonHandler: Opening panel on {gameObject.name}");
            OpenPanel();
        }
        
        public void OpenPanel()
        {
            if (!externalPanel || isPanelOpen) return;
            
            // Find current active screen FIRST
            GameObject currentScreen = FindCurrentActiveScreen();
            
            // Close the current active screen if enabled, but protect main menu/canvas objects
            if (alwaysCloseCurrentScreen && currentScreen != null && currentScreen != externalPanel.gameObject)
            {
                // Don't close objects that look like main menus or canvas containers
                bool isMainMenuOrCanvas = currentScreen.name.ToLower().Contains("main") || 
                                         currentScreen.name.ToLower().Contains("canvas") ||
                                         currentScreen.name.ToLower().Contains("menu") ||
                                         currentScreen.GetComponent<Canvas>() != null;
                
                if (!isMainMenuOrCanvas)
                {
                    Debug.Log($"CustomCategoryButtonHandler: Closing current active screen: {currentScreen.name}");
                    currentScreen.SetActive(false);
                    
                    // Register screen change for back button functionality
                    if (System.Type.GetType("SimpleBackButton") != null)
                    {
                        try
                        {
                            var method = System.Type.GetType("SimpleBackButton").GetMethod("RegisterScreenChange", 
                                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                            method?.Invoke(null, new object[] { externalPanel.gameObject, currentScreen });
                            Debug.Log($"CustomCategoryButtonHandler: Registered navigation from {currentScreen.name} to {externalPanel.gameObject.name}");
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning($"CustomCategoryButtonHandler: Could not register with SimpleBackButton: {e.Message}");
                        }
                    }
                }
                else
                {
                    Debug.Log($"CustomCategoryButtonHandler: Skipping close of main menu/canvas object: {currentScreen.name}");
                }
            }
            
            // Close other custom panels if needed
            if (closeOtherPanelsOnOpen)
            {
                CloseAllOtherPanels();
            }
            
            isPanelOpen = true;
            
            // Make sure the panel is active before we start working with it
            externalPanel.gameObject.SetActive(true);
            
            PopulateBuildableObjects();
            
            // Enter build mode immediately when panel opens (if enabled)
            if (enterBuildModeOnPanelOpen && activeEasyGridBuilderPro)
            {
                Debug.Log("CustomCategoryButtonHandler: Auto-entering build mode on panel open");
                activeEasyGridBuilderPro.SetActiveGridMode(GridMode.BuildMode);
                
                // Start watching for panel switches if auto-exit is enabled
                if (autoExitBuildMode && gameObject.activeInHierarchy)
                {
                    StartCoroutine(WatchForOutsideClick());
                }
            }
            
            if (useFadeAnimation && panelCanvasGroup)
            {
                // Ensure the panel is active and we can start coroutines
                if (gameObject.activeInHierarchy)
                {
                    StartCoroutine(FadePanel(0f, 1f));
                }
                else
                {
                    // If we can't animate, just show it immediately
                    panelCanvasGroup.alpha = 1f;
                }
            }
        }
        
        public void ClosePanel(bool animate = true)
        {
            if (!externalPanel || !isPanelOpen) return;
            
            isPanelOpen = false;
            ClearBuildableObjects();
            
            // Exit build mode when panel closes
            if (activeEasyGridBuilderPro && activeEasyGridBuilderPro.GetActiveGridMode() == GridMode.BuildMode)
            {
                Debug.Log("CustomCategoryButtonHandler: Exiting build mode because panel is closing");
                activeEasyGridBuilderPro.SetActiveGridMode(GridMode.None);
            }
            
            if (useFadeAnimation && panelCanvasGroup && animate && gameObject.activeInHierarchy)
            {
                StartCoroutine(FadePanel(1f, 0f));
            }
            else
            {
                externalPanel.gameObject.SetActive(false);
            }
        }
        
        private void CloseAllOtherPanels()
        {
            foreach (var handler in allHandlers)
            {
                if (handler != this && handler.isPanelOpen)
                {
                    handler.ClosePanel();
                }
            }
        }
        
        private void PopulateBuildableObjects()
        {
            if (!activeEasyGridBuilderPro)
            {
                Debug.LogWarning("CustomCategoryButtonHandler: No activeEasyGridBuilderPro found. Cannot populate buildable objects.");
                return;
            }
            
            ClearBuildableObjects();
            List<BuildableObjectSO> objectsToShow = GetBuildableObjectsToShow();
            
            Debug.Log($"CustomCategoryButtonHandler: Found {objectsToShow.Count} buildable objects to show for category: {(buildableObjectCategory ? buildableObjectCategory.name : "All")}");
            
            if (objectsToShow.Count == 0)
            {
                Debug.LogWarning($"CustomCategoryButtonHandler: No buildable objects found for category '{(buildableObjectCategory ? buildableObjectCategory.name : "All")}'. Check that objects are assigned to this category.");
            }
            
            foreach (var buildableObjectSO in objectsToShow)
            {
                CreateBuildableObjectButton(buildableObjectSO);
            }
            
            // Auto-select first object if build mode is enabled on panel open
            if (enterBuildModeOnPanelOpen && objectsToShow.Count > 0 && activeEasyGridBuilderPro)
            {
                var firstObject = objectsToShow[0];
                Debug.Log($"CustomCategoryButtonHandler: Auto-selecting first object: {firstObject.name}");
                
                // Set the selected buildable object for ALL grid systems
                if (gridManager)
                {
                    foreach (EasyGridBuilderPro easyGridBuilderPro in gridManager.GetEasyGridBuilderProSystemsList())
                    {
                        easyGridBuilderPro.SetInputActiveBuildableObjectSO(firstObject, onlySetBuildableExistInBuildablesList: true);
                    }
                }
            }
        }
        
        private List<BuildableObjectSO> GetBuildableObjectsToShow()
        {
            List<BuildableObjectSO> allObjects = new List<BuildableObjectSO>();
            
            // Get all buildable objects from the active grid
            var gridObjects = activeEasyGridBuilderPro.GetBuildableGridObjectSOList();
            var edgeObjects = activeEasyGridBuilderPro.GetBuildableEdgeObjectSOList();
            var cornerObjects = activeEasyGridBuilderPro.GetBuildableCornerObjectSOList();
            var freeObjects = activeEasyGridBuilderPro.GetBuildableFreeObjectSOList();
            
            allObjects.AddRange(gridObjects);
            allObjects.AddRange(edgeObjects);
            allObjects.AddRange(cornerObjects);
            allObjects.AddRange(freeObjects);
            
            Debug.Log($"CustomCategoryButtonHandler: Total objects from grid - Grid: {gridObjects.Count}, Edge: {edgeObjects.Count}, Corner: {cornerObjects.Count}, Free: {freeObjects.Count}");
            
            // Filter by category if specified
            if (buildableObjectCategory != null)
            {
                int beforeCount = allObjects.Count;
                allObjects.RemoveAll(obj => obj.buildableObjectUICategorySO != buildableObjectCategory);
                Debug.Log($"CustomCategoryButtonHandler: Filtered from {beforeCount} to {allObjects.Count} objects for category '{buildableObjectCategory.name}'");
            }
            
            return allObjects;
        }
        
        private void CreateBuildableObjectButton(BuildableObjectSO buildableObjectSO)
        {
            if (!buildableButtonTemplate)
            {
                Debug.LogError("CustomCategoryButtonHandler: buildableButtonTemplate is null! Please assign a button template in the inspector.");
                return;
            }
            
            if (!externalPanel)
            {
                Debug.LogError("CustomCategoryButtonHandler: externalPanel is null! Cannot create buttons.");
                return;
            }
            
            try
            {
                RectTransform buttonInstance = Instantiate(buildableButtonTemplate, externalPanel);
                if (buttonInstance == null)
                {
                    Debug.LogError($"CustomCategoryButtonHandler: Failed to instantiate button for {buildableObjectSO.name}");
                    return;
                }
                
                spawnedButtons.Add(buttonInstance);
                
                // Ensure the button instance is active before working with it
                buttonInstance.gameObject.SetActive(true);
                
                // Setup button functionality - check both root and children
                Button button = buttonInstance.GetComponent<Button>();
                if (button == null)
                {
                    // Try to find Button component in children
                    button = buttonInstance.GetComponentInChildren<Button>();
                }
                
                // Setup button appearance - find the ObjectIcon Image component specifically
                Image targetImage = null;
                
                // First, try to find ObjectIcon specifically
                Transform objectIconTransform = buttonInstance.transform.Find("ButtonContainer/Btn_Button/ObjectIcon");
                if (objectIconTransform == null)
                {
                    // Try recursive search for ObjectIcon
                    objectIconTransform = FindTransformRecursive(buttonInstance.transform, "ObjectIcon");
                }
                
                if (objectIconTransform != null)
                {
                    targetImage = objectIconTransform.GetComponent<Image>();
                    Debug.Log($"CustomCategoryButtonHandler: Found ObjectIcon Image component on {objectIconTransform.name}");
                }
                
                // Fallback: try to find an Image component on the same object as the Button
                if (targetImage == null && button != null)
                {
                    targetImage = button.GetComponent<Image>();
                    Debug.Log($"CustomCategoryButtonHandler: Using Button Image component as fallback on {button.gameObject.name}");
                }
                
                // Last resort: try to find any Image component
                if (targetImage == null)
                {
                    targetImage = buttonInstance.GetComponentInChildren<Image>();
                    Debug.Log($"CustomCategoryButtonHandler: Using first found Image component as last resort on {targetImage?.gameObject.name}");
                }
                
                if (targetImage && buildableObjectSO.objectIcon)
                {
                    Debug.Log($"CustomCategoryButtonHandler: Setting sprite for {buildableObjectSO.name} on {targetImage.gameObject.name}");
                    targetImage.sprite = buildableObjectSO.objectIcon;
                    targetImage.preserveAspect = true; // Preserve aspect ratio to avoid weird stretching
                }
                else if (buildableObjectSO.objectIcon)
                {
                    Debug.LogWarning($"CustomCategoryButtonHandler: Could not find Image component to set sprite for {buildableObjectSO.name}");
                }
                
                // Setup button text - find the Naam TextMeshPro component specifically
                TMPro.TextMeshProUGUI targetText = null;
                
                // First, try to find Naam specifically
                Transform nameTransform = buttonInstance.transform.Find("ButtonContainer/Naam");
                if (nameTransform == null)
                {
                    // Try recursive search for Naam
                    nameTransform = FindTransformRecursive(buttonInstance.transform, "Naam");
                }
                
                if (nameTransform != null)
                {
                    targetText = nameTransform.GetComponent<TMPro.TextMeshProUGUI>();
                    Debug.Log($"CustomCategoryButtonHandler: Found Naam TextMeshPro component on {nameTransform.name}");
                }
                
                // Fallback: try to find any TextMeshPro component
                if (targetText == null)
                {
                    targetText = buttonInstance.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    Debug.Log($"CustomCategoryButtonHandler: Using first found TextMeshPro component as fallback on {targetText?.gameObject.name}");
                }
                
                if (targetText && !string.IsNullOrEmpty(buildableObjectSO.objectName))
                {
                    Debug.Log($"CustomCategoryButtonHandler: Setting text for {buildableObjectSO.name} on {targetText.gameObject.name} to: {buildableObjectSO.objectName}");
                    targetText.text = buildableObjectSO.objectName;
                }
                else if (!string.IsNullOrEmpty(buildableObjectSO.objectName))
                {
                    Debug.LogWarning($"CustomCategoryButtonHandler: Could not find TextMeshPro component to set text for {buildableObjectSO.name}");
                }
                else
                {
                    Debug.LogWarning($"CustomCategoryButtonHandler: No objectName found in ScriptableObject {buildableObjectSO.name}");
                }
                
                // NIEUW: Configureer Product Information velden (alleen als objectName bestaat)
                if (!string.IsNullOrEmpty(buildableObjectSO?.objectName))
                {
                    try
                    {
                        ConfigureProductInformation(buttonInstance, buildableObjectSO);
                    }
                    catch (System.Exception productInfoException)
                    {
                        Debug.LogWarning($"CustomCategoryButtonHandler: Error configuring product information for {buildableObjectSO.name}: {productInfoException.Message}");
                    }
                }

                if (button)
                {
                    Debug.Log($"CustomCategoryButtonHandler: Found Button component for {buildableObjectSO.name} on {button.gameObject.name}");
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => {
                        Debug.Log($"CustomCategoryButtonHandler: Buildable object button clicked for {buildableObjectSO.name}");
                        OnBuildableObjectSelected(buildableObjectSO);
                    });
                }
                else
                {
                    Debug.LogError($"CustomCategoryButtonHandler: No Button component found on spawned buildable object button for {buildableObjectSO.name}. Checked both root and children.");
                    
                    // Debug: show hierarchy structure
                    Debug.LogError($"Template button hierarchy for debugging:");
                    LogHierarchy(buttonInstance, 0);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CustomCategoryButtonHandler: Exception while creating button for {buildableObjectSO.name}: {e.Message}");
            }
        }
        
        private void OnBuildableObjectSelected(BuildableObjectSO buildableObjectSO)
        {
            if (gridManager)
            {
                Debug.Log($"CustomCategoryButtonHandler: Selecting buildable object: {buildableObjectSO.name}");
                
                // Set the selected buildable object for ALL grid systems (like the original does)
                foreach (EasyGridBuilderPro easyGridBuilderPro in gridManager.GetEasyGridBuilderProSystemsList())
                {
                    easyGridBuilderPro.SetInputActiveBuildableObjectSO(buildableObjectSO, onlySetBuildableExistInBuildablesList: true);
                }
                
                // Auto-enter build mode if enabled (set for active grid system)
                if (autoEnterBuildMode && activeEasyGridBuilderPro)
                {
                    Debug.Log("CustomCategoryButtonHandler: Auto-entering build mode");
                    activeEasyGridBuilderPro.SetActiveGridMode(GridMode.BuildMode);
                }
                
                // Start listening for outside clicks to auto-exit build mode (only if not keeping build mode)
                if (autoExitBuildMode && !keepBuildModeAfterPlacement && gameObject.activeInHierarchy)
                {
                    StartCoroutine(WatchForOutsideClick());
                }
                
                // DON'T close panel after selection - keep it open for multiple selections
                // User will close manually or through back button
            }
        }
        
        /// <summary>
        /// Configureer alle Product Information velden in de item spawn button
        /// </summary>
        private void ConfigureProductInformation(Transform buttonInstance, BuildableObjectSO buildableObjectSO)
        {
            if (buildableObjectSO == null) return;

            Debug.Log($"[CustomCategoryButtonHandler] *** CONFIGUREREN PRODUCT INFORMATION VOOR: {buildableObjectSO.objectName} ***");

            // DEBUG: Eerst de volledige button hiërarchie bekijken
            Debug.Log($"=== VOLLEDIGE BUTTON HIËRARCHIE VOOR {buildableObjectSO.objectName} ===");
            LogCompleteHierarchy(buttonInstance, 0, 4); // 4 levels deep

            // Zoek het informatieframe binnen de button hiërarchie  
            Transform informatieframe = FindTransformRecursive(buttonInstance, "Informatie inhoud frame");
            if (informatieframe == null)
            {
                informatieframe = FindTransformRecursive(buttonInstance, "Informatieframe");
            }

            if (informatieframe == null)
            {
                Debug.LogWarning($"[CustomCategoryButtonHandler] Informatieframe niet gevonden voor {buildableObjectSO.objectName}");
                Debug.LogWarning($"[CustomCategoryButtonHandler] Probeer alternatieve zoek termen...");
                
                // Probeer alternatieve namen
                string[] alternativeNames = { "Info", "Information", "Details", "Content", "Frame", "Panel" };
                foreach (string altName in alternativeNames)
                {
                    informatieframe = FindTransformRecursive(buttonInstance, altName);
                    if (informatieframe != null)
                    {
                        Debug.Log($"[CustomCategoryButtonHandler] Informatieframe gevonden met alternatieve naam: {altName}");
                        break;
                    }
                }
                
                if (informatieframe == null)
                {
                    Debug.LogError($"[CustomCategoryButtonHandler] Geen geschikt informatieframe gevonden voor {buildableObjectSO.objectName}");
                    return;
                }
            }
            else
            {
                Debug.Log($"[CustomCategoryButtonHandler] Informatieframe gevonden: {informatieframe.name}");
            }

            // Dictionary voor field mapping: UI element naam -> ScriptableObject veld
            // Namen aangepast op basis van nieuwe prefab structuur (rode pijlen screenshot)
            // VEILIG: Gebruik null-safe strings om NullReferenceExceptions te voorkomen
            var fieldMappings = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Omschrijving", buildableObjectSO.omschrijving ?? "" },        // Direct onder eerste Togglebutton
                { "Levensduur", buildableObjectSO.levensduur ?? "" },            // In LevensduurContainer
                { "Recyclebaar", buildableObjectSO.recyclebaar ?? "" },          // In RecyclebaarContainer
                { "Prijs", buildableObjectSO.prijs ?? "" },                      // In PrijsContainer
                { "Certificeringen", buildableObjectSO.beschikbareCertificering ?? "" }, // In CertificeringenContainer (niet "Beschikbare certificeringen")
                { "CO2", buildableObjectSO.co2 ?? "" }                           // In CO2Container
            };

            Debug.Log($"[CustomCategoryButtonHandler] Data check voor {buildableObjectSO.objectName}:");
            Debug.Log($"  - Omschrijving: '{buildableObjectSO.omschrijving ?? "null"}'");
            Debug.Log($"  - Levensduur: '{buildableObjectSO.levensduur ?? "null"}'"); 
            Debug.Log($"  - Recyclebaar: '{buildableObjectSO.recyclebaar ?? "null"}'");
            Debug.Log($"  - Prijs: '{buildableObjectSO.prijs ?? "null"}'");
            Debug.Log($"  - Certificering: '{buildableObjectSO.beschikbareCertificering ?? "null"}'");
            Debug.Log($"  - CO2: '{buildableObjectSO.co2 ?? "null"}'");

            // NIEUW: Configureer het object icon in het informatieframe
            ConfigureInformatieframeIcon(informatieframe, buildableObjectSO);

            // Configureer elk tekstveld
            foreach (var mapping in fieldMappings)
            {
                ConfigureTextField(informatieframe, mapping.Key, mapping.Value, buildableObjectSO.objectName);
            }

            // Configureer certificering plaatjes apart
            ConfigureCertificatieImages(informatieframe, buildableObjectSO);
        }

        /// <summary>
        /// Configureer een individueel tekstveld in het informatieframe
        /// </summary>
        private void ConfigureTextField(Transform informatieframe, string fieldName, string fieldValue, string objectName)
        {
            Debug.Log($"[CustomCategoryButtonHandler] Zoeken naar veld '{fieldName}' voor {objectName}");
            
            // SPECIFIEK: Zoek door ALLE Togglebuttons naar het specifieke veld
            // Elk veld heeft zijn eigen Togglebutton > InfoButtonContainer structuur
            Transform fieldTransform = null;
            
            Debug.Log($"[CustomCategoryButtonHandler] Zoeken door alle Togglebuttons naar '{fieldName}'...");
            
            // Itereer door alle Togglebutton containers
            foreach (Transform child in informatieframe)
            {
                if (child.name.ToLower().Contains("togglebutton"))
                {
                    Debug.Log($"[CustomCategoryButtonHandler] Onderzoeken togglebutton: {child.name}");
                    
                    // Zoek InfoButtonContainer binnen deze togglebutton
                    Transform infoButtonContainer = null;
                    foreach (Transform grandChild in child)
                    {
                        if (grandChild.name.ToLower().Contains("infobuttoncontainer"))
                        {
                            infoButtonContainer = grandChild;
                            Debug.Log($"[CustomCategoryButtonHandler] Gevonden InfoButtonContainer: {grandChild.name} in {child.name}");
                            break;
                        }
                    }
                    
                    // Zoek het specifieke veld binnen InfoButtonContainer
                    if (infoButtonContainer != null)
                    {
                        // Zoek direct in InfoButtonContainer
                        foreach (Transform fieldChild in infoButtonContainer)
                        {
                            if (fieldChild.name.Equals(fieldName, System.StringComparison.OrdinalIgnoreCase))
                            {
                                fieldTransform = fieldChild;
                                Debug.Log($"[CustomCategoryButtonHandler] ✓ Gevonden veld: {fieldChild.name} op pad: {GetFullPath(fieldChild)}");
                                break;
                            }
                        }
                        
                        // Als niet direct gevonden, zoek ook in sub-containers (bijv. LevensduurContainer, PrijsContainer)
                        if (fieldTransform == null)
                        {
                            foreach (Transform container in infoButtonContainer)
                            {
                                // Zoek in containers die eindigen op "Container" en mogelijk het veld bevatten
                                if (container.name.ToLower().Contains(fieldName.ToLower()) || 
                                    container.name.ToLower().Contains("container"))
                                {
                                    Debug.Log($"[CustomCategoryButtonHandler] Zoeken in sub-container: {container.name}");
                                    
                                    foreach (Transform fieldChild in container)
                                    {
                                        if (fieldChild.name.Equals(fieldName, System.StringComparison.OrdinalIgnoreCase))
                                        {
                                            fieldTransform = fieldChild;
                                            Debug.Log($"[CustomCategoryButtonHandler] ✓ Gevonden veld in sub-container: {fieldChild.name} op pad: {GetFullPath(fieldChild)}");
                                            break;
                                        }
                                    }
                                    
                                    if (fieldTransform != null) break;
                                }
                            }
                        }
                        
                        if (fieldTransform != null) break; // Veld gevonden, stop zoeken
                    }
                }
            }
            
            // DEBUG: Als helemaal niet gevonden, laat alle beschikbare velden zien
            if (fieldTransform == null)
            {
                Debug.LogWarning($"[CustomCategoryButtonHandler] Veld '{fieldName}' niet gevonden in alle Togglebuttons.");
                Debug.Log($"[CustomCategoryButtonHandler] Beschikbare velden in alle InfoButtonContainers:");
                
                foreach (Transform child in informatieframe)
                {
                    if (child.name.ToLower().Contains("togglebutton"))
                    {
                        foreach (Transform grandChild in child)
                        {
                            if (grandChild.name.ToLower().Contains("infobuttoncontainer"))
                            {
                                Debug.Log($"  In {child.name} > {grandChild.name}:");
                                foreach (Transform fieldChild in grandChild)
                                {
                                    Debug.Log($"    - {fieldChild.name}");
                                    
                                    // Ook sub-containers tonen
                                    if (fieldChild.name.ToLower().Contains("container") && fieldChild.childCount > 0)
                                    {
                                        foreach (Transform subChild in fieldChild)
                                        {
                                            Debug.Log($"      └─ {subChild.name}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // Als niet gevonden via specifieke structuur, probeer recursief zoeken
            if (fieldTransform == null)
            {
                fieldTransform = FindTransformRecursive(informatieframe, fieldName);
                if (fieldTransform != null)
                {
                    Debug.Log($"[CustomCategoryButtonHandler] Veld gevonden via recursief zoeken");
                }
            }

            if (fieldTransform != null)
            {
                // Zoek TextMeshProUGUI component
                TMPro.TextMeshProUGUI textComponent = fieldTransform.GetComponent<TMPro.TextMeshProUGUI>();
                
                if (textComponent == null)
                {
                    // Probeer in child objecten
                    textComponent = fieldTransform.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        Debug.Log($"[CustomCategoryButtonHandler] TextMeshPro gevonden in child: {textComponent.gameObject.name}");
                    }
                }

                if (textComponent != null)
                {
                    string valueToSet = string.IsNullOrEmpty(fieldValue) ? "Niet opgegeven" : fieldValue;
                    textComponent.text = valueToSet;
                    Debug.Log($"[CustomCategoryButtonHandler] ✓ {fieldName} SUCCESVOL ingesteld voor {objectName}: '{valueToSet}'");
                }
                else
                {
                    Debug.LogWarning($"[CustomCategoryButtonHandler] ✗ Geen TextMeshProUGUI component gevonden voor {fieldName} in {objectName}");
                }
            }
            else
            {
                Debug.LogWarning($"[CustomCategoryButtonHandler] ✗ UI element '{fieldName}' niet gevonden voor {objectName}");
            }
        }

        /// <summary>
        /// Configureer het object icon in het informatieframe
        /// </summary>
        private void ConfigureInformatieframeIcon(Transform informatieframe, BuildableObjectSO buildableObjectSO)
        {
            if (buildableObjectSO?.objectIcon == null) 
            {
                Debug.LogWarning($"[CustomCategoryButtonHandler] Geen objectIcon gevonden voor {buildableObjectSO?.objectName}");
                return;
            }

            Debug.Log($"[CustomCategoryButtonHandler] Configureren icon in informatieframe voor: {buildableObjectSO.objectName}");

            // Zoek naar Object Icon in het informatieframe
            Transform iconTransform = FindTransformRecursive(informatieframe, "ObjectIcon");
            
            if (iconTransform == null)
            {
                // Probeer alternatieve namen
                string[] alternativeNames = { "Object Icon", "object-icon", "Icon", "Afbeelding", "Image" };
                foreach (string altName in alternativeNames)
                {
                    iconTransform = FindTransformRecursive(informatieframe, altName);
                    if (iconTransform != null)
                    {
                        Debug.Log($"[CustomCategoryButtonHandler] Icon gevonden met alternatieve naam: {altName}");
                        break;
                    }
                }
            }

            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = buildableObjectSO.objectIcon;
                    iconImage.enabled = true;
                    iconImage.color = Color.white;
                    Debug.Log($"[CustomCategoryButtonHandler] ✓ Icon succesvol ingesteld in informatieframe: {buildableObjectSO.objectIcon.name}");
                }
                else
                {
                    Debug.LogWarning($"[CustomCategoryButtonHandler] Geen Image component gevonden op {iconTransform.name} in informatieframe");
                }
            }
            else
            {
                Debug.LogWarning($"[CustomCategoryButtonHandler] Geen ObjectIcon gevonden in informatieframe voor {buildableObjectSO.objectName}");
                
                // DEBUG: Laat alle mogelijke icon objecten zien
                Debug.Log($"[CustomCategoryButtonHandler] Beschikbare objecten in informatieframe (zoeken naar mogelijke icons):");
                LogImageComponentsInChildren(informatieframe, 0);
            }
        }

        /// <summary>
        /// Debug methode om alle Image componenten in children te vinden
        /// </summary>
        private void LogImageComponentsInChildren(Transform parent, int depth)
        {
            if (depth > 3) return; // Max 3 levels diep
            
            string indent = new string(' ', depth * 2);
            
            // Check of dit object een Image component heeft
            Image imageComp = parent.GetComponent<Image>();
            if (imageComp != null)
            {
                Debug.Log($"{indent}📷 {parent.name} - Has Image component (sprite: {imageComp.sprite?.name ?? "none"})");
            }
            
            // Recursief door children
            for (int i = 0; i < parent.childCount; i++)
            {
                LogImageComponentsInChildren(parent.GetChild(i), depth + 1);
            }
        }

        /// <summary>
        /// Configureer certificering afbeeldingen in het informatieframe
        /// </summary>
        private void ConfigureCertificatieImages(Transform informatieframe, BuildableObjectSO buildableObjectSO)
        {
            if (buildableObjectSO?.certificeringPlaatjes == null || buildableObjectSO.certificeringPlaatjes.Length == 0) return;

            // Zoek naar certificering container - aangepast voor nieuwe prefab structuur
            string[] containerNames = { "Certificeringen", "CertificeringenContainer", "Certificering Plaatjes", "CertificeringPlaatjes", "Certificates" };
            Transform certificateContainer = null;

            foreach (string containerName in containerNames)
            {
                certificateContainer = FindTransformRecursive(informatieframe, containerName);
                if (certificateContainer != null) break;
            }

            if (certificateContainer != null)
            {
                // Deactiveer bestaande certificate images
                foreach (Transform child in certificateContainer)
                {
                    child.gameObject.SetActive(false);
                }

                // Activeer en configureer de benodigde afbeeldingen
                for (int i = 0; i < buildableObjectSO.certificeringPlaatjes.Length && i < certificateContainer.childCount; i++)
                {
                    Transform certificateImage = certificateContainer.GetChild(i);
                    Image imageComponent = certificateImage.GetComponent<Image>();
                    
                    if (imageComponent != null && buildableObjectSO.certificeringPlaatjes[i] != null)
                    {
                        imageComponent.sprite = buildableObjectSO.certificeringPlaatjes[i];
                        imageComponent.enabled = true;
                        certificateImage.gameObject.SetActive(true);
                        Debug.Log($"[CustomCategoryButtonHandler] Certificering afbeelding {i} ingesteld voor {buildableObjectSO.objectName}");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[CustomCategoryButtonHandler] Certificering container niet gevonden voor {buildableObjectSO.objectName}");
            }
        }

        private void ClearBuildableObjects()
        {
            foreach (var button in spawnedButtons)
            {
                if (button) DestroyImmediate(button.gameObject);
            }
            spawnedButtons.Clear();
        }
        
        private System.Collections.IEnumerator FadePanel(float startAlpha, float endAlpha)
        {
            if (!panelCanvasGroup) yield break;
            
            externalPanel.gameObject.SetActive(true);
            
            float elapsedTime = 0f;
            panelCanvasGroup.alpha = startAlpha;
            
            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / animationDuration;
                panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
                yield return null;
            }
            
            panelCanvasGroup.alpha = endAlpha;
            
            if (endAlpha <= 0f)
            {
                externalPanel.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Public method to manually set which category to show
        /// </summary>
        public void SetCategory(BuildableObjectUICategorySO category)
        {
            buildableObjectCategory = category;
            if (isPanelOpen)
            {
                PopulateBuildableObjects();
            }
        }
        
        /// <summary>
        /// Check if this panel is currently open
        /// </summary>
        public bool IsPanelOpen => isPanelOpen;
        
        /// <summary>
        /// Debug method to log hierarchy structure
        /// </summary>
        private void LogHierarchy(Transform parent, int depth)
        {
            string indent = new string(' ', depth * 2);
            Component[] components = parent.GetComponents<Component>();
            string componentList = "";
            foreach (var comp in components)
            {
                componentList += comp.GetType().Name + " ";
            }
            Debug.LogError($"{indent}{parent.name} - Components: {componentList}");
            
            for (int i = 0; i < parent.childCount; i++)
            {
                LogHierarchy(parent.GetChild(i), depth + 1);
            }
        }

        /// <summary>
        /// Debug method to log complete hierarchy structure with depth limit
        /// </summary>
        private void LogCompleteHierarchy(Transform parent, int depth, int maxDepth)
        {
            if (depth > maxDepth) return;
            
            string indent = new string(' ', depth * 2);
            Component[] components = parent.GetComponents<Component>();
            string componentList = "";
            foreach (var comp in components)
            {
                componentList += comp.GetType().Name + " ";
            }
            
            Debug.Log($"{indent}[{depth}] {parent.name} - Components: {componentList}");
            
            // Also show child count
            if (parent.childCount > 0)
            {
                Debug.Log($"{indent}    └─ Has {parent.childCount} children:");
                for (int i = 0; i < parent.childCount; i++)
                {
                    LogCompleteHierarchy(parent.GetChild(i), depth + 1, maxDepth);
                }
            }
        }
        
        /// <summary>
        /// Helper method to recursively find a Transform by name
        /// </summary>
        private Transform FindTransformRecursive(Transform parent, string name)
        {
            if (parent.name == name)
                return parent;
                
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform result = FindTransformRecursive(parent.GetChild(i), name);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Get the full path of a Transform in the hierarchy
        /// </summary>
        private string GetFullPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }
        
        /// <summary>
        /// Coroutine to watch for clicks that should exit build mode
        /// Only exits when clicking on other UI panels or non-grid areas
        /// </summary>
        private System.Collections.IEnumerator WatchForOutsideClick()
        {
            // Wait a frame to avoid detecting the same click that selected the object
            yield return null;
            
            while (activeEasyGridBuilderPro && activeEasyGridBuilderPro.GetActiveGridMode() == GridMode.BuildMode && isPanelOpen)
            {
                // Use Input System instead of legacy Input
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    // Check if we clicked on a UI element
                    bool clickedOnUI = UnityEngine.EventSystems.EventSystem.current?.IsPointerOverGameObject() ?? false;
                    
                    if (clickedOnUI)
                    {
                        // Check if we clicked on OUR panel or a different panel
                        var clickedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
                        
                        // If clicked on a different UI panel (not our external panel), exit build mode
                        if (clickedObject != null && !IsClickOnOurPanel(clickedObject))
                        {
                            Debug.Log("CustomCategoryButtonHandler: Clicked on different UI panel - exiting build mode");
                            activeEasyGridBuilderPro.SetActiveGridMode(GridMode.None);
                            yield break;
                        }
                    }
                }
                yield return null;
            }
        }
        
        /// <summary>
        /// Check if a clicked UI object belongs to our panel
        /// </summary>
        private bool IsClickOnOurPanel(GameObject clickedObject)
        {
            if (externalPanel == null || clickedObject == null) return false;
            
            // Check if the clicked object is a child of our external panel
            Transform parent = clickedObject.transform;
            while (parent != null)
            {
                if (parent == externalPanel)
                {
                    return true;
                }
                parent = parent.parent;
            }
            
            return false;
        }
        
        /// <summary>
        /// Find the current active screen for navigation purposes
        /// Improved detection to find the right active screen
        /// </summary>
        private GameObject FindCurrentActiveScreen()
        {
            // 1. Try to find Content object (common pattern in the navigation system)
            GameObject content = GameObject.Find("Content");
            if (content != null)
            {
                // Look for active children in Content
                foreach (Transform child in content.transform)
                {
                    if (child.gameObject.activeSelf && child.gameObject != externalPanel?.gameObject)
                    {
                        Debug.Log($"CustomCategoryButtonHandler: Found active screen in Content: {child.name}");
                        return child.gameObject;
                    }
                }
            }
            
            // 2. Look for common UI screen patterns
            string[] screenNames = { "Test", "MenuMain", "BuildMenu", "MainMenu", "Menu", "Screen" };
            
            foreach (string screenName in screenNames)
            {
                GameObject screen = GameObject.Find(screenName);
                if (screen != null && screen.activeSelf && screen != externalPanel?.gameObject)
                {
                    Debug.Log($"CustomCategoryButtonHandler: Found active screen by name: {screen.name}");
                    return screen;
                }
            }
            
            // 3. Look for any Canvas object with active UI panels that look like screens
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                foreach (Transform child in canvas.transform)
                {
                    if (child.gameObject.activeSelf && 
                        child.gameObject != externalPanel?.gameObject &&
                        (child.name.Contains("Test") || 
                         child.name.Contains("Menu") || 
                         child.name.Contains("Screen") ||
                         child.name.Contains("Panel")))
                    {
                        Debug.Log($"CustomCategoryButtonHandler: Found active screen in Canvas: {child.name}");
                        return child.gameObject;
                    }
                }
            }
            
            // 4. Fallback: Look for any active GameObject at root level that might be a screen
            GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject rootObj in rootObjects)
            {
                if (rootObj.activeSelf && 
                    rootObj != externalPanel?.gameObject &&
                    (rootObj.name.Contains("Test") || 
                     rootObj.name.Contains("Menu") || 
                     rootObj.name.Contains("Screen") ||
                     rootObj.name.Contains("UI")))
                {
                    Debug.Log($"CustomCategoryButtonHandler: Found active screen at root level: {rootObj.name}");
                    return rootObj;
                }
            }
            
            Debug.Log("CustomCategoryButtonHandler: No current active screen found");
            return null;
        }
        
        /// <summary>
        /// Monitor for screen changes to auto-close this panel when other screens become active
        /// </summary>
        private System.Collections.IEnumerator MonitorScreenChanges()
        {
            GameObject lastActiveScreen = null;
            
            while (this != null)
            {
                yield return new WaitForSeconds(0.5f); // Check every half second
                
                if (isPanelOpen)
                {
                    GameObject currentActiveScreen = FindCurrentActiveScreen();
                    
                    // If a different screen became active (and it's not our panel), close our panel
                    if (currentActiveScreen != null && 
                        currentActiveScreen != lastActiveScreen && 
                        currentActiveScreen != externalPanel?.gameObject &&
                        !IsClickOnOurPanel(currentActiveScreen))
                    {
                        Debug.Log($"CustomCategoryButtonHandler: Detected screen change to {currentActiveScreen.name} - closing panel");
                        ClosePanel();
                    }
                    
                    lastActiveScreen = currentActiveScreen;
                }
            }
        }
        
        /// <summary>
        /// Monitor if the panel gets closed by external means (like back button) and handle build mode exit
        /// Uses very frequent checks for immediate responsiveness
        /// </summary>
        private System.Collections.IEnumerator MonitorPanelState()
        {
            while (this != null)
            {
                // Check every frame for maximum responsiveness to back button
                yield return null;
                
                // If we think the panel is open but it's actually inactive, it was closed externally
                if (isPanelOpen && externalPanel && !externalPanel.gameObject.activeInHierarchy)
                {
                    Debug.Log("CustomCategoryButtonHandler: Panel was closed externally (e.g., back button) - handling cleanup");
                    OnPanelClosedExternally();
                }
            }
        }
        
        /// <summary>
        /// Called when the panel is closed by external means (like back button)
        /// This handles immediate cleanup and build mode exit
        /// </summary>
        public void OnPanelClosedExternally()
        {
            Debug.Log("CustomCategoryButtonHandler: OnPanelClosedExternally called - performing immediate cleanup");
            
            // Update our internal state immediately
            isPanelOpen = false;
            ClearBuildableObjects();
            
            // Exit build mode if we're in it
            if (activeEasyGridBuilderPro && activeEasyGridBuilderPro.GetActiveGridMode() == GridMode.BuildMode)
            {
                Debug.Log("CustomCategoryButtonHandler: Exiting build mode because panel was closed externally");
                activeEasyGridBuilderPro.SetActiveGridMode(GridMode.None);
            }
        }
    }
} 