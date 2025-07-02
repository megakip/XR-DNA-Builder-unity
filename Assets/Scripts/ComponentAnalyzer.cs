using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;
using System.Linq;

/// <summary>
/// Analyseert componenten van GameObjects en kan de volledige scene hierarchie scannen.
/// </summary>
[ExecuteInEditMode]
public class ComponentAnalyzer : MonoBehaviour
{
    [Header("Analyse Instellingen")]
    [Tooltip("Het GameObject om te analyseren")]
    public GameObject targetObject;

    [Tooltip("Controleer alle interactors in de scene")]
    public bool analyzeAllInteractors = true;

    [Tooltip("Controleer alle interactables in de scene")]
    public bool analyzeAllInteractables = true;

    [Tooltip("Start de analyse automatisch bij het starten")]
    public bool analyzeOnStart = true;

    [Header("Weergave Instellingen")]
    [Tooltip("Toon alleen actieve GameObjects bij het scannen van de hierarchie")]
    public bool onlyActiveObjects = false;

    [Tooltip("Toon details over transforms")]
    public bool showTransformDetails = true;

    [Tooltip("Toon details over colliders")]
    public bool showColliderDetails = true;

    [Tooltip("Toon details over child objecten")]
    public bool showChildDetails = true;

    private string lastAnalysisResult = "";

    void Start()
    {
        if (analyzeOnStart)
        {
            if (targetObject != null)
                AnalyzeGameObject(targetObject);
                
            if (analyzeAllInteractors)
                AnalyzeAllInteractors();
                
            if (analyzeAllInteractables)
                AnalyzeAllInteractables();
        }
    }

    /// <summary>
    /// Analyseert een specifiek GameObject en zijn componenten, inclusief alle children
    /// </summary>
    public void AnalyzeGameObject(GameObject obj)
    {
        if (obj == null) return;

        StringBuilder sb = new StringBuilder();
        AnalyzeGameObjectRecursive(obj, sb, 0);
        
        lastAnalysisResult = sb.ToString();
        Debug.Log(lastAnalysisResult);
    }

    /// <summary>
    /// Recursieve functie voor het analyseren van een GameObject en zijn children
    /// </summary>
    private void AnalyzeGameObjectRecursive(GameObject obj, StringBuilder sb, int depth)
    {
        if (obj == null) return;
        if (onlyActiveObjects && !obj.activeInHierarchy) return;

        string indent = new string(' ', depth * 2);

        // Header voor het huidige object
        sb.AppendLine($"{indent}=== OBJECT ANALYSE: {obj.name} ===");
        sb.AppendLine($"{indent}Pad: {GetGameObjectPath(obj)}");
        sb.AppendLine($"{indent}Actief: {obj.activeInHierarchy}");
        sb.AppendLine($"{indent}Layer: {LayerMask.LayerToName(obj.layer)}");
        sb.AppendLine($"{indent}Tag: {obj.tag}");

        if (showTransformDetails)
        {
            sb.AppendLine($"\n{indent}Transform:");
            sb.AppendLine($"{indent}Positie: {obj.transform.position}");
            sb.AppendLine($"{indent}Rotatie: {obj.transform.eulerAngles}");
            sb.AppendLine($"{indent}Lokale Schaal: {obj.transform.localScale}");
            sb.AppendLine($"{indent}Wereld Schaal: {obj.transform.lossyScale}");
        }

        // Componenten analyse
        sb.AppendLine($"\n{indent}Componenten:");
        var components = obj.GetComponents<Component>();
        foreach (var component in components)
        {
            if (component == null) continue;

            sb.Append($"{indent}- {component.GetType().Name}");

            // XR Interactor details
            if (component is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
            {
                sb.Append($" (isHovering: {interactor.hasHover}, isSelecting: {interactor.hasSelection})");
                if (interactor.hasHover)
                {
                    sb.Append(" Hover targets: ");
                    foreach (var target in interactor.interactablesHovered)
                    {
                        sb.Append($"{target.transform.name}, ");
                    }
                }
            }
            // XR Interactable details
            else if (component is UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable)
            {
                sb.Append($" (isSelected: {interactable.isSelected}, isHovered: {interactable.isHovered})");
            }
            // Collider details
            else if (showColliderDetails && component is Collider collider)
            {
                sb.Append($" (isTrigger: {collider.isTrigger}, enabled: {collider.enabled})");
                if (collider is BoxCollider box)
                {
                    sb.Append($", size: {box.size}, center: {box.center}");
                }
                else if (collider is SphereCollider sphere)
                {
                    sb.Append($", radius: {sphere.radius}, center: {sphere.center}");
                }
                else if (collider is CapsuleCollider capsule)
                {
                    sb.Append($", radius: {capsule.radius}, height: {capsule.height}, center: {capsule.center}");
                }
                else if (collider is MeshCollider mesh)
                {
                    sb.Append($", convex: {mesh.convex}, mesh: {(mesh.sharedMesh != null ? mesh.sharedMesh.name : "none")}");
                }
            }
            // RectTransform details voor UI elementen
            else if (component is RectTransform rectTransform)
            {
                sb.Append($" (anchoredPosition: {rectTransform.anchoredPosition}, sizeDelta: {rectTransform.sizeDelta})");
            }

            sb.AppendLine();
        }

        // Analyseer child objecten
        if (showChildDetails && obj.transform.childCount > 0)
        {
            sb.AppendLine($"\n{indent}Child Objects ({obj.transform.childCount}):");
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                Transform child = obj.transform.GetChild(i);
                sb.AppendLine($"\n{indent}Child {i + 1}:");
                AnalyzeGameObjectRecursive(child.gameObject, sb, depth + 1);
            }
        }

        sb.AppendLine($"\n{indent}{"=".PadRight(20, '=')}");
    }

    /// <summary>
    /// Analyseert alle interactors in de scene
    /// </summary>
    public void AnalyzeAllInteractors()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== ALLE INTERACTORS ===");

        var interactors = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>(FindObjectsSortMode.None);
        foreach (var interactor in interactors)
        {
            if (interactor == null) continue;

            sb.AppendLine($"\nInteractor: {interactor.name}");
            sb.AppendLine($"Type: {interactor.GetType().Name}");
            sb.AppendLine($"Pad: {GetGameObjectPath(interactor.gameObject)}");
            sb.AppendLine($"Is Hovering: {interactor.hasHover}");
            sb.AppendLine($"Is Selecting: {interactor.hasSelection}");

            if (interactor.hasHover)
            {
                sb.AppendLine("Hover targets:");
                foreach (var target in interactor.interactablesHovered)
                {
                    sb.AppendLine($"- {target.transform.name}");
                }
            }
        }

        lastAnalysisResult = sb.ToString();
        Debug.Log(lastAnalysisResult);
    }

    /// <summary>
    /// Analyseert alle interactables in de scene
    /// </summary>
    public void AnalyzeAllInteractables()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== ALLE INTERACTABLES ===");

        var interactables = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>(FindObjectsSortMode.None);
        foreach (var interactable in interactables)
        {
            if (interactable == null) continue;

            sb.AppendLine($"\nInteractable: {interactable.name}");
            sb.AppendLine($"Type: {interactable.GetType().Name}");
            sb.AppendLine($"Pad: {GetGameObjectPath(interactable.gameObject)}");
            sb.AppendLine($"Is Selected: {interactable.isSelected}");
            sb.AppendLine($"Is Hovered: {interactable.isHovered}");
        }

        lastAnalysisResult = sb.ToString();
        Debug.Log(lastAnalysisResult);
    }

    /// <summary>
    /// Analyseert de volledige scene hierarchie
    /// </summary>
    public void AnalyzeFullHierarchy()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== VOLLEDIGE SCENE HIERARCHIE ===");
        sb.AppendLine($"Scene: {SceneManager.GetActiveScene().name}");
        sb.AppendLine();

        var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in rootObjects)
        {
            AnalyzeHierarchyRecursive(root, 0, sb);
        }

        lastAnalysisResult = sb.ToString();
        Debug.Log(lastAnalysisResult);
    }

    /// <summary>
    /// Recursieve helper functie voor het analyseren van de hierarchie
    /// </summary>
    private void AnalyzeHierarchyRecursive(GameObject obj, int depth, StringBuilder sb)
    {
        if (obj == null) return;
        if (onlyActiveObjects && !obj.activeInHierarchy) return;

        string indent = new string('-', depth * 2);
        sb.AppendLine($"{indent}{obj.name} [{obj.activeInHierarchy}]");

        // Componenten weergeven
        var components = obj.GetComponents<Component>();
        foreach (var component in components)
        {
            if (component == null) continue;
            sb.AppendLine($"{indent}  └ {component.GetType().Name}");
        }

        // Recursief door children gaan
        foreach (Transform child in obj.transform)
        {
            AnalyzeHierarchyRecursive(child.gameObject, depth + 1, sb);
        }
    }

    /// <summary>
    /// Kopieert de laatste analyse resultaten naar het klembord
    /// </summary>
    public void CopyLastResultToClipboard()
    {
        if (!string.IsNullOrEmpty(lastAnalysisResult))
        {
            GUIUtility.systemCopyBuffer = lastAnalysisResult;
            Debug.Log("Analyse resultaten gekopieerd naar klembord!");
        }
        else
        {
            Debug.LogWarning("Geen analyse resultaten beschikbaar om te kopiëren.");
        }
    }

    /// <summary>
    /// Helper functie om het pad van een GameObject te krijgen
    /// </summary>
    private string GetGameObjectPath(GameObject obj)
    {
        StringBuilder path = new StringBuilder();
        Transform current = obj.transform;
        
        while (current != null)
        {
            path.Insert(0, current.name);
            if (current.parent != null)
                path.Insert(0, "/");
            current = current.parent;
        }
        
        return path.ToString();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ComponentAnalyzer))]
    public class ComponentAnalyzerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            ComponentAnalyzer analyzer = (ComponentAnalyzer)target;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Analyse Knoppen", EditorStyles.boldLabel);
            
            if (analyzer.targetObject != null)
            {
                if (GUILayout.Button($"Analyseer {analyzer.targetObject.name}"))
                {
                    analyzer.AnalyzeGameObject(analyzer.targetObject);
                }
                
                if (!string.IsNullOrEmpty(analyzer.lastAnalysisResult))
                {
                    if (GUILayout.Button("Kopieer Naar Klembord"))
                    {
                        analyzer.CopyLastResultToClipboard();
                    }
                }
            }
            
            if (GUILayout.Button("Scan Volledige Hierarchie"))
            {
                analyzer.AnalyzeFullHierarchy();
            }
            
            if (GUILayout.Button("Analyseer Alle Interactors"))
            {
                analyzer.AnalyzeAllInteractors();
            }
            
            if (GUILayout.Button("Analyseer Alle Interactables"))
            {
                analyzer.AnalyzeAllInteractables();
            }
        }
    }
#endif
} 