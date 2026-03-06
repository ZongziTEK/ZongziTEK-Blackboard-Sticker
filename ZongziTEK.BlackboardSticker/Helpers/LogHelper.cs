using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZongziTEK.BlackboardSticker.Helpers
{
    class LogHelper
    {
        public static string LogFile = "Log.txt";

        public static void NewLog(string str)
        {
            WriteLogToFile(str, LogType.Info);
        }

        public static void WriteLogToFile(string str, LogType logType = LogType.Info)
        {
            string strLogType = "Info";
            switch (logType)
            {
                case LogType.Event:
                    strLogType = "Event";
                    break;
                case LogType.Trace:
                    strLogType = "Trace";
                    break;
                case LogType.Error:
                    strLogType = "Error";
                    break;
            }
            try
            {
                var file = MainWindow.GetDataPath() + LogFile;
                StreamWriter sw = new StreamWriter(file, true);
                sw.WriteLine(string.Format("{0} [{1}] {2}", DateTime.Now.ToString("O"), strLogType, str));
                sw.Close();
            }
            catch { }
        }

        public enum LogType
        {
            Info,
            Trace,
            Error,
            Event
        }
    }
}
