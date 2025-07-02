using UnityEngine;
using UnityEngine.UI;
using SoulGames.EasyGridBuilderPro;

namespace FixedRotationSystem
{
    /// <summary>
    /// Fixed UI Manager that uses the corrected rotation logic
    /// Add this to your GridUIManager GameObject alongside the original GridUIManager
    /// </summary>
    public class FixedGridUIRotationHandler : MonoBehaviour
    {
        [Header("UI Button References")]
        [SerializeField] private Button rotateLeftButton;
        [SerializeField] private Button rotateRightButton;
        
        [Header("Fixed Rotation Component")]
        [SerializeField] private FixedBuildableObjectSelector fixedSelector;
        
        [Header("Settings")]
        [SerializeField] private bool useFixedRotation = true;
        [SerializeField] private bool debugMode = false;

        private GridUIManager originalUIManager;
        private BuildableObjectSelector originalSelector;

        private void Start()
        {
            // Find components
            originalUIManager = GetComponent<GridUIManager>();
            originalSelector = FindObjectOfType<BuildableObjectSelector>();
            
            if (!fixedSelector)
            {
                fixedSelector = FindObjectOfType<FixedBuildableObjectSelector>();
                
                // If not found, add it to the selector GameObject
                if (!fixedSelector && originalSelector)
                {
                    fixedSelector = originalSelector.gameObject.AddComponent<FixedBuildableObjectSelector>();
                }
            }

            // Setup button listeners if not assigned
            if (!rotateLeftButton || !rotateRightButton)
            {
                FindRotateButtons();
            }

            // Override button listeners
            if (useFixedRotation)
            {
                SetupFixedRotationButtons();
            }
        }

        private void FindRotateButtons()
        {
            // Find rotate buttons in the selector UI panel
            GameObject selectorPanel = GameObject.Find("Selector UI Panel");
            if (selectorPanel)
            {
                Transform buttonsList = selectorPanel.transform.Find("Panel BG/Buttons List");
                if (buttonsList)
                {
                    // Find rotate buttons
                    for (int i = 0; i < buttonsList.childCount; i++)
                    {
                        Transform child = buttonsList.GetChild(i);
                        string childName = child.name.ToLower();
                        
                        if (childName.Contains("rotate") && childName.Contains("left"))
                        {
                            Button button = child.GetComponentInChildren<Button>();
                            if (button) rotateLeftButton = button;
                        }
                        else if (childName.Contains("rotate") && childName.Contains("right"))
                        {
                            Button button = child.GetComponentInChildren<Button>();
                            if (button) rotateRightButton = button;
                        }
                    }
                }
            }

            if (debugMode)
            {
                Debug.Log($"[FixedRotation] Found buttons - Left: {rotateLeftButton != null}, Right: {rotateRightButton != null}");
            }
        }

        private void SetupFixedRotationButtons()
        {
            if (rotateLeftButton)
            {
                // Remove all existing listeners
                rotateLeftButton.onClick.RemoveAllListeners();
                
                // Add fixed rotation listener
                rotateLeftButton.onClick.AddListener(() => OnFixedRotateLeftClicked());
                
                if (debugMode)
                {
                    Debug.Log("[FixedRotation] Setup fixed rotate left button");
                }
            }

            if (rotateRightButton)
            {
                // Remove all existing listeners
                rotateRightButton.onClick.RemoveAllListeners();
                
                // Add fixed rotation listener
                rotateRightButton.onClick.AddListener(() => OnFixedRotateRightClicked());
                
                if (debugMode)
                {
                    Debug.Log("[FixedRotation] Setup fixed rotate right button");
                }
            }
        }

        private void OnFixedRotateLeftClicked()
        {
            if (debugMode)
            {
                Debug.Log("[FixedRotation] Rotate Left button clicked (Fixed)");
            }

            if (fixedSelector)
            {
                fixedSelector.TriggerFixedRotation(false); // Counter-clockwise
            }
            else
            {
                Debug.LogWarning("[FixedRotation] FixedBuildableObjectSelector not found!");
            }
        }

        private void OnFixedRotateRightClicked()
        {
            if (debugMode)
            {
                Debug.Log("[FixedRotation] Rotate Right button clicked (Fixed)");
            }

            if (fixedSelector)
            {
                fixedSelector.TriggerFixedRotation(true); // Clockwise
            }
            else
            {
                Debug.LogWarning("[FixedRotation] FixedBuildableObjectSelector not found!");
            }
        }

        /// <summary>
        /// Toggle between fixed and original rotation
        /// </summary>
        public void ToggleFixedRotation()
        {
            useFixedRotation = !useFixedRotation;
            
            if (useFixedRotation)
            {
                SetupFixedRotationButtons();
                Debug.Log("[FixedRotation] Switched to FIXED rotation");
            }
            else
            {
                // Restore original rotation (you'd need to implement this)
                Debug.Log("[FixedRotation] Switched to ORIGINAL rotation");
            }
        }

        /// <summary>
        /// Enable debug mode
        /// </summary>
        public void SetDebugMode(bool enabled)
        {
            debugMode = enabled;
            if (fixedSelector)
            {
                // You could add a public method to set debug mode in FixedBuildableObjectSelector
            }
        }

        /// <summary>
        /// Manual button assignment (call from inspector)
        /// </summary>
        public void AssignRotateButtons(Button leftButton, Button rightButton)
        {
            rotateLeftButton = leftButton;
            rotateRightButton = rightButton;
            
            if (useFixedRotation)
            {
                SetupFixedRotationButtons();
            }
        }

        private void OnValidate()
        {
            // Auto-setup when values change in inspector
            if (Application.isPlaying && useFixedRotation)
            {
                SetupFixedRotationButtons();
            }
        }
    }
}