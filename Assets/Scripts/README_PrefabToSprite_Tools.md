# Prefab to Sprite Converter Tools

Deze Unity Editor tools maken het mogelijk om 3D prefabs om te zetten naar 2D sprites voor gebruik in je interface. Perfect voor het maken van UI icons van je 3D objecten!

## 🔧 Tools Overview

### 1. **Prefab To Sprite Converter** (`PrefabToSpriteConverter.cs`)
- Converteer individuele prefabs naar sprites
- Real-time preview functionaliteit
- Uitgebreide camera en lighting controles
- Perfect voor het fine-tunen van individuele sprites

### 2. **Batch Sprite Converter** (`BatchSpriteConverter.cs`)
- Converteer meerdere prefabs tegelijk
- Progress tracking en error handling
- Bulk operaties met shared settings
- Ideaal voor grote aantallen prefabs

## 📖 Installatie

1. Plaats beide scripts in de `Assets/Scripts/Editor/` folder
2. Unity compileert automatisch de Editor scripts
3. De tools verschijnen in het **Tools** menu

## 🎯 Single Prefab Converter Gebruik

### Stap 1: Open de Tool
```
Unity Menu > Tools > Prefab To Sprite Converter
```

### Stap 2: Configureer Instellingen

#### **Prefab Settings**
- **Prefab to Convert**: Sleep je prefab hier naartoe

#### **Render Settings**
- **Texture Size**: Resolutie van de output sprite (64-2048px)
- **Background Color**: Achtergrondkleur (gebruik `Clear` voor transparantie)
- **Culling Mask**: Welke layers te renderen

#### **Camera Settings**
- **Camera Position**: Positie van de render camera
- **Camera Rotation**: Rotatie van de camera
- **Orthographic**: Gebruik orthografische projectie (aanbevolen voor UI)
- **Orthographic Size**: Zoom level voor orthografische camera
- **Field of View**: FOV voor perspective camera

#### **Lighting Settings**
- **Use Custom Lighting**: Gebruik custom lighting setup
- **Light Color**: Kleur van het licht
- **Light Intensity**: Sterkte van het licht
- **Light Direction**: Richting van het licht

#### **Output Settings**
- **Output Folder**: Waar sprites opslaan (`Assets/Generated Sprites`)
- **Sprite Prefix**: Prefix voor sprite names (`Sprite_`)
- **Texture Format**: Import format (RGBA32 aanbevolen)
- **Generate Mip Maps**: Genereer mip maps (meestal uit voor UI)

### Stap 3: Genereer Sprite
1. **Generate Preview**: Test je instellingen eerst
2. **Generate Sprite**: Maak de definitieve sprite

## 🔄 Batch Converter Gebruik

### Stap 1: Open de Batch Tool
```
Unity Menu > Tools > Batch Sprite Converter
```

### Stap 2: Voeg Prefabs Toe

#### **Handmatig Toevoegen**
- Klik **Add Prefab** voor lege entries
- Sleep prefabs in de velden
- Pas custom names aan indien gewenst

#### **Project Selectie**
1. Selecteer prefabs in je Project window
2. Klik **Add Selected from Project**
3. Alle geselecteerde prefabs worden toegevoegd

#### **List Management**
- **Checkbox**: Enable/disable individuele prefabs
- **Remove Selected**: Verwijder uitgeschakelde entries
- **Clear All**: Leeg de hele lijst

### Stap 3: Configureer Shared Settings
Alle instellingen zijn hetzelfde als de single converter, maar worden toegepast op alle prefabs.

#### **Quick Settings**
- **Icon Size (128)**: Optimaal voor kleine UI icons
- **UI Size (256)**: Standaard voor UI elements
- **High Quality (512)**: Voor grote displays of detail work

### Stap 4: Batch Processing
- **Show Progress Bar**: Toon voortgang tijdens conversie
- **Stop on Error**: Stop bij eerste fout (anders ga door)
- **Test First Prefab**: Test instellingen op eerste prefab
- **Convert All Prefabs**: Start batch conversie

## 💡 Tips & Best Practices

### **Camera Setup**
```csharp
// Voor isometrische view:
Camera Position: (2, 2, -2)
Camera Rotation: (30, 45, 0)
Orthographic: true
Orthographic Size: 2

// Voor front-facing view:
Camera Position: (0, 0, -3)
Camera Rotation: (0, 0, 0)
Orthographic: true
Orthographic Size: 1.5
```

### **Lighting Setup**
```csharp
// Voor helder, clean lighting:
Use Custom Lighting: true
Light Color: White
Light Intensity: 1.2
Light Direction: (-0.3, -0.3, -1)

// Voor dramatisch effect:
Light Direction: (-1, -0.5, -0.5)
Light Intensity: 1.5
```

### **Texture Sizes**
- **64px**: Mini icons, inventory slots
- **128px**: Standard UI buttons
- **256px**: Large UI elements, portraits
- **512px**: High-detail showcase images

### **Background Colors**
- **Clear (0,0,0,0)**: Transparante achtergrond voor UI
- **White**: Voor items die op donkere backgrounds komen
- **Gradient Grey**: Voor consistent appearance

## 🐛 Troubleshooting

### **Prefab niet zichtbaar**
- **Probleem**: Prefab valt buiten camera view
- **Oplossing**: Pas `Camera Position` en `Orthographic Size` aan
- **Check**: Bounds van je prefab in Scene view

### **Sprite te donker/licht**
- **Probleem**: Lighting niet optimaal
- **Oplossing**: Verhoog `Light Intensity` of pas `Light Direction` aan
- **Alternatief**: Wijzig material van je prefab

### **Transparantie werkt niet**
- **Probleem**: Background color niet op Clear
- **Oplossing**: Zet `Background Color` naar `(0,0,0,0)`
- **Check**: Material van prefab ondersteunt transparantie

### **Lage kwaliteit sprites**
- **Probleem**: Texture size te laag
- **Oplossing**: Verhoog `Texture Size` naar 512 of hoger
- **Check**: `Texture Format` op RGBA32

### **Batch conversion faalt**
- **Probleem**: Eén prefab veroorzaakt errors
- **Oplossing**: Gebruik `Test First Prefab` om probleem te vinden
- **Alternatief**: Zet `Stop on Error` uit

## 🔧 Advanced Usage

### **Custom Output Paths**
```csharp
// Organiseer sprites per categorie:
outputFolder = "Assets/UI/Icons/Weapons"
spritePrefix = "Weapon_"

outputFolder = "Assets/UI/Icons/Buildings"  
spritePrefix = "Building_"
```

### **Layer-based Rendering**
```csharp
// Render alleen UI layer:
cullingMask = LayerMask.GetMask("UI")

// Exclude bepaalde layers:
cullingMask = ~LayerMask.GetMask("Hidden", "PostProcessing")
```

### **Multiple Angles**
Voor meerdere hoeken van hetzelfde prefab:
1. Maak verschillende camera positions
2. Gebruik custom names: `"Object_Front"`, `"Object_Side"`, etc.
3. Run batch converter met verschillende settings

### **Integration met Code**
```csharp
// Laad gegenereerde sprites in runtime:
Sprite weaponIcon = Resources.Load<Sprite>("UI/Icons/Weapons/Sprite_Sword");
myButton.image.sprite = weaponIcon;
```

## 📁 File Structure
```
Assets/
├── Scripts/
│   └── Editor/
│       ├── PrefabToSpriteConverter.cs
│       └── BatchSpriteConverter.cs
├── Generated Sprites/
│   ├── Sprite_Prefab1.png
│   ├── Sprite_Prefab2.png
│   └── ...
└── Resources/
    └── UI/
        └── Icons/
            └── (organizational subfolders)
```

## 🚀 Performance Tips

### **Voor Grote Batches**
- Gebruik lagere texture sizes eerst om settings te testen
- Verhoog texture size alleen voor final output
- Gebruik `Show Progress Bar` om voortgang te volgen

### **Memory Management**
- Tools cleanen automatisch temporary objects
- Unity garbage collector handelt texture cleanup af
- Sluit tools na gebruik voor memory vrijgave

### **Build Optimization**
- Editor scripts worden niet in builds geïncludeerd
- Gegenereerde sprites zijn normale assets
- Gebruik texture compression settings voor builds

## 📞 Support

Als je problemen hebt:
1. Check Console voor error messages
2. Gebruik `Test First Prefab` voor debugging
3. Verifieer prefab setup in Scene view
4. Check output folder permissions

De tools zijn ontworpen om robuust te zijn, maar mocht je toch errors tegenkomen, dan helpen de debug messages in de Console je verder! 