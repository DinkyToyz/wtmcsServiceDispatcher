---
title: Miscellaneous Options
sort_order: 560
---
## Information

- **Config Path**:
  The path where the mods configuration and separate log file can be found.
  See [Files](Files) for more info.
  
- **Mod Version**:
  The mods version and build number.
  
## Functions

The dump functons saves files in the mod mods config folder (see above).

On windows, the functions will also open the files in whatever applicatuon is the default for .txt-files.

- **Dump desolate buildings**
  `wtmcsServiceDispatcher.DesolateBuildings.txt`
  Saves the list of buildings from the wrecking crew dispatcher.

- **Dump stuck vehicles**
  `wtmcsServiceDispatcher.StuckVehicles.txt`
  Saves a list of vehicles the recovery crew dispatcher is currently keeping tabs on. Usually most (hopefully all) of these vehicles are just beeing checked, and are not atually stuck, broken or confused.
  
- **Dump all buildings**
  `wtmcsServiceDispatcher.Buildings.txt`
  Saves a semi-colon separated list of all buildings.

- **Dump all vehicles**
  `wtmcsServiceDispatcher.Vehicles.txt`
  Saves a semi-colon separated list of all vehicles.