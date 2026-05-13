# The Car Simulator VR - Team README

This README explains the Unity project structure in a simple way so every team member knows where to work and which scene belongs to them.

## Project Summary

The project is a VR car driving simulator made in Unity. It uses Vehicle Physics Pro for car movement, OpenXR/XR tools for VR, and custom scripts for keyboard, steering wheel, throttle, brake, dashboard, and scene control.

Use this file as the main guide before changing scenes, scripts, or assets.

## Important Rule for Team Work

Each team member should work mainly inside their assigned scene in:

```text
Assets/Scenes/Scenarios/
```

Do not edit another member's scene unless the team agrees first. This avoids merge conflicts and lost work.

## Scene Assignments

| Team Member | Assigned Scene | Scene Path | Main Work Area |
|---|---|---|---|
| Abel | Abel | `Assets/Scenes/Scenarios/Abel.unity` | Abel's scenario work and testing |
| Amr | Amr | `Assets/Scenes/Scenarios/Amr.unity` | Amr's scenario work and testing |
| Ganshyam | Ganshyam | `Assets/Scenes/Scenarios/Ganshyam.unity` | Ganshyam's scenario work and testing |
| Milan | Milan | `Assets/Scenes/Scenarios/Milan.unity` | Milan's scenario work and testing |
| Ragul | Ragul | `Assets/Scenes/Scenarios/Ragul.unity` | Ragul's scenario work and testing |
| Shehab | shehab | `Assets/Scenes/Scenarios/shehab.unity` | Shehab's scenario work and testing |
| Vivk | vivk | `Assets/Scenes/Scenarios/vivk.unity` | Vivk's scenario work and testing |
| Vrindha | Vrindha | `Assets/Scenes/Scenarios/Vrindha.unity` | Vrindha's scenario work and testing |

## Main Project Hierarchy

```text
Assets/
|
|-- Editor/
|   Unity editor helper files.
|
|-- Menu Scene/
|   Main menu assets, car prefabs, menu prefabs, and custom project scripts.
|
|-- Samples/
|   Imported sample content from Unity XR packages.
|
|-- Scenes/
|   Main project scenes and team scenario scenes.
|
|-- Vehicle/
|   Vehicle Physics Pro package, car physics, vehicle prefabs, dashboard UI, and vehicle tools.
|
|-- XR/
|   XR and VR settings/assets for headset support.
|
|-- last.inputactions
|   Input Action asset for controller/keyboard input.
|
|-- last_script.cs
|   Empty starter script. Not important for main driving logic.
|
|-- trial.cs
|   Simple test car controller script using wheel colliders.
```

## Scenes Folder

```text
Assets/Scenes/
|
|-- Refernce scene.unity
|   Main reference scene. Use it to understand the complete setup.
|
|-- Reference scene/
|   Folder connected to the reference scene.
|
|-- Scenarios/
|   Individual member scenes. Each member works in their own scene here.
|
|-- Charactars/
|   Character/NPC scene and related character assets.
|
|-- SUV testing/
|   Test scene/folder for SUV-related experiments.
```

Important note: the main scene file is named `Refernce scene.unity` in the project. The spelling is different from "Reference", so use the exact filename when searching.

## Scenario Scenes

The scenario scenes are copies or variations where each member can safely work without editing the same scene file.

```text
Assets/Scenes/Scenarios/
|
|-- Abel.unity
|-- Amr.unity
|-- Ganshyam.unity
|-- Milan.unity
|-- Ragul.unity
|-- shehab.unity
|-- vivk.unity
|-- Vrindha.unity
```

Recommended workflow:

1. Open your own scene from `Assets/Scenes/Scenarios/`.
2. Make your changes only in your scene.
3. Test with Play Mode.
4. Tell the team what you changed.
5. Only move changes into the reference/final scene after team review.

## Common Scene Objects

Most driving scenes contain these important objects:

| Object | Purpose |
|---|---|
| `EventSystem` | Handles UI input. |
| `Directional Light` | Main scene lighting. |
| `The City` or environment object | Road/city/world environment. |
| `Dashboard Canvas` | Speedometer, gear, and dashboard UI. |
| `UI Canvas` | General UI/menu canvas. |
| `VPP JPickup` | Main drivable car using Vehicle Physics Pro. |
| `XR Interaction Setup` | VR camera and XR interaction setup. |

## Menu Scene Folder

```text
Assets/Menu Scene/
|
|-- Car assets/
|   Car prefabs, including VPP JPickup and VPP Sport Coupe.
|
|-- Script/
|   Menu and vehicle input bridge scripts.
|
|-- Scripts/
|   Steering wheel visuals, driver hand/arm scripts, and keyboard/wheel input script.
|
|-- Prefebs/
|   Menu prefabs used by the project.
|
|-- Assets Main Menu/
|   Main menu UI package and related menu assets.
```

Important scripts:

| Script | Path | Purpose |
|---|---|---|
| `Wheel Keyboard Car wheel Input.cs` | `Assets/Menu Scene/Scripts/Wheel Keyboard Car wheel Input.cs` | Keyboard, wheel, throttle, brake, gear, ignition, and speed/needle debug logging. |
| `WheelKeyboardVehicleInputBridge.cs` | `Assets/Menu Scene/Script/WheelKeyboardVehicleInputBridge.cs` | New Unity Input System bridge for wheel/keyboard input. |
| `SteeringWheelVisual.cs` | `Assets/Menu Scene/Scripts/SteeringWheelVisual.cs` | Rotates the visible steering wheel. |
| `DriverArmSteeringPose.cs` | `Assets/Menu Scene/Scripts/DriverArmSteeringPose.cs` | Moves driver arm/hand bones with steering input. |
| `DashboardMenuController.cs` | `Assets/Menu Scene/Script/DashboardMenuController.cs` | Dashboard/menu control script. |

## Vehicle Folder

```text
Assets/Vehicle/
|
|-- Vehicle Physics Pro/
|   Main imported vehicle physics package.
|
|-- TextMesh Pro/
|   Text rendering assets used by UI.
```

Important Vehicle Physics Pro areas:

| Folder | Purpose |
|---|---|
| `Assets/Vehicle/Vehicle Physics Pro/Vehicles/` | Vehicle prefabs and vehicle assets. |
| `Assets/Vehicle/Vehicle Physics Pro/Scenes/UI/` | Dashboard, input monitor, gauge UI, and vehicle UI scripts. |
| `Assets/Vehicle/Vehicle Physics Pro/Sdk/` | Vehicle Physics Pro DLL and SDK files. |

Important speedometer script:

```text
Assets/Vehicle/Vehicle Physics Pro/Scenes/UI/Scripts/Dashboard.cs
```

This script reads the car speed and rotates the speed needle.

## Input and Driving Logic

Main input script:

```text
Assets/Menu Scene/Scripts/Wheel Keyboard Car wheel Input.cs
```

Main controls:

| Input | Action |
|---|---|
| `W` | Throttle / gas |
| `S` | Brake |
| `A` | Steer left |
| `D` | Steer right |
| `Space` | Handbrake |
| `K` | Ignition |
| `R` | Reverse gear |
| `N` | Neutral gear |
| `F` | Drive gear |
| `Q` | Gear down |
| `E` | Gear up |

The steering wheel and pedal axes are configured in:

```text
ProjectSettings/InputManager.asset
```

Important input names:

```text
WheelSteer
WheelThrottle
WheelBrake
```

## Speed and Needle Angle

The vehicle speed comes from Vehicle Physics Pro:

```text
VehicleData.Speed
```

The dashboard converts speed from meters per second to kilometers per hour:

```text
speedKph = speedMs * 3.6
```

Dashboard UI needle equation:

```text
angle = 135 + ((speedKph - 0) / (200 - 0)) * (-135 - 135)
```

Simplified:

```text
angle = 135 - 1.35 * speedKph
```

JPickup in-car gauge fallback equation:

```text
angle = 0 + ((speedKph - 0) / (225 - 0)) * (-270 - 0)
```

Simplified:

```text
angle = -1.2 * speedKph
```

## XR and VR Folders

```text
Assets/XR/
```

This folder contains XR/VR settings and assets.

```text
Assets/Samples/
```

This folder contains imported sample content from XR Hands and XR Interaction Toolkit. Team members should avoid editing imported sample files unless necessary.

## Packages and Project Settings

```text
Packages/
|
|-- manifest.json
|-- packages-lock.json
```

These files define Unity packages used by the project.

```text
ProjectSettings/
```

This folder contains Unity project settings such as input, physics, graphics, XR, and build settings.

Only change `ProjectSettings/` when the whole team agrees, because those changes affect everyone.

## Safe Team Workflow

1. Open Unity.
2. Open your assigned scene.
3. Work only in your assigned scene and related assets.
4. Test in Play Mode.
5. Save your scene.
6. Tell the team what changed.
7. Final integration should happen in the reference/final scene after review.

## How to Push Your Own Branch

Each team member should push work to their own branch. Do not push directly to `main` unless the team lead asks you to.

### 1. Check Current Branch

Before starting work, check which branch you are on:

```powershell
git branch
```

If you are on `main`, create your own branch first.

### 2. Create Your Own Branch

Use your name in the branch name:

```powershell
git checkout -b member-your-name
```

Examples:

```powershell
git checkout -b member-abel
git checkout -b member-amr
git checkout -b member-shehab
```

### 3. Save Your Unity Work

In Unity:

1. Save your assigned scene.
2. Save the project.
3. Close Play Mode before committing.

### 4. Check Changed Files

```powershell
git status
```

Review the changed files. Make sure you are not changing another member's scene by mistake.

### 5. Add Your Changes

To add all changed files:

```powershell
git add .
```

Or add only your scene file:

```powershell
git add Assets/Scenes/Scenarios/YourSceneName.unity
```

Example:

```powershell
git add Assets/Scenes/Scenarios/Abel.unity
```

### 6. Commit Your Changes

Write a short message explaining your work:

```powershell
git commit -m "Update Abel scenario scene"
```

### 7. Push Your Branch

The first time you push your branch:

```powershell
git push -u origin member-your-name
```

Example:

```powershell
git push -u origin member-abel
```

After the first push, you can push again with:

```powershell
git push
```

### 8. If You Accidentally Commit on Main

If Git says you are leaving a commit behind when switching branches, keep it by creating a branch from that commit:

```powershell
git branch backup-your-name commit-id
```

Example:

```powershell
git branch backup-shehab f7287d5
```

Then ask the team lead before moving that commit into `main`.

## What Each Member Should Submit

Each member should provide:

1. Their updated assigned scene.
2. A short note explaining what they changed.
3. Any problem or bug they found.
4. Screenshots or video if the change is visual.

## Quick Start for Team Members

1. Open the project in Unity.
2. Go to `Assets/Scenes/Scenarios/`.
3. Open your assigned scene.
4. Press Play and test the car.
5. Work only in your own scene.

## Final Integration

The reference scene should be used as the final integration base:

```text
Assets/Scenes/Refernce scene.unity
```

When a member finishes their work, the team lead can copy the required objects, settings, or changes from that member's scenario scene into the final reference scene.
