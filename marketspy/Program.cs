using System;

namespace marketspy
{
    internal class Program
    {
        static store store = new store();
        static void Main(string[] args)
        {
            //1.启动程序需要配置一个启动时间
            //2.从启动时间开始补数据，如果数据还离得很远，用历史数据rest

            //3.当历史数据已经来到和now 同一天(utc时间，启动websocket监听。切换为同步模式，历史追上同步时间后停止

            //4.断线后切换到历史模式，并尝试重连。

            //5.历史模式，每24小时数据写入一次数据。实时模式每15分钟写入一次数据，需要知道数据完成度

            store.Open();
            Console.WriteLine("Hello World!");

            store.Clear();

            var day = new MarketDay();
            day.day = DateTime.Now;
            day.records = new Record[60 * 24];
            store.WriteDay(day);

            var start = store.GetStartTime();
            var d = store.GetDayData(DateTime.Now);

            binance.te();
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
