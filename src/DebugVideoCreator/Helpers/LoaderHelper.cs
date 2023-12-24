﻿using dbTransferUser_UserControl.ResponseObjects.App;
using dbTransferUser_UserControl.ResponseObjects.Background;
using dbTransferUser_UserControl.ResponseObjects.Company;
using dbTransferUser_UserControl.ResponseObjects.Media;
using dbTransferUser_UserControl.ResponseObjects.Projects;
using dbTransferUser_UserControl.ResponseObjects.Screen;
using dbTransferUser_UserControl.ResponseObjects.VideoEvent;
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

namespace VideoCreator.Helpers
{
    public static class LoaderHelper
    {
        public static void ShowLoader(Window mywindow, LoadingAnimation loader)
        {
            if (mywindow == null)
                return;

            loader.Visibility = Visibility.Visible;
            mywindow.IsEnabled = false;
        }

        public static void ShowLoader(UserControl usercontrol, LoadingAnimation loader)
        {
            if (usercontrol == null) 
                return;
            var mywindow = Window.GetWindow(usercontrol);
            if (mywindow == null)
                return;

            loader.Visibility = Visibility.Visible;
            mywindow.IsEnabled = false;
        }

        public static void HideLoader(Window mywindow, LoadingAnimation loader)
        {
            if (mywindow == null)
                return;

            loader.Visibility = Visibility.Hidden;
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
            mywindow.IsEnabled = true;
        }


        
    }
}