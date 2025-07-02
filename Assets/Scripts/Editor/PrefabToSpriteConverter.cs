using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Unity Editor Tool om 3D Prefabs om te zetten naar 2D Sprites
/// 
/// Gebruik:
/// 1. Ga naar Tools > Prefab To Sprite Converter
/// 2. Sleep een prefab in het Prefab veld
/// 3. Configureer de instellingen
/// 4. Klik op "Generate Sprite"
/// 5. De sprite wordt opgeslagen in Assets/Generated Sprites/
/// </summary>
public class PrefabToSpriteConverter : EditorWindow
{
    [Header("Prefab Settings")]
    private GameObject prefabToConvert;
    
    [Header("Render Settings")]
    private int textureSize = 512;
    private Color backgroundColor = Color.clear;
    private LayerMask cullingMask = -1;
    
    [Header("Camera Settings")]
    private Vector3 cameraPosition = new Vector3(0, 0, -3);
    private Vector3 cameraRotation = new Vector3(0, 0, 0);
    private float cameraFOV = 60f;
    private bool orthographic = true;
    private float orthographicSize = 2f;
    private bool autoFrameObject = true;
    private float framePadding = 0.2f;
    
    [Header("Lighting Settings")]
    private bool useCustomLighting = true;
    private Color lightColor = Color.white;
    private float lightIntensity = 1f;
    private Vector3 lightDirection = new Vector3(-0.3f, -0.3f, -1f);
    
    [Header("Output Settings")]
    private string outputFolder = "Assets/Generated Sprites";
    private string spritePrefix = "Sprite_";
    private TextureImporterFormat textureFormat = TextureImporterFormat.RGBA32;
    private bool generateMipMaps = false;
    
    [Header("Preview")]
    private Texture2D previewTexture;
    private Vector2 scrollPosition;
    
    private Camera renderCamera;
    private GameObject lightGameObject;
    private Light renderLight;
    
    [MenuItem("Tools/Prefab To Sprite Converter")]
    public static void ShowWindow()
    {
        PrefabToSpriteConverter window = GetWindow<PrefabToSpriteConverter>();
        window.titleContent = new GUIContent("Prefab to Sprite");
        window.minSize = new Vector2(400, 600);
        window.Show();
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("Prefab to Sprite Converter", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        DrawPrefabSettings();
        DrawRenderSettings();
        DrawCameraSettings();
        DrawLightingSettings();
        DrawOutputSettings();
        DrawPreviewSection();
        DrawActionButtons();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawPrefabSettings()
    {
        GUILayout.Label("Prefab Settings", EditorStyles.boldLabel);
        
        GameObject newPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Prefab to Convert", 
            prefabToConvert, 
            typeof(GameObject), 
            false
        );
        
        if (newPrefab != prefabToConvert)
        {
            prefabToConvert = newPrefab;
            // Clear preview when prefab changes
            if (previewTexture != null)
            {
                DestroyImmediate(previewTexture);
                previewTexture = null;
            }
        }
        
        GUILayout.Space(10);
    }
    
    private void DrawRenderSettings()
    {
        GUILayout.Label("Render Settings", EditorStyles.boldLabel);
        
        textureSize = EditorGUILayout.IntSlider("Texture Size", textureSize, 64, 2048);
        backgroundColor = EditorGUILayout.ColorField("Background Color", backgroundColor);
        cullingMask = LayerMaskField("Culling Mask", cullingMask);
        
        GUILayout.Space(10);
    }
    
    private void DrawCameraSettings()
    {
        GUILayout.Label("Camera Settings", EditorStyles.boldLabel);
        
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
        
        autoFrameObject = EditorGUILayout.Toggle("Auto Frame Object", autoFrameObject);
        if (autoFrameObject)
        {
            framePadding = EditorGUILayout.Slider("Frame Padding", framePadding, 0f, 1f);
        }
        
        GUILayout.Space(10);
    }
    
    private void DrawLightingSettings()
    {
        GUILayout.Label("Lighting Settings", EditorStyles.boldLabel);
        
        useCustomLighting = EditorGUILayout.Toggle("Use Custom Lighting", useCustomLighting);
        
        if (useCustomLighting)
        {
            lightColor = EditorGUILayout.ColorField("Light Color", lightColor);
            lightIntensity = EditorGUILayout.Slider("Light Intensity", lightIntensity, 0f, 3f);
            lightDirection = EditorGUILayout.Vector3Field("Light Direction", lightDirection);
        }
        
        GUILayout.Space(10);
    }
    
    private void DrawOutputSettings()
    {
        GUILayout.Label("Output Settings", EditorStyles.boldLabel);
        
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
    
    private void DrawPreviewSection()
    {
        GUILayout.Label("Preview", EditorStyles.boldLabel);
        
        if (previewTexture != null)
        {
            // Calculate preview size while maintaining aspect ratio
            float maxSize = 200f;
            float aspectRatio = (float)previewTexture.width / previewTexture.height;
            float previewWidth = Mathf.Min(maxSize, previewTexture.width);
            float previewHeight = previewWidth / aspectRatio;
            
            Rect previewRect = GUILayoutUtility.GetRect(previewWidth, previewHeight);
            EditorGUI.DrawPreviewTexture(previewRect, previewTexture);
            
            GUILayout.Label($"Preview Size: {previewTexture.width}x{previewTexture.height}", EditorStyles.miniLabel);
        }
        else
        {
            GUILayout.Label("No preview available. Generate preview or sprite to see result.", EditorStyles.helpBox);
        }
        
        GUILayout.Space(10);
    }
    
    private void DrawActionButtons()
    {
        EditorGUI.BeginDisabledGroup(prefabToConvert == null);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Generate Preview", GUILayout.Height(30)))
        {
            GeneratePreview();
        }
        
        if (GUILayout.Button("Generate Sprite", GUILayout.Height(30)))
        {
            GenerateSprite();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset Camera Settings"))
        {
            ResetCameraSettings();
        }
        
        if (GUILayout.Button("Inspector-like Settings"))
        {
            SetInspectorLikeSettings();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUI.EndDisabledGroup();
        
        if (prefabToConvert == null)
        {
            EditorGUILayout.HelpBox("Please assign a prefab to convert.", MessageType.Info);
        }
    }
    
    private void GeneratePreview()
    {
        if (prefabToConvert == null)
        {
            Debug.LogError("No prefab assigned!");
            return;
        }
        
        Texture2D texture = RenderPrefabToTexture(prefabToConvert);
        
        if (previewTexture != null)
        {
            DestroyImmediate(previewTexture);
        }
        
        previewTexture = texture;
        Repaint();
    }
    
    private void GenerateSprite()
    {
        if (prefabToConvert == null)
        {
            Debug.LogError("No prefab assigned!");
            return;
        }
        
        // Create output directory if it doesn't exist
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
            AssetDatabase.Refresh();
        }
        
        // Generate texture
        Texture2D texture = RenderPrefabToTexture(prefabToConvert);
        
        // Create filename
        string prefabName = prefabToConvert.name;
        string fileName = spritePrefix + prefabName + ".png";
        string fullPath = Path.Combine(outputFolder, fileName);
        
        // Save texture as PNG
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(fullPath, pngData);
        
        // Refresh asset database
        AssetDatabase.Refresh();
        
        // Configure texture import settings
        TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.textureFormat = textureFormat;
            importer.mipmapEnabled = generateMipMaps;
            importer.SaveAndReimport();
        }
        
        // Update preview
        if (previewTexture != null)
        {
            DestroyImmediate(previewTexture);
        }
        previewTexture = DuplicateTexture(texture);
        
        DestroyImmediate(texture);
        
        // Show success message
        EditorUtility.DisplayDialog(
            "Sprite Generated", 
            $"Sprite successfully generated at:\n{fullPath}", 
            "OK"
        );
        
        // Ping the created asset
        Object createdSprite = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
        EditorGUIUtility.PingObject(createdSprite);
        
        Debug.Log($"Sprite generated: {fullPath}");
    }
    
    private Texture2D RenderPrefabToTexture(GameObject prefab)
    {
        // Create temporary isolated scene objects
        SetupIsolatedRenderScene();
        
        // Instantiate prefab in isolated environment
        GameObject instance = Instantiate(prefab);
        
        try
        {
            // Position prefab at origin
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.identity;
            
            // Put object on isolated layer
            SetLayerRecursively(instance, LayerMask.NameToLayer("Default"));
            
            // Auto-frame the object if enabled
            if (autoFrameObject)
            {
                AutoFrameObject(instance);
            }
            
            // Setup render texture
            RenderTexture renderTexture = new RenderTexture(textureSize, textureSize, 24);
            renderTexture.Create();
            
            // Setup camera for isolated rendering
            renderCamera.targetTexture = renderTexture;
            renderCamera.backgroundColor = backgroundColor;
            renderCamera.clearFlags = CameraClearFlags.SolidColor;
            renderCamera.orthographic = orthographic;
            renderCamera.fieldOfView = cameraFOV;
            renderCamera.orthographicSize = orthographicSize;
            
            // Only render the object layer (exclude scene objects)
            renderCamera.cullingMask = 1 << LayerMask.NameToLayer("Default");
            
            // Position camera
            if (!autoFrameObject)
            {
                renderCamera.transform.position = cameraPosition;
                renderCamera.transform.rotation = Quaternion.Euler(cameraRotation);
            }
            
            // Setup lighting
            if (useCustomLighting && renderLight != null)
            {
                renderLight.color = lightColor;
                renderLight.intensity = lightIntensity;
                renderLight.transform.rotation = Quaternion.LookRotation(lightDirection);
            }
            
            // Render only our object
            renderCamera.Render();
            
            // Read pixels from render texture
            RenderTexture.active = renderTexture;
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
            texture.Apply();
            
            // Cleanup
            RenderTexture.active = null;
            renderTexture.Release();
            DestroyImmediate(renderTexture);
            
            return texture;
        }
        finally
        {
            // Always cleanup
            DestroyImmediate(instance);
            CleanupRenderScene();
        }
    }
    
    private void SetupIsolatedRenderScene()
    {
        // Create camera in isolated environment
        GameObject cameraObject = new GameObject("IsolatedRenderCamera");
        renderCamera = cameraObject.AddComponent<Camera>();
        
        // Move camera to isolated position to avoid scene interference
        renderCamera.transform.position = new Vector3(1000, 1000, 1000);
        
        // Create light if using custom lighting
        if (useCustomLighting)
        {
            lightGameObject = new GameObject("IsolatedRenderLight");
            lightGameObject.transform.position = new Vector3(1000, 1000, 1000);
            renderLight = lightGameObject.AddComponent<Light>();
            renderLight.type = LightType.Directional;
        }
    }
    
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
    
    private void AutoFrameObject(GameObject obj)
    {
        // Move object to isolated space first
        Vector3 isolatedPosition = new Vector3(1000, 1000, 1000);
        obj.transform.position = isolatedPosition;
        
        // Calculate bounds of the object at isolated position
        Bounds bounds = CalculateObjectBounds(obj);
        
        if (bounds.size == Vector3.zero)
        {
            // If no bounds, use default positioning but in isolated space
            renderCamera.transform.position = isolatedPosition + cameraPosition;
            renderCamera.transform.rotation = Quaternion.Euler(cameraRotation);
            renderCamera.transform.LookAt(isolatedPosition);
            return;
        }
        
        // Use Unity Inspector-like camera setup (similar to how Unity does prefab previews)
        Vector3 objectCenter = bounds.center;
        float objectSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        
        // Use a 3/4 view angle like Unity Inspector (30 degrees from front, 15 degrees from side)
        Vector3 cameraDirection = new Vector3(0.3f, 0.3f, 1f).normalized;
        
        if (orthographic)
        {
            // For orthographic: position camera far enough and adjust size
            float cameraDistance = objectSize * 10f; // Far enough to avoid clipping
            renderCamera.transform.position = objectCenter - (cameraDirection * cameraDistance);
            renderCamera.orthographicSize = objectSize * (0.6f + framePadding);
        }
        else
        {
            // For perspective: calculate distance based on FOV
            float halfFOV = renderCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            float cameraDistance = (objectSize * (1f + framePadding)) / (2f * Mathf.Tan(halfFOV));
            renderCamera.transform.position = objectCenter - (cameraDirection * cameraDistance);
        }
        
        // Always look at object center
        renderCamera.transform.LookAt(objectCenter);
    }
    
    private Bounds CalculateObjectBounds(GameObject obj)
    {
        Bounds bounds = new Bounds();
        bool hasBounds = false;
        
        // Get all renderers in the object
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }
        
        return bounds;
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
    
    private void ResetCameraSettings()
    {
        cameraPosition = new Vector3(0, 0, -3);
        cameraRotation = new Vector3(0, 0, 0);
        cameraFOV = 60f;
        orthographic = true;
        orthographicSize = 2f;
        autoFrameObject = false;
        framePadding = 0.2f;
    }
    
    private void SetInspectorLikeSettings()
    {
        // Unity Inspector-like settings for perfect prefab previews
        textureSize = 512;
        backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0f); // Slightly gray transparent
        orthographic = true;
        orthographicSize = 1f;
        autoFrameObject = true;
        framePadding = 0.1f;
        
        // Lighting similar to Unity Inspector
        useCustomLighting = true;
        lightColor = Color.white;
        lightIntensity = 1.2f;
        lightDirection = new Vector3(-0.2f, -0.3f, -1f);
        
        Debug.Log("Applied Unity Inspector-like settings for optimal prefab preview!");
    }
    
    private Texture2D DuplicateTexture(Texture2D source)
    {
        Texture2D duplicate = new Texture2D(source.width, source.height, source.format, false);
        duplicate.SetPixels(source.GetPixels());
        duplicate.Apply();
        return duplicate;
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
        if (previewTexture != null)
        {
            DestroyImmediate(previewTexture);
        }
        
        CleanupRenderScene();
    }
}

 