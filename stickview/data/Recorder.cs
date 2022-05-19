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
        static BinanceClient rest = new BinanceClient();
        static BinanceSocketClient socket = new BinanceSocketClient();
        public string symbol
        {
            get;
            private set;
        }

        Dictionary<int, CachedRecordArray> cachemap = new Dictionary<int, CachedRecordArray>();
        class RecordReal
        {
            public CacheRecord record;
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
            public CacheRecord? GetRecord(UInt16 index, out bool final)
            {
                final = false;
                if (records.ContainsKey(index) == false)
                    return null;
                final = records[index].final;
                return records[index].record;
            }
            public CacheRecord? GetLastResult(out UInt16 index, out bool final)
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

        public CacheRecord? GetHistoryData(DateTime day)
        {
            var time = TimeTool.GetUtcYear(day);
            int mindex = (int)((day.ToUniversalTime() - time).TotalMinutes);
            if (cachemap.ContainsKey(time.Year) == false)
                return null;
            return cachemap[time.Year].GetItem(mindex);
        }

        public CacheRecord? GetRealRecord(UInt16 index, out bool final)
        {
            return realday.GetRecord(index, out final);
        }
        public CacheRecord? GetLastRealRecord(out UInt16 index, out bool final)
        {
            return realday.GetLastResult(out index, out final);
        }
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


        public void Begin(int beginyear, string symbol = "ethusdt")
        {
            this.symbol = symbol;
            parseTime = startTime = new DateTime(beginyear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            int nowyear = TimeTool.GetNowYear();


            for (var i = beginyear; i <= nowyear; i++)
            {
                var name = i + "_kline_m1";
                var cacefile = new CachedRecordArray(name, 366 * 24 * 60);
                cachemap.Add(i, cacefile);
                cacefile.Load();
                var yeartime = new DateTime(i, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var cacheendtime = yeartime + TimeSpan.FromMinutes(cachemap[i].Count);
                if (cachemap[i].Count > 0)
                {
                    parseTime = TimeTool.GetUtcDay(cacheendtime);
                }
            }

            LogHard("db inited parseTime=" + parseTime);

            SocketRealtimeDataWatcher();
            RestHistoryDataWatcher();
        }

        async void RestHistoryDataWatcher()
        {
            CacheRecord[] records = new CacheRecord[60 * 24];
            int recordcount = 0;

            while (true)
            {


                if (recordcount < 60 * 12)
                {
                    //缺上半天数据
                    int count = await GetHistoryData(symbol, records, parseTime);
                    recordcount = count;
                    if (recordcount >= 60 * 12)//够数才写入
                    {
                        cachemap[parseTime.Year].UpdateRecords(records, 0, 60 * 12);
                        cachemap[parseTime.Year].Apply();
                        Log("write halfday:" + parseTime + "[am]");
                      
                    }

                }

                if (recordcount >= 60 * 12)//上半天数据完成才有下面的
                {
                    //缺下半天数据
                    var timepm = new DateTime(parseTime.Year, parseTime.Month, parseTime.Day, 12, 0, 0, DateTimeKind.Utc);
                    if (timepm < DateTime.Now && recordcount < 60 * 24)
                    {
                        int count = await GetHistoryData(symbol, records, timepm);
                        recordcount = 60 * 12 + count;
                        if (recordcount == 60 * 24)//够数才写入
                        {
                            cachemap[parseTime.Year].UpdateRecords(records,720,60*12);
                            cachemap[parseTime.Year].Apply();
                            Log("write halfday:" + parseTime + "[pm]");
                        }
                    }
                }



                //如果数据补齐就下一天
                if (recordcount < 60 * 24)
                {
                    LogHard("历史数据已经追上");
                    var nowday = TimeTool.GetUtcDay(DateTime.Now);
                    while (nowday == parseTime)
                    {
                        await Task.Delay(1000);
                        nowday = TimeTool.GetUtcDay(DateTime.Now);
                    }
                }
                else
                {
                    //下半天数据补齐就可以跳走了
                    parseTime += TimeSpan.FromDays(1);
                    recordcount = 0;
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
                        var nowday = TimeTool.GetUtcDay(DateTime.Now);

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
        static async Task<int> GetHistoryData(string symbol, CacheRecord[] records, DateTime begin, int count = 60 * 12)
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
                var uday = TimeTool.GetUtcDay(begin);
                var beginindex = (UInt16)(ubegin - uday).TotalMinutes;
                IList<Binance.Net.Objects.Models.Futures.BinanceFuturesUsdtKline> datas = result.Data as IList<Binance.Net.Objects.Models.Futures.BinanceFuturesUsdtKline>;

                int yearindex = (int)(ubegin - TimeTool.GetUtcYear(begin)).TotalMinutes;

                for (var i = 0; i <  datas.Count; i++)
                {
                    records[beginindex + i].index = yearindex + i;
                    records[beginindex + i].price_open = (double)datas[i].OpenPrice;
                    records[beginindex + i].price_close = (double)datas[i].ClosePrice;
                    records[beginindex + i].price_high = (double)datas[i].HighPrice;
                    records[beginindex + i].price_low = (double)datas[i].LowPrice;
                    records[beginindex + i].volume = (double)datas[i].Volume;
                    records[beginindex + i].volume_buy = (double)datas[i].TakerBuyBaseVolume;
                }
                return datas.Count;
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
                var uday = TimeTool.GetUtcDay(day.begintime);
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
