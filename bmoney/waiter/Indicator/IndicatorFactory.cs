using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMoney.Indicator
{
    class InstInfo
    {
        public Type type;
        public string[] InitParamDefine;
    }
    public static class IndicatorFactory
    {
        static Dictionary<string, InstInfo> indtypes = new Dictionary<string, InstInfo>();
        static IndicatorFactory()
        {
            //所有继承自 IIndicator 的对象自动注册
            var asm = typeof(IndicatorFactory).Assembly;
            var types = asm.GetTypes();
            foreach (var t in types)
            {

                if (t.GetInterface("IIndicator") != null)
                {
                    //创建一个临时实例只是为了取得Name 和 InitParamDefine
                    var inst = System.Activator.CreateInstance(t) as IIndicator;

                    indtypes[inst.Name] = new InstInfo
                    {
                        type = t,
                        InitParamDefine = inst.GetInitParamDefine()
                    };
                }

            }
        }
        public static string[] GetInitParamDefine(string name)
        {
            return indtypes[name].InitParamDefine.Clone() as string[];
        }
        public static IIndicator Create(string name, string[] _params)
        {
            if (indtypes.ContainsKey(name) == false)
                return null;
            var inst = System.Activator.CreateInstance(indtypes[name].type) as IIndicator;
            inst.Init(_params);
            return inst;
        }
    }
}
