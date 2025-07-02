using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the opacity of a background image for the grid system
/// Attach this script to a GameObject and configure the image and slider references
/// </summary>
public class GridBackgroundOpacityController : MonoBehaviour
{
    [Header("Background Image Settings")]
    [Tooltip("The Image or RawImage component that serves as the grid background")]
    public Image backgroundImage;
    
    [Tooltip("Alternative: RawImage component if using RawImage instead of Image")]
    public RawImage backgroundRawImage;
    
    [Header("Opacity Control")]
    [Tooltip("The slider that controls the opacity (0 = transparent, 1 = opaque)")]
    public Slider opacitySlider;
    
    [Range(0f, 1f)]
    [Tooltip("Default opacity value")]
    public float defaultOpacity = 0.5f;
    
    [Header("Debug")]
    [Tooltip("Enable logging of opacity changes")]
    public bool enableDebugLogging = false;

    private void Start()
    {
        // Initialize the system
        InitializeOpacityControl();
    }

    /// <summary>
    /// Initialize the opacity control system
    /// </summary>
    private void InitializeOpacityControl()
    {
        // Set up the slider if available
        if (opacitySlider != null)
        {
            // Set slider range
            opacitySlider.minValue = 0f;
            opacitySlider.maxValue = 1f;
            opacitySlider.value = defaultOpacity;
            
            // Add listener for slider changes
            opacitySlider.onValueChanged.AddListener(OnOpacitySliderChanged);
            
            if (enableDebugLogging)
                Debug.Log("GridBackgroundOpacityController: Slider initialized with value " + defaultOpacity);
        }
        else
        {
            Debug.LogWarning("GridBackgroundOpacityController: No opacity slider assigned!");
        }

        // Apply initial opacity
        SetOpacity(defaultOpacity);
    }

    /// <summary>
    /// Called when the opacity slider value changes
    /// </summary>
    /// <param name="value">New opacity value from slider (0-1)</param>
    private void OnOpacitySliderChanged(float value)
    {
        SetOpacity(value);
    }

    /// <summary>
    /// Sets the opacity of the background image
    /// </summary>
    /// <param name="opacity">Opacity value between 0 (transparent) and 1 (opaque)</param>
    public void SetOpacity(float opacity)
    {
        // Clamp the value to ensure it's between 0 and 1
        opacity = Mathf.Clamp01(opacity);

        // Apply opacity to Image component if available
        if (backgroundImage != null)
        {
            Color imageColor = backgroundImage.color;
            imageColor.a = opacity;
            backgroundImage.color = imageColor;
            
            if (enableDebugLogging)
                Debug.Log($"GridBackgroundOpacityController: Image opacity set to {opacity}");
        }

        // Apply opacity to RawImage component if available
        if (backgroundRawImage != null)
        {
            Color rawImageColor = backgroundRawImage.color;
            rawImageColor.a = opacity;
            backgroundRawImage.color = rawImageColor;
            
            if (enableDebugLogging)
                Debug.Log($"GridBackgroundOpacityController: RawImage opacity set to {opacity}");
        }

        // If neither image component is assigned, show warning
        if (backgroundImage == null && backgroundRawImage == null)
        {
            Debug.LogWarning("GridBackgroundOpacityController: No background image component assigned!");
        }
    }

    /// <summary>
    /// Sets the opacity using a percentage (0-100)
    /// </summary>
    /// <param name="percentage">Opacity percentage (0-100)</param>
    public void SetOpacityByPercentage(float percentage)
    {
        SetOpacity(percentage / 100f);
    }

    /// <summary>
    /// Gets the current opacity value
    /// </summary>
    /// <returns>Current opacity value (0-1)</returns>
    public float GetOpacity()
    {
        if (backgroundImage != null)
            return backgroundImage.color.a;
        
        if (backgroundRawImage != null)
            return backgroundRawImage.color.a;
            
        return 0f;
    }

    /// <summary>
    /// Resets opacity to the default value
    /// </summary>
    public void ResetToDefault()
    {
        SetOpacity(defaultOpacity);
        
        if (opacitySlider != null)
            opacitySlider.value = defaultOpacity;
    }

    /// <summary>
    /// Fade the background to a target opacity over time
    /// </summary>
    /// <param name="targetOpacity">Target opacity (0-1)</param>
    /// <param name="duration">Fade duration in seconds</param>
    public void FadeToOpacity(float targetOpacity, float duration = 1f)
    {
        StartCoroutine(FadeCoroutine(targetOpacity, duration));
    }

    /// <summary>
    /// Coroutine for smooth opacity fading
    /// </summary>
    private System.Collections.IEnumerator FadeCoroutine(float targetOpacity, float duration)
    {
        float startOpacity = GetOpacity();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float currentOpacity = Mathf.Lerp(startOpacity, targetOpacity, elapsed / duration);
            SetOpacity(currentOpacity);
            
            // Update slider if available
            if (opacitySlider != null)
                opacitySlider.value = currentOpacity;
                
            yield return null;
        }

        // Ensure we reach the exact target
        SetOpacity(targetOpacity);
        if (opacitySlider != null)
            opacitySlider.value = targetOpacity;
    }

    private void OnDestroy()
    {
        // Clean up slider listener
        if (opacitySlider != null)
        {
            opacitySlider.onValueChanged.RemoveListener(OnOpacitySliderChanged);
        }
    }

    private void OnValidate()
    {
        // Ensure default opacity is within valid range in editor
        defaultOpacity = Mathf.Clamp01(defaultOpacity);
    }
} 