using UnityEngine;
using UnityEngine.UI;
using SoulGames.EasyGridBuilderPro;

namespace VRGrid.UI
{
    /// <summary>
    /// Controller script voor het resetten van het grid naar de originele positie
    /// 
    /// Gebruik:
    /// 1. Voeg dit script toe aan een GameObject met een Button component
    /// 2. Wijs het Grid GameObject toe in de Inspector
    /// 3. De knop zal automatisch gekoppeld worden aan de Reset functie
    /// 
    /// Vereisten:
    /// - Grid GameObject met EasyGridBuilderProXZ component
    /// - Button component op hetzelfde GameObject
    /// </summary>
    public class GridResetController : MonoBehaviour
    {
        [Header("Grid References")]
        [Tooltip("Het hoofdgrid GameObject dat gereset moet worden")]
        public GameObject gridGameObject;
        
        [Tooltip("Reference naar EasyGridBuilderProXZ component (wordt automatisch gevonden)")]
        public EasyGridBuilderProXZ gridBuilderProXZ;
        
        [Header("UI Components")]
        [Tooltip("Button component voor het resetten (wordt automatisch gevonden)")]
        public Button resetButton;
        
        [Header("Reset Settings")]
        [Tooltip("Originele positie van het grid (wordt automatisch ingesteld bij Start)")]
        public Vector3 originalPosition;
        
        [Tooltip("Originele rotatie van het grid (wordt automatisch ingesteld bij Start)")]
        public Vector3 originalRotation;
        
        [Tooltip("Originele schaal van het grid (wordt automatisch ingesteld bij Start)")]
        public Vector3 originalScale = Vector3.one;
        
        [Tooltip("Animatie tijd voor smooth reset (0 voor instant reset)")]
        public float resetAnimationTime = 0.5f;
        
        [Header("Debug")]
        [Tooltip("Enable debug logging")]
        public bool debugMode = false;
        
        // Private variables
        private VRGridGrabInteractable vrGridGrab;
        private EGBProCoordinateSystemSync coordinateSync;
        private bool isResetting = false;
        
        // Animation variables
        private Vector3 startPosition;
        private Quaternion startRotation;
        private Vector3 startScale;
        private float resetTimer = 0f;

        #region Unity Lifecycle

        private void Start()
        {
            InitializeComponents();
            SetupButton();
            SaveOriginalTransform();
            
            if (debugMode)
                Debug.Log($"GridResetController initialized on {gameObject.name}");
        }
        
        private void Update()
        {
            if (isResetting && resetAnimationTime > 0)
            {
                HandleResetAnimation();
            }
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            // Auto-detect button if not assigned
            if (resetButton == null)
            {
                resetButton = GetComponent<Button>();
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
            
            // Get additional components
            if (gridGameObject != null)
            {
                vrGridGrab = gridGameObject.GetComponent<VRGridGrabInteractable>();
                coordinateSync = gridGameObject.GetComponent<EGBProCoordinateSystemSync>();
            }
            
            // Validation
            if (gridGameObject == null)
            {
                Debug.LogError("GridResetController: Geen Grid GameObject gevonden! " +
                             "Wijs het grid GameObject toe in de Inspector.");
            }
            
            if (resetButton == null)
            {
                Debug.LogError("GridResetController: Geen Button component gevonden! " +
                             "Voeg dit script toe aan een GameObject met een Button component.");
            }
        }

        private void SetupButton()
        {
            if (resetButton != null)
            {
                // Voeg click listener toe
                resetButton.onClick.AddListener(ResetGrid);
                
                if (debugMode)
                    Debug.Log("Reset Grid button listener toegevoegd");
            }
        }

        private void SaveOriginalTransform()
        {
            if (gridGameObject != null)
            {
                // Sla de huidige transform op als origineel (bij eerste start)
                if (originalPosition == Vector3.zero && originalRotation == Vector3.zero)
                {
                    originalPosition = gridGameObject.transform.position;
                    originalRotation = gridGameObject.transform.eulerAngles;
                    originalScale = gridGameObject.transform.localScale;
                    
                    if (debugMode)
                    {
                        Debug.Log($"Originele grid transform opgeslagen:");
                        Debug.Log($"- Positie: {originalPosition}");
                        Debug.Log($"- Rotatie: {originalRotation}");
                        Debug.Log($"- Schaal: {originalScale}");
                    }
                }
            }
        }

        #endregion

        #region Reset Functionality

        /// <summary>
        /// Reset het grid naar de originele positie - kan worden aangeroepen vanuit UI
        /// </summary>
        public void ResetGrid()
        {
            if (gridGameObject == null)
            {
                Debug.LogError("GridResetController: Kan grid niet resetten - geen Grid GameObject gevonden!");
                return;
            }
            
            if (isResetting)
            {
                Debug.LogWarning("Grid reset al bezig...");
                return;
            }
            
            if (debugMode)
                Debug.Log("Grid reset gestart...");
            
            // Stop huidige grid interactions
            StopGridInteractions();
            
            if (resetAnimationTime > 0)
            {
                // Start animated reset
                StartAnimatedReset();
            }
            else
            {
                // Instant reset
                PerformInstantReset();
            }
        }
        
        /// <summary>
        /// Reset naar specifieke positie en rotatie
        /// </summary>
        public void ResetGridToPosition(Vector3 position, Vector3 rotation)
        {
            originalPosition = position;
            originalRotation = rotation;
            ResetGrid();
        }
        
        /// <summary>
        /// Stel nieuwe originele positie in (zonder te resetten)
        /// </summary>
        public void SetOriginalPosition()
        {
            if (gridGameObject != null)
            {
                SaveOriginalTransform();
                Debug.Log("Nieuwe originele positie ingesteld");
            }
        }

        private void StopGridInteractions()
        {
            // Stop VR Grab interactions
            if (vrGridGrab != null && vrGridGrab.IsBeingGrabbed())
            {
                // Force release grab if currently being grabbed
                if (debugMode)
                    Debug.Log("Stopping active grid grab interaction");
            }
        }

        private void StartAnimatedReset()
        {
            // Start animation
            startPosition = gridGameObject.transform.position;
            startRotation = gridGameObject.transform.rotation;
            startScale = gridGameObject.transform.localScale;
            resetTimer = 0f;
            isResetting = true;
            
            if (debugMode)
                Debug.Log($"Animated reset gestart (duur: {resetAnimationTime}s)");
        }

        private void HandleResetAnimation()
        {
            resetTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(resetTimer / resetAnimationTime);
            
            // Smooth interpolation curve
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            
            // Interpolate position
            Vector3 currentPos = Vector3.Lerp(startPosition, originalPosition, smoothProgress);
            gridGameObject.transform.position = currentPos;
            
            // Interpolate rotation
            Quaternion targetRotation = Quaternion.Euler(originalRotation);
            Quaternion currentRot = Quaternion.Lerp(startRotation, targetRotation, smoothProgress);
            gridGameObject.transform.rotation = currentRot;
            
            // Interpolate scale
            Vector3 currentScale = Vector3.Lerp(startScale, originalScale, smoothProgress);
            gridGameObject.transform.localScale = currentScale;
            
            // Check if animation is complete
            if (progress >= 1f)
            {
                FinishReset();
            }
        }

        private void PerformInstantReset()
        {
            // Instant reset
            gridGameObject.transform.position = originalPosition;
            gridGameObject.transform.rotation = Quaternion.Euler(originalRotation);
            gridGameObject.transform.localScale = originalScale;
            
            FinishReset();
        }

        private void FinishReset()
        {
            isResetting = false;
            
            // Update coordinate system
            if (coordinateSync != null)
            {
                coordinateSync.ForceSynchronization();
            }
            
            // Notify VR Grid system about reset
            if (vrGridGrab != null)
            {
                vrGridGrab.ForceCoordinateSystemUpdate();
            }
            
            if (debugMode)
            {
                Debug.Log("Grid reset voltooid!");
                Debug.Log($"Nieuwe positie: {gridGameObject.transform.position}");
                Debug.Log($"Nieuwe rotatie: {gridGameObject.transform.eulerAngles}");
            }
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Check of het grid momenteel wordt gereset
        /// </summary>
        public bool IsResetting => isResetting;
        
        /// <summary>
        /// Get de huidige originele positie
        /// </summary>
        public Vector3 GetOriginalPosition() => originalPosition;
        
        /// <summary>
        /// Get de huidige originele rotatie
        /// </summary>
        public Vector3 GetOriginalRotation() => originalRotation;
        
        /// <summary>
        /// Zet de originele transform waarden handmatig
        /// </summary>
        public void SetOriginalTransform(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            originalPosition = position;
            originalRotation = rotation;
            originalScale = scale;
            
            if (debugMode)
                Debug.Log($"Originele transform handmatig ingesteld: Pos:{position}, Rot:{rotation}, Scale:{scale}");
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            // Cleanup button listener
            if (resetButton != null)
            {
                resetButton.onClick.RemoveListener(ResetGrid);
            }
        }

        #endregion
    }
} 