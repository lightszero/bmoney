using System;
using System.Threading.Tasks;

namespace marketspy
{
    internal class Program
    {
        static Recorder recorder = new Recorder();
       
        static void Main(string[] args)
        {
            //1.启动程序需要配置一个启动时间
            //2.从启动时间开始补数据，如果数据还离得很远，用历史数据rest

            //3.当历史数据已经来到和now 同一天(utc时间，启动websocket监听。切换为同步模式，历史追上同步时间后停止

            //4.断线后切换到历史模式，并尝试重连。

            //5.历史模式，每24小时数据写入一次数据。实时模式每15分钟写入一次数据，需要知道数据完成度

            DateTime startday;
            try
            {
                var days = args[0].Split('-',StringSplitOptions.RemoveEmptyEntries);
                startday = new DateTime(int.Parse(days[0]), int.Parse(days[1]), int.Parse(days[2]), 0, 0, 0, DateTimeKind.Utc);
            }
            catch
            {
                Console.WriteLine("error params startday:\n   sample: marketspy 2021-1-1");
                return;
            }
            Console.WriteLine("startday(utc)="+startday);
            Console.WriteLine("startday(local)=" + startday.ToLocalTime());
            recorder.Begin(startday);

            StateBar();
           
            while (true)
            {
                Console.ReadLine();
            }
        }
        static async void StateBar()
        {
            while (true)
            {
                var curtop = Console.CursorTop;
                var curleft = Console.CursorLeft;

                
                Console.SetCursorPosition(curleft, curtop-1);
               
                Console.Write("[state] symbol=" + recorder.symbol);
               
                var r = recorder.GetLastRealRecord(out UInt16 index,out bool final);
                if(r!=null)
                {
                    Console.Write(" i=" + index);
                   Console.Write("  price=" + r.Value.price_close);
                }
                  

                Console.WriteLine();
                await Task.Delay(1000);
            }
        }
    }
}
