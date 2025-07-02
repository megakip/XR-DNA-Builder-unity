using UnityEngine;
using SoulGames.EasyGridBuilderPro;

/// <summary>
/// Component to mark objects as colorable and provide additional coloring functionality
/// Attach this to any GameObject that should be colorable by the XR Controller Color Manager
/// Works with EGB Pro's BuildableObjectEffects material system
/// </summary>
public class ColorableObject : MonoBehaviour
{
    [Header("Color Settings")]
    [Tooltip("The original color of this object (will be set automatically on start)")]
    public Color originalColor;
    
    [Tooltip("Current color of this object")]
    public Color currentColor;
    
    [Tooltip("Should this object save its color state?")]
    public bool saveColorState = true;
    
    [Tooltip("Should this object emit a visual effect when colored?")]
    public bool showColorEffect = true;
    
    [Header("Visual Effects")]
    [Tooltip("Particle system to play when object is colored")]
    public ParticleSystem colorChangeEffect;
    
    [Tooltip("Audio clip to play when object is colored")]
    public AudioClip colorChangeSound;
    
    [Tooltip("Scale effect when object is colored")]
    public bool useScaleEffect = false;
    
    [Tooltip("Scale multiplier for color effect")]
    [Range(0.8f, 1.5f)]
    public float scaleEffectMultiplier = 1.1f;
    
    [Tooltip("Duration of scale effect")]
    public float scaleEffectDuration = 0.3f;
    
    [Header("EGB Pro Integration")]
    [Tooltip("BuildableObjectEffects component (auto-detected)")]
    public BuildableObjectEffects buildableObjectEffects;
    
    [Tooltip("Original base material from BuildableObjectEffects")]
    public Material originalBaseMaterial;
    
    [Tooltip("Colored version of the base material")]
    public Material coloredBaseMaterial;
    
    // Events
    public System.Action<Color> OnColorChanged;
    
    // Private variables
    private Renderer[] renderers;
    private UnityEngine.UI.Image[] images;
    private SpriteRenderer[] spriteRenderers;
    private AudioSource audioSource;
    private Vector3 originalScale;
    private bool isScaling = false;
    
    // Track the last color to detect Inspector changes
    private Color lastCurrentColor;
    
    void Start()
    {
        // Check if this is a grid object and disable coloring completely
        if (IsGridObject())
        {
            Debug.Log($"ColorableObject: Detected grid object {gameObject.name} - disabling ColorableObject component");
            this.enabled = false;
            return;
        }
        
        // Get components
        renderers = GetComponentsInChildren<Renderer>();
        images = GetComponentsInChildren<UnityEngine.UI.Image>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        originalScale = transform.localScale;
        
        // Auto-detect BuildableObjectEffects
        if (buildableObjectEffects == null)
        {
            buildableObjectEffects = GetComponent<BuildableObjectEffects>();
        }
        
        // Disable material changes in BuildableObjectEffects to prevent conflicts
        if (buildableObjectEffects != null)
        {
            // Use reflection to disable changeObjectMaterial
            var changeObjectMaterialField = typeof(BuildableObjectEffects).GetField("changeObjectMaterial", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            if (changeObjectMaterialField != null)
            {
                changeObjectMaterialField.SetValue(buildableObjectEffects, false);
                Debug.Log($"ColorableObject: Disabled changeObjectMaterial in BuildableObjectEffects for {gameObject.name}");
            }
        }
        
        // Initialize original colors
        InitializeOriginalColors();
        
        // Load saved color if enabled
        if (saveColorState)
        {
            LoadColorState();
        }
    }
    
    void Update()
    {
        // Check for Inspector color changes each frame (backup for OnValidate)
        if (currentColor != lastCurrentColor)
        {
            ChangeColor(currentColor);
            Debug.Log($"ColorableObject: Runtime color change detected - Applied {currentColor}");
        }
    }
    
    /// <summary>
    /// Initialize original colors from current materials/components
    /// </summary>
    void InitializeOriginalColors()
    {
        if (buildableObjectEffects != null)
        {
            // EGB Pro integration: use base material
            InitializeEGBProColors();
        }
        else
        {
            // Standard initialization
            InitializeStandardColors();
        }
    }
    
    /// <summary>
    /// Initialize colors for EGB Pro objects
    /// </summary>
    void InitializeEGBProColors()
    {
        Debug.Log($"ColorableObject: Attempting EGB Pro initialization on {gameObject.name}");
        
        // Get the base material from BuildableObjectEffects using reflection
        var baseMaterialField = typeof(BuildableObjectEffects).GetField("baseMaterial", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (baseMaterialField != null)
        {
            originalBaseMaterial = baseMaterialField.GetValue(buildableObjectEffects) as Material;
            
            if (originalBaseMaterial != null)
            {
                originalColor = originalBaseMaterial.color;
                currentColor = originalColor;
                lastCurrentColor = currentColor; // Initialize tracking
                
                // Create a colored version of the base material
                if (coloredBaseMaterial == null)
                {
                    coloredBaseMaterial = new Material(originalBaseMaterial);
                    coloredBaseMaterial.name = originalBaseMaterial.name + "_Colored";
                }
                
                Debug.Log($"ColorableObject: EGB Pro base material found: {originalBaseMaterial.name}, color: {originalColor}");
                return; // Success
            }
            else
            {
                Debug.Log($"ColorableObject: EGB Pro baseMaterial is null, falling back to standard mode");
            }
        }
        else
        {
            Debug.Log($"ColorableObject: EGB Pro baseMaterial field not found, falling back to standard mode");
        }
        
        // Fallback to standard initialization if EGB Pro setup fails
        InitializeStandardColors();
    }
    
    /// <summary>
    /// Initialize colors for standard objects (non-EGB Pro)
    /// </summary>
    void InitializeStandardColors()
    {
        Debug.Log($"ColorableObject: Standard initialization on {gameObject.name}");
        Debug.Log($"ColorableObject: Found {renderers.Length} renderers, {images.Length} images, {spriteRenderers.Length} sprite renderers");
        
        // Check if this object uses custom shaders that shouldn't be colored (like grid shaders)
        if (ShouldSkipColoring())
        {
            Debug.Log($"ColorableObject: Skipping coloring for {gameObject.name} - uses custom shader that shouldn't be colored");
            originalColor = Color.white;
            currentColor = originalColor;
            lastCurrentColor = currentColor;
            return;
        }
        
        // Try to get color from first renderer
        if (renderers != null && renderers.Length > 0 && renderers[0].material != null)
        {
            // Check if the material has a color property before accessing it
            if (HasColorProperty(renderers[0].material))
            {
                originalColor = renderers[0].material.color;
                Debug.Log($"ColorableObject: Using renderer material color from {renderers[0].gameObject.name}: {originalColor}");
            }
            else
            {
                Debug.Log($"ColorableObject: Material {renderers[0].material.name} doesn't have standard color property, using white");
                originalColor = Color.white;
            }
        }
        // Try to get color from first UI Image
        else if (images != null && images.Length > 0)
        {
            originalColor = images[0].color;
            Debug.Log($"ColorableObject: Using UI Image color: {originalColor}");
        }
        // Try to get color from first SpriteRenderer
        else if (spriteRenderers != null && spriteRenderers.Length > 0)
        {
            originalColor = spriteRenderers[0].color;
            Debug.Log($"ColorableObject: Using SpriteRenderer color: {originalColor}");
        }
        // Default to white
        else
        {
            originalColor = Color.white;
            Debug.Log($"ColorableObject: No renderers found, using default white color");
        }
        
        currentColor = originalColor;
        lastCurrentColor = currentColor; // Initialize tracking
        Debug.Log($"ColorableObject: Standard initialization complete - Original: {originalColor}");
    }
    
    /// <summary>
    /// Check if this object should skip coloring (e.g., grid objects with custom shaders)
    /// </summary>
    bool ShouldSkipColoring()
    {
        if (renderers == null || renderers.Length == 0) return false;
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && renderer.material != null)
            {
                string shaderName = renderer.material.shader.name;
                
                // Skip grid shaders and other custom shaders that shouldn't be colored
                if (shaderName.Contains("Grid") || 
                    shaderName.Contains("SoulGames/Grid") ||
                    gameObject.name.ToLower().Contains("grid") ||
                    gameObject.name.ToLower().Contains("plane"))
                {
                    Debug.Log($"ColorableObject: Detected grid/plane object with shader {shaderName} - skipping coloring");
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Check if a material has a standard color property
    /// </summary>
    bool HasColorProperty(Material material)
    {
        if (material == null) return false;
        
        // Check for common color properties
        return material.HasProperty("_Color") || 
               material.HasProperty("_BaseColor") || 
               material.HasProperty("_MainColor");
    }
    
    /// <summary>
    /// Change the color of this object
    /// </summary>
    /// <param name="newColor">The new color to apply</param>
    public void ChangeColor(Color newColor)
    {
        currentColor = newColor;
        lastCurrentColor = newColor; // Update tracking
        
        if (buildableObjectEffects != null)
        {
            // EGB Pro: Update the base material
            ApplyColorToEGBProObject(newColor);
        }
        else
        {
            // Standard: Update renderers directly
            ApplyColorToStandardObject(newColor);
        }
        
        // Trigger effects
        if (showColorEffect)
        {
            PlayColorEffects();
        }
        
        // Save color state
        if (saveColorState)
        {
            SaveColorState();
        }
        
        // Invoke event
        OnColorChanged?.Invoke(newColor);
        
        Debug.Log($"ColorableObject: Color changed to {newColor}");
    }
    
    /// <summary>
    /// Called when Inspector values change - handles color picker changes
    /// </summary>
    void OnValidate()
    {
        // Only apply changes during Play mode to avoid issues in Edit mode
        if (!Application.isPlaying) return;
        
        // Check if currentColor has changed since last frame
        if (currentColor != lastCurrentColor)
        {
            // Apply the new color
            ChangeColor(currentColor);
            lastCurrentColor = currentColor;
            
            Debug.Log($"ColorableObject: Inspector color change detected - Applied {currentColor}");
        }
    }
    
    /// <summary>
    /// Apply color to EGB Pro objects by updating the base material
    /// </summary>
    void ApplyColorToEGBProObject(Color newColor)
    {
        Debug.Log($"ColorableObject: Attempting EGB Pro color application with {newColor}");
        
        if (originalBaseMaterial == null || coloredBaseMaterial == null)
        {
            Debug.LogWarning("ColorableObject: Original or colored base material is null! Falling back to standard mode.");
            ApplyColorToStandardObject(newColor);
            return;
        }
        
        // Update the colored base material
        coloredBaseMaterial.color = newColor;
        Debug.Log($"ColorableObject: Updated colored base material to {newColor}");
        
        // Apply the colored base material to BuildableObjectEffects using reflection
        var baseMaterialField = typeof(BuildableObjectEffects).GetField("baseMaterial", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (baseMaterialField != null)
        {
            baseMaterialField.SetValue(buildableObjectEffects, coloredBaseMaterial);
            Debug.Log($"ColorableObject: Set EGB Pro base material to {coloredBaseMaterial.name}");
            
            // Force apply the base material to refresh the visuals
            var applyBaseMaterialMethod = typeof(BuildableObjectEffects).GetMethod("ApplyBaseMaterial");
            if (applyBaseMaterialMethod != null)
            {
                applyBaseMaterialMethod.Invoke(buildableObjectEffects, null);
                Debug.Log($"ColorableObject: Called EGB Pro ApplyBaseMaterial method");
                
                // Check if the renderers actually use the base material now
                bool materialApplied = CheckIfEGBProMaterialIsApplied();
                if (!materialApplied)
                {
                    Debug.Log($"ColorableObject: EGB Pro base material not applied to renderers, using direct approach");
                    ApplyColorToStandardObject(newColor);
                }
            }
            else
            {
                Debug.Log($"ColorableObject: EGB Pro ApplyBaseMaterial method not found, applying directly to renderers");
                // Fallback: directly apply to renderers
                ApplyColorToStandardObject(newColor);
            }
        }
        else
        {
            Debug.Log($"ColorableObject: EGB Pro baseMaterial field not accessible, falling back to standard coloring");
            // Fallback to standard coloring
            ApplyColorToStandardObject(newColor);
        }
    }
    
    /// <summary>
    /// Check if the EGB Pro base material is actually being used by the renderers
    /// </summary>
    bool CheckIfEGBProMaterialIsApplied()
    {
        if (coloredBaseMaterial == null) return false;
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                foreach (Material mat in renderer.materials)
                {
                    if (mat != null && (mat == coloredBaseMaterial || mat.name.Contains(coloredBaseMaterial.name)))
                    {
                        Debug.Log($"ColorableObject: Found EGB Pro material {mat.name} on renderer {renderer.gameObject.name}");
                        return true;
                    }
                }
            }
        }
        
        Debug.Log($"ColorableObject: EGB Pro base material not found on any renderer");
        return false;
    }
    
    /// <summary>
    /// Apply material to all renderers
    /// </summary>
    void ApplyMaterialToRenderers(Material material)
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                // Create individual material instances for each renderer
                Material[] materials = new Material[renderer.materials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = new Material(material);
                }
                renderer.materials = materials;
            }
        }
    }
    
    /// <summary>
    /// Apply color to standard objects (non-EGB Pro)
    /// </summary>
    void ApplyColorToStandardObject(Color newColor)
    {
        Debug.Log($"ColorableObject: Applying standard color {newColor} to {gameObject.name}");
        
        // Check if this object should skip coloring
        if (ShouldSkipColoring())
        {
            Debug.Log($"ColorableObject: Skipping color application for {gameObject.name} - uses custom shader that shouldn't be colored");
            return;
        }
        
        // Make sure BuildableObjectEffects won't interfere with our color changes
        if (buildableObjectEffects != null)
        {
            // Ensure changeObjectMaterial is disabled
            var changeObjectMaterialField = typeof(BuildableObjectEffects).GetField("changeObjectMaterial", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            if (changeObjectMaterialField != null)
            {
                changeObjectMaterialField.SetValue(buildableObjectEffects, false);
            }
            
            // Also try to set our material as the base material
            SetColoredMaterialAsBase(newColor);
            
            Debug.Log($"ColorableObject: Ensured BuildableObjectEffects won't override materials");
        }
        
        // Apply to Renderers (including child renderers)
        Debug.Log($"ColorableObject: Found {renderers.Length} renderers total (including children)");
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                // Skip renderers with custom shaders that shouldn't be colored
                if (renderer.material != null && !HasColorProperty(renderer.material))
                {
                    Debug.Log($"ColorableObject: Skipping renderer {renderer.gameObject.name} - material doesn't have standard color properties");
                    continue;
                }
                
                Debug.Log($"ColorableObject: Applying color to renderer on {renderer.gameObject.name} (parent: {gameObject.name})");
                Debug.Log($"ColorableObject: Renderer path: {GetGameObjectPath(renderer.gameObject)}");
                Debug.Log($"ColorableObject: Original material: {renderer.material.name}, shader: {renderer.material.shader.name}");
                
                // Create new material instances to avoid affecting other objects
                Material[] materials = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    // Skip materials that don't have standard color properties
                    if (!HasColorProperty(renderer.materials[i]))
                    {
                        Debug.Log($"ColorableObject: Skipping material {renderer.materials[i].name} - no standard color properties");
                        materials[i] = renderer.materials[i]; // Keep original material
                        continue;
                    }
                    
                    materials[i] = new Material(renderer.materials[i]);
                    materials[i].name = $"{renderer.materials[i].name}_ColoredBy_{gameObject.name}";
                    
                    // Try different approaches to set color based on shader properties
                    bool colorSet = false;
                    
                    if (materials[i].HasProperty("_Color"))
                    {
                        materials[i].SetColor("_Color", newColor);
                        Debug.Log($"ColorableObject: Set _Color property to {newColor}");
                        colorSet = true;
                    }
                    
                    if (materials[i].HasProperty("_BaseColor"))
                    {
                        materials[i].SetColor("_BaseColor", newColor);
                        Debug.Log($"ColorableObject: Set _BaseColor property to {newColor}");
                        colorSet = true;
                    }
                    
                    if (materials[i].HasProperty("_MainColor"))
                    {
                        materials[i].SetColor("_MainColor", newColor);
                        Debug.Log($"ColorableObject: Set _MainColor property to {newColor}");
                        colorSet = true;
                    }
                    
                    if (materials[i].HasProperty("_TintColor"))
                    {
                        materials[i].SetColor("_TintColor", newColor);
                        Debug.Log($"ColorableObject: Set _TintColor property to {newColor}");
                        colorSet = true;
                    }
                    
                    // Always set basic color property as well
                    materials[i].color = newColor;
                    Debug.Log($"ColorableObject: Set basic color property to {newColor}");
                    
                    if (!colorSet)
                    {
                        Debug.LogWarning($"ColorableObject: No suitable color property found for material {materials[i].name} with shader {materials[i].shader.name}");
                    }
                    
                    Debug.Log($"ColorableObject: Created colored material: {materials[i].name} from {renderer.materials[i].name}");
                }
                
                // Apply the materials
                renderer.materials = materials;
                
                // Force refresh the renderer
                renderer.enabled = false;
                renderer.enabled = true;
                
                Debug.Log($"ColorableObject: Applied {materials.Length} materials to renderer '{renderer.gameObject.name}' and refreshed");
            }
        }
        
        // Apply to UI Images
        foreach (UnityEngine.UI.Image image in images)
        {
            if (image != null)
            {
                image.color = newColor;
                Debug.Log($"ColorableObject: Applied color to UI Image");
            }
        }
        
        // Apply to SpriteRenderers
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = newColor;
                Debug.Log($"ColorableObject: Applied color to SpriteRenderer");
            }
        }
        
        Debug.Log($"ColorableObject: Standard color application complete");
    }
    
    /// <summary>
    /// Get the full path of a GameObject in the hierarchy for debugging
    /// </summary>
    string GetGameObjectPath(GameObject obj)
    {
        if (obj == null) return "null";
        
        string path = obj.name;
        Transform current = obj.transform.parent;
        
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        
        return path;
    }
    
    /// <summary>
    /// Set our colored material as the base material in BuildableObjectEffects
    /// </summary>
    void SetColoredMaterialAsBase(Color newColor)
    {
        if (buildableObjectEffects == null) return;
        
        // Get the current base material
        var baseMaterialField = typeof(BuildableObjectEffects).GetField("baseMaterial", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (baseMaterialField != null)
        {
            Material currentBaseMaterial = baseMaterialField.GetValue(buildableObjectEffects) as Material;
            
            if (currentBaseMaterial != null)
            {
                // Create a colored version of the base material
                Material coloredMaterial = new Material(currentBaseMaterial);
                coloredMaterial.color = newColor;
                coloredMaterial.name = currentBaseMaterial.name + "_Colored";
                
                // Set the colored material as the new base material
                baseMaterialField.SetValue(buildableObjectEffects, coloredMaterial);
                
                // Force apply the base material
                var applyBaseMaterialMethod = typeof(BuildableObjectEffects).GetMethod("ApplyBaseMaterial");
                if (applyBaseMaterialMethod != null)
                {
                    applyBaseMaterialMethod.Invoke(buildableObjectEffects, null);
                }
                
                Debug.Log($"ColorableObject: Set colored base material {coloredMaterial.name} with color {newColor}");
            }
        }
    }
    
    /// <summary>
    /// Reset the object to its original color
    /// </summary>
    public void ResetColor()
    {
        ChangeColor(originalColor);
    }
    
    /// <summary>
    /// Play visual and audio effects when color changes
    /// </summary>
    void PlayColorEffects()
    {
        // Play particle effect
        if (colorChangeEffect != null)
        {
            colorChangeEffect.Play();
        }
        
        // Play audio effect
        if (colorChangeSound != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(colorChangeSound);
            }
            else
            {
                AudioSource.PlayClipAtPoint(colorChangeSound, transform.position);
            }
        }
        
        // Play scale effect
        if (useScaleEffect && !isScaling)
        {
            StartCoroutine(ScaleEffect());
        }
    }
    
    /// <summary>
    /// Scale effect coroutine
    /// </summary>
    System.Collections.IEnumerator ScaleEffect()
    {
        isScaling = true;
        Vector3 targetScale = originalScale * scaleEffectMultiplier;
        float elapsed = 0f;
        
        // Scale up
        while (elapsed < scaleEffectDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (scaleEffectDuration / 2f);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }
        
        elapsed = 0f;
        
        // Scale down
        while (elapsed < scaleEffectDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (scaleEffectDuration / 2f);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }
        
        transform.localScale = originalScale;
        isScaling = false;
    }
    
    /// <summary>
    /// Save the current color state to PlayerPrefs
    /// </summary>
    void SaveColorState()
    {
        string key = $"ColorableObject_{gameObject.name}_{GetInstanceID()}";
        PlayerPrefs.SetFloat($"{key}_R", currentColor.r);
        PlayerPrefs.SetFloat($"{key}_G", currentColor.g);
        PlayerPrefs.SetFloat($"{key}_B", currentColor.b);
        PlayerPrefs.SetFloat($"{key}_A", currentColor.a);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Load the saved color state from PlayerPrefs
    /// </summary>
    void LoadColorState()
    {
        string key = $"ColorableObject_{gameObject.name}_{GetInstanceID()}";
        
        if (PlayerPrefs.HasKey($"{key}_R"))
        {
            Color savedColor = new Color(
                PlayerPrefs.GetFloat($"{key}_R"),
                PlayerPrefs.GetFloat($"{key}_G"),
                PlayerPrefs.GetFloat($"{key}_B"),
                PlayerPrefs.GetFloat($"{key}_A")
            );
            
            ChangeColor(savedColor);
        }
    }
    
    /// <summary>
    /// Context menu for testing color changes
    /// </summary>
    [ContextMenu("Test Color - Red")]
    void TestColorRed()
    {
        ChangeColor(Color.red);
    }
    
    [ContextMenu("Test Color - Blue")]
    void TestColorBlue()
    {
        ChangeColor(Color.blue);
    }
    
    [ContextMenu("Test Color - Green")]
    void TestColorGreen()
    {
        ChangeColor(Color.green);
    }
    
    [ContextMenu("Reset to Original Color")]
    void TestResetColor()
    {
        ResetColor();
    }
    
    [ContextMenu("Refresh EGB Pro Integration")]
    void RefreshEGBProIntegration()
    {
        buildableObjectEffects = GetComponent<BuildableObjectEffects>();
        InitializeOriginalColors();
    }
    
    [ContextMenu("Debug Renderer Components")]
    void DebugRendererComponents()
    {
        Debug.Log($"=== Debug Renderer Components for {gameObject.name} ===");
        
        // Get all renderers in children
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        Debug.Log($"Total renderers found in children: {allRenderers.Length}");
        
        for (int i = 0; i < allRenderers.Length; i++)
        {
            Renderer r = allRenderers[i];
            Debug.Log($"Renderer {i}: {r.gameObject.name} (Path: {GetGameObjectPath(r.gameObject)})");
            Debug.Log($"  - Material Count: {r.materials.Length}");
            for (int j = 0; j < r.materials.Length; j++)
            {
                Material mat = r.materials[j];
                Debug.Log($"    Material {j}: {mat.name} (Shader: {mat.shader.name}, Color: {mat.color})");
            }
        }
        
        // Check BuildableObjectEffects
        if (buildableObjectEffects != null)
        {
            Debug.Log($"BuildableObjectEffects present on {buildableObjectEffects.gameObject.name}");
        }
        else
        {
            Debug.Log("No BuildableObjectEffects found");
        }
        
        Debug.Log($"======================================");
    }
    
    /// <summary>
    /// Check if this object is a grid object that shouldn't be colored
    /// </summary>
    bool IsGridObject()
    {
        // Check GameObject name
        string objectName = gameObject.name.ToLower();
        if (objectName.Contains("grid") || objectName.Contains("plane"))
        {
            return true;
        }
        
        // Check if it has EasyGridBuilderPro components
        if (GetComponent<SoulGames.EasyGridBuilderPro.EasyGridBuilderPro>() != null)
        {
            return true;
        }
        
        // Check if it has grid visual handler components
        if (GetComponent<SoulGames.EasyGridBuilderPro.EditorGridVisualHandlerXZ>() != null ||
            GetComponent<SoulGames.EasyGridBuilderPro.EditorGridVisualHandlerXY>() != null)
        {
            return true;
        }
        
        // Check shader names
        if (renderers != null)
        {
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                if (renderer != null && renderer.material != null)
                {
                    string shaderName = renderer.material.shader.name;
                    if (shaderName.Contains("Grid") || shaderName.Contains("Plane"))
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
} 