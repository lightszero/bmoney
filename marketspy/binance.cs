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
            await c.UsdFuturesStreams.SubscribeToKlineUpdatesAsync("ethusdt", Binance.Net.Enums.KlineInterval.OneMinute, (ev) =>
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
        }
    }
}
