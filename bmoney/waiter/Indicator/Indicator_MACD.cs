using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney.Indicator
{


    public class Indicator_MACD : IIndicator
    {
        public string Name => "MACD";

        public string Description => "MACD指标";

        public string[] GetInitParamDefine()
        {
            return new string[] { "N1=12", "N2=26", "N3=9" };
        }
        public string[] GetParamValue()
        {
            return new string[] { N1.ToString(), N2.ToString(), N3.ToString() };
        }
        public string[] GetValuesDefine()
        {
            return new string[] { $"*EMA({N1})", $"*EMA({N2})","DIF","DEA","MACD$bar$0" };
        }


        int N1;
        int N2;
        int N3;
        public void Init(string[] Param)
        {
            if (Param == null || Param.Length == 0)
            {
                N1 = 12;
                N2 = 26;
                N3 = 9;
            }
            else
            {
                N1 = int.Parse(Param[0]);
                N2 = int.Parse(Param[1]);
                N3 = int.Parse(Param[2]);
            }
        }


        public double[] GetValues(CandlePool input, int indicatorIndex, int candleIndex)
        {
            double ema1 = IndicatorUtil.CalcEMA(input, indicatorIndex, candleIndex, 0, N1);
            double ema2 = IndicatorUtil.CalcEMA(input, indicatorIndex, candleIndex, 1, N2);
            double dif = ema1 - ema2;
            int deaindex = 3;
            double dea = IndicatorUtil.CalcEMAFromX(input, indicatorIndex,  candleIndex, deaindex,dif,  N3);
            double bar = (dif - dea) * 2;
            return new double[] { ema1, ema2, dif, dea ,bar};
        }

    }
}
