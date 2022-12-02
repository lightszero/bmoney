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
            return new string[] { N1.ToString(),N2.ToString() };
        }
        public void Init(string[] Param)
        {
            if (Param == null || Param.Length == 0)
            {
                N1 = 1;
                N2 = 20;
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
        IndicatorValueIndex depend_selfXX;
        public void OnReg(CandlePool pool)
        {
            depend_selfN1 = pool.GetIndicatorIndex("X_Vector", $"XVec({N1})");
            depend_selfN2 = pool.GetIndicatorIndex("X_Vector", $"XVec({N2})");
            depend_selfXX = pool.GetIndicatorIndex("X_Vector", $"XX");
        }

        //显示的指标
        public string[] GetValuesDefine()
        {
            return new string[] { $"XVec({N1})", $"XVec({N2})", $"XX$bar$0" };
        }


        public double[] CalcValues(CandlePool input, int indicatorIndex, int candleIndex)
        {
            double v1= IndicatorUtil.CalcMA(input, candleIndex, N1);
            double v2= IndicatorUtil.CalcMA(input, candleIndex, N2);

            //修改10天前的数据
            if (candleIndex - 10 >= 0)
            {
                input.Unsafe_SetHistoryValue(candleIndex - 10, depend_selfXX, v1);
            }
            
            //最新的数据没有意义
            double v1c = v1;
     
            return new double[] { v1, v2, v1c };
        }

    }
}
