using System.Windows;
using System.Windows.Controls;
using VideoCreatorXAMLLibrary.Loader;

namespace VideoCreatorXAMLLibrary.Helpers
{
    public static class LoaderHelper
    {
        private static string DefaultLoadingText = "";
        public static void ShowLoader(Window mywindow, LoadingAnimation loader, string message = "")
        {
            if (mywindow == null)
                return;

            loader.setTextBlockMessage(message);
            loader.Visibility = Visibility.Visible;
            mywindow.IsEnabled = false;
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
        }

        public static void HideLoader(Window mywindow, LoadingAnimation loader)
        {
            if (mywindow == null)
                return;

            loader.Visibility = Visibility.Hidden;
            loader.setTextBlockMessage(DefaultLoadingText);
            mywindow.IsEnabled = true;
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
        }
    }
}
