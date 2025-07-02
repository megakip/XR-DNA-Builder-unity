using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script om automatisch layout interferentie te voorkomen voor content panels
/// Voeg dit toe aan alle GameObjects die niet de layout mogen verstoren
/// </summary>
public class LayoutIgnoreHelper : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool ignoreLayout = true;
    [SerializeField] private bool applyOnStart = true;
    
    private LayoutElement layoutElement;
    
    private void Start()
    {
        if (applyOnStart)
        {
            ApplyLayoutIgnore();
        }
    }
    
    /// <summary>
    /// Pas layout ignore instellingen toe
    /// </summary>
    public void ApplyLayoutIgnore()
    {
        layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = gameObject.AddComponent<LayoutElement>();
        }
        
        layoutElement.ignoreLayout = ignoreLayout;
        
        Debug.Log($"[LayoutIgnoreHelper] Layout ignore set to {ignoreLayout} for {gameObject.name}");
    }
    
    /// <summary>
    /// Toggle layout ignore aan/uit
    /// </summary>
    public void ToggleLayoutIgnore()
    {
        if (layoutElement != null)
        {
            layoutElement.ignoreLayout = !layoutElement.ignoreLayout;
            Debug.Log($"[LayoutIgnoreHelper] Layout ignore toggled to {layoutElement.ignoreLayout} for {gameObject.name}");
        }
        else
        {
            ApplyLayoutIgnore();
        }
    }
    
    /// <summary>
    /// Stel layout ignore handmatig in
    /// </summary>
    /// <param name="ignore">Of layout genegeerd moet worden</param>
    public void SetLayoutIgnore(bool ignore)
    {
        ignoreLayout = ignore;
        ApplyLayoutIgnore();
    }
} 