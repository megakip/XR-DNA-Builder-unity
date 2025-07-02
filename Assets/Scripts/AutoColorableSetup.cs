using UnityEngine;
using SoulGames.EasyGridBuilderPro;

/// <summary>
/// Automatically adds ColorableObject components to newly spawned objects in Easy Grid Builder Pro
/// This ensures all spawned objects can be colored by the XR Controller Color Manager
/// Attach this to any GameObject in the scene (preferably the GridManager or a dedicated setup object)
/// </summary>
public class AutoColorableSetup : MonoBehaviour
{
    [Header("Setup Settings")]
    [Tooltip("Automatically setup ColorableObject on all spawned objects")]
    public bool autoSetupNewObjects = true;
    
    [Tooltip("Also setup existing objects in the scene on Start")]
    public bool setupExistingObjects = true;
    
    [Tooltip("Layer mask for objects that should be made colorable")]
    public LayerMask colorableLayerMask = -1;
    
    [Tooltip("Show debug information")]
    public bool showDebugInfo = true;
    
    [Header("ColorableObject Settings")]
    [Tooltip("Should colorable objects save their color state?")]
    public bool saveColorState = true;
    
    [Tooltip("Should colorable objects show visual effects when colored?")]
    public bool showColorEffect = false;
    
    [Tooltip("Should colorable objects use scale effect when colored?")]
    public bool useScaleEffect = false;
    
    // Events for tracking object spawn/destroy
    private bool isListeningForEvents = false;
    
    void Start()
    {
        if (setupExistingObjects)
        {
            SetupExistingObjectsInScene();
        }
        
        if (autoSetupNewObjects)
        {
            StartListeningForNewObjects();
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"AutoColorableSetup initialized - Auto setup: {autoSetupNewObjects}, Existing objects: {setupExistingObjects}");
        }
    }
    
    /// <summary>
    /// Setup ColorableObject components on all existing objects in the scene
    /// </summary>
    void SetupExistingObjectsInScene()
    {
        // Find all BuildableObjectEffects in the scene (EGB Pro objects)
        BuildableObjectEffects[] allBuildableObjects = FindObjectsOfType<BuildableObjectEffects>();
        
        if (showDebugInfo)
        {
            Debug.Log($"AutoColorableSetup: Found {allBuildableObjects.Length} existing BuildableObjectEffects in scene");
        }
        
        foreach (BuildableObjectEffects buildableObject in allBuildableObjects)
        {
            SetupColorableObjectOn(buildableObject.gameObject);
        }
        
        // Also find objects that might have renderers but no BuildableObjectEffects
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        foreach (Renderer renderer in allRenderers)
        {
            // Skip if it already has BuildableObjectEffects (already handled above)
            if (renderer.GetComponent<BuildableObjectEffects>() != null) continue;
            
            // Skip if it already has ColorableObject
            if (renderer.GetComponent<ColorableObject>() != null) continue;
            
            // Check if it's on the correct layer
            if (((1 << renderer.gameObject.layer) & colorableLayerMask) != 0)
            {
                SetupColorableObjectOn(renderer.gameObject);
            }
        }
    }
    
    /// <summary>
    /// Start listening for new object creation
    /// </summary>
    void StartListeningForNewObjects()
    {
        if (isListeningForEvents) return;
        
        // We'll use a coroutine to periodically check for new objects
        // since EGB Pro doesn't have direct spawn events we can hook into
        StartCoroutine(CheckForNewObjectsPeriodically());
        
        isListeningForEvents = true;
        
        if (showDebugInfo)
        {
            Debug.Log("AutoColorableSetup: Started listening for new objects");
        }
    }
    
    /// <summary>
    /// Periodically check for new objects that need ColorableObject setup
    /// </summary>
    System.Collections.IEnumerator CheckForNewObjectsPeriodically()
    {
        while (autoSetupNewObjects)
        {
            // Wait before checking
            yield return new WaitForSeconds(0.5f);
            
            // Find BuildableObjectEffects without ColorableObject
            BuildableObjectEffects[] allBuildableObjects = FindObjectsOfType<BuildableObjectEffects>();
            
            foreach (BuildableObjectEffects buildableObject in allBuildableObjects)
            {
                if (buildableObject.GetComponent<ColorableObject>() == null)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"AutoColorableSetup: Found new object without ColorableObject: {buildableObject.gameObject.name}");
                    }
                    
                    SetupColorableObjectOn(buildableObject.gameObject);
                }
            }
        }
    }
    
    /// <summary>
    /// Setup ColorableObject component on a specific GameObject
    /// </summary>
    /// <param name="targetObject">The GameObject to setup</param>
    /// <returns>The added ColorableObject component</returns>
    public ColorableObject SetupColorableObjectOn(GameObject targetObject)
    {
        if (targetObject == null) return null;
        
        // Check if it already has a ColorableObject
        ColorableObject existingColorable = targetObject.GetComponent<ColorableObject>();
        if (existingColorable != null)
        {
            if (showDebugInfo)
            {
                Debug.Log($"AutoColorableSetup: {targetObject.name} already has ColorableObject");
            }
            return existingColorable;
        }
        
        // Check layer mask
        if (((1 << targetObject.layer) & colorableLayerMask) == 0)
        {
            if (showDebugInfo)
            {
                Debug.Log($"AutoColorableSetup: {targetObject.name} is not on a colorable layer, skipping");
            }
            return null;
        }
        
        // Add ColorableObject component
        ColorableObject colorableObject = targetObject.AddComponent<ColorableObject>();
        
        // Configure the ColorableObject settings
        colorableObject.saveColorState = saveColorState;
        colorableObject.showColorEffect = showColorEffect;
        colorableObject.useScaleEffect = useScaleEffect;
        
        if (showDebugInfo)
        {
            Debug.Log($"AutoColorableSetup: Added ColorableObject to {targetObject.name}");
        }
        
        return colorableObject;
    }
    
    /// <summary>
    /// Manually setup ColorableObject on all current BuildableObjectEffects in the scene
    /// </summary>
    [ContextMenu("Setup All Current Objects")]
    public void SetupAllCurrentObjects()
    {
        SetupExistingObjectsInScene();
    }
    
    /// <summary>
    /// Remove all ColorableObject components from the scene
    /// </summary>
    [ContextMenu("Remove All ColorableObjects")]
    public void RemoveAllColorableObjects()
    {
        ColorableObject[] allColorableObjects = FindObjectsOfType<ColorableObject>();
        
        if (showDebugInfo)
        {
            Debug.Log($"AutoColorableSetup: Removing {allColorableObjects.Length} ColorableObject components");
        }
        
        foreach (ColorableObject colorableObject in allColorableObjects)
        {
            if (Application.isPlaying)
            {
                Destroy(colorableObject);
            }
            else
            {
                DestroyImmediate(colorableObject);
            }
        }
    }
    
    /// <summary>
    /// Debug method to count objects and their setup status
    /// </summary>
    [ContextMenu("Debug Object Count")]
    public void DebugObjectCount()
    {
        BuildableObjectEffects[] buildableObjects = FindObjectsOfType<BuildableObjectEffects>();
        ColorableObject[] colorableObjects = FindObjectsOfType<ColorableObject>();
        
        int buildableWithColorable = 0;
        int buildableWithoutColorable = 0;
        
        foreach (BuildableObjectEffects buildable in buildableObjects)
        {
            if (buildable.GetComponent<ColorableObject>() != null)
            {
                buildableWithColorable++;
            }
            else
            {
                buildableWithoutColorable++;
            }
        }
        
        Debug.Log($"=== AutoColorableSetup Debug ===");
        Debug.Log($"BuildableObjectEffects total: {buildableObjects.Length}");
        Debug.Log($"- With ColorableObject: {buildableWithColorable}");
        Debug.Log($"- Without ColorableObject: {buildableWithoutColorable}");
        Debug.Log($"ColorableObject total: {colorableObjects.Length}");
        Debug.Log($"==============================");
    }
    
    void OnDestroy()
    {
        // Stop listening for new objects
        isListeningForEvents = false;
    }
} 