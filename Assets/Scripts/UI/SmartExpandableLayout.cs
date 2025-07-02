using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Script voor expandable content panels die netjes andere elementen naar beneden duwen
/// In plaats van ze te overlappen
/// </summary>
[RequireComponent(typeof(Button))]
public class SmartExpandableLayout : MonoBehaviour
{
    [Header("Toggle Settings")]
    [SerializeField] private GameObject contentPanel;
    [SerializeField] private bool startExpanded = false;
    
    [Header("Animation Settings")]
    [SerializeField] private bool useAnimation = true;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color expandedColor = new Color(1f, 0.973f, 0.941f);
    
    private Button button;
    private bool isExpanded = false;
    private LayoutElement contentLayoutElement;
    private ContentSizeFitter contentSizeFitter;
    private float originalHeight = 0f;
    private Coroutine animationCoroutine;

    public bool IsExpanded => isExpanded;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ToggleExpansion);
        
        SetupContentPanel();
        SetExpandedState(startExpanded, false);
    }

    private void SetupContentPanel()
    {
        if (contentPanel == null) return;

        // Zorg ervoor dat het content panel de juiste layout componenten heeft
        contentLayoutElement = contentPanel.GetComponent<LayoutElement>();
        if (contentLayoutElement == null)
        {
            contentLayoutElement = contentPanel.AddComponent<LayoutElement>();
        }

        // ContentSizeFitter voor automatische height
        contentSizeFitter = contentPanel.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter == null)
        {
            contentSizeFitter = contentPanel.AddComponent<ContentSizeFitter>();
        }
        
        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Bewaar de originele hoogte
        RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            // Force layout calculation om de juiste preferred height te krijgen
            Canvas.ForceUpdateCanvases();
            originalHeight = LayoutUtility.GetPreferredHeight(contentRect);
        }

        Debug.Log($"[SmartExpandableLayout] Setup complete. Original height: {originalHeight}");
    }

    public void ToggleExpansion()
    {
        SetExpandedState(!isExpanded, true);
    }

    public void SetExpandedState(bool expanded, bool animate = true)
    {
        isExpanded = expanded;

        // Update button color
        SetButtonColor(isExpanded ? expandedColor : normalColor);

        if (contentPanel == null) return;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        if (animate && useAnimation && Application.isPlaying)
        {
            animationCoroutine = StartCoroutine(AnimateExpansion(expanded));
        }
        else
        {
            // Direct zonder animatie
            if (expanded)
            {
                contentPanel.SetActive(true);
                contentLayoutElement.preferredHeight = -1; // Auto size
                contentLayoutElement.minHeight = -1;
            }
            else
            {
                contentLayoutElement.preferredHeight = 0;
                contentLayoutElement.minHeight = 0;
                contentPanel.SetActive(false);
            }
            
            ForceLayoutRefresh();
        }

        Debug.Log($"[SmartExpandableLayout] {gameObject.name} expanded: {expanded}");
    }

    private IEnumerator AnimateExpansion(bool expanding)
    {
        if (expanding)
        {
            // Eerst activeren en layout berekenen
            contentPanel.SetActive(true);
            Canvas.ForceUpdateCanvases();
            
            RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
            float targetHeight = LayoutUtility.GetPreferredHeight(contentRect);
            
            // Animeer van 0 naar target height
            float startHeight = 0f;
            float elapsedTime = 0f;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / animationDuration;
                float curveValue = animationCurve.Evaluate(t);
                
                float currentHeight = Mathf.Lerp(startHeight, targetHeight, curveValue);
                contentLayoutElement.preferredHeight = currentHeight;
                contentLayoutElement.minHeight = currentHeight;
                
                ForceLayoutRefresh();
                yield return null;
            }
            
            // Zet naar auto size aan het einde
            contentLayoutElement.preferredHeight = -1;
            contentLayoutElement.minHeight = -1;
        }
        else
        {
            // Krijg huidige hoogte
            Canvas.ForceUpdateCanvases();
            RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
            float startHeight = contentRect.rect.height;
            float targetHeight = 0f;
            float elapsedTime = 0f;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / animationDuration;
                float curveValue = animationCurve.Evaluate(t);
                
                float currentHeight = Mathf.Lerp(startHeight, targetHeight, curveValue);
                contentLayoutElement.preferredHeight = currentHeight;
                contentLayoutElement.minHeight = currentHeight;
                
                ForceLayoutRefresh();
                yield return null;
            }
            
            // Deactiveer aan het einde
            contentLayoutElement.preferredHeight = 0;
            contentLayoutElement.minHeight = 0;
            contentPanel.SetActive(false);
        }
        
        ForceLayoutRefresh();
        animationCoroutine = null;
    }

    private void ForceLayoutRefresh()
    {
        // Force layout update voor parent containers
        Transform current = transform.parent;
        while (current != null)
        {
            if (current.GetComponent<LayoutGroup>() != null || current.GetComponent<ContentSizeFitter>() != null)
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

    // Public methods voor externe controle
    public void Expand() => SetExpandedState(true, true);
    public void Collapse() => SetExpandedState(false, true);
    
    // For debugging
    [ContextMenu("Toggle (Test)")]
    private void TestToggle()
    {
        ToggleExpansion();
    }
} 