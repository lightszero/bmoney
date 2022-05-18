using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stickview
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        cookdata data = new cookdata();
        private void Form1_Load(object sender, EventArgs e)
        {

            chart1.Images.Add(new System.Windows.Forms.DataVisualization.Charting.NamedImage("hi", imageList1.Images[0]));

            data.Init();

            ViewMgr.Init(chart1,timeleft,timeright,data);

            timer1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ViewMgr.FillData_Test();
        }


        private void chart1_MouseClick(object sender, MouseEventArgs e)
        {
            ViewMgr.Hit(e.X, e.Y);

        }

        private void chart1_AxisViewChanged(object sender, System.Windows.Forms.DataVisualization.Charting.ViewEventArgs e)
        {
            ViewMgr.AxisChange(e.Axis);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            this.datainfo.Text = data.GetStateString();
         
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(Enum.TryParse<KLineSpan>(this.comboBox1.Text, out KLineSpan span))
            {
                ViewMgr.FillData(this.dateTimePickerFrom.Value, this.dateTimePickerTo.Value, span);
            }
            
        }
    }
}
