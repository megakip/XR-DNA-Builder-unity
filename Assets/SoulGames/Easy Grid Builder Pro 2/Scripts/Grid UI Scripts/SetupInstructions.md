# Custom Category Button Handler - Correcte Setup Guide

## 🎯 Wat je nodig hebt:

### GameObject Structuur:
```
Canvas
├── CategoryButtonsPanel (waar je category knoppen staan)
│   ├── RoofsButton (Custom Category Button Handler script hierop)
│   ├── WallsButton (Custom Category Button Handler script hierop)
│   └── FloorsButton (Custom Category Button Handler script hierop)
├── ExternalBuildablesPanel (waar objecten verschijnen)
│   └── Content (Layout Group voor de buildable buttons)
└── TemplateButton (VERBORGEN - template voor buildable buttons)
```

## 🔧 Setup Stappen:

### Stap 1: Template Button Maken
1. **Maak een nieuwe Button** in je Canvas
2. **Noem het "TemplateButton"**
3. **Zet het INACTIEF** (`SetActive(false)`) - dit is je template
4. **Style het** zoals je buildable buttons eruit moeten zien
5. **Voeg Image component toe** voor de object preview

### Stap 2: External Panel Setup
1. **Maak een Panel** voor je buildable objects (bijv. "RoofObjectsPanel")
2. **Voeg Content Sizer Fitter toe** (bijv. Vertical/Horizontal Layout Group)
3. **Voeg Scroll View toe** als je veel objecten hebt

### Stap 3: Category Button Setup
1. **Selecteer je Category Button** (bijv. "RoofsButton")
2. **Custom Category Button Handler** toevoegen
3. **Configureer als volgt:**

#### Panel Configuration:
- **External Panel:** `RoofObjectsPanel` (waar objecten verschijnen)
- **Buildable Button Template:** `TemplateButton` (de VERBORGEN template)
- **Buildable Object Category:** `RoofsCategorySO` (je ScriptableObject)

#### Behavior Settings:
- ☑️ **Close Other Panels On Open:** AAN
- ☑️ **Allow Toggle:** AAN (optioneel)
- ☑️ **Auto Close On Grid Mode Change:** AAN

### Stap 4: ScriptableObject Category
Je hebt een **BuildableObjectUICategorySO** nodig met:
- **Category Name:** "Roofs"
- **Category Objects:** Lijst van alle roof BuildableObjectSO's

## 🎮 Workflow:
1. **Speler klikt "Roofs" button**
2. **Huidige buildables panel sluit**
3. **RoofObjectsPanel opent**
4. **TemplateButton wordt gekopieerd** voor elk roof object
5. **Elk gekopieerd button krijgt juiste image & functionaliteit**

## ⚠️ Veel voorkomende fouten:
- **Template Button = Category Button** ❌ (moet verschillend zijn)
- **External Panel niet ingesteld** ❌
- **Category SO is leeg** ❌
- **Template Button is actief** ❌ (moet inactive zijn)

## 🎨 Tips:
- **Style je Template Button** mooi - alle copies krijgen dezelfde look
- **Test met 1 category eerst** voordat je meer toevoegt
- **Gebruik Layout Groups** voor automatische button positionering 