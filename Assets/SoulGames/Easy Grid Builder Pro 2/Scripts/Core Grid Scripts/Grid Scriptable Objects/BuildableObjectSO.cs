using System;
using UnityEngine;
using SoulGames.Utilities;

namespace SoulGames.EasyGridBuilderPro
{
    public enum MaterialType
    {
        None,
        Staal,
        Beton,
        Hout
    }

    public enum RecyclabilityScore
    {
        None,
        Score1,
        Score2,
        Score3,
        Score4,
        Score5
    }

    public enum SustainabilityScore
    {
        None,
        Score1,
        Score2,
        Score3,
        Score4,
        Score5
    }

    public abstract class BuildableObjectSO : ScriptableObject
    {
        public string objectName;
        public Sprite objectIcon;
        public BuildableObjectUICategorySO buildableObjectUICategorySO;

        [Header("Product Information")]
        [TextArea(3, 6)]
        [Tooltip("Beschrijving van het object en zijn eigenschappen")]
        public string omschrijving = "";
        
        [Tooltip("Levensduur van het object")]
        public string levensduur = "";
        
        [Tooltip("Is het object recyclebaar")]
        public string recyclebaar = "";
        
        [Tooltip("Prijs van het object")]
        public string prijs = "";
        
        [Tooltip("Beschikbare certificering informatie")]
        public string beschikbareCertificering = "";
        
        [Tooltip("Certificering plaatjes/iconen")]
        public Sprite[] certificeringPlaatjes = new Sprite[0];
        
        [Tooltip("CO2 impact van het object")]
        public string co2 = "";

        [Header("Sustainability Calculations")]
        [Tooltip("Type materiaal van het object")]
        public MaterialType materialType = MaterialType.None;
        
        [Tooltip("CO2 uitstoot in kilogram (gebruikt voor berekeningen)")]
        public float co2EmissionsKg = 0f;
        
        [Tooltip("Prijs in euro (gebruikt voor berekeningen)")]
        public float priceEuros = 0f;
        
        [Tooltip("Gewicht van het object in kilogram")]
        public float weightKg = 0f;
        
        [Tooltip("Energie manufacturing verbruik (eenmalig bij productie)")]
        public float energyManufacturingKWh = 0f;
        
        [Tooltip("Energie verbruik per jaar in kWh")]
        public float energyConsumptionKWh = 0f;
        
        [Tooltip("Energie productie per jaar in kWh (bijv. zonnepanelen)")]
        public float energyProductionKWh = 0f;
        
        [Tooltip("CO2 uitstoot per jaar door energie verbruik (kg)")]
        public float energyCO2EmissionKgPerYear = 0f;
        
        [Tooltip("Energie kosten per jaar (Euros)")]
        public float energyCostEurosPerYear = 0f;
        
        [Tooltip("Recycleerbaarheid score (1-5, None = niet meegerekend)")]
        public RecyclabilityScore recyclabilityScore = RecyclabilityScore.None;
        
        [Tooltip("Duurzaamheid score (1-5, None = niet meegerekend)")]
        public SustainabilityScore sustainabilityScore = SustainabilityScore.None;

        [Serializable] public class RandomPrefabs
        {
            public Transform objectPrefab;
            public Transform ghostObjectPrefab;
            [Range(0, 100)] public float probability;
        }
        [Space]
        public RandomPrefabs[] randomPrefabs;
        public bool eachPlacementUseRandomPrefab = true;
        public Material validPlacementMaterial;
        public Material invalidPlacementMaterial;
        public bool setSpawnedObjectLayer = false;
        public LayerMask spawnedObjectLayer;

        public bool setGridSystemAsParent;

        public bool useObjectGridCustomAlphaMaskScale;
        public Vector2 objectGridCustomAlphaMaskScale;

        [Space]
        public LayerMask customSurfaceLayerMask;

        [Space]
        public bool raiseObjectWithVerticalGrids = true;

        // [Space]
        // public bool useCustomBuildConditions;
        // public List<CustomBuildConditionsSO> customBuildConditionsSOList;

        // [Space]
        // public bool useCustomPostBuildBehaviors;
        // public List<CustomBuildConditionsSO> customPostBuildBehaviorsSOList;
        
        [Space]
        public bool enablePlaceAndDeselect;

        [Space]
        public bool affectByBasicAreaDisablers = true;
        public bool affectByAreaDisablers = true;
        public bool affectByBasicAreaEnablers = true;
        public bool affectByAreaEnablers = true;

        [Space]
        public bool isObjectReplacable;
        // Not Added to the Custom Editor yet
        public bool replacingObjectIgnoreCustomConditions = true;
        public bool isObjectSelectable = true;
        public bool isSelectedObjectRotatable = true;
        public bool isObjectDestructable = true;
        public bool isObjectMovable = true;

        [Space]
        [Header("Color System")]
        [Tooltip("Automatically add ColorableObject script when this object is spawned")]
        public bool addColorableObjectScript = true;
        
        [Tooltip("Enable visual effects when object is colored (only if ColorableObject script is added)")]
        public bool enableColorEffects = true;
        
        [Tooltip("Allow saving color state between sessions (only if ColorableObject script is added)")]
        public bool saveColorState = true;

        [Space]
        public bool enableTerrainInteractions;
        public bool flattenTerrain;
        public float flattenerSize;

        [Space]
        public bool removeTerrainDetails;
        public float detailsRemoverSize;

        [Space]
        public bool paintTerrainTexture;
        public int terrainTextureIndex = 0;
        public float painterBrushSize;
        public TerrainInteractionUtilities.BrushType painterBrushType = TerrainInteractionUtilities.BrushType.Hard;
        [Range(0f, 1f)] public float painterBrushOpacity = 1f;
        [Range(0f, 1f)] public float painterBrushFallOff = 0f;
    }
}