using UnityEngine;
using UnityEngine.InputSystem;
using SoulGames.EasyGridBuilderPro;

/// <summary>
/// Geavanceerde VR Controller script voor Undo/Redo functionaliteit met Input System Actions.
/// Dit script gebruikt de Unity Input System om thumbstick input van beide controllers te detecteren.
/// 
/// Instructies:
/// 1. Voeg dit script toe aan een GameObject in je scene
/// 2. Wijs de juiste Input Action References toe in de Inspector
/// 3. Configureer threshold en cooldown waarden naar wens
/// 4. Het script werkt automatisch met beide controllers
/// </summary>
public class VRUndoRedoControllerAdvanced : MonoBehaviour
{
    [Header("Input Action References")]
    [SerializeField]
    [Tooltip("Input Action Reference voor linker controller thumbstick")]
    private InputActionReference m_LeftThumbstickAction;
    
    [SerializeField]
    [Tooltip("Input Action Reference voor rechter controller thumbstick")]
    private InputActionReference m_RightThumbstickAction;
    
    [Header("Undo/Redo Settings")]
    [SerializeField]
    [Range(0.5f, 1.0f)]
    [Tooltip("Threshold waarde voor activatie (0.8 = 80% van maximale beweging)")]
    private float m_ActivationThreshold = 0.8f;
    
    [SerializeField]
    [Range(0.1f, 2.0f)]
    [Tooltip("Cooldown tijd tussen acties om spam te voorkomen")]
    private float m_ActionCooldown = 0.5f;
    
    [SerializeField]
    [Tooltip("Alleen horizontale thumbstick beweging gebruiken (anders ook verticaal)")]
    private bool m_UseOnlyHorizontalInput = true;
    
    [Header("Feedback")]
    [SerializeField]
    [Tooltip("Toon debug informatie in console")]
    private bool m_ShowDebugInfo = true;
    
    [SerializeField]
    [Tooltip("Voeg haptic feedback toe bij undo/redo")]
    private bool m_EnableHapticFeedback = true;
    
    [SerializeField]
    [Tooltip("Sterkte van haptic feedback (0.0 - 1.0)")]
    [Range(0.0f, 1.0f)]
    private float m_HapticIntensity = 0.3f;
    
    [SerializeField]
    [Tooltip("Duur van haptic feedback in seconden")]
    [Range(0.1f, 1.0f)]
    private float m_HapticDuration = 0.2f;
    
    // Private variabelen
    private float m_LastUndoTime = -999f;
    private float m_LastRedoTime = -999f;
    private Vector2 m_PreviousLeftInput = Vector2.zero;
    private Vector2 m_PreviousRightInput = Vector2.zero;
    
    // Input Actions
    private InputAction m_LeftThumbstick;
    private InputAction m_RightThumbstick;
    
    void Start()
    {
        InitializeInputActions();
    }
    
    void OnEnable()
    {
        EnableInputActions();
        
        if (m_ShowDebugInfo)
        {
            Debug.Log("[VR Undo/Redo] Controller geactiveerd - Thumbstick links voor Undo, rechts voor Redo");
        }
    }
    
    void OnDisable()
    {
        DisableInputActions();
    }
    
    void OnDestroy()
    {
        DisableInputActions();
    }
    
    void Update()
    {
        ProcessThumbstickInput();
    }
    
    private void InitializeInputActions()
    {
        // Initialiseer Input Actions van de references
        if (m_LeftThumbstickAction != null)
        {
            m_LeftThumbstick = m_LeftThumbstickAction.action;
        }
        
        if (m_RightThumbstickAction != null)
        {
            m_RightThumbstick = m_RightThumbstickAction.action;
        }
        
        // Als er geen references zijn ingesteld, probeer de standaard actions te vinden
        if (m_LeftThumbstick == null || m_RightThumbstick == null)
        {
            TryFindDefaultThumbstickActions();
        }
    }
    
    private void TryFindDefaultThumbstickActions()
    {
        // Probeer standaard XR Input Actions te vinden
        var inputAsset = Resources.Load<InputActionAsset>("XRI Default Input Actions");
        if (inputAsset == null)
        {
            inputAsset = Resources.Load<InputActionAsset>("Meta SDK Fixed Input Actions");
        }
        
        if (inputAsset != null)
        {
            var leftHandMap = inputAsset.FindActionMap("XRI LeftHand");
            var rightHandMap = inputAsset.FindActionMap("XRI RightHand");
            
            if (leftHandMap != null && m_LeftThumbstick == null)
            {
                m_LeftThumbstick = leftHandMap.FindAction("Thumbstick");
            }
            
            if (rightHandMap != null && m_RightThumbstick == null)
            {
                m_RightThumbstick = rightHandMap.FindAction("Thumbstick");
            }
        }
        
        if (m_ShowDebugInfo)
        {
            Debug.Log($"[VR Undo/Redo] Left thumbstick action: {(m_LeftThumbstick != null ? "Found" : "Not Found")}");
            Debug.Log($"[VR Undo/Redo] Right thumbstick action: {(m_RightThumbstick != null ? "Found" : "Not Found")}");
        }
    }
    
    private void EnableInputActions()
    {
        m_LeftThumbstick?.Enable();
        m_RightThumbstick?.Enable();
    }
    
    private void DisableInputActions()
    {
        m_LeftThumbstick?.Disable();
        m_RightThumbstick?.Disable();
    }
    
    private void ProcessThumbstickInput()
    {
        Vector2 leftInput = m_LeftThumbstick?.ReadValue<Vector2>() ?? Vector2.zero;
        Vector2 rightInput = m_RightThumbstick?.ReadValue<Vector2>() ?? Vector2.zero;
        
        // Proces input van beide controllers
        ProcessControllerInput(leftInput, ref m_PreviousLeftInput, "Left");
        ProcessControllerInput(rightInput, ref m_PreviousRightInput, "Right");
    }
    
    private void ProcessControllerInput(Vector2 currentInput, ref Vector2 previousInput, string controllerName)
    {
        float horizontalInput = currentInput.x;
        float inputMagnitude = m_UseOnlyHorizontalInput ? Mathf.Abs(horizontalInput) : currentInput.magnitude;
        
        // Check voor Undo (links)
        if (horizontalInput <= -m_ActivationThreshold && previousInput.x > -m_ActivationThreshold)
        {
            if (Time.time - m_LastUndoTime >= m_ActionCooldown)
            {
                ExecuteUndo(controllerName);
                m_LastUndoTime = Time.time;
            }
        }
        
        // Check voor Redo (rechts)
        if (horizontalInput >= m_ActivationThreshold && previousInput.x < m_ActivationThreshold)
        {
            if (Time.time - m_LastRedoTime >= m_ActionCooldown)
            {
                ExecuteRedo(controllerName);
                m_LastRedoTime = Time.time;
            }
        }
        
        previousInput = currentInput;
    }
    
    private void ExecuteUndo(string controllerName)
    {
        try
        {
            if (GridManager.Instance?.GetGridCommandInvoker() != null)
            {
                GridManager.Instance.GetGridCommandInvoker().UndoCommand();
                
                if (m_ShowDebugInfo)
                {
                    Debug.Log($"[VR Undo/Redo] Undo uitgevoerd via {controllerName} controller!");
                }
                
                if (m_EnableHapticFeedback)
                {
                    TriggerHapticFeedback(controllerName);
                }
            }
            else
            {
                Debug.LogWarning("[VR Undo/Redo] GridManager of CommandInvoker niet beschikbaar voor Undo");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[VR Undo/Redo] Fout bij Undo: {e.Message}");
        }
    }
    
    private void ExecuteRedo(string controllerName)
    {
        try
        {
            if (GridManager.Instance?.GetGridCommandInvoker() != null)
            {
                GridManager.Instance.GetGridCommandInvoker().RedoCommand();
                
                if (m_ShowDebugInfo)
                {
                    Debug.Log($"[VR Undo/Redo] Redo uitgevoerd via {controllerName} controller!");
                }
                
                if (m_EnableHapticFeedback)
                {
                    TriggerHapticFeedback(controllerName);
                }
            }
            else
            {
                Debug.LogWarning("[VR Undo/Redo] GridManager of CommandInvoker niet beschikbaar voor Redo");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[VR Undo/Redo] Fout bij Redo: {e.Message}");
        }
    }
    
    private void TriggerHapticFeedback(string controllerName)
    {
        // Implementatie voor haptic feedback kan hier worden toegevoegd
        // Dit vereist toegang tot de XR controller devices
        if (m_ShowDebugInfo)
        {
            Debug.Log($"[VR Undo/Redo] Haptic feedback geactiveerd voor {controllerName} controller");
        }
    }
    
    // Editor test functies
    [ContextMenu("Test Undo")]
    private void TestUndo()
    {
        ExecuteUndo("Manual Test");
    }
    
    [ContextMenu("Test Redo")]
    private void TestRedo()
    {
        ExecuteRedo("Manual Test");
    }
    
    // GUI voor runtime informatie
    void OnGUI()
    {
        if (!m_ShowDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label("VR Undo/Redo Debug Info:");
        
        Vector2 leftInput = m_LeftThumbstick?.ReadValue<Vector2>() ?? Vector2.zero;
        Vector2 rightInput = m_RightThumbstick?.ReadValue<Vector2>() ?? Vector2.zero;
        
        GUILayout.Label($"Left Thumbstick: {leftInput:F2}");
        GUILayout.Label($"Right Thumbstick: {rightInput:F2}");
        GUILayout.Label($"Threshold: {m_ActivationThreshold:F2}");
        GUILayout.Label($"Last Undo: {Time.time - m_LastUndoTime:F1}s ago");
        GUILayout.Label($"Last Redo: {Time.time - m_LastRedoTime:F1}s ago");
        
        GUILayout.EndArea();
    }
} 