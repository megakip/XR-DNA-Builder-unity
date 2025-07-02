using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script voor een "Terug" knop die terugnavigatie naar het vorige scherm afhandelt.
/// Plaats dit op je Btn_Terug GameObject.
/// </summary>
public class BackButton : MonoBehaviour
{
    private Button button;
    
    private void Awake()
    {
        // Verkrijg de Button component
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("BackButton vereist een Button component op hetzelfde GameObject!");
            return;
        }
        
        // Voeg de onClick actie toe
        button.onClick.AddListener(GoBack);
    }
    
    /// <summary>
    /// Navigeert terug naar het vorige scherm
    /// </summary>
    public void GoBack()
    {
        if (NavigationManager.Instance != null)
        {
            NavigationManager.Instance.NavigateBack();
        }
        else
        {
            Debug.LogError("Geen NavigationManager gevonden in de scene!");
        }
    }
} 