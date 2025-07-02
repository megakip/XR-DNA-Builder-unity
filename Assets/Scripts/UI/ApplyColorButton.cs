using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Script voor een knop die de geselecteerde kleur toepast op het aangewezen object.
/// Plaats dit script op een UI Button in de scene.
/// </summary>
public class ApplyColorButton : MonoBehaviour
{
    [Header("Color Settings")]
    [Tooltip("De kleur die moet worden toegepast")]
    public Color selectedColor = Color.white;
    
    [Header("References")]
    [Tooltip("Referentie naar de XR Ray Interactor of Near-Far Interactor voor object detectie")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor;
    
    [Tooltip("Referentie naar de DirectColorChanger")]
    public DirectColorChanger directColorChanger;
    
    [Tooltip("Optionele tekst component om feedback te tonen aan de gebruiker")]
    public Text feedbackText;
    
    [Header("Settings")]
    [Tooltip("Maximale afstand voor het detecteren van objecten")]
    public float maxDistance = 10f;
    
    [Tooltip("Layer mask voor objecten die een kleur kunnen krijgen")]
    public LayerMask colorableObjectsLayerMask = ~0; // Standaard alle layers
    
    [Tooltip("Toon debug informatie in de console")]
    public bool showDebugInfo = true;
    
    [Tooltip("Naam van de Near-Far Interactor om te zoeken")]
    public string nearFarInteractorName = "Near-Far Interactor";
    
    private Button button;
    private Color currentColor = Color.white;
    private LineRenderer lineRenderer;
    
    private void Awake()
    {
        // Krijg de Button component
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("Geen Button component gevonden op dit GameObject!");
            enabled = false;
            return;
        }
        
        // Voeg een listener toe aan de button
        button.onClick.AddListener(ApplyColorToTargetObject);
        
        // Zoek automatisch de DirectColorChanger als deze niet is ingesteld
        if (directColorChanger == null)
        {
            directColorChanger = FindObjectOfType<DirectColorChanger>();
            if (directColorChanger != null && showDebugInfo)
            {
                Debug.Log("DirectColorChanger automatisch gevonden");
            }
        }
        
        // Zoek automatisch de interactor als deze niet is ingesteld
        if (interactor == null && directColorChanger != null)
        {
            interactor = directColorChanger.rayInteractor;
            if (interactor != null && showDebugInfo)
            {
                Debug.Log("Interactor automatisch gevonden via DirectColorChanger");
            }
        }
        
        if (interactor == null)
        {
            FindInteractor();
        }
        
        if (interactor == null)
        {
            Debug.LogWarning("Geen interactor gevonden, objecten kunnen niet worden gedetecteerd!");
        }
        else
        {
            // Zoek de LineRenderer die mogelijk bij deze interactor hoort
            lineRenderer = interactor.GetComponentInChildren<LineRenderer>();
            if (lineRenderer != null && showDebugInfo)
            {
                Debug.Log($"LineRenderer gevonden bij interactor: {interactor.name}");
            }
        }
        
        // Zet de huidige kleur op de geselecteerde kleur
        currentColor = selectedColor;
    }
    
    /// <summary>
    /// Zoekt naar beschikbare interactors in de scene
    /// </summary>
    private void FindInteractor()
    {
        // Probeer eerst een Near-Far Interactor te vinden op naam
        var allInteractors = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>();
        
        if (showDebugInfo)
            Debug.Log($"Gevonden interactors: {allInteractors.Length}");
        
        foreach (var foundInteractor in allInteractors)
        {
            if (showDebugInfo)
                Debug.Log($"Gevonden interactor: {foundInteractor.name}, Type: {foundInteractor.GetType().Name}");
                
            // Specifiek zoeken naar Near-Far interactor op naam
            if (foundInteractor.name.Contains(nearFarInteractorName))
            {
                interactor = foundInteractor;
                if (showDebugInfo)
                    Debug.Log($"Near-Far Interactor gevonden: {foundInteractor.name}");
                return;
            }
        }
        
        // Als alternatief, zoek naar een interactor onder een controller object
        var controllerObjects = GameObject.FindGameObjectsWithTag("MainCamera");
        foreach (var controller in controllerObjects)
        {
            var foundInteractors = controller.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>();
            if (foundInteractors.Length > 0)
            {
                interactor = foundInteractors[0];
                if (showDebugInfo)
                    Debug.Log($"Interactor gevonden via controller: {interactor.name}");
                return;
            }
        }
        
        // Als we hem nog steeds niet hebben gevonden, gebruik dan de eerste interactor in de scene
        if (allInteractors.Length > 0)
        {
            interactor = allInteractors[0];
            if (showDebugInfo)
                Debug.Log($"Eerste beschikbare interactor gebruikt: {interactor.name}");
        }
    }
    
    private void OnDestroy()
    {
        // Verwijder de listener als het script wordt vernietigd
        if (button != null)
        {
            button.onClick.RemoveListener(ApplyColorToTargetObject);
        }
    }
    
    private void Update()
    {
        // Gebruik de ingestelde kleur
        currentColor = selectedColor;
    }
    
    /// <summary>
    /// Stelt de kleur in die moet worden toegepast
    /// </summary>
    public void SetColor(Color color)
    {
        selectedColor = color;
        currentColor = color;
    }
    
    /// <summary>
    /// Past de huidige kleur toe op het object waar de interactor naar wijst
    /// </summary>
    public void ApplyColorToTargetObject()
    {
        if (interactor == null)
        {
            ShowFeedback("Geen interactor gevonden!");
            return;
        }
        
        // Als we een DirectColorChanger hebben, laat die het werk doen
        if (directColorChanger != null)
        {
            // Haal het doelwit op
            GameObject targetObject = GetTargetObject();
            if (targetObject != null)
            {
                // Gebruik de DirectColorChanger om het object te selecteren en de kleur toe te passen
                directColorChanger.SelectObjectForColoring(targetObject);
                directColorChanger.SetColor(currentColor);
                directColorChanger.ApplyAndClose();
                
                ShowFeedback($"Kleur toegepast op {targetObject.name} via DirectColorChanger");
                return;
            }
            else
            {
                ShowFeedback("Geen object gedetecteerd");
                return;
            }
        }
        
        // Fallback methode als er geen DirectColorChanger is
        GameObject fallbackTargetObject = GetTargetObject();
        if (fallbackTargetObject != null)
        {
            // Zoek een renderer component op het object of zijn children
            Renderer renderer = fallbackTargetObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                // Probeer renderers te vinden op child objecten
                Renderer[] childRenderers = fallbackTargetObject.GetComponentsInChildren<Renderer>();
                if (childRenderers.Length > 0)
                {
                    // Pas de kleur toe op alle child renderers
                    foreach (var childRenderer in childRenderers)
                    {
                        ApplyColorToRenderer(childRenderer);
                    }
                    
                    ShowFeedback($"Kleur toegepast op {fallbackTargetObject.name} (via child renderers)");
                    return;
                }
            }
            else
            {
                // Pas de kleur toe op de renderer
                ApplyColorToRenderer(renderer);
                ShowFeedback($"Kleur toegepast op {fallbackTargetObject.name}");
                return;
            }
            
            ShowFeedback($"Geen renderers gevonden op {fallbackTargetObject.name}");
        }
        else
        {
            ShowFeedback("Geen object gedetecteerd");
        }
    }
    
    /// <summary>
    /// Past de huidige kleur toe op een renderer
    /// </summary>
    private void ApplyColorToRenderer(Renderer renderer)
    {
        if (renderer != null)
        {
            // Maak een nieuwe instantie van het materiaal om te voorkomen dat andere objecten ook worden beïnvloed
            Material material = renderer.material;
            material.color = currentColor;
            
            if (showDebugInfo)
                Debug.Log($"Kleur {ColorToHex(currentColor)} toegepast op {renderer.gameObject.name}");
        }
    }
    
    /// <summary>
    /// Haalt het object op waar de interactor naar wijst
    /// </summary>
    private GameObject GetTargetObject()
    {
        // Controleer of we een interactor hebben
        if (interactor == null)
            return null;
            
        // Eerst proberen we de huidige selectie van de interactor te krijgen
        UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable selectInteractable = null;
        if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor directInteractor)
        {
            if (directInteractor.hasSelection && directInteractor.interactablesSelected.Count > 0)
            {
                selectInteractable = directInteractor.interactablesSelected[0];
                if (showDebugInfo)
                    Debug.Log($"Object geselecteerd via directe interactor: {selectInteractable}");
            }
        }
        else if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor)
        {
            // Als het een ray interactor is, probeer een raycast
            if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                if (showDebugInfo)
                    Debug.Log($"Object gedetecteerd via Ray Interactor: {hit.collider.gameObject.name}");
                
                return hit.collider.gameObject;
            }
        }
        
        // Als we de interactable hebben gevonden, haal het GameObject op
        if (selectInteractable != null)
        {
            var interactableObject = selectInteractable as MonoBehaviour;
            if (interactableObject != null)
                return interactableObject.gameObject;
        }
        
        // Als we nog steeds niets hebben gevonden, doe een raycast vanuit de interactor
        if (lineRenderer != null)
        {
            // Gebruik de richting van de LineRenderer
            Vector3 origin = lineRenderer.transform.position;
            Vector3 direction = lineRenderer.transform.forward;
            
            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit, maxDistance, colorableObjectsLayerMask))
            {
                if (showDebugInfo)
                    Debug.Log($"Object gedetecteerd via LineRenderer raycast: {hit.collider.gameObject.name}");
                
                return hit.collider.gameObject;
            }
        }
        else
        {
            // Fallback: doe een raycast vanuit de interactor transform
            RaycastHit hit;
            if (Physics.Raycast(interactor.transform.position, interactor.transform.forward, out hit, maxDistance, colorableObjectsLayerMask))
            {
                if (showDebugInfo)
                    Debug.Log($"Object gedetecteerd via fallback raycast: {hit.collider.gameObject.name}");
                
                return hit.collider.gameObject;
            }
        }
        
        // Als we hiermee nog niks vinden, kijk dan of er een object dicht bij de controller is
        Collider[] nearbyColliders = Physics.OverlapSphere(interactor.transform.position, 0.5f, colorableObjectsLayerMask);
        if (nearbyColliders.Length > 0)
        {
            if (showDebugInfo)
                Debug.Log($"Object gedetecteerd in nabijheid: {nearbyColliders[0].gameObject.name}");
            
            return nearbyColliders[0].gameObject;
        }
        
        return null;
    }
    
    /// <summary>
    /// Toont feedback aan de gebruiker
    /// </summary>
    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
        
        if (showDebugInfo)
        {
            Debug.Log(message);
        }
    }
    
    /// <summary>
    /// Converteert een Color naar een hex string
    /// </summary>
    private string ColorToHex(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
    }
}
