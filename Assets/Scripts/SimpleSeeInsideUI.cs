using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SoulGames.EasyGridBuilderPro;

/// <summary>
/// Vereenvoudigde UI script voor "See Inside" functionaliteit.
/// Deze versie werkt altijd - geen ingewikkelde target detectie.
/// Gewoon klikken en teleporteren naar het geselecteerde object.
/// </summary>
public class SimpleSeeInsideUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Referentie naar de SeeInsideTeleporter in de scene")]
    public SeeInsideTeleporter teleporter;
    
    [Tooltip("Button component voor de See Inside functionaliteit")]
    public Button seeInsideButton;
    
    [Tooltip("Text component van de knop")]
    public TextMeshProUGUI buttonText;
    
    [Tooltip("Referentie naar de GridUIManager voor selector panel functionaliteit")]
    public GridUIManager gridUIManager;
    
    [Header("Target Object")]
    [Tooltip("Het geselecteerde cel/halfcel object waar je in wilt gaan")]
    public GameObject selectedObject;
    
    [Header("Settings")]
    [Tooltip("Text wanneer je buiten een object bent")]
    public string enterText = "See Inside";
    
    [Tooltip("Text wanneer je binnen een object bent")]
    public string exitText = "Exit";
    
    private void Awake()
    {
        // Automatisch componenten vinden
        if (teleporter == null)
        {
            teleporter = FindObjectOfType<SeeInsideTeleporter>();
        }
        
        if (gridUIManager == null)
        {
            gridUIManager = FindObjectOfType<GridUIManager>();
        }
        
        if (seeInsideButton == null)
        {
            seeInsideButton = GetComponent<Button>();
        }
        
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // Button event toevoegen
        if (seeInsideButton != null)
        {
            seeInsideButton.onClick.AddListener(OnButtonClicked);
        }
        
        // Check of alles correct is ingesteld
        if (teleporter == null)
        {
            Debug.LogError("SimpleSeeInsideUI: Geen SeeInsideTeleporter gevonden! Voeg er een toe aan de scene.");
        }
        
        if (gridUIManager == null)
        {
            Debug.LogError("SimpleSeeInsideUI: Geen GridUIManager gevonden! Voeg er een toe aan de scene.");
        }
    }
    
    private void Update()
    {
        UpdateButtonText();
    }
    
    /// <summary>
    /// Update de button text afhankelijk van of we binnen of buiten een object zijn
    /// </summary>
    private void UpdateButtonText()
    {
        if (buttonText != null && teleporter != null)
        {
            if (teleporter.IsInsideObject)
            {
                buttonText.text = exitText;
            }
            else
            {
                buttonText.text = enterText;
            }
        }
    }
    

    
    /// <summary>
    /// Button click handler - teleporteer naar geselecteerde object
    /// </summary>
    private void OnButtonClicked()
    {
        if (teleporter == null)
        {
            Debug.LogError("SimpleSeeInsideUI: Geen teleporter gevonden!");
            return;
        }
        
        if (teleporter.IsInsideObject)
        {
            // Als we binnen zijn, ga naar buiten
            teleporter.ExitObject();
            Debug.Log("SimpleSeeInsideUI: Exited object");
        }
        else
        {
            // Als we buiten zijn, ga naar binnen in het geselecteerde object
            if (selectedObject != null)
            {
                teleporter.SeeInside(selectedObject);
                Debug.Log($"SimpleSeeInsideUI: Teleported inside {selectedObject.name}");
            }
            else
            {
                Debug.LogWarning("SimpleSeeInsideUI: Geen geselecteerd object! Stel selectedObject in via je menu systeem.");
            }
        }
    }
    
    /// <summary>
    /// Stel het geselecteerde object in (vanuit je menu systeem)
    /// </summary>
    /// <param name="newTarget">Het geselecteerde cel/halfcel object</param>
    public void SetSelectedObject(GameObject newTarget)
    {
        selectedObject = newTarget;
        if (newTarget != null)
        {
            Debug.Log($"SimpleSeeInsideUI: Geselecteerd object ingesteld op {newTarget.name}");
            
            // Toon het selector paneel voor dit object
            if (gridUIManager != null)
            {
                gridUIManager.ShowSelectorPanelForObject(newTarget);
            }
        }
        else
        {
            Debug.Log("SimpleSeeInsideUI: Geselecteerd object cleared (null)");
            
            // Verberg het selector paneel
            if (gridUIManager != null)
            {
                gridUIManager.HideSelectorPanel();
            }
        }
    }
    
    /// <summary>
    /// Update de button teksten
    /// </summary>
    /// <param name="newEnterText">Nieuwe "ga naar binnen" tekst</param>
    /// <param name="newExitText">Nieuwe "ga naar buiten" tekst</param>
    public void UpdateTexts(string newEnterText, string newExitText)
    {
        enterText = newEnterText;
        exitText = newExitText;
    }
    
    private void OnDestroy()
    {
        // Remove button event listener
        if (seeInsideButton != null)
        {
            seeInsideButton.onClick.RemoveListener(OnButtonClicked);
        }
    }
}
