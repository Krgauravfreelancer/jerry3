using Microsoft.Win32;
using System;
using System.IO;

namespace Sqllite_Library.Helpers
{
    public static class RegisteryHelper
    {
        const string registryPath = @"SOFTWARE\CommercialBase";
        const string registryDbPathKey = @"dbPath";
        const string registryDbNameKey = @"dbName";
        const string dbPath = @"C:\CommercialBase\vc";
        const string dbName = @"mainDB1.db";


        /// <summary>
        /// Stores database file name into registry
        /// </summary>
        /// <param name="fileName">database file with full path</param>

        public static void StoreFileName()
        {
            try
            {
                var registry = Registry.CurrentUser.CreateSubKey(registryPath, true);
                registry.SetValue(registryDbPathKey, dbPath);
                registry.SetValue(registryDbNameKey, dbName);
                registry.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public static string GetFileName()
        {
            string dbFullName;
            try
            {
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
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dbFullName;
        }

        public static void ClearRegistry()
        {
            try
            {
                var paths = registryPath.Split('\\');
                var baseKey = Registry.CurrentUser.OpenSubKey("SOFTWARE").OpenSubKey("CommercialBase", true);
                baseKey.DeleteValue(registryDbPathKey);
                baseKey.DeleteValue(registryDbNameKey);
                baseKey.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
