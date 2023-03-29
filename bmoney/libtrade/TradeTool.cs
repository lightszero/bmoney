using Newtonsoft.Json.Linq;
using System.Net;

namespace libtrade
{
    public class TradeTool
    {
        public string IP
        {
            get;
            private set;
        }
        public async void Init()
        {
            WebClient wc = new WebClient();
            var str = await wc.DownloadStringTaskAsync("http://ipinfo.io");
            IP = JObject.Parse(str)["ip"].ToString();
        }
    }
}