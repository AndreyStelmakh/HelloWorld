using System.Collections.Generic;
using System.Linq;

namespace SomeLibraries
{
    /// <summary>
    /// Алгоритм минимизирующий кол-во монет при выдаче сдачи
    /// </summary>
    public class LessCoinsAlgorithm : ICoinAlgorithm
    {
        /// <summary>
        /// Алгоритм минимизирующий кол-во монет при выдаче сдачи
        /// </summary>
        /// <param name="availableCoins"></param>
        /// <param name="changeSum"></param>
        /// <returns></returns>
        public IEnumerable<CoinPack> CalculateChange(IEnumerable<CoinPack> availableCoins, uint changeSum)
        {
            // Некоторая оптимизация - не берем больше монет каждого номинала, чем может потребоваться на всю сумму
            // Кроме того сортировка, чтобы комбинации генерились с младших номиналов
            var min_quantities = availableCoins.Select(k =>
                {
                    uint c_lim = changeSum / k.Nominal;

                    if (c_lim > k.Quantity)
                    {
                        return new CoinPack(k.Nominal, k.Quantity);

                    }
                    else
                    {
                        return new CoinPack(k.Nominal, c_lim);

                    }

                })
                .OrderBy(j => j.Nominal)
                .ToArray();

            foreach (var combination in ArithmeticRegister.AllCombinationsDescending(from t in min_quantities select t.Quantity))
            {
                var change_try = combination.Select((quantity, i) => new CoinPack(min_quantities[i].Nominal, quantity)).ToArray();

                if (change_try.Sum(u => u.Sum) == changeSum)
                {
                    return change_try;

                }

            }

            throw new NoCombinationFoundException();

        }

    }

}
