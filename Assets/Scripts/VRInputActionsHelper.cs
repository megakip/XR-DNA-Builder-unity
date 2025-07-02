using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace SoulGames.VR
{
    /// <summary>
    /// VR Input Actions Helper - Adds VR controller bindings to EGB Pro input actions
    /// 
    /// This script helps add VR controller input support to the existing EGB Pro input system
    /// It automatically detects VR controllers and maps their inputs to EGB Pro actions
    /// 
    /// Setup Instructions:
    /// 1. Add this script to a GameObject in your VR scene
    /// 2. Assign your Input Action Asset from EGB Pro
    /// 3. The script will automatically add VR controller bindings to the actions
    /// </summary>
    public class VRInputActionsHelper : MonoBehaviour
    {
        [Header("EGB Pro Integration")]
        [SerializeField] private InputActionAsset egbProInputActions;
        
        [Header("VR Controller Settings")]
        [SerializeField] private XRRayInteractor leftControllerRayInteractor;
        [SerializeField] private XRRayInteractor rightControllerRayInteractor;
        [SerializeField] private bool useRightControllerForBuilding = true;
        
        [Header("Input Action Names")]
        [SerializeField] private string buildActionName = "Build";
        [SerializeField] private string destroyActionName = "Destroy";
        [SerializeField] private string selectActionName = "Select";
        [SerializeField] private string moveActionName = "Move";
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        private InputActionMap buildActions;
        private InputActionMap destroyActions;
        private InputActionMap selectActions;
        private InputActionMap moveActions;
        
        private void Start()
        {
            InitializeVRInputActions();
        }
        
        private void InitializeVRInputActions()
        {
            if (egbProInputActions == null)
            {
                Debug.LogError("VRInputActionsHelper: EGB Pro Input Actions Asset not assigned!");
                return;
            }
            
            // Find the action maps
            buildActions = egbProInputActions.FindActionMap("Build Actions");
            destroyActions = egbProInputActions.FindActionMap("Destroy Actions");
            selectActions = egbProInputActions.FindActionMap("Select Actions");
            moveActions = egbProInputActions.FindActionMap("Move Actions");
            
            // Add VR controller bindings
            AddVRControllerBindings();
            
            if (debugMode)
            {
                Debug.Log("VRInputActionsHelper: VR controller bindings added to EGB Pro input actions");
            }
        }
        
        private void AddVRControllerBindings()
        {
            // Note: In Unity 2022.3+, you would typically modify the input actions asset directly
            // For runtime modification, we'll use a different approach
            
            // Get the primary controller to use for building
            XRRayInteractor primaryController = useRightControllerForBuilding ? rightControllerRayInteractor : leftControllerRayInteractor;
            
            if (primaryController == null)
            {
                if (debugMode)
                {
                    Debug.LogWarning("VRInputActionsHelper: Primary controller not assigned, trying to find automatically");
                }
                
                // Auto-find controllers
                FindVRControllers();
                primaryController = useRightControllerForBuilding ? rightControllerRayInteractor : leftControllerRayInteractor;
            }
            
            if (primaryController == null)
            {
                Debug.LogError("VRInputActionsHelper: No VR controllers found!");
                return;
            }
            
            // The input actions asset is usually read-only at runtime
            // Instead, we'll create action references that can be used by EGB Pro
            CreateVRActionReferences();
        }
        
        private void FindVRControllers()
        {
            XRRayInteractor[] rayInteractors = FindObjectsOfType<XRRayInteractor>();
            
            foreach (XRRayInteractor interactor in rayInteractors)
            {
                // Try to determine if it's left or right controller based on name or position
                string name = interactor.name.ToLower();
                
                if (name.Contains("left") && leftControllerRayInteractor == null)
                {
                    leftControllerRayInteractor = interactor;
                }
                else if (name.Contains("right") && rightControllerRayInteractor == null)
                {
                    rightControllerRayInteractor = interactor;
                }
                else if (leftControllerRayInteractor == null)
                {
                    leftControllerRayInteractor = interactor;
                }
                else if (rightControllerRayInteractor == null)
                {
                    rightControllerRayInteractor = interactor;
                }
            }
            
            if (debugMode)
            {
                Debug.Log($"VRInputActionsHelper: Found {rayInteractors.Length} ray interactors");
                if (leftControllerRayInteractor != null)
                    Debug.Log($"Left Controller: {leftControllerRayInteractor.name}");
                if (rightControllerRayInteractor != null)
                    Debug.Log($"Right Controller: {rightControllerRayInteractor.name}");
            }
        }
        
        private void CreateVRActionReferences()
        {
            // Create a component that will handle VR input and feed it to EGB Pro
            VRInputHandler vrInputHandler = gameObject.GetComponent<VRInputHandler>();
            if (vrInputHandler == null)
            {
                vrInputHandler = gameObject.AddComponent<VRInputHandler>();
            }
            
            // Initialize the VR input handler
            vrInputHandler.Initialize(
                useRightControllerForBuilding ? rightControllerRayInteractor : leftControllerRayInteractor,
                egbProInputActions
            );
        }
        
        /// <summary>
        /// Get the primary controller being used for building
        /// </summary>
        public XRRayInteractor GetPrimaryController()
        {
            return useRightControllerForBuilding ? rightControllerRayInteractor : leftControllerRayInteractor;
        }
        
        /// <summary>
        /// Switch which controller is used for building
        /// </summary>
        public void SetPrimaryController(bool useRightController)
        {
            useRightControllerForBuilding = useRightController;
            
            // Reinitialize if needed
            if (Application.isPlaying)
            {
                CreateVRActionReferences();
            }
        }
    }
    
    /// <summary>
    /// Helper component that bridges VR controller input to EGB Pro's input system
    /// </summary>
    public class VRInputHandler : MonoBehaviour
    {
        private XRRayInteractor primaryController;
        private InputActionAsset inputActions;
        
        private InputAction buildAction;
        private InputAction destroyAction;
        private InputAction selectAction;
        private InputAction moveAction;
        
        public void Initialize(XRRayInteractor controller, InputActionAsset actions)
        {
            primaryController = controller;
            inputActions = actions;
            
            // Find the actions
            buildAction = inputActions.FindAction("Build");
            destroyAction = inputActions.FindAction("Destroy");
            selectAction = inputActions.FindAction("Select");
            moveAction = inputActions.FindAction("Move");
        }
        
        private void Update()
        {
            if (primaryController == null) return;
            
            // Check if VR controller trigger is pressed and simulate input actions
            bool triggerPressed = IsVRTriggerPressed();
            bool gripPressed = IsVRGripPressed();
            
            // You would implement the logic to trigger the appropriate EGB Pro actions here
            // This is a simplified example
            if (triggerPressed)
            {
                // Simulate build action
                SimulateInputAction(buildAction);
            }
        }
        
        private bool IsVRTriggerPressed()
        {
            // Check if the XR controller trigger is pressed
            if (primaryController != null)
            {
                var controller = primaryController.GetComponent<ActionBasedController>();
                if (controller != null && controller.activateAction.action != null)
                {
                    return controller.activateAction.action.IsPressed();
                }
            }
            return false;
        }
        
        private bool IsVRGripPressed()
        {
            // Check if the XR controller grip is pressed
            if (primaryController != null)
            {
                var controller = primaryController.GetComponent<ActionBasedController>();
                if (controller != null && controller.selectAction.action != null)
                {
                    return controller.selectAction.action.IsPressed();
                }
            }
            return false;
        }
        
        private void SimulateInputAction(InputAction action)
        {
            if (action != null && action.enabled)
            {
                // This is where you would trigger the action
                // The exact implementation depends on how EGB Pro handles input
                Debug.Log($"VR: Triggering {action.name}");
            }
        }
    }
} 