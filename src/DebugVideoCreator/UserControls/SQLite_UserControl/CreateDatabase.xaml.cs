using System.Windows.Controls;

namespace Sqllite_Library
{
    public partial class CreateDatabaseUserControl : UserControl
    {
        public CreateDatabaseUserControl()
        {
            InitializeComponent();
        }

        //public void BtnCreateDB_Click(object sender, RoutedEventArgs e)
        //{
        //    lblFileStatus.Content = DataManagerSqlLite.CreateDatabaseIfNotExist(txtFileName.Text);
        //}

        //public void BtnAddProject_Click(object sender, RoutedEventArgs e)
        //{
        //    DataManagerSqlLite.AddProject(txtVersion.Text, txtProjectName.Text, txtComments.Text);
        //}

        //public void BtnRefresh_Click(object sender, RoutedEventArgs e)
        //{
        //    DataManagerSqlLite.RefreshProjects(cmbProjects);
        //}
    }
}
