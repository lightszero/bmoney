using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney.Indicator
{

    public interface IIndicator
    {
        public string Name { get;  }
        public string Description { get; }
        public void Init(string[] Param);
        public string[] GetInitParamDefine();
        public string[] GetParamValue();
        public string[] GetValuesDefine();
        public double[] CalcValues(CandlePool input, int indicatorIndex, int candleIndex);

    }
}
