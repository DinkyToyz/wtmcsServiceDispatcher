# Doing

- Use own SetTarget instead of (vehicle)AI, and recall vehicle instead of despawning when path can not be found.
- Continue checking for vehicles a bit instead of failing if found and assigend vehicle cannot find path to target.
- Override ServiceVehicleAI.ShouldReturnToSource:
  - Allways true -> vehicle won't make transfer offer when trasferring to source.
  - Reassign vehicles returning to source.
- road and rail recovery service crews (to remove stuck vehicles, including forgotten trailers and railway cars).
  - Effective for handled service vehicle.
- house wrecking crews (bulldozers).
- Clean offers.

# Additions and fixes

- ambulance services.
- fire fighting services?
- law enforcement services?
- send service now button on buildings?
- pipe and electricty area range (Central Services even if not really dispatching)?
- Save in vehciles how much capacity they will have left after pickup and add send-out-option to send when no vehicles are free now or will be free with enough capacity after next pickup?
- Remember problematic targets and delay next assigment to such target, so that it does not ceate a problem for the whole city.
- Social services (taking care of confused citizens and tourists).

# Changes and experiments

- Override TransferManager AddOutgoingOffer and AddIncomingOffer?
- citizens?
- Detour ProduceGoods or transfer stuff???
- sort by distance capacity %?
- When spawning, stop building from offering incoming transfers. Override ProduceGoods? Override something else to stop outgoing dead offers?

# Notes

- Ambulances: ResidentAI FindHospital, Citizen Sick, m_healthProblemTimer