using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "NewProject";

    // Deze functie hoef je dan niet meer met een parameter te koppelen
    public void SwitchScene()
    {
        Debug.Log($"Switching to scene: {sceneToLoad}");
        SceneManager.LoadScene(sceneToLoad);
    }
}