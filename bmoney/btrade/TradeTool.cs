using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Futures;
using Binance.Net.Objects.Models.Futures.Socket;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Sockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace btrade
{
    public static class TradeTool
    {
        public static string IP
        {
            get;
            private set;
        }
        static BinanceSocketClient socket;
        static BinanceClient rest;
        public static async Task Init()
        {

            WebClient wc = new WebClient();
            var str = await wc.DownloadStringTaskAsync("http://ipinfo.io");
            IP = JObject.Parse(str)["ip"].ToString();

            rest = new BinanceClient();
            socket = new BinanceSocketClient();

            rest.SetApiCredentials(new Binance.Net.Objects.BinanceApiCredentials(key.apikey, key.secert));

            var p2 = await rest.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync(CancellationToken.None);

            allsymbol = new Dictionary<string, BinanceFuturesUsdtSymbol>();
            foreach (var s in p2.Data.Symbols)
            {
                allsymbol[s.Name.ToLower()] = s;
            }
            StartUserInfoSocket();
            SetTrade("ethbusd");
            StartKLineSocket();


        }
        static Dictionary<string, BinanceFuturesUsdtSymbol> allsymbol;
        public static string[] GetAllSymbols()
        {
            return allsymbol.Keys.ToArray();
        }
        public static BinanceFuturesUsdtSymbol GetSymbol(string symbol)
        {
            return allsymbol[symbol];
        }
        //得到费率
        public static async Task<double> GetFee()
        {
            var p = await rest.UsdFuturesApi.ExchangeData.GetMarkPriceAsync(Symbol, CancellationToken.None);


            return (double)p.Data.FundingRate.Value;
        }
        //用户账户信息
        public static async Task<BinanceFuturesAccountInfo> GetInfo()
        {
            var userinfo = await rest.UsdFuturesApi.Account.GetAccountInfoAsync();
            BinanceFuturesAccountInfo info = userinfo.Data;
            return info;
        }
        public static String Symbol
        {
            get;
            private set;
        }
        static int pricePrecision;
        public static void SetTrade(string symbol)
        {
            Symbol = symbol;
            pricePrecision= allsymbol[symbol].PricePrecision;
        }
        public static event Action<IBinanceStreamKlineData> OnKLine;
        //socket接口 ，实时行情
        static void OnAccountUpdate(DataEvent<BinanceFuturesStreamAccountUpdate> e)
        {
            Console.WriteLine("收到账户变化");
        }
        static void OnOrderUpdate(DataEvent<BinanceFuturesStreamOrderUpdate> e)
        {
            Console.WriteLine("收到订单变化");
        }

        public static async void Go(bool longorshort, double count, decimal priceWin, decimal priceLose)
        {

            if (longorshort)
                MarkBuy((decimal)count, (decimal)priceWin, (decimal)priceLose);
            else
                MarkSell((decimal)count, (decimal)priceWin, (decimal)priceLose);
        }


        //开多
        static async void MarkBuy(decimal count, decimal priceWin, decimal priceLose)
        {
            {//主要订单
                Binance.Net.Enums.OrderSide orderSide = Binance.Net.Enums.OrderSide.Buy;
                Binance.Net.Enums.PositionSide side = Binance.Net.Enums.PositionSide.Both;//单向持仓模式选both
                FuturesOrderType ordertype = FuturesOrderType.Market; //先开一个市价单

                decimal? price = null;

                decimal? activprice = null;//触发价格
                decimal? stopprice = null;
                decimal? callbackRate = null;//回撤率
                bool? close = false;
                var result = await rest.UsdFuturesApi.Trading.PlaceOrderAsync(Symbol,
                    orderSide,//买？
                    ordertype //市价单
                    , count, price, side //价 量 方向
                    , null//有效方法TimeInForce.FillOrKill
                    , null//只减仓
                    , null//订单id
                    , stopprice//停止价格
                    , activprice, callbackRate, WorkingType.Contract, //触发价，回撤率，触发方法
                    close, //触发后平仓
                    OrderResponseType.Acknowledge,
                    false,//priceProtect
                    null, CancellationToken.None);
                if (!result.Success)
                {
                    Console.WriteLine("下单失败:" + result.Error.ToString());
                    return;
                }
            }
            {//止盈
                Binance.Net.Enums.OrderSide orderSide = Binance.Net.Enums.OrderSide.Sell;
                Binance.Net.Enums.PositionSide side = Binance.Net.Enums.PositionSide.Both;//单向持仓模式选both
                FuturesOrderType ordertype = FuturesOrderType.TakeProfitMarket; //先开一个市价单

                decimal? price = null;

                decimal? activprice = null;//触发价格
                decimal? stopprice = priceWin;
                decimal? callbackRate = null;//回撤率
                bool? close = true;//平仓
                var result = await rest.UsdFuturesApi.Trading.PlaceOrderAsync(Symbol,
                    orderSide,//买？
                    ordertype //市价单
                    , count, price, side //价 量 方向
                    , null//有效方法TimeInForce.FillOrKill
                    , null//只减仓
                    , null//订单id
                    , stopprice//停止价格
                    , activprice, callbackRate, WorkingType.Contract, //触发价，回撤率，触发方法
                    close, //触发后平仓
                    OrderResponseType.Acknowledge,
                    false,//priceProtect
                    null, CancellationToken.None);
                if (!result.Success)
                {
                    Console.WriteLine("下单失败:" + result.Error.ToString());
                    return;
                }
            }
            {//止损
                Binance.Net.Enums.OrderSide orderSide = Binance.Net.Enums.OrderSide.Sell;
                Binance.Net.Enums.PositionSide side = Binance.Net.Enums.PositionSide.Both;//单向持仓模式选both
                FuturesOrderType ordertype = FuturesOrderType.StopMarket; //先开一个市价单

                decimal? price = null;

                decimal? activprice = null;//触发价格
                decimal? stopprice = priceLose;
                decimal? callbackRate = null;//回撤率
                bool? close = true;//平仓
                var result = await rest.UsdFuturesApi.Trading.PlaceOrderAsync(Symbol,
                    orderSide,//买？
                    ordertype //市价单
                    , count, price, side //价 量 方向
                    , null//有效方法TimeInForce.FillOrKill
                    , null//只减仓
                    , null//订单id
                    , stopprice//停止价格
                    , activprice, callbackRate, WorkingType.Contract, //触发价，回撤率，触发方法
                    close, //触发后平仓
                    OrderResponseType.Acknowledge,
                    false,//priceProtect
                    null, CancellationToken.None);
                if (!result.Success)
                {
                    Console.WriteLine("下单失败:" + result.Error.ToString());
                    return;
                }
            }
        }
        static async void MarkSell(decimal count, decimal priceWin, decimal priceLose)
        {
            {//主要订单
                Binance.Net.Enums.OrderSide orderSide = Binance.Net.Enums.OrderSide.Sell;
                Binance.Net.Enums.PositionSide side = Binance.Net.Enums.PositionSide.Both;//单向持仓模式选both
                FuturesOrderType ordertype = FuturesOrderType.Market; //先开一个市价单

                decimal? price = null;

                decimal? activprice = null;//触发价格
                decimal? stopprice = null;
                decimal? callbackRate = null;//回撤率
                bool? close = false;
                var result = await rest.UsdFuturesApi.Trading.PlaceOrderAsync(Symbol,
                    orderSide,//买？
                    ordertype //市价单
                    , count, price, side //价 量 方向
                    , null//有效方法TimeInForce.FillOrKill
                    , null//只减仓
                    , null//订单id
                    , stopprice//停止价格
                    , activprice, callbackRate, WorkingType.Contract, //触发价，回撤率，触发方法
                    close, //触发后平仓
                    OrderResponseType.Acknowledge,
                    false,//priceProtect
                    null, CancellationToken.None);
                if (!result.Success)
                {
                    Console.WriteLine("下单失败:" + result.Error.ToString());
                    return;
                }
            }
            {//止盈
                Binance.Net.Enums.OrderSide orderSide = Binance.Net.Enums.OrderSide.Buy;
                Binance.Net.Enums.PositionSide side = Binance.Net.Enums.PositionSide.Both;//单向持仓模式选both
                FuturesOrderType ordertype = FuturesOrderType.TakeProfitMarket; //止盈利单

                decimal? price = null;

                decimal? activprice = null;//触发价格
                decimal? stopprice = priceWin;
                decimal? callbackRate = null;//回撤率
                bool? close = true;//平仓
                var result = await rest.UsdFuturesApi.Trading.PlaceOrderAsync(Symbol,
                    orderSide,//买？
                    ordertype //市价单
                    , count, price, side //价 量 方向
                    , null//有效方法TimeInForce.FillOrKill
                    , null//只减仓
                    , null//订单id
                    , stopprice//停止价格
                    , activprice, callbackRate, WorkingType.Contract, //触发价，回撤率，触发方法
                    close, //触发后平仓
                    OrderResponseType.Acknowledge,
                    false,//priceProtect
                    null, CancellationToken.None);
                if (!result.Success)
                {
                    Console.WriteLine("下单失败:" + result.Error.ToString());
                    return;
                }
            }
            {//止损
                Binance.Net.Enums.OrderSide orderSide = Binance.Net.Enums.OrderSide.Buy;
                Binance.Net.Enums.PositionSide side = Binance.Net.Enums.PositionSide.Both;//单向持仓模式选both
                FuturesOrderType ordertype = FuturesOrderType.StopMarket; //止损单

                decimal? price = null;

                decimal? activprice = null;//触发价格
                decimal? stopprice = priceLose;
                decimal? callbackRate = null;//回撤率
                bool? close = true;//平仓
                var result = await rest.UsdFuturesApi.Trading.PlaceOrderAsync(Symbol,
                    orderSide,//买？
                    ordertype //市价单
                    , count, price, side //价 量 方向
                    , null//有效方法TimeInForce.FillOrKill
                    , null//只减仓
                    , null//订单id
                    , stopprice//停止价格
                    , activprice, callbackRate, WorkingType.Contract, //触发价，回撤率，触发方法
                    close, //触发后平仓
                    OrderResponseType.Acknowledge,
                    false,//priceProtect
                    null, CancellationToken.None);
                if (!result.Success)
                {
                    Console.WriteLine("下单失败:" + result.Error.ToString());
                    return;
                }
            }
        }
        static string nowsymbol;
        static async void StartKLineSocket()
        {
            while (true)
            {
                System.Threading.CancellationToken token = System.Threading.CancellationToken.None;
                var result = await socket.UsdFuturesStreams.SubscribeToKlineUpdatesAsync(Symbol, Binance.Net.Enums.KlineInterval.OneMinute, (kdata) =>
                {
                    if (OnKLine != null)
                    {
                        OnKLine(kdata.Data);
                    }
                }, token);
                if (result.Error != null)
                {
                    Console.Error.WriteLine(result.Error.Message);
                    throw new Exception("socket 连接错误");
                }

                nowsymbol = Symbol;

                bool closed = false;
                result.Data.ConnectionClosed += () =>
                {
                    closed = true;
                };
                while (!closed)
                {
                    if (nowsymbol != Symbol)
                    //if (!IsActive)
                    {
                        await result.Data.CloseAsync();
                        //bLiveStop = true;
                    }
                    await Task.Delay(100);
                }
                await Task.Delay(1);
            }
        }
        static async void StartUserInfoSocket()
        {

            System.Threading.CancellationToken token = System.Threading.CancellationToken.None;
            var result = await socket.UsdFuturesStreams.SubscribeToUserDataUpdatesAsync(key.apikey, null, null, OnAccountUpdate, OnOrderUpdate, null, null, null, token);
            if (result.Error != null)
            {
                Console.Error.WriteLine(result.Error.Message);
                throw new Exception("socket 连接错误");
            }
            bool closed = false;
            result.Data.ConnectionClosed += () =>
            {
                closed = true;
            };
            while (!closed)
            {
                //if (!IsActive)
                {
                    //await result.Data.CloseAsync();
                    //bLiveStop = true;
                }
                await Task.Delay(100);
            }

            Console.WriteLine("订阅断线，重联。");
        }
    }
}