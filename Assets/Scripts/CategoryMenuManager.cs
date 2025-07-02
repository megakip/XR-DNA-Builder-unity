using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CategoryMenuManager : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private GameObject testMenu;
    [SerializeField] private Transform contentParent;
    
    [Header("Category Control")]
    [SerializeField] private List<GameObject> categories = new List<GameObject>();
    [SerializeField] private int currentCategoryIndex = 0;
    
    void Start()
    {
        InitializeManager();
    }
    
    void InitializeManager()
    {
        // Vind automatisch de referenties als ze niet zijn ingesteld
        if (testMenu == null)
        {
            testMenu = GameObject.Find("Test");
        }
        
        // OPGELOST: Deactiveer het Test menu direct bij initialisatie
        // De gebruiker wil niet dat dit menu automatisch verschijnt
        if (testMenu != null)
        {
            testMenu.SetActive(false);
            Debug.Log("Test menu gevonden en gedeactiveerd zoals gewenst");
        }
        
        if (contentParent == null && testMenu != null)
        {
            contentParent = testMenu.transform;
        }
        
        // Verzamel alle categorieën
        if (contentParent != null)
        {
            categories.Clear();
            for (int i = 0; i < contentParent.childCount; i++)
            {
                Transform child = contentParent.GetChild(i);
                if (child.name != "Buildables Scroll View" && child.name != "Content")
                {
                    categories.Add(child.gameObject);
                }
            }
        }
        
        Debug.Log($"CategoryMenuManager initialized with {categories.Count} categories");
    }
    
    public void ShowFirstCategory()
    {
        currentCategoryIndex = 0;
        ShowCategory(currentCategoryIndex);
    }
    
    public void ShowCategory(int index)
    {
        if (categories.Count == 0)
        {
            Debug.LogWarning("No categories found!");
            return;
        }
        
        if (index < 0 || index >= categories.Count)
        {
            Debug.LogWarning($"Category index {index} out of range (0-{categories.Count - 1})");
            return;
        }
        
        // Deactiveer alle categorieën
        foreach (GameObject category in categories)
        {
            category.SetActive(false);
        }
        
        // Activeer de gewenste categorie
        categories[index].SetActive(true);
        currentCategoryIndex = index;
        
        Debug.Log($"Showing category: {categories[index].name} (index {index})");
        
        // OPGELOST: Activeer het Test menu NIET automatisch
        // De gebruiker wil dat dit menu gedeactiveerd blijft
        // Het Test menu wordt nu alleen geactiveerd als de gebruiker dit expliciet doet
    }
    
    public void NextCategory()
    {
        int nextIndex = (currentCategoryIndex + 1) % categories.Count;
        ShowCategory(nextIndex);
    }
    
    public void PreviousCategory()
    {
        int prevIndex = (currentCategoryIndex - 1 + categories.Count) % categories.Count;
        ShowCategory(prevIndex);
    }
    
    public void ShowCategoryByName(string categoryName)
    {
        for (int i = 0; i < categories.Count; i++)
        {
            if (categories[i].name.Contains(categoryName))
            {
                ShowCategory(i);
                return;
            }
        }
        Debug.LogWarning($"Category with name containing '{categoryName}' not found!");
    }
    
    public void HideAllCategories()
    {
        foreach (GameObject category in categories)
        {
            category.SetActive(false);
        }
        
        if (testMenu != null)
        {
            testMenu.SetActive(false);
        }
    }
    
    // Public getters voor debugging
    public int CategoryCount => categories.Count;
    public int CurrentCategoryIndex => currentCategoryIndex;
    public string CurrentCategoryName => 
        categories.Count > 0 && currentCategoryIndex < categories.Count ? 
        categories[currentCategoryIndex].name : "None";
}
