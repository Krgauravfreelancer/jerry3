using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace VideoToImage_UserControl
{
    /// <summary>
    /// Interaction logic for VideoToImage_UserControl.xaml
    /// </summary>
    public partial class VideoToImage_UserControl : UserControl
    {
        public VideoToImage_UserControl()
        {
            InitializeComponent();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void btnCreateImage_Click(object sender, EventArgs e)
        {

        }
    }
}
