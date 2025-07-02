using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SoulGames.VR
{
    /// <summary>
    /// UI Controller for VR input settings
    /// Provides toggle buttons for VR mode and mouse input disable
    /// </summary>
    public class VRInputToggleUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Toggle vrModeToggle;
        [SerializeField] private Toggle mouseDisableToggle;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button testBuildModeButton;
        [SerializeField] private Button testDestroyModeButton;
        [SerializeField] private Button testSelectModeButton;
        
        [Header("Settings")]
        [SerializeField] private bool updateStatusText = true;
        [SerializeField] private float statusUpdateInterval = 1f;
        
        private EGBProVRBridge vrBridge;
        private float lastStatusUpdate;
        
        private void Start()
        {
            // Find VR Bridge
            vrBridge = FindObjectOfType<EGBProVRBridge>();
            
            // Setup UI callbacks
            if (vrModeToggle != null)
            {
                vrModeToggle.onValueChanged.AddListener(OnVRModeToggled);
                vrModeToggle.isOn = vrBridge?.enableVRInput ?? false;
            }
            
            if (mouseDisableToggle != null)
            {
                mouseDisableToggle.onValueChanged.AddListener(OnMouseDisableToggled);
                mouseDisableToggle.isOn = vrBridge?.disableMouseInput ?? false;
            }
            
            // Setup test buttons
            if (testBuildModeButton != null)
                testBuildModeButton.onClick.AddListener(() => SetEGBMode(SoulGames.EasyGridBuilderPro.GridMode.BuildMode));
                
            if (testDestroyModeButton != null)
                testDestroyModeButton.onClick.AddListener(() => SetEGBMode(SoulGames.EasyGridBuilderPro.GridMode.DestroyMode));
                
            if (testSelectModeButton != null)
                testSelectModeButton.onClick.AddListener(() => SetEGBMode(SoulGames.EasyGridBuilderPro.GridMode.SelectMode));
        }
        
        private void Update()
        {
            if (updateStatusText && statusText != null && Time.time - lastStatusUpdate > statusUpdateInterval)
            {
                UpdateStatusText();
                lastStatusUpdate = Time.time;
            }
        }
        
        private void OnVRModeToggled(bool enabled)
        {
            if (vrBridge != null)
            {
                vrBridge.SetVRInputEnabled(enabled);
                Debug.Log($"VR Mode {(enabled ? "enabled" : "disabled")} via UI");
            }
        }
        
        private void OnMouseDisableToggled(bool disabled)
        {
            if (vrBridge != null)
            {
                vrBridge.SetMouseInputDisabled(disabled);
                Debug.Log($"Mouse Input {(disabled ? "disabled" : "enabled")} via UI");
            }
        }
        
        private void SetEGBMode(SoulGames.EasyGridBuilderPro.GridMode mode)
        {
            var gridManager = SoulGames.EasyGridBuilderPro.GridManager.Instance;
            if (gridManager != null)
            {
                var egbPro = gridManager.GetActiveEasyGridBuilderPro();
                if (egbPro != null)
                {
                    egbPro.SetActiveGridMode(mode);
                    Debug.Log($"EGB Mode set to: {mode}");
                }
            }
        }
        
        private void UpdateStatusText()
        {
            if (vrBridge == null) return;
            
            string status = vrBridge.GetDebugInfo();
            statusText.text = $"VR Status: {status}";
        }
        
        private void OnDestroy()
        {
            // Clean up listeners
            if (vrModeToggle != null)
                vrModeToggle.onValueChanged.RemoveListener(OnVRModeToggled);
                
            if (mouseDisableToggle != null)
                mouseDisableToggle.onValueChanged.RemoveListener(OnMouseDisableToggled);
        }
    }
}