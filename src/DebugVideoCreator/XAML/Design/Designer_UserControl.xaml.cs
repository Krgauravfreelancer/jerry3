using System;
//using System.Collections.Generic;
using System.Windows;
using System.Data;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Newtonsoft.Json;
using Sqllite_Library.Models;
using System.Windows.Threading;
using System.ComponentModel;
using System.IO;
using System.Xml;

namespace VideoCreator.XAML
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Designer_UserControl : System.Windows.Controls.UserControl
    {
        //private DataTable designElements = null;
        public DataTable dataTableAdd;
        public DataTable dataTableUpdate; // Will come later
        public bool UserConsent = false;
        private readonly int selectedProjectId;
        private readonly List<CBVBackground> BackgroundImagesData;
        private bool toggleFlag = false;
        private CBVBackground selectedBGItem;
        private string imagePath;
        public Designer_UserControl(int _selectedProjectId, string _backgroundDatastring, bool isImagePathGiven = false)
        {
            InitializeComponent();
            selectedProjectId = _selectedProjectId;
            if(isImagePathGiven ) 
                imagePath = _backgroundDatastring;
            else
                BackgroundImagesData = JsonConvert.DeserializeObject<List<CBVBackground>>(_backgroundDatastring);

            InitialSetup();
        }

        private void InitialSetup()
        {
            dataTableAdd = createDesignDbDataTable();
            dataTableUpdate = createDesignDbDataTable();

            shapeBar.Visibility = Visibility.Hidden;
            propertyWindow.Visibility = Visibility.Hidden;
            designer.Visibility = Visibility.Hidden;

            designer.propertyWindow = propertyWindow;
            designer.shapeBar = shapeBar;
            designViewer.Visibility = Visibility.Visible;

            designer.Visibility = Visibility.Hidden;
            designViewer.Visibility = Visibility.Visible;
            DataTable designElements = designer.GetDesign();
            designViewer.LoadDesign(designElements);
        }

        private DataTable createDesignDbDataTable()
        {
            var dtDesign = new DataTable();
            dtDesign.Columns.Add("design_id", typeof(int));
            dtDesign.Columns.Add("fk_design_videoevent", typeof(int));
            dtDesign.Columns.Add("fk_design_screen", typeof(int));
            dtDesign.Columns.Add("fk_design_background", typeof(int));
            dtDesign.Columns.Add("design_xml", typeof(string));
            dtDesign.Columns.Add("design_createdate", typeof(string));
            dtDesign.Columns.Add("design_modifydate", typeof(string));
            return dtDesign;
        }

        private void BtnProceed_Click(object sender, RoutedEventArgs e)
        {
            DataTable designElements = designer.GetDesign();
            designViewer.LoadDesign(designElements);

            if (designElements.Rows.Count <= 1)
            {
                System.Windows.MessageBox.Show("Nothing to save, please create some design", "Information", (MessageBoxButton)MessageBoxButtons.OK, (MessageBoxImage)MessageBoxIcon.Error);
                return;
            }
            else
            {
                AddToDatabase(designElements);
                UserConsent = true;
                var myWindow = Window.GetWindow(this);
                myWindow.Close();
            }
        }

        private string GetBackgroundImageElement()
        {
            var height = stackDesigner.ActualHeight;
            var width = stackDesigner.ActualWidth;
            if (string.IsNullOrEmpty(imagePath))
            {
                if (BackgroundImagesData != null && BackgroundImagesData.Count > 0)
                {
                    // Process 
                    byte[] blob = (byte[])BackgroundImagesData[0].background_media;
                    using (var ms = new MemoryStream(blob))
                    {
                        var filename = $"image_{DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-ffffff")}.jpg";
                        var filepath = $@"{Directory.GetCurrentDirectory()}\{filename}";
                        using (var fs = new FileStream(filepath, FileMode.Create))
                        {
                            ms.WriteTo(fs);
                        }
                        return $"<Image Stretch=\"Uniform\" StretchDirection=\"DownOnly\" Width=\"{width}\" x:Name=\"bgImage\" Source=\"{filepath}\" Panel.ZIndex=\"0\"/>";
                    }
                }
            }
            else
            {
                return $"<Image Stretch=\"Uniform\" StretchDirection=\"DownOnly\" Width=\"{width}\" x:Name=\"bgImage\" Source=\"{imagePath}\" Panel.ZIndex=\"0\"/>";
            }
            return null;
        }

        private DataTable LoadBackgroundFromDB(string bgImageXAML)
        {
            var dataTable = designer.InitDataTable();
            dataTable.Clear();
            if (!string.IsNullOrEmpty(bgImageXAML))
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow["id"] = 1;
                dataRow["xaml"] = bgImageXAML;
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }

        private void BtnInitialiseDesigner_Click(object sender, RoutedEventArgs e)
        {
            // This can come from database
            var bgImageXAML = GetBackgroundImageElement();
            designer.LoadDesign(LoadBackgroundFromDB(bgImageXAML));

            if (!toggleFlag)
            {
                designer.Visibility = Visibility.Visible;
                designViewer.Visibility = Visibility.Hidden;
            }
            else
            {
                designer.Visibility = Visibility.Hidden;
                designViewer.Visibility = Visibility.Visible;
            }

            toggleFlag = !toggleFlag;
            BtnInitialiseDesigner.IsEnabled = false;
        }

        //private void cmbBackground_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    selectedBGItem = ((CBVBackground)cmbBackground?.SelectedItem);

        //    // Process 
        //    byte[] blob = (byte[])selectedBGItem.background_media;
        //    BitmapSource x = (BitmapSource)((new ImageSourceConverter()).ConvertFrom(blob));
        //    imgBackground.Source = x;
        //    btnSave.IsEnabled = selectedBGItem?.background_id >= 0;
        //}

        private void LoadComboBoxesBackground()
        {

            //cmbBackground.SelectedItem = null;
            //cmbBackground.DisplayMemberPath = "background_image_name";
            //cmbBackground.Items.Clear();
            //int i = 1;
            //foreach (var item in BackgroundImagesData)
            //{
            //    item.background_image_name = "background-" + i++;
            //    cmbBackground.Items.Add(item);
            //}
        }

        private void AddToDatabase(DataTable dtElements)
        {
            // For now only Add is supported.
            foreach (DataRow row in dtElements.Rows)
            {
                //var rowDesign = (int)row["id"] == -1 ? dataTableAdd.NewRow() : dataTableUpdate.NewRow();
                if (!string.IsNullOrEmpty(imagePath) && row["xaml"].ToString().StartsWith("<Image"))
                    continue;

                var rowDesign = dataTableAdd.NewRow();

                rowDesign["design_id"] = row["id"];
                rowDesign["fk_design_videoevent"] = -1;
                rowDesign["fk_design_screen"] = 1;
                rowDesign["fk_design_background"] = 1;
                rowDesign["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowDesign["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowDesign["design_xml"] = row["xaml"];
                dataTableAdd.Rows.Add(rowDesign);
                //if (-1 == (int)row["id"])
                //{
                //    dataTableAdd.Rows.Add(rowDesign);
                //}
                //else
                //{
                //    dataTableUpdate.Rows.Add(rowDesign);
                //}
            }
        }
    }
}
