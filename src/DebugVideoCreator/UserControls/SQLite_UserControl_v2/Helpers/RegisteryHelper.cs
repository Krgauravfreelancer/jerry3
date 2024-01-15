using Microsoft.Win32;
using System;
using System.IO;

namespace Sqllite_Library.Helpers
{
    public static class RegisteryHelper
    {
        const string registryPath = @"SOFTWARE\CommercialBase";
        const string registryDbPathKey = @"reg4";
        const string registryDbNameKey = @"reg3";
        const string dbPath = @"C:\CommercialBase\vc";
        const string dbName = @"mainDB1.db";


        /// <summary>
        /// Stores database file name into registry
        /// </summary>
        /// <param name="fileName">database file with full path</param>

        public static void StoreFileName()
        {
            var registry = Registry.CurrentUser.CreateSubKey(registryPath, true);
            registry.SetValue(registryDbPathKey, dbPath);
            registry.SetValue(registryDbNameKey, dbName);
            registry.Close();
        }


        public static string GetFileName()
        {
            string dbFullName;
            RegistryKey registry = Registry.CurrentUser.OpenSubKey(registryPath, true);
            if (null == registry)
                return null; // "Registry does not exists"

            object dbPathFromRegistry = registry.GetValue(registryDbPathKey);
            object dbNameFromRegistry = registry.GetValue(registryDbNameKey);
            if (null == dbPathFromRegistry || null == dbNameFromRegistry)
            {
                registry.Close();
                return null; // "Either DB Path or Name does not exists"
            }
            Directory.CreateDirectory(dbPathFromRegistry.ToString());
            if (!dbPathFromRegistry.ToString().EndsWith("\\"))
                dbPathFromRegistry += "\\";

            dbFullName = Path.Combine(dbPathFromRegistry.ToString(), dbNameFromRegistry.ToString());
            registry.Close();
            return dbFullName;
        }

        public static void ClearRegistry()
        {
            var paths = registryPath.Split('\\');
            var baseKey = Registry.CurrentUser.OpenSubKey("SOFTWARE").OpenSubKey("CommercialBase", true);
            baseKey.DeleteValue(registryDbPathKey);
            baseKey.DeleteValue(registryDbNameKey);
            baseKey.Close();
        }
    }
}
