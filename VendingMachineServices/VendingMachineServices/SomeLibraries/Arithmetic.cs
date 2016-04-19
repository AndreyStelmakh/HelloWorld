using System.Collections.Generic;
using System.Linq;

namespace SomeLibraries
{
    public class ArithmeticDigit
    {
        uint _maxValue;
        uint _currentValue;

        public ArithmeticDigit(uint maxValue)
        {
            _maxValue = maxValue;
            _currentValue = maxValue;

        }

        public uint NextValue(out bool cDigit)
        {
            if(_currentValue == 0)
            {
                cDigit = true;

                _currentValue = _maxValue;

            }
            else
            {
                cDigit = false;

                _currentValue --;

            }

            return _currentValue;

        }

        public uint CurrentValue()
        {
            return _currentValue;

        }

    }

    public class ArithmeticRegister
    {

        // "картезианское произведение всех вариантов в разрядах", чтобы не просесть по памяти будем возвращать по одной комбинации за раз
        // при чем тут имеется сортировка - первыми пойдут комбинации соответствующие наибольшим числам (с самыми большими значениями во всех разрядах)
        public static IEnumerable<uint[]> AllCombinationsDescending(IEnumerable<uint> digits_max_values_collection_from_lowest)
        {
            var digits = (from t in digits_max_values_collection_from_lowest
                          select new ArithmeticDigit(t)).ToArray();

            while (true)
            {
                // вернём текущую комбинацию
                yield return digits.Select(p => p.CurrentValue()).ToArray();

                // флаг переноса между разрядами
                bool cDigit = true;

                for (int n = 0; n < digits.Count(); n++)
                {
                    if (cDigit)
                    {
                        digits[n].NextValue(out cDigit);

                    }

                }

                // если перенос возник в самом старшем разряде, значит всё - деньги кончились :(
                if (cDigit)
                {
                    yield break;

                }

            }

        }

    }

}
