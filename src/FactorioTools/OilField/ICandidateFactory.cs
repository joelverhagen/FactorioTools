namespace Knapcode.FactorioTools.OilField;

public interface ICandidateFactory<TInfo> where TInfo : CandidateInfo
{
    TInfo Create(CountedBitArray covered);
}