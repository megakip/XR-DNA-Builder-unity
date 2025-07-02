using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Beheert de navigatiegeschiedenis tussen UI-schermen en maakt het mogelijk om terug te navigeren.
/// Dit script moet worden geplaatst op een permanent GameObject in je scene.
/// </summary>
public class NavigationManager : MonoBehaviour
{
    // Singleton-patroon voor gemakkelijke toegang
    public static NavigationManager Instance { get; private set; }
    
    // De stapel van bezochte schermen (het laatste item is het huidige scherm)
    private Stack<GameObject> screenHistory = new Stack<GameObject>();
    
    // Optioneel: referentie naar de ButtonSelectionManager
    [SerializeField] private ButtonSelectionManager buttonManager;
    
    // Koppelt schermen aan knoppen in de ButtonSelectionManager
    [SerializeField] private List<GameObject> screens = new List<GameObject>();
    [SerializeField] private List<int> screenButtonIndices = new List<int>();
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Zorg ervoor dat er slechts één scherm actief is bij het opstarten
        EnsureSingleActiveScreen();
        
        // Voeg het actieve startscherm toe aan de geschiedenis
        GameObject activeScreen = FindActiveScreen();
        if (activeScreen != null)
        {
            screenHistory.Push(activeScreen);
        }
    }
    
    /// <summary>
    /// Navigeert naar een nieuw scherm en houdt het huidige bij in de geschiedenis
    /// </summary>
    public void NavigateToScreen(GameObject targetScreen, GameObject currentScreen)
    {
        // Voeg het huidige scherm toe aan de geschiedenis als het er nog niet in zit
        if (currentScreen != null && (screenHistory.Count == 0 || screenHistory.Peek() != currentScreen))
        {
            screenHistory.Push(currentScreen);
        }
        
        // Schakel het huidige scherm uit en het doelscherm in
        if (currentScreen != null)
            currentScreen.SetActive(false);
        
        targetScreen.SetActive(true);
        
        // Update de ButtonSelectionManager indien nodig
        UpdateButtonSelection(targetScreen);
    }
    
    /// <summary>
    /// Gaat terug naar het vorige scherm in de geschiedenis
    /// </summary>
    public void NavigateBack()
    {
        // Controleer of er een scherm is om naar terug te gaan
        if (screenHistory.Count == 0)
        {
            Debug.LogWarning("Geen vorig scherm om naar terug te gaan.");
            return;
        }
        
        // Haal het huidige actieve scherm op
        GameObject currentScreen = FindActiveScreen();
        
        // Haal het vorige scherm uit de geschiedenis
        GameObject previousScreen = screenHistory.Pop();
        
        // BELANGRIJK: Schakel EERST het huidige scherm uit
        if (currentScreen != null)
        {
            currentScreen.SetActive(false);
        }
        else
        {
            // Als we geen actief scherm kunnen vinden, schakel dan alle schermen uit voor de zekerheid
            foreach (GameObject screen in screens)
            {
                if (screen != null && screen != previousScreen && screen.activeSelf)
                {
                    screen.SetActive(false);
                }
            }
        }
        
        // Activeer dan pas het vorige scherm
        previousScreen.SetActive(true);
        
        // Update de ButtonSelectionManager indien nodig
        UpdateButtonSelection(previousScreen);
        
        // In plaats van handmatig schermen activeren/deactiveren
        EnsureSingleActiveScreen(previousScreen);
    }
    
    /// <summary>
    /// Vindt het actieve scherm uit de lijst van beheerde schermen
    /// </summary>
    private GameObject FindActiveScreen()
    {
        foreach (GameObject screen in screens)
        {
            if (screen != null && screen.activeSelf)
            {
                return screen;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Vind alle actieve schermen in de lijst (voor debug doeleinden)
    /// </summary>
    private List<GameObject> FindAllActiveScreens()
    {
        List<GameObject> activeScreens = new List<GameObject>();
        foreach (GameObject screen in screens)
        {
            if (screen != null && screen.activeSelf)
            {
                activeScreens.Add(screen);
            }
        }
        return activeScreens;
    }
    
    /// <summary>
    /// Update de knopselectie in de ButtonSelectionManager
    /// </summary>
    private void UpdateButtonSelection(GameObject activeScreen)
    {
        if (buttonManager == null) return;
        
        int screenIndex = screens.IndexOf(activeScreen);
        if (screenIndex >= 0 && screenIndex < screenButtonIndices.Count)
        {
            buttonManager.SelectButtonByIndex(screenButtonIndices[screenIndex]);
        }
    }
    
    /// <summary>
    /// Zorgt ervoor dat er slechts één scherm actief is
    /// </summary>
    private void EnsureSingleActiveScreen()
    {
        GameObject activeScreen = null;
        
        // Vind het eerste actieve scherm
        foreach (GameObject screen in screens)
        {
            if (screen != null && screen.activeSelf)
            {
                activeScreen = screen;
                break;
            }
        }
        
        // Als er een actief scherm is, zorg dat alle andere schermen uit staan
        if (activeScreen != null)
        {
            foreach (GameObject screen in screens)
            {
                if (screen != null && screen != activeScreen)
                {
                    screen.SetActive(false);
                }
            }
        }
        // Als er geen actief scherm is, activeer dan het eerste scherm
        else if (screens.Count > 0 && screens[0] != null)
        {
            screens[0].SetActive(true);
        }
    }
    
    private void EnsureSingleActiveScreen(GameObject screenToKeepActive)
    {
        foreach (GameObject screen in screens)
        {
            if (screen != null && screen != screenToKeepActive)
            {
                screen.SetActive(false);
            }
        }
        
        if (screenToKeepActive != null)
        {
            screenToKeepActive.SetActive(true);
        }
    }
} 