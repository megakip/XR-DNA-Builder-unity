using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NavigationButtonData", menuName = "UI/Navigation/Button Data", order = 1)]
public class NavigationButtonData : ScriptableObject
{
    [Header("Basic Settings")]
    public Sprite navigateIcon;
    public string buttonName;
    
    [Header("Navigation - Simpel")]
    [Tooltip("Het GameObject (UI panel/scherm) dat geactiveerd moet worden")]
    [SerializeField] private Transform _targetGameObject;
    public Transform targetGameObject 
    { 
        get { return _targetGameObject; } 
        set { _targetGameObject = value; } 
    }
    
    [Tooltip("Automatisch andere actieve panels sluiten")]
    public bool closeOtherPanels = true;
    
    [Header("Custom Actions (Optioneel)")]
    [Tooltip("Extra acties die uitgevoerd worden wanneer de knop wordt ingedrukt")]
    public UnityEvent onButtonClicked;
    
    [Header("Panel Toggle Settings")]
    public bool usePanelToggle = false;
    public Sprite panelToggleIcon;
    
    [System.Serializable]
    public class InformationItem
    {
        public string informationText;
        public bool isExpanded = false;
    }
    
    [Header("Information Content")]
    public List<InformationItem> informationItems = new List<InformationItem>();
    
    [Header("Breadcrumb Settings")]
    public GameObject breadcrumbPrefab;
    public string breadcrumbText;
    public NavigationButtonData parentButton; // Voor hiërarchie in breadcrumbs
}
