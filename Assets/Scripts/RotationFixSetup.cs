using UnityEngine;
using SoulGames.EasyGridBuilderPro;

namespace FixedRotationSystem
{
    /// <summary>
    /// Automatic setup script for the rotation fix
    /// Add this to any GameObject and run SetupRotationFix() from the context menu
    /// </summary>
    public class RotationFixSetup : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField] private bool setupOnStart = true;
        [SerializeField] private bool debugMode = false;

        private void Start()
        {
            if (setupOnStart)
            {
                SetupRotationFix();
            }
        }

        [ContextMenu("Setup Rotation Fix")]
        public void SetupRotationFix()
        {
            Debug.Log("[RotationFix] Starting automatic setup...");

            // 1. Find the BuildableObjectSelector
            BuildableObjectSelector originalSelector = FindObjectOfType<BuildableObjectSelector>();
            if (originalSelector == null)
            {
                Debug.LogError("[RotationFix] BuildableObjectSelector not found! Make sure EGP Pro 2 is properly set up.");
                return;
            }

            // 2. Add FixedBuildableObjectSelector if not present
            FixedBuildableObjectSelector fixedSelector = originalSelector.GetComponent<FixedBuildableObjectSelector>();
            if (fixedSelector == null)
            {
                fixedSelector = originalSelector.gameObject.AddComponent<FixedBuildableObjectSelector>();
                Debug.Log("[RotationFix] Added FixedBuildableObjectSelector component");
            }

            // 3. Find GridUIManager
            GridUIManager uiManager = FindObjectOfType<GridUIManager>();
            if (uiManager == null)
            {
                Debug.LogError("[RotationFix] GridUIManager not found!");
                return;
            }

            // 4. Add FixedGridUIRotationHandler if not present
            FixedGridUIRotationHandler fixedHandler = uiManager.GetComponent<FixedGridUIRotationHandler>();
            if (fixedHandler == null)
            {
                fixedHandler = uiManager.gameObject.AddComponent<FixedGridUIRotationHandler>();
                Debug.Log("[RotationFix] Added FixedGridUIRotationHandler component");
            }

            // 5. Set debug mode
            fixedHandler.SetDebugMode(debugMode);

            Debug.Log("[RotationFix] Setup completed successfully!");
            Debug.Log("[RotationFix] The rotation buttons will now use the fixed rotation logic.");
            Debug.Log("[RotationFix] Objects should now rotate in place without changing vertical position.");
        }

        [ContextMenu("Test Rotation Fix")]
        public void TestRotationFix()
        {
            Debug.Log("[RotationFix] Testing rotation fix...");

            // Check if components are properly set up
            BuildableObjectSelector originalSelector = FindObjectOfType<BuildableObjectSelector>();
            FixedBuildableObjectSelector fixedSelector = FindObjectOfType<FixedBuildableObjectSelector>();
            GridUIManager uiManager = FindObjectOfType<GridUIManager>();
            FixedGridUIRotationHandler fixedHandler = FindObjectOfType<FixedGridUIRotationHandler>();

            bool setupCorrect = true;

            if (originalSelector == null)
            {
                Debug.LogError("[RotationFix] BuildableObjectSelector missing!");
                setupCorrect = false;
            }

            if (fixedSelector == null)
            {
                Debug.LogError("[RotationFix] FixedBuildableObjectSelector missing!");
                setupCorrect = false;
            }

            if (uiManager == null)
            {
                Debug.LogError("[RotationFix] GridUIManager missing!");
                setupCorrect = false;
            }

            if (fixedHandler == null)
            {
                Debug.LogError("[RotationFix] FixedGridUIRotationHandler missing!");
                setupCorrect = false;
            }

            if (setupCorrect)
            {
                Debug.Log("[RotationFix] ✅ All components are properly set up!");
                Debug.Log("[RotationFix] Instructions:");
                Debug.Log("[RotationFix] 1. Select an object in the scene");
                Debug.Log("[RotationFix] 2. Click the rotate buttons in the selector UI panel");
                Debug.Log("[RotationFix] 3. The object should rotate in place without moving up");
            }
            else
            {
                Debug.Log("[RotationFix] ❌ Setup incomplete. Run 'Setup Rotation Fix' first.");
            }
        }

        [ContextMenu("Remove Rotation Fix")]
        public void RemoveRotationFix()
        {
            // Remove the added components
            FixedBuildableObjectSelector fixedSelector = FindObjectOfType<FixedBuildableObjectSelector>();
            if (fixedSelector != null)
            {
                DestroyImmediate(fixedSelector);
                Debug.Log("[RotationFix] Removed FixedBuildableObjectSelector");
            }

            FixedGridUIRotationHandler fixedHandler = FindObjectOfType<FixedGridUIRotationHandler>();
            if (fixedHandler != null)
            {
                DestroyImmediate(fixedHandler);
                Debug.Log("[RotationFix] Removed FixedGridUIRotationHandler");
            }

            Debug.Log("[RotationFix] Rotation fix components removed. Original behavior restored.");
        }

        /// <summary>
        /// Enable or disable debug logging
        /// </summary>
        public void SetDebugMode(bool enabled)
        {
            debugMode = enabled;
            
            FixedGridUIRotationHandler handler = FindObjectOfType<FixedGridUIRotationHandler>();
            if (handler != null)
            {
                handler.SetDebugMode(enabled);
            }
        }
    }
}