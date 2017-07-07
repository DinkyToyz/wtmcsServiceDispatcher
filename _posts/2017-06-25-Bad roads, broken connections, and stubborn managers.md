---
---
The dispatcher now remembers when there are problems servicing specific buildings, and will lower the priority of those buildings. This is done per service building, so as long as it's possible for vehicles from one service building to reach the problematic building, it should get servivce evevtually.   

This typically happens due to a misplaced one-way road or a bad connection somewhere. The problem exists in the original game code as well, but, depending on settings, could become worse with the dispatcher.  

The dispatcher now also restricts the game's transfer manager from handling offers for the same services. This does not make a big difference, but does save some clock cycles and avoid some status flickering caused by the manager stubbornly creating vehicles that the dispatcher removes.