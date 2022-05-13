using Binance.Net.Clients;
using Binance.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace marketspy
{
    public class Recorder
    {
        store store = new store();
        static BinanceClient rest = new BinanceClient();
        static BinanceSocketClient socket = new BinanceSocketClient();
        public string symbol
        {
            get;
            private set;
        }
        MarketDay lastday;//最后一天的历史数据

        class RecordReal
        {
            public Record record;
            public bool final;
        }
        class MarketDayRealtime
        {
            public bool newday;
            public DateTime begintime;
            public Dictionary<UInt16, RecordReal> records = new Dictionary<ushort, RecordReal>();
            public int min
            {
                get
                {
                    if (records.Count == 0) return -1;
                    for (var i = 0; i < 60 * 24; i++)
                    {
                        if (records.ContainsKey((UInt16)i))
                            return i;
                    }
                    return -1;
                }
            }
            public int max
            {
                get
                {
                    if (records.Count == 0) return -1;
                    for (var i = 60 * 24 - 1; i >= 0; i--)
                    {
                        if (records.ContainsKey((UInt16)i))
                            return i;
                    }
                    return -1;
                }
            }
            public Record? GetRecord(UInt16 index, out bool final)
            {
                final = false;
                if (records.ContainsKey(index) == false)
                    return null;
                final = records[index].final;
                return records[index].record;
            }
            public Record? GetLastResult(out UInt16 index, out bool final)
            {
                index = 0;
                final = false;
                int i = max;
                if (i < 0) return null;
                index = (UInt16)i;
                if (records.ContainsKey(index) == false)
                    return null;
                final = records[index].final;
                return records[index].record;
            }
        }
        MarketDayRealtime realday = new MarketDayRealtime();

        public MarketDay GetHistoryData(DateTime day)
        {
            var time = GetUtcDay(day);
            if (time == lastday.day)
                return lastday;
            else
                return store.GetDayData(time);
        }
        public Record? GetRealRecord(UInt16 index, out bool final)
        {
            return realday.GetRecord(index, out final);
        }
        public Record? GetLastRealRecord(out UInt16 index, out bool final)
        {
            return realday.GetLastResult(out index, out final);
        }
        bool[] currentDayTag = new bool[60 * 24];//实时模式标志位
        static void Log(string text)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("[Recorder]" + text);
        }
        static void LogHard(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[Recorder]" + text);
            Console.ForegroundColor = ConsoleColor.White;
        }
        static void LogError(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Recorder]" + text);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public DateTime startTime
        {
            get;
            private set;
        }
        public DateTime parseTime
        {
            get;
            private set;
        }
        public static DateTime GetUtcDay(DateTime time)
        {
            var ut = time.ToUniversalTime();
            return new DateTime(ut.Year, ut.Month, ut.Day, 0, 0, 0, DateTimeKind.Utc);
        }
        public void Begin(DateTime time, string symbol = "ethusdt")
        {
            this.symbol = symbol;
            parseTime = startTime = GetUtcDay(time);


            store.Open();
            LogHard("db opened");

            var begintime = store.GetStartTime();
            var endtime = store.GetEndTime();
            if (begintime != null)
            {
                if (parseTime < begintime.Value.ToUniversalTime())
                    throw new Exception("已经初始化过数据，而你要的时间太早了");
                parseTime = GetUtcDay(endtime.Value);
            }
            SocketRealtimeDataWatcher();
            RestHistoryDataWatcher();
        }
        async void RestHistoryDataWatcher()
        {

            while (true)
            {
                MarketDay currentDay = store.GetDayData(parseTime);
                if (currentDay == null)
                {
                    currentDay = new MarketDay();
                    currentDay.day = parseTime;
                    currentDay.records = new Record[60 * 24];
                    store.WriteDay(currentDay);
                }

                if (currentDay.count < 60 * 12)
                {
                    //缺上半天数据
                    await GetHistoryData(symbol, currentDay, parseTime);
                    if (currentDay.count >= 60 * 12)//够数才写入
                    {
                        try
                        {
                            store.UpdateDay(currentDay);
                        }
                        catch
                        {
                            store.WriteDay(currentDay);
                        }

                        Log("write halfday:" + parseTime + "[am]");
                    }

                }

                if (currentDay.count >= 60 * 12)//上半天数据完成才有下面的
                {
                    //缺下半天数据
                    var timepm = new DateTime(parseTime.Year, parseTime.Month, parseTime.Day, 12, 0, 0, DateTimeKind.Utc);
                    if (timepm < DateTime.Now && currentDay.count < 60 * 24)
                    {
                        await GetHistoryData(symbol, currentDay, timepm);
                        if (currentDay.count == 60 * 24)//够数才写入
                        {
                            store.UpdateDay(currentDay);
                            Log("write halfday:" + parseTime + "[pm]");
                        }
                    }
                }



                //如果数据补齐就下一天
                if (currentDay.count < 60 * 24)
                {
                    LogHard("历史数据已经追上");
                    var nowday = GetUtcDay(DateTime.Now);
                    while (nowday == parseTime)
                    {
                        await Task.Delay(1000);
                        nowday = GetUtcDay(DateTime.Now);
                    }
                }
                else
                {
                    lastday = currentDay.Clone();
                    //下半天数据补齐就可以跳走了
                    parseTime += TimeSpan.FromDays(1);
                }



            }
        }
        async void SocketRealtimeDataWatcher()
        {
            while (true)
            {
                var result = await socket.UsdFuturesStreams.SubscribeToKlineUpdatesAsync(symbol, Binance.Net.Enums.KlineInterval.OneMinute, (ev) =>
                {

                    var dat = ev.Data.Data;
                    var timedec = (dat.OpenTime.ToUniversalTime() - realday.begintime);
                    if (timedec.TotalDays > 1.0)//切换nowday
                    {
                        var nowday = GetUtcDay(DateTime.Now);

                        realday.begintime = nowday;
                        realday.records.Clear();
                        realday.newday = true;
                        timedec = (dat.OpenTime.ToUniversalTime() - realday.begintime);
                    }

                    var index = (UInt16)timedec.TotalMinutes;
                    var rec = new RecordReal();
                    realday.records[index] = rec;
                    var s = ev.Data.Symbol;

                    rec.record.price_high = (double)dat.HighPrice;
                    rec.record.price_low = (double)dat.LowPrice;
                    rec.record.price_open = (double)dat.OpenPrice;
                    rec.record.price_close = (double)dat.ClosePrice;
                    rec.record.volume = (double)dat.Volume;
                    rec.record.volume_buy = (double)dat.TakerBuyBaseVolume;
                    rec.final = dat.Final;
                });
                if (result.Error != null)
                {
                    Console.Error.WriteLine(result.Error.Message);
                    continue;
                }
                bool closed = false;
                result.Data.ConnectionClosed += () =>
                  {
                      closed = true;
                  };

                while (!closed)
                {
                    if (realday.newday)
                    {
                        await GetRealHistoryData(symbol, realday, realday.begintime);
                        var timepm = new DateTime(realday.begintime.Year, realday.begintime.Month, realday.begintime.Day, 12, 0, 0, DateTimeKind.Utc);
                        await GetRealHistoryData(symbol, realday, timepm);
                        realday.newday = false;
                    }
                    await Task.Delay(100);
                }
                LogHard("订阅断线，重联。");
            }

        }
        static async Task GetHistoryData(string symbol, MarketDay day, DateTime begin, int count = 60 * 12)
        {
            if (begin.Kind != DateTimeKind.Utc)
                throw new Exception("must use utctime");
            while (true)
            {
                var result = await rest.UsdFuturesApi.ExchangeData.GetKlinesAsync(symbol, Binance.Net.Enums.KlineInterval.OneMinute, begin, null, 60 * 12);
                if (result.Error != null)
                {

                    Console.Error.WriteLine(result.Error.Message);
                    continue;
                }
                var ubegin = begin;
                var uday = GetUtcDay(day.day);
                var beginindex = (UInt16)(ubegin - uday).TotalMinutes;
                IList<Binance.Net.Objects.Models.Futures.BinanceFuturesUsdtKline> datas = result.Data as IList<Binance.Net.Objects.Models.Futures.BinanceFuturesUsdtKline>;

                day.count = (UInt16)(beginindex + datas.Count);
                for (var i = beginindex; i < day.count; i++)
                {
                    day.records[i].price_open = (double)datas[i - beginindex].OpenPrice;
                    day.records[i].price_close = (double)datas[i - beginindex].ClosePrice;
                    day.records[i].price_high = (double)datas[i - beginindex].HighPrice;
                    day.records[i].price_low = (double)datas[i - beginindex].LowPrice;
                    day.records[i].volume = (double)datas[i - beginindex].Volume;
                    day.records[i].volume_buy = (double)datas[i - beginindex].TakerBuyBaseVolume;
                }
                return;
            }

        }
        static async Task GetRealHistoryData(string symbol, MarketDayRealtime day, DateTime begin, int count = 60 * 12)
        {
            if (begin.Kind != DateTimeKind.Utc)
                throw new Exception("must use utctime");
            while (true)
            {
                var result = await rest.UsdFuturesApi.ExchangeData.GetKlinesAsync(symbol, Binance.Net.Enums.KlineInterval.OneMinute, begin, null, 60 * 12);
                if (result.Error != null)
                {

                    Console.Error.WriteLine(result.Error.Message);
                    continue;
                }
                var ubegin = begin;
                var uday = GetUtcDay(day.begintime);
                var beginindex = (UInt16)(ubegin - uday).TotalMinutes;
                IList<Binance.Net.Objects.Models.Futures.BinanceFuturesUsdtKline> datas = result.Data as IList<Binance.Net.Objects.Models.Futures.BinanceFuturesUsdtKline>;

                var __count = (UInt16)(beginindex + datas.Count);
                for (var i = beginindex; i < __count; i++)
                {
                    var rec = new RecordReal();
                    day.records[i] = rec;
                    rec.record.price_open = (double)datas[i - beginindex].OpenPrice;
                    rec.record.price_close = (double)datas[i - beginindex].ClosePrice;
                    rec.record.price_high = (double)datas[i - beginindex].HighPrice;
                    rec.record.price_low = (double)datas[i - beginindex].LowPrice;
                    rec.record.volume = (double)datas[i - beginindex].Volume;
                    rec.record.volume_buy = (double)datas[i - beginindex].TakerBuyBaseVolume;
                    rec.final = true;
                }
                return;
            }

        }

    }
}
