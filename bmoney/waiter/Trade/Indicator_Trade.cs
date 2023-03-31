using BMoney.Trade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney.Indicator
{
    //资金跟踪指标


    internal class Indicator_Trade : IIndicator
    {
        ITrader trader;

        public double Money
        {
            get
            {
                return money;
            }
        }
        double money;
        decimal holdvol;//持仓
        double shortmoney;//担保金

        public double MoneyTotal
        {
            get;
            private set;

        }
        //有参构造，这个指标是特别的
        public Indicator_Trade(string name, ITrader trader)
        {
            Name = "Trade_" + name;
            this.trader = trader;
            this.money = 10000.0;//10000美元本金
        }
        public string Name
        {
            get;
            private set;
        }

        public string Description => "Trader跟踪指标:" + Name;

        public string[] GetInitParamDefine()
        {
            return new string[] { };
        }
        public string[] GetParamValue()
        {
            return new string[] { };
        }
        public string[] GetValuesDefine()
        {

            //太乱，直接用一个值改进，持仓，+1 -1

            //Money //做多 //做空 //多结束 //空结束
            return new string[] { "hold$bar$0" };
        }



        public void Init(string[] Param)
        {

        }
        public void OnReg(CandlePool input)
        {
            trader.OnReg(input);
        }

        public double[] CalcValues(CandlePool input, int indicatorIndex, int candleIndex)
        {
            var candle = input.GetCandle(candleIndex);
            var price = (candle.high + candle.low) / 2;

            var action = trader.OnStick(input, candleIndex, money, holdvol);

            var fee = 0.0001;//万五手续费+上

            //先平仓
            if (holdvol < 0 && (action == TradeAction.GoLong || action == TradeAction.Close))
            {
                money = money + shortmoney + shortmoney - ((double)-holdvol * price);
                money -= shortmoney * fee;//扣万五手续费，上难度
                shortmoney = 0;
                holdvol = 0;
            }
            if (holdvol > 0 && (action == TradeAction.Short || action == TradeAction.Close))
            {
                var longmoney = ((double)holdvol * price);
                money = money + longmoney;
                money -= longmoney * fee;//扣万五手续费，上难度
                holdvol = 0;
            }

            //判断做了什么操作，在这里模拟算钱
            if (action == TradeAction.GoLong) //做多
            {

                var longmoney = price * 1.0;//先买一个
                money -= longmoney;

                money -= longmoney * fee;//扣万五手续费，上难度
                this.holdvol += (decimal)1.0;
            }
            if (action == TradeAction.Short)//做空
            {
                shortmoney = price * 1.0;//保证金
                this.holdvol -= (decimal)1.0;
                money -= shortmoney; //不结算money不变

                money -= shortmoney * fee;//扣万五手续费，上难度
            }


            var longm = holdvol > 0 ? ((double)holdvol * price) : 0;
            var shortm = holdvol < 0 ? shortmoney + shortmoney - ((double)-holdvol * price) : 0;

            MoneyTotal = money + longm + shortm;
            return new double[] { (double)holdvol };
        }

    }


    internal class Indicator_TradeRes : IIndicator
    {

        public Indicator_TradeRes(Indicator_Trade trade)
        {
            Name = trade.Name + "(Res)";
            this.trade = trade;
        }
        Indicator_Trade trade;
        public string Name
        {
            get;
            private set;
        }

        public string Description => "用跟踪指标来计算收益的指标";

        public double[] CalcValues(CandlePool input, int indicatorIndex, int candleIndex)
        {
            return new double[1] { trade.MoneyTotal };
        }

        public string[] GetInitParamDefine()
        {
            return new string[] { };
        }

        public string[] GetParamValue()
        {
            return new string[] { };
        }

        public string[] GetValuesDefine()
        {
            return new string[] { "M" };
        }

        public void Init(string[] Param)
        {
        }

        public void OnReg(CandlePool input)
        {
        }
    }
}
