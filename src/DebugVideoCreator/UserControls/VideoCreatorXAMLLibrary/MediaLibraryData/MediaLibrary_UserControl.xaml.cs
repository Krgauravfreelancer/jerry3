using ServerApiCall_UserControl.DTO.MediaLibraryModels;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VideoCreatorXAMLLibrary.Auth;
using VideoCreatorXAMLLibrary.Helpers;
using VideoCreatorXAMLLibrary.Models;
using UserControl = System.Windows.Controls.UserControl;
using Windows = System.Windows.Controls;

namespace VideoCreatorXAMLLibrary.MediaLibraryData
{
    /// <summary>
    /// Interaction logic for ManageTimeline.xaml
    /// </summary>
    public partial class MediaLibrary_UserControl : UserControl, IDisposable
    {
        private SelectedProjectEvent selectedProjectEvent;
        private readonly AuthAPIViewModel authApiViewModel;
        private string TAGS = string.Empty;
        private int PAGESIZE = 10;
        private int PAGENUMBER = 1;
        private int TOTALPAGES = 0;
        private int _trackId;
        public event Action<DataTable> BtnUseAndSaveClickedEvent;
        public event Action<MediaEventInMiddle> BtnUseAndSaveClickedEventInMiddle;
        private TimelineVideoEvent shiftVideoEventLocation;
        public MediaLibrary selectedImage;
        public bool isEventAdded = false;
        private string videoevent_start;




        public MediaLibrary_UserControl()
        {
            InitializeComponent();
            videoevent_start = "00:00:00.000";
        }

        public MediaLibrary_UserControl(int trackId, SelectedProjectEvent _selectedProjectEvent, AuthAPIViewModel _authApiViewModel, string start = "00:00:00.000", TimelineVideoEvent _shiftVideoEventLocation = null)
        {
            InitializeComponent();

            _trackId = trackId;
            selectedProjectEvent = _selectedProjectEvent;
            authApiViewModel = _authApiViewModel;
            loader.Visibility = Visibility.Visible;
            videoevent_start = start;
            FetchAndFillTags();
            FillComboBoxes();
            LoaderHelper.ShowLoader(this, loader, "Fetching and loading data ...");
            shiftVideoEventLocation = _shiftVideoEventLocation;
        }

        private async void FetchAndFillTags()
        {
            var tags = await authApiViewModel.GetTags();
            foreach (var tag in tags)
            {
                listBoxtags.Items.Add(tag);
            }
        }

        private void FillComboBoxes()
        {
            cmbRecordsPerPage.Items.Add(10);
            cmbRecordsPerPage.Items.Add(20);
            cmbRecordsPerPage.Items.Add(30);
            //cmbRecordsPerPage.Items.Add(50);
            //cmbRecordsPerPage.Items.Add(100);
            //cmbRecordsPerPage.Items.Add(250);
            //cmbRecordsPerPage.Items.Add(500);
            cmbRecordsPerPage.SelectedIndex = 0;
        }

        private async Task FetchMediaLibraryData()
        {
            LoaderHelper.ShowLoader(this, loader, "Fetching and loading data ...");
            var result = await authApiViewModel.GetImagesLibraryData(PAGESIZE, PAGENUMBER, TAGS);
            if (result == null) return;
            TOTALPAGES = result.Meta.last_page;
            lblinfo.Content = $"Record Shown: {result.Meta.from} to {result.Meta.to}    |    Total: {result.Meta.total} Records    |    Page: {result.Meta.current_page} of {result.Meta.last_page}";
            await fillItems(result.Data);
            LoaderHelper.HideLoader(this, loader);
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
                    var tagLbl = new TextBlock
                    {
                        Text = $"TAGS : {String.Join(",   ", item.tags?.ToArray())}",
                        Height = 50,
                        MaxWidth = 200,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(5, 0, 5, 0),
                        HorizontalAlignment = HorizontalAlignment.Center,
                    };
                    stackPanel.Children.Add(tagLbl);
                }
                //Add Border
                Border stackPanelBorder = new Border
                {
                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(3),
                    Child = stackPanel,
                    Margin = new Thickness(5)
                };

                stackPanel.MouseDown += (object sender, System.Windows.Input.MouseButtonEventArgs e) =>
                {
                    if (e.ClickCount >= 1)
                    {
                        if (stackPanelBorder.BorderBrush == Brushes.Transparent)
                        {
                            foreach (Border childBorder in wrapImageContainer.Children)
                                childBorder.BorderBrush = Brushes.Transparent;
                            stackPanelBorder.BorderBrush = Brushes.DarkGray;
                            btnSelecAndUsethisImage.IsEnabled = true;
                            selectedImage = item;
                        }
                        else
                        {
                            stackPanelBorder.BorderBrush = Brushes.Transparent;
                            btnSelecAndUsethisImage.IsEnabled = false;
                            selectedImage = null;
                        }
                    }
                };
                wrapImageContainer.Children.Add(stackPanelBorder);
            }
        }

        private async void cmbRecordsPerPag_SelectionChanged(object sender, Windows.SelectionChangedEventArgs e)
        {
            PAGESIZE = (int)cmbRecordsPerPage?.SelectedItem;
            PAGENUMBER = 1;
            await FetchMediaLibraryData();
        }

        private async void listBoxtags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TAGS = string.Empty;
            var items = listBoxtags.SelectedItems;
            foreach (var item in items)
            {
                TAGS += item + ",";
            }
            TAGS = TAGS.Length > 0 ? TAGS.Remove(TAGS.Length - 1) : TAGS;
            PAGENUMBER = 1;
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
            if (PAGENUMBER < TOTALPAGES)
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

        private async void btnSelecAndUsethisImage_Click(object sender, RoutedEventArgs e)
        {
            LoaderHelper.ShowLoader(this, loader, "Downloading full file ..");
            var dataTable = new DataTable();
            dataTable.Columns.Add("videoevent_id", typeof(int));
            dataTable.Columns.Add("fk_videoevent_projdet", typeof(int));
            dataTable.Columns.Add("fk_videoevent_media", typeof(int));
            dataTable.Columns.Add("videoevent_track", typeof(int));
            dataTable.Columns.Add("videoevent_start", typeof(string));
            dataTable.Columns.Add("videoevent_duration", typeof(string));
            dataTable.Columns.Add("videoevent_origduration", typeof(string));
            dataTable.Columns.Add("videoevent_createdate", typeof(string));
            dataTable.Columns.Add("videoevent_modifydate", typeof(string));

            dataTable.Columns.Add("media", typeof(byte[])); // Media Column
            dataTable.Columns.Add("fk_videoevent_screen", typeof(int));//temp column for screen    

            dataTable.Columns.Add("videoevent_isdeleted", typeof(bool));
            dataTable.Columns.Add("videoevent_issynced", typeof(bool));
            dataTable.Columns.Add("videoevent_serverid", typeof(Int64));
            dataTable.Columns.Add("videoevent_syncerror", typeof(string));


            dataTable.Columns.Add("videoevent_notes", typeof(DataTable));

            // Since this table has Referential Integrity, so lets push one by one
            dataTable.Rows.Clear();

            var row = dataTable.NewRow();
            //row["videoevent_id"] = -1;
            row["fk_videoevent_projdet"] = selectedProjectEvent.projdetId;
            row["fk_videoevent_media"] = 1;
            row["videoevent_track"] = _trackId;
            row["videoevent_start"] = videoevent_start;
            row["videoevent_duration"] = "00:00:10.000";
            row["videoevent_origduration"] = "00:00:10.000";
            row["videoevent_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            row["videoevent_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            row["videoevent_isdeleted"] = false;
            row["videoevent_issynced"] = false;
            row["videoevent_serverid"] = -1;
            row["videoevent_syncerror"] = string.Empty;

            row["fk_videoevent_screen"] = -1; // Not needed for this case
            var byteArrayIn = await authApiViewModel.GetSecuredFileByteArray(selectedImage?.media_download_link);
            row["media"] = byteArrayIn;
            dataTable.Rows.Add(row);

            BtnUseAndSaveClickedEvent.Invoke(dataTable);

            var payload = new MediaEventInMiddle
            {
                datatable = dataTable,
                shiftVideoEventLocation = shiftVideoEventLocation,
            };

            BtnUseAndSaveClickedEventInMiddle.Invoke(payload);
            isEventAdded = true;
            var myWindow = Window.GetWindow(this);
            myWindow.Close();

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            isEventAdded = false;
            var myWindow = Window.GetWindow(this);
            myWindow.Close();
        }

        public void Dispose()
        {
            Console.WriteLine("The ManageTimeline_UserControl > dispose() function has been called and the resources have been released!");
        }
    }
}
