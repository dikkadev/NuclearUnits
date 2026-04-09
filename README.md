# NuclearUnits

`NuclearUnits` is a client-side `Nuclear Option` mod that lets you choose units per HUD readout instead of relying on the game's single global metric/imperial setting.

The goal is simple: if you want one speed readout in `kts`, another in `mph`, altitude in `ft`, and some other display left on the game's default behavior, you can do that.

## Features

- Per-readout unit overrides for major HUD displays
- HUD-focused implementation rather than a game-wide unit replacement
- Separate unit choices for speed, altitude, climb rate, and range where those readouts exist
- Falls back to the game's default formatting when you choose `GameDefault`

## Current HUD Coverage

The current version supports separate configuration for these readouts:

- `Altitude HUD`
  - radar altitude
  - absolute altitude
- `Speed Gauge`
  - airspeed
- `Basic Flight Instruments`
  - airspeed
  - altitude
  - climb rate
- `Climb Rate HUD`
  - climb rate
- `Virtual MFD`
  - speed
  - altitude
- `Landing Screen`
  - altitude
  - speed
  - vertical speed
  - horizontal speed
- `Target Marker`
  - speed
  - altitude
  - range
- `Target Screen`
  - distance
  - altitude
  - relative altitude
  - speed
  - relative speed

## Supported Unit Choices

- Speed:
  - `GameDefault`
  - `KilometersPerHour`
  - `Knots`
  - `MilesPerHour`
- Altitude:
  - `GameDefault`
  - `Meters`
  - `Feet`
- Climb rate:
  - `GameDefault`
  - `MetersPerSecond`
  - `FeetPerMinute`
- Distance:
  - `GameDefault`
  - `Metric`
  - `Imperial`

## Requirements

- `Nuclear Option`
- `BepInEx 5`

BepInEx install guide:

- https://docs.bepinex.dev/articles/user_guide/installation/index.html

## Installation

1. Install BepInEx for `Nuclear Option`.
2. Download a release archive from GitHub releases.
3. Extract the `NuclearUnits` folder into `BepInEx/plugins/`.

The final layout should look like this:

```text
BepInEx/
  plugins/
    NuclearUnits/
      NuclearUnits.dll
```

## Configuration

Config entries are created automatically after the mod loads.

Main config file:

- `BepInEx/config/com.dikka.nuclearunits.cfg`

You can configure the mod through:

- the config file directly
- BepInEx Configuration Manager, if you use it

Example config values look like this:

```ini
[Speed Gauge]
## Unit override for the main speed gauge airspeed readout.
# Setting type: SpeedUnit
# Default value: GameDefault
Airspeed = Knots

[Altitude HUD]
## Unit override for the radar altitude readout.
# Setting type: AltitudeUnit
# Default value: GameDefault
Radar Altitude = Feet
```

## Scope

This mod currently targets HUD and HUD-like readouts only.

It does not currently replace unit formatting everywhere in the game, such as:

- encyclopedia pages
- mission editor screens
- general UI that is outside the patched HUD surfaces

That is intentional. The current implementation is aimed at cockpit and tactical displays where per-readout preferences matter most.

## Limitations

- The stock game still has its built-in global `UnitSystem` setting.
- `GameDefault` means a readout will still follow that stock global behavior.
- Dual-unit displays are not implemented yet.

## Local Development Install

For local installs during development:

1. Copy `.env.example` to `.env`
2. Set `STEAMAPPS_DIR`
3. Run:

```bash
./scripts/install-mod.sh -v
```

## Building

This project builds against your local `Nuclear Option` and `BepInEx` assemblies.

Standard local build:

```bash
dotnet build NuclearUnits.csproj -c Release
```

## GitHub Releases

This repo includes a GitHub Actions release workflow, but because the mod builds against assemblies from a local game install, it is intended for a self-hosted runner that has:

- `Nuclear Option` installed
- `BepInEx` installed for that copy
- the same local assembly paths available to the build

The workflow:

- builds `NuclearUnits.dll`
- packages it into a `NuclearUnits` plugin folder
- uploads a zip to the GitHub release

## Roadmap

- expand per-readout coverage further if needed
- add optional dual-unit displays for selected HUD elements
- possibly add a friendlier in-game config UI later
