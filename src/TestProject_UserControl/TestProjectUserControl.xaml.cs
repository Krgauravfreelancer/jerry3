using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Sqllite_Library.Business;


namespace TestProject_UserControl
{
    public partial class TestProjectUserControl : UserControl
    {
        public TestProjectUserControl()
        {
            InitializeComponent();
            LoadProjectDataGrid();
        }




        #region == Events ==

        private void BtnCleanRegistry_Click(object sender, RoutedEventArgs e)
        {
            var message = DataManagerSqlLite.ClearRegistryAndDeleteDB(); // Lets clean for testing purpose
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }



        #endregion == Events ==

        private void LoadProjectDataGrid()
        {
            var data = DataManagerSqlLite.GetProjects(false, true);
            //var dt = ToDataTable(data);
            datagrid.ItemsSource = data;
            datagrid.Visibility = Visibility.Visible;
        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        //private void RefreshOrLoadComboBoxes(EnumEntity entity = EnumEntity.ALL)
        //{
        //    if (entity == EnumEntity.ALL || entity == EnumEntity.MEDIA)
        //    {
        //        var data = DataManagerSqlLite.GetMedia();
        //        RefreshComboBoxes<CBVMedia>(cmbMedia, data, "media_name");
        //    }

        //    if (entity == EnumEntity.ALL || entity == EnumEntity.VIDEOEVENT)
        //    {
        //        var data = DataManagerSqlLite.GetVideoEvents(-1, false);
        //        RefreshComboBoxes<CBVVideoEvent>(cmbVideoEvent, data, "videoevent_id");
        //    }
        //}

        //private void RefreshComboBoxes<T>(System.Windows.Controls.ComboBox combo, List<T> source, string columnNameToShow)
        //{
        //    combo.SelectedItem = null;
        //    combo.DisplayMemberPath = columnNameToShow;
        //    combo.Items.Clear();
        //    foreach (var item in source)
        //    {
        //        combo.Items.Add(item);
        //    }
        //}

        //private void RefreshTempComboBoxes(System.Windows.Controls.ComboBox combo, Dictionary<int, string> source)
        //{
        //    combo.Items.Clear();
        //    combo.SelectedItem = null;
        //    foreach (var item in source)
        //    {
        //        combo.Items.Add(item.Value);
        //    }
        //}


    }
}