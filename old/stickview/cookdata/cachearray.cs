using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public class CacheArray
{
    public string Name
    {
        get;
        private set;
    }

    public int MaxCount //write head 0
    {
        get;
        private set;
    }
    public int Count //write head 4
    {
        get;
        private set;
    }
    public int ElementSize //write head 8
    {
        get;
        private set;
    }

    public string FileName
    {
        get
        {
            var _path = System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location);
            var cachepath = System.IO.Path.Combine(_path, "cache");
            if (System.IO.Directory.Exists(cachepath) == false)
                System.IO.Directory.CreateDirectory(cachepath);

            var filename = System.IO.Path.Combine(cachepath, "cache_" + Name + ".bin");
            return filename;
        }
    }
    const int headsize = 12;
    public CacheArray(string name, Type element, int maxcount)
    {
        this.Name = name;
        this.MaxCount = maxcount;
        this.Count = 0;
        this.ElementSize = element.StructLayoutAttribute.Size;
        if (this.ElementSize == 0)
            throw new Exception("error size.");
        unsafe
        {

            this.buf = new byte[headsize + MaxCount * ElementSize];
            fixed (byte* ptr = buf)
            {
                bufptr = ptr;
            }
        }
    }

    byte[] buf;
    unsafe byte* bufptr;
    protected unsafe void* GetItemPtrUnsafe(int index = 0)
    {
        unsafe
        {
            if (index == 0)
                return bufptr + headsize;

            if (index < 0 || index > this.Count)
                throw new Exception("out of index");

            byte* ptr = bufptr + headsize + index * ElementSize;
            return ptr;
        }
    }

    List<KeyValuePair<int, int>> changelist = new List<KeyValuePair<int, int>>();
    protected void MarkDirty(int itembegin, int itemend)
    {
        if (Count < itemend)
            Count = itemend;
        changelist.Add(new KeyValuePair<int, int>(itembegin, itemend));
    }
    public void Load()
    {
        var filename = this.FileName;
        if (System.IO.File.Exists(filename) == true)
        {
            using (var fs = System.IO.File.OpenRead(filename))
            {
                fs.Read(buf, 0, buf.Length);
                var maxcount = BitConverter.ToInt32(buf, 0);
                var count = BitConverter.ToInt32(buf, 4);
                var esize = BitConverter.ToInt32(buf, 8);
                if (maxcount != MaxCount)
                    throw new Exception("error MaxCount");
                if (esize != ElementSize)
                    throw new Exception("error ElementSize");
                if (count < 0 || count >= maxcount)
                    throw new Exception("error count");
                this.Count = count;
            }
        }
        else
        {
            var maxcount = BitConverter.GetBytes(MaxCount);
            var count = BitConverter.GetBytes(Count);
            var esize = BitConverter.GetBytes(ElementSize);
            for (var i = 0; i < 4; i++)
            {
                buf[i + 0] = maxcount[i];
                buf[i + 4] = count[i];
                buf[i + 8] = esize[i];
            }
            System.IO.File.WriteAllBytes(filename, buf);
        }

    }
    public void Apply()
    {


        var filename = this.FileName;

        var maxcount = BitConverter.GetBytes(MaxCount);
        var count = BitConverter.GetBytes(Count);
        var esize = BitConverter.GetBytes(ElementSize);
        for (var i = 0; i < 4; i++)
        {
            buf[i + 0] = maxcount[i];
            buf[i + 4] = count[i];
            buf[i + 8] = esize[i];
        }
        var bnew = System.IO.File.Exists(filename) == false;

        using (var fs = System.IO.File.OpenWrite(filename))
        {
            if (changelist.Count > 10 || bnew)
            {
                fs.Seek(0, System.IO.SeekOrigin.Begin);
                fs.Write(buf, 0, buf.Length);
            }
            else
            {
                fs.Seek(0, System.IO.SeekOrigin.Begin);
                fs.Write(buf, 0, headsize);

                foreach (var change in changelist)
                {
                    int seek = headsize + change.Key * ElementSize;
                    int seek2 = headsize + change.Value * ElementSize;
                    fs.Seek(seek, System.IO.SeekOrigin.Begin);
                    fs.Write(buf, seek, seek2 - seek);
                }
            }
        }

        changelist.Clear();
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 52)]
public struct CacheRecord
{
    public int index;
    public double price_open;
    public double price_close;
    public double price_high;
    public double price_low;
    public double volume;
    public double volume_buy;
}

public class CachedRecordArray : CacheArray
{
    public CachedRecordArray(string name, int maxcount) : base(name, typeof(CacheRecord), maxcount)
    {
    }
    public CacheRecord? GetItem(int index)
    {
        if (index >= Count)
            return null;
        unsafe
        {
            return GetItemUnsafe()[index];
        }
    }
    public unsafe CacheRecord* GetItemUnsafe(int index = 0)
    {
        unsafe
        {
            return (CacheRecord*)base.GetItemPtrUnsafe(index);
        }
    }
    public void UpdateRecord(CacheRecord record)
    {
        unsafe
        {
            var ptr = GetItemUnsafe(record.index);
            ptr[0] = record;
            MarkDirty(record.index, record.index + 1);
        }
    }

    public void UpdateRecords(CacheRecord[] record, int index = 0, int count = -1)
    {
        if (count < 0)
            count = record.Length;

        unsafe
        {
            int first = record[index].index;
            var ptr = GetItemUnsafe(first);

            for (var i = 0; i < count; i++)
            {
                if (record[index + i].index != first + i)
                    throw new Exception("id 不连续");
                ptr[i] = record[index + i];
            }

            MarkDirty(first, first + count);
        }
    }
}


