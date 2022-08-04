using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney
{
    public struct Candle
    {
        public DateTime time;

        public double open;

        public double high;

        public double low;

        public double close;

        public double volume;

        public void Dump()
        {
            Console.WriteLine("Candle:" + time);
            Console.WriteLine("  open=" + open);
            Console.WriteLine("  high=" + high);
            Console.WriteLine("  low=" + low);
            Console.WriteLine("  close=" + close);
            Console.WriteLine("  volume=" + volume);
        }
    }
}
