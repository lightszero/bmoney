using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Futures;
using Binance.Net.Objects.Models.Futures.Socket;

using CryptoExchange.Net.Sockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using static btrade.TradeTool;

namespace btrade
{
    public class TradeTool
    {
        public string IP
        {
            get;
            private set;
        }
        public BinanceSocketClient socket
        {
            get;
            private set;
        }
        public BinanceRestClient rest
        {
            get;
            private set;
        }
        public async Task Init()
        {

            WebClient wc = new WebClient();
            var str = await wc.DownloadStringTaskAsync("http://ipinfo.io");
            IP = JObject.Parse(str)["ip"].ToString();

            rest = new BinanceRestClient();
            socket = new BinanceSocketClient();
            rest.SetApiCredentials(new CryptoExchange.Net.Authentication.ApiCredentials(key.apikey, key.secert));

            var p2 = await rest.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync(CancellationToken.None);

            allsymbol = new Dictionary<string, BinanceFuturesUsdtSymbol>();
            foreach (var s in p2.Data.Symbols)
            {
                allsymbol[s.Name.ToLower()] = s;
            }

            //实时取余额不行了
            StartUserInfoSocket();


            //SetTrade("ethbusd");
            //StartKLineSocket();


        }
        Dictionary<string, BinanceFuturesUsdtSymbol> allsymbol;
        public string[] GetAllSymbols()
        {
            return allsymbol.Keys.ToArray();
        }
        public BinanceFuturesUsdtSymbol GetSymbol(string symbol)
        {
            return allsymbol[symbol];
        }
        public Trader CreateTrader(string symbol)
        {
            if (allsymbol.TryGetValue(symbol, out var v) == false)
            {
                return null;
            }
            Trader trader = new Trader(this, v);

            return trader;
        }

        //用户账户信息
        public async Task<BinanceFuturesAccountInfo> GetInfo()
        {
            var userinfo = await rest.UsdFuturesApi.Account.GetAccountInfoAsync();
            BinanceFuturesAccountInfo info = userinfo.Data;
            return info;
        }
        public class Wallet
        {
            public bool CanTrade = false;
            public bool CanDeposit = false;
            public bool CanWithdraw = false;
            //余额
            public Dictionary<string, BalanceItem> balance = new Dictionary<string, BalanceItem>();
            //仓位
            public Dictionary<string, PositionItem> positions = new Dictionary<string, PositionItem>();

            public decimal GetBalance(string symbol)
            {
                if (!balance.TryGetValue(symbol, out var value))
                    return 0;
                return value.Available;
            }
            public decimal GetPosition(string symbol)
            {
                if (!positions.TryGetValue(symbol, out var value))
                    return 0;
                return value.count;
            }

        }
        public class BalanceItem
        {
            public string symbol;
            public decimal Available;
            public decimal Wallet;
        }
        public enum Side
        {
            Long,
            Short,
        }

        public class PositionItem
        {
            public string symbol;
            public decimal price;
            public decimal count;
            public decimal priceStopMax;
            public decimal priceStopMin;
            public Side side;
        }
        public Wallet wallet
        {
            get;
            private set;
        }
        public async Task<Wallet> UpdateWallet()
        {
            if (wallet == null)
                wallet = new TradeTool.Wallet();
            if (wallet.balance == null)
                wallet.balance = new Dictionary<string, BalanceItem>();
            var info = await this.GetInfo();
            wallet.balance.Clear();
            foreach (var a in info.Assets)
            {
                if (a.AvailableBalance != 0 && a.WalletBalance != 0)
                {
                    wallet.balance.Add(a.Asset.ToLower(), new BalanceItem() { symbol = a.Asset.ToLower(), Available = a.AvailableBalance, Wallet = a.WalletBalance });
                }
            }
            wallet.positions.Clear();
            foreach (BinancePositionInfoUsdt p in info.Positions)
            {
                var key = p.Symbol.ToLower();
                PositionItem item = new PositionItem();
                item.symbol = key;
                item.price = p.EntryPrice;
                item.count = p.Quantity;
                item.side = p.PositionSide == PositionSide.Long ? Side.Long : Side.Short;
                wallet.positions[key] = item;
            }
            return wallet;
        }
        //public static String Symbol
        //{
        //    get;
        //    private set;
        //}
        int pricePrecision;
        //public  void SetTrade(string symbol)
        //{
        //    Symbol = symbol;
        //    pricePrecision = allsymbol[symbol].PricePrecision;
        //}

        //socket接口 ，实时行情
        public event Action OnWalletUpdate;

        public event Action<string> OnOrderUpdate;
        public void TriggerOrderUpdate(string symbol)
        {
            if (OnOrderUpdate != null)
                OnOrderUpdate(symbol);
        }
        void CBOnAccountUpdate(DataEvent<BinanceFuturesStreamAccountUpdate> e)
        {
            if (wallet == null)
                wallet = new TradeTool.Wallet();
            if (wallet.balance == null)
                wallet.balance = new Dictionary<string, BalanceItem>();
            foreach (var b in e.Data.UpdateData.Balances)
            {
                var key = b.Asset.ToLower();
                Debug.WriteLine("收到账户变化:" + key);
                if (wallet.balance.TryGetValue(key, out var bal))
                {
                    bal.Wallet = b.CrossWalletBalance;
                    bal.Available = b.WalletBalance;
                }
                else
                {
                    var item = new BalanceItem()
                    {
                        symbol = key,
                        Available = b.WalletBalance - b.CrossWalletBalance,
                        Wallet = b.CrossWalletBalance
                    };
                    wallet.balance.Add(key, item);
                }
            }
            if (OnWalletUpdate != null)
                OnWalletUpdate();

            foreach (var p in e.Data.UpdateData.Positions)
            {
                var key = p.Symbol.ToLower();
                if (wallet.positions.TryGetValue(key, out var pitem))
                {

                }
                else
                {
                    pitem = new PositionItem();
                    pitem.symbol = key;
                    wallet.positions[key] = pitem;
                }
                pitem.count = p.Quantity;
                pitem.price = p.EntryPrice;
                pitem.side = p.PositionSide == PositionSide.Long ? Side.Long : Side.Short;
            }
        }
        void CBOnOrderUpdate(DataEvent<BinanceFuturesStreamOrderUpdate> e)
        {
            var key = e.Data.UpdateData.Symbol.ToLower();
            //好像也不咋关心这玩意儿
            //e.Data.UpdateData.Type
            Debug.WriteLine("收到订单变化:" + key);
            if (wallet.positions.TryGetValue(key, out PositionItem pitem))
            {
            }
            else
            {
                pitem = new PositionItem();
                pitem.symbol = key;
                wallet.positions[key] = pitem;
            }
            if (e.Data.UpdateData.Type == FuturesOrderType.TakeProfitMarket)
            {
                pitem.priceStopMax = e.Data.UpdateData.StopPrice;
            }
            if (e.Data.UpdateData.Type == FuturesOrderType.StopMarket)
            {
                pitem.priceStopMin = e.Data.UpdateData.StopPrice;
            }
            TriggerOrderUpdate(key);

        }




        async void StartUserInfoSocket()
        {

            System.Threading.CancellationToken token = System.Threading.CancellationToken.None;
            var listenkey = await rest.UsdFuturesApi.Account.StartUserStreamAsync();
            Console.WriteLine("got key=" + listenkey.Data);
            var result = await socket.UsdFuturesApi.SubscribeToUserDataUpdatesAsync(listenkey.Data, (e) =>
            {
                Console.WriteLine("==SubscribeToUserDataUpdatesAsync==BinanceFuturesStreamConfigUpdate" + e.GetType().Name);
            },
            (e) =>
            {
                Console.WriteLine("==SubscribeToUserDataUpdatesAsync==BinanceFuturesStreamMarginUpdate" + e.GetType().Name);
            },
            CBOnAccountUpdate, CBOnOrderUpdate,
             (e) =>
             {
                 Console.WriteLine("==SubscribeToUserDataUpdatesAsync==BinanceStreamEvent" + e.GetType().Name);
             }
            ,
             (e) =>
            {
                Console.WriteLine("==SubscribeToUserDataUpdatesAsync==BinanceStrategyUpdate" + e.GetType().Name);
            }
            ,
             (e) =>
            {
                Console.WriteLine("==SubscribeToUserDataUpdatesAsync==BinanceGridUpdate" + e.GetType().Name);
            }
            , (e) =>
            {
                Console.WriteLine("==SubscribeToUserDataUpdatesAsync==BinanceConditionOrderTriggerRejectUpdate" + e.GetType().Name);
            },
             token);
            if (result.Error != null)
            {
                Console.Error.WriteLine(result.Error.Message);
                throw new Exception("socket 连接错误");
            }
            else
            {
                Console.WriteLine("订阅用户信息" + result.OriginalData);
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