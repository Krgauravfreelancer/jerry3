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
using Xceed.Wpf.Toolkit.Panels;
using DebugVideoCreator.PaginatedListView;
using dbTransferUser_UserControl.ResponseObjects.MediaLibrary;
using System.Windows.Media.Imaging;
using System.Windows.Media;

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
        int PAGESIZE = 10;
        int PAGENUMBER = 1;
        string TAGS = string.Empty;
        int TOTALPAGES = 0;

        public MediaLibrary_UserControl()
        {
            InitializeComponent();
        }

        public MediaLibrary_UserControl(int projectId, Int64 _selectedServerProjectId, AuthAPIViewModel _authApiViewModel)
        {
            InitializeComponent();
            
            selectedProjectId = projectId;
            selectedServerProjectId = _selectedServerProjectId;
            authApiViewModel = _authApiViewModel;


            FillComboBoxes();
            
            
        }

        private void FillComboBoxes()
        {
            cmbRecordsPerPage.Items.Add(10);
            cmbRecordsPerPage.Items.Add(20);
            cmbRecordsPerPage.Items.Add(30);
            cmbRecordsPerPage.Items.Add(50);
            cmbRecordsPerPage.Items.Add(100);
            cmbRecordsPerPage.Items.Add(250);
            cmbRecordsPerPage.Items.Add(500);
            cmbRecordsPerPage.SelectedIndex = 0;
        }

        private async Task FetchMediaLibraryData()
        {
            var result = await authApiViewModel.GetImagesLibraryData(PAGESIZE, PAGENUMBER, TAGS);
            if(result == null) return;
            TOTALPAGES = result.Meta.last_page;
            lblinfo.Content = $"Record Shown: {result.Meta.from} to {result.Meta.to}                      Page {result.Meta.current_page} of {result.Meta.last_page}";
            await fillItems(result.Data);
        }

        private async Task fillItems(List<MediaLibrary> data)
        {
            wrapImageContainer.Children.Clear();
            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];

                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    MaxWidth = 350,
                    MaxHeight = 350,
                    Background = Brushes.White
                };
                //Image
                var byteArrayIn = await authApiViewModel.GetSecuredFileByteArray(item.thumbnail_download_link);
                if (byteArrayIn != null)
                {
                    using (var ms = new MemoryStream(byteArrayIn))
                    {
                        var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                        var bitmapSource = decoder.Frames[0];
                        var image = new Image { Source = bitmapSource, Height = 150, MaxWidth = 200, Margin = new Thickness(5) };
                        stackPanel.Children.Add(image);
                    }
                }
                //Add Tage if there are any
                if (item.tags?.Count > 0)
                {
                    var tagLbl = new Label
                    {
                        Content = $"Tags: {String.Join(", ", item.tags?.ToArray())}",
                        Height = 50,
                        Margin = new Thickness(5, 0, 5, 0),
                        HorizontalAlignment = HorizontalAlignment.Center,
                    };
                    stackPanel.Children.Add(tagLbl);
                }
                //Add Border
                Border stackPanelBorder = new Border
                {
                    BorderBrush = Brushes.Blue,
                    BorderThickness = new Thickness(1),
                    Child = stackPanel,
                    Margin = new Thickness(5)
                };

                stackPanel.MouseDown += (object sender, System.Windows.Input.MouseButtonEventArgs e) =>
                {
                    if (e.ClickCount >= 1)
                    {
                        if(stackPanelBorder.BorderThickness == new Thickness(1))
                        {
                            foreach (Border childBorder in wrapImageContainer.Children)
                                childBorder.BorderThickness = new Thickness(1); 
                            stackPanelBorder.BorderThickness = new Thickness(3);
                        }
                        else
                            stackPanelBorder.BorderThickness = new Thickness(1);
                    }
                };
                wrapImageContainer.Children.Add(stackPanelBorder);
            }
        }

        private async void cmbRecordsPerPag_SelectionChanged(object sender, Windows.SelectionChangedEventArgs e)
        {
            PAGESIZE = (int)cmbRecordsPerPage?.SelectedItem;
            await FetchMediaLibraryData();
        }

        private async void btnFirst_Click(object sender, RoutedEventArgs e)
        {
            PAGENUMBER = 1;
            await FetchMediaLibraryData();
        }

        private async void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (PAGENUMBER > 1)
            {
                PAGENUMBER--;
                await FetchMediaLibraryData();
            }
        }

        private async void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if(PAGENUMBER < TOTALPAGES)
            {
                PAGENUMBER++;
                await FetchMediaLibraryData();
            }
        }

        private async void btnLast_Click(object sender, RoutedEventArgs e)
        {
            if (PAGENUMBER < TOTALPAGES)
            {
                PAGENUMBER = TOTALPAGES;
                await FetchMediaLibraryData();
            }
        }


        public void Dispose()
        {
            Console.WriteLine("The ManageTimeline_UserControl > dispose() function has been called and the resources have been released!");
        }
    }
}
