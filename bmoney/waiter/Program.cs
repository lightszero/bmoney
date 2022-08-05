
namespace BMoney
{
    static class Program
    {
        static void Main(string[] argv)
        {
            Console.WriteLine("cool");

            TestCandlePool();

            Console.WriteLine("按任意键退出！！！");
            Console.ReadLine();
        }

        static void TestCandlePool()
        {
            CandlePool pool = new CandlePool("tpool", TimeSpan.FromMinutes(5));
            //注册一个新的指标，必须在push data之前
            pool.RegIndicator(Indicator.IndicatorFactory.Create("KDJ",new string[]{ "9","3","3"}));
            pool.RegIndicator(Indicator.IndicatorFactory.Create("EMA", null));
            //随便喂点数据进去，每喂一个数据，都会计算所有注册的指标
            CandleUtil.PushRandomData(pool, DateTime.Now, TimeSpan.FromMinutes(5), 10000);
            

            pool.Dump();

            HtmlView.Show(pool);
        }
    }

}