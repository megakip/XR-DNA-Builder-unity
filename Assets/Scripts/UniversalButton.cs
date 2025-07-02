using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class UniversalButton : MonoBehaviour
{
    [Header("Button Data")]
    public ButtonData buttonData;
    
    [Header("UI References")]
    public Button button;
    public TextMeshProUGUI buttonText;
    public Image buttonIcon;
    public Image buttonBackground;
    
    [Header("Events")]
    public UnityEvent OnButtonClicked;
    
    private void Start()
    {
        if (buttonData != null)
            ApplyButtonData();
            
        if (button != null)
            button.onClick.AddListener(HandleButtonClick);
    }
    
    public void ApplyButtonData()
    {
        if (buttonData == null) return;
        
        // Tekst instellen
        if (buttonText != null)
            buttonText.text = buttonData.buttonText;
            
        // Icoon instellen
        if (buttonIcon != null && buttonData.buttonIcon != null)
        {
            buttonIcon.sprite = buttonData.buttonIcon;
            buttonIcon.gameObject.SetActive(true);
        }
        else if (buttonIcon != null)
        {
            buttonIcon.gameObject.SetActive(false);
        }
        
        // Kleur instellen
        if (buttonBackground != null)
            buttonBackground.color = buttonData.buttonColor;
            
        // Interactie instellen
        if (button != null)
            button.interactable = buttonData.interactable;
    }
    
    private void HandleButtonClick()
    {
        // Simpele button click event voor custom functionaliteit
        OnButtonClicked?.Invoke();
    }
    
    // Deze methode kun je aanroepen om de button data runtime te wijzigen
    public void UpdateButtonData(ButtonData newData)
    {
        buttonData = newData;
        ApplyButtonData();
    }
}
