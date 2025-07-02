using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

/// <summary>
/// VR Skybox Manager voor Meta SDK met Mixed Reality ondersteuning
/// Dit script zorgt ervoor dat je skybox zichtbaar is in VR wanneer passthrough niet actief is.
/// Bij Mixed Reality/Passthrough wordt de skybox automatisch uitgeschakeld.
/// 
/// Het probleem: OVR cameras gebruiken vaak Clear Flags = "Solid Color" met zwarte achtergrond
/// De oplossing: Zet Clear Flags naar "Skybox" voor VR en naar "SolidColor" voor Mixed Reality
/// 
/// Instructies:
/// 1. Voeg dit script toe aan een GameObject in je scene (bijvoorbeeld "VR Skybox Manager")
/// 2. Het script detecteert automatisch alle VR cameras en Mixed Reality status
/// 3. Het stelt de skybox in voor VR modus en schakelt uit voor Mixed Reality
/// 4. Optioneel: stel je gewenste skybox materiaal in via de Inspector
/// </summary>
public class VRSkyboxManager : MonoBehaviour
{
    [Header("Skybox Settings")]
    [SerializeField]
    [Tooltip("Het skybox materiaal dat gebruikt moet worden. Laat leeg om de Render Settings skybox te gebruiken")]
    private Material customSkyboxMaterial;
    
    [SerializeField]
    [Tooltip("Automatisch alle VR cameras configureren bij start")]
    private bool autoConfigureOnStart = true;
    
    [SerializeField]
    [Tooltip("Blijf cameras monitoren en aanpassen tijdens runtime")]
    private bool continuousMonitoring = true;
    
    [SerializeField]
    [Tooltip("Update frequentie voor monitoring (in seconden)")]
    private float monitoringInterval = 0.5f;
    
    [Header("Mixed Reality Settings")]
    [SerializeField]
    [Tooltip("Automatisch skybox uitschakelen tijdens Mixed Reality/Passthrough")]
    private bool autoDisableForMixedReality = true;
    
    [SerializeField]
    [Tooltip("Kleur voor achtergrond tijdens Mixed Reality (transparant voor passthrough)")]
    private Color mixedRealityBackgroundColor = new Color(0, 0, 0, 0);
    
    [SerializeField]
    [Tooltip("Camera Clear Flags methode voor Mixed Reality")]
    private CameraClearFlags mixedRealityClearFlags = CameraClearFlags.Nothing;
    
    [SerializeField]
    [Tooltip("Handmatige override: forceer Mixed Reality modus")]
    private bool forceMixedRealityMode = false;
    
    [Header("Detection Settings")]
    [SerializeField]
    [Tooltip("Zoek naar PassthroughToggleController component voor MR status")]
    private bool usePassthroughController = true;
    
    [SerializeField]
    [Tooltip("Handmatige referentie naar PassthroughToggleController (optioneel)")]
    private MonoBehaviour passthroughController;
    
    [Header("Debug")]
    [SerializeField]
    [Tooltip("Toon debug informatie in console")]
    private bool enableDebugLogs = true;
    
    [SerializeField]
    [Tooltip("Toon runtime debug GUI")]
    private bool showDebugGUI = true;
    
    // Private variabelen
    private Camera[] vrCameras;
    private float lastMonitoringTime;
    private Material originalSkyboxMaterial;
    private UnityEngine.Rendering.AmbientMode originalAmbientMode;
    private bool currentMixedRealityState = false;
    private bool lastMixedRealityState = false;
    
    // Events voor andere scripts
    public System.Action<bool> OnMixedRealityStateChanged;
    public System.Action<bool> OnSkyboxStateChanged;
    
    void Start()
    {
        // Bewaar de originele skybox uit de Render Settings
        originalSkyboxMaterial = RenderSettings.skybox;
        originalAmbientMode = RenderSettings.ambientMode;
        
        // Zoek PassthroughToggleController als gewenst
        if (usePassthroughController && passthroughController == null)
        {
            // Zoek naar PassthroughToggleController via reflection (veilig)
            MonoBehaviour[] allComponents = FindObjectsOfType<MonoBehaviour>();
            foreach (MonoBehaviour comp in allComponents)
            {
                if (comp.GetType().Name == "PassthroughToggleController")
                {
                    passthroughController = comp;
                    break;
                }
            }
            
            if (passthroughController == null && enableDebugLogs)
            {
                Debug.Log("[VR Skybox] Geen PassthroughToggleController gevonden - gebruik alternatieve detectie");
            }
        }
        
        // Subscribe to passthrough state changes
        VRGrid.UI.PassthroughToggleController.OnPassthroughStateChanged += OnPassthroughStateChanged;
        
        if (autoConfigureOnStart)
        {
            UpdateSkyboxForCurrentMode();
        }
        
        if (enableDebugLogs)
        {
            Debug.Log("[VR Skybox] Manager gestart. Mixed Reality ondersteuning: " + autoDisableForMixedReality);
        }
    }
    
    void Update()
    {
        if (continuousMonitoring && Time.time - lastMonitoringTime >= monitoringInterval)
        {
            MonitorAndUpdateCameras();
            lastMonitoringTime = Time.time;
        }
    }
    
    /// <summary>
    /// Callback voor wanneer passthrough status verandert
    /// </summary>
    private void OnPassthroughStateChanged(bool isPassthroughEnabled)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[VR Skybox] Passthrough status veranderd: {(isPassthroughEnabled ? "AAN" : "UIT")}");
        }
        
        // Trigger immediate update when passthrough state changes
        UpdateSkyboxForCurrentMode();
    }
    
    /// <summary>
    /// Update skybox gebaseerd op huidige Mixed Reality status
    /// </summary>
    public void UpdateSkyboxForCurrentMode()
    {
        bool isMixedReality = IsMixedRealityActive();
        
        // Check voor state change
        if (isMixedReality != lastMixedRealityState)
        {
            OnMixedRealityStateChanged?.Invoke(isMixedReality);
            lastMixedRealityState = isMixedReality;
            
            if (enableDebugLogs)
            {
                Debug.Log($"[VR Skybox] Mixed Reality status veranderd naar: {(isMixedReality ? "ACTIEF" : "INACTIEF")}");
            }
        }
        
        currentMixedRealityState = isMixedReality;
        
        if (isMixedReality && autoDisableForMixedReality)
        {
            ConfigureCamerasForMixedReality();
        }
        else
        {
            ConfigureVRSkybox();
        }
    }
    
    /// <summary>
    /// Configureer cameras voor Mixed Reality (geen skybox)
    /// </summary>
    private void ConfigureCamerasForMixedReality()
    {
        // Probeer eerst specifieke OVR camera configuratie
        int ovrCameras = ConfigureOVRCamerasForMixedReality();
        
        // Fallback: configureer alle VR cameras
        vrCameras = FindObjectsOfType<Camera>();
        int configuredCameras = 0;
        
        foreach (Camera cam in vrCameras)
        {
            if (IsVRCamera(cam))
            {
                ConfigureCameraForMixedReality(cam);
                configuredCameras++;
            }
        }
        
        // Schakel globale skybox volledig uit voor Mixed Reality
        RenderSettings.skybox = null;
        
        // Ook de ambient lighting aanpassen om skybox invloed te verminderen
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        
        OnSkyboxStateChanged?.Invoke(false);
        
        if (enableDebugLogs)
        {
            Debug.Log($"[VR Skybox] {configuredCameras + ovrCameras} camera(s) geconfigureerd voor Mixed Reality modus - Skybox volledig uitgeschakeld");
        }
    }
    
    /// <summary>
    /// Configureer specifiek OVR cameras voor Mixed Reality
    /// </summary>
    private int ConfigureOVRCamerasForMixedReality()
    {
        int configuredCount = 0;
        
        try
        {
            // Zoek OVR Camera Rig
            MonoBehaviour[] allComponents = FindObjectsOfType<MonoBehaviour>();
            foreach (MonoBehaviour comp in allComponents)
            {
                if (comp.GetType().Name == "OVRCameraRig")
                {
                    // Gebruik reflection om OVR cameras te vinden
                    Camera[] ovrCameras = GetOVRCameras(comp);
                    foreach (Camera cam in ovrCameras)
                    {
                        if (cam != null)
                        {
                            ConfigureCameraForMixedReality(cam);
                            configuredCount++;
                            
                            if (enableDebugLogs)
                            {
                                Debug.Log($"[VR Skybox] OVR Camera '{cam.name}' geconfigureerd voor Mixed Reality");
                            }
                        }
                    }
                    break;
                }
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[VR Skybox] Kon OVR cameras niet configureren: {e.Message}");
            }
        }
        
        return configuredCount;
    }
    
    /// <summary>
    /// Haal OVR cameras op via reflection
    /// </summary>
    private Camera[] GetOVRCameras(MonoBehaviour ovrCameraRig)
    {
        List<Camera> cameras = new List<Camera>();
        
        try
        {
            // Probeer verschillende property namen
            string[] cameraProperties = { "leftEyeCamera", "rightEyeCamera", "centerEyeCamera" };
            
            foreach (string propName in cameraProperties)
            {
                var property = ovrCameraRig.GetType().GetProperty(propName);
                if (property != null)
                {
                    Camera cam = property.GetValue(ovrCameraRig) as Camera;
                    if (cam != null)
                    {
                        cameras.Add(cam);
                    }
                }
            }
            
            // Fallback: zoek camera child objects
            if (cameras.Count == 0)
            {
                string[] anchorNames = { "LeftEyeAnchor", "RightEyeAnchor", "CenterEyeAnchor", 
                                       "TrackingSpace/LeftEyeAnchor", "TrackingSpace/RightEyeAnchor", "TrackingSpace/CenterEyeAnchor" };
                
                foreach (string anchorName in anchorNames)
                {
                    Transform anchor = ovrCameraRig.transform.Find(anchorName);
                    if (anchor != null)
                    {
                        Camera cam = anchor.GetComponent<Camera>();
                        if (cam != null)
                        {
                            cameras.Add(cam);
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[VR Skybox] Reflection fout bij OVR camera ophalen: {e.Message}");
            }
        }
        
        return cameras.ToArray();
    }
    
    /// <summary>
    /// Configureer een camera voor Mixed Reality
    /// </summary>
    private void ConfigureCameraForMixedReality(Camera cam)
    {
        CameraClearFlags originalFlags = cam.clearFlags;
        
        // Gebruik de configureerbare clear flags methode voor Mixed Reality
        cam.clearFlags = mixedRealityClearFlags;
        
        // Voor passthrough moet achtergrond volledig transparant zijn
        cam.backgroundColor = mixedRealityBackgroundColor;
        
        if (enableDebugLogs)
        {
            Debug.Log($"[VR Skybox] Camera '{cam.name}' ingesteld voor Mixed Reality: " +
                     $"Clear Flags: {originalFlags} → {cam.clearFlags}, " +
                     $"Background: {cam.backgroundColor}");
        }
    }
    
    /// <summary>
    /// Configureer alle VR cameras om skybox weer te geven
    /// </summary>
    [ContextMenu("Configure VR Skybox")]
    public void ConfigureVRSkybox()
    {
        // Skip als Mixed Reality actief is en auto-disable aan staat
        if (currentMixedRealityState && autoDisableForMixedReality)
        {
            if (enableDebugLogs)
            {
                Debug.Log("[VR Skybox] Skybox configuratie overgeslagen - Mixed Reality is actief");
            }
            return;
        }
        
        // Vind alle cameras in de scene
        vrCameras = FindObjectsOfType<Camera>();
        
        int configuredCameras = 0;
        
        foreach (Camera cam in vrCameras)
        {
            if (IsVRCamera(cam))
            {
                ConfigureCameraForSkybox(cam);
                configuredCameras++;
            }
        }
        
        // Herstel skybox materiaal
        if (customSkyboxMaterial != null)
        {
            RenderSettings.skybox = customSkyboxMaterial;
            
            if (enableDebugLogs)
            {
                Debug.Log($"[VR Skybox] Globale skybox ingesteld op: {customSkyboxMaterial.name}");
            }
        }
        else if (originalSkyboxMaterial != null)
        {
            // Herstel originele skybox als geen custom materiaal is ingesteld
            RenderSettings.skybox = originalSkyboxMaterial;
            
            if (enableDebugLogs)
            {
                Debug.Log($"[VR Skybox] Originele skybox hersteld: {originalSkyboxMaterial.name}");
            }
        }
        
        // Herstel originele ambient lighting
        RenderSettings.ambientMode = originalAmbientMode;
        
        OnSkyboxStateChanged?.Invoke(true);
        
        if (enableDebugLogs)
        {
            Debug.Log($"[VR Skybox] {configuredCameras} VR camera(s) geconfigureerd voor skybox weergave - Skybox volledig ingeschakeld");
        }
    }
    
    /// <summary>
    /// Controleer of Mixed Reality/Passthrough actief is
    /// </summary>
    private bool IsMixedRealityActive()
    {
        // Manual override
        if (forceMixedRealityMode)
            return true;
        
        // Check via PassthroughToggleController met direct access
        if (passthroughController != null)
        {
            // Probeer eerst de nieuwe IsPassthroughEnabled property
            try
            {
                var property = passthroughController.GetType().GetProperty("IsPassthroughEnabled");
                if (property != null)
                {
                    bool isEnabled = (bool)property.GetValue(passthroughController);
                    if (enableDebugLogs && currentMixedRealityState != isEnabled)
                    {
                        Debug.Log($"[VR Skybox] PassthroughController status (direct): {isEnabled}");
                    }
                    return isEnabled;
                }
            }
            catch (System.Exception e)
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning($"[VR Skybox] Kon PassthroughController status niet lezen via property: {e.Message}");
                }
            }
            
            // Fallback: oude reflection methode
            try
            {
                var field = passthroughController.GetType().GetField("passthroughEnabled", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    bool isEnabled = (bool)field.GetValue(passthroughController);
                    if (enableDebugLogs && currentMixedRealityState != isEnabled)
                    {
                        Debug.Log($"[VR Skybox] PassthroughController status (reflection): {isEnabled}");
                    }
                    return isEnabled;
                }
            }
            catch (System.Exception e)
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning($"[VR Skybox] Kon PassthroughController status niet lezen via reflection: {e.Message}");
                }
            }
        }
        
        // Fallback: check cameras voor passthrough indicatoren
        if (vrCameras != null)
        {
            foreach (Camera cam in vrCameras)
            {
                if (cam != null && IsVRCamera(cam))
                {
                    if (IsPassthroughActive(cam))
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Controleer of een camera een VR camera is
    /// </summary>
    private bool IsVRCamera(Camera cam)
    {
        // Controleer op OVR components via reflection (veilig)
        if (HasOVRComponent(cam))
            return true;
            
        // Controleer op XR camera tags/names (inclusief OVR specifieke namen)
        if (cam.name.ToLower().Contains("eye") || 
            cam.name.ToLower().Contains("vr") || 
            cam.name.ToLower().Contains("xr") ||
            cam.name.ToLower().Contains("main camera") ||
            cam.name.ToLower().Contains("center") ||
            cam.name.ToLower().Contains("left") ||
            cam.name.ToLower().Contains("right") ||
            cam.name.ToLower().Contains("anchor"))
        {
            return true;
        }
        
        // Controleer op stereo target eye
        if (cam.stereoTargetEye != StereoTargetEyeMask.None)
            return true;
            
        // Controleer of de camera een child is van een VR rig
        Transform parent = cam.transform.parent;
        while (parent != null)
        {
            if (parent.name.ToLower().Contains("vr") || 
                parent.name.ToLower().Contains("xr") || 
                parent.name.ToLower().Contains("ovr") ||
                parent.name.ToLower().Contains("rig") ||
                parent.name.ToLower().Contains("anchor") ||
                parent.name.ToLower().Contains("tracking"))
            {
                return true;
            }
            parent = parent.parent;
        }
            
        return false;
    }
    
    /// <summary>
    /// Veilig controleren op OVR components zonder compile errors
    /// </summary>
    private bool HasOVRComponent(Camera cam)
    {
        try
        {
            // Zoek naar bekende OVR component types via hun namen
            Component[] components = cam.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp != null)
                {
                    string typeName = comp.GetType().Name;
                    if (typeName.Contains("OVR") || typeName.Contains("Oculus"))
                    {
                        return true;
                    }
                }
            }
        }
        catch (System.Exception)
        {
            // Negeer fouten als OVR SDK niet beschikbaar is
        }
        
        return false;
    }
    
    /// <summary>
    /// Configureer een specifieke camera voor skybox weergave
    /// </summary>
    private void ConfigureCameraForSkybox(Camera cam)
    {
        // Bewaar originele instellingen
        CameraClearFlags originalFlags = cam.clearFlags;
        Color originalBackgroundColor = cam.backgroundColor;
        
        // Stel Clear Flags in op Skybox
        cam.clearFlags = CameraClearFlags.Skybox;
        
        // Optioneel: pas achtergrondkleur aan voor fallback
        cam.backgroundColor = new Color(0.2f, 0.2f, 0.3f, 1.0f); // Donkerblauwe fallback
        
        if (enableDebugLogs)
        {
            Debug.Log($"[VR Skybox] Camera '{cam.name}' geconfigureerd voor skybox: " +
                     $"Clear Flags: {originalFlags} → {cam.clearFlags}");
        }
    }
    
    /// <summary>
    /// Monitor cameras en update indien nodig
    /// </summary>
    private void MonitorAndUpdateCameras()
    {
        UpdateSkyboxForCurrentMode();
    }
    
    /// <summary>
    /// Controleer of passthrough actief is voor een camera
    /// </summary>
    private bool IsPassthroughActive(Camera cam)
    {
        try
        {
            // Zoek naar OVR Passthrough Layer component via reflection (veilig)
            Component[] components = cam.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp != null)
                {
                    string typeName = comp.GetType().Name;
                    if (typeName.Contains("Passthrough") && comp.GetType().GetProperty("enabled") != null)
                    {
                        bool enabled = (bool)comp.GetType().GetProperty("enabled").GetValue(comp);
                        if (enabled)
                        {
                            return true;
                        }
                    }
                }
            }
            
            // Controleer ook op passthrough in parent objects
            Transform parent = cam.transform.parent;
            while (parent != null)
            {
                Component[] parentComponents = parent.GetComponents<Component>();
                foreach (Component comp in parentComponents)
                {
                    if (comp != null && comp.GetType().Name.Contains("Passthrough"))
                    {
                        try
                        {
                            bool enabled = (bool)comp.GetType().GetProperty("enabled").GetValue(comp);
                            if (enabled) return true;
                        }
                        catch
                        {
                            // Negeer als property niet bestaat
                        }
                    }
                }
                parent = parent.parent;
            }
        }
        catch (System.Exception)
        {
            // Negeer fouten als OVR SDK componenten niet beschikbaar zijn
        }
        
        // Controleer op andere passthrough indicatoren via camera clear flags
        // Als een VR camera clear flags heeft ingesteld op "Don't Clear" kan dit passthrough zijn
        if (cam.clearFlags == CameraClearFlags.Nothing)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Handmatige controle: schakel naar Mixed Reality modus
    /// </summary>
    [ContextMenu("Force Mixed Reality Mode")]
    public void ForceMixedRealityMode()
    {
        forceMixedRealityMode = true;
        UpdateSkyboxForCurrentMode();
        
        if (enableDebugLogs)
        {
            Debug.Log("[VR Skybox] Handmatig geschakeld naar Mixed Reality modus");
        }
    }
    
    /// <summary>
    /// Handmatige controle: schakel naar VR modus (met skybox)
    /// </summary>
    [ContextMenu("Force VR Mode")]
    public void ForceVRMode()
    {
        forceMixedRealityMode = false;
        UpdateSkyboxForCurrentMode();
        
        if (enableDebugLogs)
        {
            Debug.Log("[VR Skybox] Handmatig geschakeld naar VR modus");
        }
    }
    
    /// <summary>
    /// Herstel originele camera instellingen
    /// </summary>
    [ContextMenu("Restore Original Settings")]
    public void RestoreOriginalSettings()
    {
        if (vrCameras == null)
            return;
            
        foreach (Camera cam in vrCameras)
        {
            if (cam != null && IsVRCamera(cam))
            {
                // Herstel naar solid color (typisch voor VR zonder skybox)
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
            }
        }
        
        // Herstel originele skybox
        if (originalSkyboxMaterial != null)
        {
            RenderSettings.skybox = originalSkyboxMaterial;
        }
        
        if (enableDebugLogs)
        {
            Debug.Log("[VR Skybox] Originele camera instellingen hersteld");
        }
    }
    
    /// <summary>
    /// Stel een nieuwe skybox in en update alle cameras
    /// </summary>
    public void SetSkyboxMaterial(Material newSkyboxMaterial)
    {
        customSkyboxMaterial = newSkyboxMaterial;
        RenderSettings.skybox = newSkyboxMaterial;
        
        // Update alleen als niet in Mixed Reality modus
        if (!currentMixedRealityState)
        {
            ConfigureVRSkybox();
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"[VR Skybox] Nieuwe skybox ingesteld: {newSkyboxMaterial?.name ?? "null"}");
        }
    }
    
    /// <summary>
    /// Schakel skybox weergave aan/uit
    /// </summary>
    public void ToggleSkybox(bool enabled)
    {
        if (enabled)
        {
            forceMixedRealityMode = false;
            ConfigureVRSkybox();
        }
        else
        {
            forceMixedRealityMode = true;
            ConfigureCamerasForMixedReality();
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"[VR Skybox] Skybox weergave {(enabled ? "ingeschakeld" : "uitgeschakeld")}");
        }
    }
    
    /// <summary>
    /// Handmatige refresh van alle camera instellingen
    /// </summary>
    [ContextMenu("Refresh All Cameras")]
    public void RefreshAllCameras()
    {
        vrCameras = null; // Force re-detection
        UpdateSkyboxForCurrentMode();
    }
    
    /// <summary>
    /// Get huidige Mixed Reality status
    /// </summary>
    public bool GetMixedRealityStatus()
    {
        return currentMixedRealityState;
    }
    
    /// <summary>
    /// Handmatige passthrough controller assignment
    /// </summary>
    public void SetPassthroughController(MonoBehaviour controller)
    {
        passthroughController = controller;
        if (enableDebugLogs)
        {
            Debug.Log($"[VR Skybox] PassthroughController ingesteld: {controller?.name ?? "null"}");
        }
    }
    
    // GUI voor runtime controle
    void OnGUI()
    {
        if (!showDebugGUI) return;
        
        GUILayout.BeginArea(new Rect(10, 200, 350, 250));
        GUILayout.Label("VR Skybox Manager Debug:");
        
        // Status informatie
        GUILayout.Label($"Mixed Reality Actief: {(currentMixedRealityState ? "JA" : "NEE")}");
        GUILayout.Label($"Force MR Mode: {forceMixedRealityMode}");
        GUILayout.Label($"Auto Disable for MR: {autoDisableForMixedReality}");
        
        if (vrCameras != null)
        {
            GUILayout.Label($"VR Cameras gevonden: {vrCameras.Length}");
            
            int skyboxCameras = 0;
            int solidColorCameras = 0;
            foreach (Camera cam in vrCameras)
            {
                if (cam != null && IsVRCamera(cam))
                {
                    if (cam.clearFlags == CameraClearFlags.Skybox)
                        skyboxCameras++;
                    else if (cam.clearFlags == CameraClearFlags.SolidColor)
                        solidColorCameras++;
                }
            }
            GUILayout.Label($"Cameras met Skybox: {skyboxCameras}");
            GUILayout.Label($"Cameras met SolidColor: {solidColorCameras}");
        }
        
        GUILayout.Label($"Custom Skybox: {customSkyboxMaterial?.name ?? "Geen"}");
        GUILayout.Label($"Passthrough Controller: {(passthroughController != null ? "Gevonden" : "Niet gevonden")}");
        
        // Control buttons
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("VR Mode"))
        {
            ForceVRMode();
        }
        if (GUILayout.Button("MR Mode"))
        {
            ForceMixedRealityMode();
        }
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Refresh Cameras"))
        {
            RefreshAllCameras();
        }
        
        GUILayout.EndArea();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from passthrough state changes
        VRGrid.UI.PassthroughToggleController.OnPassthroughStateChanged -= OnPassthroughStateChanged;
    }
} 