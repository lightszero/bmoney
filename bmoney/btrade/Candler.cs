using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Futures;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace btrade
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
    //获取行情的接口
    public class Candler
    {
        public Candler(TradeTool traderTool, BinanceFuturesUsdtSymbol symbol)
        {
            this._symbol = symbol;
            this._traderTool = traderTool;
        }
        protected BinanceFuturesUsdtSymbol _symbol;
        protected TradeTool _traderTool;
        public string Symbol
        {
            get
            {
                return _symbol.Name.ToLower();
            }
        }

        public event Action<Candle> OnKLine;

        UpdateSubscription socketSubscribe;
        bool socketalive = false;
        public async Task<Candle[]> GetKLines(KlineInterval tick, DateTime startTime)
        {
            var result = await _traderTool.rest.UsdFuturesApi.ExchangeData.GetKlinesAsync(this._symbol.Pair, tick, startTime, null);
            IList<Binance.Net.Objects.Models.Futures.BinanceFuturesUsdtKline> datas = result.Data as IList<Binance.Net.Objects.Models.Futures.BinanceFuturesUsdtKline>;
            Candle[] records = new Candle[datas.Count];
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
            return records;
        }
        public async Task StartKLineSocket()
        {

            System.Threading.CancellationToken token = System.Threading.CancellationToken.None;
            var result = await _traderTool.socket.UsdFuturesApi.SubscribeToKlineUpdatesAsync(Symbol, Binance.Net.Enums.KlineInterval.OneMinute, (kdata) =>
            {
                if (OnKLine != null)
                {
                    Candle data = new Candle();
                    var srcdata = kdata.Data.Data;
                    data.time = srcdata.OpenTime.ToLocalTime();
                    data.open = (double)srcdata.OpenPrice;
                    data.close = (double)srcdata.ClosePrice;
                    data.high = (double)srcdata.HighPrice;
                    data.low = (double)srcdata.LowPrice;
                    data.close = (double)srcdata.ClosePrice;
                    data.volume = (double)srcdata.Volume;
                    OnKLine(data);
                }
            }, token);
            if (result.Error != null)
            {
                Console.Error.WriteLine(result.Error.Message);
                throw new Exception("socket 连接错误");
            }
            socketSubscribe = result.Data;

            //nowsymbol = Symbol;
            socketalive = true;
            socketSubscribe.ConnectionClosed += () =>
            {
                socketalive = false;
                socketSubscribe = null;
            };
        }
        public async Task StopKLineSocket()
        {
            if (socketSubscribe != null)
            {
                await socketSubscribe.CloseAsync();
                socketSubscribe = null;
            }

            while (socketalive)
            {
                await Task.Delay(100);
            }
        }


    }
}
