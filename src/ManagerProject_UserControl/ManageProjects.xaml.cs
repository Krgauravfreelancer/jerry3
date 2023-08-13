using System;
using System.Data;
using System.Windows;
using System.Windows.Forms;

namespace ManagerProject_UserControl
{
    public partial class ManageProjectsUserControl : System.Windows.Controls.UserControl
    {
        public DataTable dataTable = new DataTable();
        public DialogResult DialogResult;

        public ManageProjectsUserControl()
        {
            InitializeComponent();
            txtProjectName.MaxLength = 30;
            txtComments.MaxLength = 200;
            InitilizeTable();
        }

        public ManageProjectsUserControl(string project_name, int project_version, string project_comments, bool project_uploaded, bool project_archived, string project_date)
        {
            InitializeComponent();
            txtProjectName.MaxLength = 30;
            txtComments.MaxLength = 200;
            InitilizeTable();
            txtProjectName.Text = project_name;
            txtComments.Text = project_comments;
            txtVersion.Text = Convert.ToString(project_version);
            cbProjectUploaded.IsChecked = project_uploaded;
            cbProjectArchived.IsChecked = project_archived;
            dtProjectDate.SelectedDate = Convert.ToDateTime(project_date);
            btnAddProject.IsEnabled = false;
        }

        private void InitilizeTable()
        {
            dataTable.Columns.Add("id", typeof(int));
            dataTable.Columns.Add("project_name", typeof(string));
            dataTable.Columns.Add("project_version", typeof(int));
            dataTable.Columns.Add("project_comments", typeof(string));
            dataTable.Columns.Add("project_uploaded", typeof(bool));
            dataTable.Columns.Add("project_date", typeof(string));
            dataTable.Columns.Add("project_archived", typeof(bool));
            dataTable.Columns.Add("project_createdate", typeof(string));
            dataTable.Columns.Add("project_modifydate", typeof(string));
        }

        public void BtnPost_Click(object sender, RoutedEventArgs e)
        {
            var validationsError = ValidationsAddProject(txtProjectName.Text, txtVersion.Text);
            if (validationsError != string.Empty)
            {
                System.Windows.MessageBox.Show(validationsError, "Error", MessageBoxButton.OK);
                DialogResult = DialogResult.Cancel;
            }
            else
            {
                BuildDataTable();
                DialogResult = DialogResult.OK;
                var window = Window.GetWindow(this);
                window?.Close();
            }
        }

        public void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window?.Close();
        }

        private void BuildDataTable()
        {
            dataTable.Clear();
            var row = dataTable.NewRow();
            row["id"] = 1;
            row["project_name"] = txtProjectName.Text;
            row["project_version"] = Convert.ToInt32(txtVersion.Text);
            row["project_comments"] = txtComments.Text;
            row["project_uploaded"] = cbProjectUploaded.IsChecked.GetValueOrDefault();
            row["project_archived"] = cbProjectArchived.IsChecked.GetValueOrDefault();
            row["project_date"] = dtProjectDate.SelectedDate.HasValue ? dtProjectDate.SelectedDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";
            row["project_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            row["project_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            dataTable.Rows.Add(row);
        }

        public void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
        }

        private static string ValidationsAddProject(string ProjectNameText, string VersionText)
        {
            if (string.IsNullOrEmpty(ProjectNameText))
                return "Project Name cannot be empty";
            if (string.IsNullOrEmpty(VersionText))
                return "Version cannot be empty";
            else if (!int.TryParse(VersionText, out int version))
                return "Version should be Number";
            return string.Empty;
        }
    }
}
