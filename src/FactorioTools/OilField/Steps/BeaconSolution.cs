using System.Collections.Generic;
using Knapcode.FactorioTools.OilField.Grid;

namespace Knapcode.FactorioTools.OilField.Steps;

public record BeaconSolution(BeaconStrategy Strategy, List<Location> Beacons, int Effects);