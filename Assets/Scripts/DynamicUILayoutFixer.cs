using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DynamicUILayoutFixer : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private RectTransform contentArea;
    [SerializeField] private VerticalLayoutGroup verticalLayout;
    [SerializeField] private ContentSizeFitter contentSizeFitter;
    [SerializeField] private float buttonHeight = 60f;
    [SerializeField] private float spacing = 5f;
    
    void Start()
    {
        if (contentArea == null)
            contentArea = GetComponent<RectTransform>();
            
        if (verticalLayout == null)
            verticalLayout = GetComponent<VerticalLayoutGroup>();
            
        if (contentSizeFitter == null)
            contentSizeFitter = GetComponent<ContentSizeFitter>();
            
        SetupLayoutCorrectly();
    }
    
    void SetupLayoutCorrectly()
    {
        // Zorg voor juiste VerticalLayoutGroup instellingen
        if (verticalLayout != null)
        {
            verticalLayout.spacing = spacing;
            verticalLayout.childAlignment = TextAnchor.UpperCenter;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = false;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.childScaleWidth = false;
            verticalLayout.childScaleHeight = false;
        }
        
        // Zorg voor juiste ContentSizeFitter instellingen
        if (contentSizeFitter == null)
        {
            contentSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
        }
        
        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Reset de positie
        if (contentArea != null)
        {
            contentArea.anchoredPosition = Vector2.zero;
        }
    }
    
    // Call deze functie wanneer nieuwe knoppen worden toegevoegd
    public void RefreshLayout()
    {
        StartCoroutine(RefreshLayoutCoroutine());
    }
    
    private IEnumerator RefreshLayoutCoroutine()
    {
        // Wacht een frame voor Unity's layout system
        yield return null;
        
        // Force rebuild van layout
        if (verticalLayout != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentArea);
        }
        
        // Reset positie als het nog steeds verkeerd staat
        if (contentArea != null && contentArea.anchoredPosition.y < -100)
        {
            contentArea.anchoredPosition = new Vector2(contentArea.anchoredPosition.x, 0);
        }
        
        // Zorg ervoor dat alle child buttons de juiste grootte hebben
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            RectTransform childRect = child.GetComponent<RectTransform>();
            LayoutElement layoutElement = child.GetComponent<LayoutElement>();
            
            if (childRect != null && child.gameObject.activeInHierarchy)
            {
                if (layoutElement == null)
                {
                    layoutElement = child.gameObject.AddComponent<LayoutElement>();
                }
                
                layoutElement.preferredHeight = buttonHeight;
                layoutElement.minHeight = buttonHeight;
            }
        }
        
        // Force nog een rebuild
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentArea);
    }
    
    // Call deze functie vanuit het script dat de knoppen spawnt
    public void OnButtonSpawned(GameObject newButton)
    {
        // Zorg ervoor dat de nieuwe knop de juiste layout instellingen heeft
        LayoutElement layoutElement = newButton.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = newButton.AddComponent<LayoutElement>();
        }
        
        layoutElement.preferredHeight = buttonHeight;
        layoutElement.minHeight = buttonHeight;
        
        RefreshLayout();
    }
    
    // Call deze om alle knoppen te resetten
    public void ResetAllButtonPositions()
    {
        contentArea.anchoredPosition = Vector2.zero;
        
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            RectTransform childRect = child.GetComponent<RectTransform>();
            
            if (childRect != null && child.gameObject.activeInHierarchy)
            {
                childRect.anchoredPosition = Vector2.zero;
                childRect.anchorMin = new Vector2(0, 1);
                childRect.anchorMax = new Vector2(1, 1);
                childRect.pivot = new Vector2(0.5f, 1f);
            }
        }
        
        RefreshLayout();
    }
}
