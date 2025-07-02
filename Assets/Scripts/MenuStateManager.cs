using UnityEngine;

public class MenuStateManager : MonoBehaviour
{
    [Header("Menu States")]
    [SerializeField] private GameObject mainMenuCollapsed;
    [SerializeField] private GameObject mainMenuExpanded;
    [SerializeField] private GameObject categoriesMenu;
    [SerializeField] private GameObject buildableObjectsPanel;
    
    public enum MenuState
    {
        Collapsed,
        MainMenu,
        Categories,
        BuildableObjects
    }
    
    private MenuState currentState = MenuState.Collapsed;
    
    void Start()
    {
        // Automatisch referenties vinden
        if (mainMenuCollapsed == null)
            mainMenuCollapsed = GameObject.Find("Hand menu collapsed");
        
        if (mainMenuExpanded == null)
            mainMenuExpanded = GameObject.Find("MenuMain");
            
        if (categoriesMenu == null)
            categoriesMenu = GameObject.Find("Test");
            
        if (buildableObjectsPanel == null)
            buildableObjectsPanel = GameObject.Find("Buildable Objects UI Panel");
        
        // Start in collapsed state
        SetMenuState(MenuState.Collapsed);
    }
    
    public void ShowMainMenu()
    {
        SetMenuState(MenuState.MainMenu);
    }
    
    public void ShowCategoriesMenu()
    {
        SetMenuState(MenuState.Categories);
    }
    
    public void ShowBuildableObjectsPanel()
    {
        SetMenuState(MenuState.BuildableObjects);
    }
    
    public void CollapseMenu()
    {
        SetMenuState(MenuState.Collapsed);
    }
    
    private void SetMenuState(MenuState newState)
    {
        // Deactiveer alle menu's eerst
        if (mainMenuCollapsed != null) mainMenuCollapsed.SetActive(false);
        if (mainMenuExpanded != null) mainMenuExpanded.SetActive(false);
        if (categoriesMenu != null) categoriesMenu.SetActive(false);
        if (buildableObjectsPanel != null) buildableObjectsPanel.SetActive(false);
        
        // Activeer het juiste menu
        switch (newState)
        {
            case MenuState.Collapsed:
                if (mainMenuCollapsed != null) mainMenuCollapsed.SetActive(true);
                break;
                
            case MenuState.MainMenu:
                if (mainMenuExpanded != null) mainMenuExpanded.SetActive(true);
                break;
                
            case MenuState.Categories:
                if (mainMenuExpanded != null) mainMenuExpanded.SetActive(true);
                if (categoriesMenu != null) categoriesMenu.SetActive(true);
                break;
                
            case MenuState.BuildableObjects:
                if (buildableObjectsPanel != null) buildableObjectsPanel.SetActive(true);
                break;
        }
        
        currentState = newState;
        Debug.Log($"Menu state changed to: {newState}");
    }
    
    public MenuState CurrentState => currentState;
}
