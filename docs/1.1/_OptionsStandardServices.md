- **Dispatch {{ include.vehicles }}**: 
  Handle dispatching for these service vehicles.

- **Dispatch by district**: 
  All buildings in the same district as a service building are considered to be in it's range.

- **Dispatch by building range**: 
  All buildings within a certain distance from a service building are considered to be in it's range. The distance is a property of the building asset.

- **Send out spare {{ include.vehicles }}**: 
  When to send out new vehicles from the service building instead of sending one that's already driving.
  See [Sending Out Spare Vehicles](OptionsStandardServices.html#SendOutSpares).  

- **{{ include.vehicle }} dispatch strategy**: 
  Choose the dispatch strategy to use for these vehicles.
  See [Dispatch Strategies](OptionsStandardServices.html#DispatchStrategies).