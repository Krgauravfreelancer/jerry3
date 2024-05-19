using System.Data.SQLite;

namespace Sqllite_Library.Models
{
    public class SqlConnectionModel
    {
        public SQLiteCommand SQLiteCommand { get; set; }
        public SQLiteConnection SQLiteConnection { get; set; }
        public SQLiteDataReader SQLiteDataReader { get; set; }

    }
}
