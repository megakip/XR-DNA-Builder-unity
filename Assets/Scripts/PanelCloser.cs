using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script voor het sluiten van een panel/canvas.
/// Dit script wordt automatisch toegevoegd aan pin buttons in gedupliceerde panels 
/// om ze om te zetten naar close buttons.
/// 
/// Gebruik:
/// - Dit script wordt automatisch door CanvasDuplicator toegevoegd
/// - Geen handmatige setup nodig
/// </summary>
public class PanelCloser : MonoBehaviour
{
    [Header("Panel Settings")]
    [Tooltip("Het panel/canvas dat gesloten moet worden")]
    [SerializeField] private GameObject panelToClose;
    
    [Header("Close Animation Settings")]
    [Tooltip("Gebruik fade out animatie bij het sluiten")]
    [SerializeField] private bool useFadeAnimation = false;
    
    [Tooltip("Duur van de fade animatie in seconden")]
    [SerializeField] private float fadeAnimationDuration = 0.3f;
    
    private Button closeButton;
    private CanvasGroup panelCanvasGroup;
    
    private void Awake()
    {
        // Zoek de Button component op dit GameObject
        closeButton = GetComponent<Button>();
        if (closeButton == null)
        {
            Debug.LogError("[PanelCloser] Geen Button component gevonden! Dit script moet op een GameObject met een Button component worden geplaatst.");
            return;
        }
    }
    
    private void Start()
    {
        // Koppel de close functie aan de button click
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
        
        // Setup animatie componenten als gewenst
        if (useFadeAnimation && panelToClose != null)
        {
            SetupFadeAnimation();
        }
    }
    
    /// <summary>
    /// Stelt het panel in dat gesloten moet worden
    /// </summary>
    public void SetPanelToClose(GameObject panel)
    {
        panelToClose = panel;
        Debug.Log($"[PanelCloser] Panel ingesteld voor sluiting: {panel.name}");
        
        // Setup animatie als gewenst
        if (useFadeAnimation)
        {
            SetupFadeAnimation();
        }
    }
    
    /// <summary>
    /// Setup voor fade animatie
    /// </summary>
    private void SetupFadeAnimation()
    {
        if (panelToClose == null) return;
        
        panelCanvasGroup = panelToClose.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
        {
            panelCanvasGroup = panelToClose.AddComponent<CanvasGroup>();
        }
        
        // Zorg ervoor dat de alpha op 1 staat
        panelCanvasGroup.alpha = 1f;
    }
    
    /// <summary>
    /// Sluit het panel
    /// </summary>
    public void ClosePanel()
    {
        if (panelToClose == null)
        {
            Debug.LogError("[PanelCloser] Geen panel ingesteld om te sluiten!");
            return;
        }
        
        Debug.Log($"[PanelCloser] Panel sluiten: {panelToClose.name}");
        
        if (useFadeAnimation && panelCanvasGroup != null)
        {
            // Start fade out animatie
            StartCoroutine(FadeOutAndDestroy());
        }
        else
        {
            // Direct sluiten
            DestroyPanel();
        }
    }
    
    /// <summary>
    /// Fade out animatie en vernietig het panel
    /// </summary>
    private System.Collections.IEnumerator FadeOutAndDestroy()
    {
        float startAlpha = panelCanvasGroup.alpha;
        float elapsedTime = 0f;
        
        // Schakel interactie uit tijdens animatie
        panelCanvasGroup.interactable = false;
        
        while (elapsedTime < fadeAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeAnimationDuration;
            
            // Lerp de alpha van start naar 0
            panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            
            yield return null;
        }
        
        // Zorg ervoor dat alpha helemaal 0 is
        panelCanvasGroup.alpha = 0f;
        
        // Vernietig het panel
        DestroyPanel();
    }
    
    /// <summary>
    /// Vernietig het panel definitief
    /// </summary>
    private void DestroyPanel()
    {
        if (panelToClose != null)
        {
            Debug.Log($"[PanelCloser] Panel vernietigd: {panelToClose.name}");
            Destroy(panelToClose);
            panelToClose = null;
        }
    }
    
    /// <summary>
    /// Alternative methode om het panel te verbergen in plaats van vernietigen
    /// </summary>
    public void HidePanel()
    {
        if (panelToClose == null)
        {
            Debug.LogError("[PanelCloser] Geen panel ingesteld om te verbergen!");
            return;
        }
        
        Debug.Log($"[PanelCloser] Panel verbergen: {panelToClose.name}");
        panelToClose.SetActive(false);
    }
    
    /// <summary>
    /// Schakel fade animatie in of uit
    /// </summary>
    public void SetFadeAnimation(bool enabled, float duration = 0.3f)
    {
        useFadeAnimation = enabled;
        fadeAnimationDuration = duration;
        
        if (enabled && panelToClose != null)
        {
            SetupFadeAnimation();
        }
    }
    
    private void OnDestroy()
    {
        // Ontkoppel de button listener wanneer het object wordt vernietigd
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(ClosePanel);
        }
    }
} 