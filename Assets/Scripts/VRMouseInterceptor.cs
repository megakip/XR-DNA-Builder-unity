using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.InputSystem;
using SoulGames.EasyGridBuilderPro;
using System.Reflection;
using System;

namespace SoulGames.VR
{
    /// <summary>
    /// VR Mouse Position Interceptor that overrides EGB Pro's mouse detection with VR ray input
    /// This component patches the MouseInteractionUtilities to work with VR controllers
    /// </summary>
    public class VRMouseInterceptor : MonoBehaviour
    {
        [Header("VR Setup")]
        [SerializeField] private NearFarInteractor leftController;
        [SerializeField] private NearFarInteractor rightController;
        [SerializeField] private bool autoDetectControllers = true;
        [SerializeField] private bool debugMode = true;
        
        [Header("Mouse Simulation")]
        [SerializeField] private bool enableMouseSimulation = true;
        [SerializeField] private LayerMask raycastLayers = -1;
        [SerializeField] private float maxRayDistance = 100f;
        
        // Static reference for mouse position override
        public static VRMouseInterceptor Instance { get; private set; }
        
        // VR state
        private Camera mainCamera;
        private Vector3 currentSimulatedMousePosition;
        private bool hasValidVRInput = false;
        
        // Active controller (right hand priority)
        private NearFarInteractor ActiveController => rightController != null ? rightController : leftController;
        
        private void Awake()
        {
            Instance = this;
            mainCamera = Camera.main;
            
            if (autoDetectControllers)
            {
                AutoDetectControllers();
            }
        }
        
        private void Start()
        {
            if (debugMode)
            {
                Debug.Log($"VR Mouse Interceptor started. Active controller: {ActiveController?.name ?? "None"}");
            }
        }
        
        private void AutoDetectControllers()
        {
            var controllers = FindObjectsOfType<NearFarInteractor>();
            
            foreach (var controller in controllers)
            {
                if (controller.name.ToLower().Contains("left") && leftController == null)
                {
                    leftController = controller;
                }
                else if (controller.name.ToLower().Contains("right") && rightController == null)
                {
                    rightController = controller;
                }
            }
            
            if (debugMode)
            {
                Debug.Log($"Auto-detected controllers - Left: {leftController?.name ?? "None"}, Right: {rightController?.name ?? "None"}");
            }
        }
        
        private void Update()
        {
            if (!enableMouseSimulation || mainCamera == null) return;
            
            UpdateVRMouseSimulation();
        }
        
        private void UpdateVRMouseSimulation()
        {
            var activeController = ActiveController;
            if (activeController == null)
            {
                hasValidVRInput = false;
                return;
            }
            
            // Get VR ray
            Ray vrRay = GetVRRay(activeController);
            
            // Perform raycast to find intersection
            if (Physics.Raycast(vrRay, out RaycastHit hit, maxRayDistance, raycastLayers))
            {
                // Convert world hit point to screen coordinates
                Vector3 screenPoint = mainCamera.WorldToScreenPoint(hit.point);
                
                // Clamp to screen bounds
                screenPoint.x = Mathf.Clamp(screenPoint.x, 0, Screen.width);
                screenPoint.y = Mathf.Clamp(screenPoint.y, 0, Screen.height);
                
                currentSimulatedMousePosition = screenPoint;
                hasValidVRInput = true;
                
                if (debugMode)
                {
                    Debug.DrawRay(vrRay.origin, vrRay.direction * hit.distance, Color.green);
                    Debug.DrawLine(hit.point, hit.point + Vector3.up * 0.1f, Color.red);
                }
            }
            else
            {
                // No hit - project ray forward and convert to screen space
                Vector3 projectedPoint = vrRay.origin + vrRay.direction * 10f;
                currentSimulatedMousePosition = mainCamera.WorldToScreenPoint(projectedPoint);
                hasValidVRInput = true;
                
                if (debugMode)
                {
                    Debug.DrawRay(vrRay.origin, vrRay.direction * maxRayDistance, Color.yellow);
                }
            }
        }
        
        private Ray GetVRRay(NearFarInteractor controller)
        {
            // Try to get ray origin from the far interactor component
            var rayInteractor = controller.GetComponentInChildren<XRRayInteractor>();
            if (rayInteractor != null && rayInteractor.rayOriginTransform != null)
            {
                Transform rayOrigin = rayInteractor.rayOriginTransform;
                return new Ray(rayOrigin.position, rayOrigin.forward);
            }
            
            // Fallback to controller transform
            return new Ray(controller.transform.position, controller.transform.forward);
        }
        
        /// <summary>
        /// Public method to get the current simulated mouse position for VR
        /// This method will be called by the patched MouseInteractionUtilities
        /// </summary>
        public static Vector3 GetVRMousePosition()
        {
            if (Instance == null || !Instance.hasValidVRInput)
            {
                // Fallback to regular mouse input
                if (Mouse.current != null)
                {
                    return Mouse.current.position.ReadValue();
                }
                return Vector3.zero;
            }
            
            return Instance.currentSimulatedMousePosition;
        }
        
        /// <summary>
        /// Check if VR input is currently active and valid
        /// </summary>
        public static bool IsVRInputActive()
        {
            return Instance != null && Instance.hasValidVRInput && Instance.enableMouseSimulation;
        }
        
        /// <summary>
        /// Get the world position where the VR ray is pointing
        /// </summary>
        public static Vector3 GetVRWorldPosition(LayerMask layerMask, float maxDistance = 100f)
        {
            if (Instance == null || Instance.ActiveController == null)
                return Vector3.zero;
            
            Ray vrRay = Instance.GetVRRay(Instance.ActiveController);
            
            if (Physics.Raycast(vrRay, out RaycastHit hit, maxDistance, layerMask))
            {
                return hit.point;
            }
            
            return Vector3.zero;
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!debugMode || !Application.isPlaying) return;
            
            var activeController = ActiveController;
            if (activeController != null)
            {
                Ray vrRay = GetVRRay(activeController);
                
                Gizmos.color = hasValidVRInput ? Color.green : Color.red;
                Gizmos.DrawRay(vrRay.origin, vrRay.direction * maxRayDistance);
                
                if (hasValidVRInput)
                {
                    // Draw screen position indicator
                    Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(currentSimulatedMousePosition.x, currentSimulatedMousePosition.y, 1f));
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(worldPos, 0.1f);
                }
            }
        }
    }
}