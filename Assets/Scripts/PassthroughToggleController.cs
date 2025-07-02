using UnityEngine;
using UnityEngine.UI;

namespace VRGrid.UI
{
    /// <summary>
    /// Controller script voor het aan- en uitzetten van passthrough in VR met Meta SDK
    /// 
    /// Gebruik:
    /// 1. Voeg dit script toe aan een GameObject
    /// 2. Wijs de Button component toe in de Inspector
    /// 3. Koppel de Toggle() methode aan de button's OnClick event
    /// 
    /// Vereisten:
    /// - Meta SDK Camera Rig (OVR Camera Rig) in de scene
    /// - Passthrough Building Block geïnstalleerd
    /// - OVR Manager component in de scene
    /// </summary>
    public class PassthroughToggleController : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Button toggleButton;
        [SerializeField] private Text buttonText; // Optioneel: voor het tonen van de huidige status
        
        [Header("Passthrough Settings")]
        [SerializeField] private bool passthroughEnabled = false;
        
        private OVRManager ovrManager;
        private OVRPassthroughLayer passthroughLayer;
        
        // Event voor communicatie met andere scripts (zoals VRSkyboxManager)
        public static System.Action<bool> OnPassthroughStateChanged;
        
        // Public property om passthrough status op te halen
        public bool IsPassthroughEnabled => passthroughEnabled;
        
        private void Start()
        {
            // Zoek OVR Manager in de scene
            ovrManager = FindObjectOfType<OVRManager>();
            
            // Zoek OVR Passthrough Layer in de scene
            passthroughLayer = FindObjectOfType<OVRPassthroughLayer>();
            
            if (ovrManager == null)
            {
                Debug.LogError("PassthroughToggleController: Geen OVR Manager gevonden in de scene! " +
                             "Zorg ervoor dat je Meta SDK Camera Rig correct is ingesteld.");
                return;
            }
            
            if (passthroughLayer == null)
            {
                Debug.LogWarning("PassthroughToggleController: Geen OVR Passthrough Layer gevonden. " +
                               "Probeer een te maken of gebruik OVR Manager instellingen.");
            }
            
            // Setup button als deze niet is toegewezen
            if (toggleButton == null)
            {
                toggleButton = GetComponent<Button>();
            }
            
            // Voeg click listener toe
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(Toggle);
            }
            else
            {
                Debug.LogError("PassthroughToggleController: Geen Button component gevonden! " +
                             "Voeg dit script toe aan een GameObject met een Button component, " +
                             "of wijs een Button toe in de Inspector.");
            }
            
            // Set initial state
            UpdatePassthrough();
            UpdateUI();
            
            // Notify other scripts about initial state
            OnPassthroughStateChanged?.Invoke(passthroughEnabled);
        }
        
        /// <summary>
        /// Toggle de passthrough aan/uit
        /// Deze methode kan worden aangeroepen vanuit de Unity Editor via button events
        /// </summary>
        public void Toggle()
        {
            passthroughEnabled = !passthroughEnabled;
            UpdatePassthrough();
            UpdateUI();
            
            // Notify other scripts about state change
            OnPassthroughStateChanged?.Invoke(passthroughEnabled);
            
            Debug.Log($"Passthrough {(passthroughEnabled ? "ingeschakeld" : "uitgeschakeld")}");
        }
        
        /// <summary>
        /// Zet passthrough expliciet aan
        /// </summary>
        public void EnablePassthrough()
        {
            if (!passthroughEnabled)
            {
                passthroughEnabled = true;
                UpdatePassthrough();
                UpdateUI();
                
                // Notify other scripts about state change
                OnPassthroughStateChanged?.Invoke(passthroughEnabled);
                
                Debug.Log("Passthrough ingeschakeld");
            }
        }
        
        /// <summary>
        /// Zet passthrough expliciet uit
        /// </summary>
        public void DisablePassthrough()
        {
            if (passthroughEnabled)
            {
                passthroughEnabled = false;
                UpdatePassthrough();
                UpdateUI();
                
                // Notify other scripts about state change
                OnPassthroughStateChanged?.Invoke(passthroughEnabled);
                
                Debug.Log("Passthrough uitgeschakeld");
            }
        }
        
        /// <summary>
        /// Update de passthrough instelling via Meta SDK
        /// </summary>
        private void UpdatePassthrough()
        {
            try 
            {
                if (ovrManager == null)
                {
                    Debug.LogError("OVR Manager niet gevonden!");
                    return;
                }
                
                // Methode 1: Via OVR Passthrough Layer
                if (passthroughLayer != null)
                {
                    passthroughLayer.enabled = passthroughEnabled;
                    
                    // Voor Meta SDK: zet ook de passthrough layer order en opacity
                    if (passthroughEnabled)
                    {
                        // Zorg dat passthrough vooraan wordt gerenderd
                        passthroughLayer.overlayType = OVROverlay.OverlayType.Underlay;
                    }
                    
                    Debug.Log($"OVR Passthrough Layer {(passthroughEnabled ? "ingeschakeld" : "uitgeschakeld")}");
                }
                
                // Methode 2: Via OVR Manager 
                if (passthroughEnabled)
                {
                    // Schakel passthrough in via OVR Manager
                    Debug.Log("Passthrough wordt ingeschakeld via OVR Manager");
                    
                    // Zet de tracking origin mode goed voor passthrough
                    ovrManager.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
                    
                    // Schakel Mixed Reality compositing in
                    ovrManager.isInsightPassthroughEnabled = true;
                }
                else
                {
                    Debug.Log("Passthrough wordt uitgeschakeld");
                    
                    // Herstel normale VR modus
                    ovrManager.trackingOriginType = OVRManager.TrackingOrigin.EyeLevel;
                    
                    // Schakel Mixed Reality compositing uit
                    ovrManager.isInsightPassthroughEnabled = false;
                }
                
                // Ook OVR camera's configureren
                ConfigureOVRCameras();
                
                Debug.Log($"Meta SDK Passthrough {(passthroughEnabled ? "ingeschakeld" : "uitgeschakeld")}");
                
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Fout bij het updaten van Meta SDK passthrough: {e.Message}");
            }
        }
        
        /// <summary>
        /// Configureer OVR Camera's specifiek voor passthrough
        /// </summary>
        private void ConfigureOVRCameras()
        {
            // Zoek alle OVR Camera Rig cameras
            OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
            if (cameraRig != null)
            {
                // Probeer de verschillende OVR camera properties
                Camera leftEyeCamera = null;
                Camera rightEyeCamera = null;
                Camera centerEyeCamera = null;
                
                // Gebruik reflection om de juiste properties te vinden
                try
                {
                    var leftEyeProp = cameraRig.GetType().GetProperty("leftEyeCamera");
                    var rightEyeProp = cameraRig.GetType().GetProperty("rightEyeCamera");
                    var centerEyeProp = cameraRig.GetType().GetProperty("centerEyeCamera");
                    
                    if (leftEyeProp != null) leftEyeCamera = leftEyeProp.GetValue(cameraRig) as Camera;
                    if (rightEyeProp != null) rightEyeCamera = rightEyeProp.GetValue(cameraRig) as Camera;
                    if (centerEyeProp != null) centerEyeCamera = centerEyeProp.GetValue(cameraRig) as Camera;
                }
                catch
                {
                    // Fallback: zoek cameras als child objects
                    Transform leftEye = cameraRig.transform.Find("LeftEyeAnchor") ?? cameraRig.transform.Find("TrackingSpace/LeftEyeAnchor");
                    Transform rightEye = cameraRig.transform.Find("RightEyeAnchor") ?? cameraRig.transform.Find("TrackingSpace/RightEyeAnchor");
                    Transform centerEye = cameraRig.transform.Find("CenterEyeAnchor") ?? cameraRig.transform.Find("TrackingSpace/CenterEyeAnchor");
                    
                    if (leftEye != null) leftEyeCamera = leftEye.GetComponent<Camera>();
                    if (rightEye != null) rightEyeCamera = rightEye.GetComponent<Camera>();
                    if (centerEye != null) centerEyeCamera = centerEye.GetComponent<Camera>();
                }
                
                ConfigureOVRCamera(leftEyeCamera, "Left Eye");
                ConfigureOVRCamera(rightEyeCamera, "Right Eye");
                ConfigureOVRCamera(centerEyeCamera, "Center Eye");
            }
            else
            {
                Debug.LogWarning("Geen OVR Camera Rig gevonden voor camera configuratie");
            }
        }
        
        /// <summary>
        /// Configureer een specifieke OVR camera voor passthrough
        /// </summary>
        private void ConfigureOVRCamera(Camera cam, string cameraName)
        {
            if (cam == null) return;
            
            if (passthroughEnabled)
            {
                // Voor passthrough: Don't Clear om passthrough door te laten
                cam.clearFlags = CameraClearFlags.Nothing;
                cam.backgroundColor = new Color(0, 0, 0, 0);
                Debug.Log($"OVR {cameraName} Camera geconfigureerd voor passthrough");
            }
            else
            {
                // Voor normale VR: gebruik skybox
                cam.clearFlags = CameraClearFlags.Skybox;
                Debug.Log($"OVR {cameraName} Camera geconfigureerd voor normale VR");
            }
        }
        
        /// <summary>
        /// Update de UI elementen
        /// </summary>
        private void UpdateUI()
        {
            if (buttonText != null)
            {
                buttonText.text = passthroughEnabled ? "Passthrough UIT" : "Passthrough AAN";
            }
        }
        
        private void OnDestroy()
        {
            // Cleanup button listener
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveListener(Toggle);
            }
        }
    }
} 