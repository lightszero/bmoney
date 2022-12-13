using BMoney.Trade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney.Indicator
{


    internal class Indicator_Trade : IIndicator
    {
        ITrader trader;

        double money;
        double shortmoney;//担保金
        double longvalue;
        double shortvalue;
        //有参构造，这个指标是特别的
        public Indicator_Trade(string name, ITrader trader)
        {
            Name = "Trade_" + name;
            this.trader = trader;
            this.money = trader.InitMoney;
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
            return new string[] { "M", "L", "S", "CL", "CS" };
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

            var items = trader.OnStick(input, candleIndex);
            bool hasgolang = false;
            bool hasshort = false;
            bool hasCloseLang = false;
            bool hasCloseShort = false;
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item.action == TradeAction.None)
                        continue;
                    //判断做了什么操作，在这里模拟算钱
                    if (item.action == TradeAction.GoLong)
                    {
                        this.money -= item.price * item.value;
                        this.longvalue += item.value;
                        hasgolang = true;
                    }
                    else if (item.action == TradeAction.CloseLong)
                    {
                        this.longvalue -= item.value;
                        this.money += item.price * item.value;
                        hasCloseLang = true;
                    }
                    else if (item.action == TradeAction.Short)
                    {
                        this.shortvalue += item.value; //开空单
                        var trademoney = item.price * item.value;
                        this.shortmoney += trademoney;
                        this.money -= trademoney; //开空单扣掉的钱是担保，
                        hasshort = true;
                    }
                    else if (item.action == TradeAction.CloseShort)//结空单算钱很麻烦
                    {
                        var shortprice = this.shortmoney / this.shortvalue;
                        var trademoney = item.value * shortprice;

                        this.shortvalue -= item.value;
                        this.shortmoney -= trademoney;//还钱
                        this.money += trademoney;

                        this.money += trademoney - item.price * item.value; //得到担保金，损失现价
                        hasCloseShort = true;
                    }
                    //判断完还得给他丢回去
                    trader.TradeResult(candleIndex, item);
                }
            }


            var moneytotal = money + price * longvalue + shortmoney;
            return new double[] { moneytotal, hasgolang ? 1000 : 0, hasshort ? 1000 : 0, hasCloseLang ? 1000 : 0, hasCloseShort ? 1000 : 0 };
        }

    }
}
