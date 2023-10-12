using btrade;

namespace traderserver
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Trader Server 启动");
            Console.WriteLine("将开启一个websocket 行情服务器");
            Init();
            while (!bExit)
            {
                System.Threading.Thread.Sleep(100);
            }
        }
        static bool bExit = false;
        static TradeTool tradeTool;
        //static Trader trader;
        static async void Init()
        {
            tradeTool = new btrade.TradeTool();
            await tradeTool.Init();
            Console.WriteLine("行情已初始化");
            //ethbusd  //ethusdt
            var trader = tradeTool.CreateTrader("ethusdt");

            //取12小时的行情
            var starttime = DateTime.Now - new TimeSpan(12, 0, 0);
            trader.OnKLine += (data) =>
            {
                Console.WriteLine("新的行情：" + data.time);
            };
            await trader.StartKLineSocket();
            var history = await trader.GetKLines(Binance.Net.Enums.KlineInterval.OneMinute, starttime);

            Console.WriteLine("行情数量=" + history.Length);
        }
    }
}