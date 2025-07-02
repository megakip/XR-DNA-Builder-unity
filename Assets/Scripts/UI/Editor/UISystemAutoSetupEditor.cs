using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor script for UI System Auto Setup Tools menu
/// </summary>
public class UISystemAutoSetupEditor
{
    /// <summary>
    /// Menu item to run setup from Tools menu
    /// </summary>
    [MenuItem("Tools/UI System/Auto Setup UI System")]
    private static void SetupFromMenu()
    {
        UISystemAutoSetup.RunSetupFromCode();
    }
} 