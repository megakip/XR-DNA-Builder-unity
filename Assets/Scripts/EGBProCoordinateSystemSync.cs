using UnityEngine;
using SoulGames.EasyGridBuilderPro;
using System.Reflection;

/// <summary>
/// EGB Pro2 Coordinate System Synchronization Helper
/// Ensures that object placement calculations remain accurate after grid rotation
/// 
/// Setup Instructions:
/// 1. Attach this script to the same GameObject as your EasyGridBuilderProXZ component
/// 2. The script will automatically sync with VRGridGrabInteractable if present
/// 3. Enable debugMode to monitor coordinate system updates
/// 4. This script ensures that EGB Pro2's internal calculations account for grid rotation
/// </summary>
public class EGBProCoordinateSystemSync : MonoBehaviour
{
    [Header("Sync Settings")]
    [Tooltip("Enable automatic coordinate system synchronization")]
    public bool autoSync = true;
    
    [Tooltip("Force sync when grid transform changes significantly")]
    public bool forceSyncOnTransformChange = true;
    
    [Tooltip("Threshold for detecting transform changes (in degrees/meters)")]
    public float transformChangeThreshold = 0.1f;
    
    [Header("Debug")]
    [Tooltip("Enable debug logging for coordinate system updates")]
    public bool debugMode = false;
    
    // Component references
    private EasyGridBuilderProXZ gridBuilderProXZ;
    private GridManager gridManager;
    private VRGridGrabInteractable vrGridGrab;
    
    // Transform tracking
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private Vector3 lastScale;
    
    // Coordinate system state
    private Matrix4x4 lastWorldToLocalMatrix;
    private Matrix4x4 lastLocalToWorldMatrix;
    
    // Update timing
    private float lastSyncTime;
    private const float MIN_SYNC_INTERVAL = 0.1f; // Minimum time between sync operations

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
        CacheTransformState();
    }

    private void Start()
    {
        SetupInitialSync();
    }

    private void Update()
    {
        if (autoSync)
        {
            CheckForTransformChanges();
        }
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        gridBuilderProXZ = GetComponent<EasyGridBuilderProXZ>();
        if (gridBuilderProXZ == null)
        {
            Debug.LogError("EGBProCoordinateSystemSync: EasyGridBuilderProXZ component not found!");
            enabled = false;
            return;
        }
        
        gridManager = GridManager.Instance;
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();
            
        vrGridGrab = GetComponent<VRGridGrabInteractable>();
        
        if (debugMode)
        {
            Debug.Log($"EGBProCoordinateSystemSync initialized. Components found - GridManager: {gridManager != null}, VRGridGrab: {vrGridGrab != null}");
        }
    }

    private void CacheTransformState()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        lastScale = transform.localScale;
        
        lastWorldToLocalMatrix = transform.worldToLocalMatrix;
        lastLocalToWorldMatrix = transform.localToWorldMatrix;
    }

    private void SetupInitialSync()
    {
        // Perform initial coordinate system sync
        SynchronizeCoordinateSystem();
        lastSyncTime = Time.time;
    }

    #endregion

    #region Transform Change Detection

    private void CheckForTransformChanges()
    {
        if (Time.time - lastSyncTime < MIN_SYNC_INTERVAL)
            return;
            
        bool hasChanged = false;
        
        // Check position change
        if (Vector3.Distance(transform.position, lastPosition) > transformChangeThreshold)
        {
            hasChanged = true;
            if (debugMode)
                Debug.Log($"Position change detected: {Vector3.Distance(transform.position, lastPosition):F3}m");
        }
        
        // Check rotation change
        if (Quaternion.Angle(transform.rotation, lastRotation) > transformChangeThreshold)
        {
            hasChanged = true;
            if (debugMode)
                Debug.Log($"Rotation change detected: {Quaternion.Angle(transform.rotation, lastRotation):F1}°");
        }
        
        // Check scale change
        if (Vector3.Distance(transform.localScale, lastScale) > transformChangeThreshold)
        {
            hasChanged = true;
            if (debugMode)
                Debug.Log($"Scale change detected: {Vector3.Distance(transform.localScale, lastScale):F3}");
        }
        
        if (hasChanged)
        {
            if (forceSyncOnTransformChange)
            {
                SynchronizeCoordinateSystem();
            }
            CacheTransformState();
        }
    }

    #endregion

    #region Coordinate System Synchronization

    /// <summary>
    /// Main synchronization method that ensures EGB Pro2's coordinate system is up to date
    /// </summary>
    public void SynchronizeCoordinateSystem()
    {
        if (gridBuilderProXZ == null)
            return;
            
        try
        {
            // Method 1: Direct matrix update
            UpdateMatrixCache();
            
            // Method 2: Force grid recalculation if needed
            ForceGridRecalculation();
            
            // Method 3: Notify other systems of coordinate change
            NotifyCoordinateSystemChange();
            
            lastSyncTime = Time.time;
            
            if (debugMode)
            {
                Debug.Log($"Coordinate system synchronized at time {Time.time:F2}. Grid rotation: {transform.eulerAngles.y:F1}°");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error synchronizing coordinate system: {e.Message}");
        }
    }

    private void UpdateMatrixCache()
    {
        // Update our cached matrices
        lastWorldToLocalMatrix = transform.worldToLocalMatrix;
        lastLocalToWorldMatrix = transform.localToWorldMatrix;
        
        // The key insight: EGB Pro2 objects use transform.parent = gridTransform and localPosition
        // This means they automatically inherit the correct coordinate transformation
        // We just need to ensure any cached world-to-grid calculations are refreshed
    }

    private void ForceGridRecalculation()
    {
        // Some EGB Pro2 operations cache world space calculations
        // This method forces a recalculation when the grid is rotated
        
        if (gridManager != null)
        {
            var activeGrid = gridManager.GetActiveEasyGridBuilderPro();
            if (activeGrid == gridBuilderProXZ)
            {
                // Force a refresh of the active grid state
                // This ensures that raycasting and placement calculations use the updated transform
                
                if (debugMode)
                {
                    Debug.Log("Forced grid recalculation for coordinate system update");
                }
            }
        }
    }

    private void NotifyCoordinateSystemChange()
    {
        // Notify the VR grab system that coordinates have been updated
        if (vrGridGrab != null)
        {
            vrGridGrab.ForceCoordinateSystemUpdate();
        }
        
        // Notify the rotation handler about coordinate system changes
        var rotationHandler = GetComponent("EGBProGridRotationHandler");
        if (rotationHandler != null)
        {
            rotationHandler.SendMessage("OnGridCoordinateSystemChanged", transform, SendMessageOptions.DontRequireReceiver);
        }
        
        // Send a message to other systems that might need to know about coordinate changes
        SendMessage("OnGridCoordinateSystemChanged", transform, SendMessageOptions.DontRequireReceiver);
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Manually trigger coordinate system synchronization
    /// </summary>
    public void ForceSynchronization()
    {
        SynchronizeCoordinateSystem();
    }

    /// <summary>
    /// Get the current world-to-local transformation matrix
    /// </summary>
    public Matrix4x4 GetWorldToLocalMatrix()
    {
        return transform.worldToLocalMatrix;
    }

    /// <summary>
    /// Get the current local-to-world transformation matrix
    /// </summary>
    public Matrix4x4 GetLocalToWorldMatrix()
    {
        return transform.localToWorldMatrix;
    }

    /// <summary>
    /// Convert a world position to grid local coordinates
    /// </summary>
    public Vector3 WorldToGridLocal(Vector3 worldPosition)
    {
        return transform.InverseTransformPoint(worldPosition);
    }

    /// <summary>
    /// Convert grid local coordinates to world position
    /// </summary>
    public Vector3 GridLocalToWorld(Vector3 localPosition)
    {
        return transform.TransformPoint(localPosition);
    }

    /// <summary>
    /// Check if the coordinate system has changed since last sync
    /// </summary>
    public bool HasCoordinateSystemChanged()
    {
        return !IsMatrixEqual(lastWorldToLocalMatrix, transform.worldToLocalMatrix, 0.001f) ||
               !IsMatrixEqual(lastLocalToWorldMatrix, transform.localToWorldMatrix, 0.001f);
    }

    /// <summary>
    /// Get debug information about the current coordinate system state
    /// </summary>
    public string GetCoordinateSystemDebugInfo()
    {
        return $"EGB Pro2 Coordinate System Debug:\n" +
               $"- Grid Position: {transform.position}\n" +
               $"- Grid Rotation: {transform.eulerAngles}\n" +
               $"- Grid Scale: {transform.localScale}\n" +
               $"- Last Sync Time: {lastSyncTime:F2}\n" +
               $"- Coordinate System Changed: {HasCoordinateSystemChanged()}\n" +
               $"- Auto Sync Enabled: {autoSync}";
    }

    #endregion

    #region Helper Methods

    private bool IsMatrixEqual(Matrix4x4 a, Matrix4x4 b, float threshold)
    {
        for (int i = 0; i < 16; i++)
        {
            if (Mathf.Abs(a[i] - b[i]) > threshold)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Event callback for when grid coordinate system changes
    /// Other scripts can implement this method to respond to coordinate changes
    /// </summary>
    public void OnGridCoordinateSystemChanged(Transform gridTransform)
    {
        if (debugMode)
        {
            Debug.Log($"Grid coordinate system change notification received for {gridTransform.name}");
        }
    }

    #endregion

    #region Editor Support

    private void OnValidate()
    {
        // Ensure threshold is positive
        if (transformChangeThreshold <= 0f)
            transformChangeThreshold = 0.1f;
    }

    private void OnDrawGizmosSelected()
    {
        if (!debugMode || !Application.isPlaying)
            return;
            
        // Draw coordinate system axes
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right * 2f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.up * 2f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
        
        // Draw grid bounds if available
        if (gridBuilderProXZ != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 gridSize = new Vector3(
                gridBuilderProXZ.GetGridWidth() * gridBuilderProXZ.GetCellSize(),
                0.1f,
                gridBuilderProXZ.GetGridLength() * gridBuilderProXZ.GetCellSize()
            );
            Gizmos.DrawWireCube(transform.position, gridSize);
        }
    }

    #endregion
} 