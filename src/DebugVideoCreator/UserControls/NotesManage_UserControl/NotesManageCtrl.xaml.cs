using NotesManage_UserControl.Models;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace NotesManage_UserControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class NotesManageCtrl : UserControl
    {
        //private bool isSavedClicked = false;
        private DataTable dtable;
        private NotesManageViewModel viewModel;
        public NotesManageCtrl()
        {
            InitializeComponent();
            viewModel = new NotesManageViewModel();
            DataContext = viewModel;
        }
        public void LoadItemsForEdit(DataTable dt)
        {
            viewModel.LoadItems(dt);
            dgList.ItemsSource = viewModel.GetAllNotes();
        }

        public DataTable GetSavedData()
        {
            return dtable;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            dtable = viewModel.SaveItems();

            //Capture above table on parent form Closing event via GetSavedData()
            var myWindow = Window.GetWindow(this);
            myWindow.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            var myWindow = Window.GetWindow(this);
            myWindow.Close();
        }

        private void btnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            var currItem = (NoteItem)dgList.SelectedItem;
            var allNotes = viewModel.GetAllNotes();
            int idx = allNotes.IndexOf(currItem);
            if (idx != 0 && idx != -1)
            {
                int idxOfPrevious = idx - 1;
                allNotes.RemoveAt(idx);
                allNotes.Insert(idxOfPrevious, currItem);
                viewModel.SetNoteItems(allNotes);
                dgList.SelectedItem = currItem;
            }
        }

        private void btnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            var currItem = (NoteItem)dgList.SelectedItem;
            var allNotes = viewModel.GetAllNotes();
            int idx = allNotes.IndexOf(currItem);
            if (idx != allNotes.Count - 1 && idx != -1)
            {
                int idxOfNext = idx + 1;
                allNotes.RemoveAt(idx);
                allNotes.Insert(idxOfNext, currItem);
                viewModel.SetNoteItems(allNotes);
                dgList.SelectedItem = currItem;
            }
        }
    }
}
