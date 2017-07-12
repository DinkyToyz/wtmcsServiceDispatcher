- **Dispatch {{ include.vehicles }}**: 
  Handle dispatching for these service vehicles.

- **Dispatch by district**: 
  All buildings in the same district as a service building are considered to be in it's range.

- **Dispatch by building range**: 
  All buildings within a certain distance from a service building are considered to be in it's range. The distance is a property of the building asset. (But can also be adjusted with the [Service Radius Adjuster](http://steamcommunity.com/sharedfiles/filedetails/?id=785237088) mod.)

- **Send out spare {{ include.vehicles }}**: 
  When to send out new vehicles from the service building instead of sending one that's already driving.
  See [Sending Out Spare Vehicles](OptionsStandardServices.html#SendOutSpares).  

- **{{ include.vehicle }} dispatch strategy**: 
  Choose the dispatch strategy to use for these vehicles.
  See [Dispatch Strategies](OptionsStandardServices.html#DispatchStrategies).

- **Closest services to use when ignoring range**:
  Only uses the specified number of service buildings closest to the target building when ignoring whether buildings are in range or not.  
  
- **Custom dispatch strategy**
  The rules used for the custom dispatch strategy.
  See [Custom Dispatch Strategy Rules](OptionsStandardServices.html#CustomDispatchStrategy).

If both *Dispatch by district* and *Dispatch by building range* are selected, a building is considered in range if it is either in the same district as, *or* within the coverage radius of, the service building. 