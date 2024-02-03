using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml;
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
            byte[] blob = (byte[])dataTable.Rows[0]["imageData"];
            BitmapSource x = (BitmapSource)((new ImageSourceConverter()).ConvertFrom(blob));
            imgDesign.Source = x;
            SaveToDataTable(blob);
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

            foreach(DataRow row in designTable.Rows)
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
    }
}
