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
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using DebugVideoCreator.Models;

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
        //private CBVBackground selectedBGItem;
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

        public void AutofillSetup(AutofillEvent autofillEvent)
        {
            var bgImageXAML = GetBackgroundImageElement();
            designer.LoadDesign(LoadBackgroundFromDB(bgImageXAML));

            //proceed
            DataTable designElements = designer.GetDesign();
            designViewer.LoadDesign(designElements);
            FillDataTableForAutofill(autofillEvent, designElements);
        }

        private void FillDataTableForAutofill(AutofillEvent autofillEvent, DataTable designElements)
        {
            // background Image
            foreach (DataRow row in designElements.Rows)
            {
                var rowDesign = dataTableAdd.NewRow();

                rowDesign["design_id"] = row["id"];
                rowDesign["fk_design_videoevent"] = -1;
                rowDesign["fk_design_screen"] = 1;
                rowDesign["fk_design_background"] = 1;
                rowDesign["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowDesign["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowDesign["design_xml"] = row["xaml"];
                dataTableAdd.Rows.Add(rowDesign);
            }

            if (autofillEvent.AutofillType == AutofillEnumType.Title)
                AddTitle();
            if (autofillEvent.AutofillType == AutofillEnumType.Objective)
                AddObjective();
        }

        private void AddTitle()
        {
            var rowTitle = dataTableAdd.NewRow();

            rowTitle["design_id"] = -1;
            rowTitle["fk_design_videoevent"] = -1;
            rowTitle["fk_design_screen"] = 1;
            rowTitle["fk_design_background"] = 1;
            rowTitle["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            rowTitle["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            rowTitle["design_xml"] = GetTitleElement("Sample Video Title for the first Video element");
            dataTableAdd.Rows.Add(rowTitle);
        }

        private void AddObjective()
        {
            var rowHeading = dataTableAdd.NewRow();

            rowHeading["design_id"] = -1;
            rowHeading["fk_design_videoevent"] = -1;
            rowHeading["fk_design_screen"] = 1;
            rowHeading["fk_design_background"] = 1;
            rowHeading["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            rowHeading["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            rowHeading["design_xml"] = GetHeading($"objective heading is here -");
            dataTableAdd.Rows.Add(rowHeading);

            for (var i = 0; i < 3; i++)
            { 
                var rowCircle = dataTableAdd.NewRow();

                rowCircle["design_id"] = -1;
                rowCircle["fk_design_videoevent"] = -1;
                rowCircle["fk_design_screen"] = 1;
                rowCircle["fk_design_background"] = 1;
                rowCircle["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowCircle["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowCircle["design_xml"] = GetBulletCircle(i + 1);
                dataTableAdd.Rows.Add(rowCircle);


                var rowText = dataTableAdd.NewRow();

                rowText["design_id"] = -1;
                rowText["fk_design_videoevent"] = -1;
                rowText["fk_design_screen"] = 1;
                rowText["fk_design_background"] = 1;
                rowText["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowText["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowText["design_xml"] = GetBulletPoints($"Bullet point - {i + 1}", i + 1);
                dataTableAdd.Rows.Add(rowText);
            }

            
        }


        private string GetTitleElement(string Text)
        {
            var title = $"<TextBox BorderThickness=\"0,0,0,0\" Background=\"#00FFFFFF\" Foreground=\"#FFF0F8FF\" FontWeight = \"800\" FontSize=\"60\" Cursor=\"Arrow\" AllowDrop=\"False\" Focusable=\"False\" Canvas.Left=\"300\" Canvas.Top=\"450\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><TextBox.RenderTransform><RotateTransform Angle=\"0\" /></TextBox.RenderTransform>{Text.ToUpper()}</TextBox>";
            return title;
        }

        private string GetHeading(string HeadingText)
        {
            var top = 250;
            var title = $"<TextBox BorderThickness=\"0,0,0,0\" Background=\"#00FFFFFF\" Foreground=\"#FFF0F8FF\" FontWeight = \"800\" FontSize=\"60\" Cursor=\"Arrow\" AllowDrop=\"False\" Focusable=\"False\" Canvas.Left=\"350\" Canvas.Top=\"{top}\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><TextBox.RenderTransform><RotateTransform Angle=\"0\" /></TextBox.RenderTransform>{HeadingText.ToUpper()}</TextBox>";
            return title;
        }

        private string GetBulletPoints(string BulletText, int bulletNumber = 1)
        {
            var top = 250 + (bulletNumber*150);
            var title = $"<TextBox BorderThickness=\"0,0,0,0\" Background=\"#00FFFFFF\" Foreground=\"#FFF0F8FF\" FontSize=\"50\" Cursor=\"Arrow\" AllowDrop=\"False\" Focusable=\"False\" Canvas.Left=\"430\" Canvas.Top=\"{top}\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><TextBox.RenderTransform><RotateTransform Angle=\"0\" /></TextBox.RenderTransform>{BulletText}</TextBox>";
            return title;
            
        }

        private string GetBulletCircle(int bulletNumber = 1)
        {
            var top = 280 + (bulletNumber * 150);
            var circle = $"<Ellipse Fill=\"#00000000\" Stroke=\"#FFF0F8FF\" StrokeThickness=\"10\" StrokeDashArray=\"\" Width=\"20\" Height=\"20\" Opacity=\"1\" Canvas.Left=\"380\" Canvas.Top=\"{top}\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Ellipse.RenderTransform><RotateTransform Angle=\"0\" /></Ellipse.RenderTransform></Ellipse>";
            return circle;
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

        private string GetBackgroundImageElement(double defaultWidth = 1920)
        {
            var height = stackDesigner.ActualHeight;
            var width = stackDesigner.ActualWidth == 0 ? defaultWidth : stackDesigner.ActualWidth;
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
            cbUseBackground.IsEnabled = true;
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

        private void cbUseBackground_Checked(object sender, RoutedEventArgs e)
        {
            if (designer == null) return;
            DataTable designElements = designer.GetDesign();
            if (designElements.Rows.Count > 0)
            {
                foreach (DataRow row in designElements.Rows)
                {
                    if (row[1].ToString().StartsWith("<Image ") == false)
                    {
                        var bgImageXAML = GetBackgroundImageElement();
                        if (!string.IsNullOrEmpty(bgImageXAML))
                        {
                            DataRow dataRow = designElements.NewRow();
                            dataRow["id"] = 1;
                            dataRow["xaml"] = bgImageXAML;
                            designElements.Rows.InsertAt(dataRow, 0);
                            break;
                        }
                    }
                }
                designer.LoadDesign(designElements);

            }
            else
            {
                var bgImageXAML = GetBackgroundImageElement();
                designer.LoadDesign(LoadBackgroundFromDB(bgImageXAML));
            }
        }

        private void cbUseBackground_Unchecked(object sender, RoutedEventArgs e)
        {
            if (designer == null) return;
            DataTable designElements = designer.GetDesign();
            foreach (DataRow row in designElements.Rows)
            {
                if (row[1].ToString().StartsWith("<Image "))
                {
                    designElements.Rows.Remove(row);
                    break;
                }
            }


            designer.LoadDesign(designElements);
        }
    }
}
