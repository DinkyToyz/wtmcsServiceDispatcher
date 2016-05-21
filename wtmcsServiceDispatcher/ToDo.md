# Doing

- road and rail recovery service crews (to remove stuck vehicles, including forgotten trailers and railway cars).

# Additions and fixes

- use buildings StartTransfer in order to allow ServiceVehicleSelector to work.
- use vehicles StartTransfer in order to allow other mods to work?
- ambulance services.
- Automatic emptying.
- Remember problematic targets and delay next assigment to such target, so that it does not ceate a problem for the whole city.
- fire fighting services?
- law enforcement services?
- send service now button on buildings?
- pipe and electricty area range (Central Services even if not really dispatching)?
- Save in vehciles how much capacity they will have left after pickup and add send-out-option to send when no vehicles are free now or will be free with enough capacity after next pickup?
- Social services (taking care of confused citizens and tourists).

# Changes and experiments

- citizens?
- Inhibit StartTransfer?
- if (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.HealthCare))

# Notes

- Ambulances: ResidentAI FindHospital, Citizen Sick, m_healthProblemTimer
