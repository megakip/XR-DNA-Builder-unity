using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace SoulGames.VR
{
    /// <summary>
    /// Test script to verify VR controller interaction is working
    /// </summary>
    public class VRTestInteractable : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = Color.yellow;
        [SerializeField] private Color selectColor = Color.green;
        
        private Renderer objectRenderer;
        private XRGrabInteractable grabInteractable;
        
        private void Awake()
        {
            objectRenderer = GetComponent<Renderer>();
            grabInteractable = GetComponent<XRGrabInteractable>();
            
            if (objectRenderer != null)
            {
                objectRenderer.material.color = normalColor;
            }
        }
        
        private void Start()
        {
            if (grabInteractable != null)
            {
                grabInteractable.hoverEntered.AddListener(OnHoverEntered);
                grabInteractable.hoverExited.AddListener(OnHoverExited);
                grabInteractable.selectEntered.AddListener(OnSelectEntered);
                grabInteractable.selectExited.AddListener(OnSelectExited);
            }
        }
        
        private void OnHoverEntered(UnityEngine.XR.Interaction.Toolkit.HoverEnterEventArgs args)
        {
            if (objectRenderer != null)
            {
                // objectRenderer.material.color = hoverColor; // Disabled - no color change on hover
            }
            Debug.Log($"VR Test Object: Hover entered by {args.interactorObject.transform.name}");
        }
        
        private void OnHoverExited(UnityEngine.XR.Interaction.Toolkit.HoverExitEventArgs args)
        {
            if (objectRenderer != null)
            {
                // Color changing logic disabled - keeping current color
                // if (grabInteractable.isSelected)
                // {
                //     objectRenderer.material.color = selectColor;
                //     Debug.Log($"VR Test Object: Hover exited but object is still selected, setting color to selectColor.");
                // }
                // else
                // {
                //     objectRenderer.material.color = normalColor;
                //     Debug.Log($"VR Test Object: Hover exited, setting color to normalColor.");
                // }
            }
            Debug.Log($"VR Test Object: Hover exited by {args.interactorObject.transform.name}");
        }
        
        private void OnSelectEntered(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
        {
            if (objectRenderer != null)
            {
                // objectRenderer.material.color = selectColor; // Disabled - no color change on select
            }
            Debug.Log($"VR Test Object: Selected by {args.interactorObject.transform.name}");
        }
        
        private void OnSelectExited(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
        {
            if (objectRenderer != null)
            {
                // Color changing logic disabled - keeping current color
                // if (grabInteractable.isHovered)
                // {
                //     objectRenderer.material.color = hoverColor;
                //     Debug.Log($"VR Test Object: Deselected but still hovered, setting to hoverColor.");
                // }
                // else
                // {
                //     objectRenderer.material.color = normalColor;
                //     Debug.Log($"VR Test Object: Deselected and not hovered, setting to normalColor.");
                // }
            }
            Debug.Log($"VR Test Object: Deselected by {args.interactorObject.transform.name}");
        }
        
        private void OnDestroy()
        {
            if (grabInteractable != null)
            {
                grabInteractable.hoverEntered.RemoveListener(OnHoverEntered);
                grabInteractable.hoverExited.RemoveListener(OnHoverExited);
                grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
                grabInteractable.selectExited.RemoveListener(OnSelectExited);
            }
        }
    }
}