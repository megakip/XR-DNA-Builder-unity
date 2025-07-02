using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script voor het dupliceren van een heel Canvas object met alle inhoud.
/// Plaats dit script op de pin button om het hele canvas te dupliceren wanneer de knop wordt ingedrukt.
/// 
/// Gebruik:
/// 1. Plaats dit script op de Btn_Pin GameObject
/// 2. Stel de Canvas To Duplicate in via de Inspector (het canvas dat gedupliceerd moet worden)
/// 3. Optioneel: configureer de offset positie waar de kopie wordt geplaatst
/// 4. De button zal automatisch worden gekoppeld aan de duplicate functie
/// </summary>
public class CanvasDuplicator : MonoBehaviour
{
    [Header("Canvas Settings")]
    [Tooltip("Het Canvas object dat gedupliceerd moet worden (inclusief alle inhoud)")]
    [SerializeField] private Canvas canvasToDuplicate;
    
    [Header("Duplicate Settings")]
    [Tooltip("Positie offset voor de gedupliceerde canvas (relatief tot originele positie)")]
    [SerializeField] private Vector3 duplicateOffset = new Vector3(100f, 0f, 0f);
    
    [Tooltip("Geef de kopie automatisch een andere naam")]
    [SerializeField] private bool renameDuplicate = true;
    
    [Tooltip("Prefix voor de naam van de gedupliceerde canvas")]
    [SerializeField] private string duplicateNamePrefix = "Copy_";
    
    [Header("Advanced Settings")]
    [Tooltip("Maak de kopie een child van een specifiek parent object (laat leeg voor hetzelfde parent)")]
    [SerializeField] private Transform customParent;
    
    [Tooltip("Schakel interactie uit voor de originele canvas na duplicatie")]
    [SerializeField] private bool disableOriginalAfterDuplicate = false;
    
    private Button pinButton;
    private static int duplicateCounter = 1; // Voor unieke namen
    
    private void Awake()
    {
        // Zoek de Button component op dit GameObject
        pinButton = GetComponent<Button>();
        if (pinButton == null)
        {
            Debug.LogError("[CanvasDuplicator] Geen Button component gevonden! Dit script moet op een GameObject met een Button component worden geplaatst.");
            return;
        }
        
        // Als geen canvas is ingesteld, probeer het automatisch te vinden
        if (canvasToDuplicate == null)
        {
            AutoFindCanvas();
        }
    }
    
    private void Start()
    {
        // Koppel de duplicate functie aan de button click
        if (pinButton != null)
        {
            pinButton.onClick.AddListener(DuplicateCanvas);
        }
    }
    
    /// <summary>
    /// Probeert automatisch het Canvas te vinden in de scene
    /// </summary>
    private void AutoFindCanvas()
    {
        // Zoek naar het Canvas dat parent is van dit button
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            canvasToDuplicate = parentCanvas;
            Debug.Log($"[CanvasDuplicator] Canvas automatisch gevonden: {parentCanvas.name}");
        }
        else
        {
            // Zoek naar de eerste Canvas in de scene
            Canvas foundCanvas = FindObjectOfType<Canvas>();
            if (foundCanvas != null)
            {
                canvasToDuplicate = foundCanvas;
                Debug.Log($"[CanvasDuplicator] Canvas automatisch gevonden in scene: {foundCanvas.name}");
            }
            else
            {
                Debug.LogWarning("[CanvasDuplicator] Geen Canvas gevonden! Stel handmatig een Canvas in via de Inspector.");
            }
        }
    }
    
    /// <summary>
    /// Dupliceert het volledige Canvas object met alle inhoud
    /// </summary>
    public void DuplicateCanvas()
    {
        if (canvasToDuplicate == null)
        {
            Debug.LogError("[CanvasDuplicator] Geen Canvas ingesteld om te dupliceren!");
            return;
        }
        
        Debug.Log($"[CanvasDuplicator] Canvas dupliceren: {canvasToDuplicate.name}");
        
        try
        {
            // Maak een volledige kopie van het Canvas GameObject
            GameObject duplicatedCanvas = Instantiate(canvasToDuplicate.gameObject);
            
            // Stel de parent in
            Transform targetParent = customParent != null ? customParent : canvasToDuplicate.transform.parent;
            duplicatedCanvas.transform.SetParent(targetParent, false);
            
            // Geef de kopie een nieuwe naam
            if (renameDuplicate)
            {
                duplicatedCanvas.name = $"{duplicateNamePrefix}{canvasToDuplicate.name}_{duplicateCounter}";
                duplicateCounter++;
            }
            
            // Pas de positie aan met de offset
            RectTransform originalRect = canvasToDuplicate.GetComponent<RectTransform>();
            RectTransform duplicateRect = duplicatedCanvas.GetComponent<RectTransform>();
            
            if (originalRect != null && duplicateRect != null)
            {
                // Voor UI elementen gebruiken we anchoredPosition
                duplicateRect.anchoredPosition = originalRect.anchoredPosition + new Vector2(duplicateOffset.x, duplicateOffset.y);
            }
            else
            {
                // Voor gewone Transform objecten
                duplicatedCanvas.transform.position = canvasToDuplicate.transform.position + duplicateOffset;
            }
            
            // Schakel de originele canvas uit als gewenst
            if (disableOriginalAfterDuplicate)
            {
                canvasToDuplicate.gameObject.SetActive(false);
                Debug.Log($"[CanvasDuplicator] Originele canvas uitgeschakeld: {canvasToDuplicate.name}");
            }
            
            Debug.Log($"[CanvasDuplicator] Canvas succesvol gedupliceerd: {duplicatedCanvas.name}");
            
            // Optioneel: roep een event aan voor verdere aanpassingen
            OnCanvasDuplicated(duplicatedCanvas);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[CanvasDuplicator] Fout bij het dupliceren van canvas: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Event dat wordt aangeroepen nadat een canvas is gedupliceerd
    /// Override deze methode in afgeleide klassen voor custom functionaliteit
    /// </summary>
    protected virtual void OnCanvasDuplicated(GameObject duplicatedCanvas)
    {
        // Hier kun je extra logica toevoegen die moet gebeuren na duplicatie
        // Bijvoorbeeld: aanpassen van specifieke componenten in de kopie
        
        // Kopieer de huidige staat van het originele canvas naar de kopie
        CopyCanvasState(canvasToDuplicate.gameObject, duplicatedCanvas);
        
        // Verander de pin button in het gedupliceerde paneel naar een close button
        ConvertPinButtonToCloseButton(duplicatedCanvas);
        
        // Voorbeeld: Verwijder CanvasDuplicator scripts van de kopie om oneindige duplicatie te voorkomen
        CanvasDuplicator[] duplicatorScripts = duplicatedCanvas.GetComponentsInChildren<CanvasDuplicator>();
        for (int i = 0; i < duplicatorScripts.Length; i++)
        {
            if (duplicatorScripts[i] != this) // Verwijder niet onszelf
            {
                Destroy(duplicatorScripts[i]);
            }
        }
    }
    
    /// <summary>
    /// Kopieert de huidige staat van het originele canvas naar het gedupliceerde canvas
    /// </summary>
    private void CopyCanvasState(GameObject originalCanvas, GameObject duplicatedCanvas)
    {
        Debug.Log("[CanvasDuplicator] Canvas staat kopiëren...");
        
        // Disable auto-initialization scripts in de kopie om te voorkomen dat ze de staat overschrijven
        DisableAutoInitializationScripts(duplicatedCanvas);
        
        // Haal alle transforms op van beide canvassen
        Transform[] originalTransforms = originalCanvas.GetComponentsInChildren<Transform>(true);
        Transform[] duplicatedTransforms = duplicatedCanvas.GetComponentsInChildren<Transform>(true);
        
        // Maak een dictionary voor snelle lookup van duplicated objects
        System.Collections.Generic.Dictionary<string, Transform> duplicatedLookup = 
            new System.Collections.Generic.Dictionary<string, Transform>();
        
        foreach (Transform dupTransform in duplicatedTransforms)
        {
            string path = GetTransformPath(dupTransform, duplicatedCanvas.transform);
            if (!duplicatedLookup.ContainsKey(path))
            {
                duplicatedLookup.Add(path, dupTransform);
            }
        }
        
        // Kopieer de staat van elk GameObject
        foreach (Transform originalTransform in originalTransforms)
        {
            string path = GetTransformPath(originalTransform, originalCanvas.transform);
            
            if (duplicatedLookup.TryGetValue(path, out Transform duplicatedTransform))
            {
                CopyGameObjectState(originalTransform.gameObject, duplicatedTransform.gameObject);
            }
        }
        
        // Forceer de juiste pagina state na een korte delay om auto-initialization te overschrijven
        StartCoroutine(ForceCorrectPageStateAfterDelay(originalCanvas, duplicatedCanvas));
        
        Debug.Log("[CanvasDuplicator] Canvas staat succesvol gekopieerd");
    }
    
    /// <summary>
    /// Schakelt scripts uit die automatisch de menu-pagina activeren
    /// </summary>
    private void DisableAutoInitializationScripts(GameObject duplicatedCanvas)
    {
        // Zoek naar UIMenuController scripts die mogelijk de state overschrijven
        UIMenuController[] menuControllers = duplicatedCanvas.GetComponentsInChildren<UIMenuController>();
        foreach (UIMenuController controller in menuControllers)
        {
            // Tijdelijk uitschakelen om auto-initialization te voorkomen
            controller.enabled = false;
        }
        
        // Zoek naar MenuStateManager scripts
        MenuStateManager[] stateManagers = duplicatedCanvas.GetComponentsInChildren<MenuStateManager>();
        foreach (MenuStateManager stateManager in stateManagers)
        {
            stateManager.enabled = false;
        }
        
        // Re-enable na een korte delay
        StartCoroutine(ReEnableScriptsAfterDelay(menuControllers, stateManagers));
    }
    
    /// <summary>
    /// Re-enabled scripts na een korte delay
    /// </summary>
    private System.Collections.IEnumerator ReEnableScriptsAfterDelay(UIMenuController[] menuControllers, MenuStateManager[] stateManagers)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame(); // Extra frame voor zekerheid
        
        // Re-enable UIMenuController scripts  
        foreach (UIMenuController controller in menuControllers)
        {
            if (controller != null)
                controller.enabled = true;
        }
        
        // Re-enable MenuStateManager scripts
        foreach (MenuStateManager stateManager in stateManagers)
        {
            if (stateManager != null)
                stateManager.enabled = true;
        }
    }
    
    /// <summary>
    /// Forceert de juiste pagina staat na een korte delay
    /// </summary>
    private System.Collections.IEnumerator ForceCorrectPageStateAfterDelay(GameObject originalCanvas, GameObject duplicatedCanvas)
    {
        // Wacht een paar frames zodat alle Start() methods zijn uitgevoerd
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f);
        
        Debug.Log("[CanvasDuplicator] Pagina staat forceren na delay...");
        
        // Zoek naar alle pagina GameObjects in het originele canvas
        Transform[] originalTransforms = originalCanvas.GetComponentsInChildren<Transform>(true);
        Transform[] duplicatedTransforms = duplicatedCanvas.GetComponentsInChildren<Transform>(true);
        
        // Maak een dictionary voor snelle lookup
        System.Collections.Generic.Dictionary<string, Transform> duplicatedLookup = 
            new System.Collections.Generic.Dictionary<string, Transform>();
        
        foreach (Transform dupTransform in duplicatedTransforms)
        {
            string path = GetTransformPath(dupTransform, duplicatedCanvas.transform);
            if (!duplicatedLookup.ContainsKey(path))
            {
                duplicatedLookup.Add(path, dupTransform);
            }
        }
        
        // Forceer alle GameObject active states opnieuw
        foreach (Transform originalTransform in originalTransforms)
        {
            string path = GetTransformPath(originalTransform, originalCanvas.transform);
            
            if (duplicatedLookup.TryGetValue(path, out Transform duplicatedTransform))
            {
                // Forceer de active state opnieuw, vooral voor pagina's
                if (originalTransform.gameObject.activeSelf != duplicatedTransform.gameObject.activeSelf)
                {
                    duplicatedTransform.gameObject.SetActive(originalTransform.gameObject.activeSelf);
                    
                    // Log belangrijke wijzigingen
                    if (originalTransform.name.ToLower().Contains("menu") || 
                        originalTransform.name.ToLower().Contains("page") ||
                        originalTransform.name.ToLower().Contains("panel"))
                    {
                        Debug.Log($"[CanvasDuplicator] Pagina state geforceerd: {originalTransform.name} = {originalTransform.gameObject.activeSelf}");
                    }
                }
            }
        }
        
        // Forceer de pin button icoon wissel opnieuw (na alle andere scripts)
        ForceIconChangeAfterScripts(duplicatedCanvas);
        
        Debug.Log("[CanvasDuplicator] Pagina staat succesvol geforceerd");
    }
    
    /// <summary>
    /// Forceert de icoon wissel na alle andere scripts
    /// </summary>
    private void ForceIconChangeAfterScripts(GameObject duplicatedCanvas)
    {
        Debug.Log("[CanvasDuplicator] Icoon wissel forceren na alle scripts...");
        
        // Zoek naar alle Btn_Pin objecten in het gedupliceerde canvas
        Transform[] allTransforms = duplicatedCanvas.GetComponentsInChildren<Transform>(true);
        
        foreach (Transform child in allTransforms)
        {
            if (child.name == "Btn_Pin")
            {
                // Zoek naar Open en Close GameObjects
                Transform openObj = FindChildRecursive(child, "Open");
                Transform closeObj = FindChildRecursive(child, "Close");
                
                if (openObj != null && closeObj != null)
                {
                    // Forceer de icoon wissel
                    openObj.gameObject.SetActive(false);
                    closeObj.gameObject.SetActive(true);
                    
                    Debug.Log($"[CanvasDuplicator] Icoon wissel geforceerd na scripts - Open: {openObj.gameObject.activeSelf}, Close: {closeObj.gameObject.activeSelf}");
                    
                    // Zet ook eventuele animators op de juiste staat
                    ForceAnimatorState(child, "Close");
                }
                
                break;
            }
        }
    }
    
    /// <summary>
    /// Forceert animator parameters voor icoon staat
    /// </summary>
    private void ForceAnimatorState(Transform buttonTransform, string targetState)
    {
        Animator animator = buttonTransform.GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            // Probeer common animator parameter namen
            string[] possibleParams = { "IsOpen", "State", "ButtonState", "IconState", "Open", "Close", "Active" };
            
            foreach (string paramName in possibleParams)
            {
                try
                {
                    // Voor bool parameters
                    AnimatorControllerParameter param = System.Array.Find(animator.parameters, p => p.name == paramName);
                    if (param != null)
                    {
                        if (param.type == AnimatorControllerParameterType.Bool)
                        {
                            bool value = targetState == "Close" ? false : true; // Close = false, Open = true
                            animator.SetBool(paramName, value);
                            Debug.Log($"[CanvasDuplicator] Animator bool parameter ingesteld: {paramName} = {value}");
                        }
                        else if (param.type == AnimatorControllerParameterType.Int)
                        {
                            int value = targetState == "Close" ? 1 : 0;
                            animator.SetInteger(paramName, value);
                            Debug.Log($"[CanvasDuplicator] Animator int parameter ingesteld: {paramName} = {value}");
                        }
                        else if (param.type == AnimatorControllerParameterType.Trigger)
                        {
                            if ((targetState == "Close" && paramName.ToLower().Contains("close")) ||
                                (targetState == "Open" && paramName.ToLower().Contains("open")))
                            {
                                animator.SetTrigger(paramName);
                                Debug.Log($"[CanvasDuplicator] Animator trigger ingesteld: {paramName}");
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[CanvasDuplicator] Fout bij instellen animator parameter {paramName}: {ex.Message}");
                }
            }
        }
    }
    
    /// <summary>
    /// Kopieert de staat van een specifiek GameObject
    /// </summary>
    private void CopyGameObjectState(GameObject original, GameObject duplicate)
    {
        // Kopieer active state
        if (original.activeSelf != duplicate.activeSelf)
        {
            duplicate.SetActive(original.activeSelf);
        }
        
        // Kopieer Button states (geselecteerd, interactable, etc.)
        CopyButtonState(original, duplicate);
        
        // Kopieer Toggle states
        CopyToggleState(original, duplicate);
        
        // Kopieer Slider states
        CopySliderState(original, duplicate);
        
        // Kopieer InputField states
        CopyInputFieldState(original, duplicate);
        
        // Kopieer Dropdown states
        CopyDropdownState(original, duplicate);
        
        // Kopieer ScrollRect states
        CopyScrollRectState(original, duplicate);
        
        // Kopieer Animator states
        CopyAnimatorState(original, duplicate);
    }
    
    /// <summary>
    /// Genereert een uniek pad voor een Transform relatief tot een root
    /// </summary>
    private string GetTransformPath(Transform target, Transform root)
    {
        if (target == root) return "";
        
        System.Collections.Generic.List<string> path = new System.Collections.Generic.List<string>();
        Transform current = target;
        
        while (current != null && current != root)
        {
            path.Insert(0, current.name);
            current = current.parent;
        }
        
        return string.Join("/", path.ToArray());
    }
    
    /// <summary>
    /// Kopieert Button component staat
    /// </summary>
    private void CopyButtonState(GameObject original, GameObject duplicate)
    {
        Button originalButton = original.GetComponent<Button>();
        Button duplicateButton = duplicate.GetComponent<Button>();
        
        if (originalButton != null && duplicateButton != null)
        {
            duplicateButton.interactable = originalButton.interactable;
            
            // Kopieer navigation settings
            Navigation originalNav = originalButton.navigation;
            Navigation duplicateNav = duplicateButton.navigation;
            duplicateNav.mode = originalNav.mode;
            duplicateButton.navigation = duplicateNav;
        }
    }
    
    /// <summary>
    /// Kopieert Toggle component staat
    /// </summary>
    private void CopyToggleState(GameObject original, GameObject duplicate)
    {
        UnityEngine.UI.Toggle originalToggle = original.GetComponent<UnityEngine.UI.Toggle>();
        UnityEngine.UI.Toggle duplicateToggle = duplicate.GetComponent<UnityEngine.UI.Toggle>();
        
        if (originalToggle != null && duplicateToggle != null)
        {
            duplicateToggle.isOn = originalToggle.isOn;
            duplicateToggle.interactable = originalToggle.interactable;
        }
    }
    
    /// <summary>
    /// Kopieert Slider component staat
    /// </summary>
    private void CopySliderState(GameObject original, GameObject duplicate)
    {
        UnityEngine.UI.Slider originalSlider = original.GetComponent<UnityEngine.UI.Slider>();
        UnityEngine.UI.Slider duplicateSlider = duplicate.GetComponent<UnityEngine.UI.Slider>();
        
        if (originalSlider != null && duplicateSlider != null)
        {
            duplicateSlider.value = originalSlider.value;
            duplicateSlider.interactable = originalSlider.interactable;
        }
    }
    
    /// <summary>
    /// Kopieert InputField component staat
    /// </summary>
    private void CopyInputFieldState(GameObject original, GameObject duplicate)
    {
        TMPro.TMP_InputField originalInput = original.GetComponent<TMPro.TMP_InputField>();
        TMPro.TMP_InputField duplicateInput = duplicate.GetComponent<TMPro.TMP_InputField>();
        
        if (originalInput != null && duplicateInput != null)
        {
            duplicateInput.text = originalInput.text;
            duplicateInput.interactable = originalInput.interactable;
        }
        
        // Legacy InputField support
        UnityEngine.UI.InputField originalLegacyInput = original.GetComponent<UnityEngine.UI.InputField>();
        UnityEngine.UI.InputField duplicateLegacyInput = duplicate.GetComponent<UnityEngine.UI.InputField>();
        
        if (originalLegacyInput != null && duplicateLegacyInput != null)
        {
            duplicateLegacyInput.text = originalLegacyInput.text;
            duplicateLegacyInput.interactable = originalLegacyInput.interactable;
        }
    }
    
    /// <summary>
    /// Kopieert Dropdown component staat
    /// </summary>
    private void CopyDropdownState(GameObject original, GameObject duplicate)
    {
        TMPro.TMP_Dropdown originalDropdown = original.GetComponent<TMPro.TMP_Dropdown>();
        TMPro.TMP_Dropdown duplicateDropdown = duplicate.GetComponent<TMPro.TMP_Dropdown>();
        
        if (originalDropdown != null && duplicateDropdown != null)
        {
            duplicateDropdown.value = originalDropdown.value;
            duplicateDropdown.interactable = originalDropdown.interactable;
        }
        
        // Legacy Dropdown support
        UnityEngine.UI.Dropdown originalLegacyDropdown = original.GetComponent<UnityEngine.UI.Dropdown>();
        UnityEngine.UI.Dropdown duplicateLegacyDropdown = duplicate.GetComponent<UnityEngine.UI.Dropdown>();
        
        if (originalLegacyDropdown != null && duplicateLegacyDropdown != null)
        {
            duplicateLegacyDropdown.value = originalLegacyDropdown.value;
            duplicateLegacyDropdown.interactable = originalLegacyDropdown.interactable;
        }
    }
    
    /// <summary>
    /// Kopieert ScrollRect component staat
    /// </summary>
    private void CopyScrollRectState(GameObject original, GameObject duplicate)
    {
        UnityEngine.UI.ScrollRect originalScroll = original.GetComponent<UnityEngine.UI.ScrollRect>();
        UnityEngine.UI.ScrollRect duplicateScroll = duplicate.GetComponent<UnityEngine.UI.ScrollRect>();
        
        if (originalScroll != null && duplicateScroll != null)
        {
            duplicateScroll.normalizedPosition = originalScroll.normalizedPosition;
            duplicateScroll.velocity = originalScroll.velocity;
        }
    }
    
    /// <summary>
    /// Kopieert Animator component staat
    /// </summary>
    private void CopyAnimatorState(GameObject original, GameObject duplicate)
    {
        Animator originalAnimator = original.GetComponent<Animator>();
        Animator duplicateAnimator = duplicate.GetComponent<Animator>();
        
        if (originalAnimator != null && duplicateAnimator != null && originalAnimator.runtimeAnimatorController != null)
        {
            // Kopieer de huidige animator state
            AnimatorStateInfo currentState = originalAnimator.GetCurrentAnimatorStateInfo(0);
            
            // Probeer dezelfde state te activeren in de kopie
            if (duplicateAnimator.runtimeAnimatorController != null)
            {
                duplicateAnimator.Play(currentState.fullPathHash, 0, currentState.normalizedTime);
                
                // Kopieer parameters
                foreach (AnimatorControllerParameter param in originalAnimator.parameters)
                {
                    switch (param.type)
                    {
                        case AnimatorControllerParameterType.Bool:
                            duplicateAnimator.SetBool(param.name, originalAnimator.GetBool(param.name));
                            break;
                        case AnimatorControllerParameterType.Float:
                            duplicateAnimator.SetFloat(param.name, originalAnimator.GetFloat(param.name));
                            break;
                        case AnimatorControllerParameterType.Int:
                            duplicateAnimator.SetInteger(param.name, originalAnimator.GetInteger(param.name));
                            break;
                        case AnimatorControllerParameterType.Trigger:
                            // Triggers worden niet gekopieerd omdat ze eenmalig zijn
                            break;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Verandert de pin button in het gedupliceerde canvas naar een close button
    /// </summary>
    private void ConvertPinButtonToCloseButton(GameObject duplicatedCanvas)
    {
        // Zoek naar alle Btn_Pin objecten in het gedupliceerde canvas
        Transform[] allTransforms = duplicatedCanvas.GetComponentsInChildren<Transform>(true);
        
        foreach (Transform child in allTransforms)
        {
            if (child.name == "Btn_Pin")
            {
                Debug.Log($"[CanvasDuplicator] Pin button gevonden in kopie: {child.name}");
                
                // Zoek naar Open en Close GameObjects binnen de button (ook in subchildren)
                Transform openObj = FindChildRecursive(child, "Open");
                Transform closeObj = FindChildRecursive(child, "Close");
                
                Debug.Log($"[CanvasDuplicator] Open object gevonden: {openObj != null}, Close object gevonden: {closeObj != null}");
                
                if (openObj != null && closeObj != null)
                {
                    // Schakel Open uit en Close aan
                    openObj.gameObject.SetActive(false);
                    closeObj.gameObject.SetActive(true);
                    
                    Debug.Log($"[CanvasDuplicator] Icoon gewisseld - Open: {openObj.gameObject.activeSelf}, Close: {closeObj.gameObject.activeSelf}");
                }
                else
                {
                    Debug.LogWarning($"[CanvasDuplicator] Open of Close GameObject niet gevonden in {child.name}");
                    
                    // Log alle children voor debugging
                    Debug.Log("[CanvasDuplicator] Beschikbare children:");
                    Transform[] children = child.GetComponentsInChildren<Transform>(true);
                    foreach (Transform c in children)
                    {
                        if (c != child) // Skip de parent zelf
                        {
                            Debug.Log($"  - {c.name} (actief: {c.gameObject.activeSelf})");
                        }
                    }
                }
                
                // Voeg PanelCloser script toe en verwijder CanvasDuplicator
                Button pinButton = child.GetComponent<Button>();
                if (pinButton != null)
                {
                    // Verwijder de bestaande CanvasDuplicator (als die er is)
                    CanvasDuplicator existingDuplicator = child.GetComponent<CanvasDuplicator>();
                    if (existingDuplicator != null)
                    {
                        // Verwijder eerst de listener
                        pinButton.onClick.RemoveListener(existingDuplicator.DuplicateCanvas);
                        DestroyImmediate(existingDuplicator);
                    }
                    
                    // Voeg PanelCloser script toe
                    PanelCloser panelCloser = child.gameObject.AddComponent<PanelCloser>();
                    panelCloser.SetPanelToClose(duplicatedCanvas);
                    
                    Debug.Log("[CanvasDuplicator] PanelCloser script toegevoegd aan pin button in kopie");
                }
                
                break; // We hebben de pin button gevonden en aangepast
            }
        }
    }
    
    /// <summary>
    /// Zoekt recursief naar een child met een specifieke naam
    /// </summary>
    private Transform FindChildRecursive(Transform parent, string childName)
    {
        // Zoek eerst in directe children
        Transform directChild = parent.Find(childName);
        if (directChild != null)
        {
            return directChild;
        }
        
        // Zoek recursief in alle children
        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, childName);
            if (result != null)
            {
                return result;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Methode om handmatig een Canvas in te stellen (kan vanuit andere scripts worden aangeroepen)
    /// </summary>
    public void SetCanvasToDuplicate(Canvas canvas)
    {
        canvasToDuplicate = canvas;
        Debug.Log($"[CanvasDuplicator] Canvas ingesteld voor duplicatie: {canvas.name}");
    }
    
    /// <summary>
    /// Reset de duplicate counter (nuttig voor testing)
    /// </summary>
    public static void ResetDuplicateCounter()
    {
        duplicateCounter = 1;
    }
    
    private void OnDestroy()
    {
        // Ontkoppel de button listener wanneer het object wordt vernietigd
        if (pinButton != null)
        {
            pinButton.onClick.RemoveListener(DuplicateCanvas);
        }
    }
} 