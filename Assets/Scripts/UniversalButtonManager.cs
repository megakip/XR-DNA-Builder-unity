using UnityEngine;
using System.Collections.Generic;

public class UniversalButtonManager : MonoBehaviour
{
    [Header("Global Button Settings")]
    public Color globalButtonColor = Color.white;
    public Font globalFont;
    
    [Header("Button Collections")]
    public List<ButtonData> allButtonData = new List<ButtonData>();
    public List<UniversalButton> allButtons = new List<UniversalButton>();
    
    [Header("Auto-Find Buttons")]
    public bool autoFindButtonsOnStart = true;
    
    [Header("Button Spawning")]
    public GameObject buttonPrefab; // Je NavigateButton prefab
    public Transform buttonContainer; // Container waar buttons gespawnd worden
    public bool spawnButtonsOnStart = true;
    public bool clearContainerFirst = true;
    
    private void Start()
    {
        if (autoFindButtonsOnStart)
            FindAllButtons();
            
        if (spawnButtonsOnStart && buttonContainer != null)
            SpawnAllButtons();
            
        ApplyGlobalSettings();
    }
    
    public void FindAllButtons()
    {
        allButtons.Clear();
        UniversalButton[] foundButtons = FindObjectsOfType<UniversalButton>();
        allButtons.AddRange(foundButtons);
        
        Debug.Log($"Found {allButtons.Count} Universal Buttons in scene");
    }
    
    public void SpawnAllButtons()
    {
        if (buttonPrefab == null || buttonContainer == null)
        {
            Debug.LogWarning("Button prefab or container not assigned!");
            return;
        }
        
        // Clear existing buttons if needed
        if (clearContainerFirst)
        {
            foreach (Transform child in buttonContainer)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
        
        // Spawn new buttons from ButtonData
        foreach (ButtonData data in allButtonData)
        {
            if (data != null)
            {
                SpawnButton(data);
            }
        }
        
        Debug.Log($"Spawned {allButtonData.Count} buttons");
    }
    
    public UniversalButton SpawnButton(ButtonData data)
    {
        if (buttonPrefab == null || buttonContainer == null || data == null)
            return null;
            
        GameObject newButtonObj = Instantiate(buttonPrefab, buttonContainer);
        UniversalButton newButton = newButtonObj.GetComponent<UniversalButton>();
        
        if (newButton != null)
        {
            newButton.UpdateButtonData(data);
            allButtons.Add(newButton);
            
            // Apply global settings to new button
            if (newButton.buttonBackground != null)
                newButton.buttonBackground.color = globalButtonColor;
                
            return newButton;
        }
        
        Debug.LogWarning("Spawned button doesn't have UniversalButton component!");
        return null;
    }
    
    public void ApplyGlobalSettings()
    {
        foreach (UniversalButton button in allButtons)
        {
            if (button != null && button.buttonBackground != null)
                button.buttonBackground.color = globalButtonColor;
                
            // Hier kun je meer globale instellingen toevoegen
        }
    }
    
    public void RefreshAllButtons()
    {
        foreach (UniversalButton button in allButtons)
        {
            if (button != null)
                button.ApplyButtonData();
        }
    }
    
    // Voor runtime wijzigingen
    public void UpdateGlobalButtonColor(Color newColor)
    {
        globalButtonColor = newColor;
        ApplyGlobalSettings();
    }
    
    // Editor methods voor testing
    [ContextMenu("Spawn Buttons Now")]
    public void SpawnButtonsNow()
    {
        SpawnAllButtons();
    }
    
    [ContextMenu("Clear All Buttons")]
    public void ClearAllButtons()
    {
        if (buttonContainer != null)
        {
            foreach (Transform child in buttonContainer)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
        allButtons.Clear();
    }
}
