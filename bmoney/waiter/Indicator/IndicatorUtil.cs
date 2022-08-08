using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney.Indicator
{
    internal class IndicatorUtil
    {
        public static void GetMinMaxPrice(CandlePool pool, int Index, int N, out double Min, out double Max)
        {
            double low = double.MaxValue;
            double high = double.MinValue;
            for (var i = Index + 1 - N; i <= Index; i++)
            {
                if (i < 0)
                {
                    //low = 0;
                    //high = 0;
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
        public static double[] CalcKDJ(CandlePool input, int kdjIndex, int candleIndex, int N = 9, int M1 = 3, int M2 = 3)
        {
            IndicatorUtil.GetMinMaxPrice(input, candleIndex, N, out double min, out double max);
            var candle = input.GetCandle(candleIndex);
            double RSV = 0;
            double k = 50;
            double d = 50;
            double j = 0;
            double km1 = 0;
            double dm1 = 0;


            if (candleIndex > 0)
            {
                var candlem1 = input.GetCandleWithIndicator(candleIndex - 1);
                km1 = candlem1.values[kdjIndex].value[0];
                dm1 = candlem1.values[kdjIndex].value[1];
            }
            RSV = (candle.close - min) / (max - min) * 100;
            k = km1 * (M1 - 1) / M1 + RSV / M1;
            d = dm1 * (M2 - 1) / M2 + k / M2;
           
            if (k < 0) k = 0;
            if (k > 100) k = 100;
            if (d < 0) d = 0;
            if (d > 100) d = 100;
            j = 3.0 * k - 2.0 * d;
            //if (j < 0) j = 0;
            //if (j > 100) j = 100;
            return new double[] { k, d, j };
        }
        public static double CalcEMA(CandlePool input, int techIndex, int candleIndex, int emaindex, int N)
        {
            var candle = input.GetCandle(candleIndex);
            var X = candle.close;

            double YM1 = 0;
            if (candleIndex > 0)
            {
                var candlem1 = input.GetCandleWithIndicator(candleIndex - 1);
                YM1 = candlem1.values[techIndex].value[emaindex];
            }
            else
            {
                return X;
            }
            var Y = (2 * X + (N - 1) * YM1) / (N + 1);
            //Y =[2 * X + (N - 1) * Y']/(N+1)
            return Y;

        }
        public static double CalcEMAFromX(CandlePool input, int techIndex, int candleIndex, int emaindex, double X, int N)
        {
            //var candle = input.GetCandleWithIndicator(candleIndex);
            //var X = candle.values[techIndex].value[difIndex];

            double YM1 = 0;
            if (candleIndex > 0)
            {
                var candlem1 = input.GetCandleWithIndicator(candleIndex - 1);
                YM1 = candlem1.values[techIndex].value[emaindex];
            }
            else
            {
                return X;
            }    
            var Y = (2 * X + (N - 1) * YM1) / (N + 1);
            //Y =[2 * X + (N - 1) * Y']/(N+1)
            return Y;

        }
    }
}
