using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney.Indicator
{
    internal class IndicatorUtil
    {
        public static void GetMinMaxPrice(CandlePool pool,int Index, int N, out double Min,out double Max)
        {
            double low = double.MaxValue;
            double high = double.MinValue;
            for (var i = Index - N; i <= Index; i++)
            {
                if (i < 0)
                {
                    low = 0;
                    high = 0;
                }
                else
                {
                    var c = pool.GetCandle(i);
                    if (c.low < low) low = c.low;
                    if (c.high > high) high = c.high;
                }
            }
            Min = low;
            Max = high;
        }
    }
}
