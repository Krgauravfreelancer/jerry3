﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Windows.Markup;
using static System.Net.WebRequestMethods;


namespace DesignImagerNp.controls
{
    /// <summary>
    /// Interaction logic for DesignImager.xaml
    /// </summary>
    public partial class DesignImager : UserControl
    {
        public DesignImager()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Returns the randered image in a datatable. <br/>
        /// The datatable has the following format <br/>
        /// Colume 0: <br/>
        ///  - Name: "id" <br/>
        ///  - Datatype: Int <br/>
        ///  - Default value -1 <br/>
        /// Colume 1: <br/>
        ///  - Name: "imageData" <br/>
        ///  - Datatype: byte[] <br/>
        ///  - Contains byte array of the image <br/>
        /// </summary>
        /// <returns></returns>
        public DataTable GetImage()
        {
            if ( container.RenderSize.Width<=0 || container.RenderSize.Height <= 0 )
            {
                return null;
            }
            
            Rect rect = new Rect(container.RenderSize);
            
            // 96dpi means 100%
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right,
              (int)rect.Bottom, 96d, 96d, PixelFormats.Default);
            rtb.Render(container);

            //endcode as PNG
            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            //save to memory stream
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            pngEncoder.Save(ms);
            ms.Close();

            DataTable dataTable = InitDataTable();
            AddImage(dataTable, ms.ToArray());

            return dataTable;
        }

        /// <summary>
        /// Loads the xamls of individual element from datatable into the canvas. <br/>
        /// /// The datatable should have the following format <br/>
        /// Colume 0: <br/>
        ///  - Name: "id" <br/>
        ///  - Datatype: Int <br/>
        /// Colume 1: <br/>
        ///  - Name: "xaml" <br/>
        ///  - Datatype: string <br/>
        ///  - Contains valid xaml of UI element for a canvas <br/>
        /// </summary>
        /// <param name="dataTable">Datatable of designes xaml</param>
        /// 
        public void LoadDesign(DataTable dataTable)
        {
            container.Children.Clear();

            foreach (DataRow dataRow in dataTable.Rows)
            {
                AddElement((string)dataRow["xaml"]);
            }
        }

        private void AddElement(string xaml)
        {
            string canvasStart = @"<Canvas 
                                    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>";
            string canvasEnd = "</Canvas>";
            string wrapped = canvasStart + xaml + canvasEnd;

            StringReader stringReader = new StringReader(wrapped);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            Canvas canvas = (Canvas)XamlReader.Load(xmlReader);
            canvas.RenderSize = new Size(1920, 1080);
            if (canvas.Children.Count == 1)
            {
                UIElement element = canvas.Children[0];
                canvas.Children.RemoveAt(0);
                container.Children.Add(element);
            }
            else
            {
                for (int i = canvas.Children.Count - 1; i >= 0; i--)
                {
                    UIElement element = canvas.Children[i];
                    canvas.Children.RemoveAt(i);
                    container.Children.Add(element);
                }
            }


            //UIElement uIElement = canvas.Children[0];
            //canvas.Children.RemoveAt(0);
            //container.Children.Add(uIElement);
        }

        private DataTable InitDataTable()
        {
            DataTable dataTable = new DataTable();
            dataTable.Clear();
            dataTable.Columns.Add("id", typeof(int));
            dataTable.Columns.Add("imageData", typeof(Byte[]));

            return dataTable;
        }

        private void AddImage(DataTable dataTable, byte[] imageData)
        {
            DataRow dataRow = dataTable.NewRow();
            dataRow["id"] = -1;
            dataRow["imageData"] = imageData;
            dataTable.Rows.Add(dataRow);
        }
    }
}
