using System.Collections.Generic;
using System.Linq;

namespace Android.BLE
{
    public static class IntExtensions
    {
        public static int[] GetDivisors(this int origin, int divisor)
        {
            List<int> result = new List<int>() { origin };
            return GetRecursiveDivisor(result, divisor).ToArray();
        }

        private static List<int> GetRecursiveDivisor(List<int> list, int divisor)
        {
            int divided = list[0] / divisor;
            list.AddRange(Enumerable.Repeat(divisor, divided));

            list[0] %= divisor;

            if (list[0] <= 0)
            {
                list.RemoveAt(0);
                return list;
            }
            else
            {
                return GetRecursiveDivisor(list, divisor / 2);
            }
        }
    }
}
