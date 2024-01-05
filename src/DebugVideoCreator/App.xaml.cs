using NLog.Config;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace VideoCreator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public App()
        {
            LogManager.Configuration = new XmlLoggingConfiguration(@".\\NLog.config");
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(AppDispatcherUnhandledException);
        }

        void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowUnhandledException(e);    
        }

        void ShowUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            if (CheckNet())
                e.Handled = true;
            else
            {
                e.Handled = false;
                return;
            }
            var exception = $"{Environment.NewLine}Exception : {e.Exception.Message}{Environment.NewLine}Inner Exception : {(e.Exception.InnerException != null ? e.Exception.InnerException.Message : "NA")}";
            string errorMessage = $"An application error occurred." +
                $"\nPlease check whether your data is correct and repeat the action. If this error occurs again there seems to be a more serious malfunction in the application, and you better close it." +
                $"\n{exception}\n\nDo you want to continue?" +
                $"\n\n(if you click Yes you will continue with your work, if you click No the application will close)";

            logger.Error($"Exception Occured. Please see below - {exception}{Environment.NewLine}StackTrace : {e.Exception.StackTrace}", this, DateTime.Now);

            if (MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Error) == MessageBoxResult.No)
            {
                if (MessageBox.Show("WARNING: The application will close. Any changes will not be saved!\nDo you really want to close it?", "Close the application!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
            }
        }

        private bool CheckNet()
        {
            bool stats;
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() == true)
                stats = true;
            else
                stats = false;
            return stats;
        }
    }
            
}
