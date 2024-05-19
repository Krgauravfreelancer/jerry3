using Sqllite_Library.Business;
using Sqllite_Library.Models;
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
using System.Windows.Shapes;

namespace TimelineWrapper.Windows
{
    /// <summary>
    /// Interaction logic for ScreenRecorderWrapperWindow.xaml
    /// </summary>
    public partial class ScreenRecorderWrapperWindow : Window
    {
        public ScreenRecorderWrapperWindow()
        {
            InitializeComponent();
        }



        #region ListBox and ComboBox

      

        private void ProjectCmbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            int SelectedIndex = ((ComboBox)sender).SelectedIndex + 1;

            var data = DataManagerSqlLite.GetVideoEvents(SelectedIndex, true);
            //FillListBoxVideoEvent(data);
        }

        #endregion
    }
}
