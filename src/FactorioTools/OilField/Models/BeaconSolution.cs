using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public record BeaconSolution(BeaconStrategy Strategy, ITableArray<Location> Beacons, int Effects);