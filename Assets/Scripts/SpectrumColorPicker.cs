using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class SpectrumColorPicker : MonoBehaviour
{
    [Header("UI Elements")]
    public Image colorPreview;
    public RawImage saturationValueArea;  // Groot kleurvierkant bovenaan
    public RawImage hueStrip;            // Dunne kleurenbalk onderaan
    public Button eyeDropperButton;
    
    [Header("Selection Indicators")]
    public GameObject svIndicator;       // Cirkel voor saturation/value area
    public GameObject hueIndicator;      // Cirkel voor hue strip
    
    [Header("Settings")]
    public int svTextureSize = 256;      // Grootte van S/V texture
    public int hueStripWidth = 360;      // Breedte van hue strip
    public int hueStripHeight = 30;      // Hoogte van hue strip
    
    [Header("Current Values")]
    [Range(0f, 1f)] public float hue = 0f;
    [Range(0f, 1f)] public float saturation = 1f;
    [Range(0f, 1f)] public float value = 1f;
    [Range(0f, 1f)] public float alpha = 1f;
    
    // Private fields
    private Texture2D svTexture;
    private Texture2D hueTexture;
    private Color currentColor;
    private RectTransform svRectTransform;
    private RectTransform hueRectTransform;
    
    // Events
    public static event Action<Color> OnColorChanged;
    
    void Start()
    {
        Debug.Log("SpectrumColorPicker started");
        InitializeColorPicker();
    }
    
    void InitializeColorPicker()
    {
        CreateTextures();
        SetupSVArea();
        SetupHueStrip();
        UpdateColor();
        UpdateIndicators();
    }
    
    void CreateTextures()
    {
        CreateSVTexture();
        CreateHueTexture();
    }
    
    void CreateSVTexture()
    {
        if (svTexture == null)
        {
            svTexture = new Texture2D(svTextureSize, svTextureSize);
            svTexture.filterMode = FilterMode.Bilinear;
        }
        
        // Generate saturation/value square
        // X-axis = Saturation (0 = left/gray, 1 = right/saturated)
        // Y-axis = Value (0 = bottom/black, 1 = top/bright)
        
        for (int x = 0; x < svTextureSize; x++)
        {
            for (int y = 0; y < svTextureSize; y++)
            {
                float s = (float)x / (svTextureSize - 1); // Saturation: 0-1
                float v = (float)y / (svTextureSize - 1); // Value: 0-1
                
                Color pixelColor = Color.HSVToRGB(hue, s, v);
                svTexture.SetPixel(x, y, pixelColor);
            }
        }
        
        svTexture.Apply();
        
        // Apply texture to UI
        if (saturationValueArea != null)
        {
            saturationValueArea.texture = svTexture;
        }
    }
    
    void CreateHueTexture()
    {
        if (hueTexture == null)
        {
            hueTexture = new Texture2D(hueStripWidth, hueStripHeight);
            hueTexture.filterMode = FilterMode.Bilinear;
        }
        
        // Generate horizontal hue strip (rainbow)
        for (int x = 0; x < hueStripWidth; x++)
        {
            float h = (float)x / (hueStripWidth - 1); // Hue: 0-1 (covers full spectrum)
            Color hueColor = Color.HSVToRGB(h, 1f, 1f); // Full saturation and brightness
            
            // Fill entire height with same color
            for (int y = 0; y < hueStripHeight; y++)
            {
                hueTexture.SetPixel(x, y, hueColor);
            }
        }
        
        hueTexture.Apply();
        
        // Apply texture to UI
        if (hueStrip != null)
        {
            hueStrip.texture = hueTexture;
        }
    }
    
    void SetupSVArea()
    {
        if (saturationValueArea == null) return;
        
        svRectTransform = saturationValueArea.GetComponent<RectTransform>();
        
        // Add event trigger for clicking/dragging
        EventTrigger trigger = saturationValueArea.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = saturationValueArea.gameObject.AddComponent<EventTrigger>();
        
        // Clear existing triggers
        trigger.triggers.Clear();
        
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
    
    void SetupHueStrip()
    {
        if (hueStrip == null) return;
        
        hueRectTransform = hueStrip.GetComponent<RectTransform>();
        
        // Add event trigger for clicking/dragging
        EventTrigger trigger = hueStrip.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = hueStrip.gameObject.AddComponent<EventTrigger>();
        
        // Clear existing triggers
        trigger.triggers.Clear();
        
        // Pointer down
        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { OnHueStripClick((PointerEventData)data); });
        trigger.triggers.Add(pointerDown);
        
        // Drag
        EventTrigger.Entry drag = new EventTrigger.Entry();
        drag.eventID = EventTriggerType.Drag;
        drag.callback.AddListener((data) => { OnHueStripClick((PointerEventData)data); });
        trigger.triggers.Add(drag);
    }
    
    void OnSVAreaClick(PointerEventData eventData)
    {
        if (svRectTransform == null) return;
        
        Vector2 localPoint;
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            svRectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            Rect rect = svRectTransform.rect;
            
            // Convert local point to normalized coordinates (0-1)
            float normalizedX = (localPoint.x - rect.x) / rect.width;
            float normalizedY = (localPoint.y - rect.y) / rect.height;
            
            // Clamp to valid range
            normalizedX = Mathf.Clamp01(normalizedX);
            normalizedY = Mathf.Clamp01(normalizedY);
            
            // Update saturation and value
            saturation = normalizedX;  // X-axis = Saturation
            value = normalizedY;       // Y-axis = Value/Brightness
            
            UpdateColor();
            UpdateIndicators();
            
            Debug.Log($"S/V Area clicked: H={hue:F2}, S={saturation:F2}, V={value:F2}");
        }
    }
    
    void OnHueStripClick(PointerEventData eventData)
    {
        if (hueRectTransform == null) return;
        
        Vector2 localPoint;
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            hueRectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            Rect rect = hueRectTransform.rect;
            
            // Convert local point to normalized X coordinate (0-1)
            float normalizedX = (localPoint.x - rect.x) / rect.width;
            normalizedX = Mathf.Clamp01(normalizedX);
            
            // Update hue
            hue = normalizedX;
            
            // Regenerate S/V texture with new hue
            CreateSVTexture();
            
            UpdateColor();
            UpdateIndicators();
            
            Debug.Log($"Hue Strip clicked: H={hue:F2}, S={saturation:F2}, V={value:F2}");
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
        
        Debug.Log($"Color updated: H={hue:F2}, S={saturation:F2}, V={value:F2}, RGB={currentColor}");
    }
    
    void UpdateIndicators()
    {
        UpdateSVIndicator();
        UpdateHueIndicator();
    }
    
    void UpdateSVIndicator()
    {
        if (svIndicator == null || svRectTransform == null) return;
        
        // Position indicator at current saturation/value position
        Rect rect = svRectTransform.rect;
        Vector2 indicatorPos = new Vector2(
            rect.x + (saturation * rect.width),
            rect.y + (value * rect.height)
        );
        
        RectTransform indicatorRect = svIndicator.GetComponent<RectTransform>();
        if (indicatorRect != null)
        {
            indicatorRect.anchoredPosition = indicatorPos;
            svIndicator.SetActive(true);
        }
    }
    
    void UpdateHueIndicator()
    {
        if (hueIndicator == null || hueRectTransform == null) return;
        
        // Position indicator at current hue position
        Rect rect = hueRectTransform.rect;
        Vector2 indicatorPos = new Vector2(
            rect.x + (hue * rect.width),
            rect.y + (rect.height * 0.5f) // Center vertically
        );
        
        RectTransform indicatorRect = hueIndicator.GetComponent<RectTransform>();
        if (indicatorRect != null)
        {
            indicatorRect.anchoredPosition = indicatorPos;
            hueIndicator.SetActive(true);
        }
    }
    
    public void SetColor(Color color)
    {
        Color.RGBToHSV(color, out hue, out saturation, out value);
        alpha = color.a;
        
        CreateSVTexture(); // Regenerate with new hue
        UpdateColor();
        UpdateIndicators();
        
        Debug.Log($"Color set to: {color}");
    }
    
    public Color GetCurrentColor()
    {
        return currentColor;
    }
    
    // Quick color presets
    public void SetColorToRed() { SetColor(Color.red); }
    public void SetColorToGreen() { SetColor(Color.green); }
    public void SetColorToBlue() { SetColor(Color.blue); }
    public void SetColorToYellow() { SetColor(Color.yellow); }
    public void SetColorToCyan() { SetColor(Color.cyan); }
    public void SetColorToMagenta() { SetColor(Color.magenta); }
    public void SetColorToWhite() { SetColor(Color.white); }
    public void SetColorToBlack() { SetColor(Color.black); }
    
    public void OnEyeDropperClicked()
    {
        Debug.Log("Eyedropper activated - Feature coming soon!");
    }
    
    void OnDestroy()
    {
        if (svTexture != null)
            DestroyImmediate(svTexture);
        if (hueTexture != null)
            DestroyImmediate(hueTexture);
    }
}
