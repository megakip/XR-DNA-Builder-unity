using UnityEngine;
using UnityEngine.UI;
using Unity.XR.CoreUtils;

namespace VRGrid.UI
{
    /// <summary>
    /// Meta SDK Compatible Map Controller voor het beheren van achtergrond kaart opacity in VR
    /// 
    /// Dit script combineert:
    /// - GridBackgroundOpacityController functionaliteit
    /// - Meta SDK compatibility
    /// - XR Canvas setup voor World Space UI
    /// 
    /// Gebruik:
    /// 1. Voeg dit script toe aan een GameObject met een Canvas component
    /// 2. Stel de Canvas in op World Space rendering
    /// 3. Configureer de background image en opacity slider referenties
    /// 4. Het script zorgt automatisch voor de juiste camera setup
    /// </summary>
    public class MetaSDKCompatibleMapController : MonoBehaviour
    {
        [Header("Background Image Settings")]
        [Tooltip("De Image component die als grid achtergrond dient (kaart)")]
        public Image backgroundImage;
        
        [Tooltip("Alternatief: RawImage component als je RawImage gebruikt")]
        public RawImage backgroundRawImage;
        
        [Header("Opacity Control")]
        [Tooltip("De slider die de opacity regelt (0 = transparant, 1 = ondoorzichtig)")]
        public Slider opacitySlider;
        
        [Range(0f, 1f)]
        [Tooltip("Standaard opacity waarde")]
        public float defaultOpacity = 0.5f;
        
        [Header("XR & Meta SDK Settings")]
        [Tooltip("Automatisch XR Origin vinden en camera instellen")]
        public bool autoSetupXRCamera = true;
        
        [Tooltip("Afstand van de camera waar het canvas wordt geplaatst")]
        public float canvasDistance = 2.0f;
        
        [Tooltip("Canvas schaal voor VR (kleinere waarden = kleinere UI)")]
        public float canvasScale = 0.001f;
        
        [Header("Debug")]
        [Tooltip("Debug logging inschakelen")]
        public bool enableDebugLogging = false;

        private Canvas canvas;
        private XROrigin xrOrigin;
        private Camera xrCamera;
        
        private void Start()
        {
            // Initialiseer het systeem
            InitializeCanvasSetup();
            InitializeOpacityControl();
        }
        
        /// <summary>
        /// Setup de Canvas voor Meta SDK compatibility
        /// </summary>
        private void InitializeCanvasSetup()
        {
            // Vind Canvas component
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("MetaSDKCompatibleMapController: Geen Canvas component gevonden!");
                enabled = false;
                return;
            }
            
            // Setup voor World Space Canvas
            if (canvas.renderMode != RenderMode.WorldSpace)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                if (enableDebugLogging)
                    Debug.Log("Canvas render mode ingesteld op World Space");
            }
            
            // Auto-setup XR camera indien gewenst
            if (autoSetupXRCamera)
            {
                SetupXRCamera();
            }
            
            // Configureer Canvas scaling
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                canvasRect.localScale = Vector3.one * canvasScale;
                if (enableDebugLogging)
                    Debug.Log($"Canvas schaal ingesteld op {canvasScale}");
            }
        }
        
        /// <summary>
        /// Setup XR camera voor Meta SDK
        /// </summary>
        private void SetupXRCamera()
        {
            // Zoek XR Origin
            xrOrigin = FindObjectOfType<XROrigin>();
            if (xrOrigin == null)
            {
                // Probeer ook de oude XRRig setup
                var xrRig = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
                if (xrRig != null)
                {
                    xrOrigin = xrRig;
                }
            }
            
            if (xrOrigin != null)
            {
                // Vind de camera in XR Origin
                xrCamera = xrOrigin.Camera;
                if (xrCamera == null)
                {
                    // Alternatieve zoeklogica
                    xrCamera = xrOrigin.GetComponentInChildren<Camera>();
                }
                
                if (xrCamera != null)
                {
                    // Stel event camera in voor Canvas
                    canvas.worldCamera = xrCamera;
                    
                    // Positioneer Canvas voor de camera
                    PositionCanvasInFrontOfCamera();
                    
                    if (enableDebugLogging)
                        Debug.Log($"XR Camera gevonden en ingesteld: {xrCamera.name}");
                }
                else
                {
                    Debug.LogWarning("Geen camera gevonden in XR Origin!");
                }
            }
            else
            {
                Debug.LogWarning("Geen XR Origin gevonden! Controleer je Meta SDK setup.");
            }
        }
        
        /// <summary>
        /// Positioneer het canvas voor de XR camera
        /// </summary>
        private void PositionCanvasInFrontOfCamera()
        {
            if (xrCamera == null) return;
            
            // Positioneer canvas voor de camera
            Vector3 cameraForward = xrCamera.transform.forward;
            Vector3 canvasPosition = xrCamera.transform.position + (cameraForward * canvasDistance);
            
            transform.position = canvasPosition;
            transform.LookAt(xrCamera.transform);
            
            // Draai 180 graden zodat de voorkant naar de camera wijst
            transform.Rotate(0, 180, 0);
            
            if (enableDebugLogging)
                Debug.Log($"Canvas gepositioneerd op {canvasPosition}");
        }
        
        /// <summary>
        /// Initialiseer het opacity control systeem
        /// </summary>
        private void InitializeOpacityControl()
        {
            // Setup slider
            if (opacitySlider != null)
            {
                opacitySlider.minValue = 0f;
                opacitySlider.maxValue = 1f;
                opacitySlider.value = defaultOpacity;
                
                // Voeg listener toe
                opacitySlider.onValueChanged.AddListener(OnOpacitySliderChanged);
                
                if (enableDebugLogging)
                    Debug.Log($"Opacity slider geïnitialiseerd met waarde {defaultOpacity}");
            }
            else
            {
                Debug.LogWarning("Geen opacity slider toegewezen!");
            }
            
            // Stel initiële opacity in
            SetOpacity(defaultOpacity);
        }
        
        /// <summary>
        /// Callback voor slider value changes
        /// </summary>
        private void OnOpacitySliderChanged(float value)
        {
            SetOpacity(value);
        }
        
        /// <summary>
        /// Stel de opacity van de achtergrond afbeelding in
        /// </summary>
        public void SetOpacity(float opacity)
        {
            opacity = Mathf.Clamp01(opacity);
            
            // Pas opacity toe op Image
            if (backgroundImage != null)
            {
                Color imageColor = backgroundImage.color;
                imageColor.a = opacity;
                backgroundImage.color = imageColor;
                
                if (enableDebugLogging)
                    Debug.Log($"Map Image opacity ingesteld op {opacity}");
            }
            
            // Pas opacity toe op RawImage
            if (backgroundRawImage != null)
            {
                Color rawImageColor = backgroundRawImage.color;
                rawImageColor.a = opacity;
                backgroundRawImage.color = rawImageColor;
                
                if (enableDebugLogging)
                    Debug.Log($"Map RawImage opacity ingesteld op {opacity}");
            }
            
            // Waarschuwing als geen image is toegewezen
            if (backgroundImage == null && backgroundRawImage == null)
            {
                Debug.LogWarning("Geen background image component toegewezen!");
            }
        }
        
        /// <summary>
        /// Stel opacity in via percentage (0-100)
        /// </summary>
        public void SetOpacityByPercentage(float percentage)
        {
            SetOpacity(percentage / 100f);
        }
        
        /// <summary>
        /// Krijg de huidige opacity waarde
        /// </summary>
        public float GetOpacity()
        {
            if (backgroundImage != null)
                return backgroundImage.color.a;
            
            if (backgroundRawImage != null)
                return backgroundRawImage.color.a;
                
            return 0f;
        }
        
        /// <summary>
        /// Reset naar standaard opacity
        /// </summary>
        public void ResetToDefault()
        {
            SetOpacity(defaultOpacity);
            
            if (opacitySlider != null)
                opacitySlider.value = defaultOpacity;
        }
        
        /// <summary>
        /// Fade naar een target opacity over tijd
        /// </summary>
        public void FadeToOpacity(float targetOpacity, float duration = 1f)
        {
            StartCoroutine(FadeCoroutine(targetOpacity, duration));
        }
        
        /// <summary>
        /// Coroutine voor smooth opacity fading
        /// </summary>
        private System.Collections.IEnumerator FadeCoroutine(float targetOpacity, float duration)
        {
            float startOpacity = GetOpacity();
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float currentOpacity = Mathf.Lerp(startOpacity, targetOpacity, elapsed / duration);
                SetOpacity(currentOpacity);
                
                // Update slider
                if (opacitySlider != null)
                    opacitySlider.value = currentOpacity;
                    
                yield return null;
            }
            
            // Zorg dat we de exacte target bereiken
            SetOpacity(targetOpacity);
            if (opacitySlider != null)
                opacitySlider.value = targetOpacity;
        }
        
        /// <summary>
        /// Update canvas positie (kan handig zijn tijdens runtime)
        /// </summary>
        public void UpdateCanvasPosition()
        {
            if (xrCamera != null)
            {
                PositionCanvasInFrontOfCamera();
            }
        }
        
        /// <summary>
        /// Public methode om de Canvas setup te refreshen
        /// </summary>
        public void RefreshCanvasSetup()
        {
            InitializeCanvasSetup();
        }
        
        private void OnDestroy()
        {
            // Cleanup slider listener
            if (opacitySlider != null)
            {
                opacitySlider.onValueChanged.RemoveListener(OnOpacitySliderChanged);
            }
        }
        
        private void OnValidate()
        {
            // Zorg dat default opacity binnen bereik blijft in editor
            defaultOpacity = Mathf.Clamp01(defaultOpacity);
            canvasScale = Mathf.Max(0.0001f, canvasScale);
        }
        
        /// <summary>
        /// Unity editor method om componenten automatisch toe te wijzen
        /// </summary>
        private void Reset()
        {
            // Probeer automatisch componenten te vinden
            if (backgroundImage == null)
                backgroundImage = GetComponentInChildren<Image>();
            
            if (backgroundRawImage == null)
                backgroundRawImage = GetComponentInChildren<RawImage>();
                
            if (opacitySlider == null)
                opacitySlider = GetComponentInChildren<Slider>();
        }
    }
} 