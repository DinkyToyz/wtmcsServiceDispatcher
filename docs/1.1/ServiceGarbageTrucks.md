---
title: Garbage Trucks
sort_order: 220
---
Garbage trucks picks up garbage from buildings they pass as well as the building they are dispatched to (*this is part of the game, not of this mod*).

One result of this is that if the trucks have to travel a long or convoluted way, and passes many buildings with garbage on the way to their target, they might have to turn back to unload before ever reaching their destination.

The *prioritize assigned buildings* option limits this behavior.

## Garbage Truck Service Options {#Options}

These options are only available for garbage trucks.

- **Prioritize assigned buildings**: 
  Limits the amount of garbage picked up from buildings the trucks passes by, in order to leave room for garbage from their assigned buildings.

- **Garbage amount patrol limit**: 
  Sets the amount of garbage a building must accumulate before a patrolling garbage truck is directed to it.

- **Garbage amount dispatch limit**: 
  Sets the amount of garbage a building must accumulate before a garbage truck is dispatch do take care of it.

## Standard Service Options

These options works the same as for other service vehicles.

{% capture StandardServices %}{% include_relative _OptionsStandardServices.md vehicle="Garbage truck" vehicles="garbage trucks" %}{% endcapture %}{{ StandardServices | markdownify }}
{% if false %}
## Landfill Options

These options controls if/when the dispatcher orders emptying of landfills.

{% capture StandardServices %}{% include_relative _OptionsEmptyableServices.md storagefacility="landfill" storagefacilities="landfills" %}{% endcapture %}{{ StandardServices | markdownify }}
{% endif %}