using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Verbeterde terugknop die het huidige scherm uitschakelt en teruggaat naar het vorige actieve scherm.
/// Houdt automatisch een geschiedenis bij van bezochte schermen.
/// Plaats dit script op je terugknop.
/// </summary>
public class SimpleBackButton : MonoBehaviour
{
    [Tooltip("Het contentObject waarin alle pagina's zich bevinden")]
    [SerializeField] private GameObject contentObject;
    
    [Tooltip("Prefix voor de tekst op de knop (leeg laten om alleen de naam te tonen)")]
    [SerializeField] private string textPrefix = "";
    
    [Tooltip("Of de tekst automatisch moet worden bijgewerkt op basis van het actieve scherm")]
    [SerializeField] private bool updateButtonText = true;
    
    [Tooltip("De Text component die moet worden bijgewerkt (kan op een ander GameObject staan)")]
    [SerializeField] private Text buttonText;
    
    [Tooltip("De TextMeshProUGUI component die moet worden bijgewerkt (kan op een ander GameObject staan)")]
    [SerializeField] private TextMeshProUGUI buttonTMP;
    
    [Tooltip("De Image component van het pijl-icoon (het Vector object)")]
    [SerializeField] private Image buttonIcon;
    
    [Tooltip("De kleur van de knop wanneer deze niet beschikbaar is")]
    [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Grijs, half transparant
    
    [Tooltip("De normale kleur van de knop")]
    [SerializeField] private Color normalColor = Color.white;
    
    // Stack om de geschiedenis van actieve schermen bij te houden
    private static Stack<GameObject> screenHistory = new Stack<GameObject>();
    
    private Button button;
    
    // Singleton instance voor globale toegang
    private static SimpleBackButton instance;
    
    private void Awake()
    {
        // Singleton setup
        instance = this;
        
        // Reset de geschiedenis bij het starten van de scene
        screenHistory.Clear();
        
        // Haal de Button component op
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("SimpleBackButton vereist een Button component op hetzelfde GameObject!");
            return;
        }
        
        // Als geen tekst component is ingesteld, proberen we deze automatisch te vinden
        if (buttonText == null && buttonTMP == null)
        {
            buttonText = GetComponentInChildren<Text>();
            buttonTMP = GetComponentInChildren<TextMeshProUGUI>();
            
            if (buttonText == null && buttonTMP == null && updateButtonText)
            {
                Debug.LogWarning("Geen Text of TextMeshProUGUI component gevonden of ingesteld. Stel deze handmatig in via de Inspector voor tekstupdates.");
            }
        }
        
        // Als geen icon component is ingesteld, proberen we deze te vinden
        if (buttonIcon == null)
        {
            // Zoek naar een Image component in een kind genaamd "Vector"
            Transform vectorTransform = transform.Find("Vector");
            if (vectorTransform != null)
            {
                buttonIcon = vectorTransform.GetComponent<Image>();
            }
            
            // Als dat niet werkt, zoek in alle kinderen
            if (buttonIcon == null)
            {
                buttonIcon = GetComponentInChildren<Image>();
            }
            
            if (buttonIcon == null)
            {
                Debug.LogWarning("Geen Image component gevonden voor de terugknop icoon. Stel deze handmatig in via de Inspector.");
            }
        }
        
        // Voeg onClick listener toe
        button.onClick.AddListener(GoBack);
    }
    
    private void Start()
    {
        // Update de knoptekst voor het huidige actieve scherm zonder het toe te voegen aan geschiedenis
        GameObject activeScreen = FindActiveScreen();
        if (activeScreen != null)
        {
            UpdateButtonTextForScreen(activeScreen);
        }
        
        // Update de visuele staat van de knop (zal grijs zijn omdat de geschiedenis leeg is)
        UpdateButtonVisualState();
    }
    
    private void OnEnable()
    {
        // Update de tekst wanneer de knop wordt ingeschakeld
        GameObject activeScreen = FindActiveScreen();
        if (activeScreen != null)
        {
            UpdateButtonTextForScreen(activeScreen);
        }
        
        // Update de visuele staat van de knop
        UpdateButtonVisualState();
    }
    
    // Update wordt elke frame aangeroepen
    private void Update()
    {
        // Controleer regelmatig of het actieve scherm is veranderd
        GameObject currentActiveScreen = FindActiveScreen();
        if (currentActiveScreen != null)
        {
            UpdateButtonTextForScreen(currentActiveScreen);
        }
        
        // Update regelmatig de visuele staat van de knop
        UpdateButtonVisualState();
    }
    
    /// <summary>
    /// Update de visuele staat van de knop op basis van beschikbaarheid
    /// </summary>
    private void UpdateButtonVisualState()
    {
        bool canGoBack = screenHistory.Count > 0;
        
        // Pas de visuele staat aan op basis van beschikbaarheid
        if (buttonIcon != null)
        {
            buttonIcon.color = canGoBack ? normalColor : disabledColor;
        }
        
        // Optioneel: Schakel de knop interactiviteit in/uit
        if (button != null)
        {
            button.interactable = canGoBack;
        }
    }
    
    /// <summary>
    /// Wordt aangeroepen wanneer een nieuw scherm wordt geactiveerd
    /// </summary>
    public static void RegisterScreenChange(GameObject newScreen, GameObject previousScreen)
    {
        if (previousScreen != null)
        {
            screenHistory.Push(previousScreen);
            Debug.Log("Scherm toegevoegd aan geschiedenis: " + previousScreen.name);
            
            // Update de terugknop tekst via de singleton instance
            if (instance != null)
            {
                instance.UpdateButtonTextForScreen(newScreen);
                instance.UpdateButtonVisualState();
            }
        }
    }
    
    /// <summary>
    /// Publieke methode om de tekst bij te werken (kan worden aangeroepen vanuit andere scripts)
    /// </summary>
    public void UpdateText()
    {
        GameObject activeScreen = FindActiveScreen();
        if (activeScreen != null)
        {
            UpdateButtonTextForScreen(activeScreen);
        }
    }
    
    /// <summary>
    /// Gaat terug naar het vorige scherm in de geschiedenis
    /// </summary>
    public void GoBack()
    {
        // Controleer of er geschiedenis is om naar terug te gaan
        if (screenHistory.Count == 0)
        {
            Debug.LogWarning("Geen vorig scherm om naar terug te gaan!");
            UpdateButtonVisualState();
            return;
        }
        
        // Haal het vorige scherm uit de geschiedenis
        GameObject previousScreen = screenHistory.Pop();
        
        // Vind ALLEEN de directe actieve kinderen van het contentObject (niet de kleinkinderen)
        List<GameObject> topLevelScreens = FindTopLevelActiveScreens();
        if (topLevelScreens.Count == 0)
        {
            Debug.LogWarning("Geen actieve schermen gevonden!");
            return;
        }
        
        // Deactiveer ALLEEN de directe actieve kinderen van het contentObject
        foreach (GameObject screen in topLevelScreens)
        {
            if (screen != null && screen != contentObject && screen != previousScreen)
            {
                Debug.Log("Deactiveren van scherm: " + screen.name);
                screen.SetActive(false);
            }
        }
        
        // Activeer het vorige scherm
        previousScreen.SetActive(true);
        
        // Update de tekst op de knop voor het nieuwe actieve scherm
        UpdateButtonTextForScreen(previousScreen);
        
        // Update de visuele staat van de knop
        UpdateButtonVisualState();
        
        Debug.Log("Teruggegaan naar scherm: " + previousScreen.name);
    }
    
    /// <summary>
    /// Update de tekst op de knop op basis van het actieve scherm
    /// </summary>
    private void UpdateButtonTextForScreen(GameObject activeScreen)
    {
        if (!updateButtonText) return;
        
        string newText;
        if (string.IsNullOrEmpty(textPrefix))
        {
            newText = activeScreen.name;
        }
        else
        {
            newText = textPrefix + " " + activeScreen.name;
        }
        
        // Update de tekst op de knop alleen als deze is veranderd
        if (buttonText != null && buttonText.text != newText)
        {
            buttonText.text = newText;
        }
        else if (buttonTMP != null && buttonTMP.text != newText)
        {
            buttonTMP.text = newText;
        }
    }
    
    /// <summary>
    /// Vindt het huidige actieve scherm binnen het contentObject
    /// </summary>
    private GameObject FindActiveScreen()
    {
        if (contentObject == null)
        {
            Debug.LogError("ContentObject is niet ingesteld!");
            return null;
        }
        
        // Zoek het eerste directe kind van contentObject dat actief is
        foreach (Transform child in contentObject.transform)
        {
            if (child.gameObject.activeSelf)
            {
                return child.gameObject;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Vindt ALLEEN de directe actieve kinderen van het contentObject (niet recursief)
    /// </summary>
    private List<GameObject> FindTopLevelActiveScreens()
    {
        List<GameObject> activeScreens = new List<GameObject>();
        
        if (contentObject == null)
        {
            Debug.LogError("ContentObject is niet ingesteld!");
            return activeScreens;
        }
        
        // Verzamel ALLEEN de directe actieve kinderen van contentObject (niet hun kinderen)
        foreach (Transform child in contentObject.transform)
        {
            if (child.gameObject.activeSelf)
            {
                activeScreens.Add(child.gameObject);
            }
        }
        
        return activeScreens;
    }
} 