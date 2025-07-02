using UnityEngine;
using UnityEngine.UI;
using SoulGames.EasyGridBuilderPro;

namespace VRGrid.UI
{
    /// <summary>
    /// Controller script voor het verbergen en tonen van het grid
    /// 
    /// Gebruik:
    /// 1. Voeg dit script toe aan een GameObject met een Button component
    /// 2. Wijs het Grid GameObject toe in de Inspector
    /// 3. De knop zal automatisch gekoppeld worden aan de Toggle functie
    /// 
    /// Vereisten:
    /// - Grid GameObject met EasyGridBuilderProXZ component
    /// - Button component op hetzelfde GameObject
    /// </summary>
    public class GridVisibilityController : MonoBehaviour
    {
        [Header("Grid References")]
        [Tooltip("Het hoofdgrid GameObject waarvan de zichtbaarheid wordt gecontroleerd")]
        public GameObject gridGameObject;
        
        [Tooltip("Reference naar EasyGridBuilderProXZ component (wordt automatisch gevonden)")]
        public EasyGridBuilderProXZ gridBuilderProXZ;
        
        [Header("UI Components")]
        [Tooltip("Button component voor het toggle functie (wordt automatisch gevonden)")]
        public Button toggleButton;
        
        [Tooltip("Text component op de button voor status weergave (optioneel)")]
        public Text buttonText;
        
        [Header("Visibility Settings")]
        [Tooltip("Is het grid momenteel zichtbaar")]
        public bool isGridVisible = true;
        
        [Tooltip("Tekst wanneer grid zichtbaar is")]
        public string visibleButtonText = "Verberg Grid";
        
        [Tooltip("Tekst wanneer grid verborgen is")]
        public string hiddenButtonText = "Toon Grid";
        
        [Header("Animation Settings")]
        [Tooltip("Animatie tijd voor smooth show/hide (0 voor instant toggle)")]
        public float toggleAnimationTime = 0.3f;
        
        [Tooltip("Gebruik fade animatie in plaats van instant show/hide")]
        public bool useFadeAnimation = true;
        
        [Header("Debug")]
        [Tooltip("Enable debug logging")]
        public bool debugMode = false;
        
        // Private variables
        private Renderer[] gridRenderers;
        private Collider[] gridColliders;
        private bool isToggling = false;
        
        // Animation variables
        private float toggleTimer = 0f;
        private float[] originalAlphaValues;
        private Material[] gridMaterials;

        #region Unity Lifecycle

        private void Start()
        {
            InitializeComponents();
            SetupButton();
            CacheGridComponents();
            UpdateButtonText();
            
            if (debugMode)
                Debug.Log($"GridVisibilityController initialized on {gameObject.name}");
        }
        
        private void Update()
        {
            if (isToggling && toggleAnimationTime > 0 && useFadeAnimation)
            {
                HandleToggleAnimation();
            }
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            // Auto-detect button if not assigned
            if (toggleButton == null)
            {
                toggleButton = GetComponent<Button>();
            }
            
            // Auto-detect button text if not assigned
            if (buttonText == null && toggleButton != null)
            {
                buttonText = toggleButton.GetComponentInChildren<Text>();
            }
            
            // Auto-detect grid if not assigned
            if (gridGameObject == null)
            {
                // Zoek naar het eerste EasyGridBuilderProXZ component in de scene
                gridBuilderProXZ = FindObjectOfType<EasyGridBuilderProXZ>();
                if (gridBuilderProXZ != null)
                {
                    gridGameObject = gridBuilderProXZ.gameObject;
                }
            }
            
            // Get grid component if we have the GameObject
            if (gridGameObject != null && gridBuilderProXZ == null)
            {
                gridBuilderProXZ = gridGameObject.GetComponent<EasyGridBuilderProXZ>();
            }
            
            // Validation
            if (gridGameObject == null)
            {
                Debug.LogError("GridVisibilityController: Geen Grid GameObject gevonden! " +
                             "Wijs het grid GameObject toe in de Inspector.");
            }
            
            if (toggleButton == null)
            {
                Debug.LogError("GridVisibilityController: Geen Button component gevonden! " +
                             "Voeg dit script toe aan een GameObject met een Button component.");
            }
        }

        private void SetupButton()
        {
            if (toggleButton != null)
            {
                // Voeg click listener toe
                toggleButton.onClick.AddListener(ToggleGridVisibility);
                
                if (debugMode)
                    Debug.Log("Grid visibility toggle button listener toegevoegd");
            }
        }

        private void CacheGridComponents()
        {
            if (gridGameObject != null)
            {
                // Cache alle renderers voor fade animatie
                gridRenderers = gridGameObject.GetComponentsInChildren<Renderer>();
                
                // Cache alle colliders voor interactie toggle
                gridColliders = gridGameObject.GetComponentsInChildren<Collider>();
                
                if (useFadeAnimation && gridRenderers.Length > 0)
                {
                    // Cache materials en originele alpha waarden
                    CacheMaterialAlphaValues();
                }
                
                if (debugMode)
                {
                    Debug.Log($"Grid components gecached: {gridRenderers.Length} renderers, {gridColliders.Length} colliders");
                }
            }
        }

        private void CacheMaterialAlphaValues()
        {
            var materialsList = new System.Collections.Generic.List<Material>();
            var alphasList = new System.Collections.Generic.List<float>();
            
            foreach (var renderer in gridRenderers)
            {
                foreach (var material in renderer.materials)
                {
                    materialsList.Add(material);
                    
                    // Cache originele alpha waarde
                    if (material.HasProperty("_Color"))
                    {
                        alphasList.Add(material.color.a);
                    }
                    else
                    {
                        alphasList.Add(1f);
                    }
                }
            }
            
            gridMaterials = materialsList.ToArray();
            originalAlphaValues = alphasList.ToArray();
        }

        #endregion

        #region Visibility Control

        /// <summary>
        /// Toggle de grid zichtbaarheid - kan worden aangeroepen vanuit UI
        /// </summary>
        public void ToggleGridVisibility()
        {
            if (gridGameObject == null)
            {
                Debug.LogError("GridVisibilityController: Kan grid zichtbaarheid niet wijzigen - geen Grid GameObject gevonden!");
                return;
            }
            
            // Toggle visibility state
            isGridVisible = !isGridVisible;
            
            if (debugMode)
                Debug.Log($"Grid visibility toggle - Nieuwe staat: {(isGridVisible ? "Zichtbaar" : "Verborgen")}");
            
            // Simpele toggle - zet het hele GameObject aan/uit
            gridGameObject.SetActive(isGridVisible);
            
            UpdateButtonText();
            
            if (debugMode)
                Debug.Log($"Grid GameObject.SetActive({isGridVisible}) - Klaar!");
        }
        
        /// <summary>
        /// Toon het grid expliciet
        /// </summary>
        public void ShowGrid()
        {
            if (!isGridVisible)
            {
                ToggleGridVisibility();
            }
        }
        
        /// <summary>
        /// Verberg het grid expliciet
        /// </summary>
        public void HideGrid()
        {
            if (isGridVisible)
            {
                ToggleGridVisibility();
            }
        }

        private void StartAnimatedToggle()
        {
            toggleTimer = 0f;
            isToggling = true;
            
            if (debugMode)
                Debug.Log($"Animated visibility toggle gestart (duur: {toggleAnimationTime}s)");
        }

        private void HandleToggleAnimation()
        {
            toggleTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(toggleTimer / toggleAnimationTime);
            
            // Bepaal alpha waarde op basis van visibility state
            float targetAlpha = isGridVisible ? 1f : 0f;
            float startAlpha = isGridVisible ? 0f : 1f;
            
            // Smooth interpolation
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            
            // Apply alpha to all materials
            ApplyAlphaToMaterials(currentAlpha);
            
            // Check if animation is complete
            if (progress >= 1f)
            {
                FinishToggle();
            }
        }

        private void ApplyAlphaToMaterials(float alpha)
        {
            for (int i = 0; i < gridMaterials.Length && i < originalAlphaValues.Length; i++)
            {
                var material = gridMaterials[i];
                if (material != null && material.HasProperty("_Color"))
                {
                    Color color = material.color;
                    color.a = originalAlphaValues[i] * alpha;
                    material.color = color;
                }
            }
        }

        private void PerformInstantToggle()
        {
            // Gebruik EGB Pro2's ingebouwde visibility system als beschikbaar
            if (gridBuilderProXZ != null)
            {
                try
                {
                    // Probeer EGB Pro2's visibility methodes te gebruiken
                    if (isGridVisible)
                    {
                        // Toon grid met EGB Pro2 methode
                        var displayMethod = gridBuilderProXZ.GetType().GetMethod("SetIsDisplayObjectGrid");
                        if (displayMethod != null)
                        {
                            displayMethod.Invoke(gridBuilderProXZ, new object[] { true });
                        }
                        else
                        {
                            // Fallback: gebruik renderers
                            SetRenderersEnabled(true);
                        }
                    }
                    else
                    {
                        // Verberg grid met EGB Pro2 methode
                        var displayMethod = gridBuilderProXZ.GetType().GetMethod("SetIsDisplayObjectGrid");
                        if (displayMethod != null)
                        {
                            displayMethod.Invoke(gridBuilderProXZ, new object[] { false });
                        }
                        else
                        {
                            // Fallback: gebruik renderers
                            SetRenderersEnabled(false);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    if (debugMode)
                        Debug.LogWarning($"EGB Pro2 visibility methode niet beschikbaar, gebruik fallback: {e.Message}");
                    
                    // Fallback method
                    SetRenderersEnabled(isGridVisible);
                }
            }
            else
            {
                // Fallback method zonder EGB Pro2
                SetRenderersEnabled(isGridVisible);
            }
            
            // Update colliders voor interactie
            SetCollidersEnabled(isGridVisible);
            
            FinishToggle();
        }

        private void SetRenderersEnabled(bool enabled)
        {
            foreach (var renderer in gridRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = enabled;
                }
            }
        }

        private void SetCollidersEnabled(bool enabled)
        {
            foreach (var collider in gridColliders)
            {
                if (collider != null)
                {
                    collider.enabled = enabled;
                }
            }
        }

        private void FinishToggle()
        {
            isToggling = false;
            
            // Ensure final state is correct
            if (useFadeAnimation)
            {
                ApplyAlphaToMaterials(isGridVisible ? 1f : 0f);
                
                // Also disable renderers when fully hidden for performance
                if (!isGridVisible)
                {
                    SetRenderersEnabled(false);
                }
                else
                {
                    SetRenderersEnabled(true);
                }
            }
            
            if (debugMode)
            {
                Debug.Log($"Grid visibility toggle voltooid! Grid is nu {(isGridVisible ? "zichtbaar" : "verborgen")}");
            }
        }

        private void UpdateButtonText()
        {
            if (buttonText != null)
            {
                // Gebruik default teksten als ze leeg zijn
                string showText = string.IsNullOrEmpty(visibleButtonText) ? "Verberg Grid" : visibleButtonText;
                string hideText = string.IsNullOrEmpty(hiddenButtonText) ? "Toon Grid" : hiddenButtonText;
                
                buttonText.text = isGridVisible ? showText : hideText;
                
                if (debugMode)
                    Debug.Log($"Button text updated: {buttonText.text}");
            }
            else if (debugMode)
            {
                Debug.Log("Geen button text gevonden voor update");
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Check of het grid momenteel zichtbaar is
        /// </summary>
        public bool IsGridVisible => isGridVisible;
        
        /// <summary>
        /// Check of er momenteel een toggle animatie actief is
        /// </summary>
        public bool IsToggling => isToggling;
        
        /// <summary>
        /// Stel de button teksten in
        /// </summary>
        public void SetButtonTexts(string visibleText, string hiddenText)
        {
            visibleButtonText = visibleText;
            hiddenButtonText = hiddenText;
            UpdateButtonText();
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            // Cleanup button listener
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveListener(ToggleGridVisibility);
            }
        }

        #endregion
    }
} 