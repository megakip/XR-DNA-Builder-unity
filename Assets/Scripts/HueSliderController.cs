using UnityEngine;
using UnityEngine.UI;

public class HueSliderController : MonoBehaviour
{
    [Header("UI References")]
    public Slider hueSlider;
    public RawImage saturationValueArea;
    public MaskableGraphic colorPreview; // Gewijzigd naar MaskableGraphic zodat zowel Image als RawImage werkt
    public Text hexColorText;
    
    [Header("Hue Gradient")]
    public RawImage hueBackground; // Sleep de Background van je HueSlider hier naartoe
    
    private Texture2D hueTexture;
    private Texture2D satValueTexture;
    private float currentHue = 0f;
    private float currentSaturation = 1f;
    private float currentValue = 1f;
    private Color currentColor;
    
    void Start()
    {
        // Controleer of alle references zijn toegewezen
        if (hueSlider == null)
        {
            hueSlider = GetComponent<Slider>();
        }
        
        if (hueSlider != null)
        {
            hueSlider.onValueChanged.AddListener(OnHueSliderChanged);
            hueSlider.minValue = 0f;
            hueSlider.maxValue = 1f;
            hueSlider.value = currentHue;
        }
        
        CreateHueGradientTexture();
        CreateSaturationValueTexture();
        UpdateColor();
    }
    
    void CreateHueGradientTexture()
    {
        int width = 360;
        int height = 20;
        hueTexture = new Texture2D(width, height);
        
        // Maak horizontale hue gradient
        for (int x = 0; x < width; x++)
        {
            float hue = (float)x / width;
            Color hueColor = Color.HSVToRGB(hue, 1f, 1f);
            
            for (int y = 0; y < height; y++)
            {
                hueTexture.SetPixel(x, y, hueColor);
            }
        }
        
        hueTexture.Apply();
        
        // Pas de texture toe op de hue background
        if (hueBackground != null)
        {
            hueBackground.texture = hueTexture;
        }
        else
        {
            Debug.LogWarning("HueBackground RawImage is niet toegewezen!");
        }
    }
    
    void CreateSaturationValueTexture()
    {
        if (saturationValueArea == null) return;
        
        int width = 256;
        int height = 256;
        satValueTexture = new Texture2D(width, height);
        
        UpdateSaturationValueTexture();
        saturationValueArea.texture = satValueTexture;
    }
    
    void UpdateSaturationValueTexture()
    {
        if (satValueTexture == null) return;
        
        int width = satValueTexture.width;
        int height = satValueTexture.height;
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float saturation = (float)x / width;
                float value = (float)y / height;
                Color color = Color.HSVToRGB(currentHue, saturation, value);
                satValueTexture.SetPixel(x, y, color);
            }
        }
        
        satValueTexture.Apply();
    }
    
    public void OnHueSliderChanged(float value)
    {
        currentHue = value;
        UpdateSaturationValueTexture();
        UpdateColor();
        
        Debug.Log($"Hue changed to: {currentHue:F2}");
    }
    
    void UpdateColor()
    {
        currentColor = Color.HSVToRGB(currentHue, currentSaturation, currentValue);
        
        // Update color preview
        if (colorPreview != null)
        {
            colorPreview.color = currentColor;
        }
        
        // Update hex text
        if (hexColorText != null)
        {
            string hex = ColorUtility.ToHtmlStringRGB(currentColor);
            hexColorText.text = "#" + hex;
        }
        
        // Broadcast color change event
        OnColorChanged?.Invoke(currentColor);
    }
    
    // Event for other scripts to listen to
    public System.Action<Color> OnColorChanged;
    
    // Method to handle saturation/value area clicks
    public void OnSatValueAreaClicked(Vector2 normalizedPosition)
    {
        currentSaturation = Mathf.Clamp01(normalizedPosition.x);
        currentValue = Mathf.Clamp01(normalizedPosition.y);
        UpdateColor();
    }
    
    // Public methods for external control
    public void SetHue(float hue)
    {
        currentHue = Mathf.Clamp01(hue);
        if (hueSlider != null)
        {
            hueSlider.value = currentHue;
        }
        UpdateSaturationValueTexture();
        UpdateColor();
    }
    
    public void SetColor(Color color)
    {
        Color.RGBToHSV(color, out currentHue, out currentSaturation, out currentValue);
        SetHue(currentHue);
    }
    
    public Color GetCurrentColor()
    {
        return currentColor;
    }
    
    void OnDestroy()
    {
        if (hueTexture != null)
        {
            DestroyImmediate(hueTexture);
        }
        if (satValueTexture != null)
        {
            DestroyImmediate(satValueTexture);
        }
    }
}