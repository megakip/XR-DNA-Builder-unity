using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Zorgt ervoor dat het Vector-icoon van een knop wit wordt wanneer de knop actief is.
/// Plaats dit script op elke menuknop die een Vector child met Image component heeft.
/// </summary>
public class MenuButtonHighlighter : MonoBehaviour
{
    [Tooltip("De kleur van het icoon wanneer de knop actief is")]
    [SerializeField] private Color activeColor = Color.white;
    
    [Tooltip("De kleur van het icoon wanneer de knop niet actief is")]
    [SerializeField] private Color inactiveColor = new Color(0.7f, 0.7f, 0.7f); // Lichtgrijs
    
    [Tooltip("De Image component van het Vector-icoon (wordt automatisch gevonden als niet ingesteld)")]
    [SerializeField] private Image iconImage;
    
    // De knop waar dit script op zit
    private Button button;
    
    // Verwijzing naar de ButtonSelectionManager
    private ButtonSelectionManager selectionManager;
    
    private void Awake()
    {
        // Zoek de Button component
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("MenuButtonHighlighter vereist een Button component op hetzelfde GameObject: " + gameObject.name);
            return;
        }
        
        // Als geen iconImage is ingesteld, zoek deze automatisch in het Vector child-object
        if (iconImage == null)
        {
            Transform vectorTransform = transform.Find("Vector");
            if (vectorTransform != null)
            {
                iconImage = vectorTransform.GetComponent<Image>();
            }
            
            if (iconImage == null)
            {
                Debug.LogWarning("Geen Vector Image component gevonden voor knop: " + gameObject.name);
            }
        }
        
        // Zoek de ButtonSelectionManager in het canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            selectionManager = canvas.GetComponentInChildren<ButtonSelectionManager>();
        }
        
        if (selectionManager == null)
        {
            selectionManager = FindObjectOfType<ButtonSelectionManager>();
        }
        
        if (selectionManager == null)
        {
            Debug.LogWarning("Geen ButtonSelectionManager gevonden voor knop: " + gameObject.name);
        }
        else
        {
            // Abonneer op button click om de kleur bij te werken
            button.onClick.AddListener(OnButtonClicked);
        }
    }
    
    private void Start()
    {
        // Update de initiële staat
        UpdateIconColor();
    }
    
    private void OnEnable()
    {
        // Update de kleur wanneer het object wordt ingeschakeld
        UpdateIconColor();
    }
    
    private void Update()
    {
        // Controleer en update de kleur regelmatig (kan worden uitgezet als performance een probleem is)
        UpdateIconColor();
    }
    
    /// <summary>
    /// Wordt aangeroepen wanneer op de knop wordt geklikt
    /// </summary>
    private void OnButtonClicked()
    {
        // Na een kort moment updaten we de kleur (om te zorgen dat ButtonSelectionManager tijd heeft om bij te werken)
        Invoke("UpdateIconColor", 0.05f);
    }
    
    /// <summary>
    /// Update de kleur van het icoon op basis van of de knop actief is
    /// </summary>
    private void UpdateIconColor()
    {
        if (iconImage == null || selectionManager == null) return;
        
        // Controleer of dit de geselecteerde knop is in de ButtonSelectionManager
        bool isActive = IsButtonSelected();
        
        // Pas de kleur aan
        iconImage.color = isActive ? activeColor : inactiveColor;
    }
    
    /// <summary>
    /// Controleert of deze knop geselecteerd is in de ButtonSelectionManager
    /// </summary>
    private bool IsButtonSelected()
    {
        // Deze methode is een beste inschatting, mogelijk moet deze worden aangepast
        // afhankelijk van hoe de ButtonSelectionManager werkt
        
        // Als we direct kunnen controleren via de ButtonSelectionManager, doe dat dan
        if (selectionManager != null && button != null)
        {
            // Gebruik reflectie om de huidige geselecteerde knop op te halen
            // (dit is een hack omdat we niet weten hoe ButtonSelectionManager is geïmplementeerd)
            System.Type type = selectionManager.GetType();
            System.Reflection.FieldInfo field = type.GetField("currentSelectedButton", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                Button selectedButton = field.GetValue(selectionManager) as Button;
                return selectedButton == button;
            }
        }
        
        // Fallback methode: controleer of deze knop de geselecteerde UI-knop is
        return UnityEngine.EventSystems.EventSystem.current != null && 
               UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == gameObject;
    }
    
    /// <summary>
    /// Zet de kleur van het icoon naar de actieve staat (kan vanuit andere scripts worden aangeroepen)
    /// </summary>
    public void SetActive(bool active)
    {
        if (iconImage != null)
        {
            iconImage.color = active ? activeColor : inactiveColor;
        }
    }
} 