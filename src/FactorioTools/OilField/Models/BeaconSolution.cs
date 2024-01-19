using System.Collections.Generic;

namespace Knapcode.FactorioTools.OilField;

public record BeaconSolution(BeaconStrategy Strategy, ITableList<Location> Beacons, int Effects);