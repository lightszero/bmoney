using BMoney;
using BMoney.Indicator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney.Trade
{
    //一个Trader 是一个交易方法，根据该交易方法进行交易
    public enum TradeAction
    {
        None,
        GoLong,//做多
        Short,//做空
        CloseLong,//平多
        CloseShort,//平空
    }
    public struct TradeItem
    {
        public int candleindex;
        public TradeAction action;
        public double value;
        public double price;
        public static TradeItem None(int index) => new TradeItem() { candleindex = index, action = TradeAction.None, price = 0, value = 0 };
        public static TradeItem GoLang(int index, double price, double value)
        {
            return new TradeItem() { candleindex = index, action = TradeAction.GoLong, price = price, value = value };
        }
        public static TradeItem Short(int index, double price, double value)
        {
            return new TradeItem() { candleindex = index, action = TradeAction.Short, price = price, value = value };
        }
        public static TradeItem CloseLong(int index, double price, double value)
        {
            return new TradeItem() { candleindex = index, action = TradeAction.CloseLong, price = price, value = value };
        }
        public static TradeItem CloseShort(int index, double price, double value)
        {
            return new TradeItem() { candleindex = index, action = TradeAction.CloseShort, price = price, value = value };
        }
    }
    public interface ITrader
    {
        public double InitMoney
        {
            get;
        }
        //钱trader自己内部管理即可，不需要暴露
        //public float Money
        //{
        //    get;
        //    set;
        //}
        ////货
        //public float Goods
        //{
        //    get;
        //    set;
        //}
        void OnReg(CandlePool pool);
        //有行情更新时调用，这里并不保证会交易成功，Trader自己保障
        TradeItem[] OnStick(CandlePool pool, int candleIndex);

        //交易结果回调
        void TradeResult(int candleIndex, TradeItem result);

    }
    public abstract class Trader : ITrader
    {

        public Trader(float initmoney = 1000.0f)
        {
            InitMoney = initmoney;
            money = initmoney;
            shortmoney = 0;
            longvalue = 0;
            shortvalue = 0;
        }
        public double InitMoney
        {
            get;
            private set;
        }

        protected double money;
        protected double shortmoney;//担保金
        protected double longvalue;
        protected double shortvalue;

        public virtual void OnReg(CandlePool pool)
        {

        }
        public abstract TradeItem[] OnStick(CandlePool pool, int candleIndex);
        public virtual void TradeResult(int candleIndex, TradeItem item)
        {
            if (item.value == 0 || item.action == TradeAction.None)
                return;
            if (item.action == TradeAction.CloseShort)//结空单算钱很麻烦
            {
                var shortprice = this.shortmoney / this.shortvalue;
                var trademoney = item.value * shortprice;

                this.shortvalue -= item.value;
                this.shortmoney -= trademoney;//还钱
                    this.money += trademoney;

                this.money += trademoney - item.price * item.value; //得到担保金，损失现价
            }
            else if (item.action == TradeAction.CloseLong)
            {
                this.longvalue -= item.value;
                this.money += item.price * item.value;
            }
            else if (item.action == TradeAction.GoLong)
            {
                this.longvalue += item.value;
                this.money -= item.price * item.value;
            }
            else if (item.action == TradeAction.Short)
            {
                this.shortvalue += item.value; //开空单
                var trademoney = item.price * item.value;
                this.shortmoney += trademoney;
                this.money -= trademoney; //开空单扣掉的钱是担保，
            }
        }
    }
}