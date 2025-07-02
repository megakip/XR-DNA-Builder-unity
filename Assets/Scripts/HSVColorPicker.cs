using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class HSVColorPicker : MonoBehaviour
{
    [Header("UI Elements")]
    public Image colorPreview;
    public RawImage saturationValueArea;
    public Slider hueSlider;
    public Button eyeDropperButton;
    
    [Header("Current Values")]
    [Range(0f, 1f)] public float hue = 0f;
    [Range(0f, 1f)] public float saturation = 1f;
    [Range(0f, 1f)] public float value = 1f;
    [Range(0f, 1f)] public float alpha = 1f;
    
    // Private textures - no Header needed
    private Texture2D hueTexture;
    private Texture2D svTexture;
    
    // Static event - no Header needed
    public static event Action<Color> OnColorChanged;
    
    private Color currentColor;
    private bool isInitialized = false;
    
    void Start()
    {
        InitializeColorPicker();
    }
    
    void InitializeColorPicker()
    {
        CreateHueTexture();
        CreateSVTexture();
        SetupSlider();
        SetupSVArea();
        UpdateColor();
        isInitialized = true;
    }
    
    void CreateHueTexture()
    {
        if (hueTexture == null)
        {
            hueTexture = new Texture2D(1, 360);
            for (int i = 0; i < 360; i++)
            {
                Color hueColor = Color.HSVToRGB(i / 360f, 1f, 1f);
                hueTexture.SetPixel(0, i, hueColor);
            }
            hueTexture.Apply();
        }
    }
    
    void CreateSVTexture()
    {
        if (svTexture == null)
        {
            int size = 256;
            svTexture = new Texture2D(size, size);
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float s = x / (float)(size - 1);
                    float v = y / (float)(size - 1);
                    Color color = Color.HSVToRGB(hue, s, v);
                    svTexture.SetPixel(x, y, color);
                }
            }
            svTexture.Apply();
        }
    }
    
    void SetupSlider()
    {
        if (hueSlider != null)
        {
            // Set background to hue texture
            Image sliderBackground = hueSlider.GetComponentInChildren<Image>();
            if (sliderBackground != null)
            {
                sliderBackground.sprite = Sprite.Create(hueTexture, 
                    new Rect(0, 0, hueTexture.width, hueTexture.height), 
                    new Vector2(0.5f, 0.5f));
            }
            
            hueSlider.value = hue;
            hueSlider.onValueChanged.AddListener(OnHueChanged);
        }
    }
    
    void SetupSVArea()
    {
        if (saturationValueArea != null)
        {
            saturationValueArea.texture = svTexture;
            
            // Add event trigger for clicking/dragging
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
            int size = svTexture.width;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float s = x / (float)(size - 1);
                    float v = y / (float)(size - 1);
                    Color color = Color.HSVToRGB(hue, s, v);
                    svTexture.SetPixel(x, y, color);
                }
            }
            svTexture.Apply();
        }
    }
    
    void UpdateColor()
    {
        currentColor = Color.HSVToRGB(hue, saturation, value);
        currentColor.a = alpha;
        
        if (colorPreview != null)
            colorPreview.color = currentColor;
        
        OnColorChanged?.Invoke(currentColor);
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
        // Activeer eyedropper mode
        Debug.Log("Eyedropper activated");
        // Hier kun je integratie met EGB Pro toevoegen
    }
    
    void OnDestroy()
    {
        if (hueTexture != null)
            DestroyImmediate(hueTexture);
        if (svTexture != null)
            DestroyImmediate(svTexture);
    }
}
