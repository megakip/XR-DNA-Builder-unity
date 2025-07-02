using UnityEngine;
using UnityEngine.UI;

public class QuickLayoutFix : MonoBehaviour
{
    [Header("Quick Fix Settings")]
    [SerializeField] private bool fixOnStart = true;
    [SerializeField] private bool fixEveryFrame = false;
    
    private RectTransform rectTransform;
    private VerticalLayoutGroup verticalLayout;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        verticalLayout = GetComponent<VerticalLayoutGroup>();
        
        if (fixOnStart)
        {
            FixLayoutNow();
        }
    }
    
    void Update()
    {
        if (fixEveryFrame)
        {
            FixLayoutNow();
        }
    }
    
    [ContextMenu("Fix Layout Now")]
    public void FixLayoutNow()
    {
        if (rectTransform == null) return;
        
        // Reset position to zero
        rectTransform.anchoredPosition = Vector2.zero;
        
        // Fix alle child buttons
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            RectTransform childRect = child.GetComponent<RectTransform>();
            
            if (childRect != null && child.gameObject.activeInHierarchy)
            {
                // Reset child position
                childRect.anchoredPosition = new Vector2(0, 0);
                
                // Zorg voor goede anchoring
                childRect.anchorMin = new Vector2(0, 1);
                childRect.anchorMax = new Vector2(1, 1);
                childRect.pivot = new Vector2(0.5f, 1f);
                
                // Add Layout Element als deze er niet is
                LayoutElement layoutElement = child.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = child.gameObject.AddComponent<LayoutElement>();
                }
                layoutElement.preferredHeight = 60f;
            }
        }
        
        // Force layout rebuild
        if (verticalLayout != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
        
        Debug.Log($"Layout fixed for {gameObject.name}. Position: {rectTransform.anchoredPosition}");
    }
}
