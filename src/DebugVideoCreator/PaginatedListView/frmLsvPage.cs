using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using DebugVideoCreator.PaginatedListView;

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
            lsvData.Height = pnlNavigate.Top - 100;
            pnlNRPP.Left = this.Width - pnlNRPP.Width - 30;
            LsvPageGlobVar.NRPP = Convert.ToInt32(nudNRPP.Value);

            LsvPageFunc.DbConnection();
            LsvPageGlobVar.Page = 1;
            LsvPageFunc.FillLsvData(LsvPageFunc.ExecSQLQry("Select * from tbl_Employee"), lsvData, 0);
            lblInfo.Text = "Record Shown: " + LsvPageGlobVar.RecStart + " to " + LsvPageGlobVar.RecEnd + "                      Page " + LsvPageGlobVar.Page + " of " + LsvPageGlobVar.TotalPages;
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



}
