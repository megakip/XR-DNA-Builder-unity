# Custom UI System voor Easy Grid Builder Pro 2

## Overzicht

Dit systeem geeft je volledige controle over waar en hoe buildable objects worden getoond. Je kunt nu:

1. **De positie van het standaard Buildable Objects panel aanpassen**
2. **Custom knoppen maken die hun eigen panelen openen**
3. **Externe panelen koppelen aan specifieke categorieën**

## 1. Standaard Panel Positie Aanpassen

### Stap 1: GridUIManager instellen
1. Ga naar je **GridUIManager** GameObject in de scene
2. Zoek naar **"Buildable Objects UI Panel Data"**
3. Vouw uit en zoek naar **"Panel Position Settings"**

### Beschikbare Posities:
- **TopLeft** - Linksboven
- **TopCenter** - Midden boven  
- **TopRight** - Rechtsboven (standaard)
- **MiddleLeft** - Links midden
- **MiddleCenter** - Midden scherm
- **MiddleRight** - Rechts midden
- **BottomLeft** - Linksonder
- **BottomCenter** - Midden onder
- **BottomRight** - Rechtsonder
- **Custom** - Handmatige positionering

### Instellingen:
- **Panel Position**: Kies uit dropdown
- **Position Offset**: Fine-tuning van positie (X, Y pixels)
- **Update Position On Start**: Panel automatisch positioneren bij opstarten

## 2. Custom Category Buttons - Eenvoudige Methode

### Voor simpele toggle functionaliteit:

1. **Maak een Button** in je Canvas
2. **Voeg SimpleCategoryButton toe**: 
   - Selecteer button → Add Component → Easy Grid Builder Pro → Grid UI → Simple Category Button
3. **Configureer**:
   - **Target Category**: Welke categorie objecten te tonen
   - **Target Panel**: Welk panel te tonen/verbergen

```csharp
// Voorbeeld: Programmatisch instellen
SimpleCategoryButton simpleButton = button.GetComponent<SimpleCategoryButton>();
simpleButton.SetTargetCategory(myCategory);
simpleButton.SetTargetPanel(myPanel);
```

## 3. Custom Category Buttons - Geavanceerde Methode

### Voor volledige controle en animaties:

1. **Maak een Button** in je Canvas
2. **Maak een leeg Panel** GameObject als child van Canvas
3. **Voeg CustomCategoryButtonHandler toe**:
   - Selecteer button → Add Component → Easy Grid Builder Pro → Grid UI → Custom Category Button Handler

### Configuratie Opties:

#### Panel Configuration:
- **External Panel**: Het panel waar buildable objects verschijnen
- **Buildable Button Template**: Template voor object buttons (copy van bestaande button)
- **Buildable Object Category**: Specifieke categorie om te filteren

#### Behavior Settings:
- **Close Other Panels On Open**: Andere custom panelen sluiten
- **Allow Toggle**: Paneel uit/aan kunnen zetten met button
- **Auto Close On Grid Mode Change**: Automatisch sluiten bij mode wissel

#### Animation Settings:
- **Use Fade Animation**: Fade in/out effect
- **Animation Duration**: Duur van animatie

## 4. Automatische Setup met CustomUISetupUtility

### Snelle complete setup:

1. **Voeg CustomUISetupUtility toe** aan een GameObject
2. **Configureer templates** (optioneel)
3. **Gebruik CreateCompleteCustomCategorySetup**:

```csharp
CustomUISetupUtility utility = GetComponent<CustomUISetupUtility>();

// Maak complete setup: button + panel
var (button, panel) = utility.CreateCompleteCustomCategorySetup(
    parentCanvas: myCanvas,
    buttonPosition: UIPanelPosition.BottomLeft,
    panelPosition: UIPanelPosition.TopLeft,
    category: myBuildableCategory,
    buttonName: "Buildings",
    panelName: "Building Panel"
);
```

### Editor Tool:
- **Target Canvas**: Canvas om elementen in te maken
- **Test Button/Panel Position**: Posities om te testen
- **Test Category**: Categorie voor test
- **Right-click → "Create Test Setup"** voor snelle test

## 5. Praktische Voorbeelden

### Voorbeeld 1: Gebouwen en Decoratie Knoppen
```csharp
// Gebouwen knop (linksonder)
var (buildingsButton, buildingsPanel) = utility.CreateCompleteCustomCategorySetup(
    canvas, UIPanelPosition.BottomLeft, UIPanelPosition.TopLeft, 
    buildingCategory, "Buildings", "Buildings Panel"
);

// Decoratie knop (rechtsonder)  
var (decorButton, decorPanel) = utility.CreateCompleteCustomCategorySetup(
    canvas, UIPanelPosition.BottomRight, UIPanelPosition.TopRight,
    decorationCategory, "Decorations", "Decoration Panel"
);
```

### Voorbeeld 2: Bestaande Button uitbreiden
```csharp
// Voeg handler toe aan bestaande button
Button myExistingButton = // ... je bestaande button
RectTransform myExistingPanel = // ... je bestaande panel

CustomCategoryButtonHandler handler = utility.SetupCustomCategoryButton(
    myExistingButton, myExistingPanel, myCategory, buttonTemplate
);
```

## 6. Tips en Best Practices

### Panel Layout:
- Gebruik **GridLayoutGroup** voor automatische button plaatsing
- Stel **Cell Size** in op bijv. (64, 64) voor buttons
- **Spacing** van (5, 5) voor mooie ruimte tussen buttons

### Button Templates:
- Copy een bestaande buildable button uit het originele systeem
- Zorg dat het een **Image** en **Button** component heeft
- **Template mag inactief zijn** - wordt automatisch geactiveerd

### Performance:
- **Panels automatisch sluiten** wanneer niet in BuildMode
- **Button pooling** overweeg je bij veel objecten
- **Lazy loading** - objecten pas laden wanneer panel opent

### Visueel:
- **CanvasGroup** gebruiken voor smooth fade animaties
- **Consistent styling** - gebruik dezelfde kleuren/fonts als origineel systeem
- **Responsive design** - test op verschillende schermresoluties

## 7. Troubleshooting

### Veel voorkomende problemen:

**Panel verschijnt niet:**
- Check of **Canvas** correct is ingesteld
- Controleer **CanvasGroup** alpha waarde
- Verify **GameObject.SetActive(true)** wordt aangeroepen

**Buttons werken niet:**
- Controleer of **BuildableObjectSO** correct is toegewezen aan grid
- Check **Button Template** heeft juiste components
- Verify **GridManager** en **EasyGridBuilderPro** zijn geinitialiseerd

**Positionering problemen:**
- **RectTransform** anchor/pivot settings controleren  
- **Canvas Scaler** settings checken voor verschillende resoluties
- **Safe Area** overwegen voor mobiele apparaten

### Debug Tips:
```csharp
// Check of grid actief is
Debug.Log($"Active Grid: {GridManager.Instance.GetActiveEasyGridBuilderPro()}");

// Check buildable objects
Debug.Log($"Grid Objects Count: {activeGrid.GetBuildableGridObjectSOList().Count}");

// Check panel status  
Debug.Log($"Panel Active: {panel.gameObject.activeSelf}");
```

## 8. Extensies

Het systeem is ontworpen om uitbreidbaar te zijn. Je kunt:

- **Eigen animaties** toevoegen aan CustomCategoryButtonHandler
- **Filtering** uitbreiden in GetBuildableObjectsToShow()
- **Event systeem** gebruiken voor communicatie tussen panelen
- **Save/Load** functionaliteit toevoegen voor panel posities

Voor vragen of uitbreidingen, check de originele Easy Grid Builder Pro documentatie of pas de scripts aan naar je behoeften! 