using UnityEngine;
using SoulGames.EasyGridBuilderPro;

/// <summary>
/// Manages priority between different systems that might interfere with coloring
/// Ensures the color picker system has priority over other material changes
/// </summary>
[AddComponentMenu("XR Color System/Color Picker Priority Manager")]
public class ColorPickerPriorityManager : MonoBehaviour
{
    [Header("Priority Settings")]
    [Tooltip("Should we automatically disable material changes on BuildableObjectEffects when ColorableObject is present?")]
    public bool autoDisableBuildableObjectEffects = true;
    
    [Tooltip("Should we monitor for newly spawned objects?")]
    public bool monitorNewObjects = true;
    
    [Tooltip("How often to check for new objects (in seconds)")]
    public float monitorInterval = 1f;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private float lastMonitorTime;
    
    void Start()
    {
        if (autoDisableBuildableObjectEffects)
        {
            ProcessExistingObjects();
        }
    }
    
    void Update()
    {
        if (monitorNewObjects && Time.time - lastMonitorTime > monitorInterval)
        {
            ProcessExistingObjects();
            lastMonitorTime = Time.time;
        }
    }
    
    /// <summary>
    /// Process all existing objects to ensure proper priority
    /// </summary>
    void ProcessExistingObjects()
    {
        ColorableObject[] colorableObjects = FindObjectsOfType<ColorableObject>();
        int processedCount = 0;
        
        foreach (ColorableObject colorableObject in colorableObjects)
        {
            if (EnsureProperPriority(colorableObject))
            {
                processedCount++;
            }
        }
        
        if (showDebugInfo && processedCount > 0)
        {
            Debug.Log($"ColorPickerPriorityManager: Processed {processedCount} objects for proper priority");
        }
    }
    
    /// <summary>
    /// Ensure a specific ColorableObject has proper priority over BuildableObjectEffects
    /// </summary>
    /// <param name="colorableObject">The ColorableObject to check</param>
    /// <returns>True if changes were made</returns>
    bool EnsureProperPriority(ColorableObject colorableObject)
    {
        if (colorableObject == null) return false;
        
        BuildableObjectEffects effects = colorableObject.GetComponent<BuildableObjectEffects>();
        if (effects == null) return false;
        
        // Use reflection to check and disable changeObjectMaterial
        var changeObjectMaterialField = typeof(BuildableObjectEffects).GetField("changeObjectMaterial", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (changeObjectMaterialField != null)
        {
            bool currentValue = (bool)changeObjectMaterialField.GetValue(effects);
            
            if (currentValue) // If material changes are enabled, disable them
            {
                changeObjectMaterialField.SetValue(effects, false);
                
                if (showDebugInfo)
                {
                    Debug.Log($"ColorPickerPriorityManager: Disabled material changes on {colorableObject.gameObject.name}");
                }
                
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Force refresh all ColorableObjects to ensure they work properly
    /// </summary>
    [ContextMenu("Force Refresh All Colorable Objects")]
    public void ForceRefreshAllColorableObjects()
    {
        ColorableObject[] colorableObjects = FindObjectsOfType<ColorableObject>();
        
        foreach (ColorableObject colorableObject in colorableObjects)
        {
            // Refresh the integration
            var refreshMethod = typeof(ColorableObject).GetMethod("RefreshEGBProIntegration", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            if (refreshMethod != null)
            {
                refreshMethod.Invoke(colorableObject, null);
            }
            
            // Ensure proper priority
            EnsureProperPriority(colorableObject);
        }
        
        Debug.Log($"ColorPickerPriorityManager: Force refreshed {colorableObjects.Length} ColorableObjects");
    }
    
    /// <summary>
    /// Setup a specific object for coloring
    /// </summary>
    /// <param name="targetObject">The object to setup</param>
    public void SetupObjectForColoring(GameObject targetObject)
    {
        if (targetObject == null) return;
        
        // Add ColorableObject if it doesn't exist
        ColorableObject colorableObject = targetObject.GetComponent<ColorableObject>();
        if (colorableObject == null)
        {
            colorableObject = targetObject.AddComponent<ColorableObject>();
        }
        
        // Ensure proper priority
        EnsureProperPriority(colorableObject);
        
        if (showDebugInfo)
        {
            Debug.Log($"ColorPickerPriorityManager: Setup {targetObject.name} for coloring");
        }
    }
    
    /// <summary>
    /// Check if an object is properly setup for coloring
    /// </summary>
    /// <param name="targetObject">The object to check</param>
    /// <returns>True if properly setup</returns>
    public bool IsObjectSetupForColoring(GameObject targetObject)
    {
        if (targetObject == null) return false;
        
        ColorableObject colorableObject = targetObject.GetComponent<ColorableObject>();
        if (colorableObject == null) return false;
        
        BuildableObjectEffects effects = targetObject.GetComponent<BuildableObjectEffects>();
        if (effects == null) return true; // No BuildableObjectEffects, so no conflict
        
        // Check if material changes are disabled
        var changeObjectMaterialField = typeof(BuildableObjectEffects).GetField("changeObjectMaterial", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        if (changeObjectMaterialField != null)
        {
            bool materialChangesEnabled = (bool)changeObjectMaterialField.GetValue(effects);
            return !materialChangesEnabled; // Good if material changes are disabled
        }
        
        return false;
    }
    
    /// <summary>
    /// Get debug information about all colorable objects
    /// </summary>
    [ContextMenu("Debug All Colorable Objects")]
    public void DebugAllColorableObjects()
    {
        ColorableObject[] colorableObjects = FindObjectsOfType<ColorableObject>();
        
        Debug.Log($"=== Color Priority Debug Info ===");
        Debug.Log($"Found {colorableObjects.Length} ColorableObjects");
        
        foreach (ColorableObject colorableObject in colorableObjects)
        {
            bool isSetupCorrectly = IsObjectSetupForColoring(colorableObject.gameObject);
            BuildableObjectEffects effects = colorableObject.GetComponent<BuildableObjectEffects>();
            
            Debug.Log($"Object: {colorableObject.gameObject.name}");
            Debug.Log($"  - Properly setup: {isSetupCorrectly}");
            Debug.Log($"  - Has BuildableObjectEffects: {effects != null}");
            Debug.Log($"  - Current Color: {colorableObject.currentColor}");
            Debug.Log($"  - Original Color: {colorableObject.originalColor}");
        }
        
        Debug.Log($"================================");
    }
} 