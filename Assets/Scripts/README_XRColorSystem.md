# XR Color Picking System

Een complete oplossing voor het selecteren van kleuren met een color picker en het kleuren van objecten in VR met XR controllers.

## Overzicht

Dit systeem bestaat uit vier hoofdcomponenten:

1. **XRControllerColorManager** - Beheert de kleur van de XR controller lijn en handelt object kleuring af
2. **ColorableObject** - Component voor objecten die gekleurd kunnen worden  
3. **BuildableObjectColorableScriptAdder** - Automatische integratie met EGB Pro 2 Grid systeem
4. **XRColorSystemDemo** - Demo script voor automatische setup en testing

## Functionaliteit

- ✅ Color picker integratie (HSVColorPicker, SimpleColorPicker, SpectrumColorPicker)
- ✅ XR controller lijn krijgt de kleur van de color picker
- ✅ Trigger input om objecten te kleuren
- ✅ Ondersteunt zowel XRRayInteractor als NearFarInteractor
- ✅ **EGB Pro 2 Grid automatische integratie** - ColorableObject scripts worden automatisch toegevoegd
- ✅ Automatische component detectie
- ✅ Visuele effecten bij kleuring
- ✅ Kleur persistentie tussen sessies

## Setup Instructies

### Stap 1: XR Controller Setup

1. Voeg het **XRControllerColorManager** script toe aan je XR controller GameObject (bijvoorbeeld de RightHandController)

2. Configureer de referenties in de inspector:
   - **Ray Interactor**: Je XRRayInteractor component
   - **Line Visual**: Je XRInteractorLineVisual component  
   - **Near Far Interactor**: Je NearFarInteractor component (indien aanwezig)
   - **Trigger Input Action**: Referentie naar je trigger input action

3. Stel de layer mask in voor objecten die gekleurd kunnen worden

### Stap 2: EGB Pro 2 Grid Integratie Setup

1. Voeg het **BuildableObjectColorableScriptAdder** script toe aan een GameObject in je scene
2. Dit script luistert automatisch naar EGB Pro object placements en voegt ColorableObject scripts toe
3. **Configureer je BuildableObjectSO assets**:
   - Open je BuildableGridObjectSO, BuildableFreeObjectSO, etc. in de inspector
   - Ga naar de **Color System** sectie (onderaan)
   - ✅ **Add Colorable Object Script**: Standaard AAN - voegt automatisch ColorableObject toe bij spawning
   - **Enable Color Effects**: Visual effecten bij kleuren
   - **Save Color State**: Kleur persistentie tussen sessies

### Stap 3: Color Picker Setup

Je color picker (HSVColorPicker, SimpleColorPicker, of SpectrumColorPicker) werkt automatisch met het systeem via de static OnColorChanged events. Geen extra setup vereist!

### Stap 4: Handmatige Objecten Colorable Maken (Optioneel)

Voor objecten die NIET via EGB Pro worden geplaatst:

1. Voeg het **ColorableObject** script toe aan het GameObject
2. Het script detecteert automatisch Renderer, UI Image, of SpriteRenderer components
3. Optioneel: configureer visual effects zoals particle systems en scale effecten

### Stap 5: Demo Setup (Optioneel)

1. Voeg het **XRColorSystemDemo** script toe aan een leeg GameObject in je scene
2. Zet **Auto Setup** en **Create Test Objects** aan
3. Start je scene - het demo script zal automatisch alles configureren en test objecten maken

## Input Setup

### Voor OpenXR/XR Interaction Toolkit:

1. Ga naar je Input Actions Asset
2. Zoek de "Select" action voor je controller
3. Assign deze action aan de **Trigger Input Action** van de XRControllerColorManager

### Alternative Input Setup:

Je kunt ook handmatig input afhandelen door de `ColorObjectAtRayHit()` methode aan te roepen vanuit je eigen input handling code.

## Gebruik

1. **Kleur Selecteren**: Gebruik je color picker om een kleur te selecteren
2. **Lijn Kleur**: De XR controller lijn krijgt automatisch de geselecteerde kleur
3. **Object Kleuren**: Richt op een object en druk de trigger in om het object te kleuren

## Code Voorbeelden

### Handmatig een kleur instellen:
```csharp
XRControllerColorManager colorManager = FindObjectOfType<XRControllerColorManager>();
colorManager.SetSelectedColor(Color.red);
```

### Een object direct kleuren:
```csharp
ColorableObject colorableObj = someGameObject.GetComponent<ColorableObject>();
colorableObj.ChangeColor(Color.blue);
```

### Luisteren naar kleur veranderingen:
```csharp
ColorableObject colorableObj = GetComponent<ColorableObject>();
colorableObj.OnColorChanged += (Color newColor) => {
    Debug.Log($"Object colored with: {newColor}");
};
```

## Troubleshooting

### "No XRInteractorLineVisual found!"
- Controleer of je XR controller een XRInteractorLineVisual component heeft
- Gebruik het demo script om automatische setup uit te voeren

### "No trigger input action assigned"
- Assign een Input Action Reference voor de trigger in de XRControllerColorManager
- Zorg dat de action enabled is in je Input Action Asset

### Objects worden niet gekleurd
- Controleer of het object een ColorableObject component heeft
- Controleer of het object op de juiste layer staat (colorableLayerMask)
- Controleer of het object binnen de maxColoringDistance staat

### Color picker werkt niet
- Controleer of je color picker script een OnColorChanged event heeft
- Het systeem ondersteunt HSVColorPicker, SimpleColorPicker, en SpectrumColorPicker automatisch

## Aanpassingen

### Aangepaste Color Picker
Om een aangepaste color picker te ondersteunen, voeg een OnColorChanged event toe:

```csharp
public static event System.Action<Color> OnColorChanged;

// Roep dit aan wanneer de kleur verandert:
OnColorChanged?.Invoke(newColor);
```

### Aangepaste Object Kleuring
Override de `ColorObject` methode in XRControllerColorManager voor aangepaste kleuring logica.

### Aangepaste Visual Effects
Configureer de visual effects in het ColorableObject component:
- Particle systems
- Audio clips  
- Scale effecten
- Kleur persistentie

## Performance Tips

- Gebruik layer masks om raycast performance te verbeteren
- Zet debug logging uit in productie builds
- Overweeg object pooling voor veel colorable objects

## Compatibiliteit

- ✅ Unity 2022.3+
- ✅ XR Interaction Toolkit 3.0.7
- ✅ OpenXR 1.14.0
- ✅ Meta XR SDK
- ✅ VR Template Assets

---

Voor vragen of problemen, check de debug output in de Console en gebruik de context menu opties in het XRColorSystemDemo script voor diagnose. 