using Microsoft.Win32;

namespace VideoCreatorXAMLLibrary.Helpers
{
    public static class TestRegisteryHelper
    {
        const string registryPath = @"SOFTWARE\CommercialBase";
        const string registryUsernameKey = @"reg2";
        const string registryPasswordKey = @"reg1";

        public static void StoreUsernameAndPassword(string username, string password)
        {
            var registry = Registry.CurrentUser.CreateSubKey(registryPath, true);
            registry.SetValue(registryUsernameKey, username);
            registry.SetValue(registryPasswordKey, password);
            registry.Close();

        }


        public static string GetUsername()
        {
            string username;

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

            return username;
        }

        public static string GetPassword()
        {
            string password;

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
            return password;
        }

        public static void ClearRegistry()
        {
            var paths = registryPath.Split('\\');
            var baseKey = Registry.CurrentUser.OpenSubKey("SOFTWARE").OpenSubKey("CommercialBase", true);
            baseKey.DeleteValue(registryUsernameKey);
            baseKey.DeleteValue(registryPasswordKey);
            baseKey.Close();

        }
    }
}
