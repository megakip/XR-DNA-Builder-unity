using UnityEngine;
using UnityEngine.UI;

public class CategoryNavigationButton : MonoBehaviour
{
    public enum NavigationType
    {
        Next,
        Previous,
        Specific
    }
    
    [Header("Navigation Settings")]
    [SerializeField] private NavigationType navigationType = NavigationType.Next;
    [SerializeField] private int specificCategoryIndex = 0;
    [SerializeField] private string specificCategoryName = "";
    
    private Button button;
    private CategoryMenuManager categoryManager;
    
    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnNavigationButtonClick);
        }
        
        // Vind de CategoryMenuManager
        categoryManager = FindObjectOfType<CategoryMenuManager>();
        
        if (categoryManager == null)
        {
            Debug.LogWarning("CategoryMenuManager not found for navigation button!");
        }
    }
    
    void OnNavigationButtonClick()
    {
        if (categoryManager == null)
        {
            Debug.LogError("CategoryMenuManager not found!");
            return;
        }
        
        switch (navigationType)
        {
            case NavigationType.Next:
                categoryManager.NextCategory();
                break;
                
            case NavigationType.Previous:
                categoryManager.PreviousCategory();
                break;
                
            case NavigationType.Specific:
                if (!string.IsNullOrEmpty(specificCategoryName))
                {
                    categoryManager.ShowCategoryByName(specificCategoryName);
                }
                else
                {
                    categoryManager.ShowCategory(specificCategoryIndex);
                }
                break;
        }
        
        Debug.Log($"Navigation button clicked: {navigationType}, now showing: {categoryManager.CurrentCategoryName}");
    }
    
    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnNavigationButtonClick);
        }
    }
}
