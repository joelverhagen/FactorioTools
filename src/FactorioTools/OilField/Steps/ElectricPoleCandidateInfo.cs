namespace Knapcode.FactorioTools.OilField.Steps;

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
