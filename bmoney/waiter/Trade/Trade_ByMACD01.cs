﻿using BMoney.Indicator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney.Trade
{
    class Trade_ByMACD01 : Trader
    {
        public Trade_ByMACD01(float initmoney = 1000.0f) : base(initmoney)
        {
        }

        bool binit = false;
        IndicatorValueIndex depend_ema12;
        IndicatorValueIndex depend_dif;
        IndicatorValueIndex depend_dea;
        IndicatorValueIndex depend_macd;
        public override TradeItem[] OnStick(CandlePool pool, int candleIndex)
        {
           
            if (!binit)
            {
                depend_ema12 = pool.GetIndicatorIndex("macd", "*ema(12)");
                depend_dif = pool.GetIndicatorIndex("macd", "dif");
                depend_dea = pool.GetIndicatorIndex("macd", "dea");
                depend_macd = pool.GetIndicatorIndex("macd", "macd");
                binit = true;
            }
            var candle = pool.GetCandle(candleIndex);
            if (candleIndex > 10)//有多少根行情才开始看
            {
                TradeItem[] items = new TradeItem[2];
                var price = (candle.high + candle.low) / 2;

                double macd_m1 = IndicatorUtil.GetFromIndex(pool, depend_macd, candleIndex - 1);
                double macd = IndicatorUtil.GetFromIndex(pool, depend_macd, candleIndex);
                if (macd < 0 && macd_m1 >= 0)
                {//下交叉，看跌
                    if (longvalue > 0)//如果之前有多仓位
                    {
                        items[0] = TradeItem.CloseLong(candleIndex, price, longvalue);
                    }
                    if (shortvalue == 0)//如果没有空仓，开空仓
                    {
                        double volume = money * 0.5 / price;
                        items[1] = TradeItem.Short(candleIndex, price, volume);
                    }
                }
                else if (macd > 0 && macd_m1 <= 0)
                {//上交叉，看涨
                    if (shortvalue > 0)
                    {
                        items[0] = TradeItem.CloseShort(candleIndex, price, shortvalue);
                    }
                    //else

                    if (longvalue == 0)
                    {
                        double volume = money * 0.5 / price;
                        items[1] = TradeItem.GoLang(candleIndex, price, volume);
                    }
                }
                return items;
            }
            return null;
        }

    }
}