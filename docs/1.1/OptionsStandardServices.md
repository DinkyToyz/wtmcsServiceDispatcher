---
title: Standard Service Options
sort_order: 520
---
These options are set separately for different standard service vehicles.

{% capture StandardServices %}{% include_relative _OptionsStandardServices.md vehicle="`service vehicle`" vehicles="`service vehicles`" %}{% endcapture %}{{ StandardServices | markdownify }}

## Specific Service Options

See the different services for their unique options:

- [Hearse Service Options](ServiceHearses.html#Options)
- [Garbage Truck Service Options](ServiceGarbageTrucks.html#Options)

## Sending Out Spare Vehicles {#SendOutSpares}

The game will send out new vehicles when it deems it a good idea, but the dispatcher can be told to do it before the game would with the *Send out spare `service vehicles`' option, which have the following alternatives:

- **Game decides**: 
  Let the game's original AIs decide when to send out vehicles.

- **None are free**: 
  Send out new vehicles from the closest available service building when that building has no free vehicles out driving.

- **Building is closer**: 
  Send out new vehicles from the closest available service building when the building is closer than it's nearest free vehicles.

## Dispatch Strategies {#DispatchStrategies}

Services are dispatched based on problem magnitude within the strategy rules, chosen with the *Dispatch Strategy* option. 

- **First first**: 
  All buildings regardless of range.

- **Forgotten first**: 
  Forgotten buildings in range, followed by forgotten buildings out of range, buildings in range and finally problematic buildings in or out of range.

- **In range**: 
  Buildings in range followed by forgotten buildings out of range.

- **In range first**: 
  Buildings in range followed by problematic buildings in or out of range.

- **Problematic first**: 
  Problematic buildings in range followed by problematic buildings out of range and finally buildings in range.

- **Very problematic first**: 
  Very problematic buildings in range followed by very problematic buildings out of range, buildings in range and finally problematic buildings in or out of range.

- **Custom**: 
  Custom strategy manually defined in the configuration file.