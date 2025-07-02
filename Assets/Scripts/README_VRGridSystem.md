# VR Grid System Setup Guide - EGB Pro2 Integration

## Overview

This guide explains how to set up the VR Grid System with EGB Pro2 for Unity VR projects. The system provides distance-based grabbing, Y-axis rotation, and proper coordinate system synchronization for object placement.

## Components

### 1. VRGridGrabInteractable.cs
Main script for VR grid interaction with distance-based grabbing and Y-axis rotation.

### 2. EGBProCoordinateSystemSync.cs
Helper script that ensures coordinate system synchronization between the grid transform and EGB Pro2's internal calculations.

## Setup Instructions

### Step 1: Basic Setup

1. **Locate your Grid GameObject** that contains the `EasyGridBuilderProXZ` component
2. **Add the VRGridGrabInteractable script** to the same GameObject
3. **Add the EGBProCoordinateSystemSync script** to the same GameObject

### Step 2: Configure VRGridGrabInteractable

In the Inspector, configure the following settings:

#### Movement Settings
- **Minimum Drag Distance**: `0.01m` (distance required before grid becomes movable)
- **Rotation Speed**: `90f` (higher = faster rotation)
- **Smooth Rotation**: `true` (enable for smooth rotation animation)
- **Smooth Rotation Speed**: `5f` (speed of smooth rotation)

#### Grid References
- **Grid Manager**: Auto-detects if not assigned
- **Grid Builder Pro XZ**: Auto-detects if not assigned

#### Debug
- **Debug Mode**: `false` (enable for debugging)

### Step 3: Configure EGBProCoordinateSystemSync

#### Sync Settings
- **Auto Sync**: `true` (enable automatic synchronization)
- **Force Sync On Transform Change**: `true` (force sync when grid moves/rotates)
- **Transform Change Threshold**: `0.1f` (sensitivity for detecting changes)

#### Debug
- **Debug Mode**: `false` (enable for debugging)

### Step 4: XR Interaction Toolkit Setup

Ensure your XR Rig has:
1. **XR Interaction Manager** in the scene
2. **XR Ray Interactor** or **XR Direct Interactor** on your controllers
3. **Near Far Interactor** for hand tracking (if using hand tracking)

The `VRGridGrabInteractable` script will automatically add and configure an `XRGrabInteractable` component.

## How It Works

### Distance-Based Grabbing
1. User grabs the grid GameObject
2. Grid requires minimum drag distance before becoming movable
3. Prevents accidental movement during building operations
4. Returns to original position if minimum distance not reached

### Y-Axis Rotation
1. Once movable, the grid follows hand position
2. Hand rotation around Y-axis rotates the grid
3. Rotation can be smooth or instant based on settings
4. Only Y-axis rotation is allowed (no X or Z tilting)

### Coordinate System Synchronization
1. When grid rotates, coordinate system updates automatically
2. New objects placed after rotation use correct local coordinates
3. Objects are parented to the grid transform (built-in EGB Pro2 feature)
4. Local positioning ensures objects stay with rotated grid

## Troubleshooting

### Problem: Objects still place incorrectly after rotation

**Solution 1: Check Component Setup**
```csharp
// Verify both scripts are on the same GameObject as EasyGridBuilderProXZ
var gridGrab = GetComponent<VRGridGrabInteractable>();
var coordSync = GetComponent<EGBProCoordinateSystemSync>();
var egbPro = GetComponent<EasyGridBuilderProXZ>();
```

**Solution 2: Enable Debug Mode**
```csharp
// Enable debug mode in both scripts to monitor coordinate updates
debugMode = true;
```

**Solution 3: Force Synchronization**
```csharp
// Manually force coordinate system update
GetComponent<EGBProCoordinateSystemSync>().ForceSynchronization();
```

### Problem: Grid rotation is too fast/slow

**Solution: Adjust Rotation Speed**
```csharp
// In VRGridGrabInteractable
rotationSpeed = 45f; // Lower for slower rotation
rotationSpeed = 180f; // Higher for faster rotation
```

### Problem: Grabbing is too sensitive

**Solution: Increase Minimum Drag Distance**
```csharp
// In VRGridGrabInteractable
minimumDragDistance = 0.05f; // Require 5cm drag before movement
```

### Problem: Grid jumps back to original position

**Cause**: Minimum drag distance not reached
**Solution**: Lower the minimum drag distance or ensure proper grabbing technique

## Technical Details

### EGB Pro2 Coordinate System
EGB Pro2 uses the following coordinate system approach:
- Objects are parented to the grid transform: `transform.parent = occupiedGridSystem.transform`
- Positions are stored as local coordinates: `transform.localPosition = calculatedPosition`
- Rotations account for grid orientation: `transform.localEulerAngles = gridAwareRotation`

This means objects automatically follow grid rotation **when the coordinate system is properly synchronized**.

### Key Insight
The main issue was that EGB Pro2's placement calculations sometimes used cached world-space coordinates instead of recalculating based on the current grid transform. The `EGBProCoordinateSystemSync` script ensures these calculations are refreshed when the grid rotates.

## Advanced Configuration

### Custom Rotation Constraints
To modify rotation behavior, edit the `HandleRotation()` method in `VRGridGrabInteractable.cs`:

```csharp
private void HandleRotation()
{
    // Add custom constraints here
    // Example: Snap to 45-degree increments
    float snapAngle = 45f;
    targetYRotation = Mathf.Round(targetYRotation / snapAngle) * snapAngle;
}
```

### Integration with Other Systems
The coordinate system sync script sends notifications:

```csharp
// Listen for coordinate system changes
public void OnGridCoordinateSystemChanged(Transform gridTransform)
{
    // Your custom logic here
    Debug.Log($"Grid coordinate system updated: {gridTransform.name}");
}
```

## Best Practices

1. **Always test object placement after rotation** to ensure coordinates are correct
2. **Use debug mode during development** to monitor coordinate system updates
3. **Keep minimum drag distance small** (0.01-0.05m) for responsive interaction
4. **Enable smooth rotation** for better user experience
5. **Ensure proper XR setup** with interaction managers and ray interactors

## Compatibility

- **Unity Version**: 2021.3 LTS or later
- **XR Interaction Toolkit**: 3.0.7 or later
- **OpenXR**: 1.14.0 or later
- **EGB Pro2**: All versions (namespace: SoulGames.EasyGridBuilderPro)

## Support

If you encounter issues:
1. Enable debug mode in both scripts
2. Check the console for coordinate system update messages
3. Verify that both scripts are on the same GameObject as EasyGridBuilderProXZ
4. Ensure your XR Interaction Toolkit is properly configured

The system is designed to work with the existing EGB Pro2 coordinate system, so object placement should be accurate after rotation when properly configured. 