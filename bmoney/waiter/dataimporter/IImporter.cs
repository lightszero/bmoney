using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney
{
    public interface IImporter
    {
        //向一个pool里开始推
        void Start(CandlePool pool);
        public TimeSpan Tick
        {
            get;
        }
        bool IsActive
        {
            get;
        }
        bool IsAsync
        {
            get;
        }
        //停止推
        void Stop();
        // DateTimeOffset
    }

}
