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

namespace Designer_UserControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DesignerUserControl : System.Windows.Controls.UserControl
    {
        //private DataTable designElements = null;
        public DataTable dataTableAdd;
        public DataTable dataTableUpdate; // Will come later
        public bool UserConsent = false;
        private readonly int selectedProjectId;
        private readonly List<CBVBackground> BackgroundImagesData;
        private bool toggleFlag = false;
        private CBVBackground selectedBGItem;
        public DesignerUserControl(int _selectedProjectId, string _backgroundDatastring)
        {
            InitializeComponent();
            selectedProjectId = _selectedProjectId;
            BackgroundImagesData = JsonConvert.DeserializeObject<List<CBVBackground>>(_backgroundDatastring);
            LoadComboBoxesBackground();

            dataTableAdd = createDesignDbDataTable();
            dataTableUpdate = createDesignDbDataTable();

            shapeBar.Visibility = Visibility.Hidden;
            propertyWindow.Visibility = Visibility.Hidden;
            designer.Visibility = Visibility.Hidden;

            designer.propertyWindow = propertyWindow;
            designer.shapeBar = shapeBar;
            designViewer.Visibility = Visibility.Visible;
            btnSave.IsEnabled = false;
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

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            designer.Visibility = Visibility.Hidden;
            designViewer.Visibility = Visibility.Visible;

            DataTable designElements = designer.GetDesign();
            designViewer.LoadDesign(designElements);

            if (designElements.Rows.Count < 1)
            {
                System.Windows.MessageBox.Show("Nothing to save, please create some design", "Information", (MessageBoxButton)MessageBoxButtons.OK, (MessageBoxImage)MessageBoxIcon.Error);
                return;
            }

            AddToDatabase(designElements);
            UserConsent = true;
            var myWindow = Window.GetWindow(this);
            myWindow.Close();
        }

        private void BtnToggleDesigner_Click(object sender, RoutedEventArgs e)
        {
            // This can come from database
            // designer.LoadDesign(LoadFromDB());

            if(!toggleFlag)
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
        }

        private void cmbBackground_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedBGItem = ((CBVBackground)cmbBackground?.SelectedItem);
            
            // Process 
            byte[] blob = (byte[])selectedBGItem.background_media;
            BitmapSource x = (BitmapSource)((new ImageSourceConverter()).ConvertFrom(blob));
            imgBackground.Source = x;
            btnSave.IsEnabled = selectedBGItem?.background_id >= 0;
        }

        private void AddToDatabase(DataTable dtElements)
        {
            // For now only Add is supported.
            foreach (DataRow row in dtElements.Rows)
            {
                var rowDesign = (int)row["id"]  == - 1  ? dataTableAdd.NewRow(): dataTableUpdate.NewRow();

                rowDesign["design_id"] = row["id"];
                rowDesign["fk_design_videoevent"] = -1;
                rowDesign["fk_design_screen"] = 1;
                rowDesign["fk_design_background"] = selectedBGItem.background_id;
                rowDesign["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowDesign["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowDesign["design_xml"] = row["xaml"];

                if (-1 == (int)row["id"])
                {
                    dataTableAdd.Rows.Add(rowDesign);
                }
                else
                {
                    dataTableUpdate.Rows.Add(rowDesign);
                }
            }
        }

        private void LoadComboBoxesBackground()
        {

            cmbBackground.SelectedItem = null;
            cmbBackground.DisplayMemberPath = "background_image_name";
            cmbBackground.Items.Clear();
            int i = 1;
            foreach (var item in BackgroundImagesData)
            {
                item.background_image_name = "background-" + i++;
                cmbBackground.Items.Add(item);
            }
        }

        //private DataTable LoadFromDB()
        //{
        //    var dataTable = designer.InitDataTable();
        //    dataTable.Clear();
        //    List<CBVDesign> cBVDesigns = DataManagerSqlLite.GetDesign(1);
        //    foreach (CBVDesign cBVDesign in cBVDesigns)
        //    {
        //        DataRow dataRow = dataTable.NewRow();
        //        dataRow["id"] = cBVDesign.design_id;
        //        dataRow["xaml"] = cBVDesign.design_xml;
        //        dataTable.Rows.Add(dataRow);
        //    }

        //    return dataTable;
        //}
    }
}
