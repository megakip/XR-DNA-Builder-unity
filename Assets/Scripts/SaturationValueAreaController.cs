using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SaturationValueAreaController : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("References")]
    public HueSliderController hueSliderController;
    public RectTransform areaRect;
    
    [Header("Visual Indicator")]
    public RectTransform selectionIndicator; // Optionele cirkel die de huidige selectie toont
    
    private Vector2 currentSelection = new Vector2(1f, 1f); // Default: volledige saturatie en value
    
    void Start()
    {
        if (areaRect == null)
        {
            areaRect = GetComponent<RectTransform>();
        }
        
        if (hueSliderController == null)
        {
            hueSliderController = FindObjectOfType<HueSliderController>();
        }
        
        // Luister naar hue veranderingen
        if (hueSliderController != null)
        {
            hueSliderController.OnColorChanged += OnColorChanged;
        }
        
        UpdateSelectionIndicator();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        HandleInput(eventData);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        HandleInput(eventData);
    }
    
    void HandleInput(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            areaRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            Rect rect = areaRect.rect;
            
            // Convert local point to normalized coordinates (0-1)
            float normalizedX = (localPoint.x - rect.x) / rect.width;
            float normalizedY = (localPoint.y - rect.y) / rect.height;
            
            // Clamp values
            normalizedX = Mathf.Clamp01(normalizedX);
            normalizedY = Mathf.Clamp01(normalizedY);
            
            currentSelection = new Vector2(normalizedX, normalizedY);
            
            // Send to hue slider controller
            if (hueSliderController != null)
            {
                hueSliderController.OnSatValueAreaClicked(currentSelection);
            }
            
            UpdateSelectionIndicator();
        }
    }
    
    void UpdateSelectionIndicator()
    {
        if (selectionIndicator != null && areaRect != null)
        {
            Rect rect = areaRect.rect;
            
            float posX = rect.x + (currentSelection.x * rect.width);
            float posY = rect.y + (currentSelection.y * rect.height);
            
            selectionIndicator.anchoredPosition = new Vector2(posX, posY);
        }
    }
    
    void OnColorChanged(Color newColor)
    {
        // Update selection indicator when color changes externally
        UpdateSelectionIndicator();
    }
    
    public void SetSelection(float saturation, float value)
    {
        currentSelection = new Vector2(saturation, value);
        UpdateSelectionIndicator();
    }
    
    void OnDestroy()
    {
        if (hueSliderController != null)
        {
            hueSliderController.OnColorChanged -= OnColorChanged;
        }
    }
}