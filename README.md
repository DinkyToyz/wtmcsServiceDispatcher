# Central Services Dispatcher (WtM)

This is a mod for [Cities: Skylines](http://www.citiesskylines.com/).

The following is from the [steam description](http://steamcommunity.com/sharedfiles/filedetails/?id=512341354).

---------------------------------------------



Dispatches Cities: Skylines services.

## Compatibility

For most optons, this mod does not override any game code, which should minimize incompatibilities with other mods.

The *prioritize assigned buildings* option does replace one method of the garbage truck ai, and may therefore be incompatible with any other mod that overrides or replaces the garbage truck ai.

## Known Issues

Some people have observed a problem where hearses go to their target buildings but don't load dead bodies.

## Services

Currently, the mod can dispatch garbage trucks and hearses.

I have vague plans to add functionality to remove stuck vehicles (including forgotten trailers and railway cars) and bulldozing, and might decide to look at ambulance, fire and police services as well.

## Dispatching

When a building needs to be serviced, the dispatcher will start looking for a free vehicle from the closest available service building first, and then the next closest and so on. When it finds a service building with free vehicles, it will send the one closest to the building needing service.

Not all service buildings are willing to dispatch to all buildings, and there are a number of strategies, which in turn uses a set of rules, that can be chosen in order do define what buildings a service buildings will dispatch to.

### Garbage Trucks

Garbage trucks picks up garbage from buildings they pass as well as the building they are dispatched to (*this is part of the game, not of this mod*).

One result of this is that if the trucks have to travel a long or convoluted way, and passes many buildings with garbage on the way to their target, they might have to turn back to unload before ever reaching their destination.

The *prioritize assigned buildings* option limits this behaviour.

## Configuration

Which services should be dispatched by Central Services can be configured, as can the dispatch strategy, in the mod options in the content manager, or a configuration file.

If no services are set to be dispatch by the mod, it should not affect the game.

### Global Options

- **Limit building ranges**: 
  Limit the minimum and maximum range of service building (mitigates effects of assets with extreme range).

- **Range modifier**: 
  A value with which service buildings ranges are multiplied. Only available i the config file at the moment.

- **Range minimum**: 
  Minimum service building range value when limiting building ranges. Only available i the config file at the moment.

- **Range maximum**: 
  Maximum service building range value when limiting building ranges. Only available i the config file at the moment.

### Standard Service Options

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

### Hearse Service Options

These options are only available for hearses.

- **Pass through** hearses
  Only available for hearses. Remove these vehicles from grid when stopped so traffic can pass.

### Garbage Truck Service Options

These options are only available for garbage trucks.

- **Prioritize assigned buildings**: 
  (*This option overrides original game code.*)
  Limits the amount of garbage picked up from buildings the trucks passes by, in order to leave room for garbage from their assigned buildings.

- **Garbage amount patrol limit**: 
  Sets the amount of garbage a building must accumulate before a patrolling garbage truck is directed to it.

- **Garbage amount dispatch limit**: 
  Sets the amount of garbage a building must accumulate before a garbage truck is dispatch do take care of it.

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

### Config File

The config file, wtmcsServiceDispatcher.xml, is stored in the folder "ModConfig" wherever the game points to with [`DataLocation.localApplicationData`]. On a Windows system that'll usually be some where like "C:\Users\[`UserName`]\AppData\Local\Colossal Order\Cities\_Skylines\ModConfig".

## Errors & Logging

When reporting severe errors, please upload [the games complete log file](http://steamcommunity.com/sharedfiles/filedetails/?id=463645931) and/or the separate log file (see below) somewhere and post a link.

The mod logs to [the games normal output log](http://steamcommunity.com/sharedfiles/filedetails/?id=463645931), and can also log to a separate log file, wtmcsServiceDispatcher.log, stored in the same directory as the settings.

Create one or more of the following files in the same directory in order to enable debug log stuff (which might slow things down quite a bit) and logging to file.

- **wtmcsServiceDispatcher.debug**: 
  Enables logging debug data to mod log file, and slightly more logging to the games standard log destinations.

- **wtmcsServiceDispatcher.debug.dev**: 
  Log more stuff to mod log file.

- **wtmcsServiceDispatcher.debug.names**: 
  Log object names in some debug log entries.

- **wtmcsServiceDispatcher.debug.lists**: 
  Log object debug lists to mod log file at startup. What this actually does, if anything, may differ between builds.

## Whatever

I made this for myself, and use it. Hopefully it works for others as well, but I make no promises.
I also make no promises about updating or fixing things fast, as that depends on how busy I am with work and other stuff.

[Source code](https://github.com/DinkyToyz/wtmcsServiceDispatcher) is released with MIT license.