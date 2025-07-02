using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Beheert navigatie tussen pagina's vanuit een menu.
/// Plaats dit script op een knop om bij het klikken naar een andere pagina te navigeren.
/// </summary>
public class PageNavigator : MonoBehaviour
{
    [Tooltip("Het GameObject van de pagina om naar toe te navigeren")]
    [SerializeField] private GameObject targetPage;

    [Tooltip("Het GameObject van de huidige pagina die moet worden uitgeschakeld")]
    [SerializeField] private GameObject currentPage;

    [Tooltip("Optioneel: De ButtonSelectionManager die moet worden bijgewerkt")]
    [SerializeField] private ButtonSelectionManager buttonManager;

    [Tooltip("De index van de knop in de ButtonSelectionManager die moet worden geselecteerd")]
    [SerializeField] private int buttonIndex = -1;

    private Button button;

    private void Awake()
    {
        // Haal de Button component op
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("PageNavigator vereist een Button component op hetzelfde GameObject!");
            return;
        }

        // Voeg onClick listener toe
        button.onClick.AddListener(NavigateToPage);
    }

    /// <summary>
    /// Navigeert naar de doelpagina en deactiveert de huidige pagina
    /// </summary>
    public void NavigateToPage()
    {
        if (targetPage == null)
        {
            Debug.LogWarning("Doelpagina is niet ingesteld voor PageNavigator op " + gameObject.name);
            return;
        }

        // Als de huidige pagina niet is ingesteld, probeer deze te vinden
        GameObject currentActiveScreen = currentPage;
        if (currentActiveScreen == null)
        {
            // Probeer de parent van de huidige pagina te vinden (meestal de Content)
            Transform parent = transform.parent;
            while (parent != null)
            {
                // Als we een scherm vinden dat actief is en niet het doel is, gebruik dat
                if (parent.gameObject.activeSelf && parent.gameObject != targetPage)
                {
                    foreach (Transform child in parent)
                    {
                        if (child.gameObject.activeSelf && child.gameObject != targetPage)
                        {
                            currentActiveScreen = child.gameObject;
                            break;
                        }
                    }
                }
                
                // Ga naar de volgende parent
                parent = parent.parent;
            }
        }

        // Registreer de schermverandering voor de terugknop
        SimpleBackButton.RegisterScreenChange(targetPage, currentActiveScreen);

        // Activeer de doelpagina
        targetPage.SetActive(true);

        // Deactiveer de huidige pagina als deze is ingesteld
        if (currentActiveScreen != null)
        {
            currentActiveScreen.SetActive(false);
        }

        // Update de ButtonSelectionManager indien nodig
        if (buttonManager != null && buttonIndex >= 0)
        {
            buttonManager.SelectButtonByIndex(buttonIndex);
        }
    }

    /// <summary>
    /// Stelt de doelpagina in via code
    /// </summary>
    public void SetTargetPage(GameObject target)
    {
        targetPage = target;
    }

    /// <summary>
    /// Stelt de huidige pagina in via code
    /// </summary>
    public void SetCurrentPage(GameObject current)
    {
        currentPage = current;
    }
} 