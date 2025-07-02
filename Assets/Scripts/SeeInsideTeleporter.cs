using UnityEngine;
using Unity.XR.CoreUtils;
using SoulGames.EasyGridBuilderPro;

/// <summary>
/// Script voor het implementeren van "See Inside" functionaliteit in VR.
/// Teleporteert de XR Origin naar binnen in cel/halfcel objecten en verandert het camera perspectief.
/// 
/// Gebruik:
/// 1. Voeg dit script toe aan een GameObject in je scene
/// 2. Assign de XR Origin referentie in de Inspector
/// 3. Roep SeeInside(targetObject) aan vanuit je UI knop
/// 
/// Compatibel met Grid Builder Pro 2 cel en halfcel objecten.
/// </summary>
public class SeeInsideTeleporter : MonoBehaviour
{
    [Header("XR Setup")]
    [Tooltip("Referentie naar de XR Origin in de scene")]
    public XROrigin xrOrigin;
    
    [Tooltip("De camera component van de XR Origin")]
    public Camera xrCamera;
    
    [Header("See Inside Settings")]
    [Tooltip("Camera Field of View wanneer binnen in een object (90 graden aanbevolen)")]
    public float insideFOV = 90f;
    
    [Tooltip("Schaal van de speler als percentage van object grootte (0.1 = 10%)")]
    [Range(0.01f, 1f)]
    public float playerScalePercentage = 0.15f;
    
    [Tooltip("Hoogte positie binnen het object (0.5 = midden, 0.1 = laag)")]
    [Range(0.1f, 0.9f)]
    public float heightPercentage = 0.4f;
    
    [Tooltip("Automatisch grid visualisatie verbergen wanneer binnen in object")]
    public bool hideGridWhenInside = true;
    
    [Header("Target Object Settings")]
    [Tooltip("Tags van objecten waarin je kunt teleporteren (cel/halfcel objecten)")]
    public string[] validObjectTags = { "Cell", "HalfCell", "BuildableObject" };
    
    [Header("Controller Compensation")]
    [Tooltip("Automatisch controller posities aanpassen voor FOV verandering")]
    public bool compensateControllerPositions = true;
    
    [Tooltip("Controller compensatie factor (hoeveel dichter bij de camera)")]
    [Range(0.1f, 2f)]
    public float controllerCompensationFactor = 0.7f;
    
    [Header("Debug")]
    [Tooltip("Toon debug informatie in console")]
    public bool showDebugInfo = true;
    
    // Private variabelen voor state management
    private bool isInsideObject = false;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private float originalFOV;
    private GameObject currentInsideObject;
    private EasyGridBuilderPro[] gridSystems;
    private bool[] originalGridVisibility;
    
    // Controller compensatie variabelen
    private Transform leftControllerTransform;
    private Transform rightControllerTransform;
    private Vector3 originalLeftControllerLocalPos;
    private Vector3 originalRightControllerLocalPos;
    private bool hasControllerReferences = false;
    
    private void Awake()
    {
        // Automatisch XR Origin vinden als niet toegewezen
        if (xrOrigin == null)
        {
            xrOrigin = FindObjectOfType<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("SeeInsideTeleporter: Geen XR Origin gevonden in de scene!");
                enabled = false;
                return;
            }
        }
        
        // Camera referentie instellen
        if (xrCamera == null && xrOrigin != null)
        {
            xrCamera = xrOrigin.Camera;
        }
        
        if (xrCamera == null)
        {
            Debug.LogError("SeeInsideTeleporter: Geen camera gevonden in XR Origin!");
            enabled = false;
            return;
        }
        
        // Grid systemen vinden voor later gebruik
        FindGridSystems();
        
        // Controller referenties vinden voor compensatie
        if (compensateControllerPositions)
        {
            FindControllerReferences();
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"SeeInsideTeleporter geïnitialiseerd. XR Origin: {xrOrigin.name}, Camera: {xrCamera.name}");
            if (hasControllerReferences)
            {
                Debug.Log($"Controller compensatie ingeschakeld. Left: {leftControllerTransform?.name}, Right: {rightControllerTransform?.name}");
            }
        }
    }
    
    /// <summary>
    /// Hoofdfunctie voor "See Inside" - teleporteer naar binnen in een object
    /// </summary>
    /// <param name="targetObject">Het object waar je in wilt gaan</param>
    public void SeeInside(GameObject targetObject)
    {
        if (targetObject == null)
        {
            Debug.LogWarning("SeeInsideTeleporter: Target object is null!");
            return;
        }
        
        // Controleer of het object geldig is voor teleportatie
        if (!IsValidTeleportTarget(targetObject))
        {
            Debug.LogWarning($"SeeInsideTeleporter: Object '{targetObject.name}' is geen geldig teleportatie doel!");
            return;
        }
        
        // Als we al binnen in een object zijn, ga eerst naar buiten
        if (isInsideObject)
        {
            ExitObject();
        }
        
        // Sla originele instellingen op
        SaveOriginalSettings();
        
        // Bereken object eigenschappen
        Bounds objectBounds = GetObjectBounds(targetObject);
        Vector3 objectCenter = objectBounds.center;
        Vector3 objectSize = objectBounds.size;
        
        if (showDebugInfo)
        {
            Debug.Log($"SeeInsideTeleporter: Entering object '{targetObject.name}'");
            Debug.Log($"Object bounds: {objectBounds}");
            Debug.Log($"Object center: {objectCenter}, Size: {objectSize}");
        }
        
        // Bereken nieuwe speler schaal op basis van object grootte
        float objectScale = Mathf.Min(objectSize.x, objectSize.z); // Gebruik kleinste horizontale dimensie
        float newPlayerScale = objectScale * playerScalePercentage;
        
        // Bereken positie binnen het object
        Vector3 insidePosition = objectCenter;
        insidePosition.y = objectBounds.min.y + (objectSize.y * heightPercentage);
        
        // Teleporteer de XR Origin
        TeleportXROrigin(insidePosition, newPlayerScale);
        
        // Verander camera perspectief
        SetCameraFOV(insideFOV);
        
        // Compenseer controller posities voor FOV verandering
        if (compensateControllerPositions && hasControllerReferences)
        {
            ApplyControllerCompensation();
        }
        
        // Verberg grid visualisatie indien gewenst
        if (hideGridWhenInside)
        {
            HideGridVisualization();
        }
        
        // Update state
        isInsideObject = true;
        currentInsideObject = targetObject;
        
        // Disable object colliders om camera clipping te voorkomen
        DisableObjectColliders(targetObject, false);
        
        if (showDebugInfo)
        {
            Debug.Log($"SeeInsideTeleporter: Successfully teleported inside '{targetObject.name}'");
            Debug.Log($"New position: {insidePosition}, New scale: {newPlayerScale}, FOV: {insideFOV}");
        }
    }
    
    /// <summary>
    /// Verlaat het huidige object en keer terug naar originele instellingen
    /// </summary>
    public void ExitObject()
    {
        if (!isInsideObject)
        {
            if (showDebugInfo)
                Debug.Log("SeeInsideTeleporter: Niet binnen in een object, geen actie nodig.");
            return;
        }
        
        // Herstel originele instellingen
        RestoreOriginalSettings();
        
        // Herstel controller posities
        if (compensateControllerPositions && hasControllerReferences)
        {
            RestoreControllerPositions();
        }
        
        // Re-enable object colliders
        if (currentInsideObject != null)
        {
            DisableObjectColliders(currentInsideObject, true);
        }
        
        // Toon grid visualisatie weer
        if (hideGridWhenInside)
        {
            RestoreGridVisualization();
        }
        
        // Reset state
        isInsideObject = false;
        currentInsideObject = null;
        
        if (showDebugInfo)
        {
            Debug.Log("SeeInsideTeleporter: Exited object and restored original settings");
        }
    }
    
    /// <summary>
    /// Toggle functie - ga naar binnen of naar buiten afhankelijk van huidige state
    /// </summary>
    /// <param name="targetObject">Het target object (alleen nodig als je naar binnen gaat)</param>
    public void ToggleSeeInside(GameObject targetObject)
    {
        if (isInsideObject)
        {
            ExitObject();
        }
        else
        {
            SeeInside(targetObject);
        }
    }
    
    /// <summary>
    /// Controleer of een object geldig is voor teleportatie
    /// </summary>
    private bool IsValidTeleportTarget(GameObject targetObject)
    {
        // Controleer tags
        foreach (string validTag in validObjectTags)
        {
            if (targetObject.CompareTag(validTag))
                return true;
        }
        
        // Controleer Grid Builder Pro components
        if (targetObject.GetComponent<BuildableGridObject>() != null)
            return true;
        
        // Controleer op cel/halfcel in naam (fallback)
        string objectName = targetObject.name.ToLower();
        if (objectName.Contains("cel") || objectName.Contains("cell") || objectName.Contains("half"))
            return true;
        
        return false;
    }
    
    /// <summary>
    /// Bereken de bounds van een object inclusief alle child renderers
    /// </summary>
    private Bounds GetObjectBounds(GameObject targetObject)
    {
        Bounds bounds = new Bounds();
        bool boundsInitialized = false;
        
        // Probeer eerst de main renderer
        Renderer mainRenderer = targetObject.GetComponent<Renderer>();
        if (mainRenderer != null)
        {
            bounds = mainRenderer.bounds;
            boundsInitialized = true;
        }
        
        // Voeg child renderers toe
        Renderer[] childRenderers = targetObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in childRenderers)
        {
            if (!boundsInitialized)
            {
                bounds = renderer.bounds;
                boundsInitialized = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }
        
        // Als geen renderers, probeer colliders
        if (!boundsInitialized)
        {
            Collider[] colliders = targetObject.GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                if (!boundsInitialized)
                {
                    bounds = col.bounds;
                    boundsInitialized = true;
                }
                else
                {
                    bounds.Encapsulate(col.bounds);
                }
            }
        }
        
        // Als nog steeds geen bounds, gebruik transform data
        if (!boundsInitialized)
        {
            bounds = new Bounds(targetObject.transform.position, Vector3.one * 2f);
            Debug.LogWarning($"SeeInsideTeleporter: Kon geen bounds bepalen voor '{targetObject.name}', gebruik default bounds.");
        }
        
        return bounds;
    }
    
    /// <summary>
    /// Teleporteer de XR Origin naar een nieuwe positie met schaling
    /// </summary>
    private void TeleportXROrigin(Vector3 newPosition, float newScale)
    {
        // Stel nieuwe positie in
        xrOrigin.transform.position = newPosition;
        
        // Stel nieuwe schaal in
        xrOrigin.transform.localScale = Vector3.one * newScale;
    }
    
    /// <summary>
    /// Verander de camera Field of View
    /// </summary>
    private void SetCameraFOV(float newFOV)
    {
        if (xrCamera != null)
        {
            xrCamera.fieldOfView = newFOV;
        }
    }
    
    /// <summary>
    /// Sla originele instellingen op
    /// </summary>
    private void SaveOriginalSettings()
    {
        originalPosition = xrOrigin.transform.position;
        originalScale = xrOrigin.transform.localScale;
        originalFOV = xrCamera.fieldOfView;
    }
    
    /// <summary>
    /// Herstel originele instellingen
    /// </summary>
    private void RestoreOriginalSettings()
    {
        xrOrigin.transform.position = originalPosition;
        xrOrigin.transform.localScale = originalScale;
        xrCamera.fieldOfView = originalFOV;
    }
    
    /// <summary>
    /// Schakel object colliders in/uit
    /// </summary>
    private void DisableObjectColliders(GameObject targetObject, bool enable)
    {
        Collider[] colliders = targetObject.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = enable;
        }
    }
    
    /// <summary>
    /// Zoek alle grid systemen in de scene
    /// </summary>
    private void FindGridSystems()
    {
        gridSystems = FindObjectsOfType<EasyGridBuilderPro>();
        if (gridSystems.Length > 0)
        {
            originalGridVisibility = new bool[gridSystems.Length];
            for (int i = 0; i < gridSystems.Length; i++)
            {
                originalGridVisibility[i] = gridSystems[i].GetIsDisplayObjectGrid();
            }
        }
    }
    
    /// <summary>
    /// Verberg grid visualisatie
    /// </summary>
    private void HideGridVisualization()
    {
        if (gridSystems == null) return;
        
        for (int i = 0; i < gridSystems.Length; i++)
        {
            if (gridSystems[i] != null)
            {
                gridSystems[i].SetDisplayObjectGrid(false);
            }
        }
    }
    
    /// <summary>
    /// Herstel grid visualisatie
    /// </summary>
    private void RestoreGridVisualization()
    {
        if (gridSystems == null || originalGridVisibility == null) return;
        
        for (int i = 0; i < gridSystems.Length && i < originalGridVisibility.Length; i++)
        {
            if (gridSystems[i] != null)
            {
                gridSystems[i].SetDisplayObjectGrid(originalGridVisibility[i]);
            }
        }
    }
    
    /// <summary>
    /// Public property om te controleren of we binnen in een object zijn
    /// </summary>
    public bool IsInsideObject => isInsideObject;
    
    /// <summary>
    /// Public property voor het huidige object waarin we zijn
    /// </summary>
    public GameObject CurrentInsideObject => currentInsideObject;
    
    /// <summary>
    /// Public methode om instellingen aan te passen tijdens runtime
    /// </summary>
    public void UpdateSettings(float newFOV, float newPlayerScale, float newHeight)
    {
        if (isInsideObject && currentInsideObject != null)
        {
            insideFOV = newFOV;
            playerScalePercentage = newPlayerScale;
            heightPercentage = newHeight;
            
            // Herbereken en apply nieuwe instellingen
            SeeInside(currentInsideObject);
        }
    }
    
    /// <summary>
    /// Zoek controller referenties in de XR Origin
    /// </summary>
    private void FindControllerReferences()
    {
        if (xrOrigin == null) return;
        
        // Zoek naar "LeftHand" en "RightHand" GameObjects in de XR Origin
        Transform[] allChildren = xrOrigin.GetComponentsInChildren<Transform>();
        
        foreach (Transform child in allChildren)
        {
            string childName = child.name.ToLower();
            
            // Zoek naar left controller/hand
            if ((childName.Contains("left") && (childName.Contains("hand") || childName.Contains("controller"))) ||
                childName == "lefthandcontroller" || childName == "left hand controller")
            {
                leftControllerTransform = child;
                originalLeftControllerLocalPos = child.localPosition;
            }
            
            // Zoek naar right controller/hand
            if ((childName.Contains("right") && (childName.Contains("hand") || childName.Contains("controller"))) ||
                childName == "righthandcontroller" || childName == "right hand controller")
            {
                rightControllerTransform = child;
                originalRightControllerLocalPos = child.localPosition;
            }
        }
        
        hasControllerReferences = (leftControllerTransform != null || rightControllerTransform != null);
        
        if (showDebugInfo)
        {
            if (hasControllerReferences)
            {
                Debug.Log($"SeeInsideTeleporter: Controller referenties gevonden. Left: {leftControllerTransform?.name}, Right: {rightControllerTransform?.name}");
            }
            else
            {
                Debug.LogWarning("SeeInsideTeleporter: Geen controller referenties gevonden voor compensatie.");
            }
        }
    }
    
    /// <summary>
    /// Pas controller compensatie toe voor FOV verandering
    /// </summary>
    private void ApplyControllerCompensation()
    {
        // Bereken compensatie factor op basis van FOV verandering
        float fovRatio = originalFOV / insideFOV;
        float compensationMultiplier = controllerCompensationFactor * fovRatio;
        
        // Pas compensatie toe op left controller
        if (leftControllerTransform != null)
        {
            Vector3 compensatedPos = originalLeftControllerLocalPos * compensationMultiplier;
            leftControllerTransform.localPosition = compensatedPos;
            
            if (showDebugInfo)
            {
                Debug.Log($"SeeInsideTeleporter: Left controller compensatie toegepast. Origineel: {originalLeftControllerLocalPos}, Nieuw: {compensatedPos}");
            }
        }
        
        // Pas compensatie toe op right controller
        if (rightControllerTransform != null)
        {
            Vector3 compensatedPos = originalRightControllerLocalPos * compensationMultiplier;
            rightControllerTransform.localPosition = compensatedPos;
            
            if (showDebugInfo)
            {
                Debug.Log($"SeeInsideTeleporter: Right controller compensatie toegepast. Origineel: {originalRightControllerLocalPos}, Nieuw: {compensatedPos}");
            }
        }
    }
    
    /// <summary>
    /// Herstel originele controller posities
    /// </summary>
    private void RestoreControllerPositions()
    {
        if (leftControllerTransform != null)
        {
            leftControllerTransform.localPosition = originalLeftControllerLocalPos;
        }
        
        if (rightControllerTransform != null)
        {
            rightControllerTransform.localPosition = originalRightControllerLocalPos;
        }
        
        if (showDebugInfo)
        {
            Debug.Log("SeeInsideTeleporter: Controller posities hersteld naar originele waarden");
        }
    }
    
    private void OnDestroy()
    {
        // Zorg ervoor dat we teruggaan naar originele state als het script wordt vernietigd
        if (isInsideObject)
        {
            ExitObject();
        }
    }
} 