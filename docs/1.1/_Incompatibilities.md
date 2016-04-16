As of this writing there are no confirmed incompatibilities, but there are some theoretical and/or suspected incompatibilities.

This mod does not completely replace any AIs or other original game objects, but it does override a few methods.
The [Allow Code Overrides](OptionsAdvanced.html#ReflectionAllowance) option can be used to stop the mod from overriding any code.

## Game Versions & Patches

Whenever the game is patched/upgraded by the develpers, there is a risk that this mod becomes incompatible.

The mod therefore has version limits, and some functionality may be disabled automatically when the game updates.

To manualy allow or disallow code overrides and deep game calls regardless of version, use the [Allow Code Overrides](OptionsAdvanced.html#ReflectionAllowance) option.

## Service Mods

Any mod that override code in, or replaces, service vehicle AIs has a risk of beeing incompatible with this mod.

Hopefully such incompatibilities will only rusult in limited functinalit loss in the mod, and no crashes, but is is impossible to know for sure.

Also, using two mods that assigns/reassigns the same service vehicles is likely to cause problems.

- [Districts](http://steamcommunity.com/sharedfiles/filedetails/?id=649522495){:target="_blank"}
  The *Dicstricts* mod completely replaces the AIs ogf service buildings an vehicles. 
  This might break the garbage truck *Prioritize assigned buildings* options, and may also result in an increased number of reassigments.   

## Road, Zone and Traffic Mods 

Any mod that affects path finding and road access has the potential to cause problems with services. Such problems typically occur with or without this mod, but it is possible that they become more pronounced, or just have different symptoms, when the dispatchers in this mod is enabled.

There has been reports of problems with hearses and garbage trucks with mods based on Traffic++ and Traffic Manager, but at the moment there are no confirmed cases of incompatibilities between those mods and this one.

- [Traffic++ V2](http://steamcommunity.com/sharedfiles/filedetails/?id=626024868){:target="_blank"}
  Traffic++ V2 has caused problems for service vehicles in some past versions, but those problems should be fixed.
- [Traffic Manager: President Edition](http://steamcommunity.com/sharedfiles/filedetails/?id=583429740){:target="_blank"}
  There are unconfirmed reports of Traffic Manager: President Edition causing problems for garbage trucks.
- [Network Extensions Project](http://steamcommunity.com/sharedfiles/filedetails/?id=478820060){:target="_blank"}
  There are unconfirmed reports of Network Extensions Project causing problems for garbage trucks.