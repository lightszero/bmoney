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
        var time = 2021;
        recoder.Begin(time);
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
        var time = recoder.startTime;
        if (time == null)
        {
            txt = "data not init.";
        }
        var time2 = recoder.parseTime;
        var timecool = TimeTool.GetUtcDay(DateTime.Now).ToLocalTime();

        if (time2 != null)
        {
            txt = time.ToLocalTime().ToString();
            txt += " --- " + time2.ToLocalTime().ToString(); ;

            if (timecool.ToUniversalTime() == time2)
            {
                if (recoder.GetLastRealRecord(out ushort index, out bool final) != null)
                {
                    var _time = TimeTool.GetTime(timecool, index);
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
        return recoder.startTime;
    }
    public DateTime? GetEndTime()
    {
        return recoder.parseTime;
    }


    CacheRecord? GetKLine1(DateTime begin, out bool final)
    {
        var index = TimeTool.GetUtcDayIndex(begin, out DateTime utcday);
        var timecool = TimeTool.GetUtcDay(DateTime.Now);
        if (timecool == utcday)
        {//realtime data
            return recoder.GetRealRecord((ushort)index, out final);
        }
        final = true;
        return recoder.GetHistoryData(begin);
     
    }
    public CacheRecord? GetKLine(KLineSpan span, DateTime begin, out bool final)
    {
        if (span == KLineSpan.Minute_1)
        {
            return GetKLine1(begin, out final);
        }
        throw new Exception("not support yet.");
    }
}


