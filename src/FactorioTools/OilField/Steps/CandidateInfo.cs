namespace Knapcode.FactorioTools.OilField.Steps;

public class CandidateInfo
{
    public CandidateInfo(CountedBitArray covered)
    {
        Covered = covered;
    }

    public CountedBitArray Covered;
    public double EntityDistance;
}