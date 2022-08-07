using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney.data
{
    //一个CSV文件导出、导入器
    public class CSVImporter : IImporter
    {
        Candle[] datas;
        public CSVImporter(string file)
        {
            LoadFromFile(file);
            Tick = CheckTick();
        }
        void LoadFromFile(string file)
        {
            var files = System.IO.File.ReadAllLines(file);
            datas = new Candle[files.Length];
            for (var i = 0; i < datas.Length; i++)
            {
                var l = files[i];
                if (string.IsNullOrEmpty(l))
                    continue;
                l = l.Replace("\t", "");
                var words = l.Split(",");
                if (words.Length >= 6)
                {
                    datas[i].time = CandleUtil.FromJSTime(double.Parse(words[0]));
                    datas[i].open = double.Parse(words[1]);
                    datas[i].high = double.Parse(words[2]);
                    datas[i].low = double.Parse(words[3]);
                    datas[i].close = double.Parse(words[4]);
                    datas[i].volume = double.Parse(words[5]);
                }
            }
        }

        public static void SaveToCSV(string file, CandlePool pool)
        {
            var lastid = pool.GetLastestCandleID(out bool final);
            string[] lines = new string[lastid + 1];
            for (int i = 0; i <= lastid; i++)
            {
                var candle = pool.GetCandle(i);
                lines[i] = CandleUtil.ToJSTime(candle.time) + "," + candle.open + "," + candle.high + "," + candle.low + "," + candle.close + "," + candle.volume;
            }
            System.IO.File.Delete(file);
            System.IO.File.WriteAllLines(file, lines);
        }
        TimeSpan CheckTick()
        {
            if (datas.Length < 2)
                throw new Exception("data is to less.");
            var tick0 = datas[1].time - datas[0].time;
            if (tick0.TotalMinutes < 1.0)
                throw new Exception("tick is wrong.");
            for (int i = 2; i < datas.Length; i++)
            {
                var tickn = datas[i].time - datas[i-1].time;
                if (tickn != tick0)
                    throw new Exception("tick is not same. from index:" + i);
            }
            return tick0;
        }
        public TimeSpan Tick
        {
            get;
            private set;
        }
        public void Start(CandlePool pool)
        {
            IsActive = true;
            for (var i = 0; i < datas.Length; i++)
            {
                pool.Push(datas[i], i < datas.Length - 1);
            }
            IsActive = false;
        }
        public bool IsActive
        {
            get;
            private set;
        }
        public bool IsAsync => false;
        public void Stop()
        {
            throw new Exception("this is not a async Importer.");
        }
    }
}
