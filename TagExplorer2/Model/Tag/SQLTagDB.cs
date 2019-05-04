using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagExplorer2.Model.Config;
using TagExplorer2.Model.FileSystem;
using TagExplorer2.Utils;

namespace TagExplorer2.Model.Tag
{
    public class SQLTagDB : IDisposable, ITagDB, ITagCmdRunner
    {

        #region 构造函数和析构函数
        public SQLTagDB()
        {
            if (StaticCfg.Ins.Opt.SqliteTagCacheOn)
            {
                id2TagCache = new Hashtable();
            }
        }
        public static SQLTagDB Load()
        {
            return IDisposableFactory.New<SQLTagDB>(new SQLTagDB());
            //数据库连接惰性打开
        }
        public void Dispose()
        {
            lock (this)
            {
                id2TagCache?.Clear();

                if (adduptCmd != null)
                {
                    adduptCmd.Dispose();
                    adduptCmd = null;
                }
                if (delCmd != null)
                {
                    delCmd.Dispose();
                    delCmd = null;
                }
                if (queryCmd != null)
                {
                    queryCmd.Dispose();
                    queryCmd = null;
                }
                if (con != null)
                {
                    con.Close();
                    con.Dispose();
                    con = null;
                }
                if (SQLiteConnection.ConnectionPool != null)
                {
                    SQLiteConnection.ConnectionPool.ClearAllPools();
                }
                SQLiteConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        #endregion

        #region 变更通知
        DataChanged dbChangedHandler;
        public DataChanged TagDBChanged
        {
            get
            {
                return dbChangedHandler;
            }

            set
            {
                dbChangedHandler += value;
            }
        }

        private bool SuspendChangeNotify = false;
        private void ChangeNotify()
        {
            if (!SuspendChangeNotify)
            {
                dbChangedHandler?.Invoke();
            }
        }
        #endregion
        enum FIELD_IDX
        {
            ID = 0,
            Title,
            Alias,
            PID,
            Children,
            Path,
        }
        #region  SQLite数据库封装
        SQLiteConnection con = null;
        SQLiteConnection Conn
        {
            get
            {
                if (con == null)
                {
                    string file = CfgUserPath.TagDBPath_SQLite;
                    con = new SQLiteConnection("Data Source=" + file);
                    if (!File.Exists(file))
                    {
                        con.Open();
                        string sql = @"CREATE TABLE [Tags](
                                    [ID] GUID  primary key,     
                                    [Title] TEXT,     
                                    [Alias] TEXT,     
                                    [PID] GUID,     
                                    [Children] TEXT,
                                    [Path] TEXT);
                                    ";
                        SQLiteCommand cmd = new SQLiteCommand(sql, con);
                        cmd.ExecuteNonQuery();

                        GUTag defaultTag = new GUTag(StaticCfg.Ins.DefaultTagTitle, StaticCfg.Ins.DefaultTagID);
                        AddUptSqlDB(defaultTag);
                    }
                    else
                    {
                        con.Open();
                    }

                }
                return con;
            }
        }

        //数据库添加
        SQLiteCommand adduptCmd = null;
        private void AddUptSqlDB(GUTag tag)
        {

            if (adduptCmd == null)
            {
                adduptCmd = new SQLiteCommand(@"REPLACE INTO Tags (ID,Title,Alias,PID,Children,Path)
VALUES (@ID,@Title,@Alias,@PID,@Children,@Path)", Conn);
                adduptCmd.Parameters.AddRange(new[]{
                    new SQLiteParameter("@ID", DbType.Guid),
                    new SQLiteParameter("@Title", DbType.String),
                    new SQLiteParameter("@Alias", DbType.String),
                    new SQLiteParameter("@PID", DbType.Guid),
                    new SQLiteParameter("@Children", DbType.String),
                    new SQLiteParameter("@Path", DbType.String),
                    });

            }
            adduptCmd.Parameters[(int)FIELD_IDX.ID].Value = tag.Id;
            adduptCmd.Parameters[(int)FIELD_IDX.Title].Value = tag.Title;
            adduptCmd.Parameters[(int)FIELD_IDX.Alias].Value = tag.AliasString();
            adduptCmd.Parameters[(int)FIELD_IDX.PID].Value = tag.PId;
            adduptCmd.Parameters[(int)FIELD_IDX.Children].Value = tag.ChildrenString();
            adduptCmd.Parameters[(int)FIELD_IDX.Path].Value = tag.Path;
            adduptCmd.ExecuteNonQuery();


        }

        //数据库删除
        SQLiteCommand delCmd = null;
        private void DelSqlDB(GUTag tag)
        {

            if (delCmd == null)
            {
                delCmd = new SQLiteCommand(@"DELETE FROM Tags where (ID = @ID)", Conn);
                delCmd.Parameters.AddRange(new[]{
                    new SQLiteParameter("@ID", DbType.Guid),

                    });

            }

            //根节点不允许删除
            if (tag.Id != StaticCfg.Ins.DefaultTagID)
            {
                delCmd.Parameters[0].Value = tag.Id;
                delCmd.ExecuteNonQuery();
            }
        }

        //数据库查询，不锁
        SQLiteCommand queryCmd = null;
        private GUTag QuerySqlDB(Guid id)
        {
            //lock (this) //这儿导致死锁
            //定时器线程：Import，进入lock，在Import时，会Notify，从而更新UI，Dispatch(UI）线程，等待UI返回。
            //UI线程：更新UI时又会Query，再次进入lock，被阻塞。
            {
                if (queryCmd == null)
                {
                    queryCmd = new SQLiteCommand(@"SELECT * FROM Tags where (ID=@ID)", Conn);
                    queryCmd.Parameters.AddRange(new[] {
                    new SQLiteParameter("@ID",DbType.Guid),
                    });
                }

                queryCmd.Parameters[0].Value = id;
                using (SQLiteDataReader r = queryCmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        GUTag tag = ReadGUTagFromR(r);
                        return tag;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

        }
        //数据库查询，不锁
        SQLiteCommand qTitleCmd = null;
        private List<GUTag> QueryTitleSqlDB(string title)
        {

            if (qTitleCmd == null)
            {
                qTitleCmd = new SQLiteCommand(@"SELECT * FROM Tags where (Title=@Title)", Conn);
                qTitleCmd.Parameters.AddRange(new[] {
                    new SQLiteParameter("@Title",DbType.String),
                    });
            }

            List<GUTag> ret = new List<GUTag>();
            qTitleCmd.Parameters[0].Value = title;
            using (SQLiteDataReader r = qTitleCmd.ExecuteReader())
            {

                while (r.Read())
                {
                    GUTag tag = ReadGUTagFromR(r);
                    ret.Add(tag);
                }
            }
            return ret;
        }
        //数据库查询，不锁
        SQLiteCommand qPathCmd = null;
        public GUTag QueryTagByPath(string path)
        {

            if (qPathCmd == null)
            {
                qPathCmd = new SQLiteCommand(@"SELECT * FROM Tags where (Path=@Path)", Conn);
                qPathCmd.Parameters.AddRange(new[] {
                    new SQLiteParameter("@Path",DbType.String),
                    });
            }

            List<GUTag> ret = new List<GUTag>();
            qPathCmd.Parameters[0].Value = path;
            using (SQLiteDataReader r = qPathCmd.ExecuteReader())
            {

                while (r.Read())
                {
                    GUTag tag = ReadGUTagFromR(r);
                    ret.Add(tag);
                }
            }
            if (ret.Count > 0) return ret[0];
            else
            {
                string tagTitle = CfgPath.GetTagByPath(path);
                List<GUTag> tags = QueryTags(tagTitle);
                foreach (var tag in tags)
                {
                    if (tag.GetDir() == path)
                    {
                        return tag;
                    }
                }
            }
            return null;
        }
        private GUTag ReadGUTagFromR(SQLiteDataReader r)
        {
            GUTag tag = new GUTag();
            //0. ID
            tag.Id = r.GetGuid((int)FIELD_IDX.ID);
            //1. Title
            tag.AddAlias(r.GetString((int)FIELD_IDX.Title));
            //2. Alias
            string alias = r.GetString((int)FIELD_IDX.Alias);
            string[] aliasList = alias.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string a in aliasList)
            {
                tag.AddAlias(a);
            }

            //3. PID
            tag.PId = r.GetGuid((int)FIELD_IDX.PID);
            //4. Children
            string chilrend = r.GetString((int)FIELD_IDX.Children);
            string[] childList = chilrend.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string c in childList)
            {
                tag.AddChild(Guid.Parse(c));
            }
            tag.Path = r.GetString((int)FIELD_IDX.Path);

            return tag;
        }
        SQLiteCommand qWildCmd = null;
        private List<GUTag> QueryWildSql(string title)
        {
            lock (this)
            {
                if (qWildCmd == null)
                {
                    qWildCmd = new SQLiteCommand(@"SELECT * FROM Tags where (Title like @Title or Alias like @Title)", Conn);
                    qWildCmd.Parameters.AddRange(new[] {
                    new SQLiteParameter("@Title",DbType.String),
                    });
                }

                List<GUTag> ret = new List<GUTag>();
                qWildCmd.Parameters[0].Value = '%' + title + '%';
                using (SQLiteDataReader r = qWildCmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        GUTag tag = ReadGUTagFromR(r);
                        ret.Add(tag);
                    }
                }
                System.Diagnostics.Debug.WriteLine("query:" + title + "==>" + ret.Count);
                return ret;
            }
        }

        SQLiteCommand qReplacePath = null;
        private void UpdatePath(string dst, string src)
        {
            if (dst == src) return;

            lock (this)
            {
                if (qReplacePath == null)
                {
                    qReplacePath = new SQLiteCommand(@"update Tags set Path = replace(Path,@SRC,@DST)", Conn);
                    qReplacePath.Parameters.AddRange(new[] {
                    new SQLiteParameter("@SRC",DbType.String),
                    new SQLiteParameter("@DST",DbType.String),
                    });
                }
                qReplacePath.Parameters[0].Value = src;
                qReplacePath.Parameters[1].Value = dst;
                qReplacePath.ExecuteNonQuery();

                return;
            }
        }
        #endregion



        /// </summary>

        //维护所有tag=》taginf（有可能有别名，存在多个tag对应一个tagInf）
        private Hashtable id2TagCache = null;// new Hashtable(); //Guid ==> Gutag


        //private void Save(GUTag tag)
        //{
        //    AddUptSqlDB(tag);
        //    ChangeNotify();
        //}

        private void AssertValid(GUTag tag)
        {
            System.Diagnostics.Debug.Assert(QueryTag(tag.Id) == tag);
        }
        #region 新建Tag
        private GUTag NewTag(GUTag t)
        {
            GUTag tag = new GUTag(t.Title, t.Id);
            SaveAndUpdateCache(tag);
            //ChangeNotify();//这个地方可以不用notify，在设置父子关系的时候再notify
            return tag;
        }
        public GUTag NewTag(string title)
        {
            lock (this)
            {
                GUTag tag = new GUTag(title);
                SaveAndUpdateCache(tag);
                //ChangeNotify();//这个地方可以不用notify，在设置父子关系的时候再notify
                return tag;
            }
        }
        private void SaveAndUpdateCache(GUTag j)
        {
            //Debug.Assert(id2Gutag[j.Id] == null);
            if (id2TagCache != null)
            {
                id2TagCache[j.Id] = j;
            }
            AddUptSqlDB(j);

        }
        #endregion

        #region 删除Tag
        private void RemoveFromHash(GUTag j)
        {
            AssertValid(j);

            id2TagCache?.Remove(j.Id);

            DelSqlDB(j);
            //AllTagSet.Remove(j);
        }
        public int RemoveTag(GUTag tag)
        {
            lock (this)
            {
                tag = QueryTag(tag.Id);
                if (tag == null || tag.Id == StaticCfg.Ins.DefaultTagID) return ITagDBConst.R_OK;

                AssertValid(tag);
                RemoveChild(tag);
                id2TagCache?.Remove(tag.Id);
                DelSqlDB(tag);
                ChangeNotify();
                return ITagDBConst.R_OK;
            }
        }
        #endregion
        //////////////////////////////////////////////////////////

        #region 修改Tag：父子关系
        public int SetParentWithPreBrother(GUTag parent, GUTag child, GUTag preBrother)
        {
            lock (this)
            {
                //添加的tag必须是有效节点
                AssertValid(parent);
                AssertValid(child);
                AssertValid(preBrother);
                GUTag pTag = QueryTag(parent.Id);
                GUTag cTag = QueryTag(child.Id);
                GUTag bTag = QueryTag(preBrother.Id);

                //保护性检查，防止调用无效
                if (pTag != null && cTag != null && bTag != null)
                {
                    pTag.AddChildWithPrebrother(cTag, bTag);
                    System.Diagnostics.Debug.Assert(cTag.PId == pTag.Id);
                    AddUptSqlDB(pTag);
                    AddUptSqlDB(cTag);
                    ChangeNotify();

                    //Save(child);  parent保存实际上已经保存所有了，这儿就不需要保存了。
                }
                return ITagDBConst.R_OK;
            }
        }
        //建立两个Tag之间的父子关系
        public int SetParent(GUTag parent, GUTag child)
        {
            lock (this)
            {
                //添加的tag必须是有效节点
                AssertValid(parent);
                AssertValid(child);
                GUTag pTag = QueryTag(parent.Id);
                GUTag cTag = QueryTag(child.Id);

                //保护性检查，防止调用无效
                if (pTag != null && cTag != null)
                {
                    pTag.AddChild(cTag);
                    System.Diagnostics.Debug.Assert(cTag.PId == pTag.Id);
                    AddUptSqlDB(pTag);
                    AddUptSqlDB(cTag);
                    ChangeNotify();

                    //Save(child);  parent保存实际上已经保存所有了，这儿就不需要保存了。
                }
                return ITagDBConst.R_OK;
            }
        }
        //解除原来child所有parent，并与新的parent建立关系
        public int ResetParent(GUTag parent, GUTag child)
        {
            lock (this)
            {
                parent = QueryTag(parent.Id);
                child = QueryTag(child.Id);
                if (parent == null || child == null) return ITagDBConst.R_OK;
                AssertValid(parent);
                AssertValid(child);
                RemoveChild(child);
                SetParent(parent, child);
                //AddUptSqlDB(parent);
                ChangeNotify();
                return ITagDBConst.R_OK;
            }
        }
        //解除child的父节点 与本child的联系
        private void RemoveChild(GUTag child)
        {
            AssertValid(child);
            GUTag pTag = QueryTag(child.PId);
            if (pTag != null)
            {
                pTag.RemoveChild(child);
                AddUptSqlDB(pTag);
            }
            child.PId = Guid.Empty;



        }

        public int ChangeChildPos(GUTag tag, int direct)
        {
            lock (this)
            {
                tag = QueryTag(tag.Id);
                if (tag == null) return ITagDBConst.R_OK;

                AssertValid(tag);
                List<GUTag> parents = QueryTagParent(tag);
                Debug.Assert(parents.Count == 1);
                if (parents.Count == 1)
                {
                    GUTag parent = parents[0];
                    parent.ChangePos(tag, direct);
                    AddUptSqlDB(parent);
                    AddUptSqlDB(tag);
                    ChangeNotify();
                }
                return ITagDBConst.R_OK;
            }

        }
        #endregion


        #region 修改Tag：标题和别名

        public int MergeAlias(GUTag mainTag, GUTag aliasTag)
        {
            lock (this)
            {
                AssertValid(mainTag);
                AssertValid(aliasTag);
                mainTag = QueryTag(mainTag.Id);
                aliasTag = QueryTag(aliasTag.Id);
                RemoveFromHash(aliasTag);
                mainTag.Merge(aliasTag);
                SaveAndUpdateCache(mainTag);
                //allTag.Add(tag2, tmp1);//别名也需要快速索引
                ChangeNotify();
                return ITagDBConst.R_OK;
            }
        }
        public GUTag ChangeTitle(GUTag tag, string newTitle)
        {
            lock (this)
            {
                tag = QueryTag(tag.Id);
                if (tag == null) return null;

                AssertValid(tag);
                tag.ChangeTitle(newTitle);
                AddUptSqlDB(tag);
                ChangeNotify();
                return tag;
            }
        }
        public int ReplacePath(string dst, string src)
        {
            lock (this)
            {
                UpdatePath(dst, src);
                ClearCache();
                ChangeNotify();
                return 0;
            }
        }
        public int SetPath(GUTag tag, string path)
        {
            lock (this)
            {
                tag = QueryTag(tag.Id);
                if (tag == null) return 0;

                AssertValid(tag);
                tag.ChangePath(path);
                AddUptSqlDB(tag);
                ChangeNotify();
                return 0;
            }
        }

        #endregion

        #region  查询函数实现
        public GUTag GetTag(Guid id)
        {
            lock (this)
            {
                return QueryTag(id);
            }
        }
        private void ClearCache()
        {
            id2TagCache?.Clear();
        }
        private GUTag QueryTag(Guid id)
        {
            if (id2TagCache != null)
            {
                GUTag tmp = id2TagCache[id] as GUTag;
                if (tmp == null)
                {
                    tmp = QuerySqlDB(id);
                    if (tmp != null)
                    {
                        id2TagCache[id] = tmp;
                    }
                }
                return tmp;
            }
            else
            {
                return QuerySqlDB(id);
            }
        }
        public int QueryChildrenCount(GUTag tag)
        {
            lock (this)
            {
                //AssertValid(tag);
                GUTag tmp = QueryTag(tag.Id);
                return tmp == null ? 0 : tmp.Children.Count;
            }
        }

        private string ParentHistory(GUTag a)
        {
            a = QueryTag(a.Id);
            string ret = a.Title;

            while (a != null)
            {
                var parents = QueryTagParent(a);
                if (parents.Count == 0) break;
                else
                {
                    a = parents[0];
                    ret = ret + ">" + a.Title;
                }
            }
            return ret;
        }

        //待优化，在SQLIte中怎样自动补充

        public List<AutoCompleteTipsItem> QueryAutoComplete(string searchTerm, bool forceOne = false)
        {
            string ls = searchTerm.ToLower();
            List<AutoCompleteTipsItem> ret = new List<AutoCompleteTipsItem>();
            List<GUTag> tags = QueryWildSql(ls);
            foreach (GUTag s in tags)
            {
                if (s.Title.ToLower().Contains(ls))
                {
                    AutoCompleteTipsItem a = new AutoCompleteTipsItem();
                    a.Tag = s;
                    a.Tip = ParentHistory(s);


                    //完全匹配，奖励1000分
                    if (searchTerm == s.Title)
                    {
                        a.Score += 10000;
                    }
                    //惩罚：长度差越大，惩罚越多
                    a.Score -= Math.Abs(a.Tag.Title.Length - searchTerm.Length);
                    //惩罚：路径越长，惩罚越多
                    a.Score -= (a.Tip.Length) * 10;
                    ret.Add(a);
                }
            }
            ret.Sort((x, y) => y.Score.CompareTo(x.Score));//Score越大越好

            //如果没有找到对应Tag，而且需要保证非空时，返回一个非空的内容
            if (forceOne && ret.Count == 0)
            {
                AutoCompleteTipsItem a = new AutoCompleteTipsItem();
                a.Tag = null;
                a.Tip = searchTerm;
                //a.Data = GUTag.Parse(StaticCfg.Ins.DefaultTagID.ToString(), this);
                ret.Add(a);
            }
            return ret;
        }

        public List<string> QueryTagAlias(GUTag tag)
        {
            //AssertValid(tag);
            tag = QueryTag(tag.Id);
            if (tag == null) return new List<string>();

            else return tag.Alias;
        }

        public List<GUTag> QueryTagChildren(GUTag tag)
        {
            //AssertValid(tag);
            tag = QueryTag(tag.Id);
            if (tag == null) return new List<GUTag>();

            List<GUTag> gutagChildren = new List<GUTag>();
            foreach (Guid id in tag.Children)
            {
                GUTag c = QueryTag(id);
                if (c != null)
                {
                    gutagChildren.Add(c);
                }
            }
            return gutagChildren;
        }

        public List<GUTag> QueryTagParent(GUTag tag)
        {
            //AssertValid(tag); 由于有两个视图，可能会用一个已经失效的GUTag进行查询。
            tag = QueryTag(tag.Id);
            if (null == tag) return new List<GUTag>();
            if (tag.Id == StaticCfg.Ins.DefaultTagID) return new List<GUTag>();


            List<GUTag> ret = new List<GUTag>();
            //如果有ParentID，直接返回
            if (tag.PId != null)
            {
                GUTag ptag = QueryTag(tag.PId);
                ret.Add(ptag);

            }

            return ret;

        }
        public List<GUTag> QueryTags(string title)
        {
            List<GUTag> ret = QueryTitleSqlDB(title);
            return ret;
        }
        #endregion

        #region 从老的json数据导出数据库
        //分时读入，一次性读入会造成界面没有响应
        public ImportResult Import(string importInf)
        {
            DateTime start = DateTime.Now;
            long offset = 0;
            try
            {
                if (!File.Exists(importInf))
                {
                    TipsCenter.Ins.TagImportInf = "SqlTagDB Import Finished! == FileNotExist";
                    return ImportResult.Finished;
                }

                string tmp = File.ReadAllText(CfgUserPath.Offset_SQLite);

                long.TryParse(tmp, out offset);
                System.IO.FileInfo fi = new System.IO.FileInfo(importInf);
                //如果文件已经读到尾部，直接返回
                if (offset >= fi.Length - 2)
                {
                    TipsCenter.Ins.TagImportInf = "SqlTagDB Import Finished!";
                    return ImportResult.Finished;
                }
                TipsCenter.Ins.TagImportInf = string.Format("SqlTagDB Import:{0}/{1}", offset, fi.Length);

                FileStream fs = new FileStream(importInf, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader r = new StreamReader(fs);
                r.SetPosition(offset);
                //r.BaseStream.Seek(offset, SeekOrigin.Begin);
                //for (int i = 0; i < 10; i++)
                while ((DateTime.Now - start).TotalMilliseconds < 1000)
                {
                    string ln = r.ReadLine();
                    if (ln != null && ln.Trim().Length > 0 &&
                        ln.Trim()[0] == '{' && ln.Trim().EndsWith("}")) //是一个json
                    {
                        TagDBCmd cmd = JsonConvert.DeserializeObject<TagDBCmd>(ln);
                        if (cmd != null)
                        {
                            Run(cmd);
                            offset = r.GetPosition();
                            int next = r.Peek();
                            if (next != '{' && next != ' ' && next != -1)//下一个cmd应该是{开头
                            {
                                //TODO ,如何自动恢复？
                                System.Diagnostics.Debug.Assert(false);
                            }
                            r.SetPosition(offset);
                            next = r.Peek();
                            if (next != '{' && next != ' ' && next != -1)//下一个cmd应该是{开头
                            {
                                //TODO ,如何自动恢复？
                                System.Diagnostics.Debug.Assert(false);
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                r.Close();
                fs.Close();
            }
            catch (Exception ee)
            {
                Logger.E(ee);
            }
            File.WriteAllText(CfgUserPath.Offset_SQLite, offset.ToString());
            ChangeNotify();
            //if (!File.Exists(importInf)) return 0;

            //int newCnt = 0, uptCnt = 0;
            //Hashtable title2GUtag = new Hashtable();
            //string[] lns = File.ReadAllLines(importInf);
            //List<GUTag> oldGUTags = new List<GUTag>();
            //foreach (string ln in lns)
            //{
            //    GUTag iTag = JsonConvert.DeserializeObject<GUTag>(ln);
            //    if (iTag != null)
            //    {
            //        GUTag oTag = QueryTag(iTag.Id);
            //        if(oTag==null)
            //        {
            //            SaveAndUpdateCache(iTag);
            //            newCnt++;
            //        }
            //        else if(!iTag.IsSame(oTag))
            //        {
            //            SaveAndUpdateCache(GUTag.MergeTag(iTag, oTag));
            //            uptCnt++;
            //        }
            //        else
            //        {
            //            //两边完全相同，不用处理
            //        }
            //    }

            //}
            //ChangeNotify();
            return ImportResult.NotFinished;
        }

        public int Export(string exportFile)
        {

            using (StreamWriter w = new StreamWriter(exportFile))
            {
                List<GUTag> all = new List<GUTag>();
                SQLiteCommand q = new SQLiteCommand(@"SELECT * FROM Tags", Conn);
                using (SQLiteDataReader r = q.ExecuteReader())
                {
                    while (r.Read())
                    {
                        GUTag tag = ReadGUTagFromR(r);
                        if (tag != null)
                        {
                            all.Add(tag);
                        }
                    }

                }

                all.Sort((x, y) => x.Id.CompareTo(y.Id));
                foreach (GUTag j in all)
                {
                    w.WriteLine(JsonConvert.SerializeObject(j));
                }

            }
            return 0;
        }

        public int Run(TagDBCmd cmd)
        {
            if (CmdGuidDB.Ins.Exist(cmd.GetOptID())) return 0;
            lock (this)
            {
                SuspendChangeNotify = true;
                try
                {
                    switch (cmd.Type)
                    {
                        case TagDBCmdType.NewTag:
                            if (cmd.Args.Count == 1)
                            {
                                NewTag(GUTag.Parse(cmd.Args[0]));
                            }
                            break;
                        case TagDBCmdType.RemoveTag:
                            if (cmd.Args.Count == 1)
                            {
                                RemoveTag(GUTag.Parse(cmd.Args[0]));
                            }
                            break;
                        case TagDBCmdType.SetParent:
                            if (cmd.Args.Count == 2)
                            {
                                SetParent(GUTag.Parse(cmd.Args[0]), GUTag.Parse(cmd.Args[1]));
                            }
                            break;
                        case TagDBCmdType.ResetParent:
                            if (cmd.Args.Count == 2)
                            {
                                ResetParent(GUTag.Parse(cmd.Args[0]), GUTag.Parse(cmd.Args[1]));
                            }
                            break;
                        case TagDBCmdType.ChangeTitle:
                            if (cmd.Args.Count == 2)
                            {
                                ChangeTitle(GUTag.Parse(cmd.Args[0]), cmd.Args[1]);
                            }
                            break;
                        case TagDBCmdType.MergeAlias:
                            if (cmd.Args.Count == 2)
                            {
                                ResetParent(GUTag.Parse(cmd.Args[0]), GUTag.Parse(cmd.Args[1]));
                            }
                            break;
                        case TagDBCmdType.ChangeChildPos:
                            if (cmd.Args.Count == 2)
                            {
                                ChangeChildPos(GUTag.Parse(cmd.Args[0]), int.Parse(cmd.Args[1]));
                            }
                            break;
                        case TagDBCmdType.SetParentWithPreBrother:
                            if (cmd.Args.Count == 3)
                            {
                                SetParentWithPreBrother(GUTag.Parse(cmd.Args[0]), GUTag.Parse(cmd.Args[1]), GUTag.Parse(cmd.Args[2]));
                            }
                            break;
                        case TagDBCmdType.SetPath:
                            if (cmd.Args.Count == 2)
                            {
                                SetPath(GUTag.Parse(cmd.Args[0]), cmd.Args[1]);
                            }
                            break;
                        case TagDBCmdType.ReplacePath:
                            if (cmd.Args.Count == 2)
                            {
                                ReplacePath(cmd.Args[0], cmd.Args[1]);
                            }
                            break;
                    }
                }
                finally
                {
                    SuspendChangeNotify = false;
                }

                return 0;
            }
        }
        #endregion
    }
}
