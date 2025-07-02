using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SoulGames.EasyGridBuilderPro;
using System.Collections;

namespace SoulGames.VR
{
    /// <summary>
    /// EGB Pro 2 UI Input Fixer
    /// Lost het probleem op van inconsistent selectie UI menu gedrag door:
    /// 1. Mouse input te blokkeren wanneer alleen raycast gebruikt wordt
    /// 2. Dubbele input events te voorkomen
    /// 3. UI panel state consistent te houden
    /// 
    /// Gebruik:
    /// - Voeg dit script toe aan hetzelfde GameObject als GridUIManager
    /// - Configureer de instellingen naar wens
    /// - Het script zal automatisch conflicten oplossen
    /// </summary>
    [RequireComponent(typeof(GridUIManager))]
    public class EGBProUIInputFixer : MonoBehaviour
    {
        [Header("Input Blocker Settings")]
        [SerializeField] private bool blockMouseInputForUI = true;
        [SerializeField] private bool onlyUseRaycastInput = true;
        [SerializeField] private bool preventDoubleInputs = true;
        [SerializeField] private float inputCooldownTime = 0.1f;
        
        [Header("UI Panel Settings")]
        [SerializeField] private bool fixSelectorPanelInconsistency = true;
        [SerializeField] private bool forceCloseOnClickOutside = true;
        [SerializeField] private float selectorPanelTimeout = 0.5f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugMessages = false;
        
        // Components
        private GridUIManager gridUIManager;
        private BuildableObjectSelector buildableObjectSelector;
        private GraphicRaycaster graphicRaycaster;
        private Canvas uiCanvas;
        
        // State tracking
        private bool lastInputState = false;
        private float lastInputTime = 0f;
        private bool selectorPanelVisible = false;
        private Coroutine selectorTimeoutCoroutine;
        
        // Original mouse settings
        private bool originalMouseInputEnabled = true;
        
        private void Start()
        {
            InitializeComponents();
            SetupInputFixer();
        }
        
        private void InitializeComponents()
        {
            gridUIManager = GetComponent<GridUIManager>();
            buildableObjectSelector = FindObjectOfType<BuildableObjectSelector>();
            uiCanvas = GetComponentInParent<Canvas>();
            
            if (uiCanvas != null)
            {
                graphicRaycaster = uiCanvas.GetComponent<GraphicRaycaster>();
            }
            
            if (showDebugMessages)
            {
                Debug.Log($"EGB Pro UI Input Fixer initialized. Canvas: {uiCanvas?.name}, Selector: {buildableObjectSelector?.name}");
            }
        }
        
        private void SetupInputFixer()
        {
            // Blokkeer mouse input voor UI als raycast-only mode actief is
            if (blockMouseInputForUI && onlyUseRaycastInput)
            {
                SetMouseInputForUI(false);
            }
            
            // Subscribe to selector events als beschikbaar
            if (buildableObjectSelector != null)
            {
                // Gebruik de echte EGB Pro selector events
                try
                {
                    buildableObjectSelector.OnBuildableObjectSelected += OnObjectSelected;
                    buildableObjectSelector.OnBuildableObjectDeselected += OnObjectDeselected;
                }
                catch (System.Exception e)
                {
                    if (showDebugMessages)
                        Debug.LogWarning($"Could not subscribe to selector events: {e.Message}");
                }
            }
        }
        
        private void Update()
        {
            if (preventDoubleInputs)
            {
                HandleInputCooldown();
            }
            
            if (fixSelectorPanelInconsistency)
            {
                HandleSelectorPanelConsistency();
            }
            
            if (forceCloseOnClickOutside)
            {
                HandleClickOutsideToClose();
            }
        }
        
        private void HandleInputCooldown()
        {
            // Check voor input en pas cooldown toe
            bool currentInput = Input.GetMouseButtonDown(0) || 
                               (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
            
            if (currentInput && Time.time - lastInputTime < inputCooldownTime)
            {
                // Blokkeer input - te snel na vorige input
                if (showDebugMessages)
                {
                    Debug.Log("Input blocked - too fast after previous input");
                }
                return;
            }
            
            if (currentInput)
            {
                lastInputTime = Time.time;
                lastInputState = true;
            }
            else
            {
                lastInputState = false;
            }
        }
        
        private void HandleSelectorPanelConsistency()
        {
            // Check selector panel state en forceer consistency
            if (buildableObjectSelector == null) return;
            
            var selectedObjects = buildableObjectSelector.GetSelectedObjectsList();
            bool shouldShowPanel = selectedObjects != null && selectedObjects.Count > 0;
            
            if (shouldShowPanel != selectorPanelVisible)
            {
                if (shouldShowPanel)
                {
                    ShowSelectorPanel();
                }
                else
                {
                    HideSelectorPanel();
                }
            }
        }
        
        private void HandleClickOutsideToClose()
        {
            // Sluit selector panel als er buiten geklikt wordt
            if (!selectorPanelVisible) return;
            
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                if (!IsPointerOverUI())
                {
                    if (showDebugMessages)
                    {
                        Debug.Log("Clicked outside UI - closing selector panel");
                    }
                    
                    HideSelectorPanel();
                    ClearSelection();
                }
            }
        }
        
        private bool IsPointerOverUI()
        {
            if (graphicRaycaster == null) return false;
            
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            
            // Use VR mouse position if available, otherwise regular mouse
            if (onlyUseRaycastInput && VRMouseInterceptor.IsVRInputActive())
            {
                eventData.position = VRMouseInterceptor.GetVRMousePosition();
            }
            else
            {
                eventData.position = Input.mousePosition;
            }
            
            var raycastResults = new System.Collections.Generic.List<RaycastResult>();
            graphicRaycaster.Raycast(eventData, raycastResults);
            
            return raycastResults.Count > 0;
        }
        
        private void OnObjectSelected(BuildableObject selectedObject)
        {
            if (showDebugMessages)
            {
                Debug.Log($"Object selected: {selectedObject?.name}");
            }
            
            ShowSelectorPanel();
            ResetSelectorTimeout();
        }
        
        private void OnObjectDeselected(BuildableObject deselectedObject)
        {
            if (showDebugMessages)
            {
                Debug.Log($"Object deselected: {deselectedObject?.name}");
            }
            
            // Check of er nog objecten geselecteerd zijn
            var selectedObjects = buildableObjectSelector?.GetSelectedObjectsList();
            if (selectedObjects == null || selectedObjects.Count == 0)
            {
                HideSelectorPanel();
            }
        }
        
        private void ShowSelectorPanel()
        {
            if (selectorPanelVisible) return;
            
            selectorPanelVisible = true;
            
            if (gridUIManager != null)
            {
                // Gebruik GridUIManager's ingebouwde functionaliteit
                var selectedObjects = buildableObjectSelector?.GetSelectedObjectsList();
                if (selectedObjects != null && selectedObjects.Count > 0)
                {
                    gridUIManager.ShowSelectorPanelForObject(selectedObjects[selectedObjects.Count - 1].gameObject);
                }
            }
            
            if (showDebugMessages)
            {
                Debug.Log("Selector panel shown");
            }
        }
        
        private void HideSelectorPanel()
        {
            if (!selectorPanelVisible) return;
            
            selectorPanelVisible = false;
            
            if (gridUIManager != null)
            {
                gridUIManager.HideSelectorPanel();
            }
            
            StopSelectorTimeout();
            
            if (showDebugMessages)
            {
                Debug.Log("Selector panel hidden");
            }
        }
        
        private void ResetSelectorTimeout()
        {
            StopSelectorTimeout();
            
            if (selectorPanelTimeout > 0)
            {
                selectorTimeoutCoroutine = StartCoroutine(SelectorTimeoutCoroutine());
            }
        }
        
        private void StopSelectorTimeout()
        {
            if (selectorTimeoutCoroutine != null)
            {
                StopCoroutine(selectorTimeoutCoroutine);
                selectorTimeoutCoroutine = null;
            }
        }
        
        private IEnumerator SelectorTimeoutCoroutine()
        {
            yield return new WaitForSeconds(selectorPanelTimeout);
            
            // Check of selector panel nog relevant is
            var selectedObjects = buildableObjectSelector?.GetSelectedObjectsList();
            if (selectedObjects == null || selectedObjects.Count == 0)
            {
                HideSelectorPanel();
            }
        }
        
        private void ClearSelection()
        {
            if (buildableObjectSelector != null)
            {
                // Gebruik de ResetIndividualSelection method van EGB Pro
                var method = buildableObjectSelector.GetType().GetMethod("ResetIndividualSelection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(buildableObjectSelector, new object[] { false });
                }
                else if (showDebugMessages)
                {
                    Debug.LogWarning("Could not find ResetIndividualSelection method");
                }
            }
        }
        
        private void SetMouseInputForUI(bool enabled)
        {
            if (graphicRaycaster != null)
            {
                // Disable/enable GraphicRaycaster for mouse input
                graphicRaycaster.enabled = enabled;
                
                if (showDebugMessages)
                {
                    Debug.Log($"Mouse input for UI {(enabled ? "enabled" : "disabled")}");
                }
            }
            
            // Zet ook XR UI Input Module mouse input uit/aan als beschikbaar
            var xrUIInputModule = FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule>();
            if (xrUIInputModule != null)
            {
                xrUIInputModule.enabled = !onlyUseRaycastInput || enabled;
                
                if (showDebugMessages)
                {
                    Debug.Log($"XR UI Input Module {(xrUIInputModule.enabled ? "enabled" : "disabled")}");
                }
            }
        }
        
        /// <summary>
        /// Publieke functies voor externe controle
        /// </summary>
        public void SetOnlyRaycastInput(bool raycastOnly)
        {
            onlyUseRaycastInput = raycastOnly;
            SetMouseInputForUI(!raycastOnly);
        }
        
        public void SetMouseInputBlocked(bool blocked)
        {
            blockMouseInputForUI = blocked;
            if (blocked && onlyUseRaycastInput)
            {
                SetMouseInputForUI(false);
            }
            else if (!blocked)
            {
                SetMouseInputForUI(true);
            }
        }
        
        public void ForceHideSelectorPanel()
        {
            HideSelectorPanel();
            ClearSelection();
        }
        
        public void ForceShowSelectorPanel()
        {
            ShowSelectorPanel();
        }
        
        public bool IsSelectorPanelVisible()
        {
            return selectorPanelVisible;
        }
        
        private void OnDestroy()
        {
            // Restore original settings
            SetMouseInputForUI(originalMouseInputEnabled);
            
            // Unsubscribe from events
            try
            {
                if (buildableObjectSelector != null)
                {
                    buildableObjectSelector.OnBuildableObjectSelected -= OnObjectSelected;
                    buildableObjectSelector.OnBuildableObjectDeselected -= OnObjectDeselected;
                }
            }
            catch (System.Exception)
            {
                // Ignore errors during cleanup
            }
        }
        
        /// <summary>
        /// Context menu functies voor testing
        /// </summary>
        [ContextMenu("Test Hide Selector Panel")]
        public void TestHideSelectorPanel()
        {
            ForceHideSelectorPanel();
        }
        
        [ContextMenu("Test Show Selector Panel")]
        public void TestShowSelectorPanel()
        {
            ForceShowSelectorPanel();
        }
        
        [ContextMenu("Toggle Raycast Only Mode")]
        public void ToggleRaycastOnlyMode()
        {
            SetOnlyRaycastInput(!onlyUseRaycastInput);
            Debug.Log($"Raycast only mode: {onlyUseRaycastInput}");
        }
    }
} 