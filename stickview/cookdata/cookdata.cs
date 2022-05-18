using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class cookdata
{
    marketspy.Recorder recoder = new marketspy.Recorder();
    public void Init()
    {
        recoder.Begin(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Local));
    }
    public string symbol
    {
        get
        {
            return recoder.symbol;
        }
    }
    public string GetStateString()
    {
        var txt = "";
        var time = recoder.GetStartTime();
        if (time == null)
        {
            txt = "data not init.";
        }
        var time2 = recoder.GetEndTime();
        var timecool = marketspy.Recorder.GetUtcDay(DateTime.Now).ToLocalTime();

        if (time2 != null)
        {
            txt = time.Value.ToLocalTime().ToString();
            txt += " --- " + time2.Value.ToLocalTime().ToString(); ;

            if (timecool == time2)
            {
                if (recoder.GetLastRealRecord(out ushort index, out bool final) != null)
                {
                    var _time = marketspy.Recorder.GetTime(timecool, index);
                    txt += "  realstick: " + index + " --- " + _time.ToLocalTime().ToString();
                }
                else
                {
                    txt += "  offline.";
                }
            }

        }

        return txt;
    }

    public DateTime? GetStartTime()
    {
        return recoder.GetStartTime();
    }
    public DateTime? GetEndTime()
    {
        return recoder.GetEndTime();
    }
   


    marketspy.Record? GetKLine1(DateTime begin, out bool final)
    {
        var index = TimeTool.GetUtcDayIndex(begin, out DateTime utcday);
        var timecool = marketspy.Recorder.GetUtcDay(DateTime.Now);
        if (timecool == utcday)
        {//realtime data
            return recoder.GetRealRecord((ushort)index, out final);
        }
        final = true;
        if (utcday > GetStartTime()?.ToUniversalTime() && utcday <= GetEndTime()?.ToUniversalTime())
        {
            var hdata = recoder.GetHistoryData(utcday);
            return hdata.records[index];
        }
        else
        {
            return null;
        }

    }
    public  marketspy.Record? GetKLine(KLineSpan span,DateTime begin,out bool final )
    {
        if(span== KLineSpan.Minute_1)
        {
            return GetKLine1(begin, out final);
        }
        throw new Exception("not support yet.");
    }
}


