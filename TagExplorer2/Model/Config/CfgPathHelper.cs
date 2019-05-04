using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TagExplorer2.Model.Config
{
    //这段代码是从TagExplorer中拷贝过来的，保证两边一致。
    //当然，最好的方法是提出一个公共项目，防止两个公用的东西，但现在暂时没有时间处理。
    public class CfgPathHelper
    {
        public static string SubDir(string parent, string name)
        {
            string subDir = Path.Combine(parent, name);
            if (!Directory.Exists(subDir))
            {
                Directory.CreateDirectory(subDir);
            }
            return subDir;
        }


        static Nullable<bool> isDbg = null;
        public static bool IsDbg
        {
            get
            {
                if (isDbg != null) return isDbg.Value;
                isDbg = File.Exists(@"B:\dbg");
                if (isDbg.Value)
                {
                    if (MessageBox.Show("以调试模式启动？", "启动方式", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    {
                        isDbg = false;
                    }

                }
                return isDbg.Value;
            }
        }
    }
}
