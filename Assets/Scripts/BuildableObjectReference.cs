using UnityEngine;
using SoulGames.EasyGridBuilderPro;

/// <summary>
/// Component dat een referentie houdt naar een BuildableObjectSO
/// Kan gebruikt worden op knoppen om de associatie met een ScriptableObject bij te houden
/// </summary>
public class BuildableObjectReference : MonoBehaviour
{
    [Header("Buildable Object Reference")]
    [SerializeField] public BuildableObjectSO buildableObjectSO;
    
    /// <summary>
    /// Stel de referentie in
    /// </summary>
    public void SetBuildableObjectSO(BuildableObjectSO objectSO)
    {
        buildableObjectSO = objectSO;
    }
    
    /// <summary>
    /// Verkrijg de referentie
    /// </summary>
    public BuildableObjectSO GetBuildableObjectSO()
    {
        return buildableObjectSO;
    }
} 