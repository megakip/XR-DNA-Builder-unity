using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SoulGames.VR
{
    /// <summary>
    /// VR UI Input Patch
    /// Verbetert de UI input handling voor VR door:
    /// 1. Consistente ray-based UI interaction
    /// 2. Preventie van conflicterende input events
    /// 3. Betere state management voor UI panels
    /// 
    /// Dit script werkt samen met EGBProUIInputFixer om alle UI input problemen op te lossen
    /// </summary>
    public class VRUIInputPatch : MonoBehaviour
    {
        [Header("VR UI Settings")]
        [SerializeField] private bool enableVRUIMode = true;
        [SerializeField] private bool disableMouseInputCompletely = true;
        [SerializeField] private bool useOnlyRaycastForUI = true;
        
        [Header("Input Event Prevention")]
        [SerializeField] private bool preventDuplicateEvents = true;
        [SerializeField] private float eventCooldownTime = 0.15f;
        
        [Header("UI Consistency")]
        [SerializeField] private bool forceUIConsistency = true;
        [SerializeField] private bool autoCloseOnLostFocus = true;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        // Event tracking
        private float lastUIEventTime = 0f;
        private static VRUIInputPatch instance;
        
        // UI State tracking
        private bool lastUIOverState = false;
        
        public static VRUIInputPatch Instance => instance;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            SetupVRUIMode();
        }
        
        private void Update()
        {
            if (enableVRUIMode)
            {
                HandleVRUIInput();
            }
            
            if (forceUIConsistency)
            {
                EnforceUIConsistency();
            }
        }
        
        private void SetupVRUIMode()
        {
            if (!enableVRUIMode) return;
            
            // Disable all Standalone Input Modules that might conflict
            var standaloneInputModules = FindObjectsOfType<StandaloneInputModule>();
            foreach (var module in standaloneInputModules)
            {
                if (disableMouseInputCompletely)
                {
                    module.enabled = false;
                    
                    if (debugMode)
                    {
                        Debug.Log($"Disabled StandaloneInputModule on {module.gameObject.name}");
                    }
                }
            }
            
            // Setup XR UI Input Module if available
            var xrUIModule = FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule>();
            if (xrUIModule != null)
            {
                xrUIModule.enabled = true;
                
                if (debugMode)
                {
                    Debug.Log($"Enabled XR UI Input Module on {xrUIModule.gameObject.name}");
                }
            }
            
            if (debugMode)
            {
                Debug.Log($"VR UI Mode setup complete. Mouse disabled: {disableMouseInputCompletely}");
            }
        }
        
        private void HandleVRUIInput()
        {
            // Check voor UI interaction events en apply cooldown
            bool isOverUI = IsPointerOverUI();
            
            if (preventDuplicateEvents && isOverUI != lastUIOverState)
            {
                if (Time.time - lastUIEventTime < eventCooldownTime)
                {
                    if (debugMode)
                    {
                        Debug.Log("UI event blocked - too fast after previous event");
                    }
                    return;
                }
                
                lastUIEventTime = Time.time;
            }
            
            lastUIOverState = isOverUI;
        }
        
        private void EnforceUIConsistency()
        {
            // Force consistent UI state based on current context
            if (autoCloseOnLostFocus)
            {
                // Check of er UI panels open zijn die gesloten moeten worden
                var egbUIFixer = FindObjectOfType<EGBProUIInputFixer>();
                if (egbUIFixer != null && egbUIFixer.IsSelectorPanelVisible())
                {
                    // Check of er nog steeds een valid selection is
                    var selector = FindObjectOfType<SoulGames.EasyGridBuilderPro.BuildableObjectSelector>();
                    if (selector != null)
                    {
                        var selectedObjects = selector.GetSelectedObjectsList();
                        if (selectedObjects == null || selectedObjects.Count == 0)
                        {
                            egbUIFixer.ForceHideSelectorPanel();
                            
                            if (debugMode)
                            {
                                Debug.Log("Forced selector panel close - no valid selection");
                            }
                        }
                    }
                }
            }
        }
        
        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;
            
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            
            // Use appropriate input source based on VR mode
            if (useOnlyRaycastForUI && VRMouseInterceptor.IsVRInputActive())
            {
                eventData.position = VRMouseInterceptor.GetVRMousePosition();
            }
            else
            {
                eventData.position = Input.mousePosition;
            }
            
            var raycastResults = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            
            return raycastResults.Count > 0;
        }
        
        /// <summary>
        /// Public API voor externe controle
        /// </summary>
        public void SetVRUIMode(bool enabled)
        {
            enableVRUIMode = enabled;
            
            if (enabled)
            {
                SetupVRUIMode();
            }
            else
            {
                RestoreNormalUIMode();
            }
        }
        
        public void SetMouseInputDisabled(bool disabled)
        {
            disableMouseInputCompletely = disabled;
            SetupVRUIMode();
        }
        
        public bool IsVRUIModeActive()
        {
            return enableVRUIMode;
        }
        
        public bool CanProcessUIInput()
        {
            if (!preventDuplicateEvents) return true;
            
            return Time.time - lastUIEventTime >= eventCooldownTime;
        }
        
        private void RestoreNormalUIMode()
        {
            // Re-enable Standalone Input Modules
            var standaloneInputModules = FindObjectsOfType<StandaloneInputModule>();
            foreach (var module in standaloneInputModules)
            {
                module.enabled = true;
            }
            
            if (debugMode)
            {
                Debug.Log("Restored normal UI input mode");
            }
        }
        
        /// <summary>
        /// Static helper functions
        /// </summary>
        public static bool ShouldBlockUIInput()
        {
            return Instance != null && Instance.enableVRUIMode && Instance.disableMouseInputCompletely;
        }
        
        public static bool IsVRModeActive()
        {
            return Instance != null && Instance.enableVRUIMode;
        }
        
        public static Vector3 GetUIInputPosition()
        {
            if (Instance != null && Instance.useOnlyRaycastForUI && VRMouseInterceptor.IsVRInputActive())
            {
                return VRMouseInterceptor.GetVRMousePosition();
            }
            
            return Input.mousePosition;
        }
        
        private void OnDestroy()
        {
            if (instance == this)
            {
                RestoreNormalUIMode();
                instance = null;
            }
        }
        
        /// <summary>
        /// Context menu functies voor testing
        /// </summary>
        [ContextMenu("Toggle VR UI Mode")]
        public void ToggleVRUIMode()
        {
            SetVRUIMode(!enableVRUIMode);
            Debug.Log($"VR UI Mode: {enableVRUIMode}");
        }
        
        [ContextMenu("Force UI Reset")]
        public void ForceUIReset()
        {
            var egbUIFixer = FindObjectOfType<EGBProUIInputFixer>();
            if (egbUIFixer != null)
            {
                egbUIFixer.ForceHideSelectorPanel();
            }
            
            Debug.Log("UI Reset forced");
        }
        
        [ContextMenu("Test UI Input")]
        public void TestUIInput()
        {
            bool isOverUI = IsPointerOverUI();
            Vector3 inputPos = GetUIInputPosition();
            
            Debug.Log($"UI Input Test - Over UI: {isOverUI}, Position: {inputPos}, Can Process: {CanProcessUIInput()}");
        }
    }
} 