using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SomeLibraries
{
    /// <summary>
    /// Символизирует кошелек, в котором монеты хранятся в кучкахъ сгруппированными по достоинству
    /// </summary>
    public class Wallet : IEnumerable<CoinPack>
    {
        Dictionary<uint, CoinPack> _availableCoins = new Dictionary<uint, CoinPack>();

        public Wallet()
        {

        }

        public Wallet(IEnumerable<CoinPack> cPacks)
        {
            // если во входящей последовательности некоторые номиналы придут неск. раз, то это не дело
            // сольём их вместе
            _availableCoins = cPacks.GroupBy(a => a.Nominal)
                                    .Select(b => new CoinPack(b.First().Nominal,
                                                            b.Select(u => u.Quantity).Aggregate((c, d) => c + d)))
                                    .ToDictionary(p=> p.Nominal, q => q);

        }

        public void Clear()
        {
            _availableCoins.Clear();

        }

        public void Add(uint nominal, uint quantity)
        {
            if (!_availableCoins.ContainsKey(nominal))
            {
                _availableCoins.Add(nominal, new CoinPack(nominal, quantity));

            }
            else
            {
                _availableCoins[nominal].Quantity += quantity;

            }

        }

        public void Remove(uint nominal, uint quantity)
        {
            if(_availableCoins.ContainsKey(nominal))
            {
                if(_availableCoins[nominal].Quantity >= quantity)
                {
                    _availableCoins[nominal].Quantity -= quantity;

                    return;

                }

            }

            throw new KnownException("В кошельке отсутствуют монеты указанного номинала.");

        }

        public void Add(IEnumerable<CoinPack> coinPacks)
        {
            foreach (var item in coinPacks)
            {
                Add(item.Nominal, item.Quantity);

            }

        }

        public void Remove(IEnumerable<CoinPack> coinPacks)
        {
            foreach (var item in coinPacks)
            {
                Remove(item.Nominal, item.Quantity);

            }

        }

        /// <summary>
        /// Возвращает найденный вариант набора указанной суммы монетами имеющимися в кошельке
        /// </summary>
        /// <typeparam name="T">алгоритм набора монет</typeparam>
        /// <param name="sum"></param>
        /// <returns></returns>
        public IEnumerable<CoinPack> CalculateChange<T>(uint sum) where T : ICoinAlgorithm, new()
        {
            return (new T() as ICoinAlgorithm)
                    .CalculateChange(_availableCoins.Values, sum)
                    .Where(p => p.Quantity != 0);

        }

        public IEnumerator<CoinPack> GetEnumerator()
        {
            return _availableCoins.Values.GetEnumerator();

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _availableCoins.Values.GetEnumerator();

        }

        public long Sum
        {
            get
            {
                return _availableCoins.Values.Sum(p => p.Quantity * p.Nominal);

            }

        }

    }

    /// <summary>
    /// Символизирует кучку монет одинакового достоинства
    /// </summary>
    public class CoinPack
    {
        public uint Nominal;

        public uint Quantity;

        public uint Sum
        {
            get
            {
                return Nominal * Quantity;

            }

        }

        public CoinPack(uint nominal, uint quantity)
        {
            Nominal = nominal;
            Quantity = quantity;

        }

        public override string ToString()
        {
            return string.Format("N:{0} Q:{1}", Nominal, Quantity);

        }

    }

}
