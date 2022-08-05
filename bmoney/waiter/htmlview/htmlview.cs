using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney
{
    public class HtmlView
    {
        public static void Show(CandlePool pool)
        {
            Console.WriteLine("===html report===");

            var html = FindSrcHtml();
            Console.WriteLine("找到展示页面原型:" + html);


            var htmltemp = CreateTemp(html);
            Console.WriteLine("复制展示页面到:" + htmltemp);

            PutTempFile("inputdata.js", GenDataFile(pool));
            //find html for show
            var htmlcall = "file://" + htmltemp.Replace("\\", "/");
            Process.Start("explorer.exe", htmlcall);
        }
        /// <summary>
        /// 找到参考展示页面
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        static string FindSrcHtml()
        {
            //find html for show
            var path = System.IO.Path.GetDirectoryName(typeof(HtmlView).Assembly.Location);


            var htmlpath = System.IO.Path.Combine(path, "viewframework/index.html");
            while (!System.IO.File.Exists(htmlpath))
            {
                var newpath = System.IO.Path.GetDirectoryName(path);
                if (path == newpath)
                    throw new Exception("not found.");
                path = newpath;
                htmlpath = System.IO.Path.Combine(path, "viewframework/index.html");
            }
            return htmlpath;
        }
        /// <summary>
        /// 得到输出目录
        /// </summary>
        /// <returns></returns>
        static string GetDestPath()
        {

            var path = System.IO.Path.GetDirectoryName(typeof(HtmlView).Assembly.Location);
            var destpath = System.IO.Path.Combine(path, "tempviewpath");
            return destpath;
        }

        /// <summary>
        /// 复制展示页面到输出目录
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        static string CreateTemp(string src)
        {
            var srcpath = System.IO.Path.GetDirectoryName(src);
            var indexfile = System.IO.Path.GetFileName(src);
            var rootfiles = System.IO.Directory.GetFiles(srcpath, "*.*");

            IEnumerable<string> files = rootfiles;
            var dirs = System.IO.Directory.GetDirectories(srcpath);
            foreach (var dir in dirs)
            {
                var subdir = System.IO.Path.GetRelativePath(srcpath, dir);
                if (subdir.Contains("node_modules") ||
                    subdir.Contains("src")
                    )
                    continue;
                var subfiles = System.IO.Directory.GetFiles(srcpath, subdir + "/*.*");
                files = files.Concat(subfiles);
            }
            var dest = GetDestPath();
            if (System.IO.Directory.Exists(dest))
                System.IO.Directory.Delete(dest, true);
            System.IO.Directory.CreateDirectory(dest);
            foreach (var f in files)
            {
                var file = System.IO.Path.GetRelativePath(srcpath, f);
                var destfile = System.IO.Path.Combine(dest, file);
                var destfilepath = System.IO.Path.GetDirectoryName(destfile);
                if (System.IO.Directory.Exists(destfilepath) == false)
                    System.IO.Directory.CreateDirectory(destfilepath);
                System.IO.File.Copy(f, destfile, true);
            }
            return System.IO.Path.Combine(dest, indexfile);
        }

        /// <summary>
        /// 向展示目录放入文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="txt"></param>
        static void PutTempFile(string file, string txt)
        {
            var outfile = System.IO.Path.Combine(GetDestPath(), file);
            if (System.IO.File.Exists(outfile))
                System.IO.File.Delete(outfile);
            System.IO.File.WriteAllText(outfile, txt);
        }

        //生成数据
        static string GenDataFile(CandlePool pool)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("input_klineDatas = \n");
            var candlejsonarray = new Newtonsoft.Json.Linq.JArray();
            int lastid = pool.GetLastestCandleID(out bool f);
            for (var i = 0; i <= lastid; i++)
            {
                var candle = pool.GetCandleWithIndicator(i);
                var candlejson = new Newtonsoft.Json.Linq.JObject();
                candlejson["timestamp"] = ToJSTime(candle.candle.time);

                candlejson["open"] = candle.candle.open;
                candlejson["high"] = candle.candle.high;
                candlejson["close"] = candle.candle.close;
                candlejson["low"] = candle.candle.low;
                candlejson["volume"] = candle.candle.volume;

                candlejsonarray.Add(candlejson);
            }

            sb.Append(candlejsonarray.ToString(Newtonsoft.Json.Formatting.Indented));


            return sb.ToString();
        }
        static double ToJSTime(DateTime time)
        {
            var tick1 = time.ToUniversalTime();
            var from =new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (tick1 - from).TotalMilliseconds;
        }
    }
}
