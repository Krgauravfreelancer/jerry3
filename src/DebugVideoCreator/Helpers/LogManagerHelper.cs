using ServerApiCall_UserControl.DTO.App;
using ServerApiCall_UserControl.DTO.Background;
using ServerApiCall_UserControl.DTO.Company;
using ServerApiCall_UserControl.DTO.Media;
using ServerApiCall_UserControl.DTO.Projects;
using ServerApiCall_UserControl.DTO.Screen;
using ServerApiCall_UserControl.DTO.VideoEvent;
using DebugVideoCreator.Models;
using Newtonsoft.Json;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml.Linq;
using VideoCreator.Auth;
using VideoCreator.Loader;
using VideoCreator.XAML;
using NLog;
using NLog.Fluent;
using System.Diagnostics;

namespace VideoCreator.Helpers
{
    public static class LogManagerHelper
    {
        public static void WriteVerboseLog(UserControl uc, string log, string className)
        {
            var isVerboseLogging = DebugVideoCreator.Properties.Settings.Default.VerboseLogging;
            var logger = LogManager.GetLogger(uc.GetType().FullName);
            if (isVerboseLogging)
                logger.Info(log, uc, DateTime.Now);
        }

        public static void WriteVerboseLog(Window w, string log)
        {
            var isVerboseLogging = DebugVideoCreator.Properties.Settings.Default.VerboseLogging;
            var logger = LogManager.GetLogger(w.GetType().FullName);
            if (isVerboseLogging)
                logger.Info(log, w, DateTime.Now);
        }
    }
}
