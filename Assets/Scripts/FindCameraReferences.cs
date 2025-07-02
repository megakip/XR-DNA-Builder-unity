using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FindCameraReferences : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== CAMERA REFERENCES SCAN ===");
        
        // Find all cameras in scene
        Camera[] allCameras = FindObjectsOfType<Camera>();
        Debug.Log($"Total cameras found: {allCameras.Length}");
        
        foreach (Camera cam in allCameras)
        {
            Debug.Log($"Camera: '{cam.name}' (InstanceID: {cam.GetInstanceID()}) - GameObject: '{cam.gameObject.name}' (InstanceID: {cam.gameObject.GetInstanceID()})");
        }
        
        Debug.Log("\n=== CANVAS CAMERA REFERENCES ===");
        
        // Find all Canvas components
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.worldCamera != null)
            {
                Debug.Log($"Canvas '{canvas.name}' worldCamera: '{canvas.worldCamera.name}' (InstanceID: {canvas.worldCamera.GetInstanceID()})");
            }
            else
            {
                Debug.Log($"Canvas '{canvas.name}' has NO worldCamera assigned");
            }
        }
        
        Debug.Log("\n=== GRAPHIC RAYCASTER CAMERA REFERENCES ===");
        
        // Find all GraphicRaycaster components
        GraphicRaycaster[] allRaycasters = FindObjectsOfType<GraphicRaycaster>();
        foreach (GraphicRaycaster raycaster in allRaycasters)
        {
            if (raycaster.eventCamera != null)
            {
                Debug.Log($"GraphicRaycaster on '{raycaster.name}' eventCamera: '{raycaster.eventCamera.name}' (InstanceID: {raycaster.eventCamera.GetInstanceID()})");
            }
            else
            {
                Debug.Log($"GraphicRaycaster on '{raycaster.name}' has NO eventCamera assigned");
            }
        }
        
        Debug.Log("\n=== OBJECTS REFERENCING MAIN CAMERA ===");
        
        // Look for "Main Camera" specifically
        GameObject mainCameraGO = GameObject.FindWithTag("MainCamera");
        if (mainCameraGO != null)
        {
            Camera mainCamera = mainCameraGO.GetComponent<Camera>();
            if (mainCamera != null)
            {
                Debug.Log($"Found Main Camera: '{mainCameraGO.name}' (InstanceID: {mainCamera.GetInstanceID()})");
                
                // Check if any Canvas references this Main Camera
                foreach (Canvas canvas in allCanvases)
                {
                    if (canvas.worldCamera == mainCamera)
                    {
                        Debug.Log($">>> Canvas '{canvas.name}' is referencing Main Camera!");
                    }
                }
                
                // Check if any GraphicRaycaster references this Main Camera
                foreach (GraphicRaycaster raycaster in allRaycasters)
                {
                    if (raycaster.eventCamera == mainCamera)
                    {
                        Debug.Log($">>> GraphicRaycaster on '{raycaster.name}' is referencing Main Camera!");
                    }
                }
            }
        }
        else
        {
            Debug.Log("No GameObject with 'MainCamera' tag found!");
        }
        
        Debug.Log("\n=== RECOMMENDATION ===");
        
        // Find CenterEyeAnchor
        GameObject centerEyeAnchor = GameObject.Find("CenterEyeAnchor");
        if (centerEyeAnchor != null)
        {
            Camera centerEyeCamera = centerEyeAnchor.GetComponent<Camera>();
            if (centerEyeCamera != null)
            {
                Debug.Log($"CenterEyeAnchor camera found: '{centerEyeAnchor.name}' (InstanceID: {centerEyeCamera.GetInstanceID()})");
                Debug.Log(">>> SOLUTION: Replace Main Camera references with CenterEyeAnchor camera!");
            }
        }
        
        // Auto-destroy this script after running
        Destroy(this);
    }
}