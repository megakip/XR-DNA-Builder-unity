using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Beheert de selectiestatus van knoppen en zorgt ervoor dat slechts één knop tegelijk geselecteerd is.
/// De geselecteerde knop krijgt een oranje normale kleur, terwijl andere states (highlighted, pressed) behouden blijven.
/// </summary>
public class ButtonSelectionManager : MonoBehaviour
{
    [Tooltip("De standaard ColorBlock voor niet-geselecteerde knoppen")]
    [SerializeField] private Color defaultNormalColor = Color.white;
    [SerializeField] private Color defaultHighlightedColor = new Color(0.9f, 0.9f, 0.9f); // Lichtgrijs
    [SerializeField] private Color defaultPressedColor = new Color(0.8f, 0.8f, 0.8f); // Donkerder grijs
    
    [Tooltip("De ColorBlock voor de geselecteerde knop")]
    [SerializeField] private Color selectedNormalColor = new Color(1f, 0.5f, 0f); // Oranje
    [SerializeField] private Color selectedHighlightedColor = new Color(1f, 0.6f, 0.2f); // Lichtere oranje
    [SerializeField] private Color selectedPressedColor = new Color(0.9f, 0.4f, 0f); // Donkerdere oranje
    
    [Tooltip("Lijst met alle knoppen die beheerd moeten worden")]
    [SerializeField] private List<Button> managedButtons = new List<Button>();
    
    [Tooltip("Koppelt elke knop aan het bijbehorende scherm GameObject")]
    [SerializeField] private List<GameObject> screenGameObjects = new List<GameObject>();
    
    // De momenteel geselecteerde knop
    private Button currentSelectedButton;
    
    // Onthoud de originele ColorBlocks
    private Dictionary<Button, ColorBlock> originalColorBlocks = new Dictionary<Button, ColorBlock>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Als er geen knoppen handmatig zijn toegevoegd, zoek ze dan automatisch
        if (managedButtons.Count == 0)
        {
            FindAllButtons();
        }
        
        // Sla de originele kleuren op en voeg listeners toe aan alle knoppen
        foreach (Button button in managedButtons)
        {
            if (button != null)
            {
                // Sla het originele ColorBlock op
                originalColorBlocks[button] = button.colors;
                
                // Voeg onClick listener toe
                Button localBtn = button;
                button.onClick.AddListener(() => SelectButton(localBtn));
            }
        }
        
        // NIEUW: Zoek welk scherm actief is bij het opstarten en selecteer de bijbehorende knop
        SelectButtonForActiveScreen();
    }

    /// <summary>
    /// Zoekt automatisch alle knoppen in het Canvas
    /// </summary>
    private void FindAllButtons()
    {
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        managedButtons.AddRange(allButtons);
        Debug.Log($"Automatisch {managedButtons.Count} knoppen gevonden voor beheer");
    }
    
    /// <summary>
    /// Selecteert een knop en werkt de kleuren bij
    /// </summary>
    public void SelectButton(Button selectedButton)
    {
        if (selectedButton == null) return;
        
        // Deselecteer de huidige knop als die er is
        if (currentSelectedButton != null)
        {
            SetButtonToDefaultState(currentSelectedButton);
        }
        
        // Stel de nieuwe knop in als geselecteerd
        currentSelectedButton = selectedButton;
        SetButtonToSelectedState(currentSelectedButton);
        
        // Hier kun je aanvullende acties uitvoeren op basis van de selectie
        // bijvoorbeeld het activeren van het juiste frame in de andere menu
        HandleFrameActivation(selectedButton);
    }
    
    /// <summary>
    /// Stelt een knop in op de standaard (niet-geselecteerde) staat
    /// </summary>
    private void SetButtonToDefaultState(Button button)
    {
        ColorBlock colors = button.colors;
        
        // Behoud de originele waarden voor andere eigenschappen
        if (originalColorBlocks.TryGetValue(button, out ColorBlock original))
        {
            colors.colorMultiplier = original.colorMultiplier;
            colors.fadeDuration = original.fadeDuration;
            colors.disabledColor = original.disabledColor;
        }
        
        // Update alleen de kleuren die we willen aanpassen
        colors.normalColor = defaultNormalColor;
        colors.highlightedColor = defaultHighlightedColor;
        colors.pressedColor = defaultPressedColor;
        colors.selectedColor = defaultNormalColor; // Meestal hetzelfde als normal
        
        button.colors = colors;
    }
    
    /// <summary>
    /// Stelt een knop in op de geselecteerde staat
    /// </summary>
    private void SetButtonToSelectedState(Button button)
    {
        ColorBlock colors = button.colors;
        
        // Behoud de originele waarden voor andere eigenschappen
        if (originalColorBlocks.TryGetValue(button, out ColorBlock original))
        {
            colors.colorMultiplier = original.colorMultiplier;
            colors.fadeDuration = original.fadeDuration;
            colors.disabledColor = original.disabledColor;
        }
        
        // Update alleen de kleuren die we willen aanpassen
        colors.normalColor = selectedNormalColor;
        colors.highlightedColor = selectedHighlightedColor;
        colors.pressedColor = selectedPressedColor;
        colors.selectedColor = selectedNormalColor; // Meestal hetzelfde als normal
        
        button.colors = colors;
    }
    
    /// <summary>
    /// Activeert het bijbehorende frame op basis van de geselecteerde knop
    /// </summary>
    private void HandleFrameActivation(Button selectedButton)
    {
        // Vind de index van de geselecteerde knop
        int selectedIndex = managedButtons.IndexOf(selectedButton);
        
        // Controleer of de index geldig is en of er een bijbehorend scherm is
        if (selectedIndex >= 0 && selectedIndex < screenGameObjects.Count && screenGameObjects[selectedIndex] != null)
        {
            // Deactiveer alle schermen eerst
            for (int i = 0; i < screenGameObjects.Count; i++)
            {
                if (screenGameObjects[i] != null)
                {
                    screenGameObjects[i].SetActive(false);
                }
            }
            
            // Activeer alleen het geselecteerde scherm
            screenGameObjects[selectedIndex].SetActive(true);
            
            Debug.Log($"Knop '{selectedButton.name}' geselecteerd, scherm '{screenGameObjects[selectedIndex].name}' geactiveerd");
        }
        else
        {
            Debug.LogWarning($"Knop '{selectedButton.name}' heeft geen bijbehorend scherm of index ({selectedIndex}) is buiten bereik");
        }
    }
    
    /// <summary>
    /// Handmatig een knop selecteren via een publieke methode
    /// </summary>
    public void SelectButtonByIndex(int index)
    {
        if (index >= 0 && index < managedButtons.Count)
        {
            SelectButton(managedButtons[index]);
        }
    }
    
    /// <summary>
    /// Reset alle knoppen naar de standaardkleur
    /// </summary>
    public void ResetAllButtons()
    {
        foreach (Button button in managedButtons)
        {
            if (button != null)
            {
                SetButtonToDefaultState(button);
            }
        }
        currentSelectedButton = null;
    }

    /// <summary>
    /// Vindt het actieve scherm en selecteert de bijbehorende knop
    /// </summary>
    private void SelectButtonForActiveScreen()
    {
        // Zoek welk scherm actief is
        for (int i = 0; i < screenGameObjects.Count; i++)
        {
            if (screenGameObjects[i] != null && screenGameObjects[i].activeSelf)
            {
                // Als we een actief scherm vinden, selecteer de bijbehorende knop
                if (i < managedButtons.Count && managedButtons[i] != null)
                {
                    // Selecteer de bijbehorende knop
                    Debug.Log($"Startscherm gevonden: {screenGameObjects[i].name}, bijbehorende knop geselecteerd: {managedButtons[i].name}");
                    SelectButton(managedButtons[i]);
                    return;
                }
            }
        }
        
        // Als geen scherm actief is of geen passende knop gevonden is, selecteer de eerste knop (optioneel)
        if (managedButtons.Count > 0 && managedButtons[0] != null)
        {
            Debug.Log("Geen actief startscherm gevonden, eerste knop geselecteerd als fallback");
            SelectButton(managedButtons[0]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
