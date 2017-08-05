---
---
The auto-emptying feature has had a problem. If the city reloaded after the dispatcher had started emptying a landfill or cemetery, the dispatcher would forget that it had started emotying, and would therefore not stop emptying or reactivating the service building.

A quick workaround was implemented: Any building that is emptying when the city is loaded is concidered as auto-emptying, so the dispatcher will stop emptying and reactivate the building again. This is not completely correct, but probably better.

A correct solution, where the dispatcher saves states in the city save-game, has been impemented and will be included in the next release.

As a nice side-effect of this, other states, such as desolate buildings, assigments and stuck vehiclescan also be saved without too much extra work. This will also be in the next release.

The same functionality will also be used to, optionally, save some settings per city instead of allways having the same settings in all cities. This will most likely not be finished for the enxt release, but the release after that.