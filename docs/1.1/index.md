---
date: 2016-04-10
title: General Info
subheadline: version 1.1
menu_title: 1.1
sort_order: 100
---
## Purpose

The Central Services Dispatcher dispatches service vehicles and crews for various functions in a City, taking distances between buildings, services and vehicles in account, in order to use service vehicles more efficiently, with options for limiting by district and fine tuning many settings.

Currently the following services can be handled by Central Services:

- [Hearses](ServiceHearses.html)
- [Garbage Trucks](ServiceGarbageTrucks.html)
- [Bulldozers](ServiceBulldozers.html)
- [Recovery Crews](ServiceRecoveryCrews.html) (experimental)

## Dispatching

### Service Vehicles

When a building needs to be serviced by a service vehicle, the dispatcher will start looking for a free vehicle from the closest available service building first, and then the next closest and so on. When it finds a service building with free vehicles, it will send the one closest to the building needing service.

Not all service buildings are willing to dispatch to all buildings, and there are a number of strategies, which in turn uses a set of rules, that can be chosen in order do define what buildings a service buildings will dispatch to.

### Invisible Services

When something needs to be done by the invisible services (such as bulldozing), the dispatcher will simply make it happen. 

## Configuration

Which services should be dispatched by Central Services can be configured, as can the dispatch strategy, in the mod options in the content manager, or a configuration file.If no services are set to be dispatch by the mod, it should not affect the game.

For available options see:

- [Global Options](OptionsGlobal.html)
- [Standard Service Options](OptionsStandardServices.html)
- [Invisible Service Options](OptionsInvisibleServices.html)
- [Configuration File](Files.html#Config)

## Compatibility

For most options, this mod is not dependant on overriding any game code, which should minimize incompatibilities with other mods.

Any mod that overrides, or replaces, service vehicle AIs may limit some of the dispatchers functionality, but hopefully in a non-fatal way.  

## The Future

Planned additions:

- More standard services, with Ambulances as the top priority.
- Recovery Services (removal of stuck vehicles).

## Whatever

I made this for myself, and use it. Hopefully it works for others as well, but I make no promises.
I also make no promises about updating or fixing things fast, as that depends on how busy I am with work and other stuff.

[Source code](https://github.com/DinkyToyz/wtmcsServiceDispatcher){:target="_blank"} is released with MIT license.