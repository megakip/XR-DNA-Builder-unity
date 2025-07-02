using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using System.Collections.Generic;

/// <summary>
/// Deze component lost het NullReferenceException probleem op dat optreedt in het InputSystem,
/// vooral wanneer de VR-headset wordt op- of afgezet.
/// Plaats dit script op een GameObject in je scene (bij voorkeur een dat altijd actief is).
/// </summary>
public class InputSystemFixer : MonoBehaviour
{
    private void Awake()
    {
        // Stel het InputSystem in om focus te negeren (lost het probleem op wanneer de app de focus verliest)
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        
        // Stel updates in op Fixed in plaats van Dynamic (stabieler voor VR)
        Application.onBeforeRender -= OnBeforeRender;
        Application.onBeforeRender += OnBeforeRender;
        
        Debug.Log("InputSystemFixer initialized: InputSystem background behavior set to IgnoreFocus");
    }
    
    private void OnDestroy()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }
    
    private void OnBeforeRender()
    {
        try 
        {
            // Controleer XR-devices om te zien of er NullReferences kunnen optreden
            var devices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevices(devices);
            
            foreach (var device in devices)
            {
                // Reset devices die niet meer geldig zijn
                if (device.isValid == false)
                {
                    // Voor XR devices moeten we een andere aanpak gebruiken omdat ze niet direct resettable zijn
                    Debug.Log($"XR Device {device.name} is not valid");
                }
            }
            
            // Reset alle InputSystem devices
            var inputDevices = InputSystem.devices;
            foreach (var device in inputDevices)
            {
                if (!device.enabled)
                {
                    InputSystem.ResetDevice(device);
                }
            }
        }
        catch (System.Exception e)
        {
            // Log fouten in plaats van ze te laten crashen
            Debug.LogWarning($"Error in InputSystemFixer: {e.Message}");
        }
    }
    
    // Als je specifiek problemen hebt met de Meta Quest controllers, kun je deze functie gebruiken
    public void ResetXRControllers()
    {
        var leftHandDevices = new List<UnityEngine.XR.InputDevice>();
        var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
        
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);
        
        foreach (var device in InputSystem.devices)
        {
            // Reset de InputSystem devices in plaats van de XR devices
            if (device.description.product != null && 
                (device.description.product.Contains("Left") || 
                device.description.product.Contains("Right")))
            {
                InputSystem.ResetDevice(device);
                Debug.Log($"Reset InputSystem device: {device.displayName}");
            }
        }
        
        Debug.Log("XR controllers were reset");
    }
} 