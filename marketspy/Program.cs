using System;

namespace marketspy
{
    internal class Program
    {
        static store store = new store();
        static void Main(string[] args)
        {
            store.Open();
            Console.WriteLine("Hello World!");

            store.Clear();

            var day = new MarketDay();
            day.day = DateTime.Now;
            day.records = new Record[60 * 24];
            store.WriteDay(day);

            var start = store.GetStartTime();
            var d = store.GetDayData(DateTime.Now);
            while(true)
            {
                Console.ReadLine();
            }
        }
    }
}
