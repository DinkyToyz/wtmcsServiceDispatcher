---
---
## Compatibility

For most optons, this mod does not override any game code, which should minimize incompatibilities with other mods.

The *prioritize assigned buildings* option does replace one method of the garbage truck ai, and may therefore be incompatible with any other mod that overrides or replaces the garbage truck ai.

## Known Issues

Some people have observed a problem where hearses go to their target buildings but don't load dead bodies.

## Services

I have vague plans to add functionality to remove stuck vehicles (including forgotten trailers and railway cars) and bulldozing, and might decide to look at ambulance, fire and police services as well.

### Garbage Trucks

Garbage trucks picks up garbage from buildings they pass as well as the building they are dispatched to (*this is part of the game, not of this mod*).

One result of this is that if the trucks have to travel a long or convoluted way, and passes many buildings with garbage on the way to their target, they might have to turn back to unload before ever reaching their destination.

The *prioritize assigned buildings* option limits this behaviour.

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
