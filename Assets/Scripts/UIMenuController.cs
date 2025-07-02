using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using SoulGames.EasyGridBuilderPro; // Voor BuildableObjectSO toegang

/// <summary>
/// Script voor het correct openen en sluiten van een scrollable menu in XR.
/// Vereenvoudigde versie voor betere stabiliteit met scrollende menu-items.
/// </summary>
public class UIMenuController : MonoBehaviour
{
    [Tooltip("Het hoofdpaneel dat menu-items bevat")]
    [SerializeField] private RectTransform contentContainer;
    
    [Tooltip("Naam van de toggle knoppen in de hiërarchie (bijv. 'Btn_Info')")]
    [SerializeField] private string toggleButtonName = "Btn_Info";
    
    [Tooltip("Naam van de content frames die worden getoond/verborgen (bijv. 'Informatie inhoud frame')")]
    [SerializeField] private string contentFrameName = "Informatie inhoud frame";
    
    [Tooltip("Optioneel: ScrollRect component als het menu scrollable is")]
    [SerializeField] private ScrollRect scrollRect;
    
    [Tooltip("Het GameObject dat in/uitgeklapt wordt")]
    [SerializeField] private GameObject menuItemsContainer;
    
    [Tooltip("Uitgebreide debug logging")]
    [SerializeField] private bool debugMode = true;
    
    [Tooltip("Voeg automatisch onClick listeners toe aan knoppen (uitschakelen als je onClick in Inspector hebt ingesteld)")]
    [SerializeField] private bool addClickListeners = false;
    
    // Direct per knop opgeslagen componenten - geen gedeelde lijst meer
    private Dictionary<Button, GameObject> buttonToContentMap = new Dictionary<Button, GameObject>();
    
    private void Awake()
    {
        // Controleer of alle benodigde componenten zijn toegewezen
        if (contentContainer == null)
        {
            Debug.LogError("ContentContainer is niet toegewezen in de inspector!");
            return;
        }
        
        if (menuItemsContainer == null)
        {
            menuItemsContainer = contentContainer.gameObject;
        }
        
        // Zoek alle knoppen en registreer ze
        SetupButtons();
    }
    
    private void SetupButtons()
    {
        // Zoek alle buttons met 'Btn_Info' in de naam
        Button[] allButtons = menuItemsContainer.GetComponentsInChildren<Button>(true);
        buttonToContentMap.Clear();
        
        if (debugMode)
        {
            Debug.Log($"[UIMenuController] Zoekend naar knoppen met naam: '{toggleButtonName}'");
            Debug.Log($"[UIMenuController] Totaal aantal knoppen gevonden: {allButtons.Length}");
        }
        
        // Voor elke knop, zoek het bijbehorende content frame
        foreach (Button button in allButtons)
        {
            if (button.name.Contains(toggleButtonName))
            {
                // Zoek het bijbehorende content frame voor deze knop
                GameObject contentFrame = FindContentFrameForButton(button);
                
                if (contentFrame != null)
                {
                    // Voeg de knop en content frame toe aan de map
                    buttonToContentMap[button] = contentFrame;
                    
                    // Verberg het content frame bij het opstarten
                    contentFrame.SetActive(false);
                    
                    // BELANGRIJK: Alleen onClick listeners toevoegen als dat expliciet is ingeschakeld
                    // Dit voorkomt dubbele handlers als de gebruiker onClick events in de Inspector heeft ingesteld
                    if (addClickListeners)
                    {
                        // Verwijder bestaande listeners alleen als we nieuwe willen toevoegen
                        button.onClick.RemoveAllListeners();
                        
                        // Voeg een directe onClick listener toe die specifiek is voor deze knop
                        button.onClick.AddListener(() => ToggleContentFrame(button, contentFrame));
                    }
                    
                    if (debugMode)
                    {
                        Debug.Log($"[UIMenuController] Knop '{button.name}' gekoppeld aan content frame '{contentFrame.name}'");
                    }
                }
                else
                {
                    Debug.LogWarning($"[UIMenuController] Geen content frame gevonden voor knop: {button.name}");
                }
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"[UIMenuController] Totaal aantal knop-content paren geregistreerd: {buttonToContentMap.Count}");
        }
    }
    
    /// <summary>
    /// Directe toggle functie die een specifieke knop en content frame toggled
    /// </summary>
    private void ToggleContentFrame(Button button, GameObject contentFrame)
    {
        if (button == null || contentFrame == null) return;
        
        // Toggle de zichtbaarheid van het content frame (simpele en directe aanpak)
        bool newState = !contentFrame.activeSelf;
        contentFrame.SetActive(newState);
        
        // NIEUW: Als het content frame wordt geopend, update het icoon
        if (newState)
        {
            UpdateInformatieframeIcon(button, contentFrame);
        }
        
        if (debugMode)
        {
            Debug.Log($"[UIMenuController] Toggle content frame '{contentFrame.name}' voor knop '{button.name}': {newState}");
        }
        
        // Forceer layout update
        UpdateCanvasLayout();
    }
    
    /// <summary>
    /// Publieke toggle functie voor gebruik in Inspector onClick events
    /// </summary>
    public void ToggleThisButtonContent()
    {
        // Vind de button die dit event heeft geactiveerd
        EventSystem currentEventSystem = EventSystem.current;
        if (currentEventSystem == null) return;
        
        GameObject selectedObject = currentEventSystem.currentSelectedGameObject;
        if (selectedObject == null) return;
        
        if (debugMode)
        {
            Debug.Log($"[UIMenuController] ToggleThisButtonContent aangeroepen voor: {selectedObject.name}");
        }
        
        // Zoek de knop component
        Button button = selectedObject.GetComponent<Button>();
        
        // Als het object zelf geen knop is, kijk dan of de parent een knop is
        if (button == null && selectedObject.transform.parent != null)
        {
            button = selectedObject.transform.parent.GetComponent<Button>();
        }
        
        // Als we een knop hebben gevonden, toggle het bijbehorende content frame
        if (button != null)
        {
            if (buttonToContentMap.TryGetValue(button, out GameObject contentFrame))
            {
                ToggleContentFrame(button, contentFrame);
            }
            else
            {
                // Als de knop niet in de map staat, probeer het content frame te vinden
                GameObject foundContentFrame = FindContentFrameForButton(button);
                if (foundContentFrame != null)
                {
                    // Voeg toe aan de map voor toekomstig gebruik
                    buttonToContentMap[button] = foundContentFrame;
                    ToggleContentFrame(button, foundContentFrame);
                }
                else
                {
                    Debug.LogWarning($"[UIMenuController] Geen contentFrame gevonden voor knop: {button.name}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[UIMenuController] Geen knop component gevonden voor object: {selectedObject.name}");
        }
    }
    
    /// <summary>
    /// Zoekt het content frame voor een knop door op verschillende niveaus te zoeken
    /// </summary>
    private GameObject FindContentFrameForButton(Button button)
    {
        if (button == null) return null;
        
        Transform buttonParent = button.transform.parent;
        if (buttonParent == null) return null;
        
        // 1. Zoek eerst op dezelfde niveau als de knop (siblings)
        foreach (Transform child in buttonParent)
        {
            if (child.name == contentFrameName)
            {
                return child.gameObject;
            }
        }
        
        // 2. Zoek in de ButtonContainer voor het content frame
        Transform containerParent = buttonParent.parent;
        if (containerParent != null)
        {
            foreach (Transform child in containerParent)
            {
                if (child.name == contentFrameName)
                {
                    return child.gameObject;
                }
            }
            
            // 3. Probeer Find te gebruiken
            Transform found = containerParent.Find(contentFrameName);
            if (found != null)
            {
                return found.gameObject;
            }
        }
        
        // 4. Ga omhoog in de hiërarchie en zoek het ItemSpawnButton object
        Transform current = buttonParent;
        while (current != null && !current.name.Contains("ItemSpawnButton"))
        {
            current = current.parent;
        }
        
        if (current != null)
        {
            // Zoek in het ItemSpawnButton object naar het content frame
            foreach (Transform child in current)
            {
                if (child.name == contentFrameName)
                {
                    return child.gameObject;
                }
            }
            
            // Extra check: probeer via Find in ItemSpawnButton
            Transform found = current.Find(contentFrameName);
            if (found != null)
            {
                return found.gameObject;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Sluit alle content frames
    /// </summary>
    public void CloseAllContent()
    {
        foreach (var pair in buttonToContentMap)
        {
            if (pair.Value != null && pair.Value.activeSelf)
            {
                pair.Value.SetActive(false);
            }
        }
        
        // Forceer layout update
        UpdateCanvasLayout();
    }
    
    // Methode om de layout onmiddellijk te forceren bijwerken
    public void UpdateCanvasLayout()
    {
        Canvas.ForceUpdateCanvases();
        if (contentContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer);
        }
    }
    
    // Reset de setup als het script wordt ingeschakeld
    private void OnEnable()
    {
        // Herregistreer alle knoppen als het script wordt ingeschakeld
        // Dit helpt bij het omgaan met dynamische UI elementen
        StartCoroutine(SetupButtonsNextFrame());
    }
    
    private IEnumerator SetupButtonsNextFrame()
    {
        yield return null;
        SetupButtons();
    }
    
    /// <summary>
    /// Toggle alleen het Informatieframe dat hoort bij de specifieke geklikte knop
    /// Deze methode gebruikt de parent-child relaties in de hiërarchie
    /// </summary>
    public void ToggleSpecificInformatieframe()
    {
        // Vind de huidige geklikte knop
        EventSystem currentEventSystem = EventSystem.current;
        if (currentEventSystem == null) return;
        
        GameObject selectedObject = currentEventSystem.currentSelectedGameObject;
        if (selectedObject == null) return;
        
        // Zoek de knop component (dit kan de knop zelf zijn of een parent)
        Button button = selectedObject.GetComponent<Button>();
        if (button == null && selectedObject.transform.parent != null)
        {
            button = selectedObject.transform.parent.GetComponent<Button>();
        }
        
        if (button == null) return;
        
        if (debugMode)
        {
            Debug.Log($"[UIMenuController] ToggleSpecificInformatieframe aangeroepen voor: {button.name}");
        }
        
        // 1. Zoek eerst naar een Informatieframe als sibling van de button
        Transform buttonParent = button.transform.parent;
        GameObject informatieframe = null;
        
        if (buttonParent != null)
        {
            // Zoek naar een sibling met de naam "Informatieframe"
            foreach (Transform child in buttonParent)
            {
                if (child.name == "Informatieframe")
                {
                    informatieframe = child.gameObject;
                    break;
                }
            }
            
            // Als niet gevonden als sibling, ga één niveau hoger en zoek daar
            if (informatieframe == null && buttonParent.parent != null)
            {
                foreach (Transform child in buttonParent.parent)
                {
                    if (child.name == "Informatieframe")
                    {
                        informatieframe = child.gameObject;
                        break;
                    }
                }
            }
        }
        
        // Als nog steeds niet gevonden, probeer als kind van de Togglebutton zelf
        if (informatieframe == null)
        {
            foreach (Transform child in button.transform)
            {
                if (child.name == "Informatieframe")
                {
                    informatieframe = child.gameObject;
                    break;
                }
            }
        }
        
        // Toggle het gevonden informatieframe
        if (informatieframe != null)
        {
            informatieframe.SetActive(!informatieframe.activeSelf);
            
            // NIEUW: Als het informatieframe wordt geopend, update het icoon
            if (informatieframe.activeSelf)
            {
                UpdateInformatieframeIcon(button, informatieframe);
            }
            
            if (debugMode)
            {
                Debug.Log($"[UIMenuController] Toggled specifieke Informatieframe: {informatieframe.GetInstanceID()}, Actief: {informatieframe.activeSelf}");
            }
            
            // Verbeterde layout update
            StartCoroutine(ForceLayoutRefresh());
        }
        else
        {
            Debug.LogWarning($"[UIMenuController] Geen Informatieframe gevonden voor knop: {button.name}");
        }
    }
    
    /// <summary>
    /// Forceert een volledige vernieuwing van de layout met een korte vertraging
    /// </summary>
    private IEnumerator ForceLayoutRefresh()
    {
        // Eerste update direct uitvoeren
        UpdateCanvasLayout();
        
        // Wacht één frame om ervoor te zorgen dat alles is bijgewerkt
        yield return null;
        
        // Update opnieuw na een frame
        UpdateCanvasLayout();
        
        // Zoek alle LayoutGroups in de hiërarchie en rebuild ze
        LayoutGroup[] layoutGroups = GetComponentsInChildren<LayoutGroup>(true);
        foreach (LayoutGroup group in layoutGroups)
        {
            if (group != null && group.gameObject.activeInHierarchy)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)group.transform);
            }
        }
        
        // Wacht nog een frame voor extra zekerheid
        yield return null;
        
        // Finale update
        UpdateCanvasLayout();
        
        // Als er een ScrollRect is, herbereken de content size
        if (scrollRect != null && scrollRect.content != null)
        {
            // Forceer content grootte te updaten
            ContentSizeFitter sizeFitter = scrollRect.content.GetComponent<ContentSizeFitter>();
            if (sizeFitter != null)
            {
                // Toggle de Content Size Fitter om een refresh te forceren
                bool enabled = sizeFitter.enabled;
                sizeFitter.enabled = false;
                sizeFitter.enabled = enabled;
            }
            
            // Update de content bounds in de ScrollRect
            scrollRect.Rebuild(CanvasUpdate.PostLayout);
        }
    }
    
    /// <summary>
    /// Specifieke methode om het Informatieframe te tonen/verbergen
    /// Kan direct worden toegewezen aan een knop in de Inspector
    /// </summary>
    public void ToggleInformatieframe()
    {
        // Zoek het Informatieframe GameObject 
        GameObject informatieframe = FindInformatieframe();
        
        if (informatieframe != null)
        {
            // Toggle de zichtbaarheid
            informatieframe.SetActive(!informatieframe.activeSelf);
            
            if (debugMode)
            {
                Debug.Log($"[UIMenuController] Informatieframe visibility: {informatieframe.activeSelf}");
            }
            
            // Forceer layout update
            UpdateCanvasLayout();
        }
        else
        {
            Debug.LogWarning("[UIMenuController] Informatieframe niet gevonden!");
        }
    }
    
    /// <summary>
    /// Hulpmethode om het Informatieframe GameObject te vinden
    /// </summary>
    private GameObject FindInformatieframe()
    {
        // Zoek eerst bij directe kinderen van dit object
        Transform found = transform.Find("Informatieframe");
        if (found != null) return found.gameObject;
        
        // Zoek in de hele scene als het niet direct gevonden is
        GameObject obj = GameObject.Find("Informatieframe");
        if (obj != null) return obj;
        
        // Zoek recursief in alle kinderen
        return FindGameObjectRecursively(gameObject, "Informatieframe");
    }
    
    /// <summary>
    /// Recursief zoeken naar een GameObject met specifieke naam
    /// </summary>
    private GameObject FindGameObjectRecursively(GameObject parent, string name)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.name == name)
            {
                return child.gameObject;
            }
            
            GameObject found = FindGameObjectRecursively(child.gameObject, name);
            if (found != null)
            {
                return found;
            }
        }
        
        return null;
    }

    /// <summary>
    /// Update het icoon in het informatieframe gebaseerd op het BuildableObjectSO van de knop
    /// </summary>
    private void UpdateInformatieframeIcon(Button button, GameObject informatieframe)
    {
        if (button == null || informatieframe == null) return;

        Sprite iconSprite = null;
        string sourceName = "";
        
        // Prioriteit 1: Zoek naar ObjectIcon in dezelfde ItemSpawnButton container
        Transform itemSpawnButton = button.transform;
        
        // Ga omhoog tot we het ItemSpawnButton vinden
        while (itemSpawnButton != null && !itemSpawnButton.name.Contains("ItemSpawnButton"))
        {
            itemSpawnButton = itemSpawnButton.parent;
        }
        
        if (itemSpawnButton != null)
        {
            if (debugMode)
            {
                Debug.Log($"[UIMenuController] ItemSpawnButton gevonden: {itemSpawnButton.name}");
            }
            
            // Zoek naar ObjectIcon binnen dit ItemSpawnButton
            Transform buttonObjectIcon = FindTransformRecursive(itemSpawnButton, "ObjectIcon");
            
            if (buttonObjectIcon != null)
            {
                Image buttonIconImage = buttonObjectIcon.GetComponent<Image>();
                if (buttonIconImage != null && buttonIconImage.sprite != null)
                {
                    iconSprite = buttonIconImage.sprite;
                    sourceName = "ItemSpawnButton ObjectIcon";
                    if (debugMode)
                    {
                        Debug.Log($"[UIMenuController] Sprite gevonden in ItemSpawnButton ObjectIcon: {iconSprite.name}");
                    }
                }
                else
                {
                    if (debugMode)
                    {
                        Debug.LogWarning($"[UIMenuController] ObjectIcon gevonden maar geen sprite: {buttonObjectIcon.name}");
                    }
                }
            }
            else
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[UIMenuController] Geen ObjectIcon gevonden in ItemSpawnButton: {itemSpawnButton.name}");
                    Debug.Log("=== ItemSpawnButton hiërarchie ===");
                    LogAllChildren(itemSpawnButton, "");
                }
            }
        }
        
        // Fallback: Probeer het BuildableObjectSO te vinden (alleen als geen sprite gevonden)
        if (iconSprite == null)
        {
            BuildableObjectSO buildableObjectSO = GetBuildableObjectSOFromButton(button);
            if (buildableObjectSO?.objectIcon != null) 
            {
                iconSprite = buildableObjectSO.objectIcon;
                sourceName = $"BuildableObjectSO ({buildableObjectSO.objectName})";
                if (debugMode)
                {
                    Debug.Log($"[UIMenuController] Sprite gevonden via BuildableObjectSO: {iconSprite.name}");
                }
            }
        }
        
        if (iconSprite == null)
        {
            if (debugMode)
            {
                Debug.LogWarning($"[UIMenuController] Geen sprite gevonden voor knop: {button.name}");
            }
            return;
        }

        // Zoek naar het icoon element in het informatieframe
        Image iconImage = FindIconImageInInformatieframe(informatieframe);
        
        if (iconImage != null)
        {
            iconImage.sprite = iconSprite;
            iconImage.enabled = true;
            iconImage.color = Color.white;
            
            if (debugMode)
            {
                Debug.Log($"[UIMenuController] Icoon succesvol bijgewerkt in informatieframe van {sourceName}: {iconSprite.name}");
            }
        }
        else
        {
            if (debugMode)
            {
                Debug.LogWarning($"[UIMenuController] Geen geschikt icoon element gevonden in informatieframe voor knop: {button.name}");
            }
        }
    }

    /// <summary>
    /// Probeer het BuildableObjectSO te verkrijgen dat hoort bij de knop
    /// </summary>
    private BuildableObjectSO GetBuildableObjectSOFromButton(Button button)
    {
        if (debugMode)
        {
            Debug.Log($"[UIMenuController] Zoeken naar BuildableObjectSO voor knop: {button.name}");
        }

        // Methode 1: Via ScriptableObjectButtonSpawner component - directe knop naam
        ScriptableObjectButtonSpawner spawner = FindObjectOfType<ScriptableObjectButtonSpawner>();
        if (spawner != null)
        {
            // Probeer de naam van de knop te gebruiken om het SO te vinden
            string buttonName = button.name;
            if (buttonName.StartsWith("ItemSpawnButton_"))
            {
                string objectName = buttonName.Substring("ItemSpawnButton_".Length);
                BuildableObjectSO result = spawner.GetBuildableObjectSOByName(objectName);
                if (result != null)
                {
                    if (debugMode)
                    {
                        Debug.Log($"[UIMenuController] BuildableObjectSO gevonden via directe knop naam: {result.objectName}");
                    }
                    return result;
                }
            }
        }

        // Methode 2: Zoek in parent hiërarchie naar ItemSpawnButton
        Transform current = button.transform;
        while (current != null)
        {
            if (current.name.StartsWith("ItemSpawnButton_"))
            {
                string objectName = current.name.Substring("ItemSpawnButton_".Length);
                if (spawner != null)
                {
                    BuildableObjectSO result = spawner.GetBuildableObjectSOByName(objectName);
                    if (result != null)
                    {
                        if (debugMode)
                        {
                            Debug.Log($"[UIMenuController] BuildableObjectSO gevonden via parent hiërarchie: {result.objectName}");
                        }
                        return result;
                    }
                }
            }
            current = current.parent;
        }

        if (debugMode)
        {
            Debug.LogWarning($"[UIMenuController] Geen BuildableObjectSO gevonden voor knop: {button.name}");
        }

        // Als geen van de methoden werkt, geef null terug
        return null;
    }

    /// <summary>
    /// Zoek het Image component dat gebruikt wordt voor het icoon in het informatieframe
    /// </summary>
    private Image FindIconImageInInformatieframe(GameObject informatieframe)
    {
        // Zoek naar verschillende mogelijke namen en paden
        string[] iconNames = { "Object-Icon", "object-icoon", "ObjectIcon", "Icon", "object-icon" };
        string[] containerNames = { "Object-Icon-Container", "object-icoon container", "IconContainer", "icon-container" };

        // Eerst proberen via container
        foreach (string containerName in containerNames)
        {
            Transform container = FindTransformRecursive(informatieframe.transform, containerName);
            if (container != null)
            {
                if (debugMode)
                {
                    Debug.Log($"[UIMenuController] Container gevonden: {containerName}");
                }
                
                foreach (string iconName in iconNames)
                {
                    Transform icon = FindTransformRecursive(container, iconName);
                    if (icon != null)
                    {
                        if (debugMode)
                        {
                            Debug.Log($"[UIMenuController] Icon gevonden: {iconName} in container {containerName}");
                        }
                        
                        Image iconImage = icon.GetComponent<Image>();
                        if (iconImage != null) 
                        {
                            if (debugMode)
                            {
                                Debug.Log($"[UIMenuController] Image component gevonden op {iconName}");
                            }
                            return iconImage;
                        }
                    }
                }
            }
        }

        // Als geen container gevonden, zoek direct in informatieframe
        foreach (string iconName in iconNames)
        {
            Transform icon = FindTransformRecursive(informatieframe.transform, iconName);
            if (icon != null)
            {
                if (debugMode)
                {
                    Debug.Log($"[UIMenuController] Direct icon gevonden: {iconName}");
                }
                
                Image iconImage = icon.GetComponent<Image>();
                if (iconImage != null) return iconImage;
            }
        }

        if (debugMode)
        {
            Debug.LogWarning($"[UIMenuController] Geen icon Image gevonden in informatieframe");
            // Debug: laat alle child objecten zien
            Debug.Log("=== Volledige hiërarchie van informatieframe ===");
            LogAllChildren(informatieframe.transform, "");
        }

        return null;
    }

    /// <summary>
    /// Helper methode om recursief een Transform te vinden op naam
    /// </summary>
    private Transform FindTransformRecursive(Transform parent, string name)
    {
        if (parent.name == name)
            return parent;
            
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindTransformRecursive(parent.GetChild(i), name);
            if (result != null)
                return result;
        }
        return null;
    }

    /// <summary>
    /// Debug methode om alle child objecten van een Transform te loggen
    /// </summary>
    private void LogAllChildren(Transform parent, string indent)
    {
        Debug.Log(indent + parent.name);
        foreach (Transform child in parent)
        {
            LogAllChildren(child, indent + "  ");
        }
    }
} 