---
title: Hearses
sort_order: 230
---
## Hearse Service Options {#Options}

These options are only available for hearses.

- **Pass through hearses**
  Only available for hearses. Remove these vehicles from grid when stopped so traffic can pass.

## Standard Service options

These options works the same as for other service vehicles.

{% capture StandardServices %}{% include_relative _OptionsStandardServices.md vehicle="Hearse" vehicles="hearses" %}{% endcapture %}{{ StandardServices | markdownify }}

## Cemetery Options

These options controls if/when the dispatcher orders emptying of cemeteries.

{% capture StandardServices %}{% include_relative _OptionsEmptyableServices.md storagefacility="cemetery" storagefacilities="cemeteries" %}{% endcapture %}{{ StandardServices | markdownify }}
