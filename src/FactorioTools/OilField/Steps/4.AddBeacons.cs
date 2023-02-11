using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Knapcode.FactorioTools.OilField.Steps.Helpers;

namespace Knapcode.FactorioTools.OilField.Steps;

internal static class AddBeacons
{
    public static void Execute(Context context)
    {
        var centerList = context.CenterToTerminals.Keys.ToList();
        var candidateToCovered = GetCandidateToCovered(context, centerList, context.Options.BeaconWidth, )
    }
}
