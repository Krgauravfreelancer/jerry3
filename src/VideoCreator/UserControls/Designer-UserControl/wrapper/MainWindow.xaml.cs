using System;
using System.Collections.Generic;
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
using System.IO;
using System.Data;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using wrapper;

namespace Designer_wrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private DataTable designElements = null;
        DBHelper dBHelper = null;
        public MainWindow()
        {
            InitializeComponent();

            dBHelper = new DBHelper();
            dBHelper.Init(); // Creates database 

            shapeBar.Visibility = Visibility.Hidden;
            propertyWindow.Visibility = Visibility.Hidden;
            designer.Visibility = Visibility.Hidden;

            designer.propertyWindow = propertyWindow;
            designer.shapeBar = shapeBar;

            designViewer.LoadDesign(LoadFromDB());
            designViewer.Visibility = Visibility.Visible;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            designer.Visibility = Visibility.Hidden;
            designViewer.Visibility = Visibility.Visible;

            DataTable designElements = designer.GetDesign();
            designViewer.LoadDesign(designElements);
            addToDatabase(designElements);
        }

        private void btnManageDesigner_Click(object sender, RoutedEventArgs e)
        {
            // This can come from database
            
            designer.LoadDesign(LoadFromDB());

            designer.Visibility = Visibility.Visible;
            designViewer.Visibility = Visibility.Hidden;
        }


        private void addToDatabase(DataTable dtElements)
        {
            DataTable dtNew = createDesignDbDataTable();
            DataTable dtUpdate = createDesignDbDataTable();

            foreach (DataRow row in dtElements.Rows)
            {
                DataRow rowDesign;

                if (-1 == (int)row["id"])
                {
                    rowDesign = dtNew.NewRow();
                }
                else
                {
                    rowDesign = dtUpdate.NewRow();
                }

                rowDesign["design_id"] = row["id"];
                rowDesign["fk_design_videoevent"] = 1;
                rowDesign["fk_design_screen"] = 1;
                rowDesign["design_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                rowDesign["design_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                rowDesign["design_xml"] = row["xaml"];

                if (-1 == (int)row["id"])
                {
                    dtNew.Rows.Add(rowDesign);
                }
                else
                {
                    dtUpdate.Rows.Add(rowDesign);
                }
            }

            if (dtNew.Rows.Count > 0)
            {
                DataManagerSqlLite.InsertRowsToDesign(dtNew);
            }

            if (dtUpdate.Rows.Count > 0)
            {
                DataManagerSqlLite.UpdateRowsToDesign(dtUpdate);
            }
        }

        private DataTable createDesignDbDataTable()
        {
            var dtDesign = new DataTable();
            dtDesign.Columns.Add("design_id", typeof(int));
            dtDesign.Columns.Add("fk_design_videoevent", typeof(int));
            dtDesign.Columns.Add("fk_design_screen", typeof(int));
            dtDesign.Columns.Add("design_xml", typeof(string));
            dtDesign.Columns.Add("design_createdate", typeof(string));
            dtDesign.Columns.Add("design_modifydate", typeof(string));

            return dtDesign;
        }

        private DataTable LoadFromDB()
        {
            DataTable dataTable = designer.InitDataTable();

            dataTable.Clear();

            List<CBVDesign> cBVDesigns = DataManagerSqlLite.GetDesign(1);
            foreach (CBVDesign cBVDesign in cBVDesigns)
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow["id"] = cBVDesign.design_id;
                dataRow["xaml"] = cBVDesign.design_xml;
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }
    }
}
