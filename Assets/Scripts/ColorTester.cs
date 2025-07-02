using UnityEngine;

/// <summary>
/// Simple color tester to manually test coloring functionality
/// </summary>
public class ColorTester : MonoBehaviour
{
    [Header("Test Colors")]
    public Color testColorRed = Color.red;
    public Color testColorBlue = Color.blue;
    public Color testColorGreen = Color.green;
    public Color testColorYellow = Color.yellow;
    
    [Header("Target Object")]
    public GameObject targetObject;
    
    [ContextMenu("Auto Find Target")]
    void AutoFindTarget()
    {
        if (targetObject == null)
        {
            ColorableObject colorable = FindObjectOfType<ColorableObject>();
            if (colorable != null)
            {
                targetObject = colorable.gameObject;
                Debug.Log($"ColorTester: Found target object: {targetObject.name}");
            }
            else
            {
                Debug.LogWarning("ColorTester: No ColorableObject found in scene");
            }
        }
    }
    
    [ContextMenu("Test Red")]
    void TestRed()
    {
        TestColor(testColorRed, "Red");
    }
    
    [ContextMenu("Test Blue")]
    void TestBlue()
    {
        TestColor(testColorBlue, "Blue");
    }
    
    [ContextMenu("Test Green")]
    void TestGreen()
    {
        TestColor(testColorGreen, "Green");
    }
    
    [ContextMenu("Test Yellow")]
    void TestYellow()
    {
        TestColor(testColorYellow, "Yellow");
    }
    
    void TestColor(Color color, string colorName)
    {
        if (targetObject == null)
        {
            AutoFindTarget();
            if (targetObject == null)
            {
                Debug.LogError("ColorTester: No target object assigned and none found automatically");
                return;
            }
        }
        
        ColorableObject colorableObject = targetObject.GetComponent<ColorableObject>();
        if (colorableObject != null)
        {
            Debug.Log($"ColorTester: Testing {colorName} color ({color}) on {targetObject.name}");
            colorableObject.ChangeColor(color);
        }
        else
        {
            Debug.LogError($"ColorTester: Target object {targetObject.name} does not have ColorableObject component");
        }
    }
    
    [ContextMenu("Debug Target Object")]
    void DebugTargetObject()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("ColorTester: No target object assigned");
            return;
        }
        
        ColorableObject colorableObject = targetObject.GetComponent<ColorableObject>();
        if (colorableObject != null)
        {
            // Use the debug method from ColorableObject
            var debugMethod = typeof(ColorableObject).GetMethod("DebugRendererComponents", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
            if (debugMethod != null)
            {
                debugMethod.Invoke(colorableObject, null);
            }
        }
        else
        {
            Debug.LogError($"ColorTester: Target object {targetObject.name} does not have ColorableObject component");
        }
    }
} 