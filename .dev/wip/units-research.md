# NuclearUnits Research

Status: WIP - initial assembly and UI archaeology for per-measurement unit overrides.

## Goal

Build a `Nuclear Option` mod that allows unit preferences to be chosen independently per measurement instead of relying on the game's single global metric/imperial switch.

Initial user goals from `../README.md`:

- separate preferences per measurement category
- support at least metric vs imperial where appropriate
- for speed, support `mph`, `km/h`, and `kts`
- possible later expansion: show a secondary unit at the same time

## Existing Mod Baseline

From `../NuclearVOIP`:

- existing local mod setup is a BepInEx C# plugin
- target framework is `netstandard2.1`
- project references game assemblies from `NuclearOption_Data/Managed`
- Harmony patching is already used in the vendored `AtomicFramework`

Implication:

- `NuclearUnits` should likely follow the same BepInEx + C# + direct game references shape
- Harmony patches are a normal fit for this workspace and probably the right tool here

## Game Assembly Findings

Assembly inspected:

- `/mnt/d/SteamLibrary/steamapps/common/Nuclear Option/NuclearOption_Data/Managed/Assembly-CSharp.dll`

Tooling used:

- `ilspycmd`
- string search against `Assembly-CSharp.dll`

### Built-in Unit Model

The game already has a global unit setting in `PlayerSettings`:

```csharp
public enum UnitSystem
{
    Metric,
    Imperial
}

public static UnitSystem unitSystem = UnitSystem.Metric;
```

The setting is loaded from `PlayerPrefs["UnitSystem"]` in `PlayerSettings.LoadPrefs()`.

### Built-in Settings UI

`GameplayMenu` contains the built-in units dropdown:

- field: `Dropdown unitSystemDropdown`
- reads with `unitSystemDropdown.SetValueWithoutNotify((int)PlayerSettings.unitSystem)`
- writes with `PlayerPrefs.SetInt("UnitSystem", unitSystemDropdown.value)`

This means the stock game only supports a single global unit mode.

## Central Formatting Hub

`UnitConverter` is the main formatting helper and is the most important type found so far.

Key methods:

- `AltitudeReading(float altitude)`
- `DistanceReading(float distance)`
- `SpeedReading(float speed)`
- `SpeedReadingGround(float speed)`
- `ClimbRateReading(float speed)`
- `DimensionReading(float length)`
- `WeightReading(float weight)`
- `PowerReading(float kW)`
- `PowerToWeightReading(float kWPerKg)`

Important behavior:

- all of these methods branch on the single global `PlayerSettings.unitSystem`
- `SpeedReading()` returns `km/h` for metric and `kt` for imperial
- `SpeedReadingGround()` returns `km/h` for metric and `mph` for imperial
- `AltitudeReading()` returns `m` for metric and `ft` for imperial
- `ClimbRateReading()` returns `m/s` for metric and `fpm` for imperial
- `DistanceReading()` returns metric values in `m` or `km`, imperial values in `yd` or `nm`

This is a strong implementation seam for the mod.

## Confirmed UI Call Sites

### Cockpit / flight displays

- `Altitude.Refresh()`
  - uses `UnitConverter.AltitudeReading()` for radar altitude and absolute altitude
- `SpeedGauge.Refresh()`
  - uses `UnitConverter.SpeedReading()` for airspeed
- `BasicFlightInstruments.Refresh()`
  - uses `UnitConverter.SpeedReading()`
  - uses `UnitConverter.AltitudeReading()`
  - uses `UnitConverter.ClimbRateReading()`
- `Climbrate.Refresh()`
  - uses `UnitConverter.ClimbRateReading()`
- `VirtualMFD`
  - uses `UnitConverter.SpeedReading()`
  - uses `UnitConverter.AltitudeReading()`
- `LandingScreenUI`
  - uses `UnitConverter.SpeedReading()`
  - uses `UnitConverter.DistanceReading()` for altitude text

### Targeting / tactical UI

- `TargetMarker`
  - uses speed, altitude, and distance readings
- `TargetScreenUI`
  - uses distance, altitude, relative altitude, speed, and relative speed readings
- `MapToolTip`
  - uses speed and altitude readings
- `TargetListSelector_UnitItem`
  - uses distance readings
- `ObjectiveOverlay`, `ObjectiveInfoList_Item`, `AirbaseOverlay`, `ThreatItem`
  - use distance readings

### Other game UI

- `AircraftSelectionMenu`
  - uses weight, range, and power-to-weight readings
- `StatusGauges`
  - uses weight and power-to-weight readings
- `PropGauge`
  - uses power readings
- `EncyclopediaBrowser`
  - uses dimension, weight, speed, distance, and ground speed readings
- mission editor panels also use these converter methods in several places

## Likely Implementation Strategies

### Option A: patch `UnitConverter`

Pros:

- broad coverage across the game
- one central override point per measurement category
- minimal need to patch each HUD widget separately
- likely the best path for per-category units like speed, altitude, climb, distance, and weight

Cons:

- affects non-flight UI too, including encyclopedia and editor panels
- may be broader than desired if the mod should only affect combat HUD

### Option B: patch specific HUD widgets

Examples:

- `Altitude.Refresh()`
- `SpeedGauge.Refresh()`
- `BasicFlightInstruments.Refresh()`
- `LandingScreenUI`
- `TargetScreenUI`

Pros:

- more precise control over exactly which displays change

Cons:

- more patch points
- easier to miss screens
- duplicates unit formatting logic if not still routing through a helper

## Current Recommendation

For the first implementation, patch `UnitConverter` and replace the global two-mode logic with mod-owned per-category logic.

Most likely configuration categories for v1:

- airspeed
- ground speed
- altitude
- climb rate
- distance/range
- weight
- dimensions
- power
- power-to-weight

Possible enum examples for the mod:

- airspeed: `km/h`, `kts`, `mph`
- ground speed: `km/h`, `mph`, `kts`
- altitude: `m`, `ft`
- climb rate: `m/s`, `fpm`
- distance: metric vs imperial-style presentation

## Notes On Dual-Unit Display

The requested future idea of primary + secondary display probably cannot be done only inside `UnitConverter`, because the existing UI generally expects a single formatted string.

That feature will likely require patching specific UI widgets such as:

- `Altitude`
- `SpeedGauge`
- `BasicFlightInstruments`
- `TargetScreenUI`

So the likely roadmap is:

1. v1: replace single-unit formatting behavior
2. v2: patch selected HUD widgets for dual-unit rendering

## Useful Type References

- `PlayerSettings`
- `GameplayMenu`
- `UnitConverter`
- `Altitude`
- `SpeedGauge`
- `BasicFlightInstruments`
- `Climbrate`
- `VirtualMFD`
- `LandingScreenUI`
- `TargetMarker`
- `TargetScreenUI`
- `MapToolTip`
- `AircraftSelectionMenu`
- `StatusGauges`
- `EncyclopediaBrowser`

## Likely Next Coding Step

Create a new BepInEx plugin project for `NuclearUnits`, then add Harmony patches around `UnitConverter` methods first.

If patching the converter proves too blunt, fall back to targeted HUD method patches.
