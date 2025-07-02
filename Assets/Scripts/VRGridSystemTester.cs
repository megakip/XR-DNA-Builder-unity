using UnityEngine;
using UnityEngine.InputSystem;
using SoulGames.EasyGridBuilderPro;

/// <summary>
/// VR Grid System Integration Tester
/// Use this script to verify that the VR Grid System is working correctly with EGB Pro2
/// 
/// Setup Instructions:
/// 1. Attach this script to any GameObject in your scene
/// 2. Assign the grid GameObject that has VRGridGrabInteractable and EGBProCoordinateSystemSync
/// 3. Run the scene and use the keyboard shortcuts to test functionality
/// 4. Check the console for test results and debug information
/// </summary>
public class VRGridSystemTester : MonoBehaviour
{
    [Header("Test Configuration")]
    [Tooltip("The grid GameObject with VRGridGrabInteractable and EGBProCoordinateSystemSync")]
    public GameObject gridGameObject;
    
    [Tooltip("Enable automatic testing on start")]
    public bool runTestsOnStart = false;
    
    [Tooltip("Show test results in UI")]
    public bool showUIResults = true;
    
    [Header("Test Controls")]
    [Tooltip("Key to run full system test")]
    public KeyCode testKey = KeyCode.T;
    
    [Tooltip("Key to test coordinate system")]
    public KeyCode coordinateTestKey = KeyCode.C;
    
    [Tooltip("Key to display debug info")]
    public KeyCode debugInfoKey = KeyCode.D;
    
    // Component references
    private VRGridGrabInteractable vrGridGrab;
    private EGBProCoordinateSystemSync coordinateSync;
    private EasyGridBuilderProXZ gridBuilderProXZ;
    private GridManager gridManager;
    private Component rotationHandler;
    
    // Test results
    private string lastTestResults = "";
    private bool testsCompleted = false;

    #region Unity Lifecycle

    private void Start()
    {
        InitializeComponents();
        
        if (runTestsOnStart)
        {
            Invoke(nameof(RunFullSystemTest), 1f); // Delay to allow all systems to initialize
        }
        
        DisplayStartupInfo();
    }

    private void Update()
    {
        HandleInputs();
    }

    private void OnGUI()
    {
        if (showUIResults && testsCompleted)
        {
            DisplayTestResultsUI();
        }
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        if (gridGameObject == null)
        {
            // Try to auto-detect grid GameObject
            var egbProComponents = FindObjectsOfType<EasyGridBuilderProXZ>();
            if (egbProComponents.Length > 0)
            {
                gridGameObject = egbProComponents[0].gameObject;
                Debug.Log($"Auto-detected grid GameObject: {gridGameObject.name}");
            }
        }
        
        if (gridGameObject != null)
        {
            vrGridGrab = gridGameObject.GetComponent<VRGridGrabInteractable>();
            coordinateSync = gridGameObject.GetComponent<EGBProCoordinateSystemSync>();
            gridBuilderProXZ = gridGameObject.GetComponent<EasyGridBuilderProXZ>();
            rotationHandler = gridGameObject.GetComponent("EGBProGridRotationHandler");
        }
        
        gridManager = GridManager.Instance;
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();
    }

    private void DisplayStartupInfo()
    {
        Debug.Log("=== VR Grid System Tester Initialized ===");
        Debug.Log($"Grid GameObject: {(gridGameObject != null ? gridGameObject.name : "NOT FOUND")}");
        Debug.Log($"VRGridGrabInteractable: {(vrGridGrab != null ? "FOUND" : "MISSING")}");
        Debug.Log($"EGBProCoordinateSystemSync: {(coordinateSync != null ? "FOUND" : "MISSING")}");
        Debug.Log($"EGBProGridRotationHandler: {(rotationHandler != null ? "FOUND" : "MISSING")}");
        Debug.Log($"EasyGridBuilderProXZ: {(gridBuilderProXZ != null ? "FOUND" : "MISSING")}");
        Debug.Log($"GridManager: {(gridManager != null ? "FOUND" : "MISSING")}");
        Debug.Log("===========================================");
        Debug.Log($"Press '{testKey}' to run full system test");
        Debug.Log($"Press '{coordinateTestKey}' to test coordinate system");
        Debug.Log($"Press '{debugInfoKey}' to display debug information");
    }

    #endregion

    #region Input Handling

    private void HandleInputs()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current[Key.T].wasPressedThisFrame)
            {
                RunFullSystemTest();
            }
            else if (Keyboard.current[Key.C].wasPressedThisFrame)
            {
                TestCoordinateSystem();
            }
            else if (Keyboard.current[Key.D].wasPressedThisFrame)
            {
                DisplayDebugInfo();
            }
        }
    }

    #endregion

    #region Testing Methods

    public void RunFullSystemTest()
    {
        Debug.Log("=== Starting Full VR Grid System Test ===");
        
        var results = new System.Text.StringBuilder();
        results.AppendLine("VR Grid System Test Results:");
        results.AppendLine("============================");
        
        // Test 1: Component presence
        results.AppendLine("1. Component Presence Test:");
        results.AppendLine($"   - Grid GameObject: {TestResult(gridGameObject != null)}");
        results.AppendLine($"   - VRGridGrabInteractable: {TestResult(vrGridGrab != null)}");
        results.AppendLine($"   - EGBProCoordinateSystemSync: {TestResult(coordinateSync != null)}");
        results.AppendLine($"   - EGBProGridRotationHandler: {TestResult(rotationHandler != null)}");
        results.AppendLine($"   - EasyGridBuilderProXZ: {TestResult(gridBuilderProXZ != null)}");
        results.AppendLine($"   - GridManager: {TestResult(gridManager != null)}");
        
        // Test 2: Component configuration
        results.AppendLine("\n2. Component Configuration Test:");
        if (vrGridGrab != null)
        {
            results.AppendLine($"   - VR Grab Script enabled: {TestResult(vrGridGrab.enabled)}");
            results.AppendLine($"   - Movement threshold > 0: {TestResult(vrGridGrab.minimumDragDistance > 0)}");
            results.AppendLine($"   - Rotation speed > 0: {TestResult(vrGridGrab.rotationSpeed > 0)}");
        }
        
        if (coordinateSync != null)
        {
            results.AppendLine($"   - Coordinate Sync enabled: {TestResult(coordinateSync.enabled)}");
            results.AppendLine($"   - Auto sync enabled: {TestResult(coordinateSync.autoSync)}");
        }
        
        // Test 3: XR Integration
        results.AppendLine("\n3. XR Integration Test:");
        var xrGrabInteractable = gridGameObject?.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        results.AppendLine($"   - XRGrabInteractable present: {TestResult(xrGrabInteractable != null)}");
        
        // Test 4: Coordinate system test
        results.AppendLine("\n4. Coordinate System Test:");
        var coordSystemWorking = TestCoordinateSystemInternal();
        results.AppendLine($"   - Coordinate system functional: {TestResult(coordSystemWorking)}");
        
        lastTestResults = results.ToString();
        testsCompleted = true;
        
        Debug.Log(lastTestResults);
        Debug.Log("=== Full VR Grid System Test Complete ===");
    }

    private void TestCoordinateSystem()
    {
        Debug.Log("=== Testing Coordinate System ===");
        
        if (coordinateSync == null)
        {
            Debug.LogError("EGBProCoordinateSystemSync component not found!");
            return;
        }
        
        // Store original position/rotation
        Vector3 originalPos = gridGameObject.transform.position;
        Quaternion originalRot = gridGameObject.transform.rotation;
        
        // Test coordinate conversion
        Vector3 testWorldPos = originalPos + Vector3.forward * 2f;
        Vector3 localPos = coordinateSync.WorldToGridLocal(testWorldPos);
        Vector3 backToWorld = coordinateSync.GridLocalToWorld(localPos);
        
        float conversionError = Vector3.Distance(testWorldPos, backToWorld);
        bool conversionAccurate = conversionError < 0.001f;
        
        Debug.Log($"Coordinate Conversion Test:");
        Debug.Log($"- Original World Pos: {testWorldPos}");
        Debug.Log($"- Converted to Local: {localPos}");
        Debug.Log($"- Back to World: {backToWorld}");
        Debug.Log($"- Conversion Error: {conversionError:F6}");
        Debug.Log($"- Conversion Accurate: {TestResult(conversionAccurate)}");
        
        // Test matrix updates
        bool matrixChanged = coordinateSync.HasCoordinateSystemChanged();
        Debug.Log($"- Coordinate system changed: {matrixChanged}");
        
        // Force synchronization test
        coordinateSync.ForceSynchronization();
        Debug.Log("- Forced synchronization: COMPLETED");
        
        Debug.Log("=== Coordinate System Test Complete ===");
    }

    private bool TestCoordinateSystemInternal()
    {
        if (coordinateSync == null) return false;
        
        try
        {
            // Simple coordinate conversion test
            Vector3 testPos = Vector3.forward;
            Vector3 local = coordinateSync.WorldToGridLocal(testPos);
            Vector3 world = coordinateSync.GridLocalToWorld(local);
            
            return Vector3.Distance(testPos, world) < 0.001f;
        }
        catch
        {
            return false;
        }
    }

    private void DisplayDebugInfo()
    {
        Debug.Log("=== VR Grid System Debug Information ===");
        
        if (vrGridGrab != null)
        {
            Debug.Log("VR Grid Grab Debug Info:");
            Debug.Log(vrGridGrab.GetDebugInfo());
        }
        
        if (coordinateSync != null)
        {
            Debug.Log("\nCoordinate System Debug Info:");
            Debug.Log(coordinateSync.GetCoordinateSystemDebugInfo());
        }
        
        if (gridGameObject != null)
        {
            Debug.Log($"\nGrid Transform Info:");
            Debug.Log($"- Position: {gridGameObject.transform.position}");
            Debug.Log($"- Rotation: {gridGameObject.transform.eulerAngles}");
            Debug.Log($"- Scale: {gridGameObject.transform.localScale}");
        }
        
        Debug.Log("==========================================");
    }

    #endregion

    #region Helper Methods

    private string TestResult(bool passed)
    {
        return passed ? "PASS ✓" : "FAIL ✗";
    }

    private void DisplayTestResultsUI()
    {
        float panelWidth = 400f;
        float panelHeight = 300f;
        float x = Screen.width - panelWidth - 20f;
        float y = 20f;
        
        GUI.Box(new Rect(x, y, panelWidth, panelHeight), "");
        
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 10;
        style.alignment = TextAnchor.UpperLeft;
        style.wordWrap = true;
        
        GUI.Label(new Rect(x + 10f, y + 10f, panelWidth - 20f, panelHeight - 60f), lastTestResults, style);
        
        if (GUI.Button(new Rect(x + 10f, y + panelHeight - 40f, 100f, 30f), "Run Test"))
        {
            RunFullSystemTest();
        }
        
        if (GUI.Button(new Rect(x + 120f, y + panelHeight - 40f, 100f, 30f), "Hide"))
        {
            showUIResults = false;
        }
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// Programmatically run the full system test
    /// </summary>
    public void RunTest()
    {
        RunFullSystemTest();
    }

    /// <summary>
    /// Get the last test results as a string
    /// </summary>
    public string GetLastTestResults()
    {
        return lastTestResults;
    }

    /// <summary>
    /// Check if all required components are present
    /// </summary>
    public bool AreAllComponentsPresent()
    {
        return gridGameObject != null && 
               vrGridGrab != null && 
               coordinateSync != null && 
               gridBuilderProXZ != null && 
               gridManager != null;
    }

    #endregion
} 