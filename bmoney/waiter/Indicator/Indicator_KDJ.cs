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
        public string[] GetParamValue()
        {
            return new string[] { N.ToString(), M1.ToString(), M2.ToString() };
        }
        public string[] GetValuesDefine()
        {
            return new string[] { "K", "D", "J" };
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
            return IndicatorUtil.CalcKDJ(input, indicatorIndex, candleIndex, N, M1, M2);
        }

    }
}
