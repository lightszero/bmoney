using System;
using System.Collections.Generic;
using System.Text;

namespace marketspy
{
    public struct Record
    {
        public double price_open;
        public double price_close;
        public double price_high;
        public double price_low;
        public double volume;
        public double volume_buy;
        public void WriteTo(byte[] data, int seek)
        {
            var o = BitConverter.GetBytes(price_open);
            var c = BitConverter.GetBytes(price_close);
            var h = BitConverter.GetBytes(price_high);
            var l = BitConverter.GetBytes(price_low);
            var v = BitConverter.GetBytes(volume);
            var vb = BitConverter.GetBytes(volume_buy);
            for (var i = 0; i < 8; i++)
            {
                data[seek + i] = o[i];
                data[seek + i + 8] = c[i];
                data[seek + i + 16] = h[i];
                data[seek + i + 24] = l[i];
                data[seek + i + 32] = v[i];
                data[seek + i + 40] = vb[i];
            }
        }
        public void ReadFrom(byte[] data, int seek)
        {
            price_open = BitConverter.ToDouble(data, seek);
            price_close = BitConverter.ToDouble(data, seek + 8);
            price_high = BitConverter.ToDouble(data, seek + 16);
            price_low = BitConverter.ToDouble(data, seek + 24);
            volume = BitConverter.ToDouble(data, seek + 32);
            volume_buy = BitConverter.ToDouble(data, seek + 40);
        }
    }

    public class MarketDay
    {
        DateTime _day;
        public UInt16 count; //maxsize=60x24
        public DateTime day
        {
            get
            {
                return _day;
            }
            set
            {
                var ltime = value.ToUniversalTime();
                _day = (new DateTime(ltime.Year, ltime.Month, ltime.Day, 0, 0, 0, ltime.Kind)).ToLocalTime();
            }
        }
        public MarketDay Clone()
        {
            MarketDay nday = new MarketDay();
            nday.count = this.count;
            nday._day = this._day;
            nday.records = new Record[this.records.Length];
            for(var i=0;i<nday.records.Length;i++)
            {
                nday.records[i] = this.records[i];
            }
            return nday;
        }
        public Record[] records;//record

        public static byte[] ToTimeValue(DateTime day)
        {
            var uday = day.ToUniversalTime();
            DateTime ntime = new DateTime(uday.Year, uday.Month, uday.Day, 0, 0, 0, DateTimeKind.Utc);
            var utime = ntime.ToFileTimeUtc();

            var body = BitConverter.GetBytes(utime);
            return body;
        }
        public byte[] ToTimeValue()
        {
            return ToTimeValue(this._day);
        }
        public static byte[] ToKey(DateTime day)
        {
            var uday = day.ToUniversalTime();
            DateTime ntime = new DateTime(uday.Year, uday.Month, uday.Day, 0, 0, 0, DateTimeKind.Utc);
            var utime = ntime.ToFileTimeUtc();
            byte[] key = new byte[12];
            var head = System.Text.Encoding.ASCII.GetBytes("day_");
            var body = BitConverter.GetBytes(utime);
            for (var i = 0; i < 12; i++)
            {
                if (i < 4)
                    key[i] = head[i];
                else
                    key[i] = body[i - 4];
            }
            return key;
        }
        public byte[] ToKey()
        {
            return ToKey(this._day);
        }

        public byte[] ToValue()
        {
            int totallen = 12 + 2+ 6 * 8 * 60 * 24;
            byte[] result = new byte[totallen];

            var key = ToKey();
            for (var i = 0; i < 12; i++)
            {
                result[i] = key[i];
            }
            byte[] len = BitConverter.GetBytes(count);
            result[12] = len[0];
            result[13] = len[1];
            for (var i = 0; i < 60 * 24; i++)
            {
                var seek = 14 + i * 6 * 8;
                records[i].WriteTo(result, seek);
            }
            return result;
        }
        public static MarketDay Parse(byte[] data)
        {
            int totallen = 12 + 2 + 6 * 8 * 60 * 24;
            if (totallen != data.Length)
            {
                return null;
            }

            var head = System.Text.Encoding.ASCII.GetBytes("day_");

            if (head[0] != data[0] || head[1] != data[1] || head[2] != data[2] || head[3] != data[3])
            {
                throw new Exception("error data.");
            }
            var utime = BitConverter.ToInt64(data, 4);
            MarketDay day = new MarketDay();
            day.day = DateTime.FromFileTimeUtc(utime).ToLocalTime();
            day.count = BitConverter.ToUInt16(data, 12);
            day.records = new Record[60 * 24];
            for (var i = 0; i < 60 * 24; i++)
            {
                var seek = 14 + i * 6 * 8;
                day.records[i].ReadFrom(data, seek);
            }
            return day;
        }
    }
    class store
    {

        RocksDbSharp.RocksDb db;
        string path;
        public void Open()
        {
            var _path = System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location);
            path = System.IO.Path.Combine(_path, "db");
            if (System.IO.Directory.Exists(path) == false)
                System.IO.Directory.CreateDirectory(path);
            Console.WriteLine("dbpath=" + path);

            var op = new RocksDbSharp.DbOptions().SetCreateIfMissing(true).SetCompression(RocksDbSharp.Compression.No);
            db = RocksDbSharp.RocksDb.Open(op, path);


        }
        public void Close()
        {
            if (this.db == null) throw new Exception("not open db:" + this.path);
            this.db.Dispose();
            this.db = null;
            Console.WriteLine("==>DB close:" + this.path);
        }

        public DateTime? GetStartTime()
        {
            var head = System.Text.Encoding.ASCII.GetBytes("beginday_");
            var data = this.db.Get(head);
            if (data == null || data.Length != 8)
                return null;

            var time = DateTime.FromFileTimeUtc(BitConverter.ToInt64(data,0));
            var rtime = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0, DateTimeKind.Utc);
            return rtime.ToLocalTime();
        }
        public DateTime? GetEndTime()
        {
            var head = System.Text.Encoding.ASCII.GetBytes("endday_");
            var data = this.db.Get(head);
            if (data == null || data.Length != 8)
                return null;

            var time = DateTime.FromFileTimeUtc(BitConverter.ToInt64(data,0));
            var rtime = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0, DateTimeKind.Utc);
            return rtime.ToLocalTime();
        }
        public MarketDay GetDayData(DateTime day)
        {
            var key = MarketDay.ToKey(day);
            var data = db.Get(key);
            if (data == null) return null;
            return MarketDay.Parse(data);
        }
        public void Clear()
        {
            var beginkey = System.Text.Encoding.ASCII.GetBytes("beginday_");
            var endkey = System.Text.Encoding.ASCII.GetBytes("endday_");
            db.Remove(beginkey);
            db.Remove(endkey);
        }
        RocksDbSharp.WriteOptions syncop = new RocksDbSharp.WriteOptions().SetSync(true);
        public void WriteDay(MarketDay day)
        {
            var start = GetStartTime();
            var end = GetEndTime();

            var beginkey = System.Text.Encoding.ASCII.GetBytes("beginday_");
            var endkey = System.Text.Encoding.ASCII.GetBytes("endday_");

            RocksDbSharp.WriteBatch wb = new RocksDbSharp.WriteBatch();

            if (start == null || end == null)
            {
                //init
                wb.Put(beginkey, day.ToTimeValue());
                wb.Put(endkey, day.ToTimeValue());
            }
            else
            {
                if (day.day < start.Value)
                {
                    if ((start.Value - day.day).Days == 1)//向前多写一天
                    {
                        db.Put(beginkey, day.ToTimeValue());
                    }
                    else
                    {
                        throw new Exception("too early day write.");
                    }
                }
                else if (day.day > end.Value)
                {
                    if ((day.day - end.Value).Days == 1)//向后多写一天
                    {
                        db.Put(endkey, day.ToTimeValue());
                    }
                    else
                    {
                        throw new Exception("too late day write.");
                    }
                }
                else
                {
                    throw new Exception("already has day data.");
                }
            }

            var key = day.ToKey();
            var value = day.ToValue();

            wb.Put(key, value);

            db.Write(wb);
        }
        public void UpdateDay(MarketDay day)
        {
            var end = GetEndTime();
            if (day.day != end)
                throw new Exception("only can update last day.");

            var key = day.ToKey();
            var value = day.ToValue();

            db.Put(key, value);
        }
    }
}
