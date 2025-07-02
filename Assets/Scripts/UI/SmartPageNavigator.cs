using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Slimme paginanavigator die automatisch alle andere actieve pagina's sluit.
/// Deze script lost het probleem op waar meerdere pagina's tegelijk actief blijven.
/// Plaats dit script op knoppen in je menu top om tussen pagina's te navigeren.
/// </summary>
public class SmartPageNavigator : MonoBehaviour
{
    [Header("Navigation Settings")]
    [Tooltip("Het GameObject van de pagina om naar toe te navigeren")]
    [SerializeField] private GameObject targetPage;

    [Tooltip("De parent container waar alle pagina's in zitten (bijv. Content)")]
    [SerializeField] private Transform contentContainer;

    [Tooltip("Alleen pagina's met deze naam-patterns sluiten (laat leeg om alle te sluiten)")]
    [SerializeField] private List<string> pageNamePatterns = new List<string>();

    [Tooltip("GameObjects die NOOIT gesloten mogen worden")]
    [SerializeField] private List<GameObject> excludeFromClosing = new List<GameObject>();

    [Header("Debug")]
    [Tooltip("Log debug informatie in de console")]
    [SerializeField] private bool debugMode = true;

    private Button button;

    private void Awake()
    {
        // Haal de Button component op
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("SmartPageNavigator vereist een Button component op hetzelfde GameObject!");
            return;
        }

        // Voeg onClick listener toe
        button.onClick.AddListener(NavigateToPage);

        // Als geen content container is ingesteld, probeer deze automatisch te vinden
        if (contentContainer == null)
        {
            TryFindContentContainer();
        }
    }

    /// <summary>
    /// Probeert automatisch de content container te vinden
    /// </summary>
    private void TryFindContentContainer()
    {
        // Zoek naar een parent met de naam "Content"
        Transform parent = transform.parent;
        while (parent != null)
        {
            if (parent.name.ToLower().Contains("content"))
            {
                contentContainer = parent;
                if (debugMode)
                {
                    Debug.Log($"[SmartPageNavigator] Automatisch content container gevonden: {contentContainer.name}");
                }
                break;
            }
            parent = parent.parent;
        }

        // Als nog steeds niet gevonden, gebruik de Canvas
        if (contentContainer == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                contentContainer = canvas.transform;
                if (debugMode)
                {
                    Debug.Log($"[SmartPageNavigator] Canvas gebruikt als content container: {contentContainer.name}");
                }
            }
        }
    }

    /// <summary>
    /// Navigeert naar de doelpagina en sluit automatisch alle andere actieve pagina's
    /// </summary>
    public void NavigateToPage()
    {
        if (targetPage == null)
        {
            Debug.LogWarning("Doelpagina is niet ingesteld voor SmartPageNavigator op " + gameObject.name);
            return;
        }

        if (contentContainer == null)
        {
            Debug.LogWarning("Content container is niet ingesteld voor SmartPageNavigator op " + gameObject.name);
            return;
        }

        // Sluit alle andere actieve pagina's
        CloseAllOtherPages();

        // Activeer de doelpagina
        targetPage.SetActive(true);

        if (debugMode)
        {
            Debug.Log($"[SmartPageNavigator] Genavigeerd naar pagina: {targetPage.name}");
        }

        // Registreer de schermverandering voor de terugknop
        SimpleBackButton.RegisterScreenChange(targetPage, null);
    }

    /// <summary>
    /// Sluit alle andere actieve pagina's behalve de doelpagina
    /// </summary>
    private void CloseAllOtherPages()
    {
        List<GameObject> closedPages = new List<GameObject>();

        // Loop door alle child objecten in de content container
        foreach (Transform child in contentContainer)
        {
            GameObject childObject = child.gameObject;

            // Skip de doelpagina
            if (childObject == targetPage)
                continue;

            // Skip objecten die uitgesloten zijn van sluiten
            if (excludeFromClosing.Contains(childObject))
                continue;

            // Als er specifieke name patterns zijn ingesteld, controleer die
            if (pageNamePatterns.Count > 0)
            {
                bool matchesPattern = false;
                foreach (string pattern in pageNamePatterns)
                {
                    if (!string.IsNullOrEmpty(pattern) && childObject.name.ToLower().Contains(pattern.ToLower()))
                    {
                        matchesPattern = true;
                        break;
                    }
                }

                // Als het object niet matcht met de patterns, skip het
                if (!matchesPattern)
                    continue;
            }

            // Sluit het object als het actief is
            if (childObject.activeSelf)
            {
                childObject.SetActive(false);
                closedPages.Add(childObject);

                if (debugMode)
                {
                    Debug.Log($"[SmartPageNavigator] Pagina gesloten: {childObject.name}");
                }
            }
        }

        if (debugMode && closedPages.Count > 0)
        {
            Debug.Log($"[SmartPageNavigator] Totaal {closedPages.Count} pagina's gesloten voor navigatie naar {targetPage.name}");
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
    /// Stelt de content container in via code
    /// </summary>
    public void SetContentContainer(Transform container)
    {
        contentContainer = container;
    }

    /// <summary>
    /// Voegt een GameObject toe aan de exclude lijst
    /// </summary>
    public void AddToExcludeList(GameObject obj)
    {
        if (!excludeFromClosing.Contains(obj))
        {
            excludeFromClosing.Add(obj);
        }
    }

    /// <summary>
    /// Voegt een name pattern toe voor selectief sluiten van pagina's
    /// </summary>
    public void AddPageNamePattern(string pattern)
    {
        if (!string.IsNullOrEmpty(pattern) && !pageNamePatterns.Contains(pattern))
        {
            pageNamePatterns.Add(pattern);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(NavigateToPage);
        }
    }

    // Editor helper methods
    #if UNITY_EDITOR
    [ContextMenu("Find Content Container")]
    private void EditorFindContentContainer()
    {
        TryFindContentContainer();
    }

    [ContextMenu("Test Navigation")]
    private void EditorTestNavigation()
    {
        if (Application.isPlaying)
        {
            NavigateToPage();
        }
        else
        {
            Debug.Log("Test Navigation kan alleen worden uitgevoerd tijdens runtime");
        }
    }
    #endif
} 