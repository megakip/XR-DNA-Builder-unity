using UnityEngine;

public class ToggleActiveState : MonoBehaviour
{
    public void ToggleGameObject(GameObject target)
    {
        if (target != null)
        {
            target.SetActive(!target.activeSelf);
        }
    }
}