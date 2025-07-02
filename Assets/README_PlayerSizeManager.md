# PlayerSizeManager - Advanced Player Scaling in XR

This component allows fine-grained control over player scale, camera field of view, and vertical position when entering objects in VR. It enhances the existing "Go Inside" functionality by providing adjustable parameters for player size and perspective, and ensures proper positioning INSIDE objects.

## Features

- Control player scale as a percentage of the object size
- Adjust camera field of view for different perspectives
- Control player vertical position (height) inside objects
- Ensures player is placed at the center of the object, not on top or beside it
- Automatically hides grid visualization when inside objects
- UI sliders for intuitive control
- Automatic integration with existing "Go Inside" functionality
- Smooth transitions between inside and outside views

## Important: Proper Inside Positioning

The main advantage of this component is that it properly positions the player **INSIDE** objects:

- Uses advanced detection of object bounds using renderers, colliders, or transform data
- Calculates the true center point of the object
- Places the player at the calculated center point, not on top or beside the object
- Provides height controls to adjust vertical position within the object
- Works with complex objects that have multiple renderers or colliders
- Automatically hides grid visualization for better viewing inside objects

## Setup Instructions

### 1. Add the PlayerSizeManager to Your Scene

1. Create a new empty GameObject in your scene and name it "PlayerSizeManager"
2. Add the `PlayerSizeManager.cs` script to this GameObject
3. The manager will automatically find your XR Origin at runtime, but you can also assign it directly

### 2. Create UI Controls (Option 1: Using Prefab)

*Note: Prefab to be created separately*

1. Drag the `PlayerSizeUI_Prefab` (to be created) into your world-space XR Canvas
2. Position it in a convenient location in your VR environment
3. The prefab will automatically find and connect to the PlayerSizeManager

### 3. Create UI Controls (Option 2: Manual Setup)

1. In your world-space XR Canvas, create a new Panel (this will hold the controls)
2. Add the following UI elements to the panel:
   - Title Text (TMP_Text)
   - Size Slider (Slider) with value text (TMP_Text)
   - FOV Slider (Slider) with value text (TMP_Text)
   - Height Slider (Slider) with value text (TMP_Text)
   - Exit Button (Button)
3. Add the `PlayerSizeUI.cs` script to the panel
4. Link the UI elements to their respective fields in the PlayerSizeUI component

### 4. Configure PlayerSizeManager Settings

The PlayerSizeManager has several customizable settings:

- **Size Settings**:
  - `defaultSizePercentage`: Default size when entering an object (as % of object size)
  - `minSizePercentage`: Minimum allowed player size
  - `maxSizePercentage`: Maximum allowed player size

- **FOV Settings**:
  - `defaultFOV`: Default camera field of view when entering an object
  - `minFOV`: Minimum allowed FOV
  - `maxFOV`: Maximum allowed FOV
  
- **Height Settings**:
  - `defaultHeightPercentage`: Default height when entering an object (0 = bottom, 0.5 = middle, 1 = top)
  - `minHeightPercentage`: Minimum allowed height
  - `maxHeightPercentage`: Maximum allowed height
  
- **Grid Settings**:
  - `hideGridWhenInside`: Whether to automatically hide the grid when inside objects (default: true)

## Usage

The PlayerSizeManager integrates automatically with the existing `ObjectMenuController` script. When a player selects "Go Inside" on an object, it will:

1. Detect if PlayerSizeManager is present in the scene
2. If found, use the PlayerSizeManager for the "Go Inside" functionality with enhanced controls
3. If not found, fall back to the original implementation (which may not position correctly INSIDE objects)
4. Automatically hide any grid visualization systems in the scene (if enabled)

### Manual Control

You can also control the PlayerSizeManager through script:

```csharp
// Find the manager
PlayerSizeManager manager = FindObjectOfType<PlayerSizeManager>();

// Enter an object
manager.EnterObject(targetGameObject);

// Set specific size (as percentage of object size)
manager.SetPlayerSize(0.1f); // 10% of object size

// Set specific FOV
manager.SetCameraFOV(90f);

// Set specific height (as percentage of object height)
manager.SetPlayerHeight(0.05f); // 5% from the bottom of the object

// Exit the object
manager.ExitObject();
```

## Grid Visualization Control

When you enter an object, the PlayerSizeManager automatically:

1. Finds all `GridSystem` components in the scene
2. Stores their current visibility state
3. Hides all grid visualizations for a cleaner view inside the object
4. Restores the original visibility state when you exit the object

This feature can be disabled by setting `hideGridWhenInside` to false in the Inspector.

## Troubleshooting

- **UI not appearing**: Make sure the control panel is assigned in the PlayerSizeManager and is part of a world-space canvas
- **Size controls not working**: Verify the sliders are properly connected to the PlayerSizeManager
- **Camera clipping issues**: Adjust the minimum size percentage to ensure the player remains properly scaled for the object size
- **Player positioned incorrectly**: Make sure you're using the PlayerSizeManager (not the original positioning system). Use the height slider to adjust the vertical position inside the object.
- **Not inside the object**: If you're not properly inside the object, check the console logs. The PlayerSizeManager provides detailed logs about how it's detecting and positioning inside the object.
- **Grid still visible inside object**: Check that there are GridSystem components in your scene and that `hideGridWhenInside` is set to true in the PlayerSizeManager.

## Height Control Details

The height control allows you to adjust where within the object your character is positioned vertically:

- Low values (near 0%) place you at the bottom of the object
- Middle values (around 50%) place you in the middle of the object's height
- High values (near 100%) place you near the top of the object

This is particularly useful when:
- Your character is initially positioned too high above the object
- You need to see specific areas inside the object
- You want to align with particular features inside the object

## Complex Object Handling

The PlayerSizeManager uses a sophisticated approach to handle complex objects:

1. First tries to use the object's main renderer bounds
2. If not found, combines bounds from all child renderers
3. If no renderers, uses colliders to determine object bounds
4. As a last resort, uses transform data
5. Based on accurate bounds, positions the player at the true center of the object

## Implementation Details

When a player enters an object:

1. Original position, scale, and FOV are stored
2. Player is scaled based on object size
3. Player is positioned at the true center of the object at the appropriate height
4. Object colliders are temporarily disabled to prevent camera clipping
5. Grid visualization is hidden for a cleaner view inside the object
6. UI controls are displayed for adjusting size, FOV, and height
7. When exiting, all original values are restored including grid visibility 