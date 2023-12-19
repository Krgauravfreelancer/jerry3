using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;


namespace VideoCreator.PaginatedListView
{
    
    public partial class frmLsvPage : Form
    {
       public frmLsvPage()
        {
            InitializeComponent();
            
        }

       private void frmLsvPage_Load(object sender, EventArgs e)
       {
           lsvData.Width = this.Width - 30;
           lsvData.Height = pnlNavigate.Top-100;
           pnlNRPP.Left = this.Width - pnlNRPP.Width-30;
           LsvPageGlobVar.NRPP = Convert.ToInt32(nudNRPP.Value);
           
           LsvPageFunc.DbConnection();
           LsvPageGlobVar.Page = 1;
           LsvPageFunc.FillLsvData(LsvPageFunc.ExecSQLQry("Select * from tbl_Employee"),lsvData,0);
           lblInfo.Text="Record Shown: " + LsvPageGlobVar.RecStart + " to "+LsvPageGlobVar.RecEnd+ "                      Page "+LsvPageGlobVar.Page+" of " + LsvPageGlobVar.TotalPages;
       }

       private void frmLsvPage_Resize(object sender, EventArgs e)
       {
           lsvData.Width = this.Width - 30;
           lsvData.Height = pnlNavigate.Top - 100;
           pnlNRPP.Left = this.Width - pnlNRPP.Width - 30;
       }

       private void btnFirst_Click(object sender, EventArgs e)
       {
           LsvPageGlobVar.Page = 1;
           LsvPageFunc.FillLsvData(LsvPageFunc.ExecSQLQry("Select * from tbl_Employee"), lsvData, 0);
           lblInfo.Text = "Record Shown: " + LsvPageGlobVar.RecStart + " to " + LsvPageGlobVar.RecEnd + "                      Page " + LsvPageGlobVar.Page + " of " + LsvPageGlobVar.TotalPages;
       }

       private void btnLast_Click(object sender, EventArgs e)
       {
           LsvPageGlobVar.Page = LsvPageGlobVar.TotalPages;
           LsvPageFunc.FillLsvData(LsvPageFunc.ExecSQLQry("Select * from tbl_Employee"), lsvData, 0);
           lblInfo.Text = "Record Shown: " + LsvPageGlobVar.RecStart + " to " + LsvPageGlobVar.RecEnd + "                      Page " + LsvPageGlobVar.Page + " of " + LsvPageGlobVar.TotalPages;
       }

       private void btnNext_Click(object sender, EventArgs e)
       {
           if (LsvPageGlobVar.Page < LsvPageGlobVar.TotalPages)
           {
               LsvPageGlobVar.Page++;
           }
           LsvPageFunc.FillLsvData(LsvPageFunc.ExecSQLQry("Select * from tbl_Employee"), lsvData, 0);
           lblInfo.Text = "Record Shown: " + LsvPageGlobVar.RecStart + " to " + LsvPageGlobVar.RecEnd + "                      Page " + LsvPageGlobVar.Page + " of " + LsvPageGlobVar.TotalPages;
       }

       private void btnPrev_Click(object sender, EventArgs e)
       {
           if (LsvPageGlobVar.Page > 1)
           {
               LsvPageGlobVar.Page--;
           }
           LsvPageFunc.FillLsvData(LsvPageFunc.ExecSQLQry("Select * from tbl_Employee"), lsvData, 0);
           lblInfo.Text = "Record Shown: " + LsvPageGlobVar.RecStart + " to " + LsvPageGlobVar.RecEnd + "                      Page " + LsvPageGlobVar.Page + " of " + LsvPageGlobVar.TotalPages;
       }

       private void nudNRPP_ValueChanged(object sender, EventArgs e)
       {
           if (nudNRPP.Value != 0)
           {
               LsvPageGlobVar.NRPP = Convert.ToInt32(nudNRPP.Value);
           }
           else
           {
               nudNRPP.Value = 1;
           }
           LsvPageFunc.FillLsvData(LsvPageFunc.ExecSQLQry("Select * from tbl_Employee"), lsvData, 0);
           lblInfo.Text = "Record Shown: " + LsvPageGlobVar.RecStart + " to " + LsvPageGlobVar.RecEnd + "                      Page " + LsvPageGlobVar.Page + " of " + LsvPageGlobVar.TotalPages;
           
       }
    }

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
    public class LsvPageFunc
    {
        public static bool DbConnection()
        {
            bool functionReturnValue = false;

            try
            {
                 LsvPageGlobVar.ConStr = "Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Data Source=./PaginatedListView/Database.mdb";
                //LsvPageGlobVar.ConStr ="Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Data Source=|DataDirectory|\data.mdb";      
                
                OleDbConnection sqlCon = new OleDbConnection();
                sqlCon.ConnectionString = LsvPageGlobVar.ConStr;
                sqlCon.Open();
                functionReturnValue = true;
                sqlCon.Close();
            }
            catch (Exception ex)
            {
                functionReturnValue = false;
                MessageBox.Show("Error : " + ex.ToString());
            }
            return functionReturnValue;
        }

        //Function to execute all queires
        public static DataTable ExecSQLQry(string SQLQuery)
        {
            try
            {
                OleDbConnection sqlCon = new OleDbConnection(LsvPageGlobVar.ConStr);
                OleDbDataAdapter sqlDA = new OleDbDataAdapter(SQLQuery, sqlCon);
                OleDbCommandBuilder sqlCB = new OleDbCommandBuilder(sqlDA);

                LsvPageGlobVar.sqlDataTable.Reset();
                sqlDA.Fill(LsvPageGlobVar.sqlDataTable);
            }
            catch (Exception ex)
            {

                MessageBox.Show("Error : " + ex.ToString());

            }
            return LsvPageGlobVar.sqlDataTable;
        }


        public static void FillLsvData(DataTable sqlData, ListView lvList, int imageID)
        {
            //Load the table data in the listview
            int i = 0;
            int j = 0;
            int m = 0;
            int xsize;
            

            lvList.Clear();
            // for Adding Column Names from the datatable

            LsvPageGlobVar.TotalRec = sqlData.Rows.Count;

            //try
            //{
                LsvPageGlobVar.TotalPages = LsvPageGlobVar.TotalRec / LsvPageGlobVar.NRPP;

                if (LsvPageGlobVar.TotalRec % LsvPageGlobVar.NRPP > 0)
                {
                    LsvPageGlobVar.TotalPages++;
                }
            //}

            //catch(DivideByZeroException e)
            //{
            //    MessageBox.Show("Error : " + e.ToString());
            //}
            

            for (i = 0; i <= sqlData.Columns.Count - 1; i++)
            {
                lvList.Columns.Add(sqlData.Columns[i].ColumnName);
            }

            //for adding records to the listview from datatable
            int l, k;
            
            l = (LsvPageGlobVar.Page - 1) * LsvPageGlobVar.NRPP;
            k = ((LsvPageGlobVar.Page) * LsvPageGlobVar.NRPP);
            
            LsvPageGlobVar.RecStart = l + 1;
            if (k > LsvPageGlobVar.TotalRec)
            {
                LsvPageGlobVar.RecEnd = LsvPageGlobVar.TotalRec;
            }
            else
            {
                LsvPageGlobVar.RecEnd = k;
            }

            for (; l < k; l++)
            {
                if (l >= LsvPageGlobVar.TotalRec)
                {
                    break;
                }

                lvList.Items.Add(sqlData.Rows[l][0].ToString(), imageID);
               
                for (j = 1; j <= sqlData.Columns.Count - 1; j++)
                {
                    if (!System.Convert.IsDBNull(sqlData.Rows[l][j]))
                    {
                        lvList.Items[m].SubItems.Add(sqlData.Rows[l][j].ToString());
                        
                    }
                    else
                    {
                        lvList.Items[m].SubItems.Add("");
                        
                    } 
                }
                m++;
            }


            //for rearrange the column size
            for (i = 0; i <= sqlData.Columns.Count - 1; i++)
            {
                xsize = lvList.Width / sqlData.Columns.Count - 8;

                if (xsize > 1450)
                {
                    lvList.Columns[i].Width = xsize;
                    lvList.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                }

                else
                {
                    lvList.Columns[i].Width = 2000;
                    lvList.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                }



            }
        }
    }
}
