using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney.Indicator
{

    
    public class Indicator_KDJ : IIndicator
    {
        public string Name => "KDJ";

        public string Description => "KDJ指标";

        public string[] GetInitParamDefine()
        {
            return new string[] { "N=9", "M1=3", "M2=3" };
        }
        public string[] GetValuesDefine()
        {
            return new string[] { $"K({N})", $"D({M1})", $"J({M2})" };
        }


        int N;
        int M1;
        int M2;
        public void Init(string[] Param)
        {
            if (Param == null || Param.Length == 0)
            {
                N = 9;
                M1 = 3;
                M2 = 2;
            }
            else
            {
                N = int.Parse(Param[0]);
                M1 = int.Parse(Param[1]);
                M2 = int.Parse(Param[2]);
            }
        }


        public double[] GetValues(CandlePool input, int indicatorIndex, int candleIndex)
        {
            IndicatorUtil.GetMinMaxPrice(input, candleIndex, N, out double min, out double max);
            var candle = input.GetCandle(candleIndex);
            double RSV = 0;
            double k = 0;
            double d = 0;
            double j = 0;
            double km1 = 0;
            double dm1 = 0;
            

            if (candleIndex > 0)
            {
                var candlem1 = input.GetCandleWithIndicator(candleIndex - 1);
                km1 = candlem1.values[indicatorIndex].value[0];
                dm1 = candlem1.values[indicatorIndex].value[1];
            }
            RSV = (candle.close - candle.low) / (candle.high - candle.low) * 100;
            k = (1 * RSV + (M1 - 1) * km1) / 1;
            d = (1 * k + (M2 - 1) * dm1) / 1;
            j = 3 * k - 2 * d;
            if (k < 0) k = 0;
            if (k > 100) k = 100;
            if (d < 0) d = 0;
            if (d > 100) d = 100;
            if (j < 0) j = 0;
            if (j > 100) j = 100;
            return new double[] { k, d, j };
        }

    }
}
