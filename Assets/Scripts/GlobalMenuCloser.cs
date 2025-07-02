using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.InputSystem;

/// <summary>
/// Luistert naar controller acties in lege ruimte om openstaande menu's te sluiten.
/// Voeg dit script toe aan de XR Origin in de scene.
/// </summary>
public class GlobalMenuCloser : MonoBehaviour
{
    [Tooltip("Of er naar controller trigger events moet worden geluisterd")]
    public bool listenToControllers = true;
    
    [Tooltip("Of er naar hand grijp-gebaren moet worden geluisterd")]
    public bool listenToHands = true;
    
    [Tooltip("Een layer mask voor wat als 'lege ruimte' wordt beschouwd (standaard: alles)")]
    public LayerMask emptySpaceLayers = ~0; // Default: all layers
    
    [Tooltip("Hoe vaak de check moet worden uitgevoerd (in seconden)")]
    public float checkInterval = 0.1f;
    
    private float lastCheckTime;
    private float lastMenuCloseTime; // Voorkomt herhaald sluiten van menu's
    
    private XRDirectInteractor[] directInteractors;
    private XRRayInteractor[] rayInteractors;
    
    // Input Action References
    private InputAction leftGripAction;
    private InputAction rightGripAction;
    private InputAction leftTriggerAction;
    private InputAction rightTriggerAction;
    
    // Reference to menu systems (to be set manually or found at runtime)
    [Header("Menu System References")]
    [Tooltip("Drag menu system objects here, or leave empty for auto-detection")]
    public GameObject[] menuObjects;
    
    private void OnEnable()
    {
        try
        {
            // Zoek Input Actions via InputSystem
            InitializeInputActions();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Kon Input Actions niet initialiseren: {e.Message}");
        }
    }
    
    private void OnDisable()
    {
        // Cleanup input actions
        DisableInputActions();
    }
    
    private void InitializeInputActions()
    {
        try
        {
            // Poging om direct toegang te krijgen tot de InputActionAssets
            var assets = Resources.FindObjectsOfTypeAll<InputActionAsset>();
            
            if (assets.Length > 0)
            {
                // Debug informatie
                Debug.Log($"Gevonden InputActionAssets: {assets.Length}");
                
                foreach (var asset in assets)
                {
                    Debug.Log($"Asset naam: {asset.name}");
                    
                    foreach (var map in asset.actionMaps)
                    {
                        Debug.Log($"  ActionMap: {map.name}");
                        
                        // Probeer alle actie-maps te vinden die gerelateerd zijn aan controllers
                        if (map.name.Contains("XRI") || map.name.Contains("Controller") || 
                            map.name.Contains("Hand") || map.name.Contains("Left") || 
                            map.name.Contains("Right"))
                        {
                            // Loop door alle acties in deze map
                            foreach (var action in map.actions)
                            {
                                // Zoek acties gerelateerd aan grip of trigger
                                if (action.name.Contains("Grip") || action.name.Contains("Trigger") || 
                                    action.name.Contains("Activate") || action.name.Contains("Select"))
                                {
                                    Debug.Log($"    Relevante actie gevonden: {action.name} in {map.name}");
                                    
                                    // Registreer voor deze actie
                                    action.performed += OnAnyControllerButtonPressed;
                                    action.Enable();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("Geen InputActionAssets gevonden!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Fout bij initialiseren van input actions: {e.Message}\n{e.StackTrace}");
        }
    }
    
    private void DisableInputActions()
    {
        try
        {
            // Poging om alle acties te deregistreren
            var assets = Resources.FindObjectsOfTypeAll<InputActionAsset>();
            foreach (var asset in assets)
            {
                foreach (var map in asset.actionMaps)
                {
                    if (map.name.Contains("XRI") || map.name.Contains("Controller") || 
                        map.name.Contains("Hand") || map.name.Contains("Left") || 
                        map.name.Contains("Right"))
                    {
                        foreach (var action in map.actions)
                        {
                            if (action.name.Contains("Grip") || action.name.Contains("Trigger") || 
                                action.name.Contains("Activate") || action.name.Contains("Select"))
                            {
                                action.performed -= OnAnyControllerButtonPressed;
                                action.Disable();
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Fout bij deregistreren van input actions: {e.Message}");
        }
        
        // Ook oorspronkelijke acties deregistreren
        if (leftGripAction != null)
        {
            leftGripAction.performed -= OnGripPerformed;
            leftGripAction.Disable();
        }
        
        if (rightGripAction != null)
        {
            rightGripAction.performed -= OnGripPerformed;
            rightGripAction.Disable();
        }
        
        if (leftTriggerAction != null)
        {
            leftTriggerAction.performed -= OnTriggerPerformed;
            leftTriggerAction.Disable();
        }
        
        if (rightTriggerAction != null)
        {
            rightTriggerAction.performed -= OnTriggerPerformed;
            rightTriggerAction.Disable();
        }
    }
    
    private void OnGripPerformed(InputAction.CallbackContext context)
    {
        // Check of de grip wordt ingedrukt in een lege ruimte
        CheckForEmptySpaceSelection();
    }
    
    private void OnTriggerPerformed(InputAction.CallbackContext context)
    {
        // Check of de trigger wordt ingedrukt in een lege ruimte
        CheckForEmptySpaceSelection();
    }
    
    private void Start()
    {
        // Vind alle interactors in de scene
        directInteractors = FindObjectsByType<XRDirectInteractor>(FindObjectsSortMode.None);
        rayInteractors = FindObjectsByType<XRRayInteractor>(FindObjectsSortMode.None);
        
        lastCheckTime = Time.time;
        lastMenuCloseTime = Time.time - 1f; // Voorkom dat menu's direct worden gesloten
    }
    
    private void Update()
    {
        // Controleer periodiek in plaats van elk frame voor betere prestaties
        if (Time.time - lastCheckTime < checkInterval)
            return;
            
        lastCheckTime = Time.time;
        
        // Controleer of er open menu's zijn
        if (!AreMenusOpen())
            return; // Als geen menu's open zijn, hoeven we niets te doen
            
        // Controleer op controller activatie in lege ruimte
        if (listenToControllers)
        {
            CheckControllerActivation();
        }
        
        // Controleer op hand-activatie in lege ruimte
        if (listenToHands)
        {
            CheckHandActivation();
        }
        
        // Extra expliciete check voor input wanneer de ray niet op een interactable wijst
        CheckInputWhenPointingAtNothing();
    }
    
    private void CheckForEmptySpaceSelection()
    {
        // Voorkom te snel opnieuw sluiten van menu's
        if (Time.time - lastMenuCloseTime < 0.3f)
            return;
            
        // Check alle ray interactors
        bool hitAnyInteractable = false;
        
        foreach (var rayInteractor in rayInteractors)
        {
            if (rayInteractor == null || !rayInteractor.isActiveAndEnabled)
                continue;
                
            // Controleer of de gebruiker een select actie uitvoert (grip of trigger)
            // We sluit menu's alleen als de gebruiker actief een knop indrukt
            bool isSelectingEmpty = rayInteractor.isSelectActive;
            
            if (!isSelectingEmpty)
                continue; // Als de gebruiker niet selecteert, ga door naar de volgende interactor
                
            // Controleer of deze ray een interactable raakt
            bool rayHitInteractable = false;
            
            // Probeer eerst te bepalen of de ray een interactable selecteert
            if (rayInteractor.hasSelection)
            {
                rayHitInteractable = true;
            }
            else
            {
                // Als er geen selectie is, controleer of de ray een interactable raakt
                if (Physics.Raycast(
                    rayInteractor.rayOriginTransform.position, 
                    rayInteractor.rayOriginTransform.forward, 
                    out RaycastHit hit, 
                    20f,
                    emptySpaceLayers))
                {
                    // Als we een object raken, controleer of het een interactable heeft
                    var interactable = hit.collider.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRInteractable>();
                    if (interactable != null)
                    {
                        rayHitInteractable = true;
                    }
                }
            }
            
            // Als we een interactable hebben geraakt, markeer dit
            if (rayHitInteractable)
            {
                hitAnyInteractable = true;
            }
            else
            {
                // Als we geen interactable raken, maar wel een select actie uitvoeren, sluit alle menu's
                if (isSelectingEmpty)
                {
                    HideAllMenus();
                    lastMenuCloseTime = Time.time;
                    return; // Stop na het sluiten van alle menu's
                }
            }
        }
    }
    
    private void CheckControllerActivation()
    {
        // Loop door ray interactors (controllers)
        foreach (var rayInteractor in rayInteractors)
        {
            if (rayInteractor == null || !rayInteractor.isActiveAndEnabled)
                continue;
                
            // Controleer of de selectie actief is
            if (rayInteractor.isSelectActive)
            {
                // Controleer of er een hit is in lege ruimte (geen interactable)
                bool hitEmptySpace = true; // Begin met aanname dat we lege ruimte raken
                
                // Is er een selectie?
                if (rayInteractor.hasSelection)
                {
                    // Als we iets selecteren, dan raken we geen lege ruimte
                    hitEmptySpace = false;
                }
                else
                {
                    // Gebruik eenvoudige raycast in plaats van TryGetCurrentRaycast
                    if (Physics.Raycast(
                        rayInteractor.rayOriginTransform.position, 
                        rayInteractor.rayOriginTransform.forward, 
                        out RaycastHit hit, 
                        20f,
                        emptySpaceLayers))
                    {
                        // Als we een object raken, controleer of het een interactable heeft
                        var interactable = hit.collider.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRInteractable>();
                        if (interactable != null)
                        {
                            hitEmptySpace = false;
                        }
                    }
                }
                
                // Als we lege ruimte raken en er is een actieve select actie, sluit alle menu's
                if (hitEmptySpace)
                {
                    // Voorkom te snel opnieuw sluiten van menu's
                    if (Time.time - lastMenuCloseTime > 0.3f)
                    {
                        Debug.Log("Sluit menu's: controller richt op lege ruimte en selecteert");
                        HideAllMenus();
                        lastMenuCloseTime = Time.time;
                    }
                    
                    // Even wachten voordat we opnieuw checken
                    lastCheckTime = Time.time + 0.5f;
                    return;
                }
            }
        }
    }
    
    private void CheckHandActivation()
    {
        // Loop door direct interactors (meestal hand tracking)
        foreach (var directInteractor in directInteractors)
        {
            if (directInteractor == null || !directInteractor.isActiveAndEnabled)
                continue;
                
            // Controleer of er een grijp-gebaar wordt gemaakt
            if (directInteractor.isSelectActive)
            {
                // Als het niet op een interactable is, beschouw het als lege ruimte
                if (directInteractor.hasSelection == false)
                {
                    // Voorkom te snel opnieuw sluiten van menu's
                    if (Time.time - lastMenuCloseTime > 0.3f)
                    {
                        // Sluit alle menu's
                        HideAllMenus();
                        lastMenuCloseTime = Time.time;
                    }
                    
                    // Even wachten voordat we opnieuw checken
                    lastCheckTime = Time.time + 0.5f;
                    return;
                }
            }
        }
    }
    
    // Controleer of er open menu's zijn
    private bool AreMenusOpen()
    {
        // Check manually assigned menu objects
        if (menuObjects != null)
        {
            foreach (var menuObj in menuObjects)
            {
                if (menuObj != null && menuObj.activeInHierarchy)
                {
                    return true;
                }
            }
        }
        
        // Fallback: try to find common menu patterns
        var canvasObjects = FindObjectsOfType<Canvas>();
        foreach (var canvas in canvasObjects)
        {
            if (canvas.gameObject.activeInHierarchy && 
                (canvas.name.ToLower().Contains("menu") || 
                 canvas.name.ToLower().Contains("ui") ||
                 canvas.name.ToLower().Contains("popup")))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private void CheckInputWhenPointingAtNothing()
    {
        // Loop door alle ray interactors
        foreach (var rayInteractor in rayInteractors)
        {
            if (rayInteractor == null || !rayInteractor.isActiveAndEnabled)
                continue;
                
            // Controleer of de gebruiker een knop indrukt (grip of trigger)
            bool isButtonPressed = rayInteractor.isSelectActive;
            
            if (!isButtonPressed)
                continue;
                
            // Controleer of de ray een interactable object raakt
            bool rayHitsInteractable = false;
            
            // Als de interactor al iets selecteert, wijst het op een interactable
            if (rayInteractor.hasSelection)
            {
                rayHitsInteractable = true;
            }
            else
            {
                // Cast ray en check of er iets wordt geraakt
                if (Physics.Raycast(
                    rayInteractor.rayOriginTransform.position, 
                    rayInteractor.rayOriginTransform.forward, 
                    out RaycastHit hit, 
                    20f))
                {
                    // Check of het geraakte object een interactable component heeft
                    var interactable = hit.collider.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRInteractable>();
                    if (interactable != null)
                    {
                        rayHitsInteractable = true;
                    }
                }
            }
            
            // Als de ray niet op een interactable wijst maar er wel een knop is ingedrukt, sluit menu's
            if (!rayHitsInteractable && isButtonPressed)
            {
                if (Time.time - lastMenuCloseTime > 0.3f)
                {
                    Debug.Log("Expliciete check: knop ingedrukt terwijl ray niet op interactable wijst - sluit menu's");
                    HideAllMenus();
                    lastMenuCloseTime = Time.time;
                    return;
                }
            }
        }
    }
    
    private void OnAnyControllerButtonPressed(InputAction.CallbackContext context)
    {
        Debug.Log($"Controller knop ingedrukt: {context.action.name}");
        
        // Alleen doorgaan als er menu's open zijn
        if (!AreMenusOpen())
            return;
        
        // Voorkom te snel opnieuw sluiten van menu's
        if (Time.time - lastMenuCloseTime < 0.3f)
            return;
        
        // Check of we op een lege plek wijzen
        bool pointingAtEmpty = true;
        
        // Loop door alle ray interactors om te checken of een ervan op een interactable wijst
        foreach (var rayInteractor in rayInteractors)
        {
            if (rayInteractor == null || !rayInteractor.isActiveAndEnabled)
                continue;
            
            // Als de interactor al iets selecteert, wijst het op een interactable
            if (rayInteractor.hasSelection)
            {
                pointingAtEmpty = false;
                break;
            }
            
            // Cast ray en check of er een interactable wordt geraakt
            if (Physics.Raycast(
                rayInteractor.rayOriginTransform.position, 
                rayInteractor.rayOriginTransform.forward, 
                out RaycastHit hit, 
                20f))
            {
                // Check of het geraakte object een interactable component heeft
                var interactable = hit.collider.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRInteractable>();
                if (interactable != null)
                {
                    pointingAtEmpty = false;
                    break;
                }
            }
        }
        
        // Als we inderdaad op een lege plek wijzen en een knop indrukken, sluit alle menu's
        if (pointingAtEmpty)
        {
            Debug.Log("Direct InputSystem Event: Knop ingedrukt terwijl ray niet op interactable wijst - sluit menu's");
            HideAllMenus();
            lastMenuCloseTime = Time.time;
        }
    }
    
    /// <summary>
    /// Hide all menus (replacement for ObjectMenuController.HideAllMenus())
    /// </summary>
    private void HideAllMenus()
    {
        // Hide manually assigned menu objects
        if (menuObjects != null)
        {
            foreach (var menuObj in menuObjects)
            {
                if (menuObj != null && menuObj.activeInHierarchy)
                {
                    menuObj.SetActive(false);
                    Debug.Log($"Hiding menu: {menuObj.name}");
                }
            }
        }
        
        // Fallback: try to hide common menu patterns
        var canvasObjects = FindObjectsOfType<Canvas>();
        foreach (var canvas in canvasObjects)
        {
            if (canvas.gameObject.activeInHierarchy && 
                (canvas.name.ToLower().Contains("menu") || 
                 canvas.name.ToLower().Contains("popup")))
            {
                canvas.gameObject.SetActive(false);
                Debug.Log($"Hiding canvas menu: {canvas.name}");
            }
        }
        
        Debug.Log("All menus hidden");
    }
    
    /// <summary>
    /// Public method to manually assign menu objects
    /// </summary>
    public void SetMenuObjects(GameObject[] menus)
    {
        menuObjects = menus;
    }
    
    /// <summary>
    /// Public method to add a single menu object
    /// </summary>
    public void AddMenuObject(GameObject menu)
    {
        if (menuObjects == null)
        {
            menuObjects = new GameObject[] { menu };
        }
        else
        {
            var newArray = new GameObject[menuObjects.Length + 1];
            menuObjects.CopyTo(newArray, 0);
            newArray[menuObjects.Length] = menu;
            menuObjects = newArray;
        }
    }
}