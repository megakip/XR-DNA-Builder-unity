using UnityEngine;
using Unity.XR.CoreUtils;

/// <summary>
/// Installer voor het GlobalMenuCloser systeem.
/// Voegt automatisch een GlobalMenuCloser component toe aan de XR Origin in de scene.
/// </summary>
public class GlobalMenuCloserInstaller : MonoBehaviour
{
    [Tooltip("Of de installer automatisch moet zoeken naar de XR Origin in de scene")]
    public bool autoFindXROrigin = true;
    
    [Tooltip("Handmatig ingestelde XR Origin als autoFindXROrigin uitstaat")]
    public XROrigin xrOrigin;
    
    [Tooltip("Of er naar controller trigger events moet worden geluisterd")]
    public bool listenToControllers = true;
    
    [Tooltip("Of er naar hand grijp-gebaren moet worden geluisterd")]
    public bool listenToHands = true;
    
    [Tooltip("Hoe vaak de check moet worden uitgevoerd (in seconden)")]
    public float checkInterval = 0.1f;
    
    [Tooltip("Verwijder dit GameObject na installatie")]
    public bool deleteAfterInstall = true;

    private void Start()
    {
        // Zoek de XR Origin als dat nodig is
        if (autoFindXROrigin || xrOrigin == null)
        {
            xrOrigin = FindFirstObjectByType<XROrigin>();
            
            if (xrOrigin == null)
            {
                Debug.LogError("Geen XR Origin gevonden in de scene. GlobalMenuCloser kan niet worden geïnstalleerd.");
                return;
            }
        }
        
        // Controleer of er al een GlobalMenuCloser component bestaat
        GlobalMenuCloser existingCloser = xrOrigin.GetComponent<GlobalMenuCloser>();
        
        if (existingCloser == null)
        {
            // Voeg een nieuw GlobalMenuCloser component toe
            GlobalMenuCloser menuCloser = xrOrigin.gameObject.AddComponent<GlobalMenuCloser>();
            menuCloser.listenToControllers = listenToControllers;
            menuCloser.listenToHands = listenToHands;
            menuCloser.checkInterval = checkInterval;
            
            Debug.Log("GlobalMenuCloser succesvol geïnstalleerd op XR Origin");
        }
        else
        {
            // Update bestaande instellingen
            existingCloser.listenToControllers = listenToControllers;
            existingCloser.listenToHands = listenToHands;
            existingCloser.checkInterval = checkInterval;
            
            Debug.Log("Bestaande GlobalMenuCloser bijgewerkt");
        }
        
        // Verwijder dit GameObject indien gewenst
        if (deleteAfterInstall)
        {
            Destroy(gameObject);
        }
    }
} 