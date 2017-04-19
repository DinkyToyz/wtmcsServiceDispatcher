---
title: Compatibility Options
sort_order: 540
---
- **Assignment compatibility mode**:
  This option configures what code the mod uses when assigning a vehicle to a target.
  See [Compatibility Modes](#CompatibilityModes).

- **Creation compatibility mode**:
  This tells the dispatcher what code to use when sending out new vehicles from a building.
  If, as an example, you also use a mod that limits which vehicles can be sent out from a specific building, this mode probably need to be set to *Use current AI*. 
  See [Compatibility Modes](#CompatibilityModes).

- **Allow Code Overrides**: 
  This option configures wether the mod can do code overrides and deep calls to game code or not. IT can have the following values:
  - *Default*:
    Let the mod decide, based on game version.
  - *Never*:
    Never do code overrides and deep calls.
  - *Always*:
    Allow code overrides even if the game version is higher than the mods coded margin.

## Compatibility Modes {#CompatibilityModes}

These modes decides how the mod tells the game to do certain things, and affects compatibility with other mods.

- **Bypass AI**:
  The mod uses it's own code where implemented. This might be more effective, but may also cause incompatibilities.
 
- **Use original AI**:
  Use the games original code. This is usually the most safe, but can bypass other mods.

- **Use current AI**:
  Use the currently assigned code, which may be the origina game code or code added by another mod.
  As long as other mods are compatible with the dispatcher, this should work and gives the other mode opportunity to override the original game.     
