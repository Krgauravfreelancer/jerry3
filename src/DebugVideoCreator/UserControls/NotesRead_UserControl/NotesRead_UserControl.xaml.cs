using NotesRead_UserControl.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace NotesRead_UserControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class NotesRead_UserControl : UserControl, INotifyPropertyChanged
    {
        private NotesReadViewModel viewModel;
        public NotesRead_UserControl()
        {
            InitializeComponent();
            viewModel = new NotesReadViewModel();
            DataContext = viewModel;
        }
        public void SetNotesList(DataTable list)
        {
            viewModel.SetNoteItems(list);
        }

        public static readonly DependencyProperty NoteItemsSourceProperty = DependencyProperty.Register("NoteItemsSource", typeof(IEnumerable<NoteItem>), typeof(NotesRead_UserControl), new PropertyMetadata(null));

        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<NoteItem> NoteItemsSource
        {
            get { return (IEnumerable<NoteItem>)GetValue(NoteItemsSourceProperty); }
            set { SetValue(NoteItemsSourceProperty, value); }
        }

        public DataTable GetUpdatedViewTable()
        {
            var viewDataTable = TableViewBuilder.BuildNotesDataTableView();
            foreach (var item in viewModel.NoteItems)
            {
                Console.WriteLine(item.Item.ToString());
                viewDataTable = TableViewBuilder.AddNoteRowView(viewDataTable, item.Id, item.Item, item.Index);
            }
            return viewDataTable;
        }
    }
}
