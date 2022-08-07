using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney.Indicator
{


    public class Indicator_EMA : IIndicator
    {
        public string Name => "EMA";

        public string Description => "EMA指标";

        public string[] GetInitParamDefine()
        {
            return new string[] { "N1=6", "N2=12", "N3=20" };
        }
        public string[] GetParamValue()
        {
            return new string[] { N1.ToString(), N2.ToString(), N3.ToString() };
        }
        public string[] GetValuesDefine()
        {
            return new string[] { $"EMA({N1})", $"EMA({N2})", $"EMA({N3})" };
        }


        int N1;
        int N2;
        int N3;
        public void Init(string[] Param)
        {
            if (Param == null || Param.Length == 0)
            {
                N1 = 6;
                N2 = 12;
                N3 = 20;
            }
            else
            {
                N1 = int.Parse(Param[0]);
                N2 = int.Parse(Param[1]);
                N3 = int.Parse(Param[2]);
            }
        }


        public double[] CalcValues(CandlePool input, int indicatorIndex, int candleIndex)
        {
            double ema1 = IndicatorUtil.CalcEMA(input, indicatorIndex, candleIndex, 0, N1);
            double ema2 = IndicatorUtil.CalcEMA(input, indicatorIndex, candleIndex, 1, N2);
            double ema3 = IndicatorUtil.CalcEMA(input, indicatorIndex, candleIndex, 2, N3);
            return new double[] { ema1, ema2, ema3 };
        }

    }
}
