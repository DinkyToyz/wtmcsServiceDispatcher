---
title: Configuration
---
Which services should be dispatched by Central Services can be configured, as can the dispatch strategy, in the mod options in the content manager, or a configuration file.

If no services are set to be dispatch by the mod, it should not affect the game.

## Global Options

- **Limit building ranges**: 
  Limit the minimum and maximum range of service building (mitigates effects of assets with extreme range).

- **Range modifier**: 
  A value with which service buildings ranges are multiplied. Only available i the config file at the moment.

- **Range minimum**: 
  Minimum service building range value when limiting building ranges. Only available i the config file at the moment.

- **Range maximum**: 
  Maximum service building range value when limiting building ranges. Only available i the config file at the moment.

## Standard Service Options

These options are set separately for different service vehicles (garbage trucks and hearses).

- **Dispatch** [`service vehicles`]: 
  Handle dispatching for these service vehicles.

- **Dispatch by district**: 
  All buildings in the same district as a service building are considered to be in it's range.

- **Dispatch by building range**: 
  All buildings within a certain distance from a service building are considered to be in it's range. The distance is a property of the building asset.

- **Send out spare** [`service vehicles`]: 
  When to send out new vehicles from the service building instead of sending one that's already driving. 

- [`Service vehicle`] **dispatch strategy**: 
  Choose the dispatch strategy to use for these vehicles.

### Sending Out Spare Vehicles

The game will send out new vehicles when it deems it a good idea, but the dispatcher can be told to do it before the game would.

- **Never**: 
  Never send out new vehicles before the game would do it anyway.

- **None are free**: 
  Send out new vehicles from the closest available service building when that building has no free vehicles out driving.

- **Building is closer**: 
  Send out new vehicles from the closest available service building when the building is closer than it's nearest free vehicles.

### Dispatch Strategies

Services are dispatched based on problem magnitude within the strategy rules.

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
