using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagExplorer2.Model.Tag
{
    class TagDBLogger : ITagDB, IDisposable
    {
        ITagDB DB = null;

        public TagDBLogger(ITagDB db)
        {
            DB = db;
        }
        private void SaveCmd(TagDBCmd cmd)
        {
            CmdHistory.Ins.Save(LogType.TagLog, cmd);
        }
        public DataChanged TagDBChanged { get => DB.TagDBChanged; set => DB.TagDBChanged = value; }

        public int ChangeChildPos(GUTag tag, int direct)
        {
            TagDBCmd cmd = new TagDBCmd
            {
                Type = TagDBCmdType.ChangeChildPos,
                Args = new List<string>() { tag.ToString(), direct.ToString() },
            };
            SaveCmd(cmd);
            return DB.ChangeChildPos(tag, direct);
        }

        public GUTag ChangeTitle(GUTag tag, string newTitle)
        {
            TagDBCmd cmd = new TagDBCmd
            {
                Type = TagDBCmdType.ChangeTitle,
                Args = new List<string>() { tag.ToString(), newTitle }
            };
            SaveCmd(cmd);
            return DB.ChangeTitle(tag, newTitle);
        }

        public void Dispose()
        {
            DB.Dispose();
            //TODO,关闭自身资源
        }

        public int Export(string exportFile)
        {
            return DB.Export(exportFile);
        }

        public GUTag GetTag(Guid id)
        {
            return DB.GetTag(id);
        }

        public ImportResult Import(string importFile)
        {

            return DB.Import(importFile);
        }

        public int MergeAlias(GUTag tag1, GUTag tag2)
        {
            TagDBCmd cmd = new TagDBCmd
            {
                Type = TagDBCmdType.MergeAlias,
                Args = new List<string>() { tag1.ToString(), tag2.ToString() }
            };
            SaveCmd(cmd);
            return DB.MergeAlias(tag1, tag2);
        }

        public GUTag NewTag(string title)
        {
            GUTag newTag = DB.NewTag(title);
            TagDBCmd cmd = new TagDBCmd
            {
                Type = TagDBCmdType.NewTag,
                Args = new List<string>() { newTag.ToString() }
            };
            SaveCmd(cmd);
            return newTag;
        }
        public int RemoveTag(GUTag tag)
        {
            TagDBCmd cmd = new TagDBCmd
            {
                Type = TagDBCmdType.RemoveTag,
                Args = new List<string>() { tag.ToString() }
            };
            SaveCmd(cmd);
            return DB.RemoveTag(tag);
        }

        public int ResetParent(GUTag parent, GUTag child)
        {
            TagDBCmd cmd = new TagDBCmd
            {
                Type = TagDBCmdType.ResetParent,
                Args = new List<string>() { parent.ToString(), child.ToString() }
            };
            SaveCmd(cmd);
            return DB.ResetParent(parent, child);
        }
        public int SetParentWithPreBrother(GUTag parent, GUTag child, GUTag preBrother)
        {
            TagDBCmd cmd = new TagDBCmd
            {
                Type = TagDBCmdType.SetParentWithPreBrother,
                Args = new List<string>() { parent.ToString(), child.ToString(), preBrother.ToString() }
            };
            SaveCmd(cmd);
            return DB.SetParentWithPreBrother(parent, child, preBrother);
        }
        public int SetParent(GUTag parent, GUTag child)
        {
            TagDBCmd cmd = new TagDBCmd
            {
                Type = TagDBCmdType.SetParent,
                Args = new List<string>() { parent.ToString(), child.ToString() }
            };
            SaveCmd(cmd);
            return DB.SetParent(parent, child);
        }
        public int ReplacePath(string dst, string src)
        {
            TagDBCmd cmd = new TagDBCmd
            {
                Type = TagDBCmdType.ReplacePath,
                Args = new List<string>() { dst, src }
            };
            SaveCmd(cmd);
            return DB.ReplacePath(dst, src);
        }
        public int SetPath(GUTag tag, string path)
        {
            TagDBCmd cmd = new TagDBCmd
            {
                Type = TagDBCmdType.SetPath,
                Args = new List<string>() { tag.ToString(), path }
            };
            SaveCmd(cmd);
            return DB.SetPath(tag, path);
        }
        public List<AutoCompleteTipsItem> QueryAutoComplete(string searchTerm, bool forceOne = false)
        {
            return DB.QueryAutoComplete(searchTerm, forceOne);
        }

        public int QueryChildrenCount(GUTag tag)
        {
            return DB.QueryChildrenCount(tag);
        }
        public GUTag QueryTagByPath(string path)
        {
            return DB.QueryTagByPath(path);
        }
        public List<string> QueryTagAlias(GUTag tag)
        {
            return DB.QueryTagAlias(tag);
        }

        public List<GUTag> QueryTagChildren(GUTag tag)
        {
            return DB.QueryTagChildren(tag);
        }

        public List<GUTag> QueryTagParent(GUTag tag)
        {
            return DB.QueryTagParent(tag);
        }

        public List<GUTag> QueryTags(string title)
        {
            return DB.QueryTags(title);
        }


    }
}
