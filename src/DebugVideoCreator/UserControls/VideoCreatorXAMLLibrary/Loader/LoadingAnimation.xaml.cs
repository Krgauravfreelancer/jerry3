using System.Windows.Controls;

namespace VideoCreator.Loader
{
    /// <summary>
    /// Interaction logic for LoadingAnimation.xaml
    /// </summary>
    public partial class LoadingAnimation : UserControl
    {
        public LoadingAnimation()
        {
            InitializeComponent();
        }

        public void setTextBlockMessage(string message = "")
        {
            txtBlock.Text = string.IsNullOrEmpty(message) ? "" : message;
        }
    }
}
