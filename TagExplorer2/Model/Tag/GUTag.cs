using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagExplorer2.Model.Config;
using TagExplorer2.Model.FileSystem;
using TagExplorer2.Utils;

namespace TagExplorer2.Model.Tag
{
    [Serializable]
    public class GUTag
    {
        #region 构造函数和简单数据
        //自身信息：ID，Title，别名（其中Title在实现上就是Alias[0]
        public Guid Id;
        //父节点信息
        public Guid PId;
        public string path = "";
        public string Path
        {
            get
            {
                if (path == "" && Id == StaticCfg.Ins.DefaultTagID)
                {
                    InitDir(null);
                }
                return path;
            }
            set
            {
                path = value;
            }
        }
        public GUTag() { }
        public GUTag(string title)
        {
            if (title == StaticCfg.Ins.DefaultTagTitle)
            {

                Id = StaticCfg.Ins.DefaultTagID;

            }
            else Id = Guid.NewGuid();
            Alias.Add(title);
            if (Id == StaticCfg.Ins.DefaultTagID)
            {
                InitDir(null);
            }
        }
        public GUTag(string title, Guid id)
        {
            Id = id;
            Alias.Add(title);
            if (Id == StaticCfg.Ins.DefaultTagID)
            {
                InitDir(null);
            }
        }
        #endregion

        #region 别名和标题
        public List<string> Alias = new List<string>();
        [JsonIgnore]
        public string Title { get { return Alias.Count > 0 ? Alias[0] : ""; } }

        public string AliasString()
        {
            string ret = "";
            for (int i = 1; i < Alias.Count; i++)
            {
                ret += Alias[i] + "\n";
            }
            return ret.Trim();
        }

        //添加一个别名
        public void AddAlias(string title)
        {
            if (!Alias.Contains(title))
            {
                Alias.Add(title);
            }
        }
        //修改Title
        public void ChangeTitle(string title)
        {
            if (title == Title) return;
            if (Alias.Count > 0) Alias.RemoveAt(0);
            if (Alias.Contains(title))
            {
                Alias.Remove(title);
            }
            Alias.Insert(0, title);
            ChangePathByTitle();
        }
        public string CalcNewDir(GUTag parent)
        {
            string pdir = parent.GetDir(false);
            return System.IO.Path.Combine(pdir, Title);
        }
        public void ResetDir(GUTag parent)
        {
            string pdir = parent.GetDir(false);
            Path = System.IO.Path.Combine(pdir, Title);

        }
        public void InitDir(GUTag parent)
        {
            if (string.IsNullOrEmpty(path))
            {
                if (Id == StaticCfg.Ins.DefaultTagID)
                {
                    path = CfgPath.DefaultTagDir;
                }
                else
                {
                    string pdir = parent.GetDir(false);
                    path = System.IO.Path.Combine(pdir, Title);
                }
            }


        }
        //修改Path
        public void ChangePathByTitle()
        {
            if (string.IsNullOrWhiteSpace(Path)) return;
            if (!Directory.Exists(Path) || System.IO.Path.GetFileName(Path) == StaticCfg.Ins.DefaultNewTag)
            {
                string p = Directory.GetParent(Path).FullName;
                Path = System.IO.Path.Combine(p, Title);

            }
        }
        public void ChangePath(string path)
        {
            if (path == Path) return;

            Path = path;
        }
        #endregion

        #region 子节点列表管理
        //子节点信息
        public List<Guid> Children = new List<Guid>();
        public string ChildrenString()
        {
            string ret = "";
            for (int i = 0; i < Children.Count; i++)
            {
                ret += Children[i] + "\n";
            }
            return ret.Trim();
        }




        //修改Child节点的位置（direct=-1：下移一个，1：上移一个）
        public void ChangePos(GUTag child, int direct)
        {
            int idx = Children.IndexOf(child.Id);
            int newIdx = idx + direct;
            if (newIdx >= 0 && newIdx < Children.Count)
            {
                Children.RemoveAt(idx);
                Children.Insert(newIdx, child.Id);
            }
        }

        public int GetChildPos(GUTag child)
        {
            return Children.IndexOf(child.Id);
        }
        public void AddChildWithPrebrother(GUTag child, GUTag brother)
        {
            if (Children.Contains(child.Id)) return;

            int idx = Children.FindIndex(id => id == brother.Id);
            if (idx >= 0)
            {
                Children.Insert(idx + 1, child.Id);
            }
            else
            {
                Children.Add(child.Id);
            }
            child.PId = Id;
        }
        public void AddChild(GUTag c)
        {
            AddChild(c.Id);
            c.PId = Id;
        }
        public void AddChild(Guid cid)
        {
            if (Children.Contains(cid)) return;
            Children.Add(cid);
        }


        public void RemoveChild(GUTag c)
        {
            if (Children.Contains(c.Id)) Children.Remove(c.Id);
        }

        public void Merge(GUTag other)
        {
            foreach (string a in other.Alias) AddAlias(a);
            foreach (Guid c in other.Children) AddChild(c);
            //foreach (string p in tag.Parents) AddParent(p);
        }
        #endregion

        #region 序列化和反序列化
        const char SplitToken = '→';
        public override string ToString()
        {
            return Id.ToString() + SplitToken + Title;
        }
        public static GUTag Parse(string strGutag)
        {
            Guid id = Guid.Empty;

            if (strGutag.IndexOf(SplitToken) != -1)
            {
                string[] tokens = strGutag.Split(SplitToken);
                if (tokens.Length == 2)
                {
                    string sID = tokens[0];
                    string title = tokens[1];
                    id = Guid.Parse(sID);
                    return new GUTag(title, id);
                }


            }
            return null;
        }
        public static GUTag Parse(string strGutag, ITagDB db)
        {
            Guid id = Guid.Empty;

            if (strGutag.IndexOf(SplitToken) != -1)
            {
                string sID = strGutag.Split(SplitToken)[0];
                id = Guid.Parse(sID);
            }
            return db.GetTag(id);
        }
        #endregion

        #region 比较函数
        public override bool Equals(object obj)
        {
            return this == obj as GUTag;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(GUTag l, GUTag r)
        {
            //需要强制转换，否则==会递归死循环。
            if (null == (object)l && null == (object)r) return true;
            else if (null != (object)l && null != (object)r) return l.Id == r.Id;
            else return false;
        }
        public static bool operator !=(GUTag l, GUTag r)
        {
            return !(l == r);
        }
        #endregion

        #region 归并函数

        public bool IsSame(GUTag other)
        {
            if (other.Id != Id) return false;
            if (other.PId != PId) return false;
            if (other.Alias.Count != Alias.Count) return false;
            if (other.Children.Count != Children.Count) return false;
            for (int i = 0; i < Alias.Count; i++)
            {
                if (other.Alias[i] != Alias[i]) return false;
            }
            for (int i = 0; i < Children.Count; i++)
            {
                if (other.Children[i] != Children[i]) return false;
            }
            return true;
        }
        public static GUTag MergeTag(GUTag iTag, GUTag oTag)
        {
            GUTag nTag = new GUTag();
            //ID
            nTag.Id = iTag.Id;
            //PID
            nTag.PId = iTag.PId;
            //Alias
            nTag.Alias.AddRange(iTag.Alias);
            foreach (string s in iTag.Alias)
            {
                nTag.AddAlias(s);
            }
            //Children
            nTag.Children.AddRange(iTag.Children);
            foreach (Guid s in iTag.Children)
            {
                nTag.AddChild(s);
            }
            return nTag;
        }
        #endregion

        #region 辅助函数
        public void CopyTagFullPathWithDateToClipboard()
        {
            string dir = GetDir(true);//拷贝目录名，需要保证路径存在
            dir = System.IO.Path.Combine(dir, DateTime.Now.ToString("yyyyMMdd") + "-");
            ClipBoardSafe.SetText(dir);
        }
        public void CopyTagFullPathToClipboard()
        {
            string dir = this.GetDir(true);//拷贝目录名，需要保证路径存在
            ClipBoardSafe.SetText(dir);
        }

        public string GetDir(bool create = false)
        {
            if (!string.IsNullOrWhiteSpace(Path))
            {
                if (create && !Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
                return Path;
            }
            else
            {
                return Path;

            }
        }
        public void OpenExplorer()
        {
            FileShell.OpenExplorer(GetDir(true));
        }

        public string GetFileByTagFileName(string fileName, bool createDir = false)
        {
            string dir = GetDir(createDir);//不要再这儿创建目录
            return System.IO.Path.Combine(dir, fileName);//TODO 待定
        }

        public string GetVDir()
        {
            return System.IO.Path.Combine(CfgUserPath.VDir, Title);
        }
        #endregion

    }
}
