using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SoulGames.EasyGridBuilderPro;

/// <summary>
/// Nieuwe Sustainability Calculator Manager die geïntegreerd werkt met BuildableObjectSO
/// Houdt bij welke objecten geplaatst zijn en berekent totalen automatisch
/// </summary>
public class SustainabilityCalculatorManager : MonoBehaviour
{
    public static SustainabilityCalculatorManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Panel dat de sustainability berekeningen toont")]
    public GameObject sustainabilityPanel;
    
    [Tooltip("Button om het sustainability panel te openen/sluiten")]
    public Button togglePanelButton;

    [Header("Average Score")]
    [Tooltip("Text voor gemiddelde recycleerbaarheid score")]
    public TextMeshProUGUI avgRecyclabilityText;
    
    [Tooltip("Text voor gemiddelde sustainability score")]
    public TextMeshProUGUI avgSustainabilityText;
    
    [Header("Calculation Display")]
    [Tooltip("Text voor totale CO2 uitstoot")]
    public TextMeshProUGUI totalCO2Text;
    
    [Tooltip("Text voor totaal energie manufacturing")]
    public TextMeshProUGUI totalEnergyManufacturingText;
    
    [Tooltip("Text voor totale prijs")]
    public TextMeshProUGUI totalPriceText;
    
    [Tooltip("Text voor aantal geplaatste objecten")]
    public TextMeshProUGUI objectCountText;
    
    [Tooltip("Text voor totaal gewicht")]
    public TextMeshProUGUI totalWeightText;
    
    [Header("Energy")]
    [Tooltip("Text voor KWH per jaar")]
    public TextMeshProUGUI kwhYearText;
    
    [Tooltip("Text voor CO2 per jaar")]
    public TextMeshProUGUI co2YearText;
    
    [Tooltip("Text voor groene energie CO2")]
    public TextMeshProUGUI greenEnergyCO2Text;
    
    [Tooltip("Text voor grijze energie CO2")]
    public TextMeshProUGUI greyEnergyCO2Text;
    
    [Tooltip("Text voor gas CO2")]
    public TextMeshProUGUI gasCO2Text;
    
    [Header("Material Use")]
    [Tooltip("Parent content paneel voor materiaal types (bijv. 'Soort materiaal')")]
    public Transform materialContentPanel;
    
    [Tooltip("Text voor Staal CO2 uitstoot")]
    public TextMeshProUGUI staalCO2Text;
    
    [Tooltip("Text voor Beton CO2 uitstoot")]
    public TextMeshProUGUI betonCO2Text;
    
    [Tooltip("Text voor Hout CO2 uitstoot")]
    public TextMeshProUGUI houtCO2Text;

    // Tracking data
    private Dictionary<BuildableObjectSO, int> placedObjectsCounts = new Dictionary<BuildableObjectSO, int>();
    private Dictionary<MaterialType, float> materialWeights = new Dictionary<MaterialType, float>();
    private Dictionary<MaterialType, float> materialCO2Emissions = new Dictionary<MaterialType, float>();
    
    // Grid Manager reference
    private GridManager gridManager;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Zorg dat panel initieel gesloten is
        if (sustainabilityPanel)
            sustainabilityPanel.SetActive(false);
            
        // Setup toggle button
        if (togglePanelButton)
            togglePanelButton.onClick.AddListener(TogglePanel);
            
        // Initialize material tracking dictionaries
        materialWeights.Clear();
        materialCO2Emissions.Clear();
    }

    private void Start()
    {
        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();
        
        // Subscribe to Easy Grid Builder Pro events
        if (GridManager.Instance != null)
        {
            GridManager.Instance.OnBuildableObjectPlaced += OnBuildableObjectPlaced;
            
            // Subscribe to destruction events through BuildableObjectDestroyer
            if (GridManager.Instance.TryGetBuildableObjectDestroyer(out BuildableObjectDestroyer buildableObjectDestroyer))
            {
                buildableObjectDestroyer.OnBuildableObjectDestroyed += OnBuildableObjectDestroyed;
                Debug.Log("[SustainabilityCalculatorManager] Successfully subscribed to Easy Grid Builder Pro events.");
            }
            else
            {
                Debug.LogError("[SustainabilityCalculatorManager] Could not get BuildableObjectDestroyer!");
            }
        }
        else
        {
            Debug.LogError("[SustainabilityCalculatorManager] GridManager.Instance is null!");
        }
        
        UpdateUI();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (GridManager.Instance != null)
        {
            GridManager.Instance.OnBuildableObjectPlaced -= OnBuildableObjectPlaced;
            
            // Unsubscribe from destruction events
            if (GridManager.Instance.TryGetBuildableObjectDestroyer(out BuildableObjectDestroyer buildableObjectDestroyer))
            {
                buildableObjectDestroyer.OnBuildableObjectDestroyed -= OnBuildableObjectDestroyed;
            }
        }
    }

    private void OnBuildableObjectPlaced(EasyGridBuilderPro easyGridBuilderPro, BuildableObject buildableObject)
    {
        if (buildableObject == null) return;
        
        BuildableObjectSO buildableObjectSO = buildableObject.GetBuildableObjectSO();
        if (buildableObjectSO == null) return;
        
        Debug.Log($"[SustainabilityCalculatorManager] Object placed: {buildableObjectSO.objectName}");
        
        // Voeg object toe aan tracking
        if (placedObjectsCounts.ContainsKey(buildableObjectSO))
        {
            placedObjectsCounts[buildableObjectSO]++;
        }
        else
        {
            placedObjectsCounts[buildableObjectSO] = 1;
        }
        
        UpdateUI();
    }

    private void OnBuildableObjectDestroyed(EasyGridBuilderPro easyGridBuilderPro, BuildableObject buildableObject)
    {
        if (buildableObject == null) return;
        
        BuildableObjectSO buildableObjectSO = buildableObject.GetBuildableObjectSO();
        if (buildableObjectSO == null) return;
        
        Debug.Log($"[SustainabilityCalculatorManager] Object destroyed: {buildableObjectSO.objectName}");
        
        // Verwijder object uit tracking
        if (placedObjectsCounts.ContainsKey(buildableObjectSO))
        {
            placedObjectsCounts[buildableObjectSO]--;
            if (placedObjectsCounts[buildableObjectSO] <= 0)
            {
                placedObjectsCounts.Remove(buildableObjectSO);
            }
        }
        
        UpdateUI();
    }

    public void TogglePanel()
    {
        if (sustainabilityPanel)
        {
            bool isActive = sustainabilityPanel.activeSelf;
            sustainabilityPanel.SetActive(!isActive);
            
            if (!isActive)
            {
                UpdateUI(); // Refresh when opening
            }
        }
    }

    private void UpdateUI()
    {
        CalculateAndDisplayTotals();
        UpdateMaterialTracking();
        UpdateEnergyDisplay();
        UpdateMaterialDisplay();
    }

    private float GetScoreValue(RecyclabilityScore score)
    {
        switch (score)
        {
            case RecyclabilityScore.None: return 0f;
            case RecyclabilityScore.Score1: return 1f;
            case RecyclabilityScore.Score2: return 2f;
            case RecyclabilityScore.Score3: return 3f;
            case RecyclabilityScore.Score4: return 4f;
            case RecyclabilityScore.Score5: return 5f;
            default: return 0f;
        }
    }
    
    private float GetScoreValue(SustainabilityScore score)
    {
        switch (score)
        {
            case SustainabilityScore.None: return 0f;
            case SustainabilityScore.Score1: return 1f;
            case SustainabilityScore.Score2: return 2f;
            case SustainabilityScore.Score3: return 3f;
            case SustainabilityScore.Score4: return 4f;
            case SustainabilityScore.Score5: return 5f;
            default: return 0f;
        }
    }

    private void CalculateAndDisplayTotals()
    {
        float totalCO2 = 0f;
        float totalPrice = 0f;
        float totalWeight = 0f;
        float totalEnergyManufacturing = 0f;
        float totalEnergyConsumption = 0f;
        float totalEnergyProduction = 0f;
        float totalEnergyCO2 = 0f;
        float totalEnergyCost = 0f;
        float totalRecyclability = 0f;
        float totalSustainability = 0f;
        int totalObjects = 0;
        
        // Reset material tracking
        materialWeights.Clear();

        foreach (var kvp in placedObjectsCounts)
        {
            BuildableObjectSO obj = kvp.Key;
            int count = kvp.Value;

            totalCO2 += obj.co2EmissionsKg * count;
            totalPrice += obj.priceEuros * count;
            totalWeight += obj.weightKg * count;
            totalEnergyManufacturing += obj.energyManufacturingKWh * count;
            totalEnergyConsumption += obj.energyConsumptionKWh * count;
            totalEnergyProduction += obj.energyProductionKWh * count;
            totalEnergyCO2 += obj.energyCO2EmissionKgPerYear * count;
            totalEnergyCost += obj.energyCostEurosPerYear * count;
            
            // Track materials
            if (obj.materialType != MaterialType.None)
            {
                if (materialWeights.ContainsKey(obj.materialType))
                {
                    materialWeights[obj.materialType] += obj.weightKg * count;
                }
                else
                {
                    materialWeights[obj.materialType] = obj.weightKg * count;
                }
            }
            
            // Convert score enums to numbers for calculation (None = 0, Score1 = 1, etc.)
            float recyclabilityValue = GetScoreValue(obj.recyclabilityScore);
            float sustainabilityValue = GetScoreValue(obj.sustainabilityScore);
            
            if (recyclabilityValue > 0) // Only count objects with actual scores
            {
                totalRecyclability += recyclabilityValue * count;
            }
            if (sustainabilityValue > 0) // Only count objects with actual scores
            {
                totalSustainability += sustainabilityValue * count;
            }
            totalObjects += count;
        }

        // Gemiddelde scores
        float avgRecyclability = totalObjects > 0 ? totalRecyclability / totalObjects : 0f;
        float avgSustainability = totalObjects > 0 ? totalSustainability / totalObjects : 0f;

        // Update UI
        if (avgRecyclabilityText) avgRecyclabilityText.text = $"{avgRecyclability:F1}/5";
        if (avgSustainabilityText) avgSustainabilityText.text = $"{avgSustainability:F1}/5";
        if (totalCO2Text) totalCO2Text.text = $"{totalCO2:F1} kg";
        if (totalEnergyManufacturingText) totalEnergyManufacturingText.text = $"{totalEnergyManufacturing:F1} kWh";
        if (totalPriceText) totalPriceText.text = $"€{totalPrice:F2}";
        if (objectCountText) objectCountText.text = $"{totalObjects} objecten";
        if (totalWeightText) totalWeightText.text = $"{totalWeight:F1} kg";
        // Energy display wordt nu apart afgehandeld in UpdateEnergyDisplay()
    }

    private void UpdateMaterialTracking()
    {
        // Reset tracking dictionaries
        materialWeights.Clear();
        materialCO2Emissions.Clear();

        // Calculate material weights and CO2 per material type
        foreach (var kvp in placedObjectsCounts)
        {
            BuildableObjectSO obj = kvp.Key;
            int count = kvp.Value;

            if (obj.materialType != MaterialType.None)
            {
                float objectWeight = obj.weightKg * count;
                float objectCO2 = obj.co2EmissionsKg * count;

                if (materialWeights.ContainsKey(obj.materialType))
                {
                    materialWeights[obj.materialType] += objectWeight;
                    materialCO2Emissions[obj.materialType] += objectCO2;
                }
                else
                {
                    materialWeights[obj.materialType] = objectWeight;
                    materialCO2Emissions[obj.materialType] = objectCO2;
                }
            }
        }
    }

    private void UpdateEnergyDisplay()
    {
        float totalEnergyConsumption = 0f;
        float totalEnergyProduction = 0f;
        float totalEnergyCO2 = 0f;

        foreach (var kvp in placedObjectsCounts)
        {
            BuildableObjectSO obj = kvp.Key;
            int count = kvp.Value;

            totalEnergyConsumption += obj.energyConsumptionKWh * count;
            totalEnergyProduction += obj.energyProductionKWh * count;
            totalEnergyCO2 += obj.energyCO2EmissionKgPerYear * count;
        }

        // Update energy UI elements
        if (kwhYearText) kwhYearText.text = $"{totalEnergyConsumption:F1} kWh/jaar";
        if (co2YearText) co2YearText.text = $"{totalEnergyCO2:F1} kg CO2/jaar";
        
        // Voor nu: green energy, grey energy en gas CO2 zijn placeholder values
        // Deze kunnen later uitgebreid worden met specifieke velden in BuildableObjectSO
        if (greenEnergyCO2Text) greenEnergyCO2Text.text = $"{(totalEnergyCO2 * 0.3f):F1} kg CO2";
        if (greyEnergyCO2Text) greyEnergyCO2Text.text = $"{(totalEnergyCO2 * 0.5f):F1} kg CO2";
        if (gasCO2Text) gasCO2Text.text = $"{(totalEnergyCO2 * 0.2f):F1} kg CO2";
    }

    private void UpdateMaterialDisplay()
    {
        // Update Staal CO2
        float staalCO2 = materialCO2Emissions.ContainsKey(MaterialType.Staal) ? materialCO2Emissions[MaterialType.Staal] : 0f;
        if (staalCO2Text) staalCO2Text.text = $"{staalCO2:F1} kg CO2";

        // Update Beton CO2
        float betonCO2 = materialCO2Emissions.ContainsKey(MaterialType.Beton) ? materialCO2Emissions[MaterialType.Beton] : 0f;
        if (betonCO2Text) betonCO2Text.text = $"{betonCO2:F1} kg CO2";

        // Update Hout CO2
        float houtCO2 = materialCO2Emissions.ContainsKey(MaterialType.Hout) ? materialCO2Emissions[MaterialType.Hout] : 0f;
        if (houtCO2Text) houtCO2Text.text = $"{houtCO2:F1} kg CO2";
    }

    private string GetMaterialDisplayName(MaterialType materialType)
    {
        switch (materialType)
        {
            case MaterialType.Staal: return "Staal";
            case MaterialType.Beton: return "Beton";
            case MaterialType.Hout: return "Hout";
            case MaterialType.None: return "None";
            default: return materialType.ToString();
        }
    }



    /// <summary>
    /// Public methode om handmatig data te resetten (voor testing)
    /// </summary>
    [ContextMenu("Reset All Data")]
    public void ResetAllData()
    {
        placedObjectsCounts.Clear();
        UpdateUI();
        Debug.Log("[SustainabilityCalculatorManager] All data reset");
    }

    /// <summary>
    /// Get totals data voor externe scripts
    /// </summary>
    public SustainabilityTotals GetCurrentTotals()
    {
        float totalCO2 = 0f;
        float totalPrice = 0f;
        float totalWeight = 0f;
        float totalEnergyManufacturing = 0f;
        float totalEnergyConsumption = 0f;
        float totalEnergyProduction = 0f;
        float totalEnergyCO2 = 0f;
        float totalEnergyCost = 0f;
        float totalRecyclability = 0f;
        float totalSustainability = 0f;
        int totalObjects = 0;

        foreach (var kvp in placedObjectsCounts)
        {
            BuildableObjectSO obj = kvp.Key;
            int count = kvp.Value;

            totalCO2 += obj.co2EmissionsKg * count;
            totalPrice += obj.priceEuros * count;
            totalWeight += obj.weightKg * count;
            totalEnergyManufacturing += obj.energyManufacturingKWh * count;
            totalEnergyConsumption += obj.energyConsumptionKWh * count;
            totalEnergyProduction += obj.energyProductionKWh * count;
            totalEnergyCO2 += obj.energyCO2EmissionKgPerYear * count;
            totalEnergyCost += obj.energyCostEurosPerYear * count;
            
            // Convert score enums to numbers for calculation (None = 0, Score1 = 1, etc.)
            float recyclabilityValue = GetScoreValue(obj.recyclabilityScore);
            float sustainabilityValue = GetScoreValue(obj.sustainabilityScore);
            
            if (recyclabilityValue > 0) // Only count objects with actual scores
            {
                totalRecyclability += recyclabilityValue * count;
            }
            if (sustainabilityValue > 0) // Only count objects with actual scores
            {
                totalSustainability += sustainabilityValue * count;
            }
            totalObjects += count;
        }

        return new SustainabilityTotals
        {
            totalCO2Kg = totalCO2,
            totalPriceEuros = totalPrice,
            totalWeightKg = totalWeight,
            totalEnergyManufacturingKWh = totalEnergyManufacturing,
            totalEnergyConsumptionKWh = totalEnergyConsumption,
            totalEnergyProductionKWh = totalEnergyProduction,
            totalEnergyCO2Kg = totalEnergyCO2,
            totalEnergyCostEuros = totalEnergyCost,
            averageRecyclabilityScore = totalObjects > 0 ? totalRecyclability / totalObjects : 0f,
            averageSustainabilityScore = totalObjects > 0 ? totalSustainability / totalObjects : 0f,
            totalObjectCount = totalObjects
        };
    }
}

/// <summary>
/// Data structure voor sustainability totals
/// </summary>
[System.Serializable]
public class SustainabilityTotals
{
    public float totalCO2Kg;
    public float totalPriceEuros;
    public float totalWeightKg;
    public float totalEnergyManufacturingKWh;
    public float totalEnergyConsumptionKWh;
    public float totalEnergyProductionKWh;
    public float totalEnergyCO2Kg;
    public float totalEnergyCostEuros;
    public float averageRecyclabilityScore;
    public float averageSustainabilityScore;
    public int totalObjectCount;
} 