using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Waiter;

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
            return new string[] { N1.ToString(), N2.ToString() };
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
            var v = input.GetCandle(candleIndex).close;
            double v1 = IndicatorUtil.CalcMA(input, candleIndex, N1);
            double v2;


            //Load sample data
            float last = 0;
            float[] close = new float[60];
            for (var i = 0; i < 60; i++)
            {
                if (candleIndex - (59-i) < 0)
                {
                    close[i] = last;
                }
                else
                {
                    close[i] = (float)input.GetCandle(candleIndex - (59-i)).close;
                    last = close[i];
                }
            }

            var sampleData = new Ml_close.ModelInput()
            {
                K0000 = close[00],
                K0001 = close[01],
                K0002 = close[02],
                K0003 = close[03],
                K0004 = close[04],
                K0005 = close[05],
                K0006 = close[06],
                K0007 = close[07],
                K0008 = close[08],
                K0009 = close[09],
                K0010 = close[10],
                K0011 = close[11],
                K0012 = close[12],
                K0013 = close[13],
                K0014 = close[14],
                K0015 = close[15],
                K0016 = close[16],
                K0017 = close[17],
                K0018 = close[18],
                K0019 = close[19],
                K0020 = close[20],
                K0021 = close[21],
                K0022 = close[22],
                K0023 = close[23],
                K0024 = close[24],
                K0025 = close[25],
                K0026 = close[26],
                K0027 = close[27],
                K0028 = close[28],
                K0029 = close[29],
                K0030 = close[30],
                K0031 = close[31],
                K0032 = close[32],
                K0033 = close[33],
                K0034 = close[34],
                K0035 = close[35],
                K0036 = close[36],
                K0037 = close[37],
                K0038 = close[38],
                K0039 = close[39],
                K0040 = close[40],
                K0041 = close[41],
                K0042 = close[42],
                K0043 = close[43],
                K0044 = close[44],
                K0045 = close[45],
                K0046 = close[46],
                K0047 = close[47],
                K0048 = close[48],
                K0049 = close[49],
                K0050 = close[50],
                K0051 = close[51],
                K0052 = close[52],
                K0053 = close[53],
                K0054 = close[54],
                K0055 = close[55],
                K0056 = close[56],
                K0057 = close[57],
                K0058 = close[58],
                K0059 = close[59],
            };

            //Load model and predict output
            var result = Ml_close.Predict(sampleData);
            v2 = result.Score;
            //修改10天前的数据
            if (candleIndex - 10 >= 0)
            {
                input.Unsafe_SetHistoryValue(candleIndex - 10, depend_selfXX, v);
            }

            //最新的数据没有意义
            double v1c = v1;

            return new double[] { v1, v2, v1c };
        }

    }
}
