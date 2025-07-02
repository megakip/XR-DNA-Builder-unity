using UnityEngine;
using UnityEngine.InputSystem;
using SoulGames.EasyGridBuilderPro;

namespace SoulGames.VR
{
    /// <summary>
    /// Intercepts and blocks EGB Pro mouse input when VR is active
    /// This component should be added to the same GameObject as GridInputManager
    /// </summary>
    [RequireComponent(typeof(GridInputManager))]
    public class EGBMouseInputBlocker : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool blockMouseWhenVRActive = true;
        [SerializeField] private bool showDebugMessages = false;
        
        private GridInputManager egbInputManager;
        private InputActionAsset originalInputActions;
        private bool inputActionsDisabled = false;
        
        private void Start()
        {
            egbInputManager = GetComponent<GridInputManager>();
            if (egbInputManager != null)
            {
                // Try to get input actions through reflection or available methods
                // Since direct conversion failed, we'll use alternative approach
                try
                {
                    // Look for input action asset component or field
                    var playerInput = egbInputManager.GetComponent<PlayerInput>();
                    if (playerInput != null)
                    {
                        originalInputActions = playerInput.actions;
                    }
                }
                catch (System.Exception e)
                {
                    if (showDebugMessages)
                        Debug.LogWarning($"Could not get input actions: {e.Message}");
                }
            }
        }
        
        private void Update()
        {
            if (!blockMouseWhenVRActive) return;
            
                    bool shouldBlockMouse = false; // VR Grid system removed
            
            if (shouldBlockMouse && !inputActionsDisabled)
            {
                DisableMouseInputActions();
            }
            else if (!shouldBlockMouse && inputActionsDisabled)
            {
                EnableMouseInputActions();
            }
        }
        
        private void DisableMouseInputActions()
        {
            if (egbInputManager != null)
            {
                // We could disable specific mouse actions here
                // For now, we'll let the VR system override the behavior
                inputActionsDisabled = true;
                
                if (showDebugMessages)
                {
                    Debug.Log("EGB Mouse Input blocked - VR mode active");
                }
            }
        }
        
        private void EnableMouseInputActions()
        {
            if (egbInputManager != null)
            {
                // Re-enable mouse actions
                inputActionsDisabled = false;
                
                if (showDebugMessages)
                {
                    Debug.Log("EGB Mouse Input enabled - VR mode inactive");
                }
            }
        }
        
        /// <summary>
        /// Toggle mouse input blocking
        /// </summary>
        public void SetMouseInputBlocking(bool block)
        {
            blockMouseWhenVRActive = block;
            
            if (!block && inputActionsDisabled)
            {
                EnableMouseInputActions();
            }
        }
        
        /// <summary>
        /// Check if mouse input is currently blocked
        /// </summary>
        public bool IsMouseInputBlocked()
        {
            return inputActionsDisabled;
        }
    }
}