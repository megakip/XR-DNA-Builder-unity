using UnityEngine;

[CreateAssetMenu(fileName = "New Button Data", menuName = "UI/Button Data")]
public class ButtonData : ScriptableObject
{
    [Header("Button Content")]
    public string buttonText = "Button";
    public Sprite buttonIcon;
    public Color buttonColor = Color.white;
    
    [Header("Custom Settings")]
    public bool interactable = true;
    public string tooltipText = "";
}
