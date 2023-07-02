using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Futures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tradetool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var path = System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location);
            var fullfile = System.IO.Path.Combine(path, "datapage", "index.html");

            InitTrade();
            InitPrice();
        }
        btrade.TradeTool tradetool;
        btrade.Trader trader;

        //Dictionary<string, decimal> wallet = new Dictionary<string, decimal>();
        async void InitTrade()
        {
            tradetool = new btrade.TradeTool();
            await tradetool.Init();
            trader = tradetool.CreateTrader("ethbusd");
            await trader.Init();

            this.webView21.Source = new Uri("https://www.binance.com/en/futures/" + trader.Symbol);
            var myip = "IP=" + tradetool.IP;
            this.label1.Text = myip + " 这是一个币安下单助手，测试下单API的稳定性";

            await tradetool.UpdateWallet();
            await trader.UpdateOrder();

            UpdateWalletUI();

            while (this.listBox2.Items.Count < 10)
                this.listBox2.Items.Add("");
            while (this.listBox2.Items.Count > 10)
            {
                this.listBox2.Items.RemoveAt(this.listBox2.Items.Count - 1);
            }


            trader.OnKLine += (kdata) =>
            {
                //处理线程安全
                Action<IBinanceStreamKlineData> onk = FillKLine;
                this.Invoke(onk, kdata);
            };
            tradetool.OnWalletUpdate += () =>
            {
                Action onacc = () =>
                {
                    UpdateWalletUI();
                };
                this.Invoke(onacc);
            };
            tradetool.OnOrderUpdate += (s) =>
            {
                Action onacc = () =>
                {
                    UpdateWalletUI();
                };
                this.Invoke(onacc);
            };
            fee = await trader.GetFee();
            this.listBox2.Items[2] = "资金费率=" + fee;

            symbol = tradetool.GetSymbol(trader.Symbol);
            PricePrecision = symbol.PricePrecision;
            NumPrecision = symbol.QuantityPrecision;

            this.listBox2.Items[3] = "价格精度" + PricePrecision;
            this.listBox2.Items[4] = "数量精度" + NumPrecision;
            this.listBox2.Items[5] = "BaseAsset:" + symbol.BaseAsset;
            this.listBox2.Items[6] = "QuoteAsset:" + symbol.QuoteAsset;
            ResetPirce();
        }


        void FillKLine(IBinanceStreamKlineData kdata)
        {

            this.listBox2.Items[0] = kdata.Symbol + ":" + kdata.Data.CloseTime;
            this.listBox2.Items[1] = "最新价格=" + kdata.Data.ClosePrice;
            finalprice = kdata.Data.ClosePrice;
            ResetPirce();
        }
        double fee;
        BinanceFuturesUsdtSymbol symbol;
        int PricePrecision;
        int NumPrecision;
        decimal finalprice;
        bool longorshort = true;
        int scale;
        decimal count;
        double winpoint;
        double losepoint;
        decimal finalwinPrice;
        decimal finallosePrice;
        void ResetPirce()
        {
            labelPrice.Text = "现价:" + finalprice;
            label2.Text = (longorshort ? "做多" : "做空") + " X" + scale;
            double winv = 0;
            double losev = 0;
            if (longorshort)
            {//做多
                winv = (1.0 + (winpoint / scale) + 0.0003 + fee);
                losev = (1.0 - ((losepoint / scale) + 0.0003 - fee));

            }
            else
            {
                winv = (1.0 - (winpoint / scale) + 0.0003 - fee);
                losev = (1.0 + ((losepoint / scale) + 0.0003 + fee));
            }
            finalwinPrice = decimal.Round((finalprice * (decimal)winv), PricePrecision);
            finallosePrice = decimal.Round((finalprice * (decimal)losev), PricePrecision);
            label8.Text = "数量" + count;
            label3.Text = "止盈" + finalwinPrice;
            label4.Text = "止损" + finallosePrice;

            //提醒用户，你会下单多少
            if (symbol != null)
            {
                label9.Text = "单位" + symbol.BaseAsset + " = " + (finalprice * count) + symbol.QuoteAsset;

                if (finalprice != 0)
                {
                    var maxcount = decimal.Round(tradetool.wallet.GetBalance(symbol.QuoteAsset.ToLower()) * (decimal)scale / finalprice * (decimal)0.99, symbol.QuantityPrecision);
                    label8.Text = "数量 " + count + "/" + maxcount;
                    label7.Text = "最大可用 " + maxcount + symbol.BaseAsset + " = " + (finalprice * count) + symbol.QuoteAsset;
                }
            }

            if (tradetool.wallet.positions.TryGetValue(trader.Symbol, out var position) == false || position.count == 0)
            {
                textBox2.Text = "0";
                button3.Enabled = false;
                button2.Enabled = true;
            }
            else
            {
                decimal win = 0;
                if (position.count > 0)
                {
                    var tpos = (finalprice - position.price) * position.count;
                    win = tpos * ((decimal)1 - (decimal)0.0003 - (decimal)fee);
                }
                else
                {
                    var tpos = (finalprice - position.price) * position.count;
                    win = tpos * ((decimal)1 - (decimal)0.0003 + (decimal)fee);
                }
                textBox2.Text = position.count + " 浮盈:" + win;
                button3.Enabled = true;
                button2.Enabled = false;
            }



        }
        void InitPrice()
        {
            if (int.TryParse(textBox1.Text, out scale) == false)
                scale = 0;
            if (double.TryParse(textBox3.Text, out winpoint) == false)
            {
                winpoint = 0;
            }
            if (double.TryParse(textBox4.Text, out losepoint) == false)
            {
                losepoint = 0;
            }
            if (decimal.TryParse(textBox5.Text, out count) == false)
                count = 0;
            longorshort = radioButton1.Checked;
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox1.Text, out scale) == false)
                scale = 0;
            ResetPirce();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (double.TryParse(textBox3.Text, out winpoint) == false)
            {
                winpoint = 0;
            }

            ResetPirce();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (double.TryParse(textBox4.Text, out losepoint) == false)
            {
                losepoint = 0;
            }
            ResetPirce();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

            if (radioButton2.Checked)
                longorshort = false;
            ResetPirce();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                longorshort = true;
            ResetPirce();

        }



        private async void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            await trader.MakeOrder(longorshort, count, finalwinPrice, finallosePrice);
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(textBox5.Text, out count) == false)
                count = 0;
            count = decimal.Round(count, NumPrecision);
            ResetPirce();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            //socket 订阅接口不起作用，只能刷钱包了
            await tradetool.UpdateWallet();
            await trader.UpdateOrder();
            UpdateWalletUI();

        }
        void UpdateWalletUI()
        {
            var info = tradetool.wallet;

            this.listBox1.Items.Clear();
            this.listBox1.Items.Add("CanTrade=" + info.CanTrade);
            this.listBox1.Items.Add("CanDeposit=" + info.CanDeposit);
            this.listBox1.Items.Add("CanWithdraw=" + info.CanWithdraw);

            foreach (var a in info.balance.Values)
            {
                if (a.Available != 0 || a.Wallet != 0)
                {
                    this.listBox1.Items.Add("余额 " + a.symbol + "=" + a.Available + "/" + a.Wallet);
                    //wallet[a.symbol.ToLower()] = a.Available;
                }
            }
            foreach (var p in info.positions.Values)
            {
                if (p.count != 0)
                {
                    this.listBox1.Items.Add("仓位 " + p.symbol + "=" + p.count + "(" + p.price + ") stop(" + p.priceStopMin + "," + p.priceStopMax + ")");
                }
            }

        }

        private async void button3_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;
            var pos = tradetool.wallet.GetPosition(trader.Symbol);
            if (pos > 0)
            {
                await trader.MakeOrder_Sell(pos);
            }
            else if (pos < 0)
            {
                await trader.MakeOrder_Buy(-pos);
            }
        }
    }
}
