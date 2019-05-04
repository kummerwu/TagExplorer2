using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using TagExplorer2.Utils;

namespace TagExplorer2.Model.Config
{
    class CfgUserPath
    {
        private static string RootSubDir(string name)
        {
            return CfgPathHelper.SubDir(UserRoot, name);
        }
        public static string VDir { get { return RootSubDir("VirtualDir"); } }

        //Root/Config/
        public static string ConfigPath { get { return RootSubDir("Config"); } }
        //Root/Config/ .ini  .xml  .json


        public static string StaticCfg { get { return Path.Combine(ConfigPath, "StaticCfg.json"); } }
        public static string DynamicCfg { get { return Path.Combine(ConfigPath, "DynamicCfg.json"); } }
        public static string Log4Cfg
        {
            get
            {
                string f = Path.Combine(ConfigPath, "Log4Cfg.xml");
                if (!File.Exists(f))
                {
                    var rm = new ResourceManager("TagExplorer.Properties.Resources", Assembly.GetExecutingAssembly());
                    string defaultCfg = rm.GetString("DefaultLog4Cfg");
                    if (defaultCfg != null)
                    {
                        defaultCfg = defaultCfg.Replace("{{LOG_FILE}}", LogFile);
                        System.IO.File.WriteAllText(f, defaultCfg);
                    }
                }
                return f;
            }
        }

        //User/Log/  TagExplorer.log
        public static string LogPath { get { return RootSubDir("Log"); } }
        public static string LogFile { get { return Path.Combine(LogPath, "TagExplorer.log"); } }

        public static string IniFilePath { get { return Path.Combine(ConfigPath, "TagExplorer.ini"); } }

        public static string LayoutCfgFilePath(int state) { return Path.Combine(ConfigPath, "TagExplorerLayout" + state.ToString() + ".xml"); }


        //User/Index

        private static string userRoot = null;
        public static string UserRoot
        {
            get
            {
                if (userRoot == null)
                {

                    userRoot = ConfigurationManager.AppSettings["UserDir"];
                    if (CfgPathHelper.IsDbg)
                    {
                        userRoot = @"B:\UserData";
                    }
                    if (userRoot == null)
                    {
                        //没有配置，采用默认值:可执行文件所在路径的父节点下的/UserData
                        System.IO.FileInfo fi = new FileInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                        string parent = fi.Directory.Parent.FullName;
                        userRoot = CfgPathHelper.SubDir(parent, "UserData");
                    }
                    if (userRoot != null && !Directory.Exists(userRoot))
                    {
                        Directory.CreateDirectory(userRoot);
                    }

                }
                return userRoot;
            }

        }
        //public static string IndexPath { get { return SubDir(RootPath, "Index"); } }
        public static string IndexPath { get { return CfgPathHelper.SubDir(UserRoot, "Index"); } }
        //User/Index/TagDB == 废弃 Lucence
        public static string TagDBPath { get { return Path.Combine(IndexPath, "TagDB"); } }
        //User/Index/TagDBJson/Tags3.json ==废弃 Json
        public static string TagDBPath_Json
        {
            get
            {
                string tdbDir = CfgPathHelper.SubDir(IndexPath, "TagDBJson");
                return Path.Combine(tdbDir, "Tags3.json");
            }
        }
        //User/Index/TagDBSQL/sqlite.db         offset.rec
        public static string TagDBSQL_Path { get { return CfgPathHelper.SubDir(IndexPath, "TagDBSQL"); } }
        public static string TagDBPath_SQLite { get { return Path.Combine(TagDBSQL_Path, "sqlite.db"); } }
        public static string OptGuidDBPath_SQLite { get { return Path.Combine(IndexPath, "optid_sqlite.db"); } }
        public static string Offset_SQLite
        {
            get
            {
                string file = Path.Combine(TagDBSQL_Path, "offset.rec");
                if (!File.Exists(file))
                {
                    //File.Create(file).Close();
                    FileShell.CreateFile(file);
                    File.WriteAllText(file, "0");
                }
                return file;
            }
        }
        //Root/Index/UriDB  Lucence自动生成一批文件
        public static string UriDBPath { get { return Path.Combine(IndexPath, "UriDB"); } }
        public static string Offset_UriDB
        {
            get
            {
                string file = Path.Combine(UriDBPath, "offset.rec");
                if (!File.Exists(file))
                {
                    //File.Create(file).Close();
                    FileShell.CreateFile(file);
                    File.WriteAllText(file, "0");
                }
                return file;
            }
        }
    }
}
