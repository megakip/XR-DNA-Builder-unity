using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRHandMenuController : MonoBehaviour
{
    [Header("Menu Settings")]
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private Transform leftController;
    [SerializeField] private float xRotationTolerance = 15f; // Hoeveel variatie in X rotatie (-25 ± 15)
    [SerializeField] private float zRotationThreshold = 30f; // Maximale Z rotatie voor "hand recht"
    
    [Header("Positioning")]
    [SerializeField] private Vector3 menuOffset = new Vector3(0.08f, 0.02f, 0.05f); // Meer naar rechts
    [SerializeField] private Vector3 menuRotationOffset = new Vector3(-10f, 180f, 0f); // 180° omgedraaid
    
    [Header("Scale")]
    [SerializeField] private float menuScale = 0.001f;
    
    [Header("Animation")]
    [SerializeField] private float animationSpeed = 5f;
    [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Interaction")]
    [SerializeField] private bool ignoreControllerInput = true; // Negeer controller input
    [SerializeField] private float inputCooldownTime = 0.5f; // Cooldown na button press
    [SerializeField] private bool forceMenuAlwaysVisible = false; // Force menu altijd zichtbaar (overruled alles)
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool forceShow = false;
    
    private bool isMenuVisible = false;
    private bool isAnimating = false;
    private float currentAnimationTime = 0f;
    private Vector3 targetScale;
    private Vector3 initialScale;
    private Camera playerCamera;
    private float lastInputTime = 0f;
    
    // Voor betere pols detectie
    private float lastValidAngle = 0f;
    private bool wasHandUp = false;
    
    void Start()
    {
        // Zoek camera
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        // Zoek naar de linker controller als deze niet is toegewezen
        if (leftController == null)
        {
            leftController = FindLeftController();
        }
        
        // Zoek naar het menu canvas als deze niet is toegewezen
        if (menuCanvas == null)
        {
            menuCanvas = GetComponent<Canvas>();
        }
        
        // Setup canvas
        if (menuCanvas != null)
        {
            SetupCanvas();
        }
        
        if (debugMode)
        {
            Debug.Log($"VRHandMenuController gestart.");
            Debug.Log($"LeftController: {(leftController != null ? leftController.name : "Niet gevonden")}");
            Debug.Log($"Camera: {(playerCamera != null ? playerCamera.name : "Niet gevonden")}");
        }
    }
    
    void SetupCanvas()
    {
        // Zorg ervoor dat de canvas op WorldSpace staat
        menuCanvas.renderMode = RenderMode.WorldSpace;
        
        // Zet canvas op UI layer om interactie conflicten te voorkomen
        menuCanvas.gameObject.layer = LayerMask.NameToLayer("UI");
        
        // OPGELOST: Schakel raycasting niet uit bij ignoreControllerInput
        // Het menu moet interactief blijven, ignoreControllerInput betekent alleen
        // dat controller input voor het tonen/verbergen van het menu wordt genegeerd
        // De raycasters moeten AAN blijven voor menu interactie!
        
        // Optioneel: Als je echt raycasting wilt uitschakelen, gebruik dan een andere flag
        // zoals "disableMenuInteraction" in plaats van "ignoreControllerInput"
        
        // Stel de juiste schaal in
        initialScale = Vector3.one * menuScale;
        targetScale = initialScale;
        
        // Start met het menu verborgen
        menuCanvas.transform.localScale = Vector3.zero;
        menuCanvas.gameObject.SetActive(false);
        
        if (debugMode)
            Debug.Log($"Canvas setup complete. Initial scale: {initialScale}. Menu blijft interactief!");
    }
    
    void Update()
    {
        if (leftController == null || menuCanvas == null) return;
        
        // FORCE MENU ALTIJD ZICHTBAAR - overschrijft alles
        if (forceMenuAlwaysVisible)
        {
            // Force het menu aan als het uit staat
            if (!menuCanvas.gameObject.activeSelf)
            {
                menuCanvas.gameObject.SetActive(true);
                if (debugMode)
                    Debug.Log("🔒 Menu geforceerd zichtbaar - was uitgeschakeld door externe input");
            }
            
            // Force de schaal correct als het weggeanimeerd is
            if (menuCanvas.transform.localScale == Vector3.zero)
            {
                menuCanvas.transform.localScale = initialScale;
                if (debugMode)
                    Debug.Log("🔒 Menu schaal hersteld - was weggeanimeerd");
            }
            
            isMenuVisible = true;
            isAnimating = false;
        }
        else
        {
            // Normale logica alleen als force mode uit staat
            
            // Force show voor testing
            if (forceShow && !isMenuVisible)
            {
                ShowMenu();
            }
            
            // Als we controller input negeren, houd menu altijd zichtbaar
            if (ignoreControllerInput && !isMenuVisible && !forceShow)
            {
                ShowMenu();
            }
            
            // Controleer of menu getoond/verborgen moet worden
            if (!forceShow) // Alleen als we niet force mode zijn
            {
                CheckMenuVisibility();
            }
            
            // Update animaties
            UpdateAnimation();
        }
        
        // Update menu positie en rotatie (altijd)
        UpdateMenuTransform();
    }
    
    void UpdateMenuTransform()
    {
        if (menuCanvas == null || leftController == null) return;
        
        // OPTIE 1: Als het menu een CHILD object is van de controller:
        // Je hoeft alleen lokale position en rotation te zetten, dan volgt het automatisch!
        // menuCanvas.transform.localPosition = menuOffset;
        // menuCanvas.transform.localRotation = Quaternion.Euler(menuRotationOffset);
        // return; // Uncomment deze regel om lokale transformatie te gebruiken
        
        // OPTIE 2: Menu volgt controller rotatie precies (huidige implementatie)
        // Bereken positie relatief aan de controller
        Vector3 targetPosition = leftController.position + leftController.TransformDirection(menuOffset);
        
        // Menu volgt EXACT de controller rotatie met rotatie offset
        // Dit zorgt ervoor dat het menu precies mee roteert met de controller
        Quaternion targetRotation = leftController.rotation * Quaternion.Euler(menuRotationOffset);
        
        // Update transform - menu volgt nu controller rotatie exact
        menuCanvas.transform.position = targetPosition;
        menuCanvas.transform.rotation = targetRotation;
        
        if (debugMode && Time.frameCount % 30 == 0) // Log elke 30 frames
        {
            Debug.Log($"Controller Rot: {leftController.rotation.eulerAngles}, Menu Rot: {targetRotation.eulerAngles}");
        }
    }
    
    void CheckMenuVisibility()
    {
        if (leftController == null) return;
        
        // Skip als we controller input negeren - menu blijft dan altijd in huidige staat
        if (ignoreControllerInput)
        {
            return;
        }
        
        // Skip als we recent input hebben gehad
        if (Time.time - lastInputTime < inputCooldownTime)
        {
            return;
        }
        
        // Gebruik de controller rotatie Euler angles voor betere detectie
        Vector3 controllerRotation = leftController.rotation.eulerAngles;
        
        // Normaliseer angles naar -180 tot 180 range voor betere vergelijking
        float xRot = NormalizeAngle(controllerRotation.x);
        float yRot = NormalizeAngle(controllerRotation.y);
        float zRot = NormalizeAngle(controllerRotation.z);
        
        // Gebaseerd op jouw screenshots:
        // Screenshot 1 (tonen): X ≈ -26, Y ≈ -21, Z ≈ 0.5
        // Screenshot 2 (verbergen): X ≈ -3, Y ≈ 3, Z ≈ -98
        
        // Hand is in "toon menu" positie als:
        // - X rotatie rond -25° (± xRotationTolerance)
        // - Z rotatie binnen threshold (hand niet te veel gedraaid)
        float targetXRotation = -25f; // Ideale X rotatie uit screenshot 1
        bool isHandUp = (Mathf.Abs(xRot - targetXRotation) <= xRotationTolerance) && 
                        (Mathf.Abs(zRot) <= zRotationThreshold);
        
        // Hand is in "verberg menu" positie als:
        // - Z rotatie meer dan threshold * 2 gedraaid (zoals in screenshot 2: -98°)
        // - OF X rotatie te horizontaal (meer dan 10° vanaf target)
        bool isHandDown = (Mathf.Abs(zRot) > zRotationThreshold * 2f) || 
                          (Mathf.Abs(xRot - targetXRotation) > xRotationTolerance * 2f);
        
        // Debug info (meer gedetailleerd)
        if (debugMode)
        {
            Debug.Log($"Controller Rot - X: {xRot:F1}°, Y: {yRot:F1}°, Z: {zRot:F1}° | HandUp: {isHandUp}, HandDown: {isHandDown}");
        }
        
        // State machine voor menu visibility
        if (isHandUp && !isMenuVisible && !wasHandUp)
        {
            // Hand in ideale positie -> toon menu
            ShowMenu();
        }
        else if (isHandDown && isMenuVisible && wasHandUp)
        {
            // Hand weggedraaid of naar beneden -> verberg menu
            HideMenu();
        }
        
        // Update state
        wasHandUp = isHandUp;
        lastValidAngle = Mathf.Abs(zRot); // Track Z rotation als primary indicator
    }
    
    void LateUpdate()
    {
        // Extra veiligheid: controleer in LateUpdate of menu nog steeds zichtbaar is
        // Dit draait NA alle andere scripts, dus vangt externe wijzigingen op
        if (forceMenuAlwaysVisible && menuCanvas != null)
        {
            if (!menuCanvas.gameObject.activeSelf)
            {
                menuCanvas.gameObject.SetActive(true);
                if (debugMode)
                    Debug.Log("🔒 LateUpdate: Menu geforceerd zichtbaar");
            }
            
            if (menuCanvas.transform.localScale != initialScale)
            {
                menuCanvas.transform.localScale = initialScale;
                if (debugMode)
                    Debug.Log("🔒 LateUpdate: Menu schaal hersteld");
            }
        }
    }
    
    // Helper functie om angles te normaliseren naar -180 tot 180 range
    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
    
    void ShowMenu()
    {
        if (isMenuVisible) return;
        
        isMenuVisible = true;
        isAnimating = true;
        currentAnimationTime = 0f;
        
        menuCanvas.gameObject.SetActive(true);
        
        if (debugMode)
            Debug.Log("🖐️ Handmenu wordt getoond");
    }
    
    void HideMenu()
    {
        if (!isMenuVisible) return;
        
        // Voorkom verbergen als force mode aan staat
        if (forceMenuAlwaysVisible)
        {
            if (debugMode)
                Debug.Log("🔒 Menu verbergen geblokkeerd - force mode actief");
            return;
        }
        
        isMenuVisible = false;
        isAnimating = true;
        currentAnimationTime = 0f;
        
        if (debugMode)
            Debug.Log("👋 Handmenu wordt verborgen");
    }
    
    void UpdateAnimation()
    {
        if (!isAnimating || menuCanvas == null) return;
        
        currentAnimationTime += Time.deltaTime * animationSpeed;
        float normalizedTime = Mathf.Clamp01(currentAnimationTime);
        
        float curveValue = showCurve.Evaluate(normalizedTime);
        
        if (isMenuVisible)
        {
            // Animatie naar zichtbaar
            menuCanvas.transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, curveValue);
        }
        else
        {
            // Animatie naar onzichtbaar
            menuCanvas.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, curveValue);
        }
        
        // Animatie voltooid?
        if (normalizedTime >= 1f)
        {
            isAnimating = false;
            
            if (!isMenuVisible)
            {
                menuCanvas.gameObject.SetActive(false);
            }
        }
    }
    
    // Detecteer controller input om cooldown te starten
    void OnControllerInput()
    {
        // Negeer alle controller input als de optie aan staat
        if (ignoreControllerInput)
        {
            if (debugMode)
                Debug.Log("Controller input ignored - menu blijft zichtbaar");
            return;
        }
        
        lastInputTime = Time.time;
        if (debugMode)
            Debug.Log("Controller input detected - cooldown started");
    }
    
    Transform FindLeftController()
    {
        // Zoek eerst naar LeftHandAnchor (Quest/Meta systemen)
        GameObject leftHandAnchor = GameObject.Find("LeftHandAnchor");
        if (leftHandAnchor != null)
        {
            if (debugMode)
                Debug.Log("LeftHandAnchor gevonden!");
            return leftHandAnchor.transform;
        }
        
        // Zoek naar Left Controller
        GameObject leftControllerObj = GameObject.Find("Left Controller");
        if (leftControllerObj != null)
        {
            if (debugMode)
                Debug.Log("Left Controller gevonden!");
            return leftControllerObj.transform;
        }
        
        // Zoek in XR Rig structuur
        GameObject xrRig = GameObject.Find("XR Origin (XR Rig)");
        if (xrRig != null)
        {
            Transform leftFound = xrRig.transform.Find("Camera Offset/LeftHandAnchor");
            if (leftFound != null)
            {
                if (debugMode)
                    Debug.Log("LeftHandAnchor in XR Rig gevonden!");
                return leftFound;
            }
        }
        
        Debug.LogWarning("⚠️ Linker controller niet gevonden! Probeer handmatig toe te wijzen.");
        return null;
    }
    
    // Public methods voor externe controle
    public void ForceShowMenu()
    {
        forceShow = true;
        ShowMenu();
    }
    
    public void ForceHideMenu()
    {
        forceShow = false;
        HideMenu();
    }
    
    public void ToggleMenu()
    {
        if (isMenuVisible)
            ForceHideMenu();
        else
            ForceShowMenu();
    }
    
    public void SetMenuScale(float scale)
    {
        menuScale = scale;
        initialScale = Vector3.one * menuScale;
        if (menuCanvas != null && !isAnimating)
        {
            menuCanvas.transform.localScale = isMenuVisible ? initialScale : Vector3.zero;
        }
    }
    
    // Call this when controller buttons are pressed
    public void OnTriggerPressed()
    {
        OnControllerInput();
    }
    
    public void OnGripPressed()
    {
        OnControllerInput();
    }
    
    // Extra methods voor andere controller inputs
    public void OnButtonPressed()
    {
        OnControllerInput();
    }
    
    public void OnThumbstickPressed()
    {
        OnControllerInput();
    }
    
    public void OnMenuButtonPressed()
    {
        OnControllerInput();
    }
}