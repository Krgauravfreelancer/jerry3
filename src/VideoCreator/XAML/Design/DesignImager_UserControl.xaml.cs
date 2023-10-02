using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Sqllite_Library.Business;
using Sqllite_Library.Models;

namespace VideoCreator.XAML
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

        #region == Events ==
        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            // Get the Image as byte stream 
            DataTable dataTable = designImager.GetImage();

            // Process 
            byte[] blob = (byte[])dataTable.Rows[0]["imageData"];
            BitmapSource x = (BitmapSource)((new ImageSourceConverter()).ConvertFrom(blob));
            imgDesign.Source = x;
            SaveToDataTable(blob);
            UserConsent = true;
            var myWindow = Window.GetWindow(this);
            myWindow.Close();
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadDesign(1);
        }

        #endregion

        private void LoadDesign(int videoEvent = 1)
        {
            DataTable dataTable = new DataTable();
            dataTable.Clear();
            dataTable.Columns.Add("id", typeof(int));
            dataTable.Columns.Add("xaml", typeof(string));

            List<CBVDesign> cBVDesigns = DataManagerSqlLite.GetDesign(videoEvent);
            foreach (CBVDesign cBVDesign in cBVDesigns)
            {
                Console.WriteLine(cBVDesign.design_id);
                Console.WriteLine(cBVDesign.fk_design_videoevent);
                Console.WriteLine(cBVDesign.fk_design_screen);
                Console.WriteLine(cBVDesign.design_xml);
                Console.WriteLine(cBVDesign.design_createdate);
                Console.WriteLine(cBVDesign.design_modifydate);

                DataRow dataRow = dataTable.NewRow();
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

        private void SaveToDataTable(byte[] blob)
        {
            var newRow = dtVideoSegment.NewRow();
            newRow["videosegment_id"] = -1;
            newRow["fk_videosegment_videoevent"] = selectedVideoEvent;
            newRow["videosegment_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            newRow["videosegment_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            newRow["videosegment_media"] = blob;
            dtVideoSegment.Rows.Add(newRow);
        }
    }
}
