using Binance.Net.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney.data
{
    //一个币安数据导入器
    class BinanceImporter : IImporter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="withlive">true 实时接收数据，false 只接收历史</param>
        /// <param name="startTime"></param>
        /// <param name="tick"></param>
        public BinanceImporter(bool withlive, DateTime startTime, TimeSpan tick)
        {
            var v = CandleUtil.ToMinute(startTime);
            int vi = (int)v;
            if (v != vi)
            {
                throw new Exception("error time不是整数分钟.");
            }
            var ts = tick.TotalMinutes;
            if (ts == 1.0)
                btick = Binance.Net.Enums.KlineInterval.OneMinute;
            else if (ts == 5.0)
                btick = Binance.Net.Enums.KlineInterval.FiveMinutes;
            else
            {
                throw new Exception("not support tick不是整数分钟.");
            }

            if (vi % ((int)ts) != 0)
            {
                throw new Exception("time不能被 tick整除.");
            }
            this.withLive = withlive;
            this.Tick = tick;
            this.symbol = "ethusdt";
            this.beginTime = startTime;

        }

        public TimeSpan Tick
        {
            get;
            private set;
        }
        public bool IsActive
        {
            get;
            private set;
        }
        public bool IsAsync => true;
        bool bStop = true;
        bool bLiveStop = true;
        public async void Start(CandlePool pool)
        {
            IsActive = true;
            this.pool = pool;
            bStop = false;
            var tickcount = (int)((DateTime.Now - beginTime) / Tick);
            if(this.withLive)
            {
                //启动实时行情
                SocketRealtimeDataWatcher();
            }
            var time = beginTime;
            while (true)
            {
                if(IsActive==false)
                {
                    bStop = true;
                    break;
                }
                var need = historyCache.Length;
                var intgot = await GetHistoryData(symbol, historyCache, time,btick, need);
                Console.WriteLine("BinanceImporter.GetHistoryData:" + time + " count=" + intgot);
                time += Tick * intgot;
                for(var i=0;i<intgot;i++)
                {
                    pool.Push(historyCache[i], true);
                }
                if (intgot < need)//没有了
                {
                    if (!withLive)
                    {
                        IsActive = false;
                        break;
                    }
                    else//在活动模式还要满足追上活动cache才行
                    {
                        if (liveCache.Count > 0)
                        {
                            var cachetime = liveCache.First.Value.time;
                            var gottime = historyCache[intgot - 1].time;
                            if (gottime > cachetime)
                                break;
} 
                    }
                }
            }
            //追上以后就开始塞cache
        }
        public void Stop()
        {
            if (IsActive)
            {
                IsActive = false;
            }
            //一直等到完全结束
            while (bStop != true || bLiveStop != true)
            {
                System.Threading.Thread.Sleep(100);
            }
        }
        async void SocketRealtimeDataWatcher()
        {
            dataNotFinal = null;
            while (IsActive)
            {
                var result = await socket.UsdFuturesStreams.SubscribeToKlineUpdatesAsync(symbol, Binance.Net.Enums.KlineInterval.OneMinute, (ev) =>
                {
                    var dat = ev.Data.Data;
                    var time = dat.OpenTime.ToLocalTime();
                    var candle = new Candle();
                    candle.time = time;
                    candle.open = (double)dat.OpenPrice;
                    candle.high = (double)dat.HighPrice;
                    candle.low = (double)dat.LowPrice;
                    candle.close = (double)dat.ClosePrice;
                    candle.volume = (double)dat.Volume;
                    var final = dat.Final;
                    if (final)
                    {
                        liveCache.AddLast(candle);
                        while(liveCache.Count>1000)
                        {
                            liveCache.RemoveFirst();
                        }
                    }
                    else
                    {
                        dataNotFinal = candle;
                    }
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
                    if(!IsActive)
                    {
                        await result.Data.CloseAsync();
                        bLiveStop = true;
                    }
                    await Task.Delay(100);
                }

                Console.WriteLine("订阅断线，重联。");
            }
            bLiveStop = true;
        }

        CandlePool pool;
        bool withLive = false;
        string symbol;
        DateTime beginTime;
        Binance.Net.Enums.KlineInterval btick;
        Candle[] historyCache = new Candle[720];
        LinkedList<Candle> liveCache = new LinkedList<Candle>();
        Candle? dataNotFinal;
        static BinanceClient rest = new BinanceClient();
        static BinanceSocketClient socket = new BinanceSocketClient();


        static async Task<int> GetHistoryData(string symbol, Candle[] records, DateTime begin, Binance.Net.Enums.KlineInterval tick,int count = 60 * 12)
        {
            begin = begin.ToUniversalTime();
            while (true)
            {
                var result = await rest.UsdFuturesApi.ExchangeData.GetKlinesAsync(symbol, tick, begin, null, count);
                if (result.Error != null)
                {

                    Console.Error.WriteLine("BinanceImporter.GetHistoryData:" + result.Error.Message);
                    continue;
                }
                IList<Binance.Net.Objects.Models.Futures.BinanceFuturesUsdtKline> datas = result.Data as IList<Binance.Net.Objects.Models.Futures.BinanceFuturesUsdtKline>;
                for (var i = 0; i < datas.Count; i++)
                {
                    records[i].time = datas[i].OpenTime.ToLocalTime();
                    records[i].open = (double)datas[i].OpenPrice;
                    records[i].close = (double)datas[i].ClosePrice;
                    records[i].high = (double)datas[i].HighPrice;
                    records[i].low = (double)datas[i].LowPrice;
                    records[i].close = (double)datas[i].ClosePrice;
                    records[i].volume = (double)datas[i].Volume;
                }
                return datas.Count;
            }

        }

    }
}
