using Microsoft.Win32;
using System;

namespace VideoCreator.Helpers
{
    public static class TestRegisteryHelper
    {
        const string registryPath = @"SOFTWARE\CommercialBase";
        const string registryUsernameKey = @"authuser";
        const string registryPasswordKey = @"authpassword";

        public static void StoreUsernameAndPassword(string username, string password)
        {
            try
            {
                var registry = Registry.CurrentUser.CreateSubKey(registryPath, true);
                registry.SetValue(registryUsernameKey, username);
                registry.SetValue(registryPasswordKey, password);
                registry.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public static string GetUsername()
        {
            string username;
            try
            {
                RegistryKey registry = Registry.CurrentUser.OpenSubKey(registryPath, true);
                if (null == registry)
                    return null; // "Registry does not exists"

                object value = registry.GetValue(registryUsernameKey);
                if (null == value)
                {
                    registry.Close();
                    return null; // "Username does not exists"
                }
                username = value.ToString();
                registry.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return username;
        }

        public static string GetPassword()
        {
            string password;
            try
            {
                RegistryKey registry = Registry.CurrentUser.OpenSubKey(registryPath, true);
                if (null == registry)
                    return null; // "Registry does not exists"

                object value = registry.GetValue(registryPasswordKey);
                if (null == value)
                {
                    registry.Close();
                    return null; // "Password does not exists"
                }
                password = value.ToString();
                registry.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return password;
        }

        public static void ClearRegistry()
        {
            try
            {
                var paths = registryPath.Split('\\');
                var baseKey = Registry.CurrentUser.OpenSubKey("SOFTWARE").OpenSubKey("CommercialBase", true);
                baseKey.DeleteValue(registryUsernameKey);
                baseKey.DeleteValue(registryPasswordKey);
                baseKey.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
