using AudioEditor_UserControl;
using AudioRecorder_UserControl;
using Newtonsoft.Json;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using Windows = System.Windows.Controls;
using Forms = System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;
using AudioPlayer_UserControl;
using dbTransferUser_UserControl.ResponseObjects.VideoEvent;
using VideoCreator.Auth;
using System.Threading.Tasks;
using NAudio.CoreAudioApi.Interfaces;
using VideoCreator.Helpers;
using System.Windows.Threading;
using System.Diagnostics.Contracts;
using System.Windows.Controls;
using System.Net;
using SixLabors.ImageSharp.PixelFormats;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using DebugVideoCreator.Models;
using System.IO;

namespace VideoCreator.PaginatedListView
{
    /// <summary>
    /// Interaction logic for ManageTimeline.xaml
    /// </summary>
    public partial class MediaLibrary_UserControl : UserControl, IDisposable
    {
        private int selectedProjectId;
        private Int64 selectedServerProjectId;
        private readonly AuthAPIViewModel authApiViewModel;
        int selectedRecordsPerPage;

        public MediaLibrary_UserControl()
        {
            InitializeComponent();
        }

        public MediaLibrary_UserControl(int projectId, Int64 _selectedServerProjectId, AuthAPIViewModel _authApiViewModel)
        {
            InitializeComponent();
            FillComboBoxes();
            cmbRecordsPerPag.SelectedIndex = 0;
            selectedProjectId = projectId;
            selectedServerProjectId = _selectedServerProjectId;
            authApiViewModel = _authApiViewModel;
        }

        private void FillComboBoxes()
        {
            cmbRecordsPerPag.Items.Add(10);
            cmbRecordsPerPag.Items.Add(20);
            cmbRecordsPerPag.Items.Add(30);
            cmbRecordsPerPag.Items.Add(50);
            cmbRecordsPerPag.Items.Add(100);
            cmbRecordsPerPag.Items.Add(250);
            cmbRecordsPerPag.Items.Add(500);
        }

        

        private void cmbRecordsPerPag_SelectionChanged(object sender, Windows.SelectionChangedEventArgs e)
        {
            selectedRecordsPerPage = (int)cmbRecordsPerPag?.SelectedItem;
            if (selectedRecordsPerPage > 0)
            {
                // Do the actual work
            }
        }

        


        private void btnFirst_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnLast_Click(object sender, RoutedEventArgs e)
        {

        }


        public void Dispose()
        {
            Console.WriteLine("The ManageTimeline_UserControl > dispose() function has been called and the resources have been released!");
        }
    }
}
