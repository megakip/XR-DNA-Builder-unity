using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using SoulGames.EasyGridBuilderPro;

/// <summary>
/// VR Controller script voor Undo/Redo functionaliteit met thumbstick input.
/// Dit script detecteert wanneer de thumbstick helemaal naar links (Undo) of rechts (Redo) wordt bewogen.
/// 
/// Instructies:
/// 1. Voeg dit script toe aan een GameObject in je scene (bijvoorbeeld een leeg GameObject genaamd "VR Undo Redo Controller")
/// 2. Configureer de XR Input Reader settings in de Inspector
/// 3. Stel de threshold waarde in (standaard 0.8f betekent 80% van de maximale thumbstick beweging)
/// 4. Optioneel: pas de cooldown tijd aan om te voorkomen dat undo/redo te snel herhaalt
/// </summary>
public class VRUndoRedoController : MonoBehaviour
{
    [Header("Thumbstick Input Settings")]
    [SerializeField]
    [Tooltip("XR Input Reader voor thumbstick input van beide controllers")]
    private XRInputValueReader<Vector2> m_LeftThumbstickInput = new XRInputValueReader<Vector2>("Left Thumbstick");
    
    [SerializeField]
    private XRInputValueReader<Vector2> m_RightThumbstickInput = new XRInputValueReader<Vector2>("Right Thumbstick");
    
    [Header("Undo/Redo Settings")]
    [SerializeField]
    [Range(0.5f, 1.0f)]
    [Tooltip("Hoe ver de thumbstick naar links/rechts moet bewegen om undo/redo te activeren (0.5 = 50%, 1.0 = 100%)")]
    private float m_ThresholdValue = 0.8f;
    
    [SerializeField]
    [Tooltip("Tijd in seconden tussen undo/redo acties om te voorkomen dat het te snel herhaalt")]
    private float m_CooldownTime = 0.5f;
    
    [Header("Debug")]
    [SerializeField]
    [Tooltip("Toon debug berichten in de console")]
    private bool m_EnableDebugLogs = true;
    
    // Private variabelen
    private float m_LastUndoTime = 0f;
    private float m_LastRedoTime = 0f;
    private bool m_WasLeftThresholdExceeded = false;
    private bool m_WasRightThresholdExceeded = false;
    
    void OnEnable()
    {
        // Activeer de input readers
        m_LeftThumbstickInput?.EnableDirectActionIfModeUsed();
        m_RightThumbstickInput?.EnableDirectActionIfModeUsed();
        
        if (m_EnableDebugLogs)
        {
            Debug.Log("VR Undo/Redo Controller geactiveerd. Beweeg thumbstick naar links voor Undo, naar rechts voor Redo.");
        }
    }
    
    void OnDisable()
    {
        // Deactiveer de input readers
        m_LeftThumbstickInput?.DisableDirectActionIfModeUsed();
        m_RightThumbstickInput?.DisableDirectActionIfModeUsed();
    }
    
    void Update()
    {
        CheckThumbstickInput();
    }
    
    private void CheckThumbstickInput()
    {
        Vector2 leftThumbstick = Vector2.zero;
        Vector2 rightThumbstick = Vector2.zero;
        
        // Lees thumbstick waarden van beide controllers
        if (m_LeftThumbstickInput != null)
        {
            leftThumbstick = m_LeftThumbstickInput.ReadValue();
        }
        
        if (m_RightThumbstickInput != null)
        {
            rightThumbstick = m_RightThumbstickInput.ReadValue();
        }
        
        // Gebruik de thumbstick input van beide controllers (welke dan ook)
        Vector2 combinedInput = Mathf.Abs(leftThumbstick.x) > Mathf.Abs(rightThumbstick.x) ? leftThumbstick : rightThumbstick;
        
        CheckUndoInput(combinedInput.x);
        CheckRedoInput(combinedInput.x);
    }
    
    private void CheckUndoInput(float horizontalInput)
    {
        bool isLeftThresholdExceeded = horizontalInput <= -m_ThresholdValue;
        
        // Detecteer wanneer de threshold voor het eerst wordt overschreden (edge detection)
        if (isLeftThresholdExceeded && !m_WasLeftThresholdExceeded)
        {
            if (Time.time - m_LastUndoTime >= m_CooldownTime)
            {
                PerformUndo();
                m_LastUndoTime = Time.time;
            }
        }
        
        m_WasLeftThresholdExceeded = isLeftThresholdExceeded;
    }
    
    private void CheckRedoInput(float horizontalInput)
    {
        bool isRightThresholdExceeded = horizontalInput >= m_ThresholdValue;
        
        // Detecteer wanneer de threshold voor het eerst wordt overschreden (edge detection)
        if (isRightThresholdExceeded && !m_WasRightThresholdExceeded)
        {
            if (Time.time - m_LastRedoTime >= m_CooldownTime)
            {
                PerformRedo();
                m_LastRedoTime = Time.time;
            }
        }
        
        m_WasRightThresholdExceeded = isRightThresholdExceeded;
    }
    
    private void PerformUndo()
    {
        try
        {
            // Gebruik de EGB Pro 2 GridManager om undo uit te voeren
            if (GridManager.Instance != null)
            {
                GridManager.Instance.GetGridCommandInvoker().UndoCommand();
                
                if (m_EnableDebugLogs)
                {
                    Debug.Log("VR Undo uitgevoerd!");
                }
            }
            else
            {
                Debug.LogWarning("GridManager.Instance is null - kan undo niet uitvoeren");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Fout bij uitvoeren van Undo: {e.Message}");
        }
    }
    
    private void PerformRedo()
    {
        try
        {
            // Gebruik de EGB Pro 2 GridManager om redo uit te voeren
            if (GridManager.Instance != null)
            {
                GridManager.Instance.GetGridCommandInvoker().RedoCommand();
                
                if (m_EnableDebugLogs)
                {
                    Debug.Log("VR Redo uitgevoerd!");
                }
            }
            else
            {
                Debug.LogWarning("GridManager.Instance is null - kan redo niet uitvoeren");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Fout bij uitvoeren van Redo: {e.Message}");
        }
    }
    
    // Context menu voor handmatige test
    [ContextMenu("Test Undo")]
    private void TestUndo()
    {
        PerformUndo();
    }
    
    [ContextMenu("Test Redo")]
    private void TestRedo()
    {
        PerformRedo();
    }
} 