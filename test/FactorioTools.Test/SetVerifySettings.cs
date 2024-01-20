using System.Runtime.CompilerServices;

namespace Knapcode.FactorioTools;

public static class SetVerifySettings
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifyDiffPlex.Initialize();
        VerifierSettings.AutoVerify(includeBuildServer: false);
    }
}
