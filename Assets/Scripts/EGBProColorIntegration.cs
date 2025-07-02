using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EGBProColorIntegration : MonoBehaviour
{
    [Header("Material Management")]
    public Material[] availableMaterials;
    public bool createMaterialCopies = true;
    
    [Header("Color Picker Type")]
    [SerializeField] private ColorPickerType colorPickerType = ColorPickerType.Auto;
    
    public enum ColorPickerType
    {
        Auto,                // Automatisch detecteren
        HSVColorPicker,      // Volledige HSV picker
        SimpleColorPicker,   // Eenvoudige picker
        SpectrumColorPicker  // Nieuwe spectrum picker met hue strip
    }
    
    // Undo system - no Header needed for private fields
    private Stack<MaterialState> undoStack = new Stack<MaterialState>();
    private Stack<MaterialState> redoStack = new Stack<MaterialState>();
    private const int maxUndoSteps = 50;
    
    [System.Serializable]
    public class MaterialState
    {
        public GameObject targetObject;
        public Material originalMaterial;
        public Material modifiedMaterial;
        public Color originalColor;
        public Color newColor;
    }
    
    // Color picker references
    private HSVColorPicker hsvColorPicker;
    private SimpleColorPicker simpleColorPicker;
    private SpectrumColorPicker spectrumColorPicker;
    private GameObject lastSelectedObject;
    
    void Start()
    {
        InitializeColorPickers();
        SubscribeToEvents();
    }
    
    void InitializeColorPickers()
    {
        // Probeer alle drie componenten te vinden
        hsvColorPicker = GetComponent<HSVColorPicker>();
        simpleColorPicker = GetComponent<SimpleColorPicker>();
        spectrumColorPicker = GetComponent<SpectrumColorPicker>();
        
        // Als geen van de componenten gevonden, zoek in children
        if (hsvColorPicker == null)
            hsvColorPicker = GetComponentInChildren<HSVColorPicker>();
        if (simpleColorPicker == null)
            simpleColorPicker = GetComponentInChildren<SimpleColorPicker>();
        if (spectrumColorPicker == null)
            spectrumColorPicker = GetComponentInChildren<SpectrumColorPicker>();
        
        // Log welke picker(s) gevonden zijn
        string foundPickers = "";
        if (hsvColorPicker != null) foundPickers += "HSVColorPicker ";
        if (simpleColorPicker != null) foundPickers += "SimpleColorPicker ";
        if (spectrumColorPicker != null) foundPickers += "SpectrumColorPicker ";
        
        Debug.Log($"EGBProColorIntegration: Found {foundPickers}");
        
        if (hsvColorPicker == null && simpleColorPicker == null && spectrumColorPicker == null)
        {
            Debug.LogWarning("EGBProColorIntegration: Geen color picker componenten gevonden!");
        }
    }
    
    void SubscribeToEvents()
    {
        // Subscribe to color changes van alle pickers
        HSVColorPicker.OnColorChanged += OnColorChanged;
        SimpleColorPicker.OnColorChanged += OnColorChanged;
        SpectrumColorPicker.OnColorChanged += OnColorChanged;
        
        // Subscribe to EGB Pro selection changes (als beschikbaar)
        // EGBProSelector.OnSelectionChanged += OnObjectSelected;
    }
    
    void OnDestroy()
    {
        // Unsubscribe van events
        HSVColorPicker.OnColorChanged -= OnColorChanged;
        SimpleColorPicker.OnColorChanged -= OnColorChanged;
        SpectrumColorPicker.OnColorChanged -= OnColorChanged;
        // EGBProSelector.OnSelectionChanged -= OnObjectSelected;
    }
    
    void OnColorChanged(Color newColor)
    {
        Debug.Log($"Color changed to: {newColor}");
        ApplyColorToSelectedObject(newColor);
    }
    
    public void OnObjectSelected(GameObject selectedObject)
    {
        if (selectedObject != lastSelectedObject)
        {
            lastSelectedObject = selectedObject;
            Debug.Log($"Object selected: {selectedObject.name}");
            
            // Get current color from object and update color picker
            Color currentColor = GetObjectColor(selectedObject);
            SetColorPickerColor(currentColor);
        }
    }
    
    void SetColorPickerColor(Color color)
    {
        // Update de actieve color picker (prioriteit: Spectrum > HSV > Simple)
        if (spectrumColorPicker != null)
        {
            spectrumColorPicker.SetColor(color);
        }
        else if (hsvColorPicker != null)
        {
            hsvColorPicker.SetColor(color);
        }
        else if (simpleColorPicker != null)
        {
            simpleColorPicker.SetColor(color);
        }
    }
    
    public Color GetCurrentPickerColor()
    {
        // Get color van de actieve picker (prioriteit: Spectrum > HSV > Simple)
        if (spectrumColorPicker != null)
        {
            return spectrumColorPicker.GetCurrentColor();
        }
        else if (hsvColorPicker != null)
        {
            return hsvColorPicker.GetCurrentColor();
        }
        else if (simpleColorPicker != null)
        {
            return simpleColorPicker.GetCurrentColor();
        }
        return Color.white;
    }
    
    void ApplyColorToSelectedObject(Color color)
    {
        GameObject selectedObject = GetSelectedObject();
        if (selectedObject == null) 
        {
            Debug.Log("No object selected for color application");
            return;
        }
        
        Renderer renderer = selectedObject.GetComponent<Renderer>();
        if (renderer == null) 
        {
            Debug.Log($"No renderer found on {selectedObject.name}");
            return;
        }
        
        // Store undo state
        StoreUndoState(selectedObject, renderer.material, color);
        
        // Apply new color
        Material targetMaterial = GetOrCreateMaterial(renderer);
        SetMaterialColor(targetMaterial, color);
        renderer.material = targetMaterial;
        
        Debug.Log($"Applied color {color} to {selectedObject.name}");
    }
    
    GameObject GetSelectedObject()
    {
        // Integratie met EGB Pro selector systeem
        // Voor nu returnen we het laatst geselecteerde object
        return lastSelectedObject;
    }
    
    Color GetObjectColor(GameObject obj)
    {
        if (obj == null) return Color.white;
        
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null || renderer.material == null) return Color.white;
        
        // Probeer verschillende color properties
        Material mat = renderer.material;
        if (mat.HasProperty("_Color"))
            return mat.color;
        if (mat.HasProperty("_BaseColor"))
            return mat.GetColor("_BaseColor");
        if (mat.HasProperty("_MainColor"))
            return mat.GetColor("_MainColor");
        
        return Color.white;
    }
    
    Material GetOrCreateMaterial(Renderer renderer)
    {
        if (createMaterialCopies)
        {
            // Maak een kopie van het materiaal
            Material originalMaterial = renderer.sharedMaterial;
            Material newMaterial = new Material(originalMaterial);
            newMaterial.name = originalMaterial.name + " (Copy)";
            return newMaterial;
        }
        else
        {
            return renderer.material;
        }
    }
    
    void SetMaterialColor(Material material, Color color)
    {
        // Probeer verschillende color properties in volgorde van prioriteit
        if (material.HasProperty("_Color"))
            material.color = color;
        else if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        else if (material.HasProperty("_MainColor"))
            material.SetColor("_MainColor", color);
        else if (material.HasProperty("_TintColor"))
            material.SetColor("_TintColor", color);
    }
    
    void StoreUndoState(GameObject targetObject, Material originalMaterial, Color newColor)
    {
        MaterialState state = new MaterialState
        {
            targetObject = targetObject,
            originalMaterial = originalMaterial,
            originalColor = GetObjectColor(targetObject),
            newColor = newColor
        };
        
        undoStack.Push(state);
        
        // Limit undo stack size
        if (undoStack.Count > maxUndoSteps)
        {
            Stack<MaterialState> tempStack = new Stack<MaterialState>();
            for (int i = 0; i < maxUndoSteps - 1; i++)
            {
                tempStack.Push(undoStack.Pop());
            }
            undoStack.Clear();
            while (tempStack.Count > 0)
            {
                undoStack.Push(tempStack.Pop());
            }
        }
        
        // Clear redo stack
        redoStack.Clear();
    }
    
    public void Undo()
    {
        if (undoStack.Count > 0)
        {
            MaterialState state = undoStack.Pop();
            redoStack.Push(state);
            
            if (state.targetObject != null)
            {
                Renderer renderer = state.targetObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    SetMaterialColor(renderer.material, state.originalColor);
                }
            }
        }
    }
    
    public void Redo()
    {
        if (redoStack.Count > 0)
        {
            MaterialState state = redoStack.Pop();
            undoStack.Push(state);
            
            if (state.targetObject != null)
            {
                Renderer renderer = state.targetObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    SetMaterialColor(renderer.material, state.newColor);
                }
            }
        }
    }
    
    // Voor manual selectie (tijdelijke functie voor testing)
    public void SelectObjectByName(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            OnObjectSelected(obj);
        }
    }
    
    // Test functies
    [ContextMenu("Test Color Picker")]
    public void TestColorPicker()
    {
        Color testColor = Color.green;
        SetColorPickerColor(testColor);
        Debug.Log($"Set color picker to: {testColor}");
    }
    
    [ContextMenu("Get Current Color")]
    public void LogCurrentColor()
    {
        Color currentColor = GetCurrentPickerColor();
        Debug.Log($"Current picker color: {currentColor}");
    }
    
    // Quick color buttons (kunnen aangeroepen worden vanuit UI buttons)
    public void SetQuickColorRed() { SetColorPickerColor(Color.red); }
    public void SetQuickColorGreen() { SetColorPickerColor(Color.green); }
    public void SetQuickColorBlue() { SetColorPickerColor(Color.blue); }
    public void SetQuickColorYellow() { SetColorPickerColor(Color.yellow); }
    public void SetQuickColorCyan() { SetColorPickerColor(Color.cyan); }
    public void SetQuickColorMagenta() { SetColorPickerColor(Color.magenta); }
    public void SetQuickColorWhite() { SetColorPickerColor(Color.white); }
    public void SetQuickColorBlack() { SetColorPickerColor(Color.black); }
    
    // EGB Pro integration points
    public void ConnectToEGBPro()
    {
        // Hier kun je de integratie met EGB Pro implementeren
        // Bijvoorbeeld door te luisteren naar EGB Pro events
        Debug.Log("Connecting to EGB Pro systems...");
    }
}
