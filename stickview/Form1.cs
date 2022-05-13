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

        private void Form1_Load(object sender, EventArgs e)
        {

            chart1.Images.Add(new System.Windows.Forms.DataVisualization.Charting.NamedImage("hi", imageList1.Images[0]));


            ViewMgr.Init(chart1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ViewMgr.FillData_Test();
        }

        private void chart1_CursorPositionChanged(object sender, System.Windows.Forms.DataVisualization.Charting.CursorEventArgs e)
        {
            if (e.ChartArea == chart1.ChartAreas[0])
            {
                chart1.ChartAreas[1].CursorX.SelectionStart = e.NewSelectionStart;
                chart1.ChartAreas[1].CursorX.SelectionEnd = e.NewSelectionEnd;
            }
        }

        private void chart1_SelectionRangeChanged(object sender, System.Windows.Forms.DataVisualization.Charting.CursorEventArgs e)
        {

        }

        private void chart1_GetToolTipText(object sender, System.Windows.Forms.DataVisualization.Charting.ToolTipEventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void chart1_MouseClick(object sender, MouseEventArgs e)
        {
            var hit = chart1.HitTest(e.X, e.Y);
            chart1.ChartAreas[1].CursorX.SetCursorPosition(chart1.ChartAreas[0].CursorX.Position);
            chart1.ChartAreas[2].CursorX.SetCursorPosition(chart1.ChartAreas[0].CursorX.Position);

        }

        private void splitContainer2_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
