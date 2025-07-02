using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Demo script to help set up and test the XR Color System
/// This script can automatically configure the SmartLineColorManager
/// </summary>
public class XRColorSystemDemo : MonoBehaviour
{
    [Header("Auto Setup")]
    [Tooltip("Automatically find and setup XR components on Start")]
    public bool autoSetup = true;
    
    [Tooltip("Create test objects if none exist")]
    public bool createTestObjects = true;
    
    [Tooltip("Number of test objects to create")]
    public int testObjectCount = 5;
    
    [Header("Test Objects")]
    [Tooltip("Prefab for test objects (leave empty to create primitive cubes)")]
    public GameObject testObjectPrefab;
    
    [Tooltip("Material for test objects")]
    public Material testObjectMaterial;
    
    [Header("Color System References")]
    [Tooltip("Smart Line Color Manager (will be found automatically if not assigned)")]
    public SmartLineColorManager smartColorManager;
    
    [Tooltip("Color picker references (will be found automatically if not assigned)")]
    public MonoBehaviour[] colorPickers;
    
    void Start()
    {
        if (autoSetup)
        {
            AutoSetupColorSystem();
        }
        
        if (createTestObjects)
        {
            CreateTestObjects();
        }
    }
    
    void AutoSetupColorSystem()
    {
        Debug.Log("XRColorSystemDemo: Starting auto setup...");
        
        // Find SmartLineColorManager
        if (smartColorManager == null)
        {
            smartColorManager = FindObjectOfType<SmartLineColorManager>();
            if (smartColorManager == null)
            {
                Debug.LogWarning("XRColorSystemDemo: No SmartLineColorManager found in scene. Please add one to a GameObject.");
                return;
            }
        }
        
        // Find color pickers
        if (colorPickers == null || colorPickers.Length == 0)
        {
            FindColorPickers();
        }
        
        // Trigger smart setup on the color manager
        if (smartColorManager != null)
        {
            smartColorManager.SetupSmartLineColorSystem();
        }
        
        // Setup EGB Pro integration
        SetupEGBProIntegration();
        
        Debug.Log("XRColorSystemDemo: Auto setup complete!");
    }
    
    void FindColorPickers()
    {
        var foundPickers = new System.Collections.Generic.List<MonoBehaviour>();
        
        // Find HSVColorPicker
        HSVColorPicker hsvPicker = FindObjectOfType<HSVColorPicker>();
        if (hsvPicker != null) foundPickers.Add(hsvPicker);
        
        // Find SimpleColorPicker
        SimpleColorPicker simplePicker = FindObjectOfType<SimpleColorPicker>();
        if (simplePicker != null) foundPickers.Add(simplePicker);
        
        // Find SpectrumColorPicker
        SpectrumColorPicker spectrumPicker = FindObjectOfType<SpectrumColorPicker>();
        if (spectrumPicker != null) foundPickers.Add(spectrumPicker);
        
        colorPickers = foundPickers.ToArray();
        
        Debug.Log($"XRColorSystemDemo: Found {colorPickers.Length} color pickers");
    }
    
    // SetupXRComponents is no longer needed - SmartLineColorManager handles this automatically
    
    void SetupEGBProIntegration()
    {
        // Try to find BuildableObjectColorableScriptAdder using reflection to avoid compile errors
        var adderType = System.Type.GetType("BuildableObjectColorableScriptAdder");
        if (adderType == null)
        {
            Debug.Log("XRColorSystemDemo: BuildableObjectColorableScriptAdder not found - EGB Pro integration will be manual");
            return;
        }
        
        // Check if an instance exists in scene
        var existingAdder = FindObjectOfType(adderType);
        
        if (existingAdder == null)
        {
            // Create a new GameObject for the EGB Pro integration
            GameObject egbProObject = new GameObject("EGB Pro Color Integration");
            var newAdder = egbProObject.AddComponent(adderType);
            
            // Configure the component using reflection
            var autoStartField = adderType.GetField("autoStart");
            if (autoStartField != null)
            {
                autoStartField.SetValue(newAdder, true);
            }
            
            // Call StartListening method
            var startListeningMethod = adderType.GetMethod("StartListening");
            if (startListeningMethod != null)
            {
                startListeningMethod.Invoke(newAdder, null);
            }
            
            Debug.Log("XRColorSystemDemo: Created BuildableObjectColorableScriptAdder for automatic EGB Pro integration");
        }
        else
        {
            Debug.Log("XRColorSystemDemo: Found existing BuildableObjectColorableScriptAdder");
            
            // Ensure it's enabled and configured
            var component = existingAdder as MonoBehaviour;
            if (component != null)
            {
                component.enabled = true;
                
                var autoStartField = adderType.GetField("autoStart");
                if (autoStartField != null)
                {
                    bool autoStart = (bool)autoStartField.GetValue(existingAdder);
                    if (!autoStart)
                    {
                        autoStartField.SetValue(existingAdder, true);
                        
                        var startListeningMethod = adderType.GetMethod("StartListening");
                        if (startListeningMethod != null)
                        {
                            startListeningMethod.Invoke(existingAdder, null);
                        }
                    }
                }
            }
        }
        
        Debug.Log("XRColorSystemDemo: EGB Pro integration setup complete");
    }
    
    void CreateTestObjects()
    {
        Debug.Log("XRColorSystemDemo: Creating test objects...");
        
        for (int i = 0; i < testObjectCount; i++)
        {
            GameObject testObj;
            
            if (testObjectPrefab != null)
            {
                testObj = Instantiate(testObjectPrefab);
            }
            else
            {
                // Create primitive cube
                testObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            }
            
            // Position objects in a circle
            float angle = (360f / testObjectCount) * i;
            float radius = 3f;
            Vector3 position = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                1f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
            );
            
            testObj.transform.position = position;
            testObj.name = $"TestObject_{i + 1}";
            
            // Apply test material if available
            if (testObjectMaterial != null)
            {
                Renderer renderer = testObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = testObjectMaterial;
                }
            }
            
            // Add ColorableObject component
            ColorableObject colorableComponent = testObj.GetComponent<ColorableObject>();
            if (colorableComponent == null)
            {
                colorableComponent = testObj.AddComponent<ColorableObject>();
            }
            
            // Ensure object is on a layer that can be colored
            testObj.layer = 0; // Default layer
            
            Debug.Log($"XRColorSystemDemo: Created test object {testObj.name} at {position}");
        }
        
        Debug.Log($"XRColorSystemDemo: Created {testObjectCount} test objects");
    }
    
    [ContextMenu("Test Color Change (Red)")]
    public void TestColorRed()
    {
        if (smartColorManager != null)
        {
            smartColorManager.TestLineColorRed();
            Debug.Log("XRColorSystemDemo: Set line color to red");
        }
    }
    
    [ContextMenu("Test Color Change (Blue)")]
    public void TestColorBlue()
    {
        if (smartColorManager != null)
        {
            smartColorManager.TestLineColorBlue();
            Debug.Log("XRColorSystemDemo: Set line color to blue");
        }
    }
    
    [ContextMenu("Test Color Change (Green)")]
    public void TestColorGreen()
    {
        if (smartColorManager != null)
        {
            smartColorManager.TestLineColorGreen();
            Debug.Log("XRColorSystemDemo: Set line color to green");
        }
    }
    
    [ContextMenu("Reset All Test Objects")]
    public void ResetAllTestObjects()
    {
        ColorableObject[] colorableObjects = FindObjectsOfType<ColorableObject>();
        foreach (var obj in colorableObjects)
        {
                            obj.ResetColor();
        }
        Debug.Log($"XRColorSystemDemo: Reset {colorableObjects.Length} colorable objects");
    }
    
    [ContextMenu("Show System Status")]
    public void ShowSystemStatus()
    {
        Debug.Log("=== XR Color System Status ===");
        
        if (smartColorManager != null)
        {
            Debug.Log($"Smart Color Manager: {smartColorManager.name}");
            Debug.Log($"XR Origin: {smartColorManager.xrOrigin?.name ?? "None"}");
            Debug.Log($"Line Renderer: {smartColorManager.targetLineRenderer?.name ?? "None"}");
            Debug.Log($"Color Preview Image: {smartColorManager.colorPreviewImage?.name ?? "None"}");
        }
        else
        {
            Debug.Log("Smart Color Manager: Not found");
        }
        
        ColorableObject[] colorableObjects = FindObjectsOfType<ColorableObject>();
        Debug.Log($"Colorable Objects in scene: {colorableObjects.Length}");
        
        HSVColorPicker hsvPicker = FindObjectOfType<HSVColorPicker>();
        SimpleColorPicker simplePicker = FindObjectOfType<SimpleColorPicker>();
        SpectrumColorPicker spectrumPicker = FindObjectOfType<SpectrumColorPicker>();
        
        Debug.Log($"HSV Color Picker: {(hsvPicker != null ? "Found" : "Not found")}");
        Debug.Log($"Simple Color Picker: {(simplePicker != null ? "Found" : "Not found")}");
        Debug.Log($"Spectrum Color Picker: {(spectrumPicker != null ? "Found" : "Not found")}");
        
        Debug.Log("==============================");
    }
} 