using System.Collections.Generic;

namespace SomeLibraries
{
    public interface ICoinAlgorithm
    {
        IEnumerable<CoinPack> CalculateChange(IEnumerable<CoinPack> availableCoins, uint changeSum);

    }

}