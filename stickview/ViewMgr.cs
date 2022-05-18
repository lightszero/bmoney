using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms;
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
        static Chart chart;
        static Label timeBegin;
        static Label timeEnd;
        static Title title;
        static cookdata cookdata;
        static KLineSpan currentSpan;
        public static void Init(Chart _chart, Label _begin, Label _end, cookdata _data)
        {
            chart = _chart;
            timeBegin = _begin;
            timeEnd = _end;
            cookdata = _data;

            title = chart.Titles.Add("1 minite");
            title.ForeColor = System.Drawing.Color.BlueViolet;
            title.Font = new System.Drawing.Font(title.Font, System.Drawing.FontStyle.Bold);


            area_kline = chart.ChartAreas[0];
            area_kline.Name = "area_kline";
            area_vol = chart.ChartAreas.Add("area_vol");
            area_macd = chart.ChartAreas.Add("area_macd");

            s_kline = chart.Series[0];
            s_kline.Name = "kline";
            s_vol = chart.Series.Add("vol");
            s_macd = chart.Series.Add("macd");
            s_macd_l1 = chart.Series.Add("macd_1");


            s_kline.IsVisibleInLegend = false;
            s_vol.IsVisibleInLegend = false;
            s_macd.IsVisibleInLegend = false;

            chart.Legends[0].Docking = Docking.Top;
            chart.Legends[0].IsDockedInsideChartArea = true;
            chart.Legends[0].DockedToChartArea = area_macd.Name;
            chart.Legends[0].LegendStyle = LegendStyle.Row;
            chart.Legends[0].BackColor = System.Drawing.Color.Transparent;
            s_kline.ChartType = SeriesChartType.Candlestick;
            s_kline.ChartArea = area_kline.Name;

            s_kline.BackGradientStyle = GradientStyle.DiagonalLeft;


            s_vol.ChartArea = area_vol.Name;
            s_vol.ChartType = SeriesChartType.Column;

            s_macd.ChartArea = area_macd.Name;
            s_macd.ChartType = SeriesChartType.Column;
            s_macd_l1.ChartArea = area_macd.Name;
            s_macd_l1.ChartType = SeriesChartType.Line;


            area_kline.InnerPlotPosition.X = 5;
            area_kline.InnerPlotPosition.Y = 0;
            area_kline.InnerPlotPosition.Width = 95;
            area_kline.InnerPlotPosition.Height = 100;
            area_kline.CursorY.IsUserEnabled = true;
            area_kline.CursorY.LineWidth = 1;
            area_kline.CursorX.IsUserEnabled = true;
            area_kline.CursorX.LineWidth = 2;
            area_kline.AxisY.IsStartedFromZero = false;

            area_kline.AxisX.ScrollBar.Enabled = false;
            area_kline.AxisX.ScaleView.Zoomable = false;
            area_kline.AxisX.ScaleView.Size = 100;
            area_kline.AxisX.LabelStyle.Enabled = false;


            area_vol.InnerPlotPosition.X = 5;
            area_vol.InnerPlotPosition.Y = 0;
            area_vol.InnerPlotPosition.Width = 95;
            area_vol.InnerPlotPosition.Height = 100;
            area_vol.CursorX.LineWidth = 2;
            area_vol.CursorX.LineColor = System.Drawing.Color.LawnGreen;
            area_vol.CursorX.IsUserEnabled = true;
            area_vol.AxisY.IsStartedFromZero = false;
            area_vol.AxisX.ScaleView.Zoomable = true;
            area_vol.AxisX.ScrollBar.Enabled = false;
            area_vol.AxisX.ScaleView.Size = 100;
            area_vol.AxisX.LabelStyle.Enabled = false;

            area_macd.InnerPlotPosition.X = 5;
            area_macd.InnerPlotPosition.Y = 0;
            area_macd.InnerPlotPosition.Width = 95;
            area_macd.InnerPlotPosition.Height = 75;
            area_macd.CursorX.LineWidth = 2;
            area_macd.CursorX.LineColor = System.Drawing.Color.DarkBlue;
            area_macd.CursorX.IsUserEnabled = true;
            area_macd.AxisY.IsStartedFromZero = false;
            area_macd.AxisX.ScaleView.Zoomable = false;
            area_macd.AxisX.ScrollBar.Enabled = true;
            area_macd.AxisX.ScaleView.Size = 100;
            area_macd.AxisX.LabelStyle.Enabled = false;

            area_macd.AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
            area_macd.AxisX.ScrollBar.IsPositionedInside = false;
            area_macd.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.None;


        }
        static void InitScroll(KLineSpan span)
        {
            double  _scale = (int)TimeTool.GetSpan(span).TotalMilliseconds * 10000;
            area_macd.AxisX.ScaleView.Size = 100* _scale;
        }
        public static void FillData(DateTime from, DateTime to, KLineSpan span)
        {
            InitScroll(span);
            var _usespan = TimeTool.GetSpan(span);
            DateTime rfrom = TimeTool.GetSpanTime(from, span);
            DateTime rto = TimeTool.GetSpanTime(to + _usespan, span);

            currentSpan = span;
            s_kline.Points.Clear();
            s_vol.Points.Clear();
            s_macd.Points.Clear();
            s_macd_l1.Points.Clear();
            for (DateTime time = rfrom; time <= rto; time += _usespan)
            {

                var id = time.ToFileTimeUtc();
                var rec = cookdata.GetKLine(span, time, out bool final);
                if (rec != null)
                {
                    int x = s_kline.Points.AddXY(id, 0);
                    var min = rec.Value.price_low;
                    var max = rec.Value.price_high;
                    var open = rec.Value.price_open;
                    var close = rec.Value.price_close;
                    s_kline.Points[x].YValues = new double[4] { max, min, open, close };
                    if (open < close)
                        s_kline.Points[x].Color = System.Drawing.Color.Red;
                    else
                        s_kline.Points[x].Color = System.Drawing.Color.Green;


                    s_vol.Points.AddXY(id, rec.Value.volume);
                    s_macd.Points.AddXY(id, rec.Value.volume);
                    s_macd_l1.Points.AddXY(id, rec.Value.volume);

                }
            }


            ScrollToEnd();
            AxisChange(area_macd.AxisX);
        }
        public static void Hit(int x, int y)
        {
            var hit = chart.HitTest(x, y);

            if (hit.ChartArea != null)
            {
                if (hit.ChartArea != area_macd)
                    area_macd.CursorX.SetCursorPosition(hit.ChartArea.CursorX.Position);
                if (hit.ChartArea != area_vol)
                    area_vol.CursorX.SetCursorPosition(hit.ChartArea.CursorX.Position);
                if (hit.ChartArea != area_kline)
                    area_kline.CursorX.SetCursorPosition(hit.ChartArea.CursorX.Position);
            }

        }
        public static void AxisChange(Axis axis)
        {

            long start = (long)axis.ScaleView.Position;
            long end = ((long)axis.ScaleView.Size + (long)axis.ScaleView.Position);
            var time1 = TimeTool.GetSpanTime(DateTime.FromFileTimeUtc(start), currentSpan);
            var time2 = TimeTool.GetSpanTime(DateTime.FromFileTimeUtc(end), currentSpan);
            timeBegin.Text = "" + time1.ToLocalTime();
            timeEnd.Text = "" + time2.ToLocalTime();

            area_macd.AxisX.ScaleView.Size = axis.ScaleView.Size;
            area_macd.AxisX.ScaleView.Position = axis.ScaleView.Position;
            area_kline.AxisX.ScaleView.Size = axis.ScaleView.Size;
            area_kline.AxisX.ScaleView.Position = axis.ScaleView.Position;
            area_vol.AxisX.ScaleView.Size = axis.ScaleView.Size;
            area_vol.AxisX.ScaleView.Position = axis.ScaleView.Position;
        }
        public static void ScrollToEnd()
        {
            chart.Update();
            area_macd.AxisX.ScaleView.Scroll(ScrollType.Last);
            chart.Update();
        }
        public static void FillData_Test()
        {
            area_macd.AxisX.ScaleView.Size = 100;

            Random price = new Random();
            s_kline.Points.Clear();
            for (var i = 100; i < 6000; i += 5)
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


            }

            s_vol.Points.Clear();

            for (var i = 100; i < 6000; i += 5)
            {
                DateTime time = new DateTime(1977, 1, 1, 1, 0, 0);
                time += new TimeSpan(0, i, 0);
                var min = price.NextDouble() * 1000 + 300;
                int x = s_vol.Points.AddXY(i, min);


            }

            s_macd.Points.Clear();
            s_macd_l1.Points.Clear();
            for (var i = 100; i < 6000; i += 5)
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
                if (i % 10 == 0)
                {
                    s_macd.Points[x].MarkerImage = "hi";
                }
                int mx = s_macd_l1.Points.AddXY(i, k);


            }

            ScrollToEnd();
            AxisChange(area_macd.AxisX);
        }
    }
}
