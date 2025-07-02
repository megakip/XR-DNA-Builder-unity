using UnityEngine;
using SoulGames.EasyGridBuilderPro;

/// <summary>
/// Debug script to analyze exactly what's happening with ColorableObject materials
/// </summary>
public class ColorableObjectDebugger : MonoBehaviour
{
    [Header("Target")]
    public ColorableObject targetColorableObject;
    
    [Header("Debug Info")]
    [TextArea(10, 20)]
    public string debugOutput = "Click 'Analyze Object' to see debug info";
    
    void Start()
    {
        if (targetColorableObject == null)
        {
            targetColorableObject = GetComponent<ColorableObject>();
        }
    }
    
    [ContextMenu("Analyze Object")]
    public void AnalyzeObject()
    {
        if (targetColorableObject == null)
        {
            debugOutput = "No ColorableObject assigned!";
            return;
        }
        
        string output = "=== COLORABLE OBJECT ANALYSIS ===\n\n";
        
        // Basic info
        output += $"Object: {targetColorableObject.gameObject.name}\n";
        output += $"Original Color: {targetColorableObject.originalColor}\n";
        output += $"Current Color: {targetColorableObject.currentColor}\n\n";
        
        // Renderer analysis
        Renderer[] renderers = targetColorableObject.GetComponentsInChildren<Renderer>();
        output += $"RENDERERS FOUND: {renderers.Length}\n";
        
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            output += $"\nRenderer {i}: {r.gameObject.name}\n";
            output += $"  Path: {GetPath(r.gameObject)}\n";
            output += $"  Active: {r.gameObject.activeInHierarchy}\n";
            output += $"  Enabled: {r.enabled}\n";
            output += $"  Materials Count: {r.materials.Length}\n";
            
            for (int j = 0; j < r.materials.Length; j++)
            {
                Material mat = r.materials[j];
                if (mat != null)
                {
                    output += $"    Mat {j}: {mat.name}\n";
                    output += $"      Color: {mat.color}\n";
                    output += $"      Shader: {mat.shader.name}\n";
                    output += $"      Instance ID: {mat.GetInstanceID()}\n";
                    
                    // Check common color properties
                    if (mat.HasProperty("_Color"))
                        output += $"      _Color: {mat.GetColor("_Color")}\n";
                    if (mat.HasProperty("_BaseColor"))
                        output += $"      _BaseColor: {mat.GetColor("_BaseColor")}\n";
                }
                else
                {
                    output += $"    Mat {j}: NULL\n";
                }
            }
        }
        
        // BuildableObjectEffects analysis
        BuildableObjectEffects effects = targetColorableObject.GetComponent<BuildableObjectEffects>();
        output += $"\nBUILDABLE OBJECT EFFECTS:\n";
        if (effects != null)
        {
            output += $"  Present: YES\n";
            
            // Use reflection to check internal state
            var changeObjectMaterialField = typeof(BuildableObjectEffects).GetField("changeObjectMaterial", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (changeObjectMaterialField != null)
            {
                bool changeObjectMaterial = (bool)changeObjectMaterialField.GetValue(effects);
                output += $"  changeObjectMaterial: {changeObjectMaterial}\n";
            }
            
            var baseMaterialField = typeof(BuildableObjectEffects).GetField("baseMaterial", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (baseMaterialField != null)
            {
                Material baseMaterial = baseMaterialField.GetValue(effects) as Material;
                if (baseMaterial != null)
                {
                    output += $"  baseMaterial: {baseMaterial.name}\n";
                    output += $"  baseMaterial Color: {baseMaterial.color}\n";
                }
                else
                {
                    output += $"  baseMaterial: NULL\n";
                }
            }
        }
        else
        {
            output += $"  Present: NO\n";
        }
        
        // ColorableObject internal state
        output += $"\nCOLORABLE OBJECT STATE:\n";
        output += $"  originalBaseMaterial: {(targetColorableObject.originalBaseMaterial?.name ?? "NULL")}\n";
        output += $"  coloredBaseMaterial: {(targetColorableObject.coloredBaseMaterial?.name ?? "NULL")}\n";
        
        debugOutput = output;
        Debug.Log(output);
    }
    
    [ContextMenu("Force Color Change Red")]
    public void ForceColorChangeRed()
    {
        if (targetColorableObject != null)
        {
            Debug.Log("=== FORCING COLOR CHANGE TO RED ===");
            targetColorableObject.ChangeColor(Color.red);
            
            // Re-analyze after color change
            Invoke(nameof(AnalyzeObject), 0.1f);
        }
    }
    
    [ContextMenu("Force Color Change Blue")]
    public void ForceColorChangeBlue()
    {
        if (targetColorableObject != null)
        {
            Debug.Log("=== FORCING COLOR CHANGE TO BLUE ===");
            targetColorableObject.ChangeColor(Color.blue);
            
            // Re-analyze after color change
            Invoke(nameof(AnalyzeObject), 0.1f);
        }
    }
    
    [ContextMenu("Reset To Original")]
    public void ResetToOriginal()
    {
        if (targetColorableObject != null)
        {
            Debug.Log("=== RESETTING TO ORIGINAL COLOR ===");
            targetColorableObject.ResetColor();
            
            // Re-analyze after reset
            Invoke(nameof(AnalyzeObject), 0.1f);
        }
    }
    
    [ContextMenu("Manual Material Override")]
    public void ManualMaterialOverride()
    {
        if (targetColorableObject == null) return;
        
        Debug.Log("=== MANUAL MATERIAL OVERRIDE ===");
        
        Renderer[] renderers = targetColorableObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r != null)
            {
                for (int i = 0; i < r.materials.Length; i++)
                {
                    Material originalMat = r.materials[i];
                    if (originalMat != null)
                    {
                        // Create completely new material
                        Material newMat = new Material(originalMat);
                        newMat.name = $"ManualOverride_{originalMat.name}";
                        newMat.color = Color.magenta; // Very visible color
                        
                        // Apply to specific material slot
                        Material[] materials = r.materials;
                        materials[i] = newMat;
                        r.materials = materials;
                        
                        Debug.Log($"Applied manual override material to {r.gameObject.name} slot {i}");
                    }
                }
            }
        }
    }
    
    string GetPath(GameObject obj)
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
} 