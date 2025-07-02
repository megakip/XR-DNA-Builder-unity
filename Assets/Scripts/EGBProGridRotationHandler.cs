using UnityEngine;
using SoulGames.EasyGridBuilderPro;
using System.Collections.Generic;

/// <summary>
/// EGB Pro2 Grid Rotation Handler
/// Ensures that objects placed on the grid use the correct local rotation relative to the grid's orientation
/// This fixes the issue where objects appear tilted when placed on a rotated grid
/// 
/// Setup Instructions:
/// 1. Attach this script to the same GameObject as your EasyGridBuilderProXZ component
/// 2. The script will automatically intercept object placement and apply correct rotations
/// 3. Works together with VRGridHandleGrabber and EGBProCoordinateSystemSync
/// 4. Enable debugMode to monitor rotation corrections
/// </summary>
public class EGBProGridRotationHandler : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Automatically correct object rotations when placed on rotated grid")]
    public bool autoCorrectRotations = true;
    
    [Tooltip("Apply correction to newly placed objects")]
    public bool correctNewPlacements = true;
    
    [Tooltip("Update existing objects when grid rotates")]
    public bool updateExistingObjects = false;
    
    [Header("Object Types")]
    [Tooltip("Correct BuildableGridObject rotations")]
    public bool correctGridObjects = true;
    
    [Tooltip("Correct BuildableEdgeObject rotations")]
    public bool correctEdgeObjects = true;
    
    [Tooltip("Correct BuildableCornerObject rotations")]
    public bool correctCornerObjects = true;
    
    [Tooltip("Correct BuildableFreeObject rotations")]
    public bool correctFreeObjects = true;
    
    [Header("Debug")]
    [Tooltip("Enable debug logging for rotation corrections")]
    public bool debugMode = false;
    
    // Component references
    private EasyGridBuilderProXZ gridBuilderProXZ;
    private GridManager gridManager;
    
    // Grid state tracking
    private Quaternion lastGridRotation;
    private List<BuildableObject> trackedObjects = new List<BuildableObject>();
    
    // Rotation correction tracking
    private Dictionary<BuildableObject, Quaternion> originalRotations = new Dictionary<BuildableObject, Quaternion>();

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        CacheInitialGridRotation();
        SetupObjectTracking();
    }

    private void Update()
    {
        CheckForGridRotationChanges();
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        gridBuilderProXZ = GetComponent<EasyGridBuilderProXZ>();
        if (gridBuilderProXZ == null)
        {
            Debug.LogError("EGBProGridRotationHandler: EasyGridBuilderProXZ component not found!");
            enabled = false;
            return;
        }
        
        gridManager = GridManager.Instance;
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();
            
        if (debugMode)
        {
            Debug.Log($"EGBProGridRotationHandler initialized on {gameObject.name}");
        }
    }

    private void CacheInitialGridRotation()
    {
        lastGridRotation = transform.rotation;
    }

    private void SetupObjectTracking()
    {
        // Find all existing buildable objects that are children of this grid
        FindAndTrackExistingObjects();
        
        // Set up monitoring for new object placements
        if (correctNewPlacements)
        {
            InvokeRepeating(nameof(CheckForNewObjects), 1f, 0.5f);
        }
    }

    #endregion

    #region Grid Rotation Monitoring

    private void CheckForGridRotationChanges()
    {
        if (!autoCorrectRotations) return;
        
        // Check if grid rotation has changed significantly
        float rotationDifference = Quaternion.Angle(lastGridRotation, transform.rotation);
        
        if (rotationDifference > 0.1f) // Threshold for rotation change
        {
            if (debugMode)
            {
                Debug.Log($"Grid rotation changed by {rotationDifference:F1}° - updating object rotations");
            }
            
            if (updateExistingObjects)
            {
                UpdateAllObjectRotations();
            }
            
            lastGridRotation = transform.rotation;
        }
    }

    #endregion

    #region Object Tracking

    private void FindAndTrackExistingObjects()
    {
        trackedObjects.Clear();
        originalRotations.Clear();
        
        // Find all buildable objects that are children of this grid
        var allBuildableObjects = GetComponentsInChildren<BuildableObject>();
        
        foreach (var buildableObject in allBuildableObjects)
        {
            TrackObject(buildableObject);
        }
        
        if (debugMode)
        {
            Debug.Log($"Found and tracking {trackedObjects.Count} existing objects");
        }
    }

    private void CheckForNewObjects()
    {
        if (!correctNewPlacements) return;
        
        var allBuildableObjects = GetComponentsInChildren<BuildableObject>();
        
        foreach (var buildableObject in allBuildableObjects)
        {
            if (!trackedObjects.Contains(buildableObject))
            {
                // New object found - apply rotation correction
                TrackObject(buildableObject);
                CorrectObjectRotation(buildableObject);
                
                if (debugMode)
                {
                    Debug.Log($"New object detected and rotation corrected: {buildableObject.name}");
                }
            }
        }
    }

    private void TrackObject(BuildableObject buildableObject)
    {
        if (buildableObject != null)
        {
            trackedObjects.Add(buildableObject);
            originalRotations[buildableObject] = buildableObject.transform.localRotation;
        }
    }

    #endregion

    #region Rotation Correction

    private void UpdateAllObjectRotations()
    {
        foreach (var buildableObject in trackedObjects)
        {
            if (buildableObject != null)
            {
                CorrectObjectRotation(buildableObject);
            }
        }
        
        // Clean up null references
        trackedObjects.RemoveAll(obj => obj == null);
    }

    private void CorrectObjectRotation(BuildableObject buildableObject)
    {
        if (buildableObject == null || !autoCorrectRotations) return;
        
        // Check if we should correct this type of object
        if (!ShouldCorrectObjectType(buildableObject)) return;
        
        try
        {
            // Get the object's current world rotation
            Quaternion currentWorldRotation = buildableObject.transform.rotation;
            
            // Calculate what the local rotation should be relative to the grid
            Quaternion correctedLocalRotation = CalculateCorrectLocalRotation(buildableObject, currentWorldRotation);
            
            // Apply the corrected rotation
            buildableObject.transform.localRotation = correctedLocalRotation;
            
            if (debugMode)
            {
                Debug.Log($"Corrected rotation for {buildableObject.name}: Local rotation set to {correctedLocalRotation.eulerAngles}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error correcting rotation for {buildableObject.name}: {e.Message}");
        }
    }

    private Quaternion CalculateCorrectLocalRotation(BuildableObject buildableObject, Quaternion worldRotation)
    {
        // The key insight: we want the object to maintain its intended orientation 
        // relative to the grid, not relative to the world
        
        // Get the grid's current rotation
        Quaternion gridRotation = transform.rotation;
        
        // Calculate what the local rotation should be
        // This ensures the object appears correctly oriented relative to the grid
        Quaternion localRotation = Quaternion.Inverse(gridRotation) * worldRotation;
        
        // For EGB Pro2 XZ grids, we typically only want Y-axis rotation
        if (gridBuilderProXZ != null)
        {
            Vector3 localEuler = localRotation.eulerAngles;
            localRotation = Quaternion.Euler(0, localEuler.y, 0);
        }
        
        return localRotation;
    }

    private bool ShouldCorrectObjectType(BuildableObject buildableObject)
    {
        // Check object type and corresponding setting
        if (buildableObject is BuildableGridObject && correctGridObjects) return true;
        if (buildableObject is BuildableEdgeObject && correctEdgeObjects) return true;
        if (buildableObject is BuildableCornerObject && correctCornerObjects) return true;
        if (buildableObject is BuildableFreeObject && correctFreeObjects) return true;
        
        return false;
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Manually correct all object rotations
    /// </summary>
    public void ForceCorrectAllRotations()
    {
        FindAndTrackExistingObjects();
        UpdateAllObjectRotations();
        
        if (debugMode)
        {
            Debug.Log("Forced rotation correction for all objects");
        }
    }

    /// <summary>
    /// Correct rotation for a specific object
    /// </summary>
    public void CorrectSpecificObject(BuildableObject buildableObject)
    {
        if (!trackedObjects.Contains(buildableObject))
        {
            TrackObject(buildableObject);
        }
        
        CorrectObjectRotation(buildableObject);
    }

    /// <summary>
    /// Enable/disable automatic rotation correction
    /// </summary>
    public void SetAutoCorrectRotations(bool enabled)
    {
        autoCorrectRotations = enabled;
        
        if (debugMode)
        {
            Debug.Log($"Auto rotation correction {(enabled ? "enabled" : "disabled")}");
        }
    }

    /// <summary>
    /// Get the number of tracked objects
    /// </summary>
    public int GetTrackedObjectCount()
    {
        trackedObjects.RemoveAll(obj => obj == null);
        return trackedObjects.Count;
    }

    /// <summary>
    /// Get debug information about rotation handling
    /// </summary>
    public string GetRotationHandlerDebugInfo()
    {
        return $"EGB Pro2 Grid Rotation Handler Debug:\n" +
               $"- Auto Correct Rotations: {autoCorrectRotations}\n" +
               $"- Correct New Placements: {correctNewPlacements}\n" +
               $"- Update Existing Objects: {updateExistingObjects}\n" +
               $"- Tracked Objects: {GetTrackedObjectCount()}\n" +
               $"- Grid Rotation: {transform.eulerAngles}\n" +
               $"- Last Grid Rotation: {lastGridRotation.eulerAngles}";
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Call this when the grid coordinate system changes
    /// This should be called by the coordinate sync system
    /// </summary>
    public void OnGridCoordinateSystemChanged(Transform gridTransform)
    {
        if (gridTransform == transform)
        {
            if (debugMode)
            {
                Debug.Log("Grid coordinate system changed - updating object rotations");
            }
            
            // Force update all object rotations
            ForceCorrectAllRotations();
        }
    }

    #endregion

    #region Editor Support

    private void OnValidate()
    {
        // Ensure at least one object type is enabled for correction
        if (!correctGridObjects && !correctEdgeObjects && !correctCornerObjects && !correctFreeObjects)
        {
            correctGridObjects = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!debugMode || !Application.isPlaying) return;
        
        // Draw grid orientation
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right * 2f);
        
        // Draw tracked objects
        Gizmos.color = Color.green;
        foreach (var obj in trackedObjects)
        {
            if (obj != null)
            {
                Gizmos.DrawWireSphere(obj.transform.position, 0.1f);
            }
        }
    }

    #endregion
} 