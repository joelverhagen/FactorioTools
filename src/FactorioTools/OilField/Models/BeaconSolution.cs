using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public record BeaconSolution(BeaconStrategy Strategy, List<Location> Beacons, int Effects);