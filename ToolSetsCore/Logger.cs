using System;
using System.IO;

namespace ToolSetsCore
{
    public static class Logger
    {
        public static string Dir = "Logs";

        public static void Log(string msg, bool hasTime = true)
        {
            PublicDatas.Instance.Aggregator?.GetEvent<LogEvent>().Publish($"\r\n[{DateTime.Now.ToString("yyyyMMdd HH:mm:ss")}] ---- {msg}");
        }

        public static void LogState(string msg, bool isFinshed = false)
        {
            if (!isFinshed)
            {
                Log($"开始{msg}......");
            }
            else
            {
                Log($"{msg}完成！");
            }
        }
    }
}