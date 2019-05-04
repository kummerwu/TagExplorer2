using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagExplorer2.Model.Config;
using TagExplorer2.Model.FileSystem;

namespace TagExplorer2.Model.Tag
{
    class CmdGuidDB : IDisposable
    {
        public static CmdGuidDB ins = null;
        public static CmdGuidDB Ins
        {
            get
            {
                if (ins == null)
                {
                    ins = new CmdGuidDB();
                }
                return ins;
            }
        }

        public bool Exist(Guid id)
        {
            return id == QuerySqlDB(id);
        }

        SQLiteConnection con = null;
        SQLiteConnection Conn
        {
            get
            {
                if (con == null)
                {
                    string file = CfgUserPath.OptGuidDBPath_SQLite;
                    con = new SQLiteConnection("Data Source=" + file);
                    if (!File.Exists(file))
                    {
                        con.Open();
                        string sql = @"CREATE TABLE [Opts](
                                    [ID] GUID  primary key)
                                    ";
                        SQLiteCommand cmd = new SQLiteCommand(sql, con);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        con.Open();
                    }

                }
                return con;
            }
        }
        SQLiteCommand adduptCmd = null;
        public void Add(Guid id)
        {
            if (adduptCmd == null)
            {
                adduptCmd = new SQLiteCommand(@"Insert INTO Opts (ID) VALUES (@ID)", Conn);
                adduptCmd.Parameters.AddRange(new[]{
                    new SQLiteParameter("@ID", DbType.Guid),
                    });

            }
            adduptCmd.Parameters[0].Value = id;
            adduptCmd.ExecuteNonQuery();

        }

        SQLiteCommand queryCmd = null;
        public Guid QuerySqlDB(Guid id)
        {
            if (queryCmd == null)
            {
                queryCmd = new SQLiteCommand(@"SELECT * FROM Opts where (ID=@ID)", Conn);
                queryCmd.Parameters.AddRange(new[] {
                    new SQLiteParameter("@ID",DbType.Guid),
                    });
            }

            queryCmd.Parameters[0].Value = id;
            using (SQLiteDataReader r = queryCmd.ExecuteReader())
            {
                if (r.Read())
                {
                    Guid ret = r.GetGuid(0);
                    return ret;
                }
                else
                {
                    return Guid.Empty;
                }
            }

        }
        public void Dispose()
        {
            if (adduptCmd != null)
            {
                adduptCmd.Dispose();
                adduptCmd = null;
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
    class CmdHistory
    {
        private static CmdHistory _ins;
        public static CmdHistory Ins
        {
            get
            {
                if (_ins == null)
                {
                    _ins = new CmdHistory();
                    _ins.TagLogFile = CfgPath.TagLogHistory_Path;
                    _ins.UriLogFile = CfgPath.UriLogHistory_Path;
                    _ins.UnkownLogFile = CfgPath.UnknownLogHistory_Path;


                }
                return _ins;
            }
        }
        private string TagLogFile = null;
        private string UriLogFile = null;
        private string UnkownLogFile = null;
        private string T2F(LogType type)
        {
            switch (type)
            {
                case LogType.TagLog: return TagLogFile;
                case LogType.UriLog: return UriLogFile;
                default: return UnkownLogFile;
            }
        }
        public void Save(LogType type, ICmd icmd)
        {
            string cmd = icmd.ToString();
            CmdGuidDB.Ins.Add(icmd.GetOptID());
            string file = T2F(type);
            FileStream fs = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter w = new StreamWriter(fs);
            w.WriteLine(cmd);
            //System.IO.File.AppendAllText(T2F(type), cmd + "\r\n");
            w.Close();
            fs.Close();
        }
    }
    public enum LogType
    {
        TagLog, UriLog
    }
    
}
