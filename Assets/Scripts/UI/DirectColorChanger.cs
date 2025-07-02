using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Direct Color Changer voor XR. Zeer eenvoudige implementatie zonder externe color picker dependency.
/// </summary>
public class DirectColorChanger : MonoBehaviour
{
    [Header("Kleur Instellingen")]
    [Tooltip("De huidige geselecteerde kleur")]
    public Color selectedColor = Color.white;
    
    [Header("Referenties")]
    [Tooltip("De Canvas met de color picker UI")]
    public Canvas colorPickerCanvas;
    
    [Tooltip("XR Ray Interactor voor objectselectie")]
    public XRRayInteractor rayInteractor;
    
    [Header("Instellingen")]
    [Tooltip("Layer mask voor objecten die gekleurd kunnen worden")]
    public LayerMask colorableObjectsLayerMask = -1; // Standaard alles
    
    [Tooltip("Toon debug info in console")]
    public bool showDebug = true;
    
    // Het geselecteerde object om te kleuren
    private GameObject selectedObject;
    private Color originalColor;
    private Color lastAppliedColor;
    
    // Tracking variables
    private int frameCounter = 0;
    private bool colorPickerActive = false;
    
    private void Start()
    {
        // Vind referenties als ze niet zijn ingesteld
        if (colorPickerCanvas == null)
        {
            // Zoek naar een Canvas in children
            colorPickerCanvas = GetComponentInChildren<Canvas>(true);
            if (colorPickerCanvas != null)
                Debug.Log("DirectColorChanger: Canvas auto-gevonden via children.");
        }
        
        if (rayInteractor == null)
        {
            rayInteractor = FindObjectOfType<XRRayInteractor>();
            if (rayInteractor != null)
                Debug.Log("DirectColorChanger: XRRayInteractor auto-gevonden in scene.");
        }
        
        // Initialize
        HideColorPicker();
        lastAppliedColor = selectedColor;
    }
    
    private void Update()
    {
        frameCounter++;
        
        // Controleer of we het color picker moeten openen
        if (!colorPickerActive && rayInteractor != null && rayInteractor.isSelectActive)
        {
            // Zoek object onder de pointer
            TrySelectObject();
        }
        
        // Als color picker actief is, update de kleur elke frame
        if (colorPickerActive && selectedObject != null)
        {
            // Als de kleur is veranderd sinds vorige keer
            if (selectedColor != lastAppliedColor || frameCounter % 10 == 0)  // Ook periodiek updaten
            {
                // Pas de kleur toe
                ApplyColorToObject(selectedObject, selectedColor);
                lastAppliedColor = selectedColor;
                
                if (showDebug)
                {
                    Debug.Log($"Kleur bijgewerkt: {ColorToHex(selectedColor)}");
                }
            }
        }
    }
    
    /// <summary>
    /// Probeer een object onder de pointer te selecteren
    /// </summary>
    private void TrySelectObject()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            GameObject hitObject = hit.collider.gameObject;
            
            // Check of het een renderer heeft
            Renderer renderer = GetObjectRenderer(hitObject);
            
            if (renderer != null)
            {
                // Selecteer dit object voor kleuren
                SelectObjectForColoring(hitObject);
            }
        }
    }
    
    /// <summary>
    /// Selecteer een object en toon de color picker
    /// </summary>
    public void SelectObjectForColoring(GameObject obj)
    {
        if (obj == null) return;
        
        selectedObject = obj;
        
        // Haal huidige kleur op
        Renderer renderer = GetObjectRenderer(obj);
        if (renderer != null && renderer.material != null)
        {
            originalColor = renderer.material.color;
            
            // Stel geselecteerde kleur in op huidige kleur
            selectedColor = originalColor;
            lastAppliedColor = originalColor;
            
            if (showDebug)
            {
                Debug.Log($"Object geselecteerd: {obj.name}, kleur: {ColorToHex(originalColor)}");
            }
            
            // Toon color picker
            ShowColorPicker();
        }
    }
    
    /// <summary>
    /// Stelt de huidige kleur in
    /// </summary>
    public void SetColor(Color color)
    {
        selectedColor = color;
        
        // Als we een geselecteerd object hebben, pas de kleur direct toe
        if (selectedObject != null)
        {
            ApplyColorToObject(selectedObject, color);
            lastAppliedColor = color;
        }
    }
    
    /// <summary>
    /// Vind de renderer van een object (direct of in children)
    /// </summary>
    private Renderer GetObjectRenderer(GameObject obj)
    {
        // Eerst proberen op hoofdobject
        Renderer renderer = obj.GetComponent<Renderer>();
        
        // Als niet gevonden, check children
        if (renderer == null)
        {
            Renderer[] childRenderers = obj.GetComponentsInChildren<Renderer>();
            if (childRenderers.Length > 0)
            {
                renderer = childRenderers[0];
            }
        }
        
        return renderer;
    }
    
    /// <summary>
    /// Pas een kleur toe op een object
    /// </summary>
    private void ApplyColorToObject(GameObject obj, Color color)
    {
        if (obj == null) return;
        
        // Check en pas toe op main renderer
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && renderer.enabled)
        {
            ApplyColorToRenderer(renderer, color);
        }
        
        // Check en pas toe op child renderers
        Renderer[] childRenderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer childRenderer in childRenderers)
        {
            if (childRenderer != renderer && childRenderer.enabled)
            {
                ApplyColorToRenderer(childRenderer, color);
            }
        }
    }
    
    /// <summary>
    /// Pas kleur toe op een specifieke renderer
    /// </summary>
    private void ApplyColorToRenderer(Renderer renderer, Color color)
    {
        if (renderer == null) return;
        
        // Hoofdmateriaal direct aanpassen
        renderer.material.color = color;
    }
    
    /// <summary>
    /// Toon de color picker
    /// </summary>
    public void ShowColorPicker()
    {
        if (colorPickerCanvas != null)
        {
            // Positioneer voor de camera
            Camera cam = Camera.main;
            if (cam != null && colorPickerCanvas.renderMode == RenderMode.WorldSpace)
            {
                // Plaats voor de gebruiker
                colorPickerCanvas.transform.position = cam.transform.position + cam.transform.forward * 0.5f;
                colorPickerCanvas.transform.rotation = Quaternion.LookRotation(colorPickerCanvas.transform.position - cam.transform.position);
            }
            
            // Activeer de canvas
            colorPickerCanvas.gameObject.SetActive(true);
            colorPickerActive = true;
            
            if (showDebug)
            {
                Debug.Log("Color picker geopend");
            }
        }
    }
    
    /// <summary>
    /// Verberg de color picker
    /// </summary>
    public void HideColorPicker()
    {
        if (colorPickerCanvas != null)
        {
            colorPickerCanvas.gameObject.SetActive(false);
            colorPickerActive = false;
            
            if (showDebug)
            {
                Debug.Log("Color picker gesloten");
            }
        }
    }
    
    /// <summary>
    /// Pas de geselecteerde kleur toe en sluit
    /// </summary>
    public void ApplyAndClose()
    {
        if (selectedObject != null)
        {
            ApplyColorToObject(selectedObject, selectedColor);
            
            if (showDebug)
            {
                Debug.Log($"Definitieve kleur toegepast: {ColorToHex(selectedColor)}");
            }
        }
        
        // Sluit de picker
        HideColorPicker();
        selectedObject = null;
    }
    
    /// <summary>
    /// Annuleer kleurverandering en sluit
    /// </summary>
    public void CancelAndClose()
    {
        if (selectedObject != null)
        {
            // Herstel originele kleur
            ApplyColorToObject(selectedObject, originalColor);
            
            if (showDebug)
            {
                Debug.Log($"Kleurverandering geannuleerd, originele kleur hersteld");
            }
        }
        
        // Sluit de picker
        HideColorPicker();
        selectedObject = null;
    }
    
    /// <summary>
    /// Convert Color naar hex string
    /// </summary>
    private string ColorToHex(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
    }
}
