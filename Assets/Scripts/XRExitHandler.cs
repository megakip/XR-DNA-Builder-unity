using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Script voor het afhandelen van XR controller input om uit "See Inside" perspectief te gaan.
/// Luistert naar verschillende XR controller knoppen en roept ExitObject aan op de SeeInsideTeleporter.
/// 
/// Gebruik:
/// 1. Voeg dit script toe aan een GameObject in je scene
/// 2. Assign de SeeInsideTeleporter referentie in de Inspector
/// 3. Kies welke knoppen je wilt gebruiken om te exiten
/// 4. Het script luistert automatisch naar de geselecteerde input acties
/// 
/// Compatibel met Unity XR Interaction Toolkit input systeem.
/// </summary>
public class XRExitHandler : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Referentie naar de SeeInsideTeleporter component")]
    public SeeInsideTeleporter seeInsideTeleporter;
    
    [Header("Exit Input Settings")]
    [Tooltip("Luister naar Menu knop (Y/B knop op controllers)")]
    public bool useMenuButton = true;
    
    [Tooltip("Luister naar Primary knop (X/A knop op controllers)")]
    public bool usePrimaryButton = false;
    
    [Tooltip("Luister naar Secondary knop (Y/B knop op controllers)")]
    public bool useSecondaryButton = false;
    
    [Tooltip("Luister naar Grip knop")]
    public bool useGripButton = false;
    
    [Tooltip("Luister naar Trigger knop")]
    public bool useTriggerButton = false;
    
    [Header("Input Actions")]
    [Tooltip("Menu button input action (automatisch gevonden als leeg)")]
    public InputActionReference menuButtonAction;
    
    [Tooltip("Primary button input action (automatisch gevonden als leeg)")]
    public InputActionReference primaryButtonAction;
    
    [Tooltip("Secondary button input action (automatisch gevonden als leeg)")]
    public InputActionReference secondaryButtonAction;
    
    [Tooltip("Grip button input action (automatisch gevonden als leeg)")]
    public InputActionReference gripButtonAction;
    
    [Tooltip("Trigger button input action (automatisch gevonden als leeg)")]
    public InputActionReference triggerButtonAction;
    
    [Header("Settings")]
    [Tooltip("Alleen exiten als we binnen in een object zijn")]
    public bool onlyExitWhenInside = true;
    
    [Tooltip("Welke hand controllers luisteren (Both, Left, Right)")]
    public HandSelection handSelection = HandSelection.Both;
    
    [Tooltip("Debug logging inschakelen")]
    public bool enableDebugLogging = true;
    
    public enum HandSelection
    {
        Both,
        LeftOnly,
        RightOnly
    }
    
    // Private variables
    private InputAction menuAction;
    private InputAction primaryAction;
    private InputAction secondaryAction;
    private InputAction gripAction;
    private InputAction triggerAction;
    
    private void Awake()
    {
        // Automatisch SeeInsideTeleporter vinden als niet ingesteld
        if (seeInsideTeleporter == null)
        {
            seeInsideTeleporter = FindObjectOfType<SeeInsideTeleporter>();
            if (seeInsideTeleporter == null)
            {
                Debug.LogError("XRExitHandler: Geen SeeInsideTeleporter gevonden in de scene!");
                enabled = false;
                return;
            }
        }
        
        // Setup input actions
        SetupInputActions();
    }
    
    private void OnEnable()
    {
        EnableInputActions();
    }
    
    private void OnDisable()
    {
        DisableInputActions();
    }
    
    /// <summary>
    /// Setup alle input actions
    /// </summary>
    private void SetupInputActions()
    {
        // Menu Button
        if (useMenuButton)
        {
            if (menuButtonAction != null)
            {
                menuAction = menuButtonAction.action;
            }
            else
            {
                // Probeer automatisch te vinden
                menuAction = FindInputAction("Menu");
            }
            
            if (menuAction != null)
            {
                menuAction.performed += OnMenuButtonPressed;
            }
        }
        
        // Primary Button
        if (usePrimaryButton)
        {
            if (primaryButtonAction != null)
            {
                primaryAction = primaryButtonAction.action;
            }
            else
            {
                primaryAction = FindInputAction("Primary Button");
            }
            
            if (primaryAction != null)
            {
                primaryAction.performed += OnPrimaryButtonPressed;
            }
        }
        
        // Secondary Button
        if (useSecondaryButton)
        {
            if (secondaryButtonAction != null)
            {
                secondaryAction = secondaryButtonAction.action;
            }
            else
            {
                secondaryAction = FindInputAction("Secondary Button");
            }
            
            if (secondaryAction != null)
            {
                secondaryAction.performed += OnSecondaryButtonPressed;
            }
        }
        
        // Grip Button
        if (useGripButton)
        {
            if (gripButtonAction != null)
            {
                gripAction = gripButtonAction.action;
            }
            else
            {
                gripAction = FindInputAction("Grip");
            }
            
            if (gripAction != null)
            {
                gripAction.performed += OnGripButtonPressed;
            }
        }
        
        // Trigger Button
        if (useTriggerButton)
        {
            if (triggerButtonAction != null)
            {
                triggerAction = triggerButtonAction.action;
            }
            else
            {
                triggerAction = FindInputAction("Select");
            }
            
            if (triggerAction != null)
            {
                triggerAction.performed += OnTriggerButtonPressed;
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"XRExitHandler: Input actions setup voltooid. Menu: {menuAction != null}, Primary: {primaryAction != null}, Secondary: {secondaryAction != null}, Grip: {gripAction != null}, Trigger: {triggerAction != null}");
        }
    }
    
    /// <summary>
    /// Probeer een input action te vinden op basis van naam
    /// </summary>
    private InputAction FindInputAction(string actionName)
    {
        // Zoek in alle XRBaseController componenten
        XRBaseController[] controllers = FindObjectsOfType<XRBaseController>();
        
        foreach (var controller in controllers)
        {
            // Check hand selection
            if (!IsValidController(controller)) continue;
            
            // Probeer verschillende action namen
            string[] possibleNames = GetPossibleActionNames(actionName);
            
            foreach (string name in possibleNames)
            {
                var actionProperty = controller.GetType().GetProperty(name.Replace(" ", "").ToLower() + "Action");
                if (actionProperty != null)
                {
                    var actionValue = actionProperty.GetValue(controller);
                    if (actionValue is InputActionProperty actionRef && actionRef.action != null)
                    {
                        if (enableDebugLogging)
                        {
                            Debug.Log($"XRExitHandler: Gevonden input action '{name}' op controller '{controller.name}'");
                        }
                        return actionRef.action;
                    }
                }
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.LogWarning($"XRExitHandler: Kon input action '{actionName}' niet automatisch vinden. Stel handmatig in via Inspector.");
        }
        
        return null;
    }
    
    /// <summary>
    /// Check of een controller geldig is op basis van hand selection
    /// </summary>
    private bool IsValidController(XRBaseController controller)
    {
        if (handSelection == HandSelection.Both) return true;
        
        string controllerName = controller.name.ToLower();
        
        if (handSelection == HandSelection.LeftOnly)
        {
            return controllerName.Contains("left");
        }
        else if (handSelection == HandSelection.RightOnly)
        {
            return controllerName.Contains("right");
        }
        
        return true;
    }
    
    /// <summary>
    /// Krijg mogelijke action namen voor een gegeven action type
    /// </summary>
    private string[] GetPossibleActionNames(string actionType)
    {
        switch (actionType.ToLower())
        {
            case "menu":
                return new[] { "menu", "menuButton", "Menu Button" };
            case "primary button":
                return new[] { "primaryButton", "primary", "Primary Button" };
            case "secondary button":
                return new[] { "secondaryButton", "secondary", "Secondary Button" };
            case "grip":
                return new[] { "grip", "gripButton", "Grip Button" };
            case "select":
                return new[] { "select", "selectAction", "trigger", "Select" };
            default:
                return new[] { actionType };
        }
    }
    
    /// <summary>
    /// Enable alle input actions
    /// </summary>
    private void EnableInputActions()
    {
        menuAction?.Enable();
        primaryAction?.Enable();
        secondaryAction?.Enable();
        gripAction?.Enable();
        triggerAction?.Enable();
    }
    
    /// <summary>
    /// Disable alle input actions
    /// </summary>
    private void DisableInputActions()
    {
        menuAction?.Disable();
        primaryAction?.Disable();
        secondaryAction?.Disable();
        gripAction?.Disable();
        triggerAction?.Disable();
    }
    
    /// <summary>
    /// Menu button event handler
    /// </summary>
    private void OnMenuButtonPressed(InputAction.CallbackContext context)
    {
        HandleExitInput("Menu Button");
    }
    
    /// <summary>
    /// Primary button event handler
    /// </summary>
    private void OnPrimaryButtonPressed(InputAction.CallbackContext context)
    {
        HandleExitInput("Primary Button");
    }
    
    /// <summary>
    /// Secondary button event handler
    /// </summary>
    private void OnSecondaryButtonPressed(InputAction.CallbackContext context)
    {
        HandleExitInput("Secondary Button");
    }
    
    /// <summary>
    /// Grip button event handler
    /// </summary>
    private void OnGripButtonPressed(InputAction.CallbackContext context)
    {
        HandleExitInput("Grip Button");
    }
    
    /// <summary>
    /// Trigger button event handler
    /// </summary>
    private void OnTriggerButtonPressed(InputAction.CallbackContext context)
    {
        HandleExitInput("Trigger Button");
    }
    
    /// <summary>
    /// Centrale exit input handler
    /// </summary>
    private void HandleExitInput(string buttonName)
    {
        if (seeInsideTeleporter == null) return;
        
        // Check of we alleen moeten exiten als we binnen zijn
        if (onlyExitWhenInside && !seeInsideTeleporter.IsInsideObject)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"XRExitHandler: {buttonName} ingedrukt, maar niet binnen in een object. Geen actie ondernomen.");
            }
            return;
        }
        
        // Exit het object
        if (seeInsideTeleporter.IsInsideObject)
        {
            seeInsideTeleporter.ExitObject();
            
            if (enableDebugLogging)
            {
                Debug.Log($"XRExitHandler: {buttonName} ingedrukt - Exited object via XR input");
            }
        }
        else
        {
            if (enableDebugLogging)
            {
                Debug.Log($"XRExitHandler: {buttonName} ingedrukt, maar niet binnen in een object");
            }
        }
    }
    
    /// <summary>
    /// Public methode om handmatig te exiten (voor andere scripts)
    /// </summary>
    public void ExitViaScript()
    {
        HandleExitInput("Script Call");
    }
    
    /// <summary>
    /// Update welke knoppen actief zijn tijdens runtime
    /// </summary>
    public void UpdateButtonSettings(bool menu, bool primary, bool secondary, bool grip, bool trigger)
    {
        useMenuButton = menu;
        usePrimaryButton = primary;
        useSecondaryButton = secondary;
        useGripButton = grip;
        useTriggerButton = trigger;
        
        // Disable alle actions en setup opnieuw
        DisableInputActions();
        SetupInputActions();
        EnableInputActions();
        
        if (enableDebugLogging)
        {
            Debug.Log("XRExitHandler: Button settings updated tijdens runtime");
        }
    }
    
    private void OnDestroy()
    {
        // Cleanup input actions
        DisableInputActions();
        
        if (menuAction != null) menuAction.performed -= OnMenuButtonPressed;
        if (primaryAction != null) primaryAction.performed -= OnPrimaryButtonPressed;
        if (secondaryAction != null) secondaryAction.performed -= OnSecondaryButtonPressed;
        if (gripAction != null) gripAction.performed -= OnGripButtonPressed;
        if (triggerAction != null) triggerAction.performed -= OnTriggerButtonPressed;
    }
} 