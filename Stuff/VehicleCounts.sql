SELECT 
    OwnVehicles,
    MadeVehicles,
    VehicleCount,
    ProductionRate,
    VehicleCountNominal,
    Budget,
    ProductionRateActual,
    VehicleCountActual,
    SpareVehicles,
    BuildingName 
FROM 
    Building 
WHERE 
    AI IN ('CemeteryAI', 'LandfillSiteAI') 
ORDER BY 
    AI, 
    BuildingName;
