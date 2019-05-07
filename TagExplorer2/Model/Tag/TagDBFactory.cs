using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagExplorer2.Utils;

namespace TagExplorer2.Model.Tag
{
    public class TagDBFactory
    {
        private static ITagDB ins = null;
        public static ITagDB Ins
        {
            get
            {
                if (ins == null) ins = CreateTagDB();
                return ins;
            }
        }
        //public static ITagDB CreateTagDB()
        //{
        //    Ins = IDisposableFactory.New<ITagDB>(new LuceneTagDB());
        //    return Ins;
        //}
        public static ITagDB CreateTagDB()
        {
            return IDisposableFactory.New<ITagDB>(new TagDBLogger(SQLTagDB.Load()));
            
        }
        //public static ITagDB CreateTagDB(string t)
        //{
        //    if (t.Contains("json"))
        //    {
        //        Ins = IDisposableFactory.New<ITagDB>(JsonTagDB.Load());
        //    }
        //    else if(t.Contains("sql"))
        //    {
        //        Ins = IDisposableFactory.New<ITagDB>(SQLTagDB.Load());
        //    }
        //    return Ins;
        //}
    }
}
