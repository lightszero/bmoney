using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney.Indicator
{


    public class Indicator_CCI : IIndicator
    {
        public string Name => "CCI";

        public string Description => "CCI指标";

        public string[] GetInitParamDefine()
        {
            return new string[] { "N1=20" };
        }
        public string[] GetParamValue()
        {
            return new string[] { N1.ToString() };
        }
        public string[] GetValuesDefine()
        {
            return new string[] { "CCI","TU","TD" };
        }

        int N1;
        public void Init(string[] Param)
        {
            if (Param == null || Param.Length == 0)
            {
                N1 = 20;
            }
            else
            {
                N1 = int.Parse(Param[0]);
            }
        }
        IndicatorValueIndex depend_selfMB;
        public void OnReg(CandlePool pool)
        {
            depend_selfMB = pool.GetIndicatorIndex("BOLL", "MB");
        }

        double typ(Candle candle)
        {
            return (candle.open + candle.close + candle.low) / 3;
        }

        public double[] CalcValues(CandlePool input, int indicatorIndex, int candleIndex)
        {
            var candlenow = input.GetCandle(candleIndex);
            var typ_now = typ(candlenow);
            // CCI: (TYP - MA(TYP, N)) / (0.015 * AVEDEV(TYP, N));
            var cci = (typ_now - IndicatorUtil.CalcMAFunc(input, candleIndex, N1, typ))
                / (0.015 * IndicatorUtil.CalcAVEDEVFunc(input, candleIndex, N1, typ)
                );

            return new double[] { cci, 100,-100};
        }

    }
}
