using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Vergroot UI-knoppen wanneer de XR-raycast er overheen hovert.
/// Voeg dit script toe aan een Canvas om het op alle knoppen toe te passen.
/// </summary>
public class ButtonHoverEffect : MonoBehaviour
{
    [Tooltip("Hoeveel de knop moet vergroten bij hover (1 = normale grootte)")]
    [SerializeField] private float hoverScaleFactor = 1.2f;
    
    [Tooltip("Hoe snel de knop moet vergroten/verkleinen")]
    [SerializeField] private float scaleSpeed = 5f;
    
    [Tooltip("Of dit script alle knoppen op het canvas moet beïnvloeden")]
    [SerializeField] private bool applyToAllButtons = true;
    
    [Tooltip("Debug modus om meer informatie te tonen in de console")]
    [SerializeField] private bool debugMode = true;
    
    private Button[] buttons;
    private Vector3[] originalScales;
    private bool[] isHovering;
    
    private void Start()
    {
        // Wacht een frame om er zeker van te zijn dat alle UI-elementen zijn geïnitialiseerd
        StartCoroutine(InitializeWithDelay());
    }
    
    private IEnumerator InitializeWithDelay()
    {
        // Wacht een frame
        yield return null;
        
        if (debugMode)
        {
            Debug.Log($"ButtonHoverEffect: Initialiseren op {gameObject.name}");
        }
        
        // Vind alle knoppen
        FindButtons();
        
        // Probeer beide setup methoden
        SetupXREvents();
        SetupStandardEvents();
        
        if (debugMode && buttons != null)
        {
            Debug.Log($"ButtonHoverEffect: {buttons.Length} knoppen gevonden en geïnitialiseerd");
        }
    }
    
    private void FindButtons()
    {
        if (applyToAllButtons && GetComponent<Canvas>() != null)
        {
            // Vind alle knoppen op het canvas
            buttons = GetComponentsInChildren<Button>(true);
            
            if (debugMode)
            {
                Debug.Log($"ButtonHoverEffect: {buttons.Length} knoppen gevonden op canvas {gameObject.name}");
                foreach (var button in buttons)
                {
                    Debug.Log($"  - Knop gevonden: {button.gameObject.name}");
                }
            }
        }
        else
        {
            // Alleen de knop op dit GameObject gebruiken
            Button button = GetComponent<Button>();
            if (button != null)
            {
                buttons = new Button[] { button };
                if (debugMode)
                {
                    Debug.Log($"ButtonHoverEffect: Enkele knop gevonden op {gameObject.name}");
                }
            }
            else if (debugMode)
            {
                Debug.LogWarning($"ButtonHoverEffect: Geen knop gevonden op {gameObject.name}");
            }
        }
        
        if (buttons != null && buttons.Length > 0)
        {
            // Sla de originele schaal van elke knop op
            originalScales = new Vector3[buttons.Length];
            isHovering = new bool[buttons.Length];
            
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null)
                {
                    originalScales[i] = buttons[i].transform.localScale;
                    isHovering[i] = false;
                }
            }
        }
        else if (debugMode)
        {
            Debug.LogWarning("ButtonHoverEffect: Geen knoppen gevonden om effect op toe te passen");
        }
    }
    
    private void SetupXREvents()
    {
        if (buttons == null || buttons.Length == 0) return;
        
        try
        {
            // Probeer XR events toe te voegen
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null) continue;
                
                // Probeer XRSimpleInteractable toe te voegen en events te registreren
                // We gebruiken volledige namespace paden om problemen te voorkomen
                var interactableType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, Unity.XR.Interaction.Toolkit");
                var raycaster = FindFirstObjectByType(interactableType);
                
                if (raycaster != null)
                {
                    // Voeg EventTrigger toe als die nog niet bestaat
                    EventTrigger trigger = buttons[i].gameObject.GetComponent<EventTrigger>();
                    if (trigger == null)
                    {
                        trigger = buttons[i].gameObject.AddComponent<EventTrigger>();
                    }
                    
                    int buttonIndex = i;
                    
                    // Voeg PointerEnter event toe
                    EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                    enterEntry.eventID = EventTriggerType.PointerEnter;
                    enterEntry.callback.AddListener((data) => { OnButtonHoverEnter(buttonIndex); });
                    trigger.triggers.Add(enterEntry);
                    
                    // Voeg PointerExit event toe
                    EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                    exitEntry.eventID = EventTriggerType.PointerExit;
                    exitEntry.callback.AddListener((data) => { OnButtonHoverExit(buttonIndex); });
                    trigger.triggers.Add(exitEntry);
                    
                    if (debugMode)
                    {
                        Debug.Log($"ButtonHoverEffect: XR events toegevoegd aan knop {buttons[i].gameObject.name}");
                    }
                }
                else if (debugMode)
                {
                    Debug.LogWarning("ButtonHoverEffect: Geen XR Raycaster gevonden in de scene. Standaard events worden gebruikt.");
                }
            }
        }
        catch (System.Exception e)
        {
            if (debugMode)
            {
                Debug.LogError($"ButtonHoverEffect: Fout bij het instellen van XR events: {e.Message}");
            }
        }
    }
    
    private void SetupStandardEvents()
    {
        if (buttons == null) return;
        
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            
            int buttonIndex = i;
            EventTrigger trigger = buttons[i].gameObject.GetComponent<EventTrigger>();
            
            if (trigger == null)
            {
                trigger = buttons[i].gameObject.AddComponent<EventTrigger>();
            }
            
            // Controleer of de events al bestaan om duplicaten te voorkomen
            bool hasEnterEvent = false;
            bool hasExitEvent = false;
            
            foreach (var entry in trigger.triggers)
            {
                if (entry.eventID == EventTriggerType.PointerEnter)
                {
                    hasEnterEvent = true;
                }
                else if (entry.eventID == EventTriggerType.PointerExit)
                {
                    hasExitEvent = true;
                }
            }
            
            // Voeg PointerEnter event toe als het nog niet bestaat
            if (!hasEnterEvent)
            {
                EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                enterEntry.eventID = EventTriggerType.PointerEnter;
                enterEntry.callback.AddListener((data) => { OnButtonHoverEnter(buttonIndex); });
                trigger.triggers.Add(enterEntry);
            }
            
            // Voeg PointerExit event toe als het nog niet bestaat
            if (!hasExitEvent)
            {
                EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                exitEntry.eventID = EventTriggerType.PointerExit;
                exitEntry.callback.AddListener((data) => { OnButtonHoverExit(buttonIndex); });
                trigger.triggers.Add(exitEntry);
            }
            
            if (debugMode)
            {
                Debug.Log($"ButtonHoverEffect: Standaard UI events toegevoegd aan knop {buttons[i].gameObject.name}");
            }
        }
    }
    
    private void Update()
    {
        if (buttons == null) return;
        
        // Update de schaal van elke knop gebaseerd op hover status
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            
            Vector3 targetScale = isHovering[i] ? 
                originalScales[i] * hoverScaleFactor : 
                originalScales[i];
            
            // Geleidelijk schalen voor een vloeiend effect
            buttons[i].transform.localScale = Vector3.Lerp(
                buttons[i].transform.localScale, 
                targetScale, 
                Time.deltaTime * scaleSpeed
            );
        }
    }
    
    private void OnButtonHoverEnter(int buttonIndex)
    {
        if (buttonIndex >= 0 && buttonIndex < isHovering.Length)
        {
            isHovering[buttonIndex] = true;
            
            if (debugMode)
            {
                Debug.Log($"ButtonHoverEffect: Hover gestart op knop {buttons[buttonIndex].gameObject.name}");
            }
        }
    }
    
    private void OnButtonHoverExit(int buttonIndex)
    {
        if (buttonIndex >= 0 && buttonIndex < isHovering.Length)
        {
            isHovering[buttonIndex] = false;
            
            if (debugMode)
            {
                Debug.Log($"ButtonHoverEffect: Hover beëindigd op knop {buttons[buttonIndex].gameObject.name}");
            }
        }
    }
    
    // Methode om handmatig de hover status van een knop te testen (kan worden aangeroepen vanuit de Editor)
    public void TestHoverEffect(int buttonIndex, bool hover)
    {
        if (buttonIndex >= 0 && buttonIndex < buttons.Length)
        {
            isHovering[buttonIndex] = hover;
            Debug.Log($"ButtonHoverEffect: Test hover {(hover ? "gestart" : "beëindigd")} op knop {buttons[buttonIndex].gameObject.name}");
        }
    }
} 