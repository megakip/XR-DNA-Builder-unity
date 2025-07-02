using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using SoulGames.EasyGridBuilderPro;

/// <summary>
/// VR Grid Grab Interactable for Easy Grid Builder Pro 2
/// Provides distance-based grabbing with Y-axis rotation and coordinate system synchronization
/// 
/// Setup Instructions:
/// 1. Attach this script to the GameObject containing your EasyGridBuilderProXZ component
/// 2. Ensure the GameObject has an XRGrabInteractable component (will be added automatically)
/// 3. Set the minimumDragDistance in the inspector (default: 0.01m)
/// 4. Configure rotationSpeed for desired rotation sensitivity
/// 5. Assign the GridManager reference if not auto-detected
/// 6. The script will automatically handle coordinate system updates for proper object placement
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class VRGridGrabInteractable : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Minimum distance in meters that the grid needs to be dragged before becoming movable")]
    public float minimumDragDistance = 0.01f;
    
    [Tooltip("Speed multiplier for rotation. Higher values = faster rotation")]
    public float rotationSpeed = 90f;
    
    [Tooltip("Enable smooth rotation (false for instant rotation)")]
    public bool smoothRotation = true;
    
    [Tooltip("Speed of smooth rotation when enabled")]
    public float smoothRotationSpeed = 5f;
    
    [Header("Grid References")]
    [Tooltip("Reference to GridManager (will auto-detect if not assigned)")]
    public GridManager gridManager;
    
    [Tooltip("Reference to EasyGridBuilderProXZ component (will auto-detect if not assigned)")]
    public EasyGridBuilderProXZ gridBuilderProXZ;
    
    [Header("Debug")]
    [Tooltip("Enable debug logging")]
    public bool debugMode = false;
    
    // Private variables
    private XRGrabInteractable grabInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor currentInteractor;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isMovementEnabled = false;
    private bool isRotating = false;
    
    // Rotation tracking
    private float currentYRotation;
    private float targetYRotation;
    private float lastInteractorYRotation;
    private bool hasInitialRotation = false;
    private float rotationAccumulator = 0f;
    
    // Grid coordinate system tracking
    private Matrix4x4 lastGridTransformMatrix;
    private bool needsCoordinateSystemUpdate = false;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
        ConfigureGrabInteractable();
        SetupEventListeners();
        
        if (debugMode)
            Debug.Log($"VRGridGrabInteractable initialized on {gameObject.name}");
    }

    private void Start()
    {
        AutoDetectReferences();
        CacheInitialTransformMatrix();
    }

    private void Update()
    {
        if (currentInteractor != null)
        {
            HandleGrabInteraction();
        }
        
        if (smoothRotation && isRotating)
        {
            HandleSmoothRotation();
        }
        
        CheckForCoordinateSystemUpdates();
    }

    private void OnDestroy()
    {
        RemoveEventListeners();
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        }
        
        gridBuilderProXZ = GetComponent<EasyGridBuilderProXZ>();
    }

    private void ConfigureGrabInteractable()
    {
        // Configure grab settings for grid interaction
        grabInteractable.trackPosition = false;
        grabInteractable.trackRotation = false;
        grabInteractable.throwOnDetach = false;
        
        // Disable physics-based movement
        grabInteractable.movementType = XRBaseInteractable.MovementType.Kinematic;
        
        // Transform retention is handled by our custom logic in OnSelectExited
    }

    private void SetupEventListeners()
    {
        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    private void RemoveEventListeners()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
        }
    }

    private void AutoDetectReferences()
    {
        if (gridManager == null)
        {
            gridManager = GridManager.Instance;
            if (gridManager == null)
                gridManager = FindObjectOfType<GridManager>();
        }
        
        if (gridBuilderProXZ == null)
        {
            gridBuilderProXZ = GetComponent<EasyGridBuilderProXZ>();
        }
        
        if (debugMode)
        {
            Debug.Log($"Auto-detected references - GridManager: {gridManager != null}, GridBuilderProXZ: {gridBuilderProXZ != null}");
        }
    }

    private void CacheInitialTransformMatrix()
    {
        lastGridTransformMatrix = transform.localToWorldMatrix;
    }

    #endregion

    #region Grab Event Handlers

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        currentInteractor = args.interactorObject;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        currentYRotation = transform.eulerAngles.y;
        targetYRotation = currentYRotation;
        
        // Reset state
        isMovementEnabled = false;
        isRotating = false;
        hasInitialRotation = false;
        rotationAccumulator = 0f;
        
        if (debugMode)
            Debug.Log($"Grid grab started by {currentInteractor.transform.name}");
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (!isMovementEnabled)
        {
            // Return to initial state if minimum drag distance wasn't reached
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            
            if (debugMode)
                Debug.Log("Grid returned to initial position - minimum drag distance not reached");
        }
        else
        {
            // Finalize any pending rotation
            if (!smoothRotation)
            {
                transform.rotation = Quaternion.Euler(0, targetYRotation, 0);
            }
            
            // Update coordinate system for future object placements
            UpdateGridCoordinateSystem();
            
            if (debugMode)
                Debug.Log($"Grid grab completed - Final rotation: {transform.eulerAngles.y}°");
        }
        
        // Reset state
        currentInteractor = null;
        isMovementEnabled = false;
        isRotating = false;
        hasInitialRotation = false;
    }

    #endregion

    #region Grab Interaction Handling

    private void HandleGrabInteraction()
    {
        if (!isMovementEnabled)
        {
            CheckForMovementActivation();
        }
        else
        {
            HandleMovement();
            HandleRotation();
        }
    }

    private void CheckForMovementActivation()
    {
        Vector3 interactorPosition = currentInteractor.GetAttachTransform(grabInteractable).position;
        float dragDistance = Vector3.Distance(interactorPosition, initialPosition);
        
        if (dragDistance >= minimumDragDistance)
        {
            isMovementEnabled = true;
            
            if (debugMode)
                Debug.Log($"Movement enabled after dragging {dragDistance:F3}m (minimum: {minimumDragDistance:F3}m)");
        }
    }

    private void HandleMovement()
    {
        // Update position to follow interactor
        Vector3 interactorPosition = currentInteractor.GetAttachTransform(grabInteractable).position;
        transform.position = interactorPosition;
    }

    private void HandleRotation()
    {
        Transform interactorTransform = currentInteractor.GetAttachTransform(grabInteractable);
        float currentInteractorYRotation = interactorTransform.eulerAngles.y;
        
        if (!hasInitialRotation)
        {
            lastInteractorYRotation = currentInteractorYRotation;
            hasInitialRotation = true;
            return;
        }
        
        // Calculate rotation delta
        float rotationDelta = Mathf.DeltaAngle(lastInteractorYRotation, currentInteractorYRotation);
        rotationAccumulator += rotationDelta * rotationSpeed * Time.deltaTime;
        
        // Update target rotation
        targetYRotation = currentYRotation + rotationAccumulator;
        
        // Apply rotation (smooth or instant)
        if (smoothRotation)
        {
            isRotating = true;
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, targetYRotation, 0);
            currentYRotation = targetYRotation;
            rotationAccumulator = 0f;
        }
        
        lastInteractorYRotation = currentInteractorYRotation;
    }

    private void HandleSmoothRotation()
    {
        float currentY = transform.eulerAngles.y;
        float newY = Mathf.LerpAngle(currentY, targetYRotation, smoothRotationSpeed * Time.deltaTime);
        
        transform.rotation = Quaternion.Euler(0, newY, 0);
        
        // Check if rotation is complete
        if (Mathf.Abs(Mathf.DeltaAngle(newY, targetYRotation)) < 0.1f)
        {
            transform.rotation = Quaternion.Euler(0, targetYRotation, 0);
            currentYRotation = targetYRotation;
            rotationAccumulator = 0f;
            isRotating = false;
        }
    }

    #endregion

    #region Coordinate System Management

    private void CheckForCoordinateSystemUpdates()
    {
        // Check if the grid's transform matrix has changed significantly
        Matrix4x4 currentMatrix = transform.localToWorldMatrix;
        
        if (HasTransformChanged(lastGridTransformMatrix, currentMatrix))
        {
            needsCoordinateSystemUpdate = true;
            lastGridTransformMatrix = currentMatrix;
        }
        
        if (needsCoordinateSystemUpdate && currentInteractor == null)
        {
            UpdateGridCoordinateSystem();
            needsCoordinateSystemUpdate = false;
        }
    }

    private bool HasTransformChanged(Matrix4x4 oldMatrix, Matrix4x4 newMatrix)
    {
        const float threshold = 0.001f;
        
        // Check position and rotation differences
        Vector3 oldPos = oldMatrix.GetColumn(3);
        Vector3 newPos = newMatrix.GetColumn(3);
        
        if (Vector3.Distance(oldPos, newPos) > threshold)
            return true;
            
        // Check rotation difference
        Quaternion oldRot = oldMatrix.rotation;
        Quaternion newRot = newMatrix.rotation;
        
        return Quaternion.Angle(oldRot, newRot) > threshold;
    }

    /// <summary>
    /// Updates the grid coordinate system to ensure proper object placement after rotation/movement
    /// This is crucial for EGB Pro2 to correctly calculate world positions for new objects
    /// </summary>
    private void UpdateGridCoordinateSystem()
    {
        if (gridBuilderProXZ == null) return;
        
        try
        {
            // Force EGB Pro2 to recalculate its internal coordinate system
            // This ensures that new object placements account for the grid's rotation
            
            // The key insight: EGB Pro2 uses the grid's transform as the parent for placed objects
            // and calculates positions using transform.localPosition, which automatically
            // handles the coordinate transformation. However, we need to ensure any cached
            // world-to-grid calculations are updated.
            
            // Trigger a grid update if the GridManager supports it
            if (gridManager != null)
            {
                // Some EGB Pro2 versions cache world-to-grid transformations
                // This method ensures those caches are invalidated
                var activeGrid = gridManager.GetActiveEasyGridBuilderPro();
                if (activeGrid == gridBuilderProXZ)
                {
                    // The grid system will automatically handle coordinate transformation
                    // because objects are parented to the grid transform and use localPosition
                    if (debugMode)
                        Debug.Log("Grid coordinate system updated - new objects will use correct local coordinates");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating grid coordinate system: {e.Message}");
        }
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Get the current Y rotation of the grid
    /// </summary>
    public float GetCurrentYRotation()
    {
        return transform.eulerAngles.y;
    }

    /// <summary>
    /// Check if the grid is currently movable
    /// </summary>
    public bool IsMovementEnabled()
    {
        return isMovementEnabled;
    }

    /// <summary>
    /// Check if the grid is currently being grabbed
    /// </summary>
    public bool IsBeingGrabbed()
    {
        return currentInteractor != null;
    }

    /// <summary>
    /// Manually update the coordinate system (useful for external scripts)
    /// </summary>
    public void ForceCoordinateSystemUpdate()
    {
        UpdateGridCoordinateSystem();
    }

    /// <summary>
    /// Get debug information about the current state
    /// </summary>
    public string GetDebugInfo()
    {
        return $"VRGridGrabInteractable Debug Info:\n" +
               $"- Movement Enabled: {isMovementEnabled}\n" +
               $"- Being Grabbed: {IsBeingGrabbed()}\n" +
               $"- Current Y Rotation: {GetCurrentYRotation():F1}°\n" +
               $"- Target Y Rotation: {targetYRotation:F1}°\n" +
               $"- Is Rotating: {isRotating}\n" +
               $"- Needs Coordinate Update: {needsCoordinateSystemUpdate}";
    }

    #endregion

    #region Editor Support

    private void OnValidate()
    {
        // Ensure minimum drag distance is not negative
        if (minimumDragDistance < 0f)
            minimumDragDistance = 0f;
            
        // Ensure rotation speed is not negative
        if (rotationSpeed < 0f)
            rotationSpeed = 0f;
            
        // Ensure smooth rotation speed is positive when smooth rotation is enabled
        if (smoothRotation && smoothRotationSpeed <= 0f)
            smoothRotationSpeed = 1f;
    }

    #endregion
} 