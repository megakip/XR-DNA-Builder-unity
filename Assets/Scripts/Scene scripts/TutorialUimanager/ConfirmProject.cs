using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmProject : MonoBehaviour
{
    [SerializeField] private Button _confirmProjectButton;

    void Start()
    {
        // Check if button is assigned in Inspector
        if (_confirmProjectButton != null)
        {
            _confirmProjectButton.onClick.AddListener(ScenesManager.instance.LoadUIscene);
        }
        else
        {
            Debug.LogError("ConfirmProject: _confirmProjectButton is not assigned in the Inspector");
        }
    }

    public void StartNewGame()
    {
        // Use the correct scene name from enum
        ScenesManager.instance.LoadScene(ScenesManager.Scene.UIscene);
    }
}
