using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms.DataVisualization.Charting;
namespace stickview
{
    static class ViewMgr
    {
        static ChartArea area_kline;
        static ChartArea area_vol;
        static ChartArea area_macd;
        static Series s_kline;
        static Series s_vol;
        static Series s_macd;
        static Series s_macd_l1;

        public static void Init(Chart chart)
        {
        
            area_kline = chart.ChartAreas[0];
            area_kline.Name = "area_kline";
            area_vol = chart.ChartAreas.Add("area_vol");
            area_macd = chart.ChartAreas.Add("area_macd");

            s_kline = chart.Series[0];
            s_kline.Name = "kline";
            s_vol = chart.Series.Add("vol");
            s_macd = chart.Series.Add("macd");
            s_macd_l1 = chart.Series.Add("macd_1");


            s_kline.ChartType = SeriesChartType.Candlestick;
            s_kline.ChartArea = area_kline.Name;
            //s_kline.XAxisType = AxisType.Primary;
            s_kline.XValueType = ChartValueType.Time;
            
            //s_kline.YAxisType = AxisType.Secondary;
            s_kline.BackGradientStyle = GradientStyle.DiagonalLeft;

            
            s_vol.ChartArea = area_vol.Name;
            s_vol.ChartType = SeriesChartType.Column;

            s_macd.ChartArea = area_macd.Name;
            s_macd.ChartType = SeriesChartType.Column;
            s_macd_l1.ChartArea = area_macd.Name;
            s_macd_l1.ChartType = SeriesChartType.Line;
            


            area_kline.CursorY.IsUserEnabled = true;
            area_kline.CursorY.LineWidth = 2;
            area_kline.CursorX.IsUserEnabled = true;
            area_kline.CursorX.LineWidth = 2;
            


            area_kline.AxisX.ScaleView.Zoomable = true;
            //area_kline.AxisX.Interval = 10;
            area_kline.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
             area_kline.AxisY.IsStartedFromZero = false;
            area_kline.AxisX.ScrollBar.Enabled = true;
            area_kline.AxisX.ScaleView.SmallScrollSize = double.NaN;
            area_kline.AxisX.ScaleView.SmallScrollMinSize = 1;


            area_vol.CursorX.LineWidth = 2;
            area_vol.CursorX.IsUserEnabled = true;
            area_vol.AxisX.ScaleView.Zoomable = true;
            area_vol.AxisY.IsStartedFromZero = false;


            area_macd.CursorX.LineWidth = 2;
            area_macd.AxisX.ScaleView.Zoomable = true;
            area_macd.AxisY.IsStartedFromZero = false;
        }
        public static void FillData()
        {

        }

        public static void FillData_Test()
        {
            Random price = new Random();
            s_kline.Points.Clear();
            for (var i = 0; i < 100; i++)
            {
                DateTime time = new DateTime(1977, 1, 1, 1, 0, 0);
                time += new TimeSpan(0, i, 0);

                int x = s_kline.Points.AddXY(i, i);
                var min = price.NextDouble() * 1000 + 300;
                var max = min + price.NextDouble() * 100;
                var opens = price.NextDouble();
                var closes = price.NextDouble();
                var open = min * opens + max * (1 - opens);
                var close = min * closes + max * (1 - closes);
                s_kline.Points[x].YValues = new double[4] { max, min, open, close };

                if (open < close)

                    s_kline.Points[x].Color = System.Drawing.Color.Red;
                else
                    s_kline.Points[x].Color = System.Drawing.Color.Green;

                s_kline.Points[x].AxisLabel = time.ToString();
            }

            s_vol.Points.Clear();
           
            for (var i = 0; i < 100; i++)
            {
                DateTime time = new DateTime(1977, 1, 1, 1, 0, 0);
                time += new TimeSpan(0, i, 0);
                var min = price.NextDouble() * 1000 + 300;
                int x=s_vol.Points.AddXY(i, min);

                s_vol.Points[x].AxisLabel = time.ToString();
            }
           
            s_macd.Points.Clear();
            s_macd_l1.Points.Clear();
            for (var i = 0; i < 100; i++)
            {
                DateTime time = new DateTime(1977, 1, 1, 1, 0, 0);
                time += new TimeSpan(0, i, 0);
                var min = price.NextDouble() * 100 - 50;
                var k = price.NextDouble() * 100 - 50;
                int x = s_macd.Points.AddXY(i, min);
                if (min > 0)

                    s_macd.Points[x].Color = System.Drawing.Color.Red;
                else
                    s_macd.Points[x].Color = System.Drawing.Color.Green;
                if(i%10==0)
                {
                    s_macd.Points[x].MarkerImage = "hi";
                }
                s_macd_l1.Points.AddXY(x, k);

                s_macd.Points[x].AxisLabel = time.ToString();
            }


        }
    }
}
