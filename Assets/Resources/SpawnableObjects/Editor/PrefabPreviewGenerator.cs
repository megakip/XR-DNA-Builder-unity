using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class PrefabPreviewGenerator : EditorWindow
{
    private string resourcesPath = "Assets/Resources";
    private string outputPath = "Assets/Resources/Previews";
    private bool includeSubfolders = true;
    private Vector2 scrollPosition;
    private List<GameObject> prefabs = new List<GameObject>();
    private bool showPrefabList = false;
    private bool useCustomRenderer = true;
    private bool useWellLitPreview = true;
    private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    private int previewSize = 256;
    private float cameraDistance = 2f;
    private float cameraAngle = 30f;
    private bool showAdvancedOptions = false;
    private bool useOriginalMaterials = true;
    private bool useTransparentBackground = true;
    private Color defaultBackgroundColor = Color.white;

    [MenuItem("Tools/Enhanced Prefab Preview Generator")]
    public static void ShowWindow()
    {
        GetWindow<PrefabPreviewGenerator>("Prefab Preview Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Enhanced Prefab Preview Generator", EditorStyles.boldLabel);

        // Input pad
        EditorGUILayout.BeginHorizontal();
        resourcesPath = EditorGUILayout.TextField("Resources Path", resourcesPath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Resources Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                // Converteer naar project-relatief pad
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                    resourcesPath = path;
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        // Output pad
        EditorGUILayout.BeginHorizontal();
        outputPath = EditorGUILayout.TextField("Output Path", outputPath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                // Converteer naar project-relatief pad
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                    outputPath = path;
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        includeSubfolders = EditorGUILayout.Toggle("Include Subfolders", includeSubfolders);
        useCustomRenderer = EditorGUILayout.Toggle("Use Custom Renderer (Better Quality)", useCustomRenderer);
        
        if (!useCustomRenderer)
        {
            EditorGUI.indentLevel++;
            useWellLitPreview = EditorGUILayout.Toggle("Use Well-Lit Preview", useWellLitPreview);
            EditorGUI.indentLevel--;
        }
        
        // Always show transparency option
        useTransparentBackground = EditorGUILayout.Toggle("Transparent Background", useTransparentBackground);
        
        if (!useTransparentBackground)
        {
            defaultBackgroundColor = EditorGUILayout.ColorField("Default Background Color", defaultBackgroundColor);
        }

        // Geavanceerde opties
        showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Advanced Options");
        if (showAdvancedOptions)
        {
            EditorGUI.indentLevel++;
            backgroundColor = EditorGUILayout.ColorField("Background Color (Custom Renderer)", backgroundColor);
            previewSize = EditorGUILayout.IntSlider("Preview Size", previewSize, 128, 1024);
            cameraDistance = EditorGUILayout.Slider("Camera Distance", cameraDistance, 0.5f, 10f);
            cameraAngle = EditorGUILayout.Slider("Camera Angle", cameraAngle, 0f, 90f);
            useOriginalMaterials = EditorGUILayout.Toggle("Use Original Materials", useOriginalMaterials);
            EditorGUI.indentLevel--;
        }

        // Zoek prefabs knop
        if (GUILayout.Button("Find Prefabs"))
        {
            FindPrefabs();
        }

        // Toon gevonden prefabs
        if (prefabs.Count > 0)
        {
            showPrefabList = EditorGUILayout.Foldout(showPrefabList, $"Found Prefabs ({prefabs.Count})");
            
            if (showPrefabList)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                for (int i = 0; i < prefabs.Count; i++)
                {
                    if (prefabs[i] == null) continue;
                    
                    EditorGUILayout.BeginHorizontal();
                    
                    // Toon preview thumbnail
                    Texture2D preview = AssetPreview.GetAssetPreview(prefabs[i]);
                    if (preview != null)
                    {
                        GUILayout.Box(preview, GUILayout.Width(64), GUILayout.Height(64));
                    }
                    else
                    {
                        GUILayout.Box("No Preview", GUILayout.Width(64), GUILayout.Height(64));
                    }
                    
                    EditorGUILayout.LabelField(prefabs[i].name);
                    
                    if (GUILayout.Button("Generate", GUILayout.Width(80)))
                    {
                        GenerateSinglePreview(prefabs[i]);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }
            
            // Genereer previews knop
            if (GUILayout.Button("Generate All Previews"))
            {
                GenerateAllPreviews();
            }
        }
    }

    private void FindPrefabs()
    {
        prefabs.Clear();
        
        if (!Directory.Exists(resourcesPath))
        {
            Debug.LogError($"Directory does not exist: {resourcesPath}");
            return;
        }
        
        string[] prefabPaths;
        
        if (includeSubfolders)
        {
            prefabPaths = Directory.GetFiles(resourcesPath, "*.prefab", SearchOption.AllDirectories);
        }
        else
        {
            prefabPaths = Directory.GetFiles(resourcesPath, "*.prefab", SearchOption.TopDirectoryOnly);
        }
        
        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                prefabs.Add(prefab);
            }
        }
        
        Debug.Log($"Found {prefabs.Count} prefabs in {resourcesPath}");
    }

    private void GenerateAllPreviews()
    {
        // Zorg ervoor dat de output map bestaat
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        
        int successCount = 0;
        
        // Bewaar de huidige scene
        string currentScenePath = EditorSceneManager.GetActiveScene().path;
        bool sceneIsDirty = EditorSceneManager.GetActiveScene().isDirty;
        
        if (sceneIsDirty)
        {
            if (EditorUtility.DisplayDialog("Save Current Scene", 
                "The current scene has unsaved changes. Do you want to save before proceeding?", 
                "Save", "Don't Save"))
            {
                EditorSceneManager.SaveOpenScenes();
            }
        }
        
        // Maak een tijdelijke scene voor het renderen van previews
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        foreach (GameObject prefab in prefabs)
        {
            if (prefab == null) continue;
            
            if (useCustomRenderer)
            {
                Texture2D customPreview = GenerateCustomPreview(prefab);
                if (customPreview != null)
                {
                    string path = $"{outputPath}/{prefab.name}_preview.png";
                    byte[] bytes = customPreview.EncodeToPNG();
                    File.WriteAllBytes(path, bytes);
                    Debug.Log($"Custom preview saved: {path}");
                    successCount++;
                }
            }
            else if (useWellLitPreview)
            {
                Texture2D customPreview = GenerateSimpleLitPreview(prefab);
                if (customPreview != null)
                {
                    string path = $"{outputPath}/{prefab.name}_preview.png";
                    byte[] bytes = customPreview.EncodeToPNG();
                    File.WriteAllBytes(path, bytes);
                    Debug.Log($"Well-lit preview saved: {path}");
                    successCount++;
                }
            }
            else
            {
                // Wacht tot de preview is geladen
                Texture2D preview = null;
                int attempts = 0;
                while (preview == null && attempts < 10)
                {
                    preview = AssetPreview.GetAssetPreview(prefab);
                    if (preview == null)
                    {
                        attempts++;
                        System.Threading.Thread.Sleep(100);
                    }
                }
                
                if (preview != null)
                {
                    // Process the preview texture for transparency if needed
                    if (useTransparentBackground)
                    {
                        preview = ProcessTextureForTransparency(preview);
                    }
                    else if (defaultBackgroundColor != Color.clear)
                    {
                        preview = SetSolidBackground(preview, defaultBackgroundColor);
                    }
                    
                    string path = $"{outputPath}/{prefab.name}_preview.png";
                    byte[] bytes = preview.EncodeToPNG();
                    File.WriteAllBytes(path, bytes);
                    Debug.Log($"Preview saved: {path}");
                    successCount++;
                }
                else
                {
                    Debug.LogWarning($"Could not generate preview for: {prefab.name}");
                }
            }
        }
        
        // Herstel de vorige scene
        if (!string.IsNullOrEmpty(currentScenePath))
        {
            EditorSceneManager.OpenScene(currentScenePath);
        }
        else
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
        }
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Preview Generation Complete", 
            $"Successfully generated {successCount} previews out of {prefabs.Count} prefabs.", "OK");
    }
    
    private void GenerateSinglePreview(GameObject prefab)
    {
        // Zorg ervoor dat de output map bestaat
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        
        // Bewaar de huidige scene
        string currentScenePath = EditorSceneManager.GetActiveScene().path;
        bool sceneIsDirty = EditorSceneManager.GetActiveScene().isDirty;
        
        if (sceneIsDirty)
        {
            if (EditorUtility.DisplayDialog("Save Current Scene", 
                "The current scene has unsaved changes. Do you want to save before proceeding?", 
                "Save", "Don't Save"))
            {
                EditorSceneManager.SaveOpenScenes();
            }
        }
        
        // Maak een tijdelijke scene voor het renderen van previews
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        if (useCustomRenderer)
        {
            Texture2D customPreview = GenerateCustomPreview(prefab);
            if (customPreview != null)
            {
                string path = $"{outputPath}/{prefab.name}_preview.png";
                byte[] bytes = customPreview.EncodeToPNG();
                File.WriteAllBytes(path, bytes);
                Debug.Log($"Custom preview saved: {path}");
            }
        }
        else if (useWellLitPreview)
        {
            Texture2D customPreview = GenerateSimpleLitPreview(prefab);
            if (customPreview != null)
            {
                string path = $"{outputPath}/{prefab.name}_preview.png";
                byte[] bytes = customPreview.EncodeToPNG();
                File.WriteAllBytes(path, bytes);
                Debug.Log($"Well-lit preview saved: {path}");
            }
        }
        else
        {
            Texture2D preview = AssetPreview.GetAssetPreview(prefab);
            if (preview != null)
            {
                // Process the preview texture for transparency if needed
                if (useTransparentBackground)
                {
                    preview = ProcessTextureForTransparency(preview);
                }
                else if (defaultBackgroundColor != Color.clear)
                {
                    preview = SetSolidBackground(preview, defaultBackgroundColor);
                }
                
                string path = $"{outputPath}/{prefab.name}_preview.png";
                byte[] bytes = preview.EncodeToPNG();
                File.WriteAllBytes(path, bytes);
                Debug.Log($"Preview saved: {path}");
            }
            else
            {
                Debug.LogWarning($"Could not generate preview for: {prefab.name}");
            }
        }
        
        // Herstel de vorige scene
        if (!string.IsNullOrEmpty(currentScenePath))
        {
            EditorSceneManager.OpenScene(currentScenePath);
        }
        else
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
        }
        
        AssetDatabase.Refresh();
    }

    private Texture2D GenerateSimpleLitPreview(GameObject prefab)
    {
        // Create a simplified well-lit preview with ambient lighting for better visibility
        GameObject cameraObj = new GameObject("PreviewCamera");
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = useTransparentBackground ? Color.clear : defaultBackgroundColor;
        camera.orthographic = false;
        
        // Main key light (front)
        GameObject keyLightObj = new GameObject("KeyLight");
        Light keyLight = keyLightObj.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.intensity = 0.7f;
        keyLight.color = Color.white;
        keyLightObj.transform.rotation = Quaternion.Euler(20, -20, 0);
        
        // Fill light (side)
        GameObject fillLightObj = new GameObject("FillLight");
        Light fillLight = fillLightObj.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.intensity = 0.5f;
        fillLight.color = new Color(0.9f, 0.9f, 1f); // Slight blue tint
        fillLightObj.transform.rotation = Quaternion.Euler(0, 60, 0);
        
        // Back light (rim light)
        GameObject backLightObj = new GameObject("BackLight");
        Light backLight = backLightObj.AddComponent<Light>();
        backLight.type = LightType.Directional;
        backLight.intensity = 0.3f;
        backLight.color = new Color(1f, 0.95f, 0.9f); // Slight warm tint
        backLightObj.transform.rotation = Quaternion.Euler(0, 180, 0);
        
        // Top light
        GameObject topLightObj = new GameObject("TopLight");
        Light topLight = topLightObj.AddComponent<Light>();
        topLight.type = LightType.Directional;
        topLight.intensity = 0.3f;
        topLight.color = Color.white;
        topLightObj.transform.rotation = Quaternion.Euler(90, 0, 0);
        
        // Set ambient light (important for even lighting)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.4f, 0.4f, 0.4f);
        
        // Instantiate the prefab
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        
        // Calculate bounds
        Bounds bounds = CalculateBounds(instance);
        
        // Position the camera
        float distance = bounds.extents.magnitude * 2.0f; // Use a fixed distance multiplier
        Vector3 cameraPos = bounds.center + Quaternion.Euler(15, 45, 0) * new Vector3(0, 0, -distance);
        camera.transform.position = cameraPos;
        camera.transform.LookAt(bounds.center);
        
        // Configure camera
        float objectSize = bounds.extents.magnitude;
        camera.fieldOfView = 30f; // Use a fixed field of view for consistency
        
        // Render to texture
        RenderTexture rt = new RenderTexture(previewSize, previewSize, 24);
        if (useTransparentBackground)
        {
            rt.format = RenderTextureFormat.ARGB32;
        }
        camera.targetTexture = rt;
        camera.Render();
        
        // Read pixels
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(previewSize, previewSize, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, previewSize, previewSize), 0, 0);
        texture.Apply();
        
        // Cleanup
        RenderTexture.active = null;
        camera.targetTexture = null;
        DestroyImmediate(rt);
        DestroyImmediate(instance);
        DestroyImmediate(cameraObj);
        DestroyImmediate(keyLightObj);
        DestroyImmediate(fillLightObj);
        DestroyImmediate(backLightObj);
        DestroyImmediate(topLightObj);
        
        return texture;
    }

    private Texture2D ProcessTextureForTransparency(Texture2D source)
    {
        // Create a readable copy of the texture
        Texture2D readableTexture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        readableTexture.SetPixels(source.GetPixels());
        readableTexture.Apply();
        
        Color[] pixels = readableTexture.GetPixels();
        
        // Detect the background color (usually the color of the corners)
        Color bgColor = DetectBackgroundColor(pixels, readableTexture.width, readableTexture.height);
        
        // Replace background color with transparent
        for (int i = 0; i < pixels.Length; i++)
        {
            // If the color is close to the background color, make it transparent
            if (ColorDistance(pixels[i], bgColor) < 0.1f)
            {
                pixels[i] = Color.clear;
            }
        }
        
        // Create a new texture with the processed pixels
        Texture2D result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        result.SetPixels(pixels);
        result.Apply();
        
        return result;
    }
    
    private Color DetectBackgroundColor(Color[] pixels, int width, int height)
    {
        // Take the average of the four corners to determine the background color
        Color topLeft = pixels[0];
        Color topRight = pixels[width - 1];
        Color bottomLeft = pixels[(height - 1) * width];
        Color bottomRight = pixels[(height * width) - 1];
        
        return (topLeft + topRight + bottomLeft + bottomRight) * 0.25f;
    }
    
    private float ColorDistance(Color a, Color b)
    {
        // Calculate the Euclidean distance between two colors
        return Mathf.Sqrt(
            (a.r - b.r) * (a.r - b.r) +
            (a.g - b.g) * (a.g - b.g) +
            (a.b - b.b) * (a.b - b.b)
        );
    }
    
    private Texture2D SetSolidBackground(Texture2D source, Color backgroundColor)
    {
        // Create a readable copy of the texture
        Texture2D readableTexture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        readableTexture.SetPixels(source.GetPixels());
        readableTexture.Apply();
        
        Color[] pixels = readableTexture.GetPixels();
        Color bgDetected = DetectBackgroundColor(pixels, readableTexture.width, readableTexture.height);
        
        // Replace detected background color with the specified background color
        for (int i = 0; i < pixels.Length; i++)
        {
            if (ColorDistance(pixels[i], bgDetected) < 0.1f)
            {
                pixels[i] = backgroundColor;
            }
        }
        
        // Create a new texture with the processed pixels
        Texture2D result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        result.SetPixels(pixels);
        result.Apply();
        
        return result;
    }

    private Texture2D GenerateCustomPreview(GameObject prefab)
    {
        // Maak een tijdelijke camera
        GameObject cameraObj = new GameObject("PreviewCamera");
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = backgroundColor;
        camera.orthographic = false;
        
        // Voeg licht toe
        GameObject lightObj = new GameObject("PreviewLight");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = Color.white;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
        
        // Voeg fill light toe
        GameObject fillLightObj = new GameObject("FillLight");
        Light fillLight = fillLightObj.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.intensity = 0.7f;
        fillLight.color = new Color(0.7f, 0.7f, 1f); // Licht blauwe tint
        fillLightObj.transform.rotation = Quaternion.Euler(340, 30, 0);
        
        // Instantieer het prefab tijdelijk
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        
        // Zorg ervoor dat alle materialen correct worden geladen
        if (useOriginalMaterials)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null)
                    {
                        // Maak een kopie van het materiaal
                        Material newMat = new Material(materials[i]);
                        
                        // Optioneel: Gebruik een standaard shader als de originele problemen geeft
                        // newMat.shader = Shader.Find("Standard");
                        
                        renderer.materials[i] = newMat;
                    }
                }
            }
        }
        
        // Bereken de bounds van het object
        Bounds bounds = CalculateBounds(instance);
        
        // Positioneer de camera
        float distance = bounds.extents.magnitude * cameraDistance;
        Vector3 cameraPos = bounds.center + Quaternion.Euler(cameraAngle, 45, 0) * new Vector3(0, 0, -distance);
        camera.transform.position = cameraPos;
        camera.transform.LookAt(bounds.center);
        
        // Stel de camera in om het object goed te kadreren
        float objectSize = bounds.extents.magnitude;
        camera.fieldOfView = 2.0f * Mathf.Atan(objectSize / distance) * Mathf.Rad2Deg;
        
        // Pas de lichtintensiteit aan op basis van de grootte van het object
        light.intensity = Mathf.Clamp(1.2f * (1f / objectSize), 0.8f, 2.0f);
        
        // Render naar texture
        RenderTexture rt = new RenderTexture(previewSize, previewSize, 24);
        camera.targetTexture = rt;
        camera.Render();
        
        // Lees de pixels
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(previewSize, previewSize, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, previewSize, previewSize), 0, 0);
        texture.Apply();
        
        // Cleanup
        RenderTexture.active = null;
        camera.targetTexture = null;
        DestroyImmediate(rt);
        DestroyImmediate(instance);
        DestroyImmediate(cameraObj);
        DestroyImmediate(lightObj);
        DestroyImmediate(fillLightObj);
        
        return texture;
    }

    private Bounds CalculateBounds(GameObject obj)
    {
        Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        
        if (renderers.Length > 0)
        {
            // Initialiseer met de eerste renderer
            bounds = renderers[0].bounds;
            
            // Voeg alle andere renderers toe
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }
        else
        {
            // Als er geen renderers zijn, gebruik een standaard grootte
            bounds.center = obj.transform.position;
            bounds.extents = new Vector3(1, 1, 1);
        }
        
        return bounds;
    }

    private void GenerateMultiAnglePreviews(GameObject prefab)
    {
        // Hoeken om te renderen
        Vector3[] angles = new Vector3[] {
            new Vector3(30, 45, 0),   // Standaard isometrisch
            new Vector3(0, 0, 0),     // Front
            new Vector3(0, 90, 0),    // Side
            new Vector3(90, 0, 0)      // Top
        };
        
        string[] suffixes = new string[] { "_iso", "_front", "_side", "_top" };
        
        for (int i = 0; i < angles.Length; i++)
        {
            Texture2D preview = GenerateCustomPreviewWithAngle(prefab, angles[i]);
            if (preview != null)
            {
                string path = $"{outputPath}/{prefab.name}{suffixes[i]}.png";
                byte[] bytes = preview.EncodeToPNG();
                File.WriteAllBytes(path, bytes);
            }
        }
    }

    private Texture2D GenerateCustomPreviewWithAngle(GameObject prefab, Vector3 cameraRotation)
    {
        // Kopie van GenerateCustomPreview maar met aangepaste cameraRotation
        // ...
        return null; // Placeholder return, actual implementation needed
    }
}
