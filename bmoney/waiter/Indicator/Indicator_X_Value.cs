using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney.Indicator
{

    //自定义指标都用X开头
    //这里主要是为了验证依赖其他指标
    public class Indicator_X_Value : IIndicator
    {
        public string Name => "X_Value";

        public string Description => "自定义指标XValue 强弱标志";

        public string[] GetInitParamDefine()
        {
            return new string[] { "N1=10", "N2=20", "N3=60" };
        }
        public string[] GetParamValue()
        {
            return new string[] { N1.ToString(), N2.ToString(), N3.ToString() };
        }
        public string[] GetValuesDefine()
        {
            return new string[] { $"v1({N1})", $"v2({N2})", $"v3({N3})", "Total$bar$0","Total2$bar$0" };
        }


        int N1;
        int N2;
        int N3;

        IndicatorValueIndex depend_ema12;



        public void Init(string[] Param)
        {
            //参数初始化
            if (Param == null || Param.Length == 0)
            {
                N1 = 10;
                N2 = 20;
                N3 = 60;
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
        }
        public double[] CalcValues(CandlePool input, int indicatorIndex, int candleIndex)
        {

            if (candleIndex > N3)
            {
                //用均价计算
                var ema = IndicatorUtil.GetFromIndex(input, depend_ema12, candleIndex);
                var ema_m1 = IndicatorUtil.GetFromIndex(input, depend_ema12, candleIndex - N1);
                var ema_m2 = IndicatorUtil.GetFromIndex(input, depend_ema12, candleIndex - N2);
                var ema_m3 = IndicatorUtil.GetFromIndex(input, depend_ema12, candleIndex - N3);

                var v1 = (ema - ema_m1) / ema_m1 * 100;
                var v2 = (ema - ema_m2) / ema_m2 * 100;
                var v3 = (ema - ema_m3) / ema_m3 * 100;

                //用收盘价计算
                //var cm = input.GetCandle(candleIndex).close;
                //var cm1 = input.GetCandle(candleIndex - N1).close;
                //var cm2 = input.GetCandle(candleIndex - N2).close;
                //var cm3 = input.GetCandle(candleIndex - N3).close;

                //var v1 = (cm - cm1) / cm1 * 100;
                //var v2 = (cm - cm2) / cm2 * 100;
                //var v3 = (cm - cm3) / cm3 * 100;

                double vtotal = 0;
                var s1 = Math.Sign(v1);
                var s2 = Math.Sign(v2);
                var s3 = Math.Sign(v3);

                {
                    vtotal = (v1 + v2 + v3) / 3.0;
                }
                var vtotal2 = vtotal;
                if (s1 == s2 && s1 == s3)
                {
                    vtotal2 = vtotal;
                }
                else
                {
                    vtotal2 = -vtotal;
                }
                return new double[5] { v1, v2, v3, vtotal,vtotal2 };
            }
            return new double[5];
        }

    }
}
