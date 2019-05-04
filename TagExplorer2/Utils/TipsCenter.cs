using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagExplorer2.Utils
{
    public class TipsCenter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        //最多有60个Inf供使用
        public string TagInf { set { SetInf(value, 0); } }
        public string UriInf { set { SetInf(value, 1); } }
        public string MainInf { set { SetInf(value, 2); } }
        public string ListInf { set { SetInf(value, 3); } }
        public string TagDBInf { set { SetInf(value, 4); } }
        public string UriDBInf { set { SetInf(value, 5); } }
        public string BackTaskInf { set { SetInf(value, 6); } }
        //public string StartTime  7
        public string UriImportInf { set { SetInf(value, 8); } }
        public string TagImportInf { set { SetInf(value, 9); } }
        public string DbgInf { set { SetInf(value, 10); } }

        private DateTime lastTime = DateTime.MinValue;
        private string lastInf = null;
        public void ClearStartTime() { lastInf = null; infs[7] = null; }
        public string StartTime
        {
            set
            {
                lock (this)
                {
                    DateTime now = DateTime.Now;
                    string t = now.ToString("mm:ss.fff");
                    string inf = t;// value + DateTime.Now.ToString("===mm:ss.fff  ##");
                    if (lastInf == null)
                    {
                        inf += (" 0000ms     " + value + "@START@");
                    }
                    else
                    {
                        inf += " " + ((int)((DateTime.Now - lastTime).TotalMilliseconds)).ToString("0000") + "ms     " + lastInf + " <->  " + value;
                    }
                    inf += "\r\n";
                    lastTime = now;
                    lastInf = value;

                    SetInf((infs[7] == null ? "" : infs[7]) + inf, 7);
                    StartProgress = value;
                }
            }
        }
        public static TipsCenter Ins
        {
            get
            {
                if (_ins == null) _ins = new TipsCenter();
                return _ins;
            }
        }

        string startProgress = "";
        public string StartProgress
        {
            get
            {
                return startProgress;
            }
            set
            {
                startProgress = value;
                //触发事件
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("StartProgress"));
                }
            }
        }
        public string Tips
        {
            get { return tips; }
            set
            {
                tips = value;
                //触发事件
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Tips"));
                }
            }
        }




        private string tips;
        private static TipsCenter _ins;
        private string[] infs = new string[60];
        private string MergeInf
        {
            get
            {
                string all = "";
                for (int i = 0; i < infs.Length; i++)
                {
                    if (!string.IsNullOrEmpty(infs[i]))
                    {
                        all += infs[i] + "\r\n";
                    }
                }
                return all;
            }
        }
        private void SetInf(string v, int idx) { infs[idx] = v; Tips = MergeInf; }

    }
}
