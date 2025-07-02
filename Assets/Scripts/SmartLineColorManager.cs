using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Smart Line Color Manager that automatically finds XR Origin components
/// and monitors ColorPreview Image for line color changes
/// Much simpler than looking for specific ColorPicker scripts!
/// </summary>
public class SmartLineColorManager : MonoBehaviour
{
    [Header("Auto Detection")]
    [Tooltip("Automatically find XR Origin and components on Start")]
    public bool autoDetectOnStart = true;
    
    [Tooltip("Continuously monitor ColorPreview for changes")]
    public bool monitorColorPreview = true;
    
    [Tooltip("Show debug information")]
    public bool showDebugInfo = true;
    
    [Header("Manual References (Optional)")]
    [Tooltip("XR Origin GameObject (leave empty for auto-detection)")]
    public GameObject xrOrigin;
    
    [Tooltip("ColorPreview Image (leave empty for auto-detection)")]
    public Image colorPreviewImage;
    
    [Tooltip("Line Renderer to control (leave empty for auto-detection)")]
    public LineRenderer targetLineRenderer;
    
    [Header("Line Color Settings")]
    [Tooltip("Alpha value for line start (0-1)")]
    [Range(0f, 1f)]
    public float lineStartAlpha = 0.8f;
    
    [Tooltip("Alpha value for line end (0-1)")]
    [Range(0f, 1f)]
    public float lineEndAlpha = 0.2f;
    
    // Private tracking
    private Color lastKnownColor = Color.white;
    private bool isInitialized = false;
    
    void Start()
    {
        if (autoDetectOnStart)
        {
            SetupSmartLineColorSystem();
        }
    }
    
    void Update()
    {
        if (monitorColorPreview && isInitialized)
        {
            CheckForColorChanges();
        }
    }
    
    /// <summary>
    /// Main setup method - automatically finds all required components
    /// </summary>
    public void SetupSmartLineColorSystem()
    {
        if (showDebugInfo)
        {
            Debug.Log("SmartLineColorManager: Starting smart setup...");
        }
        
        // Step 1: Find XR Origin
        FindXROrigin();
        
        // Step 2: Find LineRenderer in XR Origin
        FindLineRenderer();
        
        // Step 3: Find ColorPreview Image
        FindColorPreviewImage();
        
        // Step 4: Initialize with current color
        InitializeLineColor();
        
        // Step 5: Verify setup
        VerifySetup();
        
        if (showDebugInfo)
        {
            Debug.Log("SmartLineColorManager: Smart setup complete!");
        }
    }
    
    /// <summary>
    /// Find XR Origin in the scene
    /// </summary>
    void FindXROrigin()
    {
        if (xrOrigin != null)
        {
            if (showDebugInfo)
            {
                Debug.Log($"SmartLineColorManager: Using manually assigned XR Origin: {xrOrigin.name}");
            }
            return;
        }
        
        // Look for GameObject with "XR Origin" in name
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("xr origin") || 
                obj.name.ToLower().Contains("xrorigin"))
            {
                xrOrigin = obj;
                if (showDebugInfo)
                {
                    Debug.Log($"SmartLineColorManager: Found XR Origin: {xrOrigin.name}");
                }
                return;
            }
        }
        
        // Fallback: look for NearFarInteractor and use its root
        NearFarInteractor nearFarInteractor = FindObjectOfType<NearFarInteractor>();
        if (nearFarInteractor != null)
        {
            // Navigate up to find the root
            Transform current = nearFarInteractor.transform;
            while (current.parent != null)
            {
                current = current.parent;
            }
            xrOrigin = current.gameObject;
            
            if (showDebugInfo)
            {
                Debug.Log($"SmartLineColorManager: Found XR Origin via NearFarInteractor: {xrOrigin.name}");
            }
        }
        else
        {
            Debug.LogWarning("SmartLineColorManager: Could not find XR Origin!");
        }
    }
    
    /// <summary>
    /// Find LineRenderer in XR Origin hierarchy
    /// </summary>
    void FindLineRenderer()
    {
        if (targetLineRenderer != null)
        {
            if (showDebugInfo)
            {
                Debug.Log($"SmartLineColorManager: Using manually assigned LineRenderer: {targetLineRenderer.name}");
            }
            return;
        }
        
        if (xrOrigin == null)
        {
            Debug.LogWarning("SmartLineColorManager: No XR Origin found, cannot find LineRenderer!");
            return;
        }
        
        // Search in XR Origin hierarchy
        LineRenderer[] lineRenderers = xrOrigin.GetComponentsInChildren<LineRenderer>();
        
        if (lineRenderers.Length > 0)
        {
            // Prefer LineRenderer that's associated with an interactor
            foreach (LineRenderer lr in lineRenderers)
            {
                // Check if it's on or near an XR interactor
                if (lr.GetComponent<XRRayInteractor>() != null ||
                    lr.GetComponentInParent<XRRayInteractor>() != null ||
                    lr.GetComponent<NearFarInteractor>() != null ||
                    lr.GetComponentInParent<NearFarInteractor>() != null)
                {
                    targetLineRenderer = lr;
                    if (showDebugInfo)
                    {
                        Debug.Log($"SmartLineColorManager: Found LineRenderer on XR interactor: {lr.name}");
                    }
                    return;
                }
            }
            
            // Fallback to first LineRenderer
            targetLineRenderer = lineRenderers[0];
            if (showDebugInfo)
            {
                Debug.Log($"SmartLineColorManager: Using first LineRenderer in XR Origin: {targetLineRenderer.name}");
            }
        }
        else
        {
            Debug.LogWarning("SmartLineColorManager: No LineRenderer found in XR Origin hierarchy!");
        }
    }
    
    /// <summary>
    /// Find ColorPreview Image in the scene
    /// </summary>
    void FindColorPreviewImage()
    {
        if (colorPreviewImage != null)
        {
            if (showDebugInfo)
            {
                Debug.Log($"SmartLineColorManager: Using manually assigned ColorPreview: {colorPreviewImage.name}");
            }
            return;
        }
        
        // Look for Image with "ColorPreview" in name
        Image[] allImages = FindObjectsOfType<Image>();
        foreach (Image img in allImages)
        {
            if (img.name.ToLower().Contains("colorpreview") ||
                img.name.ToLower().Contains("color preview") ||
                img.name.ToLower().Contains("preview"))
            {
                colorPreviewImage = img;
                if (showDebugInfo)
                {
                    Debug.Log($"SmartLineColorManager: Found ColorPreview Image: {img.name}");
                }
                return;
            }
        }
        
        Debug.LogWarning("SmartLineColorManager: Could not find ColorPreview Image! Please assign manually.");
    }
    
    /// <summary>
    /// Initialize line color with current ColorPreview color
    /// </summary>
    void InitializeLineColor()
    {
        if (colorPreviewImage != null && targetLineRenderer != null)
        {
            lastKnownColor = colorPreviewImage.color;
            UpdateLineColor(lastKnownColor);
            isInitialized = true;
            
            if (showDebugInfo)
            {
                Debug.Log($"SmartLineColorManager: Initialized with color: {ColorToHex(lastKnownColor)}");
            }
        }
    }
    
    /// <summary>
    /// Check for color changes in ColorPreview and update line accordingly
    /// </summary>
    void CheckForColorChanges()
    {
        if (colorPreviewImage == null || targetLineRenderer == null) return;
        
        Color currentColor = colorPreviewImage.color;
        
        // Check if color has changed
        if (currentColor != lastKnownColor)
        {
            UpdateLineColor(currentColor);
            lastKnownColor = currentColor;
            
            if (showDebugInfo)
            {
                Debug.Log($"SmartLineColorManager: Color changed to: {ColorToHex(currentColor)}");
            }
        }
    }
    
    /// <summary>
    /// Update the LineRenderer color with both gradient and material
    /// </summary>
    void UpdateLineColor(Color baseColor)
    {
        if (targetLineRenderer == null) return;
        
        // Method 1: Update Gradient
        UpdateLineGradient(baseColor);
        
        // Method 2: Update Material (if present)
        UpdateLineMaterial(baseColor);
        
        if (showDebugInfo)
        {
            Debug.Log($"SmartLineColorManager: Updated LineRenderer color to {ColorToHex(baseColor)}");
            Debug.Log($"SmartLineColorManager: Material: {targetLineRenderer.material?.name}, UseWorldSpace: {targetLineRenderer.useWorldSpace}");
        }
    }
    
    /// <summary>
    /// Update the LineRenderer gradient
    /// </summary>
    void UpdateLineGradient(Color baseColor)
    {
        // Create gradient with selected color
        Gradient gradient = new Gradient();
        
        // Start and end colors with different alpha values
        Color startColor = new Color(baseColor.r, baseColor.g, baseColor.b, lineStartAlpha);
        Color endColor = new Color(baseColor.r, baseColor.g, baseColor.b, lineEndAlpha);
        
        // Set gradient keys
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0] = new GradientColorKey(startColor, 0f);
        colorKeys[1] = new GradientColorKey(endColor, 1f);
        
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(lineStartAlpha, 0f);
        alphaKeys[1] = new GradientAlphaKey(lineEndAlpha, 1f);
        
        gradient.SetKeys(colorKeys, alphaKeys);
        
        // Apply to LineRenderer
        targetLineRenderer.colorGradient = gradient;
        
        if (showDebugInfo)
        {
            Debug.Log($"SmartLineColorManager: Updated gradient with colors {ColorToHex(startColor)} to {ColorToHex(endColor)}");
        }
    }
    
    /// <summary>
    /// Update the LineRenderer material if it has color properties
    /// </summary>
    void UpdateLineMaterial(Color baseColor)
    {
        if (targetLineRenderer.material == null) return;
        
        // Create a runtime instance of the material to avoid affecting the original
        Material runtimeMaterial = new Material(targetLineRenderer.material);
        runtimeMaterial.name = targetLineRenderer.material.name + "_Runtime_Colored";
        
        // Try different color properties that line materials might use
        bool colorSet = false;
        
        if (runtimeMaterial.HasProperty("_Color"))
        {
            runtimeMaterial.SetColor("_Color", baseColor);
            colorSet = true;
            if (showDebugInfo) Debug.Log($"SmartLineColorManager: Set _Color property");
        }
        
        if (runtimeMaterial.HasProperty("_BaseColor"))
        {
            runtimeMaterial.SetColor("_BaseColor", baseColor);
            colorSet = true;
            if (showDebugInfo) Debug.Log($"SmartLineColorManager: Set _BaseColor property");
        }
        
        if (runtimeMaterial.HasProperty("_TintColor"))
        {
            runtimeMaterial.SetColor("_TintColor", baseColor);
            colorSet = true;
            if (showDebugInfo) Debug.Log($"SmartLineColorManager: Set _TintColor property");
        }
        
        if (runtimeMaterial.HasProperty("_StartColor"))
        {
            runtimeMaterial.SetColor("_StartColor", baseColor);
            colorSet = true;
            if (showDebugInfo) Debug.Log($"SmartLineColorManager: Set _StartColor property");
        }
        
        if (runtimeMaterial.HasProperty("_EndColor"))
        {
            runtimeMaterial.SetColor("_EndColor", baseColor);
            colorSet = true;
            if (showDebugInfo) Debug.Log($"SmartLineColorManager: Set _EndColor property");
        }
        
        if (colorSet)
        {
            // Apply the modified material
            targetLineRenderer.material = runtimeMaterial;
            if (showDebugInfo)
            {
                Debug.Log($"SmartLineColorManager: Applied runtime material with color {ColorToHex(baseColor)}");
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"SmartLineColorManager: No suitable color property found in material {runtimeMaterial.name} (Shader: {runtimeMaterial.shader.name})");
            }
        }
    }
    
    /// <summary>
    /// Verify the setup is working correctly
    /// </summary>
    void VerifySetup()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== SmartLineColorManager Verification ===");
            Debug.Log($"XR Origin: {xrOrigin?.name ?? "None"}");
            Debug.Log($"LineRenderer: {targetLineRenderer?.name ?? "None"}");
            Debug.Log($"ColorPreview: {colorPreviewImage?.name ?? "None"}");
            Debug.Log($"Current Color: {(colorPreviewImage != null ? ColorToHex(colorPreviewImage.color) : "None")}");
            Debug.Log($"Setup Valid: {isInitialized}");
            Debug.Log("==========================================");
        }
    }
    
    /// <summary>
    /// Convert Color to Hex string for debugging
    /// </summary>
    string ColorToHex(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
    }
    
    /// <summary>
    /// Manual test methods
    /// </summary>
    [ContextMenu("Test Line Color - Red")]
    public void TestLineColorRed()
    {
        TestLineColor(Color.red);
    }
    
    [ContextMenu("Test Line Color - Blue")]
    public void TestLineColorBlue()
    {
        TestLineColor(Color.blue);
    }
    
    [ContextMenu("Test Line Color - Green")]
    public void TestLineColorGreen()
    {
        TestLineColor(Color.green);
    }
    
    /// <summary>
    /// Test line color directly
    /// </summary>
    void TestLineColor(Color testColor)
    {
        if (targetLineRenderer != null)
        {
            UpdateLineColor(testColor);
            Debug.Log($"SmartLineColorManager: Test color applied: {ColorToHex(testColor)}");
        }
        else
        {
            Debug.LogError("SmartLineColorManager: No LineRenderer available for testing!");
        }
    }
    
    /// <summary>
    /// Force color update from current ColorPreview
    /// </summary>
    [ContextMenu("Force Update from ColorPreview")]
    public void ForceUpdateFromColorPreview()
    {
        if (colorPreviewImage != null)
        {
            UpdateLineColor(colorPreviewImage.color);
            lastKnownColor = colorPreviewImage.color;
            Debug.Log($"SmartLineColorManager: Forced update to: {ColorToHex(colorPreviewImage.color)}");
        }
        else
        {
            Debug.LogError("SmartLineColorManager: No ColorPreview Image available!");
        }
    }
    
    /// <summary>
    /// Manual setup trigger
    /// </summary>
    [ContextMenu("Manual Setup")]
    public void ManualSetup()
    {
        SetupSmartLineColorSystem();
    }
    
    /// <summary>
    /// Public method to manually set ColorPreview reference
    /// </summary>
    public void SetColorPreviewImage(Image previewImage)
    {
        colorPreviewImage = previewImage;
        if (showDebugInfo)
        {
            Debug.Log($"SmartLineColorManager: ColorPreview manually set to: {previewImage.name}");
        }
        
        if (targetLineRenderer != null)
        {
            InitializeLineColor();
        }
    }
} 