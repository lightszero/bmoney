using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Futures;
using CryptoExchange.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static btrade.TradeTool;

namespace btrade
{
    public class Trader: Candler
    {

        public Trader(TradeTool traderTool, BinanceFuturesUsdtSymbol symbol)
            :base(traderTool,symbol)
        {
            //this._symbol = symbol;
            //this._traderTool = traderTool;
        }
        //BinanceFuturesUsdtSymbol _symbol;
        //TradeTool _traderTool;

        //public event Action<IBinanceStreamKlineData> OnKLine;




        //得到费率
        public async Task<double> GetFee()
        {
            var p = await this._traderTool.rest.UsdFuturesApi.ExchangeData.GetMarkPriceAsync(Symbol, CancellationToken.None);

            return (double)p.Data.FundingRate.Value;
        }

        //安全下单接口
        public async Task MakeOrder_Safe(bool longorshort, decimal count, decimal priceWin, decimal priceLose)
        {
            //下单助手会先清理之前的，万一有个之前的止损单给我平仓了
            var cresult = await _traderTool.rest.UsdFuturesApi.Trading.CancelAllOrdersAsync(Symbol);

            if (longorshort)
                await MakeOrder_Buy_Limit((decimal)count, (decimal)priceWin, (decimal)priceLose);
            else
                await MakeOrder_Sell_Limit((decimal)count, (decimal)priceWin, (decimal)priceLose);
        }
        public async Task MakeOrder_Close_Safe(bool longorshort, decimal count)
        {
            var cresult = await _traderTool.rest.UsdFuturesApi.Trading.CancelAllOrdersAsync(Symbol);
            if (longorshort)
                await MakeOrder_Buy((decimal)count);
            else
                await MakeOrder_Sell((decimal)count);
        }
        //一个买单，可以用来平仓空单。
        async Task MakeOrder_Buy(decimal count)
        {


            BinanceFuturesBatchOrder[] orders = new BinanceFuturesBatchOrder[1];
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
            var result = await _traderTool.rest.UsdFuturesApi.Trading.PlaceMultipleOrdersAsync(orders);
            if (result.Success == false)
            {
                Console.WriteLine("下单失败" + result.Error.ToString());
            }
            else
            {
                Console.WriteLine("下单成功");
            }
        }
        //一个卖单，可以用来平仓多单
        async Task MakeOrder_Sell(decimal count)
        {
            BinanceFuturesBatchOrder[] orders = new BinanceFuturesBatchOrder[1];
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
            var result = await _traderTool.rest.UsdFuturesApi.Trading.PlaceMultipleOrdersAsync(orders);
            if (result.Success == false)
            {
                Console.WriteLine("下单失败" + result.Error.ToString());
            }
            else
            {
                Console.WriteLine("下单成功");
            }
        }

        //开多
        async Task MakeOrder_Buy_Limit(decimal count, decimal priceWin, decimal priceLose)
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
            var result = await _traderTool.rest.UsdFuturesApi.Trading.PlaceMultipleOrdersAsync(orders);
            if (result.Success == false)
            {
                Console.WriteLine("下单失败" + result.Error.ToString());
            }
            else
            {
                Console.WriteLine("下单成功");
            }
        }
        async Task MakeOrder_Sell_Limit(decimal count, decimal priceWin, decimal priceLose)
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
            var result = await _traderTool.rest.UsdFuturesApi.Trading.PlaceMultipleOrdersAsync(orders);
            if (result.Success == false)
            {
                Console.WriteLine("下单失败" + result.Error.ToString());
            }
            else
            {
                Console.WriteLine("下单成功");
            }
        }

        public async Task UpdateOrder()
        {
            var orders = await _traderTool.rest.UsdFuturesApi.Trading.GetOpenOrdersAsync(Symbol);
            
            foreach (var o in orders.Data)
            {
                
                var key = o.Symbol.ToLower();
                if (_traderTool.wallet.positions.TryGetValue(key, out var pitem))
                {
                }
                else
                {
                    pitem = new PositionItem();
                    pitem.symbol = key;
                    _traderTool.wallet.positions[key] = pitem;
                }

                if (o.Type == FuturesOrderType.TakeProfitMarket)
                {
                    pitem.priceStopMax = o.StopPrice.Value;
                }
                else if (o.Type == FuturesOrderType.StopMarket)
                {
                    pitem.priceStopMin = o.StopPrice.Value;
                }

            }
            _traderTool.TriggerOrderUpdate(Symbol);
        }
    }
}
