using System.Collections.Generic;
using System.IO;

namespace Sqllite_Library.Helpers
{
    public static class PathHelper
    {
        const string suffixCommon = @"CommercialBase";
        static string TempPath = @"";
        static List<string> pathList = new List<string>();
        /// <summary>
        /// returns temp path
        /// </summary>
        /// <param name="subPath">subpath is needed</param>
        public static string GetTempPath(string subPath = "")
        {
            if (string.IsNullOrEmpty(TempPath))
                TempPath = $"{Path.GetTempPath()}{suffixCommon}";
            var path = string.IsNullOrEmpty(subPath) ? TempPath : $"{TempPath}\\{subPath}";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            pathList.Add(path);
            return path;
        }

        public static void ClearTempDirectories()
        {
            foreach (var path in pathList)
            {
                try
                {
                    if (Directory.Exists(path))
                        Directory.Delete(path, true);
                }
                catch { }
            }
        }
    }
}
