using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonToggle : MonoBehaviour
{
    [Header("Toggle Settings")]
    [SerializeField] private GameObject[] menusToToggle;
    [SerializeField] private bool startSelected = false;
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 0.973f, 0.941f); // #FFF8F0
    [SerializeField] private bool useOutline = true;
    
    [Header("Layout Settings")]
    [SerializeField] private bool preventLayoutInterference = true;
    [SerializeField] private bool useAbsolutePositioning = false;
    [SerializeField] private Vector2 contentOffset = new Vector2(0, -70);
    
    [Header("Audio Settings (Optional)")]
    [SerializeField] private AudioClip toggleOnSound;
    [SerializeField] private AudioClip toggleOffSound;
    
    private Button button;
    private Outline outline;
    private AudioSource audioSource;
    private bool isSelected = false;
    private LayoutElement[] contentLayoutElements;
    private RectTransform[] contentRectTransforms;

    // Public property om de toggle staat op te vragen
    public bool IsSelected => isSelected;

    private void Awake()
    {
        button = GetComponent<Button>();
        outline = GetComponent<Outline>();
        audioSource = GetComponent<AudioSource>();
        
        // Als er geen AudioSource is maar wel geluiden zijn ingesteld, voeg er een toe
        if (audioSource == null && (toggleOnSound != null || toggleOffSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Setup het content panel om layout interferentie te voorkomen
        SetupContentLayoutPrevention();
        
        // Zet de outline initieel uit (als we outline gebruiken)
        if (outline != null && useOutline)
        {
            outline.enabled = startSelected;
        }
        
        // Voeg de toggle functie toe aan de button click event
        button.onClick.AddListener(ToggleButton);
        
        // Zet de initiële staat
        SetToggleState(startSelected, false); // false = geen geluid bij start
    }

    private void SetupContentLayoutPrevention()
    {
        if (menusToToggle == null || menusToToggle.Length == 0) return;
        
        // Initialiseer arrays
        contentRectTransforms = new RectTransform[menusToToggle.Length];
        contentLayoutElements = new LayoutElement[menusToToggle.Length];
        
        for (int i = 0; i < menusToToggle.Length; i++)
        {
            if (menusToToggle[i] == null) continue;
            
            contentRectTransforms[i] = menusToToggle[i].GetComponent<RectTransform>();
            if (contentRectTransforms[i] == null) continue;
            
            if (preventLayoutInterference)
            {
                // Voeg LayoutElement toe en stel ignoreLayout in op true
                contentLayoutElements[i] = menusToToggle[i].GetComponent<LayoutElement>();
                if (contentLayoutElements[i] == null)
                {
                    contentLayoutElements[i] = menusToToggle[i].AddComponent<LayoutElement>();
                }
                contentLayoutElements[i].ignoreLayout = true;
                
                Debug.Log($"[ButtonToggle] Layout interference prevention enabled for {menusToToggle[i].name}");
            }
            
            if (useAbsolutePositioning)
            {
                // Zet absolute positioning voor het content panel
                SetupAbsolutePositioning(i);
            }
        }
    }
    
    private void SetupAbsolutePositioning(int index)
    {
        if (contentRectTransforms[index] == null) return;
        
        // Zet anchors voor absolute positioning (ten opzichte van deze button)
        RectTransform buttonRect = GetComponent<RectTransform>();
        
        // Maak het content panel een direct child van de canvas of parent container
        Transform canvas = contentRectTransforms[index].root;
        Canvas canvasComponent = canvas.GetComponent<Canvas>();
        if (canvasComponent != null)
        {
            contentRectTransforms[index].SetParent(canvas, false);
        }
        
        // Stel anchors in voor absolute positioning
        contentRectTransforms[index].anchorMin = new Vector2(0, 1);
        contentRectTransforms[index].anchorMax = new Vector2(0, 1);
        contentRectTransforms[index].pivot = new Vector2(0, 1);
        
        Debug.Log($"[ButtonToggle] Absolute positioning setup for {menusToToggle[index].name}");
    }

    public void ToggleButton()
    {
        SetToggleState(!isSelected, true); // true = speel geluid af
    }
    
    /// <summary>
    /// Handmatig de toggle staat instellen
    /// </summary>
    /// <param name="selected">Nieuwe staat</param>
    /// <param name="playSound">Of er geluid moet worden afgespeeld</param>
    public void SetToggleState(bool selected, bool playSound = false)
    {
        isSelected = selected;
        
        // Update de kleur
        SetButtonColor(isSelected ? selectedColor : normalColor);
        
        // Toggle alle menu's
        if (menusToToggle != null && menusToToggle.Length > 0)
        {
            for (int i = 0; i < menusToToggle.Length; i++)
            {
                if (menusToToggle[i] != null)
                {
                    menusToToggle[i].SetActive(isSelected);
                    
                    // Update positie als we absolute positioning gebruiken
                    if (useAbsolutePositioning && isSelected)
                    {
                        UpdateContentPosition(i);
                    }
                }
            }
        }

        // Toggle de outline (als we het gebruiken)
        if (outline != null && useOutline)
        {
            outline.enabled = isSelected;
        }
        
        // Speel geluid af (als beschikbaar en gewenst)
        if (playSound && audioSource != null)
        {
            AudioClip clipToPlay = isSelected ? toggleOnSound : toggleOffSound;
            if (clipToPlay != null)
            {
                audioSource.clip = clipToPlay;
                audioSource.Play();
            }
        }
        
        // Force layout refresh als nodig
        if (preventLayoutInterference)
        {
            ForceLayoutRefresh();
        }
        
        string menuNames = "";
        if (menusToToggle != null && menusToToggle.Length > 0)
        {
            for (int i = 0; i < menusToToggle.Length; i++)
            {
                if (menusToToggle[i] != null)
                {
                    menuNames += menusToToggle[i].name;
                    if (i < menusToToggle.Length - 1) menuNames += ", ";
                }
            }
        }
        
        Debug.Log($"[ButtonToggle] {gameObject.name} toggle state: {(isSelected ? "ON" : "OFF")} - Objects: {menuNames}");
    }
    
    private void UpdateContentPosition(int index)
    {
        if (contentRectTransforms[index] == null) return;
        
        // Bereken positie ten opzichte van deze button
        RectTransform buttonRect = GetComponent<RectTransform>();
        Vector3 buttonWorldPos = buttonRect.TransformPoint(Vector3.zero);
        
        // Converteer naar local space van de canvas
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            contentRectTransforms[index].parent as RectTransform, 
            RectTransformUtility.WorldToScreenPoint(null, buttonWorldPos),
            null, 
            out localPos
        );
        
        // Pas offset toe
        contentRectTransforms[index].anchoredPosition = new Vector2(localPos.x + contentOffset.x, localPos.y + contentOffset.y);
    }
    
    private void ForceLayoutRefresh()
    {
        // Force layout update voor parent containers
        Transform current = transform.parent;
        while (current != null)
        {
            LayoutGroup layoutGroup = current.GetComponent<LayoutGroup>();
            ContentSizeFitter sizeFitter = current.GetComponent<ContentSizeFitter>();
            
            if (layoutGroup != null || sizeFitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(current as RectTransform);
            }
            
            current = current.parent;
        }
    }

    private void SetButtonColor(Color color)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        button.colors = colors;
    }
    
    /// <summary>
    /// Publieke methode om de toggle uit te voeren (voor gebruik in Inspector onClick events)
    /// </summary>
    public void Toggle()
    {
        ToggleButton();
    }
    
    /// <summary>
    /// Schakel naar ON staat
    /// </summary>
    public void TurnOn()
    {
        if (!isSelected)
            SetToggleState(true, true);
    }
    
    /// <summary>
    /// Schakel naar OFF staat
    /// </summary>
    public void TurnOff()
    {
        if (isSelected)
            SetToggleState(false, true);
    }
} 