using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney
{

    public class LastestInfo
    {
        public DateTime LastTime;
        public bool Final;
    }
    public class CandlePool
    {
        public CandlePool(string name, TimeSpan tick)
        {
            this.Name = name;
            this.Tick = tick;
            this.BeginTime = null;
            LastestInfo = null;
        }
        public string Name
        {
            get;
            private set;
        }
        public DateTime? BeginTime
        {
            get;
            private set;
        }
        public TimeSpan Tick
        {
            get;
            private set;
        }
        public LastestInfo LastestInfo
        {
            get;
            private set;
        }

        public bool IsBegin()
        {
            return BeginTime != null;
        }
        List<Indicator.IIndicator> regdIndicator = new List<Indicator.IIndicator>();
        List<Candle> candles = new List<Candle>();
        List<double[][]> valueIndicator = new List<double[][]>();
        public int CandleTimeToID(DateTime time)
        {
            var ltime = time.Kind == DateTimeKind.Utc ? time.ToLocalTime() : time;

            if (BeginTime == null) return -1;
            if (ltime < BeginTime) return -2;
            var id = (ltime - BeginTime).Value / Tick;
            return (int)id;
        }
        public void RegIndicator(Indicator.IIndicator ind)
        {
            if (ind == null) throw new Exception("error IIndicator");
            foreach (var i in regdIndicator)
            {
                if (i.Name == ind.Name)
                    throw new Exception("already have this IIndicator:" + ind.Name);
            }
            if (regdIndicator.Contains(ind)) throw new Exception("already have this IIndicator:" + ind.Name);
            if (IsBegin())
                throw new Exception("RegIndicator need before Push any Data.");
            regdIndicator.Add(ind);
            ind.OnReg(this);

        }
        public void Push(Candle candle, bool final)
        {
            var ltime = candle.time.Kind == DateTimeKind.Utc ? candle.time.ToLocalTime() : candle.time;
            if (BeginTime == null)
            {
                BeginTime = candle.time;
                LastestInfo = new LastestInfo();
                LastestInfo.LastTime = ltime;
                LastestInfo.Final = final;
            }
            var index = CandleTimeToID(ltime);
            if (index > candles.Count || index < candles.Count - 1)
                throw new Exception("CandlePool.Push数据不连续:" + index + " count=" + candles.Count);
            if (index == candles.Count - 1)//update last
            {
                if (LastestInfo != null && LastestInfo.Final)
                    throw new Exception("CandlePool.Push 尝试更新已经final的数据:" + index);
                candles[index] = candle;
                LastestInfo.Final = final;
            }
            if (index == candles.Count)
            {
                candles.Add(candle);
                //var indexcheck = candles.IndexOf(candle);
                //if (indexcheck != index)
                //    throw new Exception("CandlePool.Push 索引已损坏:" + index);
                //new
                LastestInfo.LastTime = ltime;
                LastestInfo.Final = final;
            }

            //计算指标
            if (index == valueIndicator.Count)
            {
                valueIndicator.Add(new double[regdIndicator.Count][]);
            }
            for (var i = 0; i < regdIndicator.Count; i++)
            {
                double[] v = regdIndicator[i].CalcValues(this, i, index);
                valueIndicator[index][i] = v;
            }
        }

        public Candle GetCandle(int index)
        {
            return candles[index];
        }
        public CandleWithIndicator GetCandleWithIndicator(int index)
        {
            IndicatorInfo[] vinfos = new IndicatorInfo[regdIndicator.Count];
            for (var i = 0; i < regdIndicator.Count; i++)
            {
                var vinfo = new IndicatorInfo() { indicator = regdIndicator[i], value = valueIndicator[index][i] };
                vinfos[i] = vinfo;
            }

            var v = new CandleWithIndicator() { candle = candles[index], values = vinfos };
            return v;
        }
        /// <summary>
        /// 危险操作，慎用
        /// </summary>
        /// <param name="candleindex"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void Unsafe_SetHistoryValue(int candleindex, IndicatorValueIndex index, double value)
        {
            valueIndicator[candleindex][index.IndicatorIndex][index.ValueIndex] = value;
        }
        public double Unsafe_GetHistoryValue(int candleindex, IndicatorValueIndex index)
        {
            return valueIndicator[candleindex][index.IndicatorIndex][index.ValueIndex];
        }
        public IndicatorValueIndex GetIndicatorIndex(string IndicatorName, string ValueName = null)
        {
            ValueName = ValueName.ToLower();
            IndicatorValueIndex index = new IndicatorValueIndex();
            index.IndicatorName = IndicatorName;
            index.ValueName = ValueName;
            index.IndicatorIndex = -1;
            for (var i = 0; i < regdIndicator.Count; i++)
            {
                if (regdIndicator[i].Name.ToLower() == IndicatorName.ToLower())
                {
                    index.Indicator = regdIndicator[i];
                    index.IndicatorIndex = i;
                    break;
                }
            }
            if (index.IndicatorIndex < 0)
                throw new Exception("not found IndicatorName:" + IndicatorName);

            index.ValueIndex = -1;
            if (ValueName != null)
            {
                var vs = index.Indicator.GetValuesDefine();
                for (var i = 0; i < vs.Length; i++)
                {
                    var idoller = vs[i].IndexOf('$');
                    var name = vs[i].ToLower();
                    if (idoller > 0)
                        name = name.Substring(0, idoller);
                    if (name == ValueName)
                    {
                        index.ValueIndex = i;
                        break;
                    }

                }
                if (index.ValueIndex < 0)
                    throw new Exception("not found ValueName.");
            }

            return index;

        }
        public int GetLastestCandleID(out bool final)
        {
            final = LastestInfo.Final;
            return candles.Count - 1;
        }
        public int GetLastestCandle(out Candle candle, out bool final)
        {

            if (LastestInfo == null)
            {
                candle = new Candle();
                final = false;
                return -2;
            }

            var index = CandleTimeToID(LastestInfo.LastTime);
            if (index < 0)
            {
                candle = new Candle();
                final = false;
                return -1;
            }
            if (index >= candles.Count)
                throw new Exception("CandlePool.GetLastestCandle Error Candle");
            candle = GetCandle(index);
            final = LastestInfo.Final;

            return index;
        }
        public void Dump()
        {
            Console.WriteLine("CandlePool:" + Name + " from:" + BeginTime + " Tick:" + Tick);

            int id = GetLastestCandleID(out bool f1);
            Console.WriteLine("LastestCandleID=" + id);
            var data = GetCandleWithIndicator(id);
            data.Dump();
            //int i = GetLastestCandle(out Candle c, out bool f);
            //if (i > 0)
            //{
            //    Console.WriteLine("last Candle =" + i + " final=" + f);
            //    c.Dump();
            //}
        }
        public void GenDatasForML(string filename, IndicatorValueIndex value, int N)
        {
            int id = GetLastestCandleID(out bool f1);
            StringBuilder total = new StringBuilder();
            StringBuilder line = new StringBuilder();
            for(var i=0;i<N;i++)
            {
                line.Append("k" + i.ToString("D4") + ",");
            }
            line.Append("will");
            total.Append(line.ToString());
            total.AppendLine();
            for (var i =N;i<id-N;i++)
            {
                line.Clear();

                for (var j = 1;j<=N;j++) //1~60
                {
                    var c = this.GetCandle(i - N + j);
                    line.Append(c.close + ",");
                }
                line.Append(Unsafe_GetHistoryValue(i, value));

                total.Append(line.ToString());
                total.AppendLine();
            }
            System.IO.File.WriteAllText(filename,total.ToString());
        }
    }
}
