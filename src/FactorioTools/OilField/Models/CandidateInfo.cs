namespace Knapcode.FactorioTools.OilField;

public class CandidateInfo
{
    public CandidateInfo(CountedBitArray covered)
    {
        Covered = covered;
    }

    public CountedBitArray Covered;
    public double EntityDistance;
}