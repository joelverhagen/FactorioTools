namespace Knapcode.FactorioTools.OilField;

public class ElectricPoleCandidateInfo : CandidateInfo
{
    public ElectricPoleCandidateInfo(CountedBitArray covered) : base(covered)
    {
    }

    public int PriorityPowered;
    public int OthersConnected;
    public int PoleDistance;
    public int MiddleDistance;
}
