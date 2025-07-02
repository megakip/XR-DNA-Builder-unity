# XR DNA Builder - README

## 📝 Project Overview

XR DNA Builder is een interactieve Virtual Reality applicatie voor Meta/Oculus headsets waarmee gebruikers 3D objecten kunnen plaatsen, manipuleren en bouwen op een grid-systeem. De applicatie maakt gebruiker van zowel AR als VR (XR in het kort), en biedt een interface die gekoppeld is aan je voor waarmee je objecten kan selecteren die vervolgens op het grid geplaatst kunnen worden. In de XR applicatie wordt ook bijgehouden wat je op het grid hebt geplaatst in het informatiepaneel zodat je bijvoorbeeld kan zien hoeveel materiaal je hebt gebruikt. 

## 🎯 Key Features

- Grid-based Building System: Plaats objecten op een smart grid met automatische snapping
- VR Controller Interactie: Natuurlijke hand/controller-based interacties
- Mixed Reality Support: Passthrough functionaliteit voor AR-achtige ervaring
- Google Maps integratie voor location-based building

## 🛠️ Unity versie en add ons

### Frontend (VR)

- Unity 6000.0.35f1 - Game engine
- XR Interaction Toolkit - VR interacties
- Meta/Oculus SDK - Passthrough en VR functionaliteit
- Easy Grid Builder Pro 2 - Grid systeem (Unity Asset Store)

### UI & Graphics

- Unity UGUI - User interface systeem
- TextMesh Pro - Tekst rendering
- Translucent Image Blur - UI background effects
- Figma converter
- True shadow

## 📺 Installation & Setup

- Unity 6000.0.35f1 (wellicht is het beter om over te stappen naar een nieuwere versie)
- Meta XR SDK setup in Unity (Er zijn wel performance issues ontstaan toen ik de SDK net installeerde maar kan zijn dan ik het niet goed heb geconfigureerd)
- XRI Interaction toolkit (XRI heb ik door het hele project gebruikt)
- Easy Grid Builder Pro 2 van Unity Asset Store (is wel een betaalde add on maar het bied een stabiel grid systeem)

## ⚠️ Known Issues & Limitations

### Current Bugs

- VR Integration: Easy Grid Builder Pro 2 heeft geen native VR support
- Vertical Building: Automatische verdieping detectie is buggy in VR omdat het geen EGB2 geen native vr support heeft.
- Performance: Meta SDK veroorzaakt performance issues
- Mixed Reality: Skybox blijft gedeeltelijk zichtbaar tijdens passthrough
- Hand interactie werkt niet meer omdat ik meta SDK heb toegevoegd en Easy grid builder Pro 2 heeft een point en klik systeem (geen grab systeem zoals in de video’s)

## 👨‍💻 Developer Handoff Notes

### Code Architecture

- VR interacties zijn gescheiden van grid logica (Want met AI heb ik mouse inputs vertaald naar raycast inputs op basis van waar je richt met je controller)
- UI systeem gebruikt traditional UGUI maar sinds de 6.2 beta is UI toolkit ook te gebruiken in world space. Ook in de XRI project sample is dit te testen
- 3D modellen pipeline: SketchUp → Blender → Unity
- Het Unity project is wel een beetje rommelig en ben zelf geen game developer maar hoop genoeg context te bieden om een goed idee te krijgen wat er is gebruikt voor dit project en wat de bedoeling is van de applicatie. Zelf heb ik veel met Cursor en Claude gewerkt (in combinatie met Unity MCP server) en dat is misschien ook wel te zien.

### Belangrijke Asset Store Links

Easy Grid Builder Pro 2:

[https://assetstore.unity.com/packages/tools/game-toolkits/easy-grid-builder-pro-2-modular-building-system-298820](https://assetstore.unity.com/packages/tools/game-toolkits/easy-grid-builder-pro-2-modular-building-system-298820)

Documentatie:

[https://soulgamess-studio.gitbook.io/easy-grid-builder-pro-2-documentation/walk-through-tutorial/make-a-2d-city-builder/setting-up-the-scene](https://soulgamess-studio.gitbook.io/easy-grid-builder-pro-2-documentation/walk-through-tutorial/make-a-2d-city-builder/setting-up-the-scene)

## 🎥 Demo Video's

Er zijn twee demo video's beschikbaar die de functionaliteit tonen:

- Eerste prototype: Object grab & drop systeem
- Huidige versie: Controller pointing & grid placement (eerste systeem voelde wat speelser en je kon meteen het object goed roteren. Tweede systeem vereiste minder beweging maar kan niet direct roteren)

---