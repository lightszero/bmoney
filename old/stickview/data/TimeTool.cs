using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum KLineSpan
{
    Minute_1,
    Minute_5,
    Minute_15,
    Minute_30,
    Hour_1,
    Hour_4,
    Hour_12,
    Day
}
public class TimeTool
{

    public static long GetIndex(DateTime time)
    {
        return time.ToUniversalTime().ToFileTimeUtc();
    }
    public static DateTime FromIndex(long value)
    {
        return DateTime.FromFileTimeUtc(value).ToLocalTime();
    }
    public static DateTime FromIndex(double value)
    {
        return DateTime.FromFileTimeUtc((long)value).ToLocalTime();
    }
    public static DateTime GetTime(DateTime src, int mindex)
    {
        var ut = src.ToLocalTime();
        return ut + new TimeSpan(0, 0, mindex, 0, 0);
    }
    public static DateTime GetUtcDay(DateTime time)
    {
        var ut = time.ToUniversalTime();
        return new DateTime(ut.Year, ut.Month, ut.Day, 0, 0, 0, DateTimeKind.Utc);
    }
    public static DateTime GetUtcYear(DateTime time)
    {
        var ut = time.ToUniversalTime();
        return new DateTime(ut.Year, 1,1, 0, 0, 0, DateTimeKind.Utc);
    }
    public static int GetNowYear()
    {
        return DateTime.Now.ToUniversalTime().Year;
    }
    public static ushort GetUtcDayIndex(DateTime time, out DateTime utc)
    {
        utc = GetUtcDay(time);
        ushort index = (ushort)(time.ToUniversalTime() - utc).TotalMinutes;
        return index;
    }

    public static DateTime GetSpanTime(DateTime time,KLineSpan span)
    {
        var ut = time.ToUniversalTime();
        switch (span)
        {
            case KLineSpan.Minute_1:
                return new DateTime(ut.Year, ut.Month, ut.Day, ut.Hour, ut.Minute, 0, DateTimeKind.Utc);
            case KLineSpan.Minute_5:
                {
                    int intm = ut.Minute / 5;
                    return new DateTime(ut.Year, ut.Month, ut.Day, ut.Hour, intm * 5, 0, DateTimeKind.Utc);
                }
            case KLineSpan.Minute_15:
                {
                    int intm = ut.Minute / 15;
                    return new DateTime(ut.Year, ut.Month, ut.Day, ut.Hour, intm * 15, 0, DateTimeKind.Utc);
                }
            case KLineSpan.Minute_30:
                {
                    int intm = ut.Minute / 30;
                    return new DateTime(ut.Year, ut.Month, ut.Day, ut.Hour, intm * 30, 0, DateTimeKind.Utc);
                }
            case KLineSpan.Hour_1:
                return new DateTime(ut.Year, ut.Month, ut.Day, ut.Hour, 0, 0, DateTimeKind.Utc);
            case KLineSpan.Hour_4:
                {
                    int inth = ut.Hour / 4;
                    return new DateTime(ut.Year, ut.Month, ut.Day, inth*4, 0, 0, DateTimeKind.Utc);
                }
            case KLineSpan.Hour_12:
                {
                    int inth = ut.Hour / 12;
                    return new DateTime(ut.Year, ut.Month, ut.Day, inth * 12, 0, 0, DateTimeKind.Utc);
                }
            case KLineSpan.Day:
                return new DateTime(ut.Year, ut.Month, ut.Day, 0, 0, 0, DateTimeKind.Utc);
            default:
                throw new Exception("not support");
        }
       
    }
    public static TimeSpan GetSpan(KLineSpan span)
    {
        switch (span)
        {
            case KLineSpan.Minute_1:
                return new TimeSpan(0, 1, 0);
            case KLineSpan.Minute_5:
                return new TimeSpan(0, 5, 0);
            case KLineSpan.Minute_15:
                return new TimeSpan(0, 15, 0);
            case KLineSpan.Minute_30:
                return new TimeSpan(0, 39, 0);
            case KLineSpan.Hour_1:
                return new TimeSpan(1, 0, 0);
            case KLineSpan.Hour_4:
                return new TimeSpan(4, 0, 0);
            case KLineSpan.Hour_12:
                return new TimeSpan(4, 0, 0);
            case KLineSpan.Day:
                return new TimeSpan(1, 0, 0, 0);
            default:
                throw new Exception("not support");
        }
    }
}

