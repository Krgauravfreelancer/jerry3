using NotesRead_UserControl.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

namespace NotesRead_UserControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class NotesReadCtrl : UserControl//, INotifyPropertyChanged
    {
       private NotesReadViewModel viewModel;
        public NotesReadCtrl()
        {
            InitializeComponent();
            viewModel = new NotesReadViewModel();
            DataContext = viewModel;
        }
        public void SetNotesList(DataTable list)
        {
            viewModel.SetNoteItems(list);
        }

    //    public static readonly DependencyProperty NoteItemsSourceProperty =
    //DependencyProperty.Register("NoteItemsSource", typeof(IEnumerable<NoteItem>), typeof(NotesReadCtrl), new PropertyMetadata(null));

    //    public event PropertyChangedEventHandler PropertyChanged;

    //    public IEnumerable<NoteItem> NoteItemsSource
    //    {
    //        get { return (IEnumerable<NoteItem>)GetValue(NoteItemsSourceProperty); }
    //        set { SetValue(NoteItemsSourceProperty, value); }
    //    }
    }
}
