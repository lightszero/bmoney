using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney
{

    public class IndicatorInfo
    {
        public Indicator.IIndicator indicator;//反正都不是struct了，破罐子破摔
        public double[] value;
    }
    //包含指标的Candle图
    public class CandleWithIndicator
    {
        public Candle candle;
        public IndicatorInfo[] values;
        public void Dump()
        {
            candle.Dump();
            foreach (var v in values)
            {
                Console.WriteLine("====V===" + v.indicator.Name);
                var names = v.indicator.GetValuesDefine();
                for (var i = 0; i < v.value.Length; i++)
                {
                    var dv = v.value[i];
                    var name = names[i];
                    Console.WriteLine(name + "[" + i + "]=" + dv);
                }

            }
        }
    }
}
