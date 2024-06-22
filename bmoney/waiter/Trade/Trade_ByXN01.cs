using BMoney.Indicator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace BMoney.Trade
{
    class Trade_ByXN01 : ITrader
    {

        IndicatorValueIndex depend_ema12;
        IndicatorValueIndex depend_dif;
        IndicatorValueIndex depend_dea;
        IndicatorValueIndex depend_macd;

        //处理依赖
        public void OnReg(CandlePool pool)
        {
            depend_ema12 = pool.GetIndicatorIndex("macd", "*ema(12)");
            depend_dif = pool.GetIndicatorIndex("macd", "dif");
            depend_dea = pool.GetIndicatorIndex("macd", "dea");
            depend_macd = pool.GetIndicatorIndex("macd", "macd");
        }
        //是否会在平仓后立即做反向，没必要，做反向自动平仓
        public TradeItem OnStick(CandlePool pool, int candleIndex, double money, double holdvol, double fee)
        {
            var candle = pool.GetCandle(candleIndex);
            var price = (candle.high + candle.low) / 2;
            var holdvalue = price * holdvol;
            var side = Math.Abs((holdvalue - money) / (holdvol + money));
            {
                if (side > 0.01)
                {//偏差值大于1%
                    if (holdvalue < money)
                    {//买
                        var bulvol = (money - holdvalue) / 2 / price;
                        return new TradeItem { direct = TradDirect.Buy, value = bulvol };
                    }
                    else
                    {//卖
                        var sellvol = (holdvalue - money) / 2 / price;
                        return new TradeItem { direct = TradDirect.Sell, value = sellvol };
                    }
                }
            }
            return TradeItem.None;
        }
    }

}
