using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney
{
    public class CandleUtil
    {
        public static double ToJSTime(DateTime time)
        {
            var tick1 = time.ToUniversalTime();
            var from = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (tick1 - from).TotalMilliseconds;
        }
        public static DateTime FromJSTime(double v)
        {
            var from = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            from += TimeSpan.FromMilliseconds(v);
            return from.ToLocalTime();
        }
        public static double ToMinute(DateTime time)
        {
            var tick1 = time.ToUniversalTime();
            var from = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (tick1 - from).TotalMinutes;
        }
        public static void PushRandomData(CandlePool pool, DateTime begintime, TimeSpan tick, int count)
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

                candle.volume = (int)(ran.NextDouble() * 10000);
                pool.Push(candle, true);
            }
        }

        public static string FindFile(string filename)
        {
            //find html for show
            var path = System.IO.Path.GetDirectoryName(typeof(HtmlView).Assembly.Location);


            var htmlpath = System.IO.Path.Combine(path, filename);
            while (!System.IO.File.Exists(htmlpath))
            {
                var newpath = System.IO.Path.GetDirectoryName(path);
                if (path == newpath)
                    throw new Exception("not found.");
                path = newpath;
                htmlpath = System.IO.Path.Combine(path, filename);
            }
            return htmlpath;
        }
    }
}
