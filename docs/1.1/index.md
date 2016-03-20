---
title: General Info
subheadline: version 1.1
navigation_menu: I100100
menu_title: 1.1
---
## Purpose

The Central Services Dispatcher dispatches service vehicles and crews for various functions in a City.

Currently the following services can be handled by Central Services:

- [Hearses](Hearses.html)
- [Garbage Trucks](Garbage Trucks.html)
- [Bulldozers](Bulldozers)

## Dispatching

### Service Vehicles

When a building needs to be serviced by a service vehicle, the dispatcher will start looking for a free vehicle from the closest available service building first, and then the next closest and so on. When it finds a service building with free vehicles, it will send the one closest to the building needing service.

Not all service buildings are willing to dispatch to all buildings, and there are a number of strategies, which in turn uses a set of rules, that can be chosen in order do define what buildings a service buildings will dispatch to.

### Invisible Services

When something needs to be dne by the invisible services (such as bulldozing), the dispatcher will simply make it happen. 

## Configuration

Which services should be dispatched by Central Services can be configured, as can the dispatch strategy, in the mod options in the content manager, or a configuration file.If no services are set to be dispatch by the mod, it should not affect the game.

## Whatever

I made this for myself, and use it. Hopefully it works for others as well, but I make no promises.
I also make no promises about updating or fixing things fast, as that depends on how busy I am with work and other stuff.

[Source code](https://github.com/DinkyToyz/wtmcsServiceDispatcher) is released with MIT license.
