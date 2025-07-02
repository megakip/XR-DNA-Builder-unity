using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Positions a canvas in front of the XR Origin's main camera.
/// </summary>
/// <remarks>
/// Attach this script to a GameObject with a Canvas component.
/// The script will position the canvas in front of the main camera that is a child of the XR Origin.
/// </remarks>
public class CanvasPositioner : MonoBehaviour
{
    [Tooltip("Reference to the XR Origin GameObject")]
    public Transform xrOrigin;

    [Tooltip("Distance from the camera to position the canvas")]
    public float distanceFromCamera = 1.0f;

    [Tooltip("Vertical offset from camera height (positive values move up)")]
    public float verticalOffset = 0.0f;

    [Tooltip("Whether to update position every frame")]
    public bool continuousUpdate = true;

    private Transform cameraTransform;
    private Canvas canvas;

    private void Start()
    {
        // Get the Canvas component
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("CanvasPositioner requires a Canvas component on the same GameObject.");
            enabled = false;
            return;
        }

        // Find the main camera in the XR Origin
        if (xrOrigin != null)
        {
            // Try to find the main camera as a child of XR Origin
            Camera mainCamera = xrOrigin.GetComponentInChildren<Camera>();
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
                PositionCanvas();
            }
            else
            {
                Debug.LogError("Could not find a Camera component in the children of XR Origin.");
                enabled = false;
            }
        }
        else
        {
            Debug.LogError("XR Origin reference is not set. Please assign it in the Inspector.");
            enabled = false;
        }
    }

    private void Update()
    {
        if (continuousUpdate && cameraTransform != null)
        {
            PositionCanvas();
        }
    }

    /// <summary>
    /// Positions the canvas in front of the camera at the specified distance.
    /// </summary>
    private void PositionCanvas()
    {
        if (cameraTransform == null) return;

        // Position the canvas in front of the camera
        Vector3 position = cameraTransform.position + cameraTransform.forward * distanceFromCamera;
        
        // Apply vertical offset
        position.y += verticalOffset;
        
        // Update the canvas position
        transform.position = position;
        
        // Make the canvas face the camera
        transform.rotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
    }

    /// <summary>
    /// Manually update the canvas position.
    /// </summary>
    public void UpdateCanvasPosition()
    {
        PositionCanvas();
    }
}
