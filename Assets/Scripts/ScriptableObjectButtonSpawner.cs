using UnityEngine;
using UnityEngine.UI;
using SoulGames.EasyGridBuilderPro;
using TMPro;

public class ScriptableObjectButtonSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject buttonTemplate;
    [SerializeField] private DynamicUILayoutFixer layoutFixer;
    
    [Header("ScriptableObject Data")]
    [SerializeField] private BuildableObjectSO[] itemData;

    [Header("Debug Tools")]
    [SerializeField] private bool showDebugTools = true;
    
    void Start()
    {
        // Automatisch referenties vinden
        if (contentParent == null)
        {
            // Probeer eerst een specifieke container te vinden
            GameObject buttonContainer = GameObject.Find("ButtonContainer");
            if (buttonContainer != null)
            {
                contentParent = buttonContainer.transform;
                Debug.Log($"[ScriptableObjectButtonSpawner] Automatisch ButtonContainer gevonden: {buttonContainer.name}");
            }
            else
            {
                // Fallback naar huidige transform
                contentParent = transform;
                Debug.Log($"[ScriptableObjectButtonSpawner] Geen ButtonContainer gevonden, gebruik current transform: {transform.name}");
            }
        }
            
        if (layoutFixer == null)
            layoutFixer = GetComponent<DynamicUILayoutFixer>();
            
        if (buttonTemplate == null)
            buttonTemplate = FindInactiveTemplateButton();
            
        // Spawn knoppen voor alle data
        SpawnButtonsFromData();
    }

    void Update()
    {
        // TIJDELIJKE DEBUG: Press R key om alle buttons te herconfigureren
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("[ScriptableObjectButtonSpawner] R-toets ingedrukt - herconfigureren alle buttons");
            ReconfigureAllButtons();
        }
    }
    
    GameObject FindInactiveTemplateButton()
    {
        // Zoek naar Template Button BG (de originele template)
        for (int i = 0; i < contentParent.childCount; i++)
        {
            Transform child = contentParent.GetChild(i);
            if (child.name.Contains("Template Button BG") && !child.name.Contains("(Clone)"))
            {
                return child.gameObject;
            }
        }
        return null;
    }
    
    void SpawnButtonsFromData()
    {
        if (itemData == null || buttonTemplate == null) return;
        
        for (int i = 0; i < itemData.Length; i++)
        {
            SpawnButton(itemData[i], i);
        }
        
        // Fix layout na het spawnen van alle knoppen
        if (layoutFixer != null)
        {
            layoutFixer.ResetAllButtonPositions();
        }
    }
    
    public GameObject SpawnButton(BuildableObjectSO data, int index = -1)
    {
        if (buttonTemplate == null || contentParent == null) return null;
        
        // Instantiate nieuwe knop
        GameObject newButton = Instantiate(buttonTemplate, contentParent);
        newButton.name = $"ItemSpawnButton_{data.objectName}";
        newButton.SetActive(true);
        
        // Configureer de knop met data
        ConfigureButton(newButton, data);
        
        // Fix layout
        if (layoutFixer != null)
        {
            layoutFixer.OnButtonSpawned(newButton);
        }
        
        return newButton;
    }
    
    void ConfigureButton(GameObject button, BuildableObjectSO data)
    {
        if (data == null) 
        {
            Debug.LogError("[ScriptableObjectButtonSpawner] ConfigureButton: data is null!");
            return;
        }

        Debug.Log($"[ScriptableObjectButtonSpawner] *** CONFIGUREREN VAN KNOP VOOR: {data.objectName} ***");
        Debug.Log($"[ScriptableObjectButtonSpawner] Button naam: {button.name}");

        // Zoek specifiek naar ObjectIcon GameObject in de knop
        Transform iconTransform = null;
        
        // Eerst proberen we het directe pad
        iconTransform = button.transform.Find("ButtonContainer/Btn_Button/ObjectIcon");
        
        if (iconTransform == null)
        {
            // Als het directe pad niet werkt, zoeken we recursief
            iconTransform = FindTransformRecursive(button.transform, "ObjectIcon");
        }
        
        if (iconTransform != null)
        {
            Debug.Log($"[ScriptableObjectButtonSpawner] ObjectIcon GameObject gevonden: {iconTransform.name} op pad: {GetFullPath(iconTransform)}");
            
            // Zorg ervoor dat we alleen de Image component op dit specifieke GameObject pakken
            Image buttonImage = iconTransform.GetComponent<Image>();
            if (buttonImage != null)
            {
                Debug.Log($"[ScriptableObjectButtonSpawner] Huidige sprite op ObjectIcon: {buttonImage.sprite?.name}");
                Debug.Log($"[ScriptableObjectButtonSpawner] Nieuwe sprite instellen: {data.objectIcon?.name}");
                
                // Stel de nieuwe sprite in
                buttonImage.sprite = data.objectIcon;
                buttonImage.enabled = data.objectIcon != null;
                
                // Reset de kleur om zichtbaarheid te garanderen
                if (data.objectIcon != null)
                {
                    buttonImage.color = Color.white;
                    Debug.Log($"[ScriptableObjectButtonSpawner] Sprite succesvol ingesteld op ObjectIcon: {data.objectIcon.name}");
                }
                else
                {
                    Debug.LogWarning($"[ScriptableObjectButtonSpawner] Geen objectIcon gevonden in ScriptableObject {data.objectName}");
                }
                
                // Verificatie: controleer of de sprite echt is ingesteld
                Debug.Log($"[ScriptableObjectButtonSpawner] Verificatie - sprite na instelling: {buttonImage.sprite?.name}");
            }
            else
            {
                Debug.LogError($"[ScriptableObjectButtonSpawner] Geen Image component gevonden op ObjectIcon GameObject voor {data.objectName}");
            }
        }
        else
        {
            Debug.LogError($"[ScriptableObjectButtonSpawner] Kon 'ObjectIcon' GameObject niet vinden voor {data.objectName}");
            // Debug: laat alle child objecten zien
            Debug.Log("=== Volledige hiërarchie van de knop ===");
            LogAllChildren(button.transform, "");
        }

        // NIEUW: Ook het plaatje instellen in het informatieframe
        SetIconInInformationFrame(button, data);
        
        // Zoek TextMeshPro component en zet naam
        Transform nameTransform = button.transform.Find("ButtonContainer/Naam");
        if (nameTransform == null)
        {
            nameTransform = FindTransformRecursive(button.transform, "Naam");
        }
        
        if (nameTransform != null)
        {
            TextMeshProUGUI buttonText = nameTransform.GetComponent<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = data.objectName;
                Debug.Log($"[ScriptableObjectButtonSpawner] Tekst ingesteld op {nameTransform.name}: {data.objectName}");
            }
            else
            {
                Debug.LogWarning($"[ScriptableObjectButtonSpawner] Geen TextMeshProUGUI component gevonden op 'Naam' voor {data.objectName}");
            }
        }
        else
        {
            Debug.LogWarning($"[ScriptableObjectButtonSpawner] Kon 'Naam' GameObject niet vinden voor {data.objectName}");
        }
        
        // Zoek Button component en voeg onClick listener toe
        Transform buttonTransform = button.transform.Find("ButtonContainer/Btn_Button");
        if (buttonTransform == null)
        {
            buttonTransform = FindTransformRecursive(button.transform, "Btn_Button");
        }
        
        if (buttonTransform != null)
        {
            Button buttonComponent = buttonTransform.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.RemoveAllListeners();
                buttonComponent.onClick.AddListener(() => OnButtonClicked(data));
            }
        }
        
        // NIEUW: Configureer Product Information velden
        ConfigureProductInformation(button, data);
    }
    
    /// <summary>
    /// Configureer alle Product Information velden in het item spawn button menu
    /// </summary>
    void ConfigureProductInformation(GameObject button, BuildableObjectSO data)
    {
        if (data == null) 
        {
            Debug.LogError("[ScriptableObjectButtonSpawner] ConfigureProductInformation: data is null!");
            return;
        }

        Debug.Log($"[ScriptableObjectButtonSpawner] *** CONFIGUREREN PRODUCT INFORMATION VOOR: {data.objectName} ***");
        Debug.Log($"[ScriptableObjectButtonSpawner] Button voor product info: {button.name}");

        // Extra debug: Controleer of de data daadwerkelijk gevuld is
        Debug.Log($"[ScriptableObjectButtonSpawner] DATA CONTROLE voor {data.objectName}:");
        Debug.Log($"  - Omschrijving: '{data.omschrijving}' (length: {data.omschrijving?.Length ?? 0})");
        Debug.Log($"  - Levensduur: '{data.levensduur}' (length: {data.levensduur?.Length ?? 0})");
        Debug.Log($"  - Recyclebaar: '{data.recyclebaar}' (length: {data.recyclebaar?.Length ?? 0})");
        Debug.Log($"  - Prijs: '{data.prijs}' (length: {data.prijs?.Length ?? 0})");
        Debug.Log($"  - Certificering: '{data.beschikbareCertificering}' (length: {data.beschikbareCertificering?.Length ?? 0})");
        Debug.Log($"  - CO2: '{data.co2}' (length: {data.co2?.Length ?? 0})");

        // Zoek het informatieframe binnen de button hiërarchie  
        Transform informatieframe = FindTransformRecursive(button.transform, "Informatieframe");
        if (informatieframe == null)
        {
            informatieframe = FindTransformRecursive(button.transform, "Informatie inhoud frame");
        }

        if (informatieframe == null)
        {
            Debug.LogWarning($"[ScriptableObjectButtonSpawner] Informatieframe niet gevonden voor {data.objectName}");
            Debug.Log($"[ScriptableObjectButtonSpawner] Zoeken naar alle mogelijke frames...");
            
            // Debug: Zoek naar alle mogelijke frame namen
            Transform[] allChildren = button.GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                if (child.name.ToLower().Contains("frame") || child.name.ToLower().Contains("info"))
                {
                    Debug.Log($"[ScriptableObjectButtonSpawner] Mogelijk frame gevonden: {child.name} op pad: {GetFullPath(child)}");
                }
            }
            return;
        }

        Debug.Log($"[ScriptableObjectButtonSpawner] Informatieframe gevonden: {informatieframe.name} op pad: {GetFullPath(informatieframe)}");

        // DEBUG: Laat alle child objecten zien in het informatieframe
        Debug.Log($"=== INFORMATIEFRAME STRUCTUUR VOOR {data.objectName} ===");
        LogAllChildren(informatieframe, "");

        // Dictionary voor field mapping: UI element naam -> ScriptableObject veld
        // Namen gebaseerd op screenshot hiërarchie
        var fieldMappings = new System.Collections.Generic.Dictionary<string, string>
        {
            { "Omschrijving", data.omschrijving },
            { "Levensduur", data.levensduur },
            { "Recyclebaar", data.recyclebaar }, 
            { "Prijs", data.prijs },
            { "Beschikbare certificeringen", data.beschikbareCertificering },
            { "CO2", data.co2 }
        };

        Debug.Log($"[ScriptableObjectButtonSpawner] Data check voor {data.objectName}:");
        Debug.Log($"  - Omschrijving: '{data.omschrijving}'");
        Debug.Log($"  - Levensduur: '{data.levensduur}'"); 
        Debug.Log($"  - Recyclebaar: '{data.recyclebaar}'");
        Debug.Log($"  - Prijs: '{data.prijs}'");
        Debug.Log($"  - Certificering: '{data.beschikbareCertificering}'");
        Debug.Log($"  - CO2: '{data.co2}'");

        // Probeer eerst de standaard methode
        foreach (var mapping in fieldMappings)
        {
            ConfigureTextField(informatieframe, mapping.Key, mapping.Value, data.objectName);
        }

        // Als de standaard methode niet werkt, probeer de alternatieve methode
        ConfigureTextFieldsAlternative(informatieframe, data);

        // Configureer certificering plaatjes apart
        ConfigureCertificatieImages(informatieframe, data);
    }

    /// <summary>
    /// Configureer een individueel tekstveld in het informatieframe
    /// </summary>
    void ConfigureTextField(Transform informatieframe, string fieldName, string fieldValue, string objectName)
    {
        Debug.Log($"[ScriptableObjectButtonSpawner] Zoeken naar veld '{fieldName}' voor {objectName}");
        
        // VERBETERD: Meerdere zoekstrategieën
        Transform fieldTransform = null;
        
        // Strategie 1: Exacte naam matching (case-insensitive)
        fieldTransform = FindChildByNameCaseInsensitive(informatieframe, fieldName);
        if (fieldTransform != null)
        {
            Debug.Log($"[ScriptableObjectButtonSpawner] Veld gevonden via exacte naam: {GetFullPath(fieldTransform)}");
        }
        
        // Strategie 2: Partial matching (bevat de fieldName)
        if (fieldTransform == null)
        {
            Transform[] allChildren = informatieframe.GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                if (child.name.ToLower().Contains(fieldName.ToLower()))
                {
                    fieldTransform = child;
                    Debug.Log($"[ScriptableObjectButtonSpawner] Veld gevonden via partial matching: {GetFullPath(fieldTransform)}");
                    break;
                }
            }
        }
        
        // Strategie 3: Zoek naar TextMeshPro componenten met matching text
        if (fieldTransform == null)
        {
            TextMeshProUGUI[] allTextComponents = informatieframe.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI textComp in allTextComponents)
            {
                if (textComp.text.ToLower().Contains(fieldName.ToLower()) || 
                    textComp.gameObject.name.ToLower().Contains(fieldName.ToLower()))
                {
                    fieldTransform = textComp.transform;
                    Debug.Log($"[ScriptableObjectButtonSpawner] Veld gevonden via TextMeshPro text/naam matching: {GetFullPath(fieldTransform)}");
                    break;
                }
            }
        }
        
        // Strategie 4: Specifieke structuur matching voor ToggleButton -> InfoButtonContainer
        if (fieldTransform == null)
        {
            Transform[] toggleButtons = informatieframe.GetComponentsInChildren<Transform>();
            
            foreach (Transform toggleButton in toggleButtons)
            {
                if (toggleButton.name.ToLower().Contains("togglebutton"))
                {
                    Debug.Log($"[ScriptableObjectButtonSpawner] Gevonden togglebutton: {toggleButton.name}");
                    
                    // Zoek InfoButtonContainer binnen deze togglebutton
                    Transform infoButtonContainer = null;
                    foreach (Transform child in toggleButton)
                    {
                        if (child.name.ToLower().Contains("infobuttoncontainer"))
                        {
                            infoButtonContainer = child;
                            Debug.Log($"[ScriptableObjectButtonSpawner] Gevonden InfoButtonContainer: {child.name}");
                            break;
                        }
                    }
                    
                    // Zoek het specifieke veld binnen InfoButtonContainer
                    if (infoButtonContainer != null)
                    {
                        foreach (Transform fieldChild in infoButtonContainer)
                        {
                            if (fieldChild.name.Equals(fieldName, System.StringComparison.OrdinalIgnoreCase))
                            {
                                fieldTransform = fieldChild;
                                Debug.Log($"[ScriptableObjectButtonSpawner] Veld gevonden via ToggleButton structuur: {GetFullPath(fieldChild)}");
                                break;
                            }
                        }
                        
                        if (fieldTransform != null) break;
                    }
                }
            }
        }

        if (fieldTransform != null)
        {
            // Zoek TextMeshProUGUI component
            TextMeshProUGUI textComponent = fieldTransform.GetComponent<TextMeshProUGUI>();
            
            if (textComponent == null)
            {
                // Probeer in child objecten
                textComponent = fieldTransform.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    Debug.Log($"[ScriptableObjectButtonSpawner] TextMeshPro gevonden in child: {textComponent.gameObject.name}");
                }
            }

            if (textComponent != null)
            {
                string valueToSet = string.IsNullOrEmpty(fieldValue) ? "Niet opgegeven" : fieldValue;
                string oldValue = textComponent.text;
                textComponent.text = valueToSet;
                Debug.Log($"[ScriptableObjectButtonSpawner] ✓ {fieldName} SUCCESVOL ingesteld voor {objectName}");
                Debug.Log($"[ScriptableObjectButtonSpawner]   Oude waarde: '{oldValue}'");
                Debug.Log($"[ScriptableObjectButtonSpawner]   Nieuwe waarde: '{valueToSet}'");
            }
            else
            {
                Debug.LogWarning($"[ScriptableObjectButtonSpawner] ✗ Geen TextMeshProUGUI component gevonden voor {fieldName} in {objectName}");
                
                // DEBUG: Laat componenten zien op dit GameObject
                Component[] components = fieldTransform.GetComponents<Component>();
                Debug.Log($"Componenten op {fieldTransform.name}: {string.Join(", ", System.Array.ConvertAll(components, c => c.GetType().Name))}");
            }
        }
        else
        {
            Debug.LogWarning($"[ScriptableObjectButtonSpawner] ✗ UI element '{fieldName}' niet gevonden voor {objectName}");
            
            // DEBUG: Laat alle beschikbare UI elementen zien
            Debug.Log($"[ScriptableObjectButtonSpawner] Beschikbare UI elementen in informatieframe:");
            Transform[] allChildren = informatieframe.GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                if (child != informatieframe) // Skip de parent zelf
                {
                    Debug.Log($"  - {child.name} (pad: {GetFullPath(child)})");
                }
            }
        }
    }

    /// <summary>
    /// Hulp methode om child te vinden op basis van naam (case-insensitive)
    /// </summary>
    Transform FindChildByNameCaseInsensitive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (string.Equals(child.name, name, System.StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }
            
            // Recursief zoeken in children
            Transform found = FindChildByNameCaseInsensitive(child, name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    /// <summary>
    /// Alternatieve methode om tekstvelden te configureren - zoekt naar alle TextMeshPro componenten
    /// </summary>
    void ConfigureTextFieldsAlternative(Transform informatieframe, BuildableObjectSO data)
    {
        Debug.Log($"[ScriptableObjectButtonSpawner] Alternatieve methode voor {data.objectName}");
        
        // Vind alle TextMeshPro componenten in het informatieframe
        TextMeshProUGUI[] allTextComponents = informatieframe.GetComponentsInChildren<TextMeshProUGUI>();
        
        Debug.Log($"[ScriptableObjectButtonSpawner] Gevonden {allTextComponents.Length} TextMeshPro componenten");
        
        foreach (TextMeshProUGUI textComponent in allTextComponents)
        {
            string componentName = textComponent.gameObject.name.ToLower();
            string componentPath = GetFullPath(textComponent.transform);
            
            Debug.Log($"[ScriptableObjectButtonSpawner] TextMeshPro component: {textComponent.gameObject.name} (pad: {componentPath})");
            
            // Match op basis van component naam
            if (componentName.Contains("omschrijving") || componentName.Contains("description"))
            {
                textComponent.text = string.IsNullOrEmpty(data.omschrijving) ? "Niet opgegeven" : data.omschrijving;
                Debug.Log($"[ScriptableObjectButtonSpawner] Omschrijving ingesteld via alternatieve methode: {textComponent.text}");
            }
            else if (componentName.Contains("levensduur") || componentName.Contains("lifespan"))
            {
                textComponent.text = string.IsNullOrEmpty(data.levensduur) ? "Niet opgegeven" : data.levensduur;
                Debug.Log($"[ScriptableObjectButtonSpawner] Levensduur ingesteld via alternatieve methode: {textComponent.text}");
            }
            else if (componentName.Contains("recyclebaar") || componentName.Contains("recyclable"))
            {
                textComponent.text = string.IsNullOrEmpty(data.recyclebaar) ? "Niet opgegeven" : data.recyclebaar;
                Debug.Log($"[ScriptableObjectButtonSpawner] Recyclebaar ingesteld via alternatieve methode: {textComponent.text}");
            }
            else if (componentName.Contains("prijs") || componentName.Contains("price"))
            {
                textComponent.text = string.IsNullOrEmpty(data.prijs) ? "Niet opgegeven" : data.prijs;
                Debug.Log($"[ScriptableObjectButtonSpawner] Prijs ingesteld via alternatieve methode: {textComponent.text}");
            }
            else if (componentName.Contains("certificering") || componentName.Contains("certification"))
            {
                textComponent.text = string.IsNullOrEmpty(data.beschikbareCertificering) ? "Niet opgegeven" : data.beschikbareCertificering;
                Debug.Log($"[ScriptableObjectButtonSpawner] Certificering ingesteld via alternatieve methode: {textComponent.text}");
            }
            else if (componentName.Contains("co2") || componentName.Contains("carbon"))
            {
                textComponent.text = string.IsNullOrEmpty(data.co2) ? "Niet opgegeven" : data.co2;
                Debug.Log($"[ScriptableObjectButtonSpawner] CO2 ingesteld via alternatieve methode: {textComponent.text}");
            }
        }
    }

    /// <summary>
    /// Configureer certificering afbeeldingen in het informatieframe
    /// </summary>
    void ConfigureCertificatieImages(Transform informatieframe, BuildableObjectSO data)
    {
        if (data.certificeringPlaatjes == null || data.certificeringPlaatjes.Length == 0) return;

        // Zoek naar certificering container
        string[] containerNames = { "Certificering Plaatjes", "CertificeringPlaatjes", "Certificeringen", "Certificates" };
        Transform certificateContainer = null;

        foreach (string containerName in containerNames)
        {
            certificateContainer = FindTransformRecursive(informatieframe, containerName);
            if (certificateContainer != null) break;
        }

        if (certificateContainer != null)
        {
            // Deactiveer bestaande certificate images
            foreach (Transform child in certificateContainer)
            {
                child.gameObject.SetActive(false);
            }

            // Activeer en configureer de benodigde afbeeldingen
            for (int i = 0; i < data.certificeringPlaatjes.Length && i < certificateContainer.childCount; i++)
            {
                Transform certificateImage = certificateContainer.GetChild(i);
                Image imageComponent = certificateImage.GetComponent<Image>();
                
                if (imageComponent != null && data.certificeringPlaatjes[i] != null)
                {
                    imageComponent.sprite = data.certificeringPlaatjes[i];
                    imageComponent.enabled = true;
                    certificateImage.gameObject.SetActive(true);
                    Debug.Log($"[ScriptableObjectButtonSpawner] Certificering afbeelding {i} ingesteld voor {data.objectName}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[ScriptableObjectButtonSpawner] Certificering container niet gevonden voor {data.objectName}");
        }
    }
    
    /// <summary>
    /// Stel hetzelfde icon in binnen het informatieframe
    /// </summary>
    void SetIconInInformationFrame(GameObject button, BuildableObjectSO data)
    {
        if (data?.objectIcon == null) return;

        Debug.Log($"[ScriptableObjectButtonSpawner] Proberen icon in te stellen voor informatieframe: {data.objectName}");

        // Zoek het informatieframe binnen de button hiërarchie
        Transform informatieframe = FindTransformRecursive(button.transform, "Informatieframe");
        
        if (informatieframe == null)
        {
            // Als Informatieframe niet gevonden, probeer "Informatie inhoud frame"
            informatieframe = FindTransformRecursive(button.transform, "Informatie inhoud frame");
        }

        if (informatieframe != null)
        {
            Debug.Log($"[ScriptableObjectButtonSpawner] Informatieframe gevonden: {informatieframe.name}");
            
            // Zoek naar Object-Icon-Container en vervolgens Object-Icon
            Transform iconContainer = FindTransformRecursive(informatieframe, "Object-Icon-Container");
            if (iconContainer != null)
            {
                Debug.Log($"[ScriptableObjectButtonSpawner] Object-Icon-Container gevonden: {iconContainer.name}");
                
                Transform infoIcon = FindTransformRecursive(iconContainer, "Object-Icon");
                if (infoIcon != null)
                {
                    Debug.Log($"[ScriptableObjectButtonSpawner] Object-Icon gevonden: {infoIcon.name}");
                    
                    Image infoIconImage = infoIcon.GetComponent<Image>();
                    if (infoIconImage != null)
                    {
                        infoIconImage.sprite = data.objectIcon;
                        infoIconImage.enabled = true;
                        infoIconImage.color = Color.white;
                        Debug.Log($"[ScriptableObjectButtonSpawner] Icon succesvol ingesteld in informatieframe voor: {data.objectName}");
                    }
                    else
                    {
                        Debug.LogWarning($"[ScriptableObjectButtonSpawner] Geen Image component gevonden op Object-Icon in informatieframe voor {data.objectName}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[ScriptableObjectButtonSpawner] Object-Icon niet gevonden in Object-Icon-Container voor {data.objectName}");
                }
            }
            else
            {
                Debug.LogWarning($"[ScriptableObjectButtonSpawner] Object-Icon-Container niet gevonden, proberen fallback opties...");
                
                // Fallback: probeer direct te zoeken naar een ObjectIcon of andere namen
                string[] fallbackNames = { "Object-Icon", "object-icoon", "ObjectIcon", "Icon" };
                Transform fallbackIcon = null;
                
                foreach (string name in fallbackNames)
                {
                    fallbackIcon = FindTransformRecursive(informatieframe, name);
                    if (fallbackIcon != null)
                    {
                        Debug.Log($"[ScriptableObjectButtonSpawner] Fallback icon gevonden: {name}");
                        break;
                    }
                }
                
                if (fallbackIcon != null)
                {
                    Image fallbackIconImage = fallbackIcon.GetComponent<Image>();
                    if (fallbackIconImage != null)
                    {
                        fallbackIconImage.sprite = data.objectIcon;
                        fallbackIconImage.enabled = true;
                        fallbackIconImage.color = Color.white;
                        Debug.Log($"[ScriptableObjectButtonSpawner] Icon succesvol ingesteld via fallback in informatieframe voor: {data.objectName}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[ScriptableObjectButtonSpawner] Geen geschikte icon container of element gevonden in informatieframe voor {data.objectName}");
                    // Debug: laat alle child objecten van informatieframe zien
                    Debug.Log("=== Volledige hiërarchie van informatieframe ===");
                    LogAllChildren(informatieframe, "");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[ScriptableObjectButtonSpawner] Informatieframe niet gevonden voor {data.objectName}");
            // Debug: laat alle child objecten van de knop zien
            Debug.Log("=== Volledige hiërarchie van de knop ===");
            LogAllChildren(button.transform, "");
        }
    }
    
    // Helper functie om recursief een GameObject te vinden op naam
    Transform FindTransformRecursive(Transform parent, string name)
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
    
    // Debug functie om alle children te loggen
    void LogAllChildren(Transform parent, string indent)
    {
        Debug.Log($"{indent}{parent.name}");
        for (int i = 0; i < parent.childCount; i++)
        {
            LogAllChildren(parent.GetChild(i), indent + "  ");
        }
    }
    
    void OnButtonClicked(BuildableObjectSO data)
    {
        Debug.Log($"Button clicked with data: {data.objectName}");
        // Hier kun je de logica toevoegen voor wat er gebeurt als de knop wordt geklikt
    }
    
    // Public methode om handmatig layout te fixen
    public void FixLayout()
    {
        if (layoutFixer != null)
        {
            layoutFixer.ResetAllButtonPositions();
        }
    }
    
    // Public methode om alle gekloonde knoppen te verwijderen
    public void ClearAllButtons()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Transform child = contentParent.GetChild(i);
            if (child.name.Contains("(Clone)"))
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
    
    // Helper functie om het volledige pad van een Transform te krijgen
    string GetFullPath(Transform transform)
    {
        string path = transform.name;
        Transform current = transform.parent;
        
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        
        return path;
    }

    /// <summary>
    /// Public methode om BuildableObjectSO te vinden op basis van object naam
    /// </summary>
    public BuildableObjectSO GetBuildableObjectSOByName(string objectName)
    {
        if (itemData == null || string.IsNullOrEmpty(objectName)) return null;
        
        foreach (var item in itemData)
        {
            if (item != null && item.objectName == objectName)
            {
                return item;
            }
        }
        
        return null;
    }

    /// <summary>
    /// Herconfigueer alle bestaande buttons met de nieuwe Product Information
    /// </summary>
    [ContextMenu("Reconfigure All Buttons")]
    public void ReconfigureAllButtons()
    {
        Debug.Log("[ScriptableObjectButtonSpawner] *** HERCONFIGURERERN ALLE BUTTONS ***");
        
        // Vind alle bestaande ItemSpawnButton objecten
        GameObject[] existingButtons = GameObject.FindGameObjectsWithTag("Untagged");
        
        foreach (GameObject button in existingButtons)
        {
            if (button.name.StartsWith("ItemSpawnButton_"))
            {
                Debug.Log($"[ScriptableObjectButtonSpawner] Gevonden bestaande button: {button.name}");
                
                // Extract object naam uit button naam
                string objectName = button.name.Replace("ItemSpawnButton_", "");
                
                // Vind de bijhorende BuildableObjectSO
                BuildableObjectSO data = GetBuildableObjectSOByName(objectName);
                
                if (data != null)
                {
                    Debug.Log($"[ScriptableObjectButtonSpawner] Herconfigurerern button voor: {data.objectName}");
                    ConfigureProductInformation(button, data);
                }
                else
                {
                    Debug.LogWarning($"[ScriptableObjectButtonSpawner] Geen BuildableObjectSO gevonden voor: {objectName}");
                }
            }
        }
    }

    /// <summary>
    /// Debug methode om te controleren welke ScriptableObjects er geladen zijn
    /// </summary>
    [ContextMenu("Debug - Show Loaded ScriptableObjects")]
    public void DebugShowLoadedScriptableObjects()
    {
        Debug.Log("[ScriptableObjectButtonSpawner] *** DEBUG: GELADEN SCRIPTABLEOBJECTS ***");
        
        if (itemData == null)
        {
            Debug.LogError("[ScriptableObjectButtonSpawner] itemData array is null!");
            return;
        }
        
        if (itemData.Length == 0)
        {
            Debug.LogWarning("[ScriptableObjectButtonSpawner] itemData array is leeg! Voeg ScriptableObjects toe in de Inspector.");
            return;
        }
        
        Debug.Log($"[ScriptableObjectButtonSpawner] Aantal ScriptableObjects in itemData: {itemData.Length}");
        
        for (int i = 0; i < itemData.Length; i++)
        {
            if (itemData[i] == null)
            {
                Debug.LogWarning($"[ScriptableObjectButtonSpawner] itemData[{i}] is null!");
                continue;
            }
            
            BuildableObjectSO so = itemData[i];
            Debug.Log($"[ScriptableObjectButtonSpawner] itemData[{i}]: {so.name}");
            Debug.Log($"  - objectName: '{so.objectName}'");
            Debug.Log($"  - omschrijving: '{so.omschrijving}' (length: {so.omschrijving?.Length ?? 0})");
            Debug.Log($"  - levensduur: '{so.levensduur}' (length: {so.levensduur?.Length ?? 0})");
            Debug.Log($"  - recyclebaar: '{so.recyclebaar}' (length: {so.recyclebaar?.Length ?? 0})");
            Debug.Log($"  - prijs: '{so.prijs}' (length: {so.prijs?.Length ?? 0})");
            Debug.Log($"  - certificering: '{so.beschikbareCertificering}' (length: {so.beschikbareCertificering?.Length ?? 0})");
            Debug.Log($"  - co2: '{so.co2}' (length: {so.co2?.Length ?? 0})");
            Debug.Log($"  - objectIcon: {(so.objectIcon != null ? so.objectIcon.name : "null")}");
        }
    }

    /// <summary>
    /// Debug methode om een specifiek ScriptableObject te testen
    /// </summary>
    [ContextMenu("Debug - Test Single Button Configuration")]
    public void DebugTestSingleButton()
    {
        Debug.Log("[ScriptableObjectButtonSpawner] *** DEBUG: TEST SINGLE BUTTON ***");
        
        if (itemData == null || itemData.Length == 0)
        {
            Debug.LogError("[ScriptableObjectButtonSpawner] Geen itemData beschikbaar voor test!");
            return;
        }
        
        // Test met het eerste ScriptableObject
        BuildableObjectSO testData = itemData[0];
        if (testData == null)
        {
            Debug.LogError("[ScriptableObjectButtonSpawner] Eerste itemData is null!");
            return;
        }
        
        Debug.Log($"[ScriptableObjectButtonSpawner] Test button creatie voor: {testData.objectName}");
        
        // Probeer een button te spawnen
        GameObject testButton = SpawnButton(testData, 999);
        if (testButton != null)
        {
            Debug.Log($"[ScriptableObjectButtonSpawner] Test button succesvol gecreëerd: {testButton.name}");
        }
        else
        {
            Debug.LogError("[ScriptableObjectButtonSpawner] Test button creatie mislukt!");
        }
    }
}
