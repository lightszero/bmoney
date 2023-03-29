using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Futures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

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
            InitTrade();
            InitPrice();
        }
        Dictionary<string, decimal> wallet = new Dictionary<string, decimal>();
        async void InitTrade()
        {
            await btrade.TradeTool.Init();
            var myip = "IP=" + btrade.TradeTool.IP;
            this.label1.Text = myip + " 这是一个币安下单助手，测试下单API的稳定性";

            await GetWallet();

            while (this.listBox2.Items.Count < 10)
                this.listBox2.Items.Add("");
            while (this.listBox2.Items.Count > 10)
            {
                this.listBox2.Items.RemoveAt(this.listBox2.Items.Count - 1);
            }


            btrade.TradeTool.OnKLine += (kdata) =>
            {
                //处理线程安全
                Action<IBinanceStreamKlineData> onk = FillKLine;
                this.Invoke(onk, kdata);
            };
            fee = await btrade.TradeTool.GetFee();
            this.listBox2.Items[2] = "资金费率=" + fee;

            symbol = btrade.TradeTool.GetSymbol(btrade.TradeTool.Symbol);
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
                    var maxcount = decimal.Round(wallet[symbol.QuoteAsset.ToLower()] * (decimal)scale / finalprice * (decimal)0.99, symbol.QuantityPrecision);
                    label8.Text = "数量 " + count + "/" + maxcount;
                    label7.Text = "最大可用 " + maxcount + symbol.BaseAsset + " = " + (finalprice * count) + symbol.QuoteAsset;
                }
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



        private void button2_Click(object sender, EventArgs e)
        {
            btrade.TradeTool.Go(longorshort, count, finalwinPrice, finallosePrice);
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(textBox5.Text, out count) == false)
                count = 0;
            count = decimal.Round(count, NumPrecision);
            ResetPirce();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //socket 订阅接口不起作用，只能刷钱包了
            GetWallet();

        }
        async Task GetWallet()
        {
            var info = await btrade.TradeTool.GetInfo();
            this.listBox1.Items.Clear();
            this.listBox1.Items.Add("CanTrade=" + info.CanTrade);
            this.listBox1.Items.Add("CanDeposit=" + info.CanDeposit);
            this.listBox1.Items.Add("CanWithdraw=" + info.CanWithdraw);
            foreach (var a in info.Assets)
            {
                this.listBox1.Items.Add("item=" + a.Asset + "=" + a.AvailableBalance + " ," + a.WalletBalance);
                wallet[a.Asset.ToLower()] = a.AvailableBalance;
            }

        }
    }
}
