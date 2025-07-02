using UnityEngine;
using Unity.XR.CoreUtils;

/// <summary>
/// Helper script dat automatisch de event camera instelt voor een Canvas in World Space modus.
/// Plaats dit script op een GameObject met een Canvas component in World Space modus.
/// </summary>
public class XRCanvasEventCamera : MonoBehaviour
{
    [Tooltip("XR Origin in de scene (wordt automatisch gevonden als niet ingesteld)")]
    public XROrigin xrOrigin;
    
    [Tooltip("Referentie naar de camera die gebruikt moet worden als event camera (wordt automatisch gevonden als niet ingesteld)")]
    public Camera eventCamera;
    
    [Tooltip("Automatisch de canvas updaten als de camera verandert")]
    public bool autoUpdate = true;
    
    [Tooltip("Debug informatie weergeven")]
    public bool showDebugInfo = true;
    
    private Canvas canvas;
    
    private void Awake()
    {
        // Zoek de Canvas component
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Geen Canvas component gevonden op dit GameObject!");
            enabled = false;
            return;
        }
        
        // Controleer of de Canvas in World Space modus staat
        if (canvas.renderMode != RenderMode.WorldSpace)
        {
            Debug.LogWarning("Canvas is niet in World Space modus! Event camera is alleen nodig voor World Space canvassen.");
        }
        
        // Zoek de XR Origin als die niet is ingesteld
        if (xrOrigin == null)
        {
            xrOrigin = FindObjectOfType<XROrigin>();
            if (xrOrigin != null && showDebugInfo)
            {
                Debug.Log($"XR Origin automatisch gevonden: {xrOrigin.name}");
            }
        }
        
        // Zoek de main camera als er geen event camera is ingesteld
        if (eventCamera == null)
        {
            if (xrOrigin != null && xrOrigin.Camera != null)
            {
                eventCamera = xrOrigin.Camera;
                if (showDebugInfo)
                    Debug.Log($"Event camera automatisch ingesteld op XR Camera: {eventCamera.name}");
            }
            else
            {
                eventCamera = Camera.main;
                if (showDebugInfo)
                    Debug.Log($"Event camera automatisch ingesteld op Main Camera: {(eventCamera != null ? eventCamera.name : "null")}");
            }
        }
        
        // Stel de event camera in op de Canvas
        SetEventCamera();
    }
    
    private void Update()
    {
        if (autoUpdate && canvas != null)
        {
            // Controleer of de camera nog steeds bestaat
            if (eventCamera == null)
            {
                if (xrOrigin != null && xrOrigin.Camera != null)
                {
                    eventCamera = xrOrigin.Camera;
                    if (showDebugInfo)
                        Debug.Log("Event camera opnieuw ingesteld op XR Camera");
                }
                else
                {
                    eventCamera = Camera.main;
                    if (showDebugInfo)
                        Debug.Log("Event camera opnieuw ingesteld op Main Camera");
                }
                
                SetEventCamera();
            }
            
            // Controleer of de event camera is ingesteld op de Canvas
            if (canvas.worldCamera != eventCamera)
            {
                SetEventCamera();
            }
        }
    }
    
    /// <summary>
    /// Stelt de event camera in op de Canvas
    /// </summary>
    private void SetEventCamera()
    {
        if (canvas != null && eventCamera != null)
        {
            canvas.worldCamera = eventCamera;
            
            if (showDebugInfo)
                Debug.Log($"Event camera ingesteld op Canvas: {eventCamera.name}");
        }
        else if (canvas != null && eventCamera == null)
        {
            Debug.LogError("Geen camera gevonden om als event camera in te stellen!");
        }
    }
    
    /// <summary>
    /// Stelt de event camera handmatig in
    /// </summary>
    public void SetEventCamera(Camera camera)
    {
        eventCamera = camera;
        SetEventCamera();
    }
} 