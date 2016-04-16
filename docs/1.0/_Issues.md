There are a few known issues, but those issues has been seen both with and without this mod, and seems to be related to things that should not be affected by this mod.

*Some problems caused by road planning, the original game or other mods may become or appear worse with this mod because the dispatcher will keep will keep assigning vehicles to the same targets more often than the original game.*

- **Service vehicles despawn**
  When service vehicles can't find a path to their target they will sometimes despawn.
  This mod does not affect path finding at all, so again its is likely that the cause of the problem lies somewhere else. 
  *Things to check when this happens*:
  - Are there any one-way roads making it impossible to reach certain targets?
  - Are there custom limits (turning, vehicle types) created with mods that may make it impossible for the despawning vehicles to reach their target buildings?


- **Hearses drive to their target buildings but do not load dead bodies**
  This mod doesn't actually change how the vehicles get loaded, it just decides what targets they should go to, so it's likely that the actual problem is somewhere else.
  *Things to check when this happens*:
  - Does the problem occur mainly on specific road types? Are those original road types, or from mods?
  - Does it occur for any building, or is it for specific buildings? Are those building original game buildings or custom assets?


- **Service vehicles are assigned, but looses the target assignment immediately**
  This may be a different symptom of the same problem as for despawning vehicles. If the vehicle can't get to the target, the game may remove the assignment.
  *Things to check when this happens*:
  - Are there any one-way roads making it impossible to reach certain targets?
  - Are there custom limits (turning, vehicle types) created with mods that may make it impossible for the despawning vehicles to reach their target buildings?
  - Is there another mod enabled that also assigns targets to the same service vehicles?