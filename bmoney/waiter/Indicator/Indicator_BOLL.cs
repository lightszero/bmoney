using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney.Indicator
{


    public class Indicator_BOLL : IIndicator
    {
        public string Name => "BOLL";

        public string Description => "BOLL指标";

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
            return new string[] { "MB","UP","DN","CLOSE" };
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

        public double[] CalcValues(CandlePool input, int indicatorIndex, int candleIndex)
        {
            double ma = IndicatorUtil.CalcMA(input, candleIndex, N1);//中轨线

            double close = input.GetCandle(candleIndex).close;
            double cha0 = close  - ma;
            double total = cha0*cha0;  //(C-MA)的平方  累积N日
            int useN = 1;
            for(var i=1;i<N1;i++)
            {
                if (candleIndex - i >= 0)
                {
                    useN++;
                    double malast = input.Unsafe_GetHistoryValue(candleIndex - i, depend_selfMB);
                    double cha = input.GetCandle(candleIndex - i).close - malast;
                    total += cha * cha;
                }
                else
                {
                    break;
                }
            }
           
            double md = Math.Sqrt(total / useN);
            double up = ma+md*2;
            double dn = ma-md*2;
            return new double[] { ma,up,dn,close};
        }

    }
}
