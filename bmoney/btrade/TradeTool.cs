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
        static BinanceSocketClient socketKLine;
        static BinanceSocketClient socketInfo;
        static BinanceClient rest;
        public static async Task Init()
        {

            WebClient wc = new WebClient();
            var str = await wc.DownloadStringTaskAsync("http://ipinfo.io");
            IP = JObject.Parse(str)["ip"].ToString();

            rest = new BinanceClient();
            socketKLine = new BinanceSocketClient();
            socketInfo = new BinanceSocketClient();
            rest.SetApiCredentials(new Binance.Net.Objects.BinanceApiCredentials(key.apikey, key.secert));

            var p2 = await rest.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync(CancellationToken.None);

            allsymbol = new Dictionary<string, BinanceFuturesUsdtSymbol>();
            foreach (var s in p2.Data.Symbols)
            {
                allsymbol[s.Name.ToLower()] = s;
            }
            
            //实时取余额不行了
            //StartUserInfoSocket();


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
            pricePrecision = allsymbol[symbol].PricePrecision;
        }
        public static event Action<IBinanceStreamKlineData> OnKLine;
        //socket接口 ，实时行情

        //很遗憾，拿余额的接口不工作，没关系，我们还有rest版本
        //static void OnAccountUpdate(DataEvent<BinanceFuturesStreamAccountUpdate> e)
        //{
        //    Console.WriteLine("收到账户变化");
        //}
        //static void OnOrderUpdate(DataEvent<BinanceFuturesStreamOrderUpdate> e)
        //{
        //    Console.WriteLine("收到订单变化");
        //}

        public static async void Go(bool longorshort, decimal count, decimal priceWin, decimal priceLose)
        {
            //下单助手会先清理之前的，万一有个之前的止损单给我平仓了
            var cresult = await rest.UsdFuturesApi.Trading.CancelAllOrdersAsync(Symbol);

            if (longorshort)
                MarkBuy((decimal)count, (decimal)priceWin, (decimal)priceLose);
            else
                MarkSell((decimal)count, (decimal)priceWin, (decimal)priceLose);
        }


        //开多
        static async void MarkBuy(decimal count, decimal priceWin, decimal priceLose)
        {
            

            BinanceFuturesBatchOrder[] orders = new BinanceFuturesBatchOrder[3];
            {//主要订单
                BinanceFuturesBatchOrder order = new BinanceFuturesBatchOrder();
                order.Side = Binance.Net.Enums.OrderSide.Buy;
                order.PositionSide = Binance.Net.Enums.PositionSide.Both;//单向持仓模式选both
                order.Type = FuturesOrderType.Market;//先开一个市价单
                order.Symbol = Symbol;
                order.Quantity = count;//量
                order.WorkingType = WorkingType.Contract;
                orders[0] = order;
            }
            {//止盈
                BinanceFuturesBatchOrder order = new BinanceFuturesBatchOrder();
                order.Side = Binance.Net.Enums.OrderSide.Sell;
                order.PositionSide = Binance.Net.Enums.PositionSide.Both;//单向持仓模式选both
                order.Type = FuturesOrderType.TakeProfitMarket;//止盈
                order.Symbol = Symbol;
                order.Quantity = count;
                order.StopPrice = priceWin;
                order.WorkingType = WorkingType.Contract;
                //没有平仓选项了，只能按照数量，回头又得跑一下实时
                orders[1] = order;
            }
            {//止损
                BinanceFuturesBatchOrder order = new BinanceFuturesBatchOrder();
                order.Side = Binance.Net.Enums.OrderSide.Sell;
                order.PositionSide = Binance.Net.Enums.PositionSide.Both;//单向持仓模式选both
                order.Type = FuturesOrderType.StopMarket;//止损
                order.Symbol = Symbol;
                order.Quantity = count;
                order.StopPrice = priceLose;
                order.WorkingType = WorkingType.Contract;
                //没有平仓选项了，只能按照数量，回头又得跑一下实时
                orders[2] = order;
            }
            var result = await rest.UsdFuturesApi.Trading.PlaceMultipleOrdersAsync(orders);
            if(result.Success==false)
            {
                Console.WriteLine("下单失败" + result.Error.ToString());
            }
            else
            {
                Console.WriteLine("下单成功");
            }
        }
        static async void MarkSell(decimal count, decimal priceWin, decimal priceLose)
        {
            BinanceFuturesBatchOrder[] orders = new BinanceFuturesBatchOrder[3];
            {//主要订单
                BinanceFuturesBatchOrder order = new BinanceFuturesBatchOrder();
                order.Side = Binance.Net.Enums.OrderSide.Sell;
                order.PositionSide = Binance.Net.Enums.PositionSide.Both;//单向持仓模式选both
                order.Type = FuturesOrderType.Market;//先开一个市价单
                order.Symbol = Symbol;
                order.Quantity = count;//量
                order.WorkingType = WorkingType.Contract;
                orders[0] = order;
            }
            {//止盈
                BinanceFuturesBatchOrder order = new BinanceFuturesBatchOrder();
                order.Side = Binance.Net.Enums.OrderSide.Buy;
                order.PositionSide = Binance.Net.Enums.PositionSide.Both;//单向持仓模式选both
                order.Type = FuturesOrderType.TakeProfitMarket;//止盈
                order.Symbol = Symbol;
                order.Quantity = count;
                order.StopPrice = priceWin;
                order.WorkingType = WorkingType.Contract;
                //没有平仓选项了，只能按照数量，回头又得跑一下实时
                orders[1] = order;
            }
            {//止损
                BinanceFuturesBatchOrder order = new BinanceFuturesBatchOrder();
                order.Side = Binance.Net.Enums.OrderSide.Buy;
                order.PositionSide = Binance.Net.Enums.PositionSide.Both;//单向持仓模式选both
                order.Type = FuturesOrderType.StopMarket;//止损
                order.Symbol = Symbol;
                order.Quantity = count;
                order.StopPrice = priceLose;
                order.WorkingType = WorkingType.Contract;
                //没有平仓选项了，只能按照数量，回头又得跑一下实时
                orders[2] = order;
            }
            var result = await rest.UsdFuturesApi.Trading.PlaceMultipleOrdersAsync(orders);
            if (result.Success == false)
            {
                Console.WriteLine("下单失败" + result.Error.ToString());
            }
            else
            {
                Console.WriteLine("下单成功");
            }
        }
        static string nowsymbol;
        static async void StartKLineSocket()
        {
            while (true)
            {
                System.Threading.CancellationToken token = System.Threading.CancellationToken.None;
                var result = await socketKLine.UsdFuturesStreams.SubscribeToKlineUpdatesAsync(Symbol, Binance.Net.Enums.KlineInterval.OneMinute, (kdata) =>
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
        //static async void StartUserInfoSocket()
        //{

        //    System.Threading.CancellationToken token = System.Threading.CancellationToken.None;
        //    var result = await socketInfo.UsdFuturesStreams.SubscribeToUserDataUpdatesAsync(key.apikey, null, null, OnAccountUpdate, OnOrderUpdate, null, null, null, token);
        //    if (result.Error != null)
        //    {
        //        Console.Error.WriteLine(result.Error.Message);
        //        throw new Exception("socket 连接错误");
        //    }
        //    bool closed = false;
        //    result.Data.ConnectionClosed += () =>
        //    {
        //        closed = true;
        //    };
        //    while (!closed)
        //    {
        //        //if (!IsActive)
        //        {
        //            //await result.Data.CloseAsync();
        //            //bLiveStop = true;
        //        }
        //        await Task.Delay(100);
        //    }

        //    Console.WriteLine("订阅断线，重联。");
        //}
    }
}