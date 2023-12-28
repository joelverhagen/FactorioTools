using System.Runtime.CompilerServices;

namespace Knapcode.FactorioTools;

public static class SetVerifySettings
{
    [ModuleInitializer]
    public static void Initialize()
    {
#if DEBUG
        VerifierSettings.AutoVerify(includeBuildServer: false);
#endif
    }
}
