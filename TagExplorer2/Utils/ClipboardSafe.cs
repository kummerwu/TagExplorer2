using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TagExplorer2.Model.FileSystem;

namespace TagExplorer2.Utils
{
    //恶心的WPF剪贴板和Z，总是导致剪贴板操作失败
    class ClipBoardSafe
    {
        public static void test(bool cut)
        {
            //FilesClipBoard(new string[] { @"B:\rtest1", @"B:\rtest2.accdb", @"B:\rtest3.xlsx", @"B:\r中文测试4.zip",
            ////@"B:\目录\test1", @"B:\目录\test2.accdb", @"B:\目录\test3.xlsx", @"B:\目录\中文测试4.zip"
            //@"B:\目录"
            //}, cut);
        }
        public static string GetAppendInf()
        {
            try
            {
                IDataObject o = Clipboard.GetDataObject();
                object opt = o.GetData(ClipboardConst.KUMMERWU_CLIPBOARD_OPT);
                string appendInf = opt as string;
                if (appendInf == null) appendInf = "";
                if (string.IsNullOrEmpty(appendInf))
                {
                    appendInf = GetText();
                }
                return appendInf;
            }
            catch (Exception ee)
            {
                Logger.E(ee);
            }
            return "";
        }
        public static FileShellOpt GetFileOprationFromClipboard()
        {
            try
            {
                IDataObject o = Clipboard.GetDataObject();
                object opt = o.GetData("Preferred DropEffect");
                byte[] moveEffect = new byte[] { 0, 0, 0, 0 };
                MemoryStream stream = opt as MemoryStream;
                stream.Read(moveEffect, 0, 4);
                if (moveEffect[0] == 2) return FileShellOpt.CUT;
                else return FileShellOpt.COPY;
            }
            catch (Exception ee)
            {
                Logger.E(ee);
            }
            return FileShellOpt.COPY;
        }
        /// <summary>
        /// 获得当前剪贴板中的文件列表
        /// </summary>
        /// <returns></returns>
        public static void GetFileListFromClipboard(out List<string> files, out List<string> weblinks)
        {
            files = new List<string>();
            weblinks = new List<string>();

            string txt = ClipBoardSafe.GetText();
            txt = txt.Trim('"');
            txt = txt.Trim();
            if (PathHelper.IsValidHttp(txt))
            {
                if (!weblinks.Contains(txt))
                {
                    weblinks.Add(txt);
                }
            }
            else if (PathHelper.IsValidUri(txt))
            {
                if (!files.Contains(txt))
                {
                    files.Add(txt);
                }
            }
            StringCollection sc = Clipboard.GetFileDropList();

            foreach (string f in sc)
            {

                txt = f.Trim('"');

                if (PathHelper.IsValidHttp(txt))
                {
                    if (!weblinks.Contains(txt))
                    {
                        weblinks.Add(txt);
                    }
                }
                else if (PathHelper.IsValidUri(txt))
                {
                    if (!files.Contains(txt))
                    {
                        files.Add(txt);
                    }
                }
            }

            return;
        }

        //将文件操作放到剪贴板中去，剪贴板中有两个格式，一个是标准文件操作（和explorer互动），一个是自己定义的剪贴板格式）
        public static bool SetFilesOptToClipBoard(string[] files, bool cut, string appendInf)
        {
            if (files == null) return true;

            IDataObject data = new DataObject(DataFormats.FileDrop, files);
            MemoryStream memo = new MemoryStream(4);
            byte[] bytes = new byte[] { (byte)(cut ? 2 : 5), 0, 0, 0 };
            memo.Write(bytes, 0, bytes.Length);
            data.SetData("Preferred DropEffect", memo);
            data.SetData(ClipboardConst.KUMMERWU_CLIPBOARD_OPT, appendInf);


            string fileInfTxt = "";
            foreach (string f in files)
            {
                fileInfTxt += string.Format("\"{0}\"", f);
            }
            fileInfTxt.Trim();
            data.SetData(DataFormats.Text, fileInfTxt);

            Clipboard.SetDataObject(data);
            //SetTextInner(appendInf);
            return true;
        }

        private static string _innerMockClipboard = "";
        public static bool SetTextInner(string txt)
        {
            _innerMockClipboard = txt;
            return true;
        }
        public static string GetTextInner()
        {
            return _innerMockClipboard;
        }
        public static void ClearTextInner()
        {
            _innerMockClipboard = "";
        }
        //为了解决WPF剪贴板总是设置失败的问题，增加了重试保护和多种设置方式
        public static bool SetText(string txt)
        {
            if (txt == null) txt = "";
            startTime = DateTime.Now;

            SetTextInner(txt);

            if (SetText(txt, 1)) return true;           //尝试多种方式，每种方式尝试1次
            else if (SetText(txt, 3)) return true;      //尝试多种方式，每一种方式尝试3次
            else if (SetText(txt, 5)) return true;      //尝试多种方式，每一种方式再次尝试5次
            else
            {
                ReportErr(txt);                         //失败了，记录失败原因返回
                return false;
            }
        }
        public static string GetText()
        {
            return Clipboard.GetText();
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetOpenClipboardWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        static DateTime startTime;

        private static bool SetText(string txt, int retryCnt)
        {
            if (SetTextWinFormClip(txt, retryCnt)) return true;     //先尝试Window.Forms的SetText
            else if (SetTextWPFClip(txt, retryCnt)) return true;    //在尝试WPF中Window中的SetText
            else if (SetTextByData(txt, retryCnt)) return true;     //再尝试SetData方法
            else return false;
        }

        private delegate void SetClipTxt(string txt);
        private static bool SetTextTemplateFunc(string txt, int retryCnt, SetClipTxt func)
        {
            if ((DateTime.Now - startTime).TotalSeconds > 5)
            {
                return false;
            }
            for (int i = 0; i < retryCnt; i++)
            {
                try
                {
                    func(txt);
                    return true;
                }
                catch (Exception e)
                {
                    Logger.E(e);
                    System.Threading.Thread.Sleep(10);
                }
            }

            return false;
        }
        //使用window.form的剪贴板
        private static bool SetTextWinFormClip(string txt, int retryCnt)
        {
            return SetTextTemplateFunc(txt, retryCnt, System.Windows.Forms.Clipboard.SetText);
        }
        //使用WPF的剪贴板
        private static bool SetTextWPFClip(string txt, int retryCnt)
        {
            return SetTextTemplateFunc(txt, retryCnt, Clipboard.SetText);
        }
        //使用WPF的剪贴板 SetDataObject
        private static bool SetTextByData(string txt, int retryCnt)
        {
            return SetTextTemplateFunc(txt, retryCnt, Clipboard.SetDataObject);
        }

        //获得剪贴板访问错误信息：当前剪贴板被哪个进程占用？该进程窗口标题是什么？
        private static Process ProcessHoldingClipboard()
        {
            Process theProc = null;

            IntPtr hwnd = GetOpenClipboardWindow();

            if (hwnd != IntPtr.Zero)
            {
                uint processId;
                uint threadId = GetWindowThreadProcessId(hwnd, out processId);

                Process[] procs = Process.GetProcesses();
                foreach (Process proc in procs)
                {
                    IntPtr handle = proc.MainWindowHandle;

                    if (handle == hwnd)
                    {
                        theProc = proc;
                    }
                    else if (processId == proc.Id)
                    {
                        theProc = proc;
                    }
                }
            }

            return theProc;
        }

        //报告错误信息
        private static void ReportErr(string txt)
        {
            Process p = ProcessHoldingClipboard();
            if (p == null)
            {
                MessageBox.Show("将内容拷贝到剪贴板失败" + txt, "设置剪贴板错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("将内容拷贝到剪贴板失败：\r\n占用剪贴板程序为：" + p.MainModule.FileName + "\r\n窗口标题为：" + p.MainWindowTitle + "\r\n附-操作数据：" + txt, "设置剪贴板错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
