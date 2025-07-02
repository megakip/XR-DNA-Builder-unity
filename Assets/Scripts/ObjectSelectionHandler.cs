using UnityEngine;
using SoulGames.EasyGridBuilderPro;

/// <summary>
/// Script dat automatisch het geselecteerde cel/halfcel object doorgeeft aan de SimpleSeeInsideUI.
/// Dit script zorgt ervoor dat wanneer je op een cel klikt en het menu verschijnt,
/// de "See Inside" knop automatisch weet welk object je hebt geselecteerd.
/// 
/// Gebruik:
/// 1. Voeg dit script toe aan een GameObject in je scene (bijvoorbeeld op de GridManager)
/// 2. Assign de SimpleSeeInsideUI referentie in de Inspector
/// 3. Het script luistert automatisch naar object selecties van Grid Builder Pro
/// 
/// Werkt met Grid Builder Pro 2 BuildableObjectSelector events.
/// </summary>
public class ObjectSelectionHandler : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Referentie naar de SimpleSeeInsideUI component")]
    public SimpleSeeInsideUI seeInsideUI;
    
    [Header("Settings")]
    [Tooltip("Alleen cel/halfcel objecten selecteren voor See Inside functionaliteit")]
    public bool onlyCellObjects = true;
    
    [Tooltip("Debug logging inschakelen")]
    public bool enableDebugLogging = true;
    
    // Private variables
    private BuildableObjectSelector buildableObjectSelector;
    private GameObject currentSelectedObject;
    
    private void Start()
    {
        // Automatisch SimpleSeeInsideUI vinden als niet ingesteld
        if (seeInsideUI == null)
        {
            seeInsideUI = FindObjectOfType<SimpleSeeInsideUI>();
            if (seeInsideUI == null)
            {
                Debug.LogError("ObjectSelectionHandler: Geen SimpleSeeInsideUI gevonden in de scene!");
                return;
            }
        }
        
        // BuildableObjectSelector vinden en events subscriben
        if (GridManager.Instance.TryGetBuildableObjectSelector(out buildableObjectSelector))
        {
            buildableObjectSelector.OnBuildableObjectSelected += OnObjectSelected;
            buildableObjectSelector.OnBuildableObjectDeselected += OnObjectDeselected;
            
            // Check of er al objecten geselecteerd zijn bij opstarten
            var selectedObjects = buildableObjectSelector.GetSelectedObjectsList();
            if (selectedObjects != null && selectedObjects.Count > 0)
            {
                // Gebruik het eerste geselecteerde object
                OnObjectSelected(selectedObjects[0]);
            }
            
            if (enableDebugLogging)
            {
                Debug.Log("ObjectSelectionHandler: Successfully connected to BuildableObjectSelector events");
            }
        }
        else
        {
            Debug.LogError("ObjectSelectionHandler: Geen BuildableObjectSelector gevonden!");
        }
    }
    
    private void OnDestroy()
    {
        // Events unsubscriben om memory leaks te voorkomen
        if (buildableObjectSelector != null)
        {
            buildableObjectSelector.OnBuildableObjectSelected -= OnObjectSelected;
            buildableObjectSelector.OnBuildableObjectDeselected -= OnObjectDeselected;
        }
    }
    
    /// <summary>
    /// Wordt aangeroepen wanneer een object wordt geselecteerd
    /// </summary>
    /// <param name="buildableObject">Het geselecteerde object</param>
    private void OnObjectSelected(BuildableObject buildableObject)
    {
        if (buildableObject == null || seeInsideUI == null) return;
        
        GameObject selectedGameObject = buildableObject.gameObject;
        
        // Check of het een cel/halfcel object is (als onlyCellObjects aan staat)
        if (onlyCellObjects && !IsCellObject(selectedGameObject))
        {
            if (enableDebugLogging)
            {
                Debug.Log($"ObjectSelectionHandler: {selectedGameObject.name} is geen cel/halfcel object, wordt genegeerd");
            }
            return;
        }
        
        // Stel het geselecteerde object in op de SimpleSeeInsideUI
        currentSelectedObject = selectedGameObject;
        seeInsideUI.SetSelectedObject(currentSelectedObject);
        
        if (enableDebugLogging)
        {
            Debug.Log($"ObjectSelectionHandler: Object '{currentSelectedObject.name}' geselecteerd en doorgegeven aan SimpleSeeInsideUI");
        }
    }
    
    /// <summary>
    /// Wordt aangeroepen wanneer een object wordt gedeselecteerd
    /// </summary>
    /// <param name="buildableObject">Het gedeselecteerde object</param>
    private void OnObjectDeselected(BuildableObject buildableObject)
    {
        if (buildableObject == null || seeInsideUI == null) return;
        
        GameObject deselectedGameObject = buildableObject.gameObject;
        
        // Als het huidige geselecteerde object wordt gedeselecteerd, clear de selectie
        if (currentSelectedObject == deselectedGameObject)
        {
            currentSelectedObject = null;
            seeInsideUI.SetSelectedObject(null);
            
            if (enableDebugLogging)
            {
                Debug.Log($"ObjectSelectionHandler: Object '{deselectedGameObject.name}' gedeselecteerd");
            }
        }
    }
    
    /// <summary>
    /// Controleer of een GameObject een cel/halfcel object is
    /// </summary>
    /// <param name="obj">Het te controleren GameObject</param>
    /// <returns>True als het een cel/halfcel object is</returns>
    private bool IsCellObject(GameObject obj)
    {
        if (obj == null) return false;
        
        string objName = obj.name.ToLower();
        
        // Check voor cel/halfcel in de naam
        return objName.Contains("cel") || 
               objName.Contains("cell") || 
               objName.Contains("half") ||
               objName.Contains("full");
    }
    
    /// <summary>
    /// Handmatig een object selecteren (voor debugging/testing)
    /// </summary>
    /// <param name="obj">Het object om te selecteren</param>
    public void ManuallySelectObject(GameObject obj)
    {
        if (obj == null || seeInsideUI == null) return;
        
        currentSelectedObject = obj;
        seeInsideUI.SetSelectedObject(currentSelectedObject);
        
        if (enableDebugLogging)
        {
            Debug.Log($"ObjectSelectionHandler: Handmatig object '{obj.name}' geselecteerd");
        }
    }
    
    /// <summary>
    /// Get het huidige geselecteerde object
    /// </summary>
    /// <returns>Het huidige geselecteerde object, of null als er niets geselecteerd is</returns>
    public GameObject GetCurrentSelectedObject()
    {
        return currentSelectedObject;
    }
    
    /// <summary>
    /// Check of er momenteel een object geselecteerd is
    /// </summary>
    /// <returns>True als er een object geselecteerd is</returns>
    public bool HasSelectedObject()
    {
        return currentSelectedObject != null;
    }
} 