using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
using Image = System.Drawing;
using System.Data;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System.ComponentModel.Design;

namespace wrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //
            // In real application this data will be filled from database. 
            //
            //DataTable dataTable = prepareData();

            // Load the design. 
            //designImager.LoadDesign(dataTable);
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            // Get the Image as byte stream 
            DataTable dataTable = designImager.GetImage();

            // Process 
            byte[] design = (byte[])dataTable.Rows[0]["imageData"];
            BitmapSource x = (BitmapSource)((new ImageSourceConverter()).ConvertFrom(design));
            imgDesign.Source = x;
        }


        private DataTable prepareData()
        {
            //
            // In real application this data will be filled from database. 
            //

            DataRow dataRow;

            DataTable dataTable = new DataTable();
            dataTable.Clear();
            dataTable.Columns.Add("id", typeof(int));
            dataTable.Columns.Add("xaml", typeof(string));

            // Create new
            dataRow = dataTable.NewRow();
            dataRow["id"] = -1;
            dataRow["xaml"] = "<Rectangle Fill = '#00FFFFFF' Stroke = '#FF0345BF' StrokeThickness = '2' Width = '50' Height = '50' Canvas.Left = '51.2' Canvas.Top = '34.8'/>";
            dataTable.Rows.Add(dataRow);

            dataRow = dataTable.NewRow();
            dataRow["id"] = -1;
            dataRow["xaml"] = "<Ellipse Fill = '#00FFFFFF' Stroke = '#FF0345BF' StrokeThickness = '2' Width = '50' Height = '50' Canvas.Left = '86.4' Canvas.Top = '127.6' />";
            dataTable.Rows.Add(dataRow);

            dataRow = dataTable.NewRow();
            dataRow["id"] = -1;
            dataRow["xaml"] = "<Canvas Tag = 'Arrow' xmlns = 'http://schemas.microsoft.com/winfx/2006/xaml/presentation' ><Line X1 = '115.2' Y1 = '195.6' X2 = '165.2' Y2 = '195.6' Stroke = '#FF0345BF' StrokeThickness = '20' Tag = 'Arrow' /><Line X1 = '165.2' Y1 = '195.6' X2 = '166.2' Y2 = '195.6' Stroke = '#FF0345BF' StrokeThickness = '50' StrokeEndLineCap = 'Triangle' Tag = 'Arrow' /></Canvas>";
            dataTable.Rows.Add(dataRow);

            dataRow = dataTable.NewRow();
            dataRow["id"] = -1;
            dataRow["xaml"] = "<Path Data = 'M40,50A50,25,0,1,1,65,50L25,75z' Fill = '#00FFFFFF' Stroke = '#FF0345BF' StrokeThickness = '3' Canvas.Left = '140.6' Canvas.Top = '75.6' />";
            dataTable.Rows.Add(dataRow);

            return dataTable;
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            DataTable dataTable = new DataTable();
            dataTable.Clear();
            dataTable.Columns.Add("id", typeof(int));
            dataTable.Columns.Add("xaml", typeof(string));

            List<CBVDesign> cBVDesigns = DataManagerSqlLite.GetDesign(1);
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
    }
}
