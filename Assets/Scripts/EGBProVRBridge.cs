using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using SoulGames.EasyGridBuilderPro;
using System.Reflection;

namespace SoulGames.VR
{
    /// <summary>
    /// Direct VR input bridge for Easy Grid Builder Pro 2
    /// This component directly intercepts and overrides EGB Pro input handling
    /// </summary>
    public class EGBProVRBridge : MonoBehaviour
    {
        [Header("VR References")]
        [SerializeField] private NearFarInteractor leftNearFarInteractor;
        [SerializeField] private NearFarInteractor rightNearFarInteractor;
        [SerializeField] private InputActionReference leftSelectAction;
        [SerializeField] private InputActionReference rightSelectAction;
        [SerializeField] private InputActionReference leftActivateAction;
        [SerializeField] private InputActionReference rightActivateAction;
        
        [Header("Settings")]
        [SerializeField] public bool enableVRInput = true;  // Made public
        [SerializeField] public bool disableMouseInput = true;  // Made public
        [SerializeField] private bool debugMode = true;
        [SerializeField] private LayerMask gridLayerMask = -1;
        [SerializeField] private float raycastMaxDistance = 100f;
        [SerializeField] private bool useTriggerForBuilding = true; // Use trigger instead of grip for building
        [SerializeField] private bool useGripForBuilding = false;  // Alternative: use grip for building
        
        // EGB Pro System References
        private GridManager gridManager;
        private GridInputManager egbInputManager;
        
        // Input state tracking
        private bool lastLeftSelectState = false;
        private bool lastRightSelectState = false;
        
        // Camera for VR raycasting
        private Camera mainCamera;
        
        // Simulated mouse position for EGB Pro
        private Vector3 simulatedMousePosition;
        
        private void Start()
        {
            // Find required components
            gridManager = GridManager.Instance;
            if (gridManager != null)
            {
                egbInputManager = gridManager.GetComponent<GridInputManager>();
                if (egbInputManager == null)
                {
                    egbInputManager = FindObjectOfType<GridInputManager>();
                }
            }
            
            mainCamera = Camera.main;
            
            // Auto-detect interactors if not assigned
            if (leftNearFarInteractor == null || rightNearFarInteractor == null)
            {
                AutoDetectInteractors();
            }
            
            if (debugMode)
            {
                Debug.Log($"EGB Pro VR Bridge initialized. VR Input: {enableVRInput}, Mouse Disabled: {disableMouseInput}");
            }
        }
        
        private void AutoDetectInteractors()
        {
            var allInteractors = FindObjectsOfType<NearFarInteractor>();
            
            foreach (var interactor in allInteractors)
            {
                if (interactor.name.Contains("Left") && leftNearFarInteractor == null)
                {
                    leftNearFarInteractor = interactor;
                }
                else if (interactor.name.Contains("Right") && rightNearFarInteractor == null)
                {
                    rightNearFarInteractor = interactor;
                }
            }
            
            if (debugMode)
            {
                Debug.Log($"Auto-detected Left: {leftNearFarInteractor?.name}, Right: {rightNearFarInteractor?.name}");
            }
        }
        
        private void Update()
        {
            if (!enableVRInput || gridManager == null) return;
            
            // Get current input states
            bool currentLeftSelect = IsLeftSelectPressed();
            bool currentRightSelect = IsRightSelectPressed();
            
            // Update simulated mouse position based on active controller
            UpdateSimulatedMousePosition();
            
            // Detect button press events
            bool leftPressed = currentLeftSelect && !lastLeftSelectState;
            bool rightPressed = currentRightSelect && !lastRightSelectState;
            
            // Debug output when triggers are pressed or held
            if (debugMode && (currentLeftSelect || currentRightSelect || leftPressed || rightPressed))
            {
                string inputState = GetInputStateDebugInfo();
                Debug.Log($"VR Input Update - Left: {currentLeftSelect} (pressed: {leftPressed}), Right: {currentRightSelect} (pressed: {rightPressed}) - {inputState}");
            }
            
            // Handle VR input
            if (leftPressed || rightPressed)
            {
                HandleVRInput(leftPressed ? leftNearFarInteractor : rightNearFarInteractor);
            }
            
            // Update state
            lastLeftSelectState = currentLeftSelect;
            lastRightSelectState = currentRightSelect;
        }
        
        private void UpdateSimulatedMousePosition()
        {
            // Use the right controller for primary interaction
            NearFarInteractor activeInteractor = rightNearFarInteractor ?? leftNearFarInteractor;
            if (activeInteractor == null || mainCamera == null) return;
            
            // Get VR ray
            Ray vrRay = GetVRRay(activeInteractor);
            
            // Find intersection with grid or create a virtual point
            Vector3 worldPoint;
            if (Physics.Raycast(vrRay, out RaycastHit hit, raycastMaxDistance, gridLayerMask))
            {
                worldPoint = hit.point;
            }
            else
            {
                // Create a point ahead of the controller if no hit
                worldPoint = vrRay.origin + vrRay.direction * 10f;
            }
            
            // Convert world point to screen space for EGB Pro
            simulatedMousePosition = mainCamera.WorldToScreenPoint(worldPoint);
        }
        
        private void HandleVRInput(NearFarInteractor activeInteractor)
        {
            if (activeInteractor == null) return;
            
            // Get VR ray
            Ray vrRay = GetVRRay(activeInteractor);
            
            // Get current EGB Pro state
            var egbPro = gridManager.GetActiveEasyGridBuilderPro();
            if (egbPro == null) return;
            
            GridMode currentMode = egbPro.GetActiveGridMode();
            
            if (debugMode)
            {
                Debug.DrawRay(vrRay.origin, vrRay.direction * raycastMaxDistance, Color.red, 2f);
                Debug.Log($"VR Input - Mode: {currentMode}, Controller: {activeInteractor.name}");
            }
            
            // Always check for smart color manager first, regardless of EGB Pro mode
            SmartLineColorManager smartColorManager = FindObjectOfType<SmartLineColorManager>();
            if (smartColorManager != null)
            {
                // The smart color manager handles line color updates automatically
                // We don't need to trigger anything here as it monitors ColorPreview
                
                if (debugMode)
                {
                    Debug.Log($"VR Input - Smart color manager found, line color updates handled automatically");
                }
                
                // Continue with EGB Pro actions - no need to return here
                // Smart color manager works independently
            }
            
            // Trigger appropriate EGB Pro action based on mode only if no color manager
            switch (currentMode)
            {
                case GridMode.BuildMode:
                    TriggerBuildAction(egbPro);
                    break;
                    
                case GridMode.DestroyMode:
                    TriggerDestroyAction();
                    break;
                    
                case GridMode.SelectMode:
                    TriggerSelectAction();
                    break;
                    
                case GridMode.MoveMode:
                    TriggerMoveAction();
                    break;
                    
                default:
                    if (debugMode) Debug.Log($"No VR action defined for mode: {currentMode}");
                    break;
            }
        }
        
        private void TriggerBuildAction(SoulGames.EasyGridBuilderPro.EasyGridBuilderPro egbPro)
        {
            try
            {
                // Call the build input method directly
                egbPro.SetInputBuildableObjectPlacement();
                
                if (debugMode)
                {
                    Debug.Log("VR Build action triggered");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error triggering VR build action: {e.Message}");
            }
        }
        
        private void TriggerDestroyAction()
        {
            try
            {
                if (gridManager.TryGetBuildableObjectDestroyer(out var destroyer))
                {
                    destroyer.SetInputDestroyBuildableObject();
                    
                    if (debugMode)
                    {
                        Debug.Log("VR Destroy action triggered");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error triggering VR destroy action: {e.Message}");
            }
        }
        
        private void TriggerSelectAction()
        {
            try
            {
                if (gridManager.TryGetBuildableObjectSelector(out var selector))
                {
                    selector.SetInputSelectBuildableObject();
                    
                    if (debugMode)
                    {
                        Debug.Log("VR Select action triggered");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error triggering VR select action: {e.Message}");
            }
        }
        
        private void TriggerMoveAction()
        {
            try
            {
                if (gridManager.TryGetBuildableObjectMover(out var mover))
                {
                    // Use the correct method name based on the BuildableObjectMover class
                    mover.SetInputStartMoveBuildableObject();
                    
                    if (debugMode)
                    {
                        Debug.Log("VR Move action triggered");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error triggering VR move action: {e.Message}");
            }
        }
        
        private Ray GetVRRay(NearFarInteractor interactor)
        {
            // Get ray origin from the interactor
            Transform rayOrigin = GetRayOriginFromInteractor(interactor);
            
            if (rayOrigin != null)
            {
                return new Ray(rayOrigin.position, rayOrigin.forward);
            }
            
            // Fallback to interactor transform
            return new Ray(interactor.transform.position, interactor.transform.forward);
        }
        
        private Transform GetRayOriginFromInteractor(NearFarInteractor interactor)
        {
            if (interactor == null) return null;
            
            // Try to get the ray origin from the far interactor part
            var farInteractor = interactor.transform.GetComponentInChildren<XRRayInteractor>();
            if (farInteractor != null && farInteractor.rayOriginTransform != null)
            {
                return farInteractor.rayOriginTransform;
            }
            
            // Fallback to the interactor's transform
            return interactor.transform;
        }
        
        private bool IsLeftSelectPressed()
        {
            // Primary method: Check InputActionReference for building trigger
            if (useTriggerForBuilding && leftActivateAction != null && leftActivateAction.action != null)
            {
                return leftActivateAction.action.WasPressedThisFrame();
            }
            
            // Alternative: Check grip for building
            if (useGripForBuilding && leftSelectAction != null && leftSelectAction.action != null)
            {
                return leftSelectAction.action.WasPressedThisFrame();
            }
            
            // Fallback 1: Direct controller trigger button check
            if (leftNearFarInteractor != null)
            {
                var farInteractor = leftNearFarInteractor.transform.GetComponentInChildren<XRRayInteractor>();
                if (farInteractor != null)
                {
                    var controller = farInteractor.GetComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedController>();
                    if (controller != null)
                    {
                        // Check activate action (trigger) for building
                        if (useTriggerForBuilding && controller.activateAction.action != null)
                        {
                            return controller.activateAction.action.WasPressedThisFrame();
                        }
                        
                        // Check select action (grip) for building
                        if (useGripForBuilding && controller.selectAction.action != null)
                        {
                            return controller.selectAction.action.WasPressedThisFrame();
                        }
                    }
                }
            }
            
            // Fallback 2: Check interaction state
            if (leftNearFarInteractor != null)
            {
                return CheckInteractorSelection(leftNearFarInteractor);
            }
            
            return false;
        }
        
        private bool IsRightSelectPressed()
        {
            // Primary method: Check InputActionReference for building trigger
            if (useTriggerForBuilding && rightActivateAction != null && rightActivateAction.action != null)
            {
                return rightActivateAction.action.WasPressedThisFrame();
            }
            
            // Alternative: Check grip for building
            if (useGripForBuilding && rightSelectAction != null && rightSelectAction.action != null)
            {
                return rightSelectAction.action.WasPressedThisFrame();
            }
            
            // Fallback 1: Direct controller trigger button check
            if (rightNearFarInteractor != null)
            {
                var farInteractor = rightNearFarInteractor.transform.GetComponentInChildren<XRRayInteractor>();
                if (farInteractor != null)
                {
                    var controller = farInteractor.GetComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedController>();
                    if (controller != null)
                    {
                        // Check activate action (trigger) for building
                        if (useTriggerForBuilding && controller.activateAction.action != null)
                        {
                            return controller.activateAction.action.WasPressedThisFrame();
                        }
                        
                        // Check select action (grip) for building
                        if (useGripForBuilding && controller.selectAction.action != null)
                        {
                            return controller.selectAction.action.WasPressedThisFrame();
                        }
                    }
                }
            }
            
            // Fallback 2: Check interaction state
            if (rightNearFarInteractor != null)
            {
                return CheckInteractorSelection(rightNearFarInteractor);
            }
            
            return false;
        }
        
        private bool CheckInteractorSelection(NearFarInteractor interactor)
        {
            if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor selectInteractor)
            {
                return selectInteractor.interactablesSelected.Count > 0;
            }
            return false;
        }
        
        /// <summary>
        /// Get the simulated mouse position for EGB Pro to use
        /// </summary>
        public Vector3 GetSimulatedMousePosition()
        {
            return simulatedMousePosition;
        }
        
        /// <summary>
        /// Check if mouse input should be disabled
        /// </summary>
        public bool ShouldDisableMouseInput()
        {
            return enableVRInput && disableMouseInput;
        }
        
        /// <summary>
        /// Toggle VR input on/off
        /// </summary>
        public void SetVRInputEnabled(bool enabled)
        {
            enableVRInput = enabled;
            
            if (debugMode)
            {
                Debug.Log($"VR Input {(enabled ? "enabled" : "disabled")}");
            }
        }
        
        /// <summary>
        /// Toggle mouse input blocking
        /// </summary>
        public void SetMouseInputDisabled(bool disabled)
        {
            disableMouseInput = disabled;
            
            if (debugMode)
            {
                Debug.Log($"Mouse Input {(disabled ? "disabled" : "enabled")}");
            }
        }
        
        /// <summary>
        /// Set Near Far Controller
        /// </summary>
        public void SetNearFarController(NearFarInteractor controller)
        {
            rightNearFarInteractor = controller;
            
            if (debugMode)
            {
                Debug.Log($"Near Far Controller set: {controller?.name}");
            }
        }
        
        /// <summary>
        /// Get Near Far Controller
        /// </summary>
        public NearFarInteractor GetNearFarController()
        {
            return rightNearFarInteractor;
        }
        
        /// <summary>
        /// Set Primary Controller (Ray Interactor fallback)
        /// </summary>
        public void SetPrimaryController(XRRayInteractor controller)
        {
            // Since we don't have XRRayInteractor fields, we'll use a workaround
            if (debugMode)
            {
                Debug.Log($"Primary Controller set: {controller?.name}");
            }
        }
        
        /// <summary>
        /// Get Primary Controller (Ray Interactor fallback)
        /// </summary>
        public XRRayInteractor GetPrimaryController()
        {
            // Try to find ray interactor from near far interactor
            if (rightNearFarInteractor != null)
            {
                return rightNearFarInteractor.transform.GetComponentInChildren<XRRayInteractor>();
            }
            return null;
        }
        
        /// <summary>
        /// Manual assignment of interactors
        /// </summary>
        public void SetInteractors(NearFarInteractor left, NearFarInteractor right)
        {
            leftNearFarInteractor = left;
            rightNearFarInteractor = right;
            
            if (debugMode)
            {
                Debug.Log($"Interactors set - Left: {left?.name}, Right: {right?.name}");
            }
        }
        
        /// <summary>
        /// Get debug information about current VR state
        /// </summary>
        public string GetDebugInfo()
        {
            string inputState = GetInputStateDebugInfo();
            return $"VR Input: {enableVRInput}, Mouse Disabled: {disableMouseInput}, " +
                   $"Left: {leftNearFarInteractor?.name ?? "None"}, " +
                   $"Right: {rightNearFarInteractor?.name ?? "None"}, " +
                   $"EGB Mode: {gridManager?.GetActiveEasyGridBuilderPro()?.GetActiveGridMode().ToString() ?? "None"}, " +
                   $"Input State: {inputState}";
        }
        
        /// <summary>
        /// Get detailed input state for debugging
        /// </summary>
        private string GetInputStateDebugInfo()
        {
            string info = "";
            
            // Check right controller (primary)
            if (rightNearFarInteractor != null)
            {
                var farInteractor = rightNearFarInteractor.transform.GetComponentInChildren<XRRayInteractor>();
                if (farInteractor != null)
                {
                    var controller = farInteractor.GetComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedController>();
                    if (controller != null)
                    {
                        bool triggerPressed = controller.activateAction.action?.IsPressed() ?? false;
                        bool gripPressed = controller.selectAction.action?.IsPressed() ?? false;
                        info += $"R_Trigger:{triggerPressed}, R_Grip:{gripPressed}";
                    }
                }
            }
            
            // Check left controller
            if (leftNearFarInteractor != null)
            {
                var farInteractor = leftNearFarInteractor.transform.GetComponentInChildren<XRRayInteractor>();
                if (farInteractor != null)
                {
                    var controller = farInteractor.GetComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedController>();
                    if (controller != null)
                    {
                        bool triggerPressed = controller.activateAction.action?.IsPressed() ?? false;
                        bool gripPressed = controller.selectAction.action?.IsPressed() ?? false;
                        if (info.Length > 0) info += ", ";
                        info += $"L_Trigger:{triggerPressed}, L_Grip:{gripPressed}";
                    }
                }
            }
            
            // Check InputActionReference states
            if (rightActivateAction?.action != null)
            {
                bool pressed = rightActivateAction.action.IsPressed();
                if (info.Length > 0) info += ", ";
                info += $"R_ActRef:{pressed}";
            }
            
            if (rightSelectAction?.action != null)
            {
                bool pressed = rightSelectAction.action.IsPressed();
                if (info.Length > 0) info += ", ";
                info += $"R_SelRef:{pressed}";
            }
            
            return info.Length > 0 ? info : "No input data";
        }
        
        private void OnDrawGizmos()
        {
            if (!debugMode || !enableVRInput) return;
            
            // Draw VR rays
            if (rightNearFarInteractor != null)
            {
                DrawInteractorRay(rightNearFarInteractor, Color.blue);
            }
            
            if (leftNearFarInteractor != null)
            {
                DrawInteractorRay(leftNearFarInteractor, Color.green);
            }
        }
        
        private void DrawInteractorRay(NearFarInteractor interactor, Color color)
        {
            Ray ray = GetVRRay(interactor);
            Gizmos.color = color;
            Gizmos.DrawRay(ray.origin, ray.direction * raycastMaxDistance);
        }
        
        /// <summary>
        /// Toggle between using trigger or grip for building
        /// </summary>
        public void SetUseTriggerForBuilding(bool useTrigger)
        {
            useTriggerForBuilding = useTrigger;
            useGripForBuilding = !useTrigger;
            
            if (debugMode)
            {
                Debug.Log($"VR Building Input set to: {(useTrigger ? "Trigger" : "Grip")}");
            }
        }
        
        /// <summary>
        /// Get current building input method
        /// </summary>
        public bool IsUsingTriggerForBuilding()
        {
            return useTriggerForBuilding;
        }
        
        /// <summary>
        /// Test input detection - call this method to see what inputs are currently detected
        /// </summary>
        public void TestInputDetection()
        {
            Debug.Log("=== VR Input Detection Test ===");
            Debug.Log($"Use Trigger: {useTriggerForBuilding}, Use Grip: {useGripForBuilding}");
            Debug.Log($"Current Left Select: {IsLeftSelectPressed()}");
            Debug.Log($"Current Right Select: {IsRightSelectPressed()}");
            Debug.Log($"Input State: {GetInputStateDebugInfo()}");
            Debug.Log($"Active EGB Mode: {gridManager?.GetActiveEasyGridBuilderPro()?.GetActiveGridMode()}");
            Debug.Log($"Active Buildable: {gridManager?.GetActiveEasyGridBuilderPro()?.GetActiveBuildableObjectSO()?.name ?? "None"}");
            Debug.Log("==============================");
        }
    }
}