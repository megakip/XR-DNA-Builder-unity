using UnityEngine;
using UnityEngine.UI;

public class ScrollViewFixer : MonoBehaviour
{
    [Header("Scroll View Setup")]
    [Tooltip("De hoofdscroll view die scroll functionaliteit moet hebben")]
    public GameObject scrollViewObject;
    
    [Tooltip("Automatisch zoeken naar de juiste objecten gebaseerd op de hierarchy")]
    public bool autoDetect = true;
    
    [Header("Manual References (Optional)")]
    public GameObject viewportObject;
    public GameObject contentObject;
    
    [Header("Settings")]
    public bool verticalScroll = true;
    public bool horizontalScroll = false;
    
    [ContextMenu("Fix Scroll View")]
    public void FixScrollView()
    {
        if (autoDetect)
        {
            AutoDetectObjects();
        }
        
        SetupScrollView();
    }
    
    void AutoDetectObjects()
    {
        // Zoek automatisch de scroll view in de hierarchy
        if (scrollViewObject == null)
        {
            // Zoek naar een object met "Scroll View" in de naam
            ScrollRect[] scrollRects = FindObjectsOfType<ScrollRect>();
            if (scrollRects.Length > 0)
            {
                scrollViewObject = scrollRects[0].gameObject;
            }
            else
            {
                // Zoek naar een object genoemd "Scroll View"
                GameObject found = GameObject.Find("Scroll View");
                if (found != null)
                {
                    scrollViewObject = found;
                }
            }
        }
        
        if (scrollViewObject != null)
        {
            // Zoek Viewport child
            Transform viewport = scrollViewObject.transform.Find("Viewport");
            if (viewport != null)
            {
                viewportObject = viewport.gameObject;
                
                // Zoek Content grandchild
                Transform content = viewport.Find("Content");
                if (content != null)
                {
                    contentObject = content.gameObject;
                }
            }
        }
    }
    
    void SetupScrollView()
    {
        if (scrollViewObject == null)
        {
            Debug.LogError("Scroll View object not found!");
            return;
        }
        
        Debug.Log("Setting up scroll view for: " + scrollViewObject.name);
        
        // 1. Voeg ScrollRect component toe (als die er niet is)
        ScrollRect scrollRect = scrollViewObject.GetComponent<ScrollRect>();
        if (scrollRect == null)
        {
            scrollRect = scrollViewObject.AddComponent<ScrollRect>();
            Debug.Log("Added ScrollRect component");
        }
        
        // 2. Setup Viewport
        if (viewportObject != null)
        {
            // Voeg RectMask2D toe aan viewport (als die er niet is)
            RectMask2D mask = viewportObject.GetComponent<RectMask2D>();
            if (mask == null)
            {
                mask = viewportObject.AddComponent<RectMask2D>();
                Debug.Log("Added RectMask2D to Viewport");
            }
            
            // Stel ScrollRect viewport in
            scrollRect.viewport = viewportObject.GetComponent<RectTransform>();
        }
        
        // 3. Setup Content
        if (contentObject != null)
        {
            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            scrollRect.content = contentRect;
            
            // Voeg Content Size Fitter toe
            ContentSizeFitter sizeFitter = contentObject.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = contentObject.AddComponent<ContentSizeFitter>();
            }
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            
            // Voeg Vertical Layout Group toe (als die er niet is)
            VerticalLayoutGroup layoutGroup = contentObject.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = contentObject.AddComponent<VerticalLayoutGroup>();
            }
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            
            // Set pivot to top (0, 1) voor correcte scroll behavior
            contentRect.pivot = new Vector2(0.5f, 1f);
            
            Debug.Log("Setup Content with ContentSizeFitter and VerticalLayoutGroup");
        }
        
        // 4. Configure ScrollRect settings
        scrollRect.horizontal = horizontalScroll;
        scrollRect.vertical = verticalScroll;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 10f;
        
        Debug.Log("ScrollView setup complete!");
    }
    
    // Called from inspector button
    void Start()
    {
        if (autoDetect)
        {
            FixScrollView();
        }
    }
}
