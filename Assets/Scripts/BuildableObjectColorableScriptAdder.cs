using UnityEngine;
using SoulGames.EasyGridBuilderPro;

/// <summary>
/// Automatically adds ColorableObject script to buildable objects when they are spawned
/// based on the BuildableObjectSO settings
/// </summary>
[AddComponentMenu("XR Color System/Buildable Object Colorable Script Adder")]
public class BuildableObjectColorableScriptAdder : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Should this script start automatically on Start?")]
    public bool autoStart = true;
    
    [Tooltip("Show debug information in console")]
    public bool showDebugInfo = false;
    
    [Header("Statistics")]
    [SerializeField, ReadOnly] private int objectsProcessed = 0;
    [SerializeField, ReadOnly] private int scriptsAdded = 0;
    
    private void Start()
    {
        if (autoStart)
        {
            StartListening();
        }
    }
    
    /// <summary>
    /// Start listening to buildable object placement events
    /// </summary>
    public void StartListening()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.OnBuildableObjectPlaced += OnBuildableObjectPlaced;
            
            if (showDebugInfo)
                Debug.Log("BuildableObjectColorableScriptAdder: Started listening to buildable object placement events");
        }
        else
        {
            Debug.LogWarning("BuildableObjectColorableScriptAdder: GridManager.Instance not found! Make sure there is a GridManager in the scene.");
        }
    }
    
    /// <summary>
    /// Stop listening to buildable object placement events
    /// </summary>
    public void StopListening()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.OnBuildableObjectPlaced -= OnBuildableObjectPlaced;
            
            if (showDebugInfo)
                Debug.Log("BuildableObjectColorableScriptAdder: Stopped listening to buildable object placement events");
        }
    }
    
    private void OnDestroy()
    {
        StopListening();
    }
    
    /// <summary>
    /// Called when a buildable object is placed on the grid
    /// </summary>
    private void OnBuildableObjectPlaced(EasyGridBuilderPro gridSystem, BuildableObject buildableObject)
    {
        if (buildableObject == null) return;
        
        objectsProcessed++;
        
        // Get the BuildableObjectSO from the buildable object
        BuildableObjectSO buildableObjectSO = GetBuildableObjectSO(buildableObject);
        
        if (buildableObjectSO == null)
        {
            if (showDebugInfo)
                Debug.LogWarning($"BuildableObjectColorableScriptAdder: Could not get BuildableObjectSO from {buildableObject.name}");
            return;
        }
        
        // Check if we should add the ColorableObject script
        if (buildableObjectSO.addColorableObjectScript)
        {
            // Check if the object already has the ColorableObject script
            ColorableObject existingColorableObject = buildableObject.GetComponent<ColorableObject>();
            
            if (existingColorableObject == null)
            {
                // Add the ColorableObject script
                ColorableObject colorableObject = buildableObject.gameObject.AddComponent<ColorableObject>();
                
                // Configure the ColorableObject based on SO settings
                colorableObject.showColorEffect = buildableObjectSO.enableColorEffects;
                colorableObject.saveColorState = buildableObjectSO.saveColorState;
                
                scriptsAdded++;
                
                if (showDebugInfo)
                    Debug.Log($"BuildableObjectColorableScriptAdder: Added ColorableObject script to {buildableObject.name}");
            }
            else
            {
                // Update existing ColorableObject settings
                existingColorableObject.showColorEffect = buildableObjectSO.enableColorEffects;
                existingColorableObject.saveColorState = buildableObjectSO.saveColorState;
                
                if (showDebugInfo)
                    Debug.Log($"BuildableObjectColorableScriptAdder: Updated existing ColorableObject script on {buildableObject.name}");
            }
            
            // Refresh any existing XR Color Manager references
            RefreshXRColorManagerReferences();
        }
        else
        {
            if (showDebugInfo)
                Debug.Log($"BuildableObjectColorableScriptAdder: Skipped {buildableObject.name} (addColorableObjectScript = false)");
        }
    }
    
    /// <summary>
    /// Get the BuildableObjectSO from a BuildableObject
    /// </summary>
    private BuildableObjectSO GetBuildableObjectSO(BuildableObject buildableObject)
    {
        // Try different types of buildable objects
        if (buildableObject is BuildableGridObject gridObject)
        {
            return GetSOFromGridObject(gridObject);
        }
        else if (buildableObject is BuildableFreeObject freeObject)
        {
            return GetSOFromFreeObject(freeObject);
        }
        else if (buildableObject is BuildableEdgeObject edgeObject)
        {
            return GetSOFromEdgeObject(edgeObject);
        }
        else if (buildableObject is BuildableCornerObject cornerObject)
        {
            return GetSOFromCornerObject(cornerObject);
        }
        
        return null;
    }
    
    /// <summary>
    /// Get SO from BuildableGridObject using reflection (since the SO field might be private)
    /// </summary>
    private BuildableObjectSO GetSOFromGridObject(BuildableGridObject gridObject)
    {
        try
        {
            // Try to get the BuildableGridObjectSO through reflection
            var field = typeof(BuildableGridObject).GetField("buildableGridObjectSO", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                return field.GetValue(gridObject) as BuildableObjectSO;
            }
            
            // Alternative: try public getter method if it exists
            var method = typeof(BuildableGridObject).GetMethod("GetBuildableGridObjectSO");
            if (method != null)
            {
                return method.Invoke(gridObject, null) as BuildableObjectSO;
            }
        }
        catch (System.Exception e)
        {
            if (showDebugInfo)
                Debug.LogWarning($"BuildableObjectColorableScriptAdder: Failed to get SO from GridObject: {e.Message}");
        }
        
        return null;
    }
    
    /// <summary>
    /// Get SO from BuildableFreeObject
    /// </summary>
    private BuildableObjectSO GetSOFromFreeObject(BuildableFreeObject freeObject)
    {
        try
        {
            var field = typeof(BuildableFreeObject).GetField("buildableFreeObjectSO", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                return field.GetValue(freeObject) as BuildableObjectSO;
            }
        }
        catch (System.Exception e)
        {
            if (showDebugInfo)
                Debug.LogWarning($"BuildableObjectColorableScriptAdder: Failed to get SO from FreeObject: {e.Message}");
        }
        
        return null;
    }
    
    /// <summary>
    /// Get SO from BuildableEdgeObject
    /// </summary>
    private BuildableObjectSO GetSOFromEdgeObject(BuildableEdgeObject edgeObject)
    {
        try
        {
            var field = typeof(BuildableEdgeObject).GetField("buildableEdgeObjectSO", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                return field.GetValue(edgeObject) as BuildableObjectSO;
            }
        }
        catch (System.Exception e)
        {
            if (showDebugInfo)
                Debug.LogWarning($"BuildableObjectColorableScriptAdder: Failed to get SO from EdgeObject: {e.Message}");
        }
        
        return null;
    }
    
    /// <summary>
    /// Get SO from BuildableCornerObject
    /// </summary>
    private BuildableObjectSO GetSOFromCornerObject(BuildableCornerObject cornerObject)
    {
        try
        {
            var field = typeof(BuildableCornerObject).GetField("buildableCornerObjectSO", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                return field.GetValue(cornerObject) as BuildableObjectSO;
            }
        }
        catch (System.Exception e)
        {
            if (showDebugInfo)
                Debug.LogWarning($"BuildableObjectColorableScriptAdder: Failed to get SO from CornerObject: {e.Message}");
        }
        
        return null;
    }
    
    /// <summary>
    /// Refresh Smart Color Manager references to newly added ColorableObjects
    /// </summary>
    private void RefreshXRColorManagerReferences()
    {
        // SmartLineColorManager works independently and doesn't need refresh
        // It handles line color based on ColorPreview Image, not direct ColorableObject references
        // ColorableObjects are detected dynamically when needed by the VR input system
        
        if (showDebugInfo)
        {
            Debug.Log("BuildableObjectColorableScriptAdder: ColorableObjects added - SmartLineColorManager will detect them automatically");
        }
    }
    
    /// <summary>
    /// Get statistics about processed objects
    /// </summary>
    public void GetStatistics(out int processed, out int added)
    {
        processed = objectsProcessed;
        added = scriptsAdded;
    }
    
    /// <summary>
    /// Reset statistics
    /// </summary>
    public void ResetStatistics()
    {
        objectsProcessed = 0;
        scriptsAdded = 0;
    }
    
    /// <summary>
    /// Manual method to add ColorableObject to all existing buildable objects
    /// Useful for retrofitting existing scenes
    /// </summary>
    [ContextMenu("Add ColorableObject to All Existing Buildables")]
    public void AddColorableObjectToAllExisting()
    {
        BuildableObject[] allBuildables = FindObjectsOfType<BuildableObject>();
        int addedCount = 0;
        
        foreach (BuildableObject buildable in allBuildables)
        {
            BuildableObjectSO so = GetBuildableObjectSO(buildable);
            
            if (so != null && so.addColorableObjectScript)
            {
                ColorableObject existing = buildable.GetComponent<ColorableObject>();
                if (existing == null)
                {
                    ColorableObject colorableObject = buildable.gameObject.AddComponent<ColorableObject>();
                    colorableObject.showColorEffect = so.enableColorEffects;
                    colorableObject.saveColorState = so.saveColorState;
                    addedCount++;
                }
            }
        }
        
        Debug.Log($"BuildableObjectColorableScriptAdder: Added ColorableObject script to {addedCount} existing buildable objects");
    }
}

/// <summary>
/// ReadOnly attribute for inspector fields
/// </summary>
public class ReadOnlyAttribute : PropertyAttribute { } 