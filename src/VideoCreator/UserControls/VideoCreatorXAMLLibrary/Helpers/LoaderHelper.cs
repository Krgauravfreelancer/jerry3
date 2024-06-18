using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using VideoCreatorXAMLLibrary.Loader;

namespace VideoCreatorXAMLLibrary.Helpers
{
    public static class LoaderHelper
    {
        private static string DefaultLoadingText = "";
        public static List<UserControl> UserControls = new List<UserControl>();
        public static List<Window> Windows = new List<Window>();
        
        
        public static void HideAllLoader()
        {
            while (UserControls?.Count > 0)
            {
                var item = UserControls[0];
                var loader = (LoadingAnimation)item.FindName("loader");
                if (loader != null)
                    HideLoader(item, loader);
            }

            while (Windows?.Count > 0)
            {
                var item = Windows[0];
                var loader = (LoadingAnimation)item.FindName("loader");
                if (loader != null)
                    HideLoader(item, loader);
            }
        }
        
        
        public static void ShowLoader(Window mywindow, LoadingAnimation loader, string message = "")
        {
            if (mywindow == null)
                return;

            loader.setTextBlockMessage(message);
            loader.Visibility = Visibility.Visible;
            mywindow.IsEnabled = false;
            if (!Windows.Contains(mywindow))
                Windows.Add(mywindow);
            Console.WriteLine($"ShowLoaderForWindow | Total windows count - {Windows.Count} & UserControls.Count - {UserControls.Count}");
        }

        public static void ShowLoader(UserControl usercontrol, LoadingAnimation loader, string message = "", bool disableParentWindow = true)
        {
            if (usercontrol == null)
                return;

            var mywindow = Window.GetWindow(usercontrol);
            if (mywindow == null)
                return;

            loader.setTextBlockMessage(message);
            loader.Visibility = Visibility.Visible;
            if (disableParentWindow)
                mywindow.IsEnabled = false;
            if (!UserControls.Contains(usercontrol))
                UserControls.Add(usercontrol);
            Console.WriteLine($"ShowLoaderForUserControl | Total windows count - {Windows.Count} & UserControls.Count - {UserControls.Count}");
        }

        public static void HideLoader(Window mywindow, LoadingAnimation loader)
        {
            if (mywindow == null)
                return;

            loader.Visibility = Visibility.Hidden;
            loader.setTextBlockMessage(DefaultLoadingText);
            mywindow.IsEnabled = true;

            if (Windows.Contains(mywindow))
                Windows.Remove(mywindow);
            Console.WriteLine($"HideLoaderForWindow | Total windows count - {Windows.Count} & UserControls.Count - {UserControls.Count}");
        }

        public static void HideLoader(UserControl usercontrol, LoadingAnimation loader)
        {
            if (usercontrol == null)
                return;
            var mywindow = Window.GetWindow(usercontrol);
            if (mywindow == null)
                return;

            loader.Visibility = Visibility.Hidden;
            loader.setTextBlockMessage(DefaultLoadingText);
            mywindow.IsEnabled = true;

            if (UserControls.Contains(usercontrol))
                UserControls.Remove(usercontrol);
            Console.WriteLine($"HideLoaderForUserControl | Total windows count - {Windows.Count} & UserControls.Count - {UserControls.Count}");
        }
    }
}
