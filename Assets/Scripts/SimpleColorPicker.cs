using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class SimpleColorPicker : MonoBehaviour
{
    [Header("UI Elements")]
    public Image colorPreview;
    public Slider hueSlider;
    public RawImage saturationValueArea;  // Nieuw: S/V area
    public Button eyeDropperButton;
    
    [Header("Current Values")]
    [Range(0f, 1f)] public float hue = 0f;
    [Range(0f, 1f)] public float saturation = 1f;  // Nu aanpasbaar
    [Range(0f, 1f)] public float value = 1f;       // Nu aanpasbaar
    [Range(0f, 1f)] public float alpha = 1f;
    
    // Private textures
    private Texture2D svTexture;
    
    // Event voor color changes
    public static event Action<Color> OnColorChanged;
    
    private Color currentColor;
    
    void Start()
    {
        Debug.Log("Enhanced SimpleColorPicker started");
        InitializeColorPicker();
    }
    
    void InitializeColorPicker()
    {
        if (hueSlider != null)
        {
            hueSlider.onValueChanged.AddListener(OnHueChanged);
        }
        
        SetupSVArea();
        UpdateColor();
    }
    
    void SetupSVArea()
    {
        if (saturationValueArea != null)
        {
            // Maak S/V texture
            CreateSVTexture();
            saturationValueArea.texture = svTexture;
            
            // Add event trigger voor clicking/dragging
            EventTrigger trigger = saturationValueArea.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = saturationValueArea.gameObject.AddComponent<EventTrigger>();
            
            // Pointer down
            EventTrigger.Entry pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((data) => { OnSVAreaClick((PointerEventData)data); });
            trigger.triggers.Add(pointerDown);
            
            // Drag
            EventTrigger.Entry drag = new EventTrigger.Entry();
            drag.eventID = EventTriggerType.Drag;
            drag.callback.AddListener((data) => { OnSVAreaClick((PointerEventData)data); });
            trigger.triggers.Add(drag);
        }
    }
    
    void CreateSVTexture()
    {
        if (svTexture == null)
        {
            int size = 256;
            svTexture = new Texture2D(size, size);
        }
        
        int size2 = svTexture.width;
        for (int x = 0; x < size2; x++)
        {
            for (int y = 0; y < size2; y++)
            {
                float s = x / (float)(size2 - 1);
                float v = y / (float)(size2 - 1);
                Color color = Color.HSVToRGB(hue, s, v);
                svTexture.SetPixel(x, y, color);
            }
        }
        svTexture.Apply();
    }
    
    public void OnHueChanged(float newHue)
    {
        hue = newHue;
        UpdateSVTexture();
        UpdateColor();
    }
    
    void OnSVAreaClick(PointerEventData eventData)
    {
        RectTransform rectTransform = saturationValueArea.GetComponent<RectTransform>();
        Vector2 localPoint;
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            Rect rect = rectTransform.rect;
            float normalizedX = (localPoint.x - rect.x) / rect.width;
            float normalizedY = (localPoint.y - rect.y) / rect.height;
            
            saturation = Mathf.Clamp01(normalizedX);
            value = Mathf.Clamp01(normalizedY);
            
            UpdateColor();
        }
    }
    
    void UpdateSVTexture()
    {
        if (svTexture != null)
        {
            CreateSVTexture();
        }
    }
    
    void UpdateColor()
    {
        currentColor = Color.HSVToRGB(hue, saturation, value);
        currentColor.a = alpha;
        
        if (colorPreview != null)
            colorPreview.color = currentColor;
        
        // Trigger event
        OnColorChanged?.Invoke(currentColor);
        
        Debug.Log($"Color updated: H={hue:F2}, S={saturation:F2}, V={value:F2}, Color={currentColor}");
    }
    
    public void SetColor(Color color)
    {
        Color.RGBToHSV(color, out hue, out saturation, out value);
        alpha = color.a;
        
        if (hueSlider != null)
            hueSlider.value = hue;
        
        UpdateSVTexture();
        UpdateColor();
    }
    
    public Color GetCurrentColor()
    {
        return currentColor;
    }
    
    public void OnEyeDropperClicked()
    {
        Debug.Log("Eyedropper activated");
    }
    
    void OnDestroy()
    {
        if (svTexture != null)
            DestroyImmediate(svTexture);
    }
}
