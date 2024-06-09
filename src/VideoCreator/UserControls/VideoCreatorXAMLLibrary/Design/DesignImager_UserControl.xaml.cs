using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace VideoCreatorXAMLLibrary
{
    public partial class DesignImager_UserControl : System.Windows.Controls.UserControl
    {
        private int selectedVideoEvent;
        public DataTable dtVideoSegment;
        public bool UserConsent;
        public DesignImager_UserControl(int selectedVideoEvent)
        {
            InitializeComponent();
            this.selectedVideoEvent = selectedVideoEvent;
            LoadDesign(this.selectedVideoEvent);
            InitializeTable();
        }

        public DesignImager_UserControl(DataTable designTable)
        {
            InitializeComponent();
            LoadDesign(designTable);
            InitializeTable();
        }

        #region == Events ==
        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            // Get the Image as byte stream 
            DataTable dataTable = designImager.GetImage();

            // Process 
            if (dataTable != null)
            {
                byte[] blob = (byte[])dataTable.Rows[0]["imageData"];
                BitmapSource x = (BitmapSource)((new ImageSourceConverter()).ConvertFrom(blob));
                imgDesign.Source = x;
                SaveToDataTable(blob);
            }
            UserConsent = true;
            var myWindow = Window.GetWindow(this);
            myWindow.Close();
        }

        #endregion

        private void LoadDesign(DataTable designTable)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("id", typeof(int));
            dataTable.Columns.Add("xaml", typeof(string));

            foreach (DataRow row in designTable.Rows)
            {
                var dataRow = dataTable.NewRow();
                dataRow["id"] = -1;
                dataRow["xaml"] = Convert.ToString(row["design_xml"]);
                dataTable.Rows.Add(dataRow);
            }
            designImager.LoadDesign(dataTable);
        }

        private void LoadDesign(int videoEvent = 1)
        {
            DataTable dataTable = new DataTable();
            dataTable.Clear();
            dataTable.Columns.Add("id", typeof(int));
            dataTable.Columns.Add("xaml", typeof(string));

            List<CBVDesign> cBVDesigns = DataManagerSqlLite.GetDesign(videoEvent);
            foreach (CBVDesign cBVDesign in cBVDesigns)
            {
                var dataRow = dataTable.NewRow();
                dataRow["id"] = -1;
                dataRow["xaml"] = cBVDesign.design_xml;
                dataTable.Rows.Add(dataRow);
            }

            designImager.LoadDesign(dataTable);
        }

        private void InitializeTable()
        {
            dtVideoSegment = new DataTable();
            dtVideoSegment.Columns.Add("videosegment_id", typeof(int));
            dtVideoSegment.Columns.Add("fk_videosegment_videoevent", typeof(int));
            dtVideoSegment.Columns.Add("videosegment_media", typeof(byte[]));
            dtVideoSegment.Columns.Add("videosegment_createdate", typeof(string));
            dtVideoSegment.Columns.Add("videosegment_modifydate", typeof(string));
        }

        public void SaveToDataTable(byte[] blob)
        {
            var newRow = dtVideoSegment.NewRow();
            newRow["videosegment_id"] = -1;
            newRow["fk_videosegment_videoevent"] = selectedVideoEvent;
            newRow["videosegment_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            newRow["videosegment_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            newRow["videosegment_media"] = blob;
            dtVideoSegment.Rows.Add(newRow);
        }

        public byte[] XMLToImage(DataTable dataTable)
        {
            // Get the Image as byte stream 
            Canvas container = new Canvas();
            container.RenderSize = new Size(1920, 1080);

            //Canvas canvas = new Canvas();
            //canvas.RenderSize = new Size(1920, 1080);
            string text = $@"<Canvas
                                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>";
            string text2 = "</Canvas>";
            foreach (DataRow row in dataTable.Rows)
            {
                var xaml = (string)row["design_xml"];
                string s = text + xaml + text2;
                StringReader input = new StringReader(s);
                XmlReader reader = XmlReader.Create(input);
                var canvas = (Canvas)XamlReader.Load(reader);
                canvas.RenderSize = new Size(1920, 1080);
                if (canvas.Children.Count == 1)
                {
                    UIElement element = canvas.Children[0];
                    canvas.Children.RemoveAt(0);
                    container.Children.Add(element);
                }
                else
                {
                    while (canvas.Children.Count > 0)
                    {
                        UIElement element = canvas.Children[0];
                        canvas.Children.RemoveAt(0);
                        container.Children.Add(element);
                    }
                }
            }

            container.Measure(new Size(1920, 1080));
            Rect rect = new Rect(0, 0, 1920, 1080);
            container.Arrange(rect);

            //Rect rect = new Rect(canvas.RenderSize);
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)rect.Right, (int)rect.Bottom, 96.0, 96.0, PixelFormats.Default);
            renderTargetBitmap.Render(container);

            BitmapEncoder bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            MemoryStream memoryStream = new MemoryStream();
            bitmapEncoder.Save(memoryStream);
            // Process 
            byte[] blob = (byte[])memoryStream.ToArray();
            BitmapSource x = (BitmapSource)((new ImageSourceConverter()).ConvertFrom(blob));
            memoryStream.Close();
            return blob;
        }
    }
}
