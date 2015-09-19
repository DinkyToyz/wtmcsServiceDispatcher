# Central Services Dispatcher (WtM)

This is a mod for Cities: Skylines.

The following is from the steam description.

---------------------------------------------

Dispatches Cities: Skylines services.

## Experimental

This mod is experimental. It might work, but it also might not. It should not affect saves, but it may well wreak havoc with services while enabled.

## Known Bugs

- When changing options in-game the new settings are saved but not applied correctly to the current game.

## Services

Currently, the mod can dispatch garbage trucks and hearses.

I have vague plans to add road and rail clean-up crews (to remove stuck vehicles, including forgotten trailers and railway cars), house wrecking crews (bulldozers), and might decide to look at ambulances as well.

There is absolutely no time table for any additional services, and I'm not promising they'll ever get added.

## Dispatching

When a building needs to be serviced, the dispatcher will start looking for a free vehicle from the closest available service building first, and then the next closest and so on. When it finds a service building with free vehicles, it will send the one closest to the buidling needing service.

Not all service budlings are willing to dispatch to all buildings, and there are a number of strategies, wich in turn uses a set of rules, that can be chosen in order do define what buildings a service buildings will dispatch to.

### Garbage Trucks

Garbage trucks picks up garabge from buildings they pass as well as the building they are dispatched to (this is part of the game, not of this mod).

One result of this is that if the trucks have to travel a long or convoluted way, and passes many buildings with garbage on the way to their target, they might have to turn back to unload before ever reaching their detsination.

## Configuration

Which services should be dispatched by Central Services can be configured, as can the dispatch strategy, in the mod options in the content manager (unfortunately the options controls are quite limited, so there's a bunch of sliders without any display of the numeric values for now), or a configuration file.

If no services are set to be dispatch by the mod, it should not affect the game.

### Global Options

- **Dispatch by district**: 
  All buildings in the same district as a service building are considered to be in it's range.

- **Dispatch by building range**: 
  All buildings within a certain distince from a service buidling are considered to be in it's range. The distance is a property of the building asset.

- **Limit building ranges**: 
  Limit the minimum and maximum range of service bulding (mitigates effects of assets with extreme range).

- **Range modifier**: 
  A value with wich service buildings ranges are multiplied. Only available i the config file at the moment.

- **Range minimum**: 
  Minimum service bulting range value when limiting building ranges. Only available i the config file at the moment.

- **Range maximum**: 
  Maximum service bulting range value when limiting building ranges. Only available i the config file at the moment.

### Service options

These options are set separately for different service vehicles (garbage trucks and hearses).

- **Dispatch** [`service vehicles`]: 
  Handle dispatching for these service vehicles.

- **Pass through** [`service vehicles`]: 
  Remove these vehicles from grid when stopped so traffic can pass.

- [`Service vehicle`] **dispatch strategy**: 
  Choose the dispatch strategy to use for these vehicles.

- **Garbage amount limit**: 
  Only available for garbage trucks. Sets the amount of garbage a bulding must accumulate before a garbage truck is dispatch do take care of it.

### Dispatch Strategies

Services are dispatched based on problem magnitude within the strategy rules.

- **First first**: 
  All buldings regardless of range.

- **Forgotten first**: 
  Forgotten buidlings in range, followed by forgotten buildings out of range, buildings in range and finally problematic buildings in or out of range.

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

The config file, wtmcsServiceDispatcher.xml, is stored in the folder "ModConfig" wherever the game points to with [`DataLocation.localApplicationData`]. On a Windows system that'd usually be some where like "C:\Users\[`UserName`]\AppData\Local\Colossal Order\Cities\_Skylines\ModConfig".

## Errors & Logging

When reporting severe errors, please upload [the games complete log file](http://steamcommunity.com/sharedfiles/filedetails/?id=463645931) and/or the separate log file (see below) somewhere and post a link.

The mod logs to [the games normal output log](http://steamcommunity.com/sharedfiles/filedetails/?id=463645931), and can also log to a separate log file, wtmcsServiceDispatcher.log, stored in the same directory as the settings.

Create one or more the following files in the same directory in order to enable debug log stuff (wich might slow things down quite a bit) and logging to file.

- **wtmcsServiceDispatcher.debug**: 
  Enables logging debug data to mod log file, and slightly more logging to the games standard log destinations.

- **wtmcsServiceDispatcher.debug.dev**: 
  Log more stuff to mod log file.

- **wtmcsServiceDispatcher.debug.names**: 
  Log object names in some debug log entries.

- **wtmcsServiceDispatcher.debug.lists**: 
  Log object debug lists to mod log file at startup. What this actually does, if anything, may differ between builds.

## To-do

- Fix: In-game options changes.
- New: Make service building send out needed vehicles instead of keeping some in building.
- Road and rail clean-up crews.
- House wrecking crews.

## Whatever

I made this for myself, and use it. Hopefully it works for others as well, but I make no promises.
I also make no promises about updating or fixing things fast, as that depends on how busy I am with work and other stuff.

[Source code](https://github.com/DinkyToyz/wtmcsServiceDispatcher) is realesed with MIT license.