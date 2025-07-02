using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Script om een pijltje (arrow) 180 graden te laten draaien bij expand/collapse
/// Werkt samen met buttons, dropdowns of andere toggle mechanismen
/// </summary>
public class ExpandArrowRotator : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private Transform arrowTransform;
    [SerializeField] private bool findArrowAutomatically = true;
    [SerializeField] private string arrowObjectName = "Arrow";
    
    [Header("Rotation Settings")]
    [SerializeField] private float collapsedRotation = 0f;  // Naar beneden
    [SerializeField] private float expandedRotation = 180f; // Naar boven
    [SerializeField] private bool useZAxisRotation = true;
    
    [Header("Animation Settings")]
    [SerializeField] private bool useAnimation = true;
    [SerializeField] private float rotationDuration = 0.3f;
    [SerializeField] private AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Auto Detection")]
    [SerializeField] private bool detectButtonClicks = true;
    [SerializeField] private bool detectSmartExpandableLayout = true;
    [SerializeField] private bool detectDropdown = true;
    
    private bool isExpanded = false;
    private Coroutine rotationCoroutine;
    private Button button;
    private SmartExpandableLayout smartLayout;
    private Dropdown dropdown;
    private TMP_Dropdown tmpDropdown;

    // Public property voor externe controle
    public bool IsExpanded => isExpanded;

    private void Awake()
    {
        SetupArrowReference();
        SetupAutoDetection();
    }

    private void SetupArrowReference()
    {
        if (arrowTransform == null && findArrowAutomatically)
        {
            // Zoek recursief naar een GameObject met "Arrow" in de naam
            arrowTransform = FindArrowInChildren(transform);
            
            if (arrowTransform == null)
            {
                Debug.LogWarning($"[ExpandArrowRotator] Geen arrow transform gevonden in {gameObject.name}. Zorg ervoor dat je een GameObject hebt met '{arrowObjectName}' in de naam of wijs handmatig een transform toe.");
            }
            else
            {
                Debug.Log($"[ExpandArrowRotator] Arrow automatisch gevonden: {arrowTransform.name}");
            }
        }
    }

    private Transform FindArrowInChildren(Transform parent)
    {
        // Zoek in directe children
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name.ToLower().Contains(arrowObjectName.ToLower()))
            {
                return child;
            }
        }
        
        // Zoek recursief in grandchildren
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindArrowInChildren(parent.GetChild(i));
            if (found != null)
                return found;
        }
        
        return null;
    }

    private void SetupAutoDetection()
    {
        if (detectButtonClicks)
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(ToggleExpansion);
                Debug.Log($"[ExpandArrowRotator] Button click detection ingeschakeld voor {gameObject.name}");
            }
        }

        if (detectSmartExpandableLayout)
        {
            smartLayout = GetComponent<SmartExpandableLayout>();
            if (smartLayout != null)
            {
                Debug.Log($"[ExpandArrowRotator] SmartExpandableLayout gedetecteerd voor {gameObject.name}");
            }
        }

        if (detectDropdown)
        {
            // Zoek naar Unity's standaard Dropdown
            dropdown = GetComponent<Dropdown>();
            if (dropdown != null)
            {
                // Subscribe to dropdown events
                dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
                Debug.Log($"[ExpandArrowRotator] Dropdown gedetecteerd voor {gameObject.name}");
                
                // Set initial state based on dropdown
                SetExpanded(false, false); // Dropdowns start closed
            }
            
            // Zoek naar TextMeshPro Dropdown
            tmpDropdown = GetComponent<TMP_Dropdown>();
            if (tmpDropdown != null)
            {
                // Subscribe to TMP dropdown events
                tmpDropdown.onValueChanged.AddListener(OnTMPDropdownValueChanged);
                Debug.Log($"[ExpandArrowRotator] TMP_Dropdown gedetecteerd voor {gameObject.name}");
                
                // Set initial state based on dropdown
                SetExpanded(false, false); // Dropdowns start closed
            }
        }
    }

    private void OnDropdownValueChanged(int index)
    {
        // Voor dropdowns betekent value change meestal dat de dropdown weer is gesloten
        // We checken of de dropdown template actief is om te bepalen of het open of dicht is
        CheckDropdownState();
    }

    private void OnTMPDropdownValueChanged(int index)
    {
        // Voor TMP dropdowns betekent value change meestal dat de dropdown weer is gesloten
        CheckDropdownState();
    }

    private void CheckDropdownState()
    {
        bool dropdownOpen = false;
        
        if (dropdown != null)
        {
            // Check if dropdown template is active
            Transform template = dropdown.template;
            if (template != null)
            {
                dropdownOpen = template.gameObject.activeInHierarchy;
            }
        }
        else if (tmpDropdown != null)
        {
            // Check if TMP dropdown template is active
            RectTransform template = tmpDropdown.template;
            if (template != null)
            {
                dropdownOpen = template.gameObject.activeInHierarchy;
            }
        }

        SetExpanded(dropdownOpen, useAnimation);
    }

    private void Update()
    {
        // Synchroniseer met SmartExpandableLayout als beschikbaar
        if (smartLayout != null && smartLayout.IsExpanded != isExpanded)
        {
            SetExpanded(smartLayout.IsExpanded, useAnimation);
        }
        
        // Check dropdown state if we have one
        if ((dropdown != null || tmpDropdown != null) && detectDropdown)
        {
            CheckDropdownState();
        }
    }

    public void ToggleExpansion()
    {
        SetExpanded(!isExpanded, useAnimation);
    }

    /// <summary>
    /// Stel de expanded staat in en roteer de arrow
    /// </summary>
    /// <param name="expanded">Of het content panel open is</param>
    /// <param name="animate">Of er animatie gebruikt moet worden</param>
    public void SetExpanded(bool expanded, bool animate = true)
    {
        if (arrowTransform == null) return;

        isExpanded = expanded;
        
        float targetRotation = expanded ? expandedRotation : collapsedRotation;

        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }

        if (animate && useAnimation && Application.isPlaying)
        {
            rotationCoroutine = StartCoroutine(AnimateRotation(targetRotation));
        }
        else
        {
            // Direct zonder animatie
            ApplyRotation(targetRotation);
        }

        Debug.Log($"[ExpandArrowRotator] Arrow rotated to {targetRotation}° (expanded: {expanded})");
    }

    private IEnumerator AnimateRotation(float targetRotation)
    {
        Vector3 startRotation = arrowTransform.localEulerAngles;
        Vector3 endRotation = useZAxisRotation ? 
            new Vector3(startRotation.x, startRotation.y, targetRotation) :
            new Vector3(targetRotation, startRotation.y, startRotation.z);

        // Handle rotation wrap-around (bijv. van 350° naar 10°)
        if (useZAxisRotation)
        {
            float currentZ = startRotation.z;
            if (currentZ > 180f) currentZ -= 360f;
            if (targetRotation > 180f) targetRotation -= 360f;
            
            float diff = Mathf.Abs(targetRotation - currentZ);
            if (diff > 180f)
            {
                if (currentZ < targetRotation)
                    currentZ += 360f;
                else
                    targetRotation += 360f;
            }
            
            startRotation.z = currentZ;
            endRotation.z = targetRotation;
        }

        float elapsedTime = 0f;
        
        while (elapsedTime < rotationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / rotationDuration;
            float curveValue = rotationCurve.Evaluate(t);
            
            Vector3 currentRotation = Vector3.Lerp(startRotation, endRotation, curveValue);
            arrowTransform.localEulerAngles = currentRotation;
            
            yield return null;
        }
        
        // Zorg ervoor dat we exact op de target rotation eindigen
        arrowTransform.localEulerAngles = endRotation;
        rotationCoroutine = null;
    }

    private void ApplyRotation(float targetRotation)
    {
        Vector3 rotation = arrowTransform.localEulerAngles;
        
        if (useZAxisRotation)
            rotation.z = targetRotation;
        else
            rotation.x = targetRotation;
            
        arrowTransform.localEulerAngles = rotation;
    }

    // Public methods voor externe controle
    public void ExpandArrow() => SetExpanded(true, useAnimation);
    public void CollapseArrow() => SetExpanded(false, useAnimation);
    
    // Voor gebruik in Inspector onClick events
    public void ToggleArrow() => ToggleExpansion();

    // Voor debugging
    [ContextMenu("Test Toggle")]
    private void TestToggle()
    {
        ToggleExpansion();
    }
    
    [ContextMenu("Test Expand")]
    private void TestExpand()
    {
        SetExpanded(true, true);
    }
    
    [ContextMenu("Test Collapse")]
    private void TestCollapse()
    {
        SetExpanded(false, true);
    }

    private void OnValidate()
    {
        // Update in editor wanneer settings veranderen
        if (arrowTransform != null && !Application.isPlaying)
        {
            ApplyRotation(isExpanded ? expandedRotation : collapsedRotation);
        }
    }

    private void OnDestroy()
    {
        // Clean up event listeners
        if (dropdown != null)
        {
            dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        }
        if (tmpDropdown != null)
        {
            tmpDropdown.onValueChanged.RemoveListener(OnTMPDropdownValueChanged);
        }
    }
} 