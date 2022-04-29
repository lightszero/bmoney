using Binance.Net.Clients;
using System;
using System.Collections.Generic;
using System.Text;

namespace marketspy
{
    internal class binance
    {
        public static async void te()
        {
            Binance.Net.Clients.BinanceSocketClient c = new BinanceSocketClient();
           var ra= await c.UsdFuturesStreams.SubscribeToKlineUpdatesAsync("ethusdt", Binance.Net.Enums.KlineInterval.OneMinute, (ev) =>
             {
                 var dat = ev.Data.Data;
                 var s = ev.Data.Symbol;
                 var time = dat.OpenTime.ToLocalTime();
                 var hp = dat.HighPrice;
                 var lp = dat.LowPrice;
                 var op = ev.Data.Data.OpenPrice;
                 var cp = ev.Data.Data.ClosePrice;
                 var v = dat.Volume;
                 var vb = dat.TakerBuyBaseVolume;
                 var f = dat.Final;
                 Console.WriteLine("recv=" + s + " t=" + time + " p=" + cp + " f=" + f);
             });
            //ra.Data.ConnectionClosed;通过这个事件可以发现断线
            
                Console.Write("start SubscribeToKline");
            var rest =new BinanceClient();
            //rest.UsdFuturesApi.ExchangeData.PingAsync();//可以通过ping去发现 是否接通

            DateTime time = DateTime.Now.ToUniversalTime();
            var starttime = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0, DateTimeKind.Utc);
            var lines = await rest.UsdFuturesApi.ExchangeData.GetKlinesAsync("ethusdt", Binance.Net.Enums.KlineInterval.OneMinute, starttime, null, 60 * 12);
        }
    }
}
