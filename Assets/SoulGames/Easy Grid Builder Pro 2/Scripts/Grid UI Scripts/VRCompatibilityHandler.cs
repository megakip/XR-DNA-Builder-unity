using UnityEngine;
using UnityEngine.UI;
using SoulGames.VR;

namespace SoulGames.EasyGridBuilderPro
{
    /// <summary>
    /// VR Compatibility Handler for Custom Category Buttons
    /// Ensures that UI interactions work properly in VR mode
    /// </summary>
    [AddComponentMenu("Easy Grid Builder Pro/Grid UI/VR Compatibility Handler", 3)]
    public class VRCompatibilityHandler : MonoBehaviour
    {
        [Header("VR Integration")]
        [Tooltip("Temporarily disable VR input during UI interactions")]
        public bool temporarilyDisableVRInput = true;
        
        [Tooltip("Duration to disable VR input after UI interaction")]
        public float vrInputDisableDuration = 0.5f;
        
        [Header("Debug")]
        public bool debugMode = true;
        
        private EGBProVRBridge vrBridge;
        private float vrInputDisableEndTime;
        private bool originalVRInputState;
        
        private void Start()
        {
            // Find VR Bridge
            vrBridge = FindObjectOfType<EGBProVRBridge>();
            
            if (vrBridge && debugMode)
            {
                Debug.Log($"VRCompatibilityHandler: Found VR Bridge on {gameObject.name}");
            }
            
            // Listen to button clicks on this object and children
            SetupButtonListeners();
        }
        
        private void SetupButtonListeners()
        {
            // Get all buttons on this object and children
            Button[] buttons = GetComponentsInChildren<Button>(true);
            
            foreach (Button button in buttons)
            {
                // Add our pre-click handler
                button.onClick.AddListener(() => OnUIButtonClicked(button));
            }
            
            if (debugMode)
            {
                Debug.Log($"VRCompatibilityHandler: Setup listeners for {buttons.Length} buttons on {gameObject.name}");
            }
        }
        
        private void OnUIButtonClicked(Button clickedButton)
        {
            if (debugMode)
            {
                Debug.Log($"VRCompatibilityHandler: UI Button clicked: {clickedButton.name}");
            }
            
            if (temporarilyDisableVRInput && vrBridge)
            {
                // Temporarily disable VR input to prevent conflicts
                originalVRInputState = vrBridge.enableVRInput;
                vrBridge.SetVRInputEnabled(false);
                vrInputDisableEndTime = Time.time + vrInputDisableDuration;
                
                if (debugMode)
                {
                    Debug.Log($"VRCompatibilityHandler: VR input disabled temporarily for {vrInputDisableDuration}s");
                }
            }
        }
        
        private void Update()
        {
            // Re-enable VR input after timeout
            if (vrInputDisableEndTime > 0 && Time.time >= vrInputDisableEndTime)
            {
                if (vrBridge && temporarilyDisableVRInput)
                {
                    vrBridge.SetVRInputEnabled(originalVRInputState);
                    vrInputDisableEndTime = 0;
                    
                    if (debugMode)
                    {
                        Debug.Log("VRCompatibilityHandler: VR input re-enabled");
                    }
                }
            }
        }
        
        /// <summary>
        /// Manually trigger VR input disable
        /// </summary>
        public void DisableVRInputTemporarily(float duration = -1)
        {
            if (duration < 0) duration = vrInputDisableDuration;
            
            if (vrBridge)
            {
                originalVRInputState = vrBridge.enableVRInput;
                vrBridge.SetVRInputEnabled(false);
                vrInputDisableEndTime = Time.time + duration;
                
                if (debugMode)
                {
                    Debug.Log($"VRCompatibilityHandler: VR input manually disabled for {duration}s");
                }
            }
        }
        
        /// <summary>
        /// Force re-enable VR input
        /// </summary>
        public void ReEnableVRInput()
        {
            if (vrBridge)
            {
                vrBridge.SetVRInputEnabled(originalVRInputState);
                vrInputDisableEndTime = 0;
                
                if (debugMode)
                {
                    Debug.Log("VRCompatibilityHandler: VR input manually re-enabled");
                }
            }
        }
        
        /// <summary>
        /// Check if VR is currently affecting input
        /// </summary>
        public bool IsVRActive()
        {
            return vrBridge != null && vrBridge.enableVRInput;
        }
    }
} 