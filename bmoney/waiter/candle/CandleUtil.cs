using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney
{
    public class CandleUtil
    {
        public static void PushRandomData(CandlePool pool,DateTime begintime,TimeSpan tick,int count)
        {
            Random ran = new Random();
            var begin = begintime.ToLocalTime();
            for (var i = 0; i < count; i++)
            {
                var span = tick;

                Candle candle;
                candle.time = begin + span * i;
                candle.open = ran.NextDouble() * 100;
                candle.close = ran.NextDouble() * 100;
                candle.high = Math.Max(candle.open, candle.close) + ran.NextDouble() * 100;
                candle.low = Math.Min(candle.open, candle.close) - ran.NextDouble() * 100;
                if (candle.low < 0)
                    candle.low = 0;

                candle.volume = ran.NextDouble();
                pool.Push(candle, true);
            }
        }
    }
}
