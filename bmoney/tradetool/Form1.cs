using Binance.Net.Interfaces;
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

        async void InitTrade()
        {
            await btrade.TradeTool.Init();
            var myip = "IP=" + btrade.TradeTool.IP;
            this.label1.Text = myip + " 这是一个币安下单助手，测试下单API的稳定性";

            var info = await btrade.TradeTool.GetInfo();
            this.listBox1.Items.Clear();
            this.listBox1.Items.Add("CanTrade=" + info.CanTrade);
            this.listBox1.Items.Add("CanDeposit=" + info.CanDeposit);
            this.listBox1.Items.Add("CanWithdraw=" + info.CanWithdraw);
            foreach (var a in info.Assets)
            {
                this.listBox1.Items.Add("item=" + a.Asset + "=" + a.AvailableBalance + " ," + a.WalletBalance);
            }


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

            var sym = btrade.TradeTool.GetSymbol(btrade.TradeTool.Symbol);
            PricePrecision = sym.PricePrecision;
            this.listBox2.Items[3] = "价格精度" + sym.PricePrecision;

            ResetPirce();
        }


        void FillKLine(IBinanceStreamKlineData kdata)
        {

            this.listBox2.Items[0] = kdata.Symbol + ":" + kdata.Data.CloseTime;
            this.listBox2.Items[1] = "最新价格=" + kdata.Data.ClosePrice;
            finalprice =kdata.Data.ClosePrice;
            ResetPirce();
        }
        double fee;
        int PricePrecision;
        decimal finalprice;
        bool longorshort = true;
        int scale;
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

            label3.Text = "止盈" + finalwinPrice;
            label4.Text = "止损" + finallosePrice;
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

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            double winv = (1.0 + (winpoint / scale) + 0.0003 + fee);
            double losev = (1.0 - ((losepoint / scale) + 0.0003 - fee));
            btrade.TradeTool.Go(longorshort, 10, finalwinPrice,finallosePrice);
        }
    }
}
