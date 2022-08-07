
namespace BMoney
{
    static class Program
    {
        static void Main(string[] argv)
        {
            Console.WriteLine("cool");

            //TestCandlePool_Random();
            TestCandlePool_CSV();

            //Test_Binance();


            //Console.WriteLine("按任意键退出！！！");
            //Console.ReadLine();
        }

        static void Test_Binance()
        {
            var start = DateTime.Now - TimeSpan.FromHours(8);
            var startonM = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, 0);
            var import = new data.BinanceImporter(false, startonM, TimeSpan.FromMinutes(1.0));
            CandlePool pool = new CandlePool("tpool", import.Tick);
            //注册一个新的指标，必须在push data之前
            pool.RegIndicator(Indicator.IndicatorFactory.Create("KDJ", new string[] { "9", "3", "3" }));
            //pool.RegIndicator(Indicator.IndicatorFactory.Create("EMA", null));
            pool.RegIndicator(Indicator.IndicatorFactory.Create("MACD", null));

            import.Start(pool);

            if(import.IsAsync)
            {

                while(import.IsActive)
                {
                    System.Threading.Thread.Sleep(1000);
                    Console.WriteLine("Wait async Importer.");
                }
            }
            Console.WriteLine("Importer Done.");

            data.CSVImporter.SaveToCSV("data_b.csv", pool);

            pool.Dump();

            HtmlView.Show(pool);
        }
        static void TestCandlePool_CSV()
        {
            //概念超级简单的量化交易系统
            //pool 就是k线数据库，K线数据不大，暂时不用限制
            //1.首先是指标系统，现在实现了KDJ，MACD
            //先RegIndicator 再Push数据（）
            //2.Push数据通常由Importer负责
            //会实现CSV 和 币安Importer
            //3.报告系统
            //目前实现了HtmlView报告
            //有了1 2 3 已经可以开始搞分析，把分析工具也做成指标
            //4.下单系统
            //*** 未实现给Indicator增加事件，下单系统通过接收指标回传的事件行动

            var file = CandleUtil.FindFile("testdata/data_b.csv");

            var import = new data.CSVImporter(file);
            CandlePool pool = new CandlePool("tpool", import.Tick);
            //注册一个新的指标，必须在push data之前
            pool.RegIndicator(Indicator.IndicatorFactory.Create("KDJ", new string[] { "9", "3", "3" }));
            //pool.RegIndicator(Indicator.IndicatorFactory.Create("EMA", null));
            pool.RegIndicator(Indicator.IndicatorFactory.Create("MACD", null));

            //使用import推数据进去
            import.Start(pool);

            pool.Dump();

            HtmlView.Show(pool);

        }
        static void TestCandlePool_Random()
        {
            CandlePool pool = new CandlePool("tpool", TimeSpan.FromMinutes(5));
            //注册一个新的指标，必须在push data之前
            pool.RegIndicator(Indicator.IndicatorFactory.Create("KDJ",new string[]{ "9","3","3"}));
            //pool.RegIndicator(Indicator.IndicatorFactory.Create("EMA", null));
            pool.RegIndicator(Indicator.IndicatorFactory.Create("MACD", null));
            //随便喂点数据进去，每喂一个数据，都会计算所有注册的指标
            CandleUtil.PushRandomData(pool, DateTime.Now, TimeSpan.FromMinutes(5), 10000);

            data.CSVImporter.SaveToCSV("data1.csv", pool);

            pool.Dump();

            HtmlView.Show(pool);
        }
    }

}