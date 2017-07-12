# Still or recently experimental
- Automatic emptying.
- Override TransferManager.AddIncomingOffer and TransferManager.AddOutgoingOffer?
- Remember problematic targets and delay next assigment to such target, so that it does not ceate a problem for the whole city.
- Make a simple textbox for adding comma separated custom strategy.
- New stragety option: Make it so IgnoreRange only ignores range for the n closest buildings.

# Doing
- Ambulance services.

# Dox etc

# Additions and fixes

- Replace MonoDetour with [Harmony](https://github.com/pardeike/Harmony).
- Fire fighting services?
- Law enforcement services?
- Send service now button on buildings? Dispatch to specific building?
- Pipe and electricty area range (Central Services even if not really dispatching)?
- Save in vehicles how much capacity they will have left after pickup and add send-out-option to send when no vehicles are free now or will be free with enough capacity after next pickup?
- Social services (taking care of confused citizens and tourists).
- Fix invisible vehicles (flags == none, but not in m_vehicles.m_unusedItems)?
- Save states in save?

# Changes and experiments

- Citizens?
- Ambulances: only emergency if citizen is sick enough?
- If (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.HealthCare))
- Find path to building for better range check?

# Notes

- Ambulances: ResidentAI FindHospital, Citizen Sick, m_healthProblemTimer
