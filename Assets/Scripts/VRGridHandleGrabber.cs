using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using SoulGames.EasyGridBuilderPro;

/// <summary>
/// VR Grid Handle Grabber for Easy Grid Builder Pro 2
/// Attach this to a Handle object to grab and manipulate the grid indirectly
/// 
/// Setup Instructions:
/// 1. Create a Handle GameObject (or use existing one) - this is what you'll grab
/// 2. Attach this script to the Handle GameObject
/// 3. Assign the Grid GameObject (with EasyGridBuilderProXZ) in the Grid Target field
/// 4. The handle will control the grid's position and rotation
/// 5. Add EGBProCoordinateSystemSync to the Grid GameObject (not the handle)
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class VRGridHandleGrabber : MonoBehaviour
{
    [Header("Grid Target")]
    [Tooltip("The grid GameObject that contains the EasyGridBuilderProXZ component")]
    public GameObject gridTarget;
    
    [Header("Movement Settings")]
    [Tooltip("Minimum distance in meters that the handle needs to be dragged before grid becomes movable")]
    public float minimumDragDistance = 0.01f;
    
    [Tooltip("Enable grid rotation (set to false for position-only movement)")]
    public bool enableRotation = false;
    
    [Tooltip("Speed multiplier for rotation when enabled. Higher values = faster rotation")]
    public float rotationSpeed = 90f;
    
    [Tooltip("Enable smooth rotation (false for instant rotation)")]
    public bool smoothRotation = true;
    
    [Tooltip("Speed of smooth rotation when enabled")]
    public float smoothRotationSpeed = 5f;
    
    [Header("Handle Behavior")]
    [Tooltip("Keep handle position relative to grid")]
    public bool maintainRelativePosition = true;
    
    [Tooltip("Offset of handle from grid center")]
    public Vector3 handleOffset = Vector3.up * 0.5f;
    
    [Header("Debug")]
    [Tooltip("Enable debug logging")]
    public bool debugMode = false;
    
    // Private variables
    private XRGrabInteractable grabInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor currentInteractor;
    private Vector3 initialHandlePosition;
    private Vector3 initialGridPosition;
    private Quaternion initialGridRotation;
    private bool isMovementEnabled = false;
    private bool isRotating = false;
    
    // Grid components
    private EasyGridBuilderProXZ gridBuilderProXZ;
    private EGBProCoordinateSystemSync coordinateSync;
    
    // Rotation tracking
    private float currentYRotation;
    private float targetYRotation;
    private float lastInteractorYRotation;
    private bool hasInitialRotation = false;
    private float rotationAccumulator = 0f;
    
    // Relative positioning
    private Vector3 gridToHandleOffset;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
        ConfigureGrabInteractable();
        SetupEventListeners();
        
        if (debugMode)
            Debug.Log($"VRGridHandleGrabber initialized on {gameObject.name}");
    }

    private void Start()
    {
        AutoDetectGridTarget();
        CalculateInitialOffsets();
    }

    private void Update()
    {
        if (currentInteractor != null)
        {
            HandleGrabInteraction();
        }
        
        if (enableRotation && smoothRotation && isRotating)
        {
            HandleSmoothRotation();
        }
        
        if (maintainRelativePosition && !isMovementEnabled)
        {
            UpdateHandlePosition();
        }
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
    }

    private void ConfigureGrabInteractable()
    {
        // Configure grab settings for handle interaction
        grabInteractable.trackPosition = false;
        grabInteractable.trackRotation = false;
        grabInteractable.throwOnDetach = false;
        
        // Use kinematic movement
        grabInteractable.movementType = XRBaseInteractable.MovementType.Kinematic;
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

    private void AutoDetectGridTarget()
    {
        if (gridTarget == null)
        {
            // Try to find EasyGridBuilderProXZ component in scene
            var egbProComponent = FindObjectOfType<EasyGridBuilderProXZ>();
            if (egbProComponent != null)
            {
                gridTarget = egbProComponent.gameObject;
                if (debugMode)
                    Debug.Log($"Auto-detected grid target: {gridTarget.name}");
            }
        }
        
        if (gridTarget != null)
        {
            gridBuilderProXZ = gridTarget.GetComponent<EasyGridBuilderProXZ>();
            coordinateSync = gridTarget.GetComponent<EGBProCoordinateSystemSync>();
            
            if (debugMode)
            {
                Debug.Log($"Grid components - EGBProXZ: {gridBuilderProXZ != null}, CoordinateSync: {coordinateSync != null}");
            }
        }
    }

    private void CalculateInitialOffsets()
    {
        if (gridTarget != null)
        {
            gridToHandleOffset = transform.position - gridTarget.transform.position;
            
            if (debugMode)
                Debug.Log($"Calculated grid-to-handle offset: {gridToHandleOffset}");
        }
    }

    #endregion

    #region Grab Event Handlers

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        currentInteractor = args.interactorObject;
        initialHandlePosition = transform.position;
        
        if (gridTarget != null)
        {
            initialGridPosition = gridTarget.transform.position;
            initialGridRotation = gridTarget.transform.rotation;
            currentYRotation = gridTarget.transform.eulerAngles.y;
            targetYRotation = currentYRotation;
        }
        
        // Reset state
        isMovementEnabled = false;
        isRotating = false;
        hasInitialRotation = false;
        rotationAccumulator = 0f;
        
        if (debugMode)
            Debug.Log($"Handle grab started by {currentInteractor.transform.name}");
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (!isMovementEnabled)
        {
            // Return to initial state if minimum drag distance wasn't reached
            transform.position = initialHandlePosition;
            if (gridTarget != null)
            {
                gridTarget.transform.position = initialGridPosition;
                gridTarget.transform.rotation = initialGridRotation;
            }
            
            if (debugMode)
                Debug.Log("Handle and grid returned to initial position - minimum drag distance not reached");
        }
        else
        {
            // Finalize any pending rotation (only if rotation is enabled)
            if (enableRotation && !smoothRotation && gridTarget != null)
            {
                gridTarget.transform.rotation = Quaternion.Euler(0, targetYRotation, 0);
            }
            
            // Update coordinate system for future object placements
            if (coordinateSync != null)
            {
                coordinateSync.ForceSynchronization();
            }
            
            if (debugMode)
                Debug.Log($"Handle grab completed - Grid position: {(gridTarget != null ? gridTarget.transform.position : Vector3.zero)}");
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
            if (enableRotation)
            {
                HandleRotation();
            }
        }
    }

    private void CheckForMovementActivation()
    {
        Vector3 interactorPosition = currentInteractor.GetAttachTransform(grabInteractable).position;
        float dragDistance = Vector3.Distance(interactorPosition, initialHandlePosition);
        
        if (dragDistance >= minimumDragDistance)
        {
            isMovementEnabled = true;
            
            if (debugMode)
                Debug.Log($"Movement enabled after dragging {dragDistance:F3}m (minimum: {minimumDragDistance:F3}m)");
        }
    }

    private void HandleMovement()
    {
        if (gridTarget == null) return;
        
        // Update handle position to follow interactor
        Vector3 interactorPosition = currentInteractor.GetAttachTransform(grabInteractable).position;
        transform.position = interactorPosition;
        
        // Update grid position based on handle position and offset
        gridTarget.transform.position = interactorPosition - gridToHandleOffset;
    }

    private void HandleRotation()
    {
        if (gridTarget == null) return;
        
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
            gridTarget.transform.rotation = Quaternion.Euler(0, targetYRotation, 0);
            currentYRotation = targetYRotation;
            rotationAccumulator = 0f;
        }
        
        lastInteractorYRotation = currentInteractorYRotation;
    }

    private void HandleSmoothRotation()
    {
        if (gridTarget == null) return;
        
        float currentY = gridTarget.transform.eulerAngles.y;
        float newY = Mathf.LerpAngle(currentY, targetYRotation, smoothRotationSpeed * Time.deltaTime);
        
        gridTarget.transform.rotation = Quaternion.Euler(0, newY, 0);
        
        // Check if rotation is complete
        if (Mathf.Abs(Mathf.DeltaAngle(newY, targetYRotation)) < 0.1f)
        {
            gridTarget.transform.rotation = Quaternion.Euler(0, targetYRotation, 0);
            currentYRotation = targetYRotation;
            rotationAccumulator = 0f;
            isRotating = false;
        }
    }

    private void UpdateHandlePosition()
    {
        if (gridTarget != null && !isMovementEnabled)
        {
            // Keep handle in relative position to grid
            Vector3 targetHandlePosition = gridTarget.transform.position + gridToHandleOffset;
            transform.position = targetHandlePosition;
        }
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Set the grid target manually
    /// </summary>
    public void SetGridTarget(GameObject newGridTarget)
    {
        gridTarget = newGridTarget;
        if (gridTarget != null)
        {
            gridBuilderProXZ = gridTarget.GetComponent<EasyGridBuilderProXZ>();
            coordinateSync = gridTarget.GetComponent<EGBProCoordinateSystemSync>();
            CalculateInitialOffsets();
        }
    }

    /// <summary>
    /// Get the current grid target
    /// </summary>
    public GameObject GetGridTarget()
    {
        return gridTarget;
    }

    /// <summary>
    /// Check if the handle is currently being grabbed
    /// </summary>
    public bool IsBeingGrabbed()
    {
        return currentInteractor != null;
    }

    /// <summary>
    /// Check if movement is currently enabled
    /// </summary>
    public bool IsMovementEnabled()
    {
        return isMovementEnabled;
    }

    /// <summary>
    /// Get debug information about the current state
    /// </summary>
    public string GetDebugInfo()
    {
        return $"VRGridHandleGrabber Debug Info:\n" +
               $"- Grid Target: {(gridTarget != null ? gridTarget.name : "None")}\n" +
               $"- Movement Enabled: {isMovementEnabled}\n" +
               $"- Being Grabbed: {IsBeingGrabbed()}\n" +
               $"- Rotation Enabled: {enableRotation}\n" +
               $"- Grid Position: {(gridTarget != null ? gridTarget.transform.position : Vector3.zero)}\n" +
               $"- Handle Position: {transform.position}\n" +
               $"- Grid-Handle Offset: {gridToHandleOffset}";
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

    private void OnDrawGizmosSelected()
    {
        if (gridTarget != null)
        {
            // Draw connection line between handle and grid
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, gridTarget.transform.position);
            
            // Draw handle offset visualization
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
            
            // Draw grid center
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(gridTarget.transform.position, 0.1f);
        }
    }

    #endregion
} 