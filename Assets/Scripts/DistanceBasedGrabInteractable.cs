using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Component that adds distance-based movement to XR interactable objects.
/// Objects can only be moved after being dragged beyond a minimum distance from their initial position.
/// Rotation is limited to 90-degree increments around the Y-axis.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class DistanceBasedGrabInteractable : MonoBehaviour
{
    [Tooltip("The minimum distance in meters that the object needs to be dragged before becoming movable")]
    public float minimumDragDistance = 0.01f;

    [Tooltip("Multiplier for rotation sensitivity. Higher values mean you need to rotate your wrist less.")]
    public float rotationSensitivity = 5f;

    private XRGrabInteractable grabInteractable;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor currentInteractor;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isMovementEnabled = false;
    private Rigidbody rb;
    private bool wasKinematic;
    private RigidbodyConstraints originalConstraints;
    private float currentYRotation;
    private float lastInteractorYRotation;
    private bool hasInitialRotation = false;
    private float accumulatedRotation = 0f;

    private void Awake()
    {
        // Use the default minimumDragDistance set in the inspector
        // Removed dependency on DistanceBasedGrabInstaller class
        
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Store original settings
            wasKinematic = rb.isKinematic;
            originalConstraints = rb.constraints;
        }

        // Configure the grab interactable
        grabInteractable.trackPosition = false;
        grabInteractable.trackRotation = false;
        grabInteractable.throwOnDetach = false;

        // Subscribe to selection events
        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        currentInteractor = args.interactorObject;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        currentYRotation = transform.eulerAngles.y;
        isMovementEnabled = false;
        hasInitialRotation = false;
        accumulatedRotation = 0f;

        // Disable physics and movement
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        
        // Disable position/rotation tracking
        grabInteractable.trackPosition = false;
        grabInteractable.trackRotation = false;
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        currentInteractor = null;
        
        if (!isMovementEnabled)
        {
            // If movement wasn't enabled, return to initial position and rotation
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }
        else
        {
            // Keep current position and snap Y rotation to nearest 90 degrees
            Vector3 currentEuler = transform.eulerAngles;
            float snappedY = Mathf.Round(currentEuler.y / 90f) * 90f;
            ApplyRotation(snappedY);
        }
        
        // Reset state
        isMovementEnabled = false;
        hasInitialRotation = false;
        RestoreOriginalSettings();
    }

    private void Update()
    {
        if (currentInteractor != null)
        {
            if (!isMovementEnabled)
            {
                // Calculate distance between current interactor position and initial grab position
                Vector3 interactorPosition = currentInteractor.GetAttachTransform(grabInteractable).position;
                float distance = Vector3.Distance(interactorPosition, initialPosition);
                
                if (distance >= minimumDragDistance)
                {
                    isMovementEnabled = true;
                    EnableMovement();
                    Debug.Log($"Movement enabled for {gameObject.name} after dragging {distance:F3}m");
                }
            }
            else
            {
                // Update position to follow interactor
                transform.position = currentInteractor.GetAttachTransform(grabInteractable).position;
                
                // Handle Y-axis rotation with 90-degree snapping
                HandleRotation();
            }
        }
    }

    private void HandleRotation()
    {
        float interactorYRotation = currentInteractor.GetAttachTransform(grabInteractable).eulerAngles.y;
        
        if (!hasInitialRotation)
        {
            lastInteractorYRotation = interactorYRotation;
            hasInitialRotation = true;
            return;
        }

        // Calculate rotation delta with increased sensitivity
        float rotationDelta = Mathf.DeltaAngle(lastInteractorYRotation, interactorYRotation) * rotationSensitivity;
        accumulatedRotation += rotationDelta;

        // Check if we've accumulated enough rotation for a 90-degree snap
        if (Mathf.Abs(accumulatedRotation) >= 90f)
        {
            // Calculate how many 90-degree steps we should rotate
            int steps = Mathf.RoundToInt(accumulatedRotation / 90f);
            float newRotation = currentYRotation + steps * 90f;
            
            // Apply the new rotation
            ApplyRotation(newRotation);
            
            // Update tracking variables
            currentYRotation = newRotation;
            accumulatedRotation = 0f; // Reset accumulated rotation
        }

        lastInteractorYRotation = interactorYRotation;
    }
    
    private void ApplyRotation(float yRotation)
    {
        // Apply the rotation to the object
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    private void EnableMovement()
    {
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = true; // Keep kinematic to prevent physics
        }
        
        // Enable position tracking only, rotation is handled manually
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = false;
    }

    private void RestoreOriginalSettings()
    {
        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
            rb.constraints = originalConstraints;
        }
        
        // Reset grab interactable settings
        grabInteractable.trackPosition = false;
        grabInteractable.trackRotation = false;
    }

    /// <summary>
    /// Public method to get the minimum drag distance (replacement for DistanceBasedGrabInstaller)
    /// </summary>
    public static float GetMinimumDragDistance()
    {
        // Return a default value since we don't have the installer class
        return 0.01f;
    }
}