using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugVideoCreator.PaginatedListView
{
    public class LsvPageGlobVar
    {
        public static string ConStr;
        public static DataTable sqlDataTable = new DataTable();
        public static int TotalRec; //Variable for getting Total Records of the Table
        public static int NRPP; //Variable for Setting the Number of Recrods per listiview page
        public static int Page; //List View Page for Navigate or move
        public static int TotalPages; //Varibale for Counting Total Pages.
        public static int RecStart; //Variable for Getting Every Page Starting Record Index
        public static int RecEnd; //Variable for Getting Every Page End Record Index
    }
}
