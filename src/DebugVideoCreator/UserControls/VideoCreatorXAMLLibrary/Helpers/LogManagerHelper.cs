using NLog;
using System;
using System.Configuration;

namespace VideoCreatorXAMLLibrary.Helpers
{
    public static class LogManagerHelper
    {
        public static void WriteVerboseLog(string log,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            var isVerboseLogging = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("VerboseLogging"));
            if (isVerboseLogging)
            {
                string location = sourceFilePath;

                if (location.Contains("\\"))
                {
                    string[] temp = location.Split('\\');
                    location = temp[temp.Length - 1].Replace(".cs", "");
                }

                var Method = memberName;
                var className = location;

                var logger = LogManager.GetLogger($"{className}.cs >> {memberName} >> LineNumber-{sourceLineNumber}");
                logger.Info($"{Environment.NewLine}\t{log}", DateTime.Now);
            }
        }

        public static void WriteErroreLog(string log,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            string location = sourceFilePath;

            if (location.Contains("\\"))
            {
                string[] temp = location.Split('\\');
                location = temp[temp.Length - 1].Replace(".cs", "");
            }

            var Method = memberName;
            var className = location;

            var logger = LogManager.GetLogger($"{className}.cs >> {memberName} >> LineNumber-{sourceLineNumber}");
            logger.Error($"{Environment.NewLine}\t{log}", DateTime.Now);

        }
    }
}
