using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Batch Sprite Converter - Voor het omzetten van meerdere prefabs naar sprites tegelijk
/// 
/// Gebruik:
/// 1. Ga naar Tools > Batch Sprite Converter
/// 2. Voeg prefabs toe aan de lijst
/// 3. Configureer instellingen
/// 4. Klik op "Convert All Prefabs"
/// </summary>
public class BatchSpriteConverter : EditorWindow
{
    [System.Serializable]
    public class PrefabEntry
    {
        public GameObject prefab;
        public string customName = "";
        public bool enabled = true;
        
        public PrefabEntry(GameObject prefab)
        {
            this.prefab = prefab;
            this.customName = prefab != null ? prefab.name : "";
        }
    }
    
    [Header("Batch Settings")]
    private List<PrefabEntry> prefabList = new List<PrefabEntry>();
    private Vector2 prefabListScrollPos;
    
    [Header("Shared Render Settings")]
    private int textureSize = 512;
    private Color backgroundColor = Color.clear;
    private LayerMask cullingMask = -1;
    
    [Header("Shared Camera Settings")]
    private Vector3 cameraPosition = new Vector3(0, 0, -3);
    private Vector3 cameraRotation = new Vector3(0, 0, 0);
    private float cameraFOV = 60f;
    private bool orthographic = true;
    private float orthographicSize = 2f;
    
    [Header("Shared Lighting Settings")]
    private bool useCustomLighting = true;
    private Color lightColor = Color.white;
    private float lightIntensity = 1f;
    private Vector3 lightDirection = new Vector3(-0.3f, -0.3f, -1f);
    
    [Header("Shared Output Settings")]
    private string outputFolder = "Assets/Generated Sprites";
    private string spritePrefix = "Sprite_";
    private TextureImporterFormat textureFormat = TextureImporterFormat.RGBA32;
    private bool generateMipMaps = false;
    
    [Header("Batch Processing")]
    private bool showProgress = true;
    private bool stopOnError = false;
    
    private Camera renderCamera;
    private GameObject lightGameObject;
    private Light renderLight;
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Batch Sprite Converter")]
    public static void ShowWindow()
    {
        BatchSpriteConverter window = GetWindow<BatchSpriteConverter>();
        window.titleContent = new GUIContent("Batch Sprite Converter");
        window.minSize = new Vector2(500, 700);
        window.Show();
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("Batch Sprite Converter", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        DrawPrefabList();
        DrawSharedSettings();
        DrawBatchControls();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawPrefabList()
    {
        GUILayout.Label("Prefabs to Convert", EditorStyles.boldLabel);
        
        // Add/Remove buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Prefab"))
        {
            prefabList.Add(new PrefabEntry(null));
        }
        
        if (GUILayout.Button("Remove Selected"))
        {
            for (int i = prefabList.Count - 1; i >= 0; i--)
            {
                if (!prefabList[i].enabled)
                {
                    prefabList.RemoveAt(i);
                }
            }
        }
        
        if (GUILayout.Button("Clear All"))
        {
            prefabList.Clear();
        }
        
        if (GUILayout.Button("Add Selected from Project"))
        {
            AddSelectedPrefabsFromProject();
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        // Prefab list
        prefabListScrollPos = EditorGUILayout.BeginScrollView(prefabListScrollPos, GUILayout.Height(200));
        
        for (int i = 0; i < prefabList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            
            // Enable checkbox
            prefabList[i].enabled = EditorGUILayout.Toggle(prefabList[i].enabled, GUILayout.Width(20));
            
            // Prefab field
            GameObject newPrefab = (GameObject)EditorGUILayout.ObjectField(
                prefabList[i].prefab, 
                typeof(GameObject), 
                false,
                GUILayout.Width(150)
            );
            
            if (newPrefab != prefabList[i].prefab)
            {
                prefabList[i].prefab = newPrefab;
                if (newPrefab != null && string.IsNullOrEmpty(prefabList[i].customName))
                {
                    prefabList[i].customName = newPrefab.name;
                }
            }
            
            // Custom name field
            GUILayout.Label("Name:", GUILayout.Width(40));
            prefabList[i].customName = EditorGUILayout.TextField(prefabList[i].customName);
            
            // Remove button
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                prefabList.RemoveAt(i);
                break;
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
        
        // Status
        int enabledCount = 0;
        int validCount = 0;
        foreach (var entry in prefabList)
        {
            if (entry.enabled)
            {
                enabledCount++;
                if (entry.prefab != null)
                    validCount++;
            }
        }
        
        EditorGUILayout.HelpBox($"Total: {prefabList.Count}, Enabled: {enabledCount}, Valid: {validCount}", MessageType.Info);
        
        GUILayout.Space(10);
    }
    
    private void DrawSharedSettings()
    {
        GUILayout.Label("Shared Settings", EditorStyles.boldLabel);
        
        // Render Settings
        GUILayout.Label("Render Settings", EditorStyles.miniBoldLabel);
        textureSize = EditorGUILayout.IntSlider("Texture Size", textureSize, 64, 2048);
        backgroundColor = EditorGUILayout.ColorField("Background Color", backgroundColor);
        cullingMask = LayerMaskField("Culling Mask", cullingMask);
        
        GUILayout.Space(5);
        
        // Camera Settings
        GUILayout.Label("Camera Settings", EditorStyles.miniBoldLabel);
        cameraPosition = EditorGUILayout.Vector3Field("Camera Position", cameraPosition);
        cameraRotation = EditorGUILayout.Vector3Field("Camera Rotation", cameraRotation);
        orthographic = EditorGUILayout.Toggle("Orthographic", orthographic);
        
        if (orthographic)
        {
            orthographicSize = EditorGUILayout.FloatField("Orthographic Size", orthographicSize);
        }
        else
        {
            cameraFOV = EditorGUILayout.Slider("Field of View", cameraFOV, 10f, 170f);
        }
        
        GUILayout.Space(5);
        
        // Lighting Settings
        GUILayout.Label("Lighting Settings", EditorStyles.miniBoldLabel);
        useCustomLighting = EditorGUILayout.Toggle("Use Custom Lighting", useCustomLighting);
        
        if (useCustomLighting)
        {
            lightColor = EditorGUILayout.ColorField("Light Color", lightColor);
            lightIntensity = EditorGUILayout.Slider("Light Intensity", lightIntensity, 0f, 3f);
            lightDirection = EditorGUILayout.Vector3Field("Light Direction", lightDirection);
        }
        
        GUILayout.Space(5);
        
        // Output Settings
        GUILayout.Label("Output Settings", EditorStyles.miniBoldLabel);
        
        EditorGUILayout.BeginHorizontal();
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                outputFolder = "Assets" + path.Substring(Application.dataPath.Length);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        spritePrefix = EditorGUILayout.TextField("Sprite Prefix", spritePrefix);
        textureFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup("Texture Format", textureFormat);
        generateMipMaps = EditorGUILayout.Toggle("Generate Mip Maps", generateMipMaps);
        
        GUILayout.Space(10);
    }
    
    private void DrawBatchControls()
    {
        GUILayout.Label("Batch Processing", EditorStyles.boldLabel);
        
        showProgress = EditorGUILayout.Toggle("Show Progress Bar", showProgress);
        stopOnError = EditorGUILayout.Toggle("Stop on Error", stopOnError);
        
        GUILayout.Space(10);
        
        // Action buttons
        EditorGUI.BeginDisabledGroup(GetValidPrefabCount() == 0);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Convert All Prefabs", GUILayout.Height(40)))
        {
            ConvertAllPrefabs();
        }
        
        if (GUILayout.Button("Test First Prefab", GUILayout.Height(40)))
        {
            TestFirstPrefab();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUI.EndDisabledGroup();
        
        if (GetValidPrefabCount() == 0)
        {
            EditorGUILayout.HelpBox("No valid prefabs to convert. Please add prefabs to the list.", MessageType.Warning);
        }
        
        // Quick settings
        GUILayout.Space(10);
        GUILayout.Label("Quick Settings", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Icon Size (128)"))
        {
            textureSize = 128;
            orthographicSize = 1f;
        }
        
        if (GUILayout.Button("UI Size (256)"))
        {
            textureSize = 256;
            orthographicSize = 1.5f;
        }
        
        if (GUILayout.Button("High Quality (512)"))
        {
            textureSize = 512;
            orthographicSize = 2f;
        }
        EditorGUILayout.EndHorizontal();
    }
    
    private void AddSelectedPrefabsFromProject()
    {
        Object[] selectedObjects = Selection.objects;
        
        foreach (Object obj in selectedObjects)
        {
            GameObject prefab = obj as GameObject;
            if (prefab != null && PrefabUtility.IsPartOfPrefabAsset(prefab))
            {
                // Check if already in list
                bool alreadyExists = false;
                foreach (var entry in prefabList)
                {
                    if (entry.prefab == prefab)
                    {
                        alreadyExists = true;
                        break;
                    }
                }
                
                if (!alreadyExists)
                {
                    prefabList.Add(new PrefabEntry(prefab));
                }
            }
        }
    }
    
    private int GetValidPrefabCount()
    {
        int count = 0;
        foreach (var entry in prefabList)
        {
            if (entry.enabled && entry.prefab != null)
                count++;
        }
        return count;
    }
    
    private void TestFirstPrefab()
    {
        PrefabEntry firstValid = null;
        foreach (var entry in prefabList)
        {
            if (entry.enabled && entry.prefab != null)
            {
                firstValid = entry;
                break;
            }
        }
        
        if (firstValid != null)
        {
            Debug.Log($"Testing conversion of: {firstValid.prefab.name}");
            
            try
            {
                Texture2D texture = RenderPrefabToTexture(firstValid.prefab);
                
                // Show in a temporary window or save as temp file
                string tempPath = "Assets/temp_preview.png";
                byte[] pngData = texture.EncodeToPNG();
                File.WriteAllBytes(tempPath, pngData);
                AssetDatabase.Refresh();
                
                Object tempSprite = AssetDatabase.LoadAssetAtPath<Texture2D>(tempPath);
                EditorGUIUtility.PingObject(tempSprite);
                
                DestroyImmediate(texture);
                
                Debug.Log("Test successful! Check the temp_preview.png in Assets folder.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Test failed: {e.Message}");
            }
        }
    }
    
    private void ConvertAllPrefabs()
    {
        List<PrefabEntry> validPrefabs = new List<PrefabEntry>();
        
        foreach (var entry in prefabList)
        {
            if (entry.enabled && entry.prefab != null)
            {
                validPrefabs.Add(entry);
            }
        }
        
        if (validPrefabs.Count == 0)
        {
            EditorUtility.DisplayDialog("No Prefabs", "No valid prefabs to convert.", "OK");
            return;
        }
        
        // Create output directory
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
            AssetDatabase.Refresh();
        }
        
        int successCount = 0;
        int errorCount = 0;
        
        try
        {
            for (int i = 0; i < validPrefabs.Count; i++)
            {
                var entry = validPrefabs[i];
                
                if (showProgress)
                {
                    float progress = (float)i / validPrefabs.Count;
                    if (EditorUtility.DisplayCancelableProgressBar(
                        "Converting Prefabs", 
                        $"Processing {entry.prefab.name} ({i + 1}/{validPrefabs.Count})", 
                        progress))
                    {
                        break;
                    }
                }
                
                try
                {
                    ConvertSinglePrefab(entry);
                    successCount++;
                    Debug.Log($"Converted: {entry.prefab.name}");
                }
                catch (System.Exception e)
                {
                    errorCount++;
                    Debug.LogError($"Failed to convert {entry.prefab.name}: {e.Message}");
                    
                    if (stopOnError)
                    {
                        break;
                    }
                }
            }
        }
        finally
        {
            if (showProgress)
            {
                EditorUtility.ClearProgressBar();
            }
        }
        
        // Show results
        string message = $"Conversion completed!\n\nSuccess: {successCount}\nErrors: {errorCount}";
        EditorUtility.DisplayDialog("Batch Conversion Complete", message, "OK");
        
        // Refresh project
        AssetDatabase.Refresh();
    }
    
    private void ConvertSinglePrefab(PrefabEntry entry)
    {
        // Generate texture
        Texture2D texture = RenderPrefabToTexture(entry.prefab);
        
        // Create filename
        string fileName = spritePrefix + (string.IsNullOrEmpty(entry.customName) ? entry.prefab.name : entry.customName) + ".png";
        string fullPath = Path.Combine(outputFolder, fileName);
        
        // Save texture as PNG
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(fullPath, pngData);
        
        DestroyImmediate(texture);
        
        // Configure import settings (will be applied after AssetDatabase.Refresh())
        EditorApplication.delayCall += () =>
        {
            TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.textureFormat = textureFormat;
                importer.mipmapEnabled = generateMipMaps;
                importer.SaveAndReimport();
            }
        };
    }
    
    // Reuse rendering methods from the single converter
    private Texture2D RenderPrefabToTexture(GameObject prefab)
    {
        SetupRenderScene();
        
        GameObject instance = Instantiate(prefab);
        
        try
        {
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.identity;
            
            RenderTexture renderTexture = new RenderTexture(textureSize, textureSize, 24);
            renderTexture.Create();
            
            renderCamera.targetTexture = renderTexture;
            renderCamera.transform.position = cameraPosition;
            renderCamera.transform.rotation = Quaternion.Euler(cameraRotation);
            renderCamera.backgroundColor = backgroundColor;
            renderCamera.clearFlags = CameraClearFlags.SolidColor;
            renderCamera.orthographic = orthographic;
            renderCamera.fieldOfView = cameraFOV;
            renderCamera.orthographicSize = orthographicSize;
            renderCamera.cullingMask = cullingMask;
            
            if (useCustomLighting && renderLight != null)
            {
                renderLight.color = lightColor;
                renderLight.intensity = lightIntensity;
                renderLight.transform.rotation = Quaternion.LookRotation(lightDirection);
            }
            
            renderCamera.Render();
            
            RenderTexture.active = renderTexture;
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
            texture.Apply();
            
            RenderTexture.active = null;
            renderTexture.Release();
            DestroyImmediate(renderTexture);
            
            return texture;
        }
        finally
        {
            DestroyImmediate(instance);
            CleanupRenderScene();
        }
    }
    
    private void SetupRenderScene()
    {
        GameObject cameraObject = new GameObject("BatchRenderCamera");
        renderCamera = cameraObject.AddComponent<Camera>();
        
        if (useCustomLighting)
        {
            lightGameObject = new GameObject("BatchRenderLight");
            renderLight = lightGameObject.AddComponent<Light>();
            renderLight.type = LightType.Directional;
        }
    }
    
    private void CleanupRenderScene()
    {
        if (renderCamera != null)
        {
            DestroyImmediate(renderCamera.gameObject);
            renderCamera = null;
        }
        
        if (lightGameObject != null)
        {
            DestroyImmediate(lightGameObject);
            lightGameObject = null;
            renderLight = null;
        }
    }
    
    private LayerMask LayerMaskField(string label, LayerMask layerMask)
    {
        List<string> layers = new List<string>();
        List<int> layerNumbers = new List<int>();
        
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (layerName != "")
            {
                layers.Add(layerName);
                layerNumbers.Add(i);
            }
        }
        
        int maskWithoutEmpty = 0;
        for (int i = 0; i < layerNumbers.Count; i++)
        {
            if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                maskWithoutEmpty |= (1 << i);
        }
        
        maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());
        
        int mask = 0;
        for (int i = 0; i < layerNumbers.Count; i++)
        {
            if ((maskWithoutEmpty & (1 << i)) > 0)
                mask |= (1 << layerNumbers[i]);
        }
        
        layerMask.value = mask;
        return layerMask;
    }
    
    private void OnDestroy()
    {
        CleanupRenderScene();
    }
} 