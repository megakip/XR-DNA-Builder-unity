using UnityEngine;
using UnityEngine.UI;

public class EyeDropperButtonHandler : MonoBehaviour
{
    private HSVColorPicker colorPicker;
    private Button button;
    
    void Start()
    {
        button = GetComponent<Button>();
        colorPicker = GetComponentInParent<HSVColorPicker>();
        
        if (button != null && colorPicker != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }
    
    void OnButtonClick()
    {
        if (colorPicker != null)
        {
            colorPicker.OnEyeDropperClicked();
        }
    }
    
    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }
}
