using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney.Indicator
{

    //自定义指标都用X开头
    //这里主要是为了验证依赖其他指标
    public class Indicator_XValue : IIndicator
    {
        public string Name => "XValue";

        public string Description => "自定义指标XValue";

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
            return new string[] { "v1", "v2" };
        }


        int N1;
        int N2;
        int N3;

        IndicatorValueIndex depend_ema12;
        IndicatorValueIndex depend_dif;
        IndicatorValueIndex depend_dea;
        IndicatorValueIndex depend_macd;



        public void Init(string[] Param)
        {
            //参数初始化
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

        public void OnReg(CandlePool pool)
        {
            depend_ema12 = pool.GetIndicatorIndex("macd", "*ema(12)");
            depend_dif = pool.GetIndicatorIndex("macd", "dif");
            depend_dea = pool.GetIndicatorIndex("macd", "dea");
            depend_macd = pool.GetIndicatorIndex("macd", "macd");
        }
        public double[] CalcValues(CandlePool input, int indicatorIndex, int candleIndex)
        {
            return new double[] { 0, 0 };
        }

    }
}
