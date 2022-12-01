using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BMoney.Indicator
{


    public class Indicator_X_Vector : IIndicator
    {
        public string Name => "X_Vector";

        public string Description => "Vector指标,自定义,反应涨跌情况";



        //参数
        //通过 GetInitParamDefine GetParamValue 和 Init 实现
        int N1;
        int N2;
        public string[] GetInitParamDefine()
        {
            return new string[] { "N1=20", "N2=60" };
        }
        public string[] GetParamValue()
        {
            return new string[] { N1.ToString() };
        }
        public void Init(string[] Param)
        {
            if (Param == null || Param.Length == 0)
            {
                N1 = 20;
                N2 = 60;
            }
            else
            {
                N1 = int.Parse(Param[0]);
                N2 = int.Parse(Param[1]);
            }
        }
        //如果依赖其他指标，在这里找出来
        IndicatorValueIndex depend_selfN1;
        IndicatorValueIndex depend_selfN2;
        public void OnReg(CandlePool pool)
        {
            depend_selfN1 = pool.GetIndicatorIndex("X_Vector", $"XVec({N1})");
            depend_selfN2 = pool.GetIndicatorIndex("X_Vector", $"XVec({N2})");
        }

        //显示的指标
        public string[] GetValuesDefine()
        {
            return new string[] { $"XVec({N1})", $"XVec({N2})", $"XX$bar$0" };
        }


        public double[] CalcValues(CandlePool input, int indicatorIndex, int candleIndex)
        {
            IndicatorUtil.GetMinMaxPrice(input, candleIndex, N1, out double min1, out double max1, out double open1, out double close1);
            IndicatorUtil.GetMinMaxPrice(input, candleIndex, N2, out double min2, out double max2, out double open2, out double close2);
            double v1 = close1 - open1; //涨跌趋势
            v1 = v1 / (v1 < 0 ? open1 : close1) * 1000.0;
            double v2 = close2 - open2; //涨跌趋势
            v2 = v2 / (v2 < 0 ? open2 : close2) * 1000.0;


            double v1c = 0;//强弱趋势


            if (-3 < v1 && v1 < 3)
            {
                v1 = 0;
            }
            if (-3 < v2 && v2 < 3)
            {
                v2 = 0;
            }
            int sign1 = Math.Sign(v1);
            int sign2 = Math.Sign(v2);
            if (sign1 == sign2 && sign1 != 0)
            {


                for (int i = 1; i <= N1; i++)
                {
                    var lastindex = candleIndex - i;
                    if (lastindex < 0)
                        break;
                    var lastv1 = IndicatorUtil.GetFromIndex(input, depend_selfN1, lastindex);//不能取当前candle 还没入
                    var lastv2 = IndicatorUtil.GetFromIndex(input, depend_selfN2, lastindex);//不能取当前candle 还没入
                    int slast1 = Math.Sign(lastv1);
                    int slast2 = Math.Sign(lastv2);
                    if (
                        slast1 == slast2 && slast1 == sign1)

                    {
                        v1c++;
                    }
                    else
                    {
                        break;
                    }
                }
                //把1空出来
                if (v1c != 0)
                    v1c = 2 + v1c * sign1;
                
            }
            return new double[] { v1, v2, v1c };
        }

    }
}
