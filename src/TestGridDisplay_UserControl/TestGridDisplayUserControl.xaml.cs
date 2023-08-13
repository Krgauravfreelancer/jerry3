using System;
using System.Collections.Generic;
using System.Windows;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Data;

namespace TestGridDisplay_UserControl
{
    public partial class TestGridDisplayUserControl : UserControl
    {
        public TestGridDisplayUserControl()
        {
            InitializeComponent();
            RefreshOrLoadComboBoxes();
        }

        #region == Public Functions ==
        public void SelectionChanged(EnumEntity entity, int projectId = -1)
        {
            switch (entity)
            {
                case EnumEntity.PROJECT:
                    CreateAndSetGrid(DataManagerSqlLite.GetProjects(true, false));
                    break;
                case EnumEntity.VIDEOEVENT:
                    CreateAndSetGrid(DataManagerSqlLite.GetVideoEvents(projectId, true));
                    break;
                case EnumEntity.SCREEN:
                    CreateAndSetGrid(DataManagerSqlLite.GetScreens());
                    break;
                case EnumEntity.APP:
                    CreateAndSetGrid(DataManagerSqlLite.GetApp());
                    break;
                case EnumEntity.MEDIA:
                    CreateAndSetGrid(DataManagerSqlLite.GetMedia());
                    break;
                case EnumEntity.RESOLUTION:
                    CreateAndSetGrid(DataManagerSqlLite.GetResolution());
                    break;
                case EnumEntity.AUDIO:
                    CreateAndSetGrid(DataManagerSqlLite.GetAudio());
                    break;
                case EnumEntity.DESIGN:
                    CreateAndSetGrid(DataManagerSqlLite.GetDesign());
                    break;
                case EnumEntity.VIDEOSEGMENT:
                    CreateAndSetGrid(DataManagerSqlLite.GetVideoSegment());
                    break;
                case EnumEntity.NOTES:
                    CreateAndSetGrid(DataManagerSqlLite.GetNotes());
                    break;
                case EnumEntity.HLSTS:
                    CreateAndSetGrid(DataManagerSqlLite.GetHlsts());
                    break;
                case EnumEntity.STREAMTS:
                    CreateAndSetGrid(DataManagerSqlLite.GetStreamts());
                    break;
                default:
                    MessageBox.Show("No data present for the selected option", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }

        //public void SetSelectedProjectIdText(string text)
        //{
        //    lblSelectedProjectId.Content = text;
        //}

        #endregion

        #region == Events ==

        private void CMBTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var entity = ((ComboTableItem)cmbTable.SelectedItem)?.Index;
            SelectionChanged(entity.Value);
        }


       
        #endregion

        #region == Grid Functions ==

        private void CreateAndSetGrid<T>(List<T> data)
        {
            dynamicGrid.Children.Clear();
            dynamicGrid.RowDefinitions.Clear();
            dynamicGrid.ColumnDefinitions.Clear();
            if (data == null || data.Count == 0)
            {
                //MessageBox.Show("No data present for the selected option", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }


            var Fields = data[0].GetType().GetProperties().Select(x => x.Name).ToList();

            // Header Section
            int row = 0;
            int column = 0;
            foreach (var field in Fields)
            {
                var gridRowHeader = new RowDefinition
                {
                    Height = new GridLength(2, GridUnitType.Star)
                };
                dynamicGrid.RowDefinitions.Add(gridRowHeader);
                // Add header columns based upon the fields in data
                var gridColheader = new ColumnDefinition
                {
                    Width = new GridLength(140)
                };
                dynamicGrid.ColumnDefinitions.Add(gridColheader);
                // Add Header Text
                var txtBlock = new TextBlock
                {
                    Text = field.Replace("_", " "),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Colors.Blue),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.WrapWithOverflow,
                    TextAlignment = TextAlignment.Center
                };

                Grid.SetRow(txtBlock, row);
                Grid.SetColumn(txtBlock, column);
                dynamicGrid.Children.Add(txtBlock);

                var border = new Border
                {
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Colors.Blue)
                };
                Grid.SetRow(border, row);
                Grid.SetColumn(border, column);
                dynamicGrid.Children.Add(border);
                
                column++;
            }

            row++;
            foreach (var item in data)
            {
                var gridRow = new RowDefinition
                {
                    Height = new GridLength(1, GridUnitType.Star),
                    MaxHeight = 40
                };
                dynamicGrid.RowDefinitions.Add(gridRow);
                column = 0;
                foreach (var field in Fields)
                {
                    var rowData = data[row - 1];
                    var myObject = rowData.GetType().GetProperty(field)?.GetValue(rowData, null);
                    string textValue = Convert.ToString(myObject);
                    if (myObject.GetType().IsGenericType && myObject is IEnumerable)
                    {
                        var list = myObject as ICollection;
                        textValue = list == null ? "0 Records" : $"{list.Count} Records";
                    }

                    var textBlock = new TextBlock
                    {
                        Text = textValue,
                        FontSize = 12,
                        Foreground = new SolidColorBrush(Colors.Black),
                        FontWeight = FontWeights.Regular,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    };
                    Grid.SetRow(textBlock, row);
                    Grid.SetColumn(textBlock, column);
                    dynamicGrid.Children.Add(textBlock);

                    var border = new Border
                    {
                        BorderThickness = new Thickness(0.5),
                        BorderBrush = new SolidColorBrush(Colors.Blue)
                    };
                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, column);
                    dynamicGrid.Children.Add(border);
                    column++;
                }
                row++;
            }
        }

        #endregion

        private void RefreshOrLoadComboBoxes()
        {
            var data = new List<ComboTableItem>
            {
                new ComboTableItem(EnumEntity.PROJECT, "cbv_project"),
                new ComboTableItem(EnumEntity.VIDEOEVENT, "cbv_videoevent"),
                new ComboTableItem(EnumEntity.SCREEN, "cbv_screen"),
                new ComboTableItem(EnumEntity.APP, "cbv_app"),
                new ComboTableItem(EnumEntity.MEDIA, "cbv_media"),
                new ComboTableItem(EnumEntity.DESIGN, "cbv_design"),
                new ComboTableItem(EnumEntity.AUDIO, "cbv_audio"),
                new ComboTableItem(EnumEntity.VIDEOSEGMENT, "cbv_videosegment"),
                new ComboTableItem(EnumEntity.NOTES, "cbv_notes"),
                new ComboTableItem(EnumEntity.HLSTS, "cbv_hlsts"),
                new ComboTableItem(EnumEntity.RESOLUTION, "cbv_resolution"),
                new ComboTableItem(EnumEntity.STREAMTS, "cbv_streamts"),
                //new ComboTableItem(EnumEntity.HISTORY, "cbv_history")
            };

            RefreshTempComboBoxes(cmbTable, data);
        }

        private void RefreshTempComboBoxes(ComboBox combo, List<ComboTableItem> source)
        {
            combo.SelectedItem = null;
            combo.DisplayMemberPath = "TableName";
            combo.Items.Clear();
            foreach (var item in source)
            {
                combo.Items.Add(item);
            }
        }

        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();
            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }
    }

    public class ComboTableItem
    {
        public EnumEntity Index { get; set; }
        public string TableName { get; set; }

        public ComboTableItem(EnumEntity i, string n)
        {
            this.Index = i;
            this.TableName = n;
        }

    }
}