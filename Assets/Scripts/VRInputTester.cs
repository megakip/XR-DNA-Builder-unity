using UnityEngine;
using SoulGames.VR;

namespace SoulGames.VR
{
    /// <summary>
    /// VR Input Tester - Helper script to test and debug VR input configuration
    /// Add this to any GameObject to test VR input from the Inspector
    /// 
    /// Usage:
    /// 1. Add this script to any GameObject in your scene
    /// 2. Assign the EGBProVRBridge reference
    /// 3. Use the buttons in the Inspector to test different input methods
    /// 4. Check the Console for debug output
    /// </summary>
    public class VRInputTester : MonoBehaviour
    {
        [Header("VR Bridge Reference")]
        [SerializeField] private EGBProVRBridge vrBridge;
        
        [Header("Test Controls")]
        [SerializeField] private bool autoFindVRBridge = true;
        
        private void Start()
        {
            if (autoFindVRBridge && vrBridge == null)
            {
                vrBridge = FindObjectOfType<EGBProVRBridge>();
                if (vrBridge == null)
                {
                    Debug.LogWarning("VRInputTester: No EGBProVRBridge found in scene!");
                }
                else
                {
                    Debug.Log($"VRInputTester: Found EGBProVRBridge on {vrBridge.gameObject.name}");
                }
            }
        }
        
        [Header("Testing Buttons")]
        [Space]
        [InspectorButton("TestInputDetection")]
        public bool testInput = false;
        
        [InspectorButton("ToggleToTrigger")]
        public bool setTrigger = false;
        
        [InspectorButton("ToggleToGrip")]
        public bool setGrip = false;
        
        [InspectorButton("GetDebugInfo")]
        public bool getDebug = false;
        
        [InspectorButton("EnableVRInput")]
        public bool enableVR = false;
        
        [InspectorButton("DisableVRInput")]
        public bool disableVR = false;
        
        /// <summary>
        /// Test current input detection
        /// </summary>
        public void TestInputDetection()
        {
            if (vrBridge != null)
            {
                vrBridge.TestInputDetection();
            }
            else
            {
                Debug.LogError("VRInputTester: VR Bridge reference not assigned!");
            }
        }
        
        /// <summary>
        /// Switch to using trigger for building
        /// </summary>
        public void ToggleToTrigger()
        {
            if (vrBridge != null)
            {
                vrBridge.SetUseTriggerForBuilding(true);
                Debug.Log("VRInputTester: Switched to Trigger for building");
            }
            else
            {
                Debug.LogError("VRInputTester: VR Bridge reference not assigned!");
            }
        }
        
        /// <summary>
        /// Switch to using grip for building
        /// </summary>
        public void ToggleToGrip()
        {
            if (vrBridge != null)
            {
                vrBridge.SetUseTriggerForBuilding(false);
                Debug.Log("VRInputTester: Switched to Grip for building");
            }
            else
            {
                Debug.LogError("VRInputTester: VR Bridge reference not assigned!");
            }
        }
        
        /// <summary>
        /// Get current debug information
        /// </summary>
        public void GetDebugInfo()
        {
            if (vrBridge != null)
            {
                Debug.Log($"VRInputTester Debug Info: {vrBridge.GetDebugInfo()}");
            }
            else
            {
                Debug.LogError("VRInputTester: VR Bridge reference not assigned!");
            }
        }
        
        /// <summary>
        /// Enable VR input
        /// </summary>
        public void EnableVRInput()
        {
            if (vrBridge != null)
            {
                vrBridge.SetVRInputEnabled(true);
                Debug.Log("VRInputTester: VR Input enabled");
            }
            else
            {
                Debug.LogError("VRInputTester: VR Bridge reference not assigned!");
            }
        }
        
        /// <summary>
        /// Disable VR input
        /// </summary>
        public void DisableVRInput()
        {
            if (vrBridge != null)
            {
                vrBridge.SetVRInputEnabled(false);
                Debug.Log("VRInputTester: VR Input disabled");
            }
            else
            {
                Debug.LogError("VRInputTester: VR Bridge reference not assigned!");
            }
        }
        
        private void Update()
        {
            // Show real-time input state in Inspector during play mode
            if (Application.isPlaying && vrBridge != null)
            {
                // This could be used for real-time display if needed
            }
        }
    }

    /// <summary>
    /// Custom attribute to create buttons in the Inspector
    /// </summary>
    public class InspectorButtonAttribute : PropertyAttribute
    {
        public string MethodName { get; private set; }
        
        public InspectorButtonAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
} 