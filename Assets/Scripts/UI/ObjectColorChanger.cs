using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Manages color selection and application to objects without external color picker dependency
/// </summary>
public class ObjectColorChanger : MonoBehaviour
{
    [Header("Color Settings")]
    [Tooltip("Currently selected color")]
    public Color selectedColor = Color.white;

    [Header("Object References")]
    [Tooltip("Objects that can have their colors changed")]
    public List<GameObject> targetObjects = new List<GameObject>();
    
    [Tooltip("Currently selected object to apply color to")]
    public GameObject currentSelectedObject;
    
    [Header("UI References")]
    [Tooltip("Button to activate/deactivate the color picker")]
    public Button colorPickerToggleButton;
    
    [Tooltip("Panel containing the color picker")]
    public GameObject colorPickerPanel;
    
    [Header("Color Preset Buttons")]
    [Tooltip("Buttons for quick color selection")]
    public Button[] colorPresetButtons;
    
    [Tooltip("Colors for the preset buttons")]
    public Color[] presetColors = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.magenta,
        Color.cyan,
        Color.white,
        Color.black
    };

    // Track if we're currently in color selection mode
    private bool isPickingColor = false;

    private void Start()
    {
        // Hide the picker initially
        if (colorPickerPanel != null)
            colorPickerPanel.SetActive(false);
        
        // Setup toggle button if assigned
        if (colorPickerToggleButton != null)
        {
            colorPickerToggleButton.onClick.AddListener(ToggleColorPicker);
        }
        
        // Setup preset color buttons
        SetupPresetButtons();
    }
    
    /// <summary>
    /// Setup the preset color buttons
    /// </summary>
    private void SetupPresetButtons()
    {
        if (colorPresetButtons != null)
        {
            for (int i = 0; i < colorPresetButtons.Length && i < presetColors.Length; i++)
            {
                int index = i; // Capture the index for the closure
                Color presetColor = presetColors[i];
                
                if (colorPresetButtons[i] != null)
                {
                    // Set button color if it has an Image component
                    Image buttonImage = colorPresetButtons[i].GetComponent<Image>();
                    if (buttonImage != null)
                    {
                        buttonImage.color = presetColor;
                    }
                    
                    // Add click listener
                    colorPresetButtons[i].onClick.AddListener(() => SetSelectedColor(presetColor));
                }
            }
        }
    }
    
    /// <summary>
    /// Set the selected color
    /// </summary>
    public void SetSelectedColor(Color color)
    {
        selectedColor = color;
        
        // Apply to current selected object if one is selected
        if (currentSelectedObject != null)
        {
            ApplyColorToObject(currentSelectedObject, selectedColor);
        }
    }
    
    /// <summary>
    /// Toggles the color picker visibility
    /// </summary>
    public void ToggleColorPicker()
    {
        isPickingColor = !isPickingColor;
        
        if (colorPickerPanel != null)
            colorPickerPanel.SetActive(isPickingColor);
            
        // If we're starting to pick color and have a selected object,
        // initialize the selected color with its current color
        if (isPickingColor && currentSelectedObject != null)
        {
            Renderer renderer = currentSelectedObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                selectedColor = renderer.material.color;
            }
        }
    }
    
    /// <summary>
    /// Apply the selected color to a specific object
    /// </summary>
    public void ApplyColorToObject(GameObject obj, Color color)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Check if we need to instantiate a new material
            Material material = renderer.material;
            material.color = color;
        }
        
        // Also check child renderers
        Renderer[] childRenderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer childRenderer in childRenderers)
        {
            if (childRenderer != renderer) // Don't apply twice to the same renderer
            {
                Material childMaterial = childRenderer.material;
                childMaterial.color = color;
            }
        }
    }
    
    /// <summary>
    /// Apply the currently selected color to the current object
    /// </summary>
    public void ApplyCurrentColor()
    {
        if (currentSelectedObject != null)
        {
            ApplyColorToObject(currentSelectedObject, selectedColor);
        }
    }
    
    /// <summary>
    /// Set the current object to receive color changes
    /// </summary>
    public void SelectObject(GameObject obj)
    {
        currentSelectedObject = obj;
        
        // Update selected color to match current object color if picker is active
        if (isPickingColor)
        {
            Renderer renderer = currentSelectedObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                selectedColor = renderer.material.color;
            }
        }
    }
    
    /// <summary>
    /// Called by UI elements to select objects by index in the targetObjects list
    /// </summary>
    public void SelectObjectByIndex(int index)
    {
        if (index >= 0 && index < targetObjects.Count)
        {
            SelectObject(targetObjects[index]);
        }
    }
    
    /// <summary>
    /// Add an object to the list of colorable objects
    /// </summary>
    public void AddTargetObject(GameObject obj)
    {
        if (!targetObjects.Contains(obj))
        {
            targetObjects.Add(obj);
        }
    }
    
    /// <summary>
    /// Remove an object from the list of colorable objects
    /// </summary>
    public void RemoveTargetObject(GameObject obj)
    {
        targetObjects.Remove(obj);
        
        // If this was the current selected object, clear the selection
        if (currentSelectedObject == obj)
        {
            currentSelectedObject = null;
        }
    }
    
    /// <summary>
    /// Apply a specific color to all target objects
    /// </summary>
    public void ApplyColorToAllTargets(Color color)
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null)
            {
                ApplyColorToObject(obj, color);
            }
        }
    }
    
    /// <summary>
    /// Apply the currently selected color to all target objects
    /// </summary>
    public void ApplyCurrentColorToAll()
    {
        ApplyColorToAllTargets(selectedColor);
    }
}
