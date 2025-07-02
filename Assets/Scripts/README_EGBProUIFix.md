# EGB Pro 2 UI Input Fix

Dit document beschrijft de oplossing voor de inconsistente selectie UI menu problemen in EGB Pro 2 wanneer VR raycast input wordt gebruikt.

## Het Probleem

Het EGB Pro 2 selectie UI menu vertoont inconsistent gedrag:
- Het menu pop't soms wel en soms niet op
- Na het klikken buiten het menu en opnieuw selecteren werkt het niet meer
- Dubbele inputs zorgen voor conflicten
- Mouse input interfereert met raycast input

## De Oplossing

Er zijn twee scripts gemaakt die samen alle problemen oplossen:

### 1. EGBProUIInputFixer.cs
**Hoofdscript dat direct het selector panel beheert**

**Features:**
- Blokkeert mouse input voor UI wanneer alleen raycast gebruikt wordt
- Voorkomt dubbele input events met cooldown timer
- Dwingt consistent selector panel gedrag af
- Sluit automatisch panels bij click-outside
- Timeout functionaliteit voor panels

### 2. VRUIInputPatch.cs  
**Ondersteunend script voor algemene VR UI verbeteringen**

**Features:**
- Schakelt conflicterende Input Modules uit
- Zorgt voor consistente ray-based UI interaction
- Algemene UI state management
- Event cooldown voor alle UI interacties

## Setup Instructies

### Stap 1: Scripts Toevoegen
1. Voeg `EGBProUIInputFixer.cs` toe aan hetzelfde GameObject als je `GridUIManager`
2. Voeg `VRUIInputPatch.cs` toe aan een persistent GameObject (wordt automatisch DontDestroyOnLoad)

### Stap 2: EGBProUIInputFixer Configureren
```
Input Blocker Settings:
✅ Block Mouse Input For UI = true
✅ Only Use Raycast Input = true  
✅ Prevent Double Inputs = true
   Input Cooldown Time = 0.1f

UI Panel Settings:
✅ Fix Selector Panel Inconsistency = true
✅ Force Close On Click Outside = true
   Selector Panel Timeout = 0.5f

Debug:
   Show Debug Messages = false (zet op true voor troubleshooting)
```

### Stap 3: VRUIInputPatch Configureren
```
VR UI Settings:
✅ Enable VR UI Mode = true
✅ Disable Mouse Input Completely = true
✅ Use Only Raycast For UI = true

Input Event Prevention:
✅ Prevent Duplicate Events = true
   Event Cooldown Time = 0.15f

UI Consistency:
✅ Force UI Consistency = true
✅ Auto Close On Lost Focus = true

Debug:
   Debug Mode = false (zet op true voor troubleshooting)
```

## Hoe Het Werkt

### Input Conflict Oplossing
- Mouse input wordt volledig geblokkeerd voor UI elementen
- Alleen VR raycast input wordt gebruikt
- StandaloneInputModule wordt uitgeschakeld
- XRUIInputModule wordt geforceerd actief

### Selector Panel Consistency
- Panel state wordt continu gemonitored
- Automatische correctie bij inconsistenties
- Click-outside detectie met proper cleanup
- Timeout systeem voorkomt "stuck" panels

### Event Filtering
- Cooldown timer voorkomt dubbele events
- Input state tracking voorkomt conflicts
- Proper event cleanup bij selection changes

## Troubleshooting

### Probleem: Selector panel verschijnt nog steeds niet
**Oplossing:**
1. Zet Debug Messages aan in EGBProUIInputFixer
2. Check Console voor error messages
3. Controleer of BuildableObjectSelector correct gevonden wordt
4. Test met Context Menu: "Test Show Selector Panel"

### Probleem: Mouse input werkt nog steeds
**Oplossing:**
1. Controleer VRUIInputPatch settings
2. Zet "Disable Mouse Input Completely" op true
3. Check of alle StandaloneInputModules uitgeschakeld zijn
4. Test met Context Menu: "Toggle VR UI Mode"

### Probleem: UI reageert helemaal niet meer
**Oplossing:**
1. Zet VR UI Mode tijdelijk uit
2. Check of XRUIInputModule aanwezig en actief is
3. Controleer VRMouseInterceptor settings
4. Test met Context Menu: "Force UI Reset"

### Probleem: Compile errors over BuildableObjectSelector API
**Oplossing:**
1. Controleer dat je de nieuwste versie van EGB Pro 2 hebt
2. Check of `OnBuildableObjectSelected` en `OnBuildableObjectDeselected` events bestaan
3. Probeer de scripts opnieuw te compileren
4. Als events anders heten, pas de script aan naar de juiste event names

## Context Menu Functies

Beide scripts hebben handige Context Menu functies voor testing:

**EGBProUIInputFixer:**
- Test Hide Selector Panel
- Test Show Selector Panel  
- Toggle Raycast Only Mode

**VRUIInputPatch:**
- Toggle VR UI Mode
- Force UI Reset
- Test UI Input

## Script Dependencies

Deze scripts hebben de volgende dependencies:
- `VRMouseInterceptor.cs` (voor VR mouse positie)
- `GridUIManager.cs` (EGB Pro 2 UI manager)
- `BuildableObjectSelector.cs` (EGB Pro 2 selector)
- Unity XR Interaction Toolkit
- System.Reflection (voor toegang tot private methods)

**Belangrijk:** De scripts gebruiken de correcte EGB Pro 2 API:
- `OnBuildableObjectSelected` event
- `OnBuildableObjectDeselected` event 
- `GetSelectedObjectsList()` method
- Reflection voor `ResetIndividualSelection()` (private method)

## Performance Impact

Beide scripts zijn geoptimaliseerd voor minimale performance impact:
- Update loops zijn efficiënt
- Alleen noodzakelijke checks worden uitgevoerd
- Event-driven architecture waar mogelijk
- Geen continue raycasting

## Testen

1. Start je VR scene
2. Selecteer objecten met raycast
3. Controleer dat selector panel consistent verschijnt
4. Test click-outside functionaliteit
5. Controleer dat panel automatisch sluit bij deselection

Het probleem zou nu opgelost moeten zijn! 🎉 