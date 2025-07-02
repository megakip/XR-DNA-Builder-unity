# Meta SDK Map Achtergrond Opacity Fix

## Probleem
Na overstap naar Meta SDK werkt de map achtergrond opacity niet meer correct. Dit komt doordat:

1. **XR Canvas Setup**: World Space canvassen hebben een andere camera setup nodig
2. **Event Camera**: Meta SDK vereist specifieke camera referenties voor UI interactie
3. **Canvas Positioning**: Canvassen moeten correct gepositioneerd worden voor VR

## Oplossing

### Stap 1: Gebruik het nieuwe MetaSDKCompatibleMapController script

Replace je oude `GridBackgroundOpacityController` met het nieuwe `MetaSDKCompatibleMapController` script.

### Stap 2: Canvas Setup

1. **Canvas RenderMode**: Zorg dat je Canvas op `World Space` staat
2. **Event Camera**: Het script stelt dit automatisch in
3. **Canvas Scale**: Gebruik een kleine scale waarde (bijv. 0.001) voor VR

### Stap 3: Script Configuration

```csharp
// In de Inspector:
[Header("Background Image Settings")]
backgroundImage = // Je kaart Image component
backgroundRawImage = // Of je kaart RawImage component

[Header("Opacity Control")]  
opacitySlider = // Je opacity slider

[Header("XR & Meta SDK Settings")]
autoSetupXRCamera = true // Laat automatisch instellen
canvasDistance = 2.0f // Afstand van camera
canvasScale = 0.001f // VR canvas schaal
```

### Stap 4: Setup in Unity Editor

1. Voeg `MetaSDKCompatibleMapController` toe aan je Canvas GameObject
2. Sleep je kaart Image/RawImage naar het `backgroundImage` veld
3. Sleep je opacity Slider naar het `opacitySlider` veld
4. Laat de XR instellingen op default staan

## Veelvoorkomende Problemen & Oplossingen

### Canvas niet zichtbaar in VR
- **Probleem**: Canvas staat te dichtbij of te ver weg
- **Oplossing**: Pas `canvasDistance` aan (aanbevolen: 1.5-3.0)

### Slider werkt niet in VR
- **Probleem**: Geen Event Camera ingesteld
- **Oplossing**: Script stelt automatisch in, of handmatig `canvas.worldCamera` instellen

### Canvas te groot/klein
- **Probleem**: Canvas schaal niet goed voor VR
- **Oplossing**: Pas `canvasScale` aan (aanbevolen: 0.0005-0.002)

### Opacity werkt niet
- **Probleem**: Geen Image component toegewezen
- **Oplossing**: Controleer of `backgroundImage` of `backgroundRawImage` is ingesteld

## Code Voorbeeld voor Manual Setup

```csharp
// Als je handmatig wilt instellen:
var mapController = GetComponent<MetaSDKCompatibleMapController>();

// Stel opacity in
mapController.SetOpacity(0.5f); // 50% doorzichtig

// Fade naar nieuwe opacity
mapController.FadeToOpacity(0.8f, 2.0f); // Fade naar 80% in 2 seconden

// Reset naar default
mapController.ResetToDefault();
```

## Debugging

Schakel `enableDebugLogging` in om te zien wat er gebeurt:

```
Canvas render mode ingesteld op World Space
XR Camera gevonden en ingesteld: Main Camera
Canvas gepositioneerd op (0.0, 1.8, 2.0)
Opacity slider geïnitialiseerd met waarde 0.5
Map Image opacity ingesteld op 0.5
```

## Meta SDK Specifieke Overwegingen

- **OVR Camera Rig**: Script werkt met beide oude en nieuwe XR Origin setups
- **Passthrough**: Werkt samen met de PassthroughToggleController
- **Hand Tracking**: Canvas is compatible met hand tracking interacties
- **Controllers**: Werkt met beide Touch controllers en hand tracking

## Troubleshooting Script Errors

### "Geen XR Origin gevonden"
1. Controleer of je Meta SDK correct is geïnstalleerd
2. Zorg dat je een OVR Camera Rig of XR Origin in je scene hebt
3. Controleer of het script na de XR Origin wordt geladen

### "Geen Canvas component gevonden"
- Voeg het script toe aan een GameObject met een Canvas component

### Canvas positioneert verkeerd
- Gebruik `mapController.UpdateCanvasPosition()` om positie te updaten
- Controleer of de XR camera correct is ingesteld

## Performance Tips

- Gebruik `continuousUpdate = false` als de canvas niet hoeft te bewegen
- Zet `enableDebugLogging = false` in production builds
- Gebruik een lagere `canvasScale` voor betere performance

## Integratie met Bestaande Code

Het script is backwards compatible:

```csharp
// Oude code blijft werken:
SetOpacity(0.7f);
GetOpacity();
ResetToDefault();

// Nieuwe VR-specifieke functies:
UpdateCanvasPosition();
RefreshCanvasSetup();
``` 