using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    [SerializeField] Button _newProjectButton;
    // Start is called before the first frame update
    void Start()
    {
        _newProjectButton.onClick.AddListener(ScenesManager.instance.LoadNewProject);
    }

    public void StartNewGame()
    {
        ScenesManager.instance.LoadNewGame();
    }

}
