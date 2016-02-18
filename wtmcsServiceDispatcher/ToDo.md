# CS 1.3.0 incl Snowfall

- Check replacemnet helper methods (create, settarget, ...).
- Check TransferManager limits.
- Check pathfind.
- Chek everything accessed by reflection.
- Check new level stuff, to see if it affects or should affect the dispatcher
- Test...

# Doing

- Use own SetTarget instead of (vehicle)AI, and recall vehicle instead of despawning when path can not be found.
- Continue checking for vehicles a bit instead of failing if found and assigend vehicle cannot find path to target.
- Override ServiceVehicleAI.ShouldReturnToSource: Allways true -> vehicle won't make transfer offer when trasferring to source.

# Additions and fixes

- road and rail clean-up crews (to remove stuck vehicles, including forgotten trailers and railway cars)
- house wrecking crews (bulldozers)
- ambulance services?
- fire fighting services?
- law enforcement services?
- send service now button on buildings?

# Changes and experiments

- Override TransferManager AddOutgoingOffer and AddIncomingOffer?
- Clean offers?
- Use detoring instead of invoke for faster calls of private/protected methods (ie StartPathFind in VehicleHelper).
- citizens?
- Detour ProduceGoods or transfer stuff???
- sort by distance capacity %?
- When spawning, stop building from offering incoming transfers. Override ProduceGoods? Regularly remove offers (fast by zeroing m_incomingCount?)? Override something else to stop outgoing dead offers?

# Notes

- Ambulances: ResidentAI FindHospital, Citizen Sick, m_healthProblemTimer