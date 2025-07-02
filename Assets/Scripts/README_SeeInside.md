# See Inside Teleporter - VR Object Teleportation

Dit systeem biedt "See Inside" functionaliteit voor je VR applicatie, waarmee spelers kunnen teleporteren naar binnen in cel/halfcel objecten met Grid Builder Pro 2.

## Overzicht

Het systeem bestaat uit vier hoofdcomponenten:
1. **SeeInsideTeleporter.cs** - De core teleportatie logica
2. **SimpleSeeInsideUI.cs** - Eenvoudige UI interface voor de "See Inside" knop
3. **ObjectSelectionHandler.cs** - Automatische koppeling tussen Grid Builder Pro selectie en UI
4. **XRExitHandler.cs** - XR controller input voor het exiten van "See Inside" perspectief

## Features

- ✅ Teleporteer naar binnen in cel/halfcel objecten
- ✅ Camera FOV verandert naar 90 graden voor beter perspectief
- ✅ Automatische speler schaling (15% van object grootte)
- ✅ Positie in het midden/onderin het object
- ✅ Grid visualisatie wordt automatisch verborgen
- ✅ Object colliders worden uitgeschakeld om clipping te voorkomen
- ✅ **Controller positie compensatie** voor FOV veranderingen
- ✅ **XR controller input** voor het exiten met VR knoppen
- ✅ Automatische object selectie via Grid Builder Pro events
- ✅ Eenvoudige setup zonder ingewikkelde configuratie
- ✅ Compatibel met Grid Builder Pro 2 objecten

## Installatie

### Stap 1: SeeInsideTeleporter toevoegen

1. Maak een nieuw leeg GameObject in je scene en noem het "SeeInsideTeleporter"
2. Voeg het `SeeInsideTeleporter.cs` script toe aan dit GameObject
3. In de Inspector:
   - **XR Origin**: Sleep je XR Origin GameObject hiernaartoe
   - **XR Camera**: Sleep de Main Camera van je XR Origin hiernaartoe (wordt automatisch gevonden)
   - **Inside FOV**: Stel in op 90 (aanbevolen voor beter perspectief)
   - **Player Scale Percentage**: 0.15 (15% van object grootte)
   - **Height Percentage**: 0.4 (40% van de hoogte, dus laag in het object)
   - **Compensate Controller Positions**: ✅ (corrigeert controller posities voor FOV verandering)
   - **Controller Compensation Factor**: 0.7 (hoeveel dichter controllers bij camera komen)

### Stap 2: "See Inside" knop maken

1. Voeg het `SimpleSeeInsideUI.cs` script toe aan je "See Inside" Button
2. In de Inspector:
   - **Teleporter**: Wordt automatisch gevonden
   - **See Inside Button**: Wordt automatisch gevonden
   - **Button Text**: Wordt automatisch gevonden
   - **Selected Object**: Wordt automatisch ingesteld door ObjectSelectionHandler

### Stap 3: Automatische Selectie Setup

1. Maak een nieuw leeg GameObject in je scene en noem het "ObjectSelectionHandler"
2. Voeg het `ObjectSelectionHandler.cs` script toe aan dit GameObject
3. In de Inspector:
   - **See Inside UI**: Sleep je SimpleSeeInsideUI component hiernaartoe
   - **Only Cell Objects**: Laat aan staan (✅) voor alleen cel/halfcel objecten
   - **Enable Debug Logging**: Voor debugging (optioneel)

### Stap 4: XR Exit Handler Setup (Optioneel)

Voor het exiten met VR controller knoppen:

1. Maak een nieuw leeg GameObject in je scene en noem het "XRExitHandler"
2. Voeg het `XRExitHandler.cs` script toe aan dit GameObject
3. In de Inspector:
   - **See Inside Teleporter**: Wordt automatisch gevonden
   - **Use Menu Button**: ✅ (standaard - Menu knop om te exiten)
   - **Only Exit When Inside**: ✅ (voorkomt ongewenst exiten)
   - **Hand Selection**: Both (luistert naar beide controllers)

### Stap 5: Testing

1. **Spawn een cel/halfcel object** met Grid Builder Pro 2
2. **Klik op het cel object** → Menu verschijnt automatisch
3. **Klik op "See Inside"** → Automatische teleportatie! 🎮
4. **Druk Menu knop op VR controller** → Exit automatisch uit het perspectief! 🎮

Het systeem werkt nu volledig automatisch - geen handmatige target configuratie meer nodig!

## Gebruik

### Automatische Target Detectie
- Richt je camera op een cel/halfcel object
- De "See Inside" knop wordt groen als er een geldig target is
- Klik op de knop om naar binnen te teleporteren
- De knop wordt rood en toont "Exit" wanneer je binnen bent
- Klik nogmaals om naar buiten te gaan

### Handmatige Target Instelling
```csharp
// Vind de UI component
SeeInsideUI seeInsideUI = FindObjectOfType<SeeInsideUI>();

// Stel handmatig een target in
seeInsideUI.SetManualTarget(mijnCelObject);
seeInsideUI.SetAutoDetectTarget(false);
```

### Script-gebaseerd gebruik
```csharp
// Vind de teleporter
SeeInsideTeleporter teleporter = FindObjectOfType<SeeInsideTeleporter>();

// Ga naar binnen in een object
teleporter.SeeInside(targetGameObject);

// Ga naar buiten
teleporter.ExitObject();

// Controleer of je binnen bent
if (teleporter.IsInsideObject)
{
    Debug.Log("Je bent binnen in: " + teleporter.CurrentInsideObject.name);
}
```

## Instellingen Aanpassen

### SeeInsideTeleporter Instellingen

| Instelling | Beschrijving | Aanbevolen Waarde |
|------------|-------------|-------------------|
| Inside FOV | Camera perspectief binnen object | 90 |
| Player Scale Percentage | Speler grootte als % van object | 0.15 (15%) |
| Height Percentage | Hoogte positie in object | 0.4 (40%) |
| Hide Grid When Inside | Grid verbergen binnen object | true |
| Valid Object Tags | Tags voor geldige objecten | ["Cell", "HalfCell", "BuildableObject"] |
| Show Debug Info | Console debug informatie | true (tijdens development) |

### SeeInsideUI Instellingen

| Instelling | Beschrijving | Standaard Waarde |
|------------|-------------|------------------|
| Enter Text | Knop tekst voor "ga naar binnen" | "See Inside" |
| Exit Text | Knop tekst voor "ga naar buiten" | "Exit" |
| Auto Detect Target | Automatische target detectie | true |
| Target Layers | Layers voor target detectie | All (-1) |
| Max Detection Distance | Maximum detectie afstand | 10 meter |
| Valid Target Color | Knop kleur bij geldig target | Groen |
| Invalid Target Color | Knop kleur bij geen target | Grijs |
| Inside Object Color | Knop kleur binnen object | Rood |

## Troubleshooting

### "Geen XR Origin gevonden"
- Zorg ervoor dat je een XR Origin in je scene hebt
- Assign de XR Origin handmatig in de SeeInsideTeleporter Inspector

### "Geen camera gevonden"
- Controleer of je XR Origin een Camera child heeft
- De camera moet de tag "MainCamera" hebben

### "Object is geen geldig teleportatie doel"
- Controleer of het object de juiste tags heeft
- Zorg ervoor dat het object een `BuildableGridObject` component heeft
- Controleer of de objectnaam "cel", "cell" of "half" bevat

### Knop reageert niet
- Controleer of de Button component correct is toegewezen
- Zorg ervoor dat de Button interactable is
- Controleer of er een geldig target object is

### Speler wordt verkeerd gepositioneerd
- Pas de Height Percentage aan (0.1 = heel laag, 0.9 = heel hoog)
- Controleer of het object Renderer of Collider components heeft voor bounds berekening
- Kijk naar de console logs voor debug informatie

### Camera clipping problemen
- Object colliders worden automatisch uitgeschakeld, maar kunnen handmatig worden beheerd
- Pas de Player Scale Percentage aan voor betere grootte verhouding

### Grid blijft zichtbaar
- Controleer of Hide Grid When Inside aanstaat
- Zorg ervoor dat je `EasyGridBuilderPro` components in de scene hebt

## Integratie met Bestaand Systeem

### UniversalButton Integration
```csharp
// In je UniversalButtonManager of soortgelijk systeem
public void OnSeeInsideButtonPressed()
{
    SeeInsideTeleporter teleporter = FindObjectOfType<SeeInsideTeleporter>();
    
    if (teleporter.IsInsideObject)
    {
        teleporter.ExitObject();
    }
    else
    {
        // Gebruik je bestaande object selectie logica
        GameObject selectedObject = GetCurrentSelectedObject();
        teleporter.SeeInside(selectedObject);
    }
}
```

### Menu Integration
Het systeem kan eenvoudig worden geïntegreerd met je bestaande menu systemen door de publieke methodes van `SeeInsideTeleporter` aan te roepen.

## Geavanceerd Gebruik

### Runtime Instellingen Wijzigen
```csharp
SeeInsideTeleporter teleporter = FindObjectOfType<SeeInsideTeleporter>();

// Wijzig instellingen tijdens runtime
teleporter.UpdateSettings(
    newFOV: 120f,           // Nieuwe FOV
    newPlayerScale: 0.2f,   // Nieuwe schaal (20%)
    newHeight: 0.5f         // Nieuwe hoogte (50%)
);
```

### Custom Target Validatie
Je kunt de target validatie uitbreiden door de `IsValidTeleportTarget` methode in `SeeInsideTeleporter` aan te passen.

### Event Callbacks
Het systeem kan worden uitgebreid met UnityEvents voor callbacks wanneer de speler naar binnen/buiten gaat.

## Performance Overwegingen

- Target detectie gebeurt elke frame - overweeg een lagere update frequentie voor performance
- Grid hiding/showing kan impact hebben op performance bij veel grid objecten
- Object bounds berekening wordt cached per object

## Veelgestelde Vragen

**Q: Kan ik meerdere objecten tegelijk binnengaan?**
A: Nee, je kunt slechts in één object tegelijk zijn. Het systeem verlaat automatisch het huidige object als je een nieuw object binnenging.

**Q: Werkt het met alle Grid Builder Pro 2 objecten?**
A: Ja, alle objecten met een `BuildableGridObject` component worden ondersteund.

**Q: Kan ik de teleportatie animeren?**
A: Momenteel is er geen animatie, maar je kunt het systeem uitbreiden met smooth teleportatie door `TeleportXROrigin` aan te passen.

**Q: Wat gebeurt er met object physics?**
A: Object colliders worden tijdelijk uitgeschakeld om camera clipping te voorkomen en worden weer ingeschakeld bij exit. 