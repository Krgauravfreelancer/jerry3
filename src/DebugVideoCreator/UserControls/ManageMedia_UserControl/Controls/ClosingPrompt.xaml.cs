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

namespace ManageMedia_UserControl.Controls
{
    /// <summary>
    /// Interaction logic for SavePrompt.xaml
    /// </summary>
    public partial class ClosingPrompt : UserControl
    {
        public ClosingPrompt()
        {
            InitializeComponent();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SaveClicked != null)
            {
                SaveClicked(this, EventArgs.Empty);
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CloseClicked != null)
            {
                CloseClicked(this, EventArgs.Empty);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CancelClicked != null)
            {
                CancelClicked(this, EventArgs.Empty);
            }
        }

        public event EventHandler<EventArgs> SaveClicked;

        public event EventHandler<EventArgs> CloseClicked;

        public event EventHandler<EventArgs> CancelClicked;
    }
}
