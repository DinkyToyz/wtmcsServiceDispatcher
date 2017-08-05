# Next release
- Save states in save-game.
- Dumping stuck vehicles and desolate buldings.
- On windows, dump-buttons also opens the file.
- New experimental setinsg group, used when there are public experiments.
  - Experimental checking for lost trailers.
  - Experimental checking for cargo children.

# Still or recently experimental
- Automatic emptying.
- Override TransferManager.AddIncomingOffer and TransferManager.AddOutgoingOffer.
- Remember problematic targets and delay next assigment to such target, so that it does not create a problem for the whole city.
- Make a simple textbox for adding comma separated custom strategy.
- New stragety option: Make it so IgnoreRange only ignores range for the n closest buildings.
- Save target assigments, desolate buildings and auto-emptying state in save-file.

# Doing
- Ambulance services.
- Being able to have separate settings on global level and map level.

# Dox etc

# Additions and fixes

- WhereSelectToArray
- Replace MonoDetour with [Harmony](https://github.com/pardeike/Harmony).
- Fire fighting services?
- Law enforcement services?
- Send service now button on buildings? Dispatch to specific building?
- Pipe and electricty area range (Central Services even if not really dispatching)?
- Save in vehicles how much capacity they will have left after pickup and add send-out-option to send when no vehicles are free now or will be free with enough capacity after next pickup?
- Social services (taking care of confused citizens and tourists).
- Fix invisible vehicles (flags == none, but not in m_vehicles.m_unusedItems)?
- Being able to have separate settings for district and specific buildings?
- Find path to building for better range check?
- VS Task List.

# Changes and experiments

- Citizens?
- Ambulances: only emergency if citizen is sick enough?
- If (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.HealthCare))

# Notes

- Ambulances: ResidentAI FindHospital, Citizen Sick, m_healthProblemTimer
