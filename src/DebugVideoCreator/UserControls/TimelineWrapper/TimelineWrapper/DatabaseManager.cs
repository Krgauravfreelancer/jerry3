using Sqllite_Library.Business;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TimelineWrapper
{
    internal class DatabaseManager
    {
        public static void InitializeDatabase()
        {
            try
            {
                var message = DataManagerSqlLite.CreateDatabaseIfNotExist(false, true); // Lets keep the flag false for now
                MessageBox.Show(message + ", syncing lookup tables !!");
                SyncApp();
                SyncMedia();
                SyncScreen();
                SyncResolution();
                MessageBox.Show("lookup tables synced successfully !!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //this.Close();
            }
        }


        #region == Sync Functions ==

        public static void SyncApp()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("app_id", typeof(int));
                datatable.Columns.Add("app_name", typeof(string));
                datatable.Columns.Add("app_active", typeof(int));

                var row = datatable.NewRow();
                row["app_id"] = -1;
                row["app_name"] = "draft";
                row["app_active"] = 1;
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["app_id"] = -1;
                row2["app_name"] = "write";
                row2["app_active"] = 0;
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["app_id"] = -1;
                row3["app_name"] = "talk";
                row3["app_active"] = 0;
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["app_id"] = -1;
                row4["app_name"] = "admin";
                row4["app_active"] = 0;
                datatable.Rows.Add(row4);

                var row5 = datatable.NewRow();
                row5["app_id"] = -1;
                row5["app_name"] = "superadmin";
                row5["app_active"] = 0;
                datatable.Rows.Add(row5);

                var insertedIds = DataManagerSqlLite.SyncApp(datatable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void SyncMedia()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("media_id", typeof(int));
                datatable.Columns.Add("media_name", typeof(string));
                datatable.Columns.Add("media_color", typeof(string));

                var row = datatable.NewRow();
                row["media_id"] = -1;
                row["media_name"] = "image";
                row["media_color"] = "Tomato";
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["media_id"] = -1;
                row2["media_name"] = "video";
                row2["media_color"] = "Thistle";
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["media_id"] = -1;
                row3["media_name"] = "audio";
                row3["media_color"] = "Yellow";
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["media_id"] = -1;
                row4["media_name"] = "form";
                row4["media_color"] = "LightSalmon";
                datatable.Rows.Add(row4);

                var insertedIds = DataManagerSqlLite.SyncMedia(datatable);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static void SyncScreen()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("screen_id", typeof(int));
                datatable.Columns.Add("screen_name", typeof(string));
                datatable.Columns.Add("screen_color", typeof(string));

                var row = datatable.NewRow();
                row["screen_id"] = -1;
                row["screen_name"] = "intro";
                row["screen_color"] = "LightSalmon";
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["screen_id"] = -1;
                row2["screen_name"] = "prerequisites";
                row2["screen_color"] = "Azure";
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["screen_id"] = -1;
                row3["screen_name"] = "screen cast";
                row3["screen_color"] = "Beige";
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["screen_id"] = -1;
                row4["screen_name"] = "conclusion";
                row4["screen_color"] = "Aqua";
                datatable.Rows.Add(row4);

                var row5 = datatable.NewRow();
                row5["screen_id"] = -1;
                row5["screen_name"] = "next";
                row5["screen_color"] = "LightSteelBlue";
                datatable.Rows.Add(row5);

                var insertedIds = DataManagerSqlLite.SyncScreen(datatable);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void SyncResolution()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("resolution_id", typeof(int));
                datatable.Columns.Add("resolution_name", typeof(string));

                var row = datatable.NewRow();
                row["resolution_id"] = -1;
                row["resolution_name"] = "480px";
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["resolution_id"] = -1;
                row2["resolution_name"] = "720px";
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["resolution_id"] = -1;
                row3["resolution_name"] = "1080px";
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["resolution_id"] = -1;
                row4["resolution_name"] = "1280px";
                datatable.Rows.Add(row4);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

    }
}
