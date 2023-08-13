using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;
using Sqllite_Library.Models;
using System.Data;
using Sqllite_Library.Helpers;
using System.Windows;
using System.Data.Entity.Infrastructure;


namespace Sqllite_Library.Data
{
    public static class SqlLiteData
    {

        #region == Database Methods ==
        public static void CreateDatabaseIfNotExist(bool encryptFlag, bool canCreateRegistryIfNotExists)
        {
            try
            {
                // Open Database connection 
                var completeFileName = RegisteryHelper.GetFileName();
                if (completeFileName == null)

                {
                    if (canCreateRegistryIfNotExists)
                    {
                        RegisteryHelper.StoreFileName();
                        completeFileName = RegisteryHelper.GetFileName();
                    }
                    else
                        throw new Exception("Registry keys not present, please run the installer first !!");
                }
                string connectionString = string.Format("Data Source={0};Version=3;", completeFileName);
                //if (encryptFlag) connectionString += "Password=mypassword";
                var sqlCon = new SQLiteConnection(connectionString);
                //if (encryptFlag) sqlCon.SetPassword("mypassword");
                sqlCon.Open();
                CreateAllTables(sqlCon);
                //if (encryptFlag) sqlCon.SetPassword("mypassword");
                // else sqlCon.SetPassword(); // We need to decrypt only if the DB is encrypted.
                // Close connection
                sqlCon?.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool IsDbCreated()
        {
            bool fileExists;
            try
            {
                string fileName = RegisteryHelper.GetFileName();
                fileExists = File.Exists(fileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return fileExists;
        }

        public static void DeleteDB()
        {
            try
            {
                // First delete DB recursively
                var filename = RegisteryHelper.GetFileName();
                File.Delete(filename);
                MessageBox.Show($"DB Deleted Succesfully !!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // MessageBox.Show($"Error in DB deletion - {ex.Message}");
                throw ex;
            }
        }

        #region == Create Table Region ==
        private static void CreateAllTables(SQLiteConnection sqlCon)
        {
            CreateAppTable(sqlCon);
            CreateMediaTable(sqlCon);
            CreateScreenTable(sqlCon);
            CreateProjectTable(sqlCon);
            CreateVideoEventTable(sqlCon);
            CreateVideoSegmentTable(sqlCon);
            CreateAudioTable(sqlCon);
            CreateNotesTable(sqlCon);
            CreateDesignTable(sqlCon);
            CreateHlstsTable(sqlCon);
            CreateResolutionTable(sqlCon);
            CreateStreamtsTable(sqlCon);
        }
        private static void CreateAppTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_app' (
                'app_id' INTEGER NOT NULL  DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'app_name' TEXT(5) NOT NULL DEFAULT 'Write',
                'app_active' INTEGER(1) NOT NULL  DEFAULT 0,
                UNIQUE (app_name)
                );";

            try
            {
                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                sqlQuery.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private static void CreateMediaTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_media' (
                'media_id' INTEGER NOT NULL DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                'media_name' TEXT(20) NOT NULL  DEFAULT 'NULL', 
                'media_color' TEXT(15) NOT NULL  DEFAULT 'NULL', 
                UNIQUE (media_name)
                );";

            try
            {
                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                sqlQuery.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private static void CreateScreenTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_screen' (
                'screen_id' INTEGER NOT NULL DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'screen_name' TEXT(20) NOT NULL DEFAULT 'NULL',
                'screen_color' TEXT(15) NOT NULL DEFAULT 'NULL',
                UNIQUE (screen_name)
                );";

            try
            {
                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                sqlQuery.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private static void CreateProjectTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_project' (
                    'project_id' INTEGER NOT NULL  DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                    'project_name' TEXT(30) UNIQUE NOT NULL  DEFAULT 'NULL',
                    'project_version' INTEGER NOT NULL  DEFAULT 1,
                    'project_comments' TEXT(200) DEFAULT NULL,
                    'project_uploaded' INTEGER(1) NOT NULL  DEFAULT 0,
                    'project_date' TEXT(25) DEFAULT NULL, 
                    'project_archived' INTEGER(1) NOT NULL DEFAULT 0,
                    'project_createdate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                    'project_modifydate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                    UNIQUE (project_name, project_version)
                    );";
            try
            {
                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                sqlQuery.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private static void CreateVideoEventTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_videoevent'(
                'videoevent_id' INTEGER NOT NULL DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'fk_videoevent_project' INTEGER NOT NULL DEFAULT 1 REFERENCES 'cbv_project'('project_id'),
                'fk_videoevent_media' INTEGER NOT NULL DEFAULT 1 REFERENCES 'cbv_media'('media_id'),
                'videoevent_track' INTEGER NOT NULL  DEFAULT 1,
                'videoevent_start' TEXT(8) NOT NULL DEFAULT '00:00:00',
                'videoevent_duration' INTEGER NOT NULL DEFAULT 0,
                'videoevent_createdate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'videoevent_modifydate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00'
                );";

            try
            {
                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                sqlQuery.ExecuteNonQuery();
                sqlQuery.Dispose();
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        private static void CreateHlstsTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_hlsts' (
                'hlsts_id' INTEGER NOT NULL DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                'fk_hlsts_project' INTEGER NOT NULL  DEFAULT NULL REFERENCES 'cbv_project' ('project_id'),
                'hlsts_version' INTEGER NOT NULL  DEFAULT 1,
                'hlsts_comments' TEXT(30) DEFAULT NULL,
                'hlsts_media480' BLOB NOT NULL DEFAULT NULL,
                'hlsts_media720' BLOB NOT NULL DEFAULT NULL,
                'hlsts_media1080' BLOB NOT NULL DEFAULT NULL,
                'hlsts_media1280' BLOB NOT NULL DEFAULT NULL,
                'hlsts_master' BLOB NOT NULL DEFAULT NULL,
                'hlsts_encryption' BLOB DEFAULT NULL,
                'hlsts_createdate' TEXT NOT NULL DEFAULT '1999-01-01 00:00:00',
                'hlsts_modifydate' TEXT NOT NULL DEFAULT '1999-01-01 00:00:00',
                UNIQUE (fk_hlsts_project, hlsts_version)
                );";
            try
            {
                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                sqlQuery.ExecuteNonQuery();
                sqlQuery.Dispose();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private static void CreateResolutionTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_resolution' (
                'resolution_id' INTEGER NOT NULL DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                'resolution_name' TEXT(6) NOT NULL DEFAULT 'NULL',
                UNIQUE (resolution_name)
                );";
            try
            {
                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                sqlQuery.ExecuteNonQuery();
                sqlQuery.Dispose();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private static void CreateStreamtsTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_streamts' (
                'streamts_id' INTEGER NOT NULL DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                'fk_streamts_hlsts' INTEGER NOT NULL DEFAULT 1 REFERENCES 'cbv_hlsts' ('hlsts_id'),
                'fk_streamts_resolution' INTEGER NOT NULL DEFAULT 1 REFERENCES 'cbv_resolution' ('resolution_id'),
                'streamts_increment' INTEGER NOT NULL DEFAULT 1,
                'streamts_stream' NONE NOT NULL DEFAULT NULL,
                UNIQUE (fk_streamts_hlsts, fk_streamts_resolution, streamts_increment)
                );";
            try
            {
                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                sqlQuery.ExecuteNonQuery();
                sqlQuery.Dispose();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private static void CreateVideoSegmentTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_videosegment' (
                'videosegment_id' INTEGER NOT NULL DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'fk_videosegment_videoevent' INTEGER NOT NULL  DEFAULT NULL REFERENCES 'cbv_videoevent' ('videoevent_id'),
                'videosegment_media' BLOB NOT NULL DEFAULT NULL,
                'videosegment_createdate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'videosegment_modifydate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00'
                );";
            try
            {
                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                sqlQuery.ExecuteNonQuery();
                sqlQuery.Dispose();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private static void CreateAudioTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_audio' (
                'audio_id' INTEGER NOT NULL DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'fk_audio_videoevent' INTEGER NOT NULL  DEFAULT NULL REFERENCES 'cbv_videoevent' ('videoevent_id'),
                'audio_media' BLOB NOT NULL DEFAULT NULL,
                'audio_createdate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'audio_modifydate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00'
                );";
            try
            {
                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                sqlQuery.ExecuteNonQuery();
                sqlQuery.Dispose();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private static void CreateNotesTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_notes' (
                'notes_id' INTEGER NOT NULL DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'fk_notes_videoevent' INTEGER NOT NULL  DEFAULT NULL REFERENCES 'cbv_videoevent' ('videoevent_id'),
                'notes_line' TEXT(255) NOT NULL  DEFAULT 'NULL',
                'notes_wordcount' INTEGER NOT NULL  DEFAULT 0,
                'notes_index' INTEGER NOT NULL  DEFAULT 0,
                'notes_createdate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'notes_modifydate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00'
                );";
            try
            {
                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                sqlQuery.ExecuteNonQuery();
                sqlQuery.Dispose();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private static void CreateDesignTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_design' (
                'design_id' INTEGER NOT NULL DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'fk_design_videoevent' INTEGER NOT NULL  DEFAULT NULL REFERENCES 'cbv_videoevent' ('videoevent_id'),
                'fk_design_screen' INTEGER NOT NULL DEFAULT 1 REFERENCES 'cbv_screen'('screen_id'),
                'design_xml' TEXT(255) NOT NULL  DEFAULT 'NULL',
                'design_createdate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'design_modifydate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00'
                );";
            try
            {
                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                sqlQuery.ExecuteNonQuery();
                sqlQuery.Dispose();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion  == Create Table Region ==

        #endregion


        #region == Sync Methods  ==

        public static List<int> SyncApp(DataTable dataTable)
        {
            var insertedIds = new List<int>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var values = $"'{dr["app_name"]}', {dr["app_active"]}";
                var whereClause = $" NOT EXISTS(SELECT 1 FROM cbv_app WHERE app_name = '{dr["app_name"]}');";
                string sqlQueryString =
                $@"INSERT INTO cbv_app 
                    (app_name, app_active) 
                    SELECT 
                        {values}
                    WHERE
                        {whereClause}";

                var insertedId = InsertRecordsInTable("cbv_app", sqlQueryString);
                insertedIds.Add(insertedId);
            }
            return insertedIds;
        }

        public static List<int> SyncScreen(DataTable dataTable)
        {
            var insertedIds = new List<int>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var values = $"'{dr["screen_name"]}', '{dr["screen_color"]}'";
                var whereClause = $" NOT EXISTS(SELECT 1 FROM cbv_screen WHERE screen_name = '{dr["screen_name"]}');";
                string sqlQueryString =
                    $@"INSERT INTO cbv_screen 
                        (screen_name, screen_color) 
                        SELECT 
                            {values}
                        WHERE
                            {whereClause}";

                var insertedId = InsertRecordsInTable("cbv_screen", sqlQueryString);
                insertedIds.Add(insertedId);
            }
            return insertedIds;
        }

        public static List<int> SyncMedia(DataTable dataTable)
        {
            var insertedIds = new List<int>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var values = $"'{dr["media_name"]}', '{dr["media_color"]}'";
                var whereClause = $" NOT EXISTS(SELECT 1 FROM cbv_media WHERE media_name = '{dr["media_name"]}');";
                string sqlQueryString =
                $@"INSERT INTO cbv_media 
                    (media_name, media_color) 
                    SELECT 
                        {values}
                    WHERE
                        {whereClause}";

                var insertedId = InsertRecordsInTable("cbv_media", sqlQueryString);
                insertedIds.Add(insertedId);
            }
            return insertedIds;
        }

        public static List<int> SyncResolution(DataTable dataTable)
        {
            var insertedIds = new List<int>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var values = $"'{dr["resolution_name"]}'";
                var whereClause = $" NOT EXISTS(SELECT 1 FROM cbv_resolution WHERE resolution_name = '{dr["resolution_name"]}');";
                string sqlQueryString =
                $@"INSERT INTO cbv_resolution 
                    (resolution_name) 
                    SELECT 
                        {values}
                    WHERE
                        {whereClause}";
                var insertedId = InsertRecordsInTable("cbv_resolution", sqlQueryString);
                insertedIds.Add(insertedId);
            }
            return insertedIds;
        }

        #endregion


        #region == Insert Methods ==

        public static int InsertRowsToProject(DataTable dataTable)
        {
            var values = new List<string>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var projectUploaded = Convert.ToBoolean(dr["project_uploaded"]);
               
                var projectDate = Convert.ToString(dr["project_date"]);
                if (string.IsNullOrEmpty(projectDate))
                    projectDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var projectArchived = Convert.ToBoolean(dr["project_archived"]);

                var createDate = Convert.ToString(dr["project_createdate"]);
                if (string.IsNullOrEmpty(createDate))
                    createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["project_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


                values.Add($"('{dr["project_name"]}', {dr["project_version"]}, '{dr["project_comments"]}', {projectUploaded}, '{projectDate}', {projectArchived}, '{createDate}', '{modifyDate}')");
            }
            var valuesString = string.Join(",", values.ToArray());
            string sqlQueryString =
                $@"INSERT INTO  cbv_project 
                    (project_name, project_version, project_comments, project_uploaded, project_date, project_archived, project_createdate, project_modifydate) 
                VALUES 
                    {valuesString}";

            var insertedId = InsertRecordsInTable("cbv_project", sqlQueryString);
            return insertedId;
        }

        #region == Video Event and Depenedent Tables ==
        public static int InsertRowsToVideoEvent(DataRow dr)
        {
            var values = new List<string>();
            
            var createDate = Convert.ToString(dr["videoevent_createdate"]);
            if (string.IsNullOrEmpty(createDate))
                createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var modifyDate = Convert.ToString(dr["videoevent_modifydate"]);
            if (string.IsNullOrEmpty(modifyDate))
                modifyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            values.Add($"({dr["fk_videoevent_project"]}, {dr["fk_videoevent_media"]}, {dr["videoevent_track"]}, " +
                $"'{dr["videoevent_start"]}', {dr["videoevent_duration"]}, '{createDate}', '{modifyDate}')");
           
            var valuesString = string.Join(",", values.ToArray());
            string sqlQueryString =
                $@"INSERT INTO cbv_videoevent 
                    (fk_videoevent_project, fk_videoevent_media, videoevent_track, 
                        videoevent_start, videoevent_duration, videoevent_createdate, videoevent_modifydate) 
                VALUES 
                    {valuesString}";

            var insertedId = InsertRecordsInTable("cbv_videoevent", sqlQueryString);
            return insertedId;
        }

        public static int InsertRowsToVideoSegment(DataTable dataTable)
        {
            var values = new List<string>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var createDate = Convert.ToString(dr["videosegment_createdate"]);
                if (string.IsNullOrEmpty(createDate))
                    createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["videosegment_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                values.Add($"({dr["fk_videosegment_videoevent"]}, @blob, '{createDate}', '{modifyDate}')");
            }
            var valuesString = string.Join(",", values.ToArray());

            string sqlQueryString =
                $@"INSERT INTO cbv_videosegment 
                    (fk_videosegment_videoevent, videosegment_media, videosegment_createdate, videosegment_modifydate) 
                VALUES 
                    {valuesString}";

            var insertedId = InsertBlobRecordsInTable("cbv_videosegment", sqlQueryString, dataTable.Rows[0]["videosegment_media"] as byte[]);
            return insertedId;
        }

        public static int InsertRowsToAudio(DataTable dataTable)
        {
            var values = new List<string>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var createDate = Convert.ToString(dr["audio_createdate"]);
                if (string.IsNullOrEmpty(createDate))
                    createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["audio_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                values.Add($"({dr["fk_audio_videoevent"]}, @blob, '{createDate}', '{modifyDate}')");
            }
            var valuesString = string.Join(",", values.ToArray());

            string sqlQueryString =
                $@"INSERT INTO cbv_audio 
                    (fk_audio_videoevent, audio_media, audio_createdate, audio_modifydate) 
                VALUES 
                    {valuesString}";

            var insertedId = InsertBlobRecordsInTable("cbv_audio", sqlQueryString, dataTable.Rows[0]["audio_media"] as byte[]);
            return insertedId;
        }

        public static int InsertRowsToDesign(DataTable dataTable)
        {
            var values = new List<string>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var createDate = Convert.ToString(dr["design_createdate"]);
                if (string.IsNullOrEmpty(createDate))
                    createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["design_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                values.Add($"({dr["fk_design_videoevent"]}, {dr["fk_design_screen"]}, '{dr["design_xml"]}', '{createDate}', '{modifyDate}')");
            }
            var valuesString = string.Join(",", values.ToArray());

            string sqlQueryString =
                $@"INSERT INTO cbv_design 
                    (fk_design_videoevent, fk_design_screen, design_xml, design_createdate, design_modifydate) 
                VALUES 
                    {valuesString}";

            var insertedId = InsertRecordsInTable("cbv_design", sqlQueryString);
            return insertedId;
        }

        #endregion

        #region == HLSTS and Depenedent Tables ==

        public static int InsertRowsToHLSTS(DataTable dataTable)
        {
            var values = new List<string>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var createDate = Convert.ToString(dr["hlsts_createdate"]);
                if (string.IsNullOrEmpty(createDate))
                    createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["hlsts_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                values.Add($"({dr["fk_hlsts_project"]}, {dr["hlsts_version"]}, '{dr["hlsts_comments"]}', @blob480,  @blob720,  @blob1080,  @blob1280, @master, @encryption, '{createDate}', '{modifyDate}')");
            }
            var valuesString = string.Join(",", values.ToArray());

            string sqlQueryString =
                $@"INSERT INTO cbv_hlsts 
                    (fk_hlsts_project, hlsts_version, hlsts_comments, hlsts_media480, hlsts_media720, hlsts_media1080, hlsts_media1280, hlsts_master, hlsts_encryption, hlsts_createdate, hlsts_modifydate) 
                VALUES 
                    {valuesString}";


            var blob480 = dataTable.Rows[0]["hlsts_media480"] as byte[];
            var blob720 = dataTable.Rows[0]["hlsts_media720"] as byte[];
            var blob1080 = dataTable.Rows[0]["hlsts_media1080"] as byte[];
            var blob1280 = dataTable.Rows[0]["hlsts_media1280"] as byte[];
            var master = dataTable.Rows[0]["hlsts_master"] as byte[];
            var encryption = dataTable.Rows[0]["hlsts_encryption"] as byte[];

            var insertedId = InsertBlobRecordsInHlsts("cbv_hlsts", sqlQueryString, blob480, blob720, blob1080, blob1280, master, encryption);
            return insertedId;
        }

        public static int InsertRowsToStreamts(DataTable dataTable)
        {
            var values = new List<string>();
            foreach (DataRow dr in dataTable.Rows)
            {
                values.Add($"({dr["fk_streamts_hlsts"]}, {dr["fk_streamts_resolution"]}, {dr["streamts_increment"]}, @blob)");
            }
            var valuesString = string.Join(",", values.ToArray());

            string sqlQueryString =
                $@"INSERT INTO cbv_streamts 
                    (fk_streamts_hlsts, fk_streamts_resolution, streamts_increment, streamts_stream) 
                VALUES 
                    {valuesString}";

            var insertedId = InsertBlobRecordsInTable("cbv_streamts", sqlQueryString, dataTable.Rows[0]["streamts_stream"] as byte[]);
            return insertedId;
        }

        #endregion

        public static int InsertRowsToNotes(DataTable dataTable)
        {
            var insertedId = 0;
            foreach (DataRow dr in dataTable.Rows)
            {

                //(Select Max(notes_index) + 1 from cbv_notes where fk_notes_videoevent = 1) as index
                var max_index = GetMaxIndexForNotes(Convert.ToInt32(dr["fk_notes_videoevent"]));
                var createDate = Convert.ToString(dr["notes_createdate"]);
                if (string.IsNullOrEmpty(createDate))
                    createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["notes_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var values = new List<string>();
                values.Add($"({dr["fk_notes_videoevent"]}, '{dr["notes_line"]}', {dr["notes_wordcount"]},  {max_index}, '{createDate}', '{modifyDate}')");
                var valuesString = string.Join(",", values.ToArray());

                string sqlQueryString =
                    $@"INSERT INTO cbv_notes 
                    (fk_notes_videoevent, notes_line, notes_wordcount, notes_index, notes_createdate, notes_modifydate) 
                VALUES 
                    {valuesString}";

                insertedId = InsertRecordsInTable("cbv_notes", sqlQueryString);
            }
            
            return insertedId;
        }

        #region == Insert Helper ==
        private static int InsertBlobRecordsInHlsts(string tableName, string InsertQuery, byte[] blob480, byte[] blob720, byte[] blob1080, byte[] blob1280, byte[] master, byte[] encryption)
        {
            // Check if database is created
            if (false == IsDbCreated())
            {
                throw new Exception("Database is not present.");
            }

            SQLiteConnection sqlCon = null;
            var id = -1;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                using (var command = new SQLiteCommand(InsertQuery, sqlCon))
                {
                    command.Parameters.Add(new SQLiteParameter("blob480", DbType.Binary) { Value = blob480 });
                    command.Parameters.Add(new SQLiteParameter("blob720", DbType.Binary) { Value = blob720 });
                    command.Parameters.Add(new SQLiteParameter("blob1080", DbType.Binary) { Value = blob1080 });
                    command.Parameters.Add(new SQLiteParameter("blob1280", DbType.Binary) { Value = blob1280 });
                    command.Parameters.Add(new SQLiteParameter("master", DbType.Binary) { Value = master });
                    command.Parameters.Add(new SQLiteParameter("encryption", DbType.Binary) { Value = encryption });
                    command.ExecuteNonQuery();
                }
                //// Execute the SQLite query
                //var sqlQuery = new SQLiteCommand(InsertQuery, sqlCon);
                //sqlQuery.ExecuteNonQuery();

                // Read the project ID
                var sqlQueryString = $@"SELECT seq from sqlite_sequence where name = '{tableName}'";
                using (var command = new SQLiteCommand(sqlQueryString, sqlCon))
                {
                    using (SQLiteDataReader sqlReader = command.ExecuteReader())
                    {
                        if (sqlReader.HasRows)
                            if (sqlReader.Read())
                                id = sqlReader.GetInt32(0);
                    }
                }
            }
            catch (Exception e)
            {
                if (null != sqlCon)
                {
                    sqlCon.Close();
                }
                throw e;
            }
            finally
            {
                if (sqlCon != null)
                {
                    sqlCon.Close();
                }
            }
            return id;
        }

        private static int InsertBlobRecordsInTable(string tableName, string InsertQuery, byte[] blob)
        {
            // Check if database is created
            if (false == IsDbCreated())
            {
                throw new Exception("Database is not present.");
            }

            SQLiteConnection sqlCon = null;
            var id = -1;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                using (var command = new SQLiteCommand(InsertQuery, sqlCon))
                {
                    var dataParameter = new SQLiteParameter("blob", DbType.Binary) { Value = blob };
                    command.Parameters.Add(dataParameter);
                    command.ExecuteNonQuery();
                }
                //// Execute the SQLite query
                //var sqlQuery = new SQLiteCommand(InsertQuery, sqlCon);
                //sqlQuery.ExecuteNonQuery();

                // Read the project ID
                var sqlQueryString = $@"SELECT seq from sqlite_sequence where name = '{tableName}'";
                using (var command = new SQLiteCommand(sqlQueryString, sqlCon))
                {
                    using (SQLiteDataReader sqlReader = command.ExecuteReader())
                    {
                        if (sqlReader.HasRows)
                            if (sqlReader.Read())
                                id = sqlReader.GetInt32(0);
                    }
                }
            }
            catch (Exception e)
            {
                if (null != sqlCon)
                {
                    sqlCon.Close();
                }
                throw e;
            }
            finally
            {
                if (sqlCon != null)
                {
                    sqlCon.Close();
                    sqlCon = null;
                }
            }
            return id;
        }

        private static int InsertRecordsInTable(string tableName, string InsertQuery)
        {
            // Check if database is created
            if (false == IsDbCreated())
            {
                throw new Exception("Database is not present.");
            }

            SQLiteConnection sqlCon = null;
            var id = -1;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                // Execute the SQLite query
                var sqlQuery = new SQLiteCommand(InsertQuery, sqlCon);
                sqlQuery.ExecuteNonQuery();
                sqlQuery.Dispose();

                // Read the project ID
                var sqlQueryString = $@"SELECT seq from sqlite_sequence where name = '{tableName}'";
                var command = new SQLiteCommand(sqlQueryString, sqlCon);
                using (SQLiteDataReader sqlReader = command.ExecuteReader())
                {
                    if (sqlReader.HasRows)
                    {
                        if (sqlReader.Read())
                            id = sqlReader.GetInt32(0);
                    }
                    sqlQuery.Dispose();
                }
                command.Dispose();
                sqlCon?.Close();
            }
            catch (Exception e)
            {
                sqlCon?.Close();
                throw e;
            }
            return id;
        }

        #endregion

        #endregion


        #region == GET Methods ==


        private static int GetMaxIndexForNotes(int fkVideoEventId)
        {
            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            SQLiteConnection sqlCon = null; 
            int count;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                string sqlQueryString = $@"Select IFNULL(Max(notes_index),0) + 1 from cbv_notes where fk_notes_videoevent = {fkVideoEventId}";
                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                count = Convert.ToInt32(sqlQuery.ExecuteScalar());
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception e)
            {
                sqlCon?.Close();
                throw e;
            }

            return count;
        }

        public static int GetCount(string TableName)
        {
            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            SQLiteConnection sqlCon = null;
            int count;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                string sqlQueryString = $@"SELECT count(*) FROM {TableName}";
                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                count = Convert.ToInt32(sqlQuery.ExecuteScalar());
                sqlQuery.Dispose();
                //using (var sqlReader = sqlQuery.ExecuteReader())
                //{
                //    count = Convert.ToInt32(sqlReader.GetInt32(0));
                //}
                // Close database
                sqlCon.Close();
            }
            catch (Exception e)
            {
                sqlCon?.Close();
                throw e;
            }

            return count;
        }

        public static List<CBVScreen> GetScreens()
        {
            var data = new List<CBVScreen>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_screen";

            SQLiteConnection sqlCon = null;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                using (var sqlReader = sqlQuery.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        var obj = new CBVScreen
                        {
                            screen_id = sqlReader.GetInt32(0),
                            screen_name = sqlReader.GetString(1),
                            screen_color = sqlReader.GetString(2),
                        };
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception e)
            {
                if (null != sqlCon)
                    sqlCon.Close();
                throw e;
            }

            return data;
        }

        public static List<CBVApp> GetApp()
        {
            var data = new List<CBVApp>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_app";

            SQLiteConnection sqlCon = null;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                using (var sqlReader = sqlQuery.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        var obj = new CBVApp
                        {
                            app_id = sqlReader.GetInt32(0),
                            app_name = sqlReader.GetString(1),
                            app_active = sqlReader.GetBoolean(2),
                        };
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception e)
            {
                if (null != sqlCon)
                    sqlCon.Close();
                throw e;
            }

            return data;
        }

        public static List<CBVMedia> GetMedia()
        {
            var data = new List<CBVMedia>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_media";

            SQLiteConnection sqlCon = null;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                using (var sqlReader = sqlQuery.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        var obj = new CBVMedia
                        {
                            media_id = sqlReader.GetInt32(0),
                            media_name = sqlReader.GetString(1),
                            media_color = sqlReader.GetString(2),
                        };
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception e)
            {
                if (null != sqlCon)
                    sqlCon.Close();
                throw e;
            }

            return data;
        }

        public static List<CBVResolution> GetResolution()
        {
            var data = new List<CBVResolution>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_resolution";

            SQLiteConnection sqlCon = null;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                using (var sqlReader = sqlQuery.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        var obj = new CBVResolution
                        {
                            resolution_id = sqlReader.GetInt32(0),
                            resolution_name = sqlReader.GetString(1),
                        };
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception e)
            {
                sqlCon?.Close();
                throw e;
            }

            return data;
        }

        public static List<CBVProject> GetProjects(bool includeArchived, bool startedFlag = false)
        {
            var projects = new List<CBVProject>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");
            string sqlQueryString = "SELECT * FROM cbv_project ";
           
            if (startedFlag)
            {
                sqlQueryString = $@"
                            SELECT P.*,
                                   CASE 
			                            When VECount.VideoEventCount is NULL Then 0
			                            When VECount.VideoEventCount = 0 Then 0
			                            Else 1
		                            End project_started,
		                            CASE 
		                                When VECount.VideoEventCount Is Null Then 0
		                                Else VECount.VideoEventCount 
		                            End project_videoeventcount,
									CASE 
			                            When HlstsCount.HlstsCount is NULL Then 0
			                            When HlstsCount.HlstsCount = 0 Then 0
			                            Else 1
		                            End project_downloaded,
		                            CASE 
		                                When HlstsCount.HlstsCount Is Null Then 0
		                                Else HlstsCount.HlstsCount 
		                            End project_hlstscount
                            FROM cbv_project P
                            LEFT JOIN (
                                       Select fk_videoevent_project, count(fk_videoevent_project) as VideoEventCount
                                       from cbv_videoevent VE 
			                            group by fk_videoevent_project
                                       ) as VECount on VECount.fk_videoevent_project = P.project_id
						    LEFT JOIN (
                                       Select fk_hlsts_project, count(fk_hlsts_project) as HlstsCount
                                       from cbv_hlsts H 
			                            group by fk_hlsts_project
                                       ) as HlstsCount on HlstsCount.fk_hlsts_project = P.project_id";
            }
            if (!includeArchived)
                sqlQueryString += $@" WHERE project_archived=false";
          
            SQLiteConnection sqlCon = null;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                using (var sqlReader = sqlQuery.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        var project = new CBVProject
                        {
                            project_id = sqlReader.GetInt32(0),
                            project_name = sqlReader.GetString(1),
                            project_version = sqlReader.GetInt32(2),
                            project_comments = sqlReader.GetString(3),
                            project_uploaded = sqlReader.GetBoolean(4),
                            project_date = sqlReader.GetDateTime(5),
                            project_archived = sqlReader.GetBoolean(6),
                            project_createdate = sqlReader.GetDateTime(7),
                            project_modifydate = sqlReader.GetDateTime(8)
                        };
                        if (startedFlag)
                        {
                            project.project_started = Convert.ToBoolean(sqlReader["project_started"]);
                            project.project_videoeventcount = Convert.ToInt32(sqlReader["project_videoeventcount"]);
                            project.project_downloaded = Convert.ToBoolean(sqlReader["project_downloaded"]);
                            project.project_hlstscount = Convert.ToInt32(sqlReader["project_hlstscount"]);
                        }
                        projects.Add(project);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception e)
            {
                if (null != sqlCon)
                {
                    sqlCon.Close();
                }
                throw e;
            }

            return projects;
        }

        public static List<CBVVideoEvent> GetVideoEvents(int projectId, bool dependentDataFlag = false)
        {
            var data = new List<CBVVideoEvent>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_videoevent ";
            sqlQueryString += projectId <= 0 ? "" : $@" where fk_videoevent_project = {projectId} ";
            SQLiteConnection sqlCon = null;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                using (var sqlReader = sqlQuery.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        var obj = new CBVVideoEvent
                        {
                            videoevent_id = sqlReader.GetInt32(0),
                            fk_videoevent_project = sqlReader.GetInt32(1),
                            fk_videoevent_media = sqlReader.GetInt32(2),
                            videoevent_track = sqlReader.GetInt32(3),
                            videoevent_start = sqlReader.GetString(4),
                            videoevent_duration = sqlReader.GetInt32(5),
                            videoevent_createdate = sqlReader.GetDateTime(6),
                            videoevent_modifydate = sqlReader.GetDateTime(7)
                        };
                        if (dependentDataFlag)
                        {
                            var videoEventId = sqlReader.GetInt32(0);

                            var audioData = GetAudio(videoEventId);
                            obj.audio_data = audioData;

                            var videoSegmentData = GetVideoSegment(videoEventId);
                            obj.videosegment_data = videoSegmentData;

                            var designData = GetDesign(videoEventId);
                            obj.design_data = designData;
                        }
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception e)
            {
                sqlCon?.Close();
                throw e;
            }

            return data;
        }

        public static List<CBVAudio> GetAudio(int videoEventId)
        {
            var data = new List<CBVAudio>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_audio ";
            if (videoEventId > -1)
                sqlQueryString += $@" where fk_audio_videoevent = {videoEventId}";
            SQLiteConnection sqlCon = null;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                using (var sqlReader = sqlQuery.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        var obj = new CBVAudio
                        {
                            audio_id = Convert.ToInt32(sqlReader["audio_id"]),
                            fk_audio_videoevent = Convert.ToInt32(sqlReader["fk_audio_videoevent"]),
                            audio_createdate = Convert.ToDateTime(sqlReader["audio_createdate"]),
                            audio_modifydate = Convert.ToDateTime(sqlReader["audio_modifydate"]),
                        };
                        var byteData = GetBlobMedia(sqlReader, "audio_media");
                        obj.audio_media = byteData;
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception e)
            {
                sqlCon?.Close();
                throw e;
            }
            return data;
        }

        public static List<CBVVideoSegment> GetVideoSegment(int videoEventId)
        {
            var data = new List<CBVVideoSegment>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_videosegment ";
            if (videoEventId > -1)
                sqlQueryString += $@" where fk_videosegment_videoevent = {videoEventId};";
            SQLiteConnection sqlCon = null;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                using (var sqlReader = sqlQuery.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        var obj = new CBVVideoSegment
                        {
                            videosegment_id = Convert.ToInt32(sqlReader["videosegment_id"]),
                            fk_videosegment_videoevent = Convert.ToInt32(sqlReader["fk_videosegment_videoevent"]),
                            videosegment_createdate = Convert.ToDateTime(sqlReader["videosegment_createdate"]),
                            videosegment_modifydate = Convert.ToDateTime(sqlReader["videosegment_modifydate"]),
                        };
                        var byteData = GetBlobMedia(sqlReader, "videosegment_media");
                        obj.videosegment_media = byteData;
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception e)
            {
                sqlCon?.Close();
                throw e;
            }

            return data;
        }

        public static List<CBVDesign> GetDesign(int videoEventId)
        {
            var data = new List<CBVDesign>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_design ";
            if (videoEventId > -1)
                sqlQueryString += $@" where fk_design_videoevent = {videoEventId} ";
            SQLiteConnection sqlCon = null;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                using (var sqlReader = sqlQuery.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        var obj = new CBVDesign
                        {
                            design_id = Convert.ToInt32(sqlReader["design_id"]),
                            fk_design_screen = Convert.ToInt32(sqlReader["fk_design_screen"]),
                            fk_design_videoevent = Convert.ToInt32(sqlReader["fk_design_videoevent"]),
                            design_xml = Convert.ToString(sqlReader["design_xml"]),
                            design_createdate = Convert.ToDateTime(sqlReader["design_createdate"]),
                            design_modifydate = Convert.ToDateTime(sqlReader["design_modifydate"]),
                        };
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception e)
            {
                sqlCon?.Close();
                throw e;
            }

            return data;
        }

        public static List<CBVHlsts> GetHlsts(int projectId = -1, bool dependentDataFlag = false)
        {
            var data = new List<CBVHlsts>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_hlsts ";
            if (projectId > -1)
                sqlQueryString += $@" where fk_hlsts_project = {projectId} ";
            SQLiteConnection sqlCon = null;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                using (var sqlReader = sqlQuery.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        var obj = new CBVHlsts
                        {
                            hlsts_id = Convert.ToInt32(sqlReader["hlsts_id"]),
                            fk_hlsts_project = Convert.ToInt32(sqlReader["fk_hlsts_project"]),
                            hlsts_version = Convert.ToInt32(sqlReader["hlsts_version"]),
                            hlsts_comments = Convert.ToString(sqlReader["hlsts_comments"]),
                            hlsts_createdate = Convert.ToDateTime(sqlReader["hlsts_createdate"]),
                            hlsts_modifydate = Convert.ToDateTime(sqlReader["hlsts_modifydate"]),
                            hlsts_media480 = GetBlobMedia(sqlReader, "hlsts_media480"),
                            hlsts_media720 = GetBlobMedia(sqlReader, "hlsts_media720"),
                            hlsts_media1080 = GetBlobMedia(sqlReader, "hlsts_media1080"),
                            hlsts_media1280 = GetBlobMedia(sqlReader, "hlsts_media1280"),
                            hlsts_master = GetBlobMedia(sqlReader, "hlsts_master"),
                            hlsts_encryption = GetBlobMedia(sqlReader, "hlsts_encryption")
                        };

                        if (dependentDataFlag)
                        {
                            var hlstsId = sqlReader.GetInt32(0);
                            var streamtsData = GetStreamts(hlstsId);
                            obj.streamts_data = streamtsData;
                        }

                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception e)
            {
                sqlCon?.Close();
                throw e;
            }

            return data;
        }

        public static List<CBVStreamts> GetStreamts(int hlstsId = -1)
        {
            var data = new List<CBVStreamts>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_streamts ";
            if (hlstsId > -1)
                sqlQueryString += $@" where fk_streamts_hlsts = {hlstsId} ";
            SQLiteConnection sqlCon = null;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                using (var sqlReader = sqlQuery.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        var obj = new CBVStreamts
                        {
                            streamts_id = Convert.ToInt32(sqlReader["streamts_id"]),
                            fk_streamts_hlsts = Convert.ToInt32(sqlReader["fk_streamts_hlsts"]),
                            fk_streamts_resolution = Convert.ToInt32(sqlReader["fk_streamts_resolution"]),
                            streamts_increment = Convert.ToInt32(sqlReader["streamts_increment"])
                        };
                        obj.streamts_stream = GetBlobMedia(sqlReader, "streamts_stream");
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception e)
            {
                sqlCon?.Close();
                throw e;
            }

            return data;
        }

        public static List<CBVNotes> GetNotes(int videoEventId)
        {
            var data = new List<CBVNotes>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_notes";
            if (videoEventId > -1)
                sqlQueryString += $@" where fk_notes_videoevent = {videoEventId} order by notes_index";
            SQLiteConnection sqlCon = null;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                using (var sqlReader = sqlQuery.ExecuteReader())
                {
                    while (sqlReader.Read())
                    {
                        var obj = new CBVNotes
                        {
                            notes_id = Convert.ToInt32(sqlReader["notes_id"]),
                            fk_notes_videoevent = Convert.ToInt32(sqlReader["fk_notes_videoevent"]),
                            notes_line = Convert.ToString(sqlReader["notes_line"]),
                            notes_wordcount = Convert.ToInt32(sqlReader["notes_wordcount"]),
                            notes_index = Convert.ToInt32(sqlReader["notes_index"]),
                            notes_createdate = Convert.ToDateTime(sqlReader["notes_createdate"]),
                            notes_modifydate = Convert.ToDateTime(sqlReader["notes_modifydate"]),
                        };
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception e)
            {
                sqlCon?.Close();
                throw e;
            }

            return data;
        }

        #endregion


        #region == Helper Methods ==

        public static bool ReferentialKeyPresent(string tablename, string IdColumnName, int id = -1)
        {
            var flag = false;

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM {tablename} WHERE {IdColumnName} = {id}";

            SQLiteConnection sqlCon = null;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                using (var sqlReader = sqlQuery.ExecuteReader())
                {
                    flag = sqlReader.HasRows;
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception e)
            {
                sqlCon?.Close();
                throw e;
            }

            return flag;
        }

        private static byte[] GetBlobMedia(SQLiteDataReader dr, string columnName)
        {
            try
            {
                object media = dr[columnName];
                if (!Convert.IsDBNull(media))
                    return (byte[])media;
                else
                    return null;
            }
            catch (Exception)
            {
                return null;
            }
        }



        #endregion


        #region == Update Methods ==

        #region == Update Helper methods ==
        private static bool UpdateRecordsInTable(string UpdateQuery)
        {
            // Check if database is created
            if (false == IsDbCreated())
            {
                throw new Exception("Database is not present.");
            }

            SQLiteConnection sqlCon = null;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                // Execute the SQLite query
                var sqlQuery = new SQLiteCommand(UpdateQuery, sqlCon);
                sqlQuery.ExecuteNonQuery();
                sqlQuery.Dispose();
                sqlCon?.Close();
                return true;
            }
            catch (Exception e)
            {
                sqlCon?.Close();
                throw e;
            }
        }

        private static bool UpdateBlobRecordsInTable(string UpdateQuery, byte[] blob)
        {
            // Check if database is created
            if (false == IsDbCreated())
            {
                throw new Exception("Database is not present.");
            }

            SQLiteConnection sqlCon = null;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                using (var command = new SQLiteCommand(UpdateQuery, sqlCon))
                {
                    var dataParameter = new SQLiteParameter("blob", DbType.Binary) { Value = blob };
                    command.Parameters.Add(dataParameter);
                    command.ExecuteNonQuery();
                }
                sqlCon?.Close();
                return true;
            }
            catch (Exception e)
            {
                if (null != sqlCon)
                {
                    sqlCon.Close();
                }
                throw e;
            }
        }

        private static bool UpdateBlobRecordsInHlsts(string UpdateQuery, byte[] blob480, byte[] blob720, byte[] blob1080, byte[] blob1280, byte[] master, byte[] encryption)
        {
            // Check if database is created
            if (false == IsDbCreated())
            {
                throw new Exception("Database is not present.");
            }

            SQLiteConnection sqlCon = null;
            try
            {
                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                using (var command = new SQLiteCommand(UpdateQuery, sqlCon))
                {
                    command.Parameters.Add(new SQLiteParameter("blob480", DbType.Binary) { Value = blob480 });
                    command.Parameters.Add(new SQLiteParameter("blob720", DbType.Binary) { Value = blob720 });
                    command.Parameters.Add(new SQLiteParameter("blob1080", DbType.Binary) { Value = blob1080 });
                    command.Parameters.Add(new SQLiteParameter("blob1280", DbType.Binary) { Value = blob1280 });
                    command.Parameters.Add(new SQLiteParameter("master", DbType.Binary) { Value = master });
                    command.Parameters.Add(new SQLiteParameter("encryption", DbType.Binary) { Value = encryption });
                    command.ExecuteNonQuery();
                }
                sqlCon?.Close();
                return true;
            }
            catch (Exception e)
            {
                sqlCon?.Close();
                throw e;
            }
        }

        #endregion


        public static void UpdateRowsToProject(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var modifyDate = Convert.ToString(dr["project_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var project_id = Convert.ToInt32(dr["project_id"]);

                var updateQueryString = $@" UPDATE cbv_project 
                                        SET 
                                            project_name = '{Convert.ToString(dr["project_name"])}',
                                            project_version = {Convert.ToInt32(dr["project_version"])},
                                            project_comments = '{Convert.ToString(dr["project_comments"])}',
                                            project_uploaded = {Convert.ToInt32(dr["project_uploaded"])},
                                            project_date = '{Convert.ToString(dr["project_date"])}',
                                            project_archived = {Convert.ToInt32(dr["project_archived"])},
                                            project_modifydate = '{modifyDate}'
                                        WHERE 
                                            project_id = {project_id}";
                var updateFlag = UpdateRecordsInTable(updateQueryString);
                Console.WriteLine($@"cbv_project table update status for id - {project_id} result - {updateFlag}");
            }
        }

        public static void UpdateRowsToVideoEvent(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var modifyDate = Convert.ToString(dr["videoevent_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var videoevent_id = Convert.ToInt32(dr["videoevent_id"]);
                var updateQueryString = $@" UPDATE cbv_videoevent 
                                        SET 
                                            fk_videoevent_project = {Convert.ToInt32(dr["fk_videoevent_project"])},
                                            fk_videoevent_media = {Convert.ToInt32(dr["fk_videoevent_media"])},
                                            videoevent_track = {Convert.ToInt32(dr["videoevent_track"])},
                                            videoevent_start = '{Convert.ToString(dr["videoevent_start"])}',
                                            videoevent_duration = {Convert.ToInt32(dr["videoevent_duration"])},
                                            videoevent_modifydate = '{modifyDate}'
                                        WHERE 
                                            videoevent_id = {videoevent_id}";
                var updateFlag = UpdateRecordsInTable(updateQueryString);
                Console.WriteLine($@"cbv_videoevent table update status for id - {videoevent_id} result - {updateFlag}");
            }
        }

        public static void UpdateRowsToVideoSegment(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var modifyDate = Convert.ToString(dr["videosegment_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var videosegment_id = Convert.ToInt32(dr["videosegment_id"]);
                var updateQueryString = $@" UPDATE cbv_videosegment 
                                        SET 
                                            fk_videosegment_videoevent = {Convert.ToInt32(dr["fk_videosegment_videoevent"])},
                                            videosegment_media = @blob,
                                            videosegment_modifydate = '{modifyDate}'
                                        WHERE 
                                            videosegment_id = {videosegment_id}";
                var updateFlag = UpdateBlobRecordsInTable(updateQueryString, dr["videosegment_media"] as byte[]);
                Console.WriteLine($@"cbv_videosegment table update status for id - {videosegment_id} result - {updateFlag}");
            }
        }

        public static void UpdateRowsToAudio(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var modifyDate = Convert.ToString(dr["audio_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var audio_id = Convert.ToInt32(dr["audio_id"]);
                var updateQueryString = $@" UPDATE cbv_audio
                                        SET 
                                            fk_audio_videoevent = {Convert.ToInt32(dr["fk_audio_videoevent"])},
                                            audio_media = @blob,
                                            audio_modifydate = '{modifyDate}'
                                        WHERE 
                                            audio_id = {audio_id}";
                var updateFlag = UpdateBlobRecordsInTable(updateQueryString, dr["audio_media"] as byte[]);
                Console.WriteLine($@"cbv_audio table update status for id - {audio_id} result - {updateFlag}");
            }
        }

        public static void UpdateRowsToDesign(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var modifyDate = Convert.ToString(dr["design_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var design_id = Convert.ToInt32(dr["design_id"]);
                var updateQueryString = $@" UPDATE cbv_design
                                        SET 
                                            fk_design_videoevent = {Convert.ToInt32(dr["fk_design_videoevent"])},
                                            fk_design_screen = {Convert.ToInt32(dr["fk_design_screen"])},
                                            design_xml = '{Convert.ToString(dr["design_xml"])}',
                                            design_modifydate = '{modifyDate}'
                                        WHERE 
                                            design_id = {design_id}";
                var updateFlag = UpdateRecordsInTable(updateQueryString);
                Console.WriteLine($@"cbv_design table update status for id - {design_id} result - {updateFlag}");
            }
        }

        public static void UpdateRowsToHlsts(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var modifyDate = Convert.ToString(dr["hlsts_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var hlsts_id = Convert.ToInt32(dr["hlsts_id"]);
                var updateQueryString = $@" UPDATE cbv_hlsts
                                        SET 
                                            fk_hlsts_project = {Convert.ToInt32(dr["fk_hlsts_project"])},
                                            hlsts_version={Convert.ToInt32(dr["hlsts_version"])},
                                            hlsts_comments='{Convert.ToString(dr["hlsts_comments"])}',
                                            hlsts_media480 = @blob480,
                                            hlsts_media720 = @blob720,
                                            hlsts_media1080 = @blob1080,
                                            hlsts_media1280 = @blob1280,
                                            hlsts_master = @master,
                                            hlsts_encryption = @encryption,
                                            hlsts_modifydate = '{modifyDate}'
                                        WHERE 
                                            hlsts_id = {hlsts_id}";

                var blob480 = dataTable.Rows[0]["hlsts_media480"] as byte[];
                var blob720 = dataTable.Rows[0]["hlsts_media720"] as byte[];
                var blob1080 = dataTable.Rows[0]["hlsts_media1080"] as byte[];
                var blob1280 = dataTable.Rows[0]["hlsts_media1280"] as byte[];
                var master = dataTable.Rows[0]["hlsts_master"] as byte[];
                var encryption = dataTable.Rows[0]["hlsts_encryption"] as byte[];
                var updateFlag = UpdateBlobRecordsInHlsts(updateQueryString, blob480, blob720, blob1080, blob1280, master, encryption);
                Console.WriteLine($@"cbv_hlsts table update status for id - {hlsts_id} result - {updateFlag}");
            }
        }

        public static void UpdateRowsToStreamts(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var streamts_id = Convert.ToInt32(dr["streamts_id"]);
                var updateQueryString = $@" UPDATE cbv_streamts
                                        SET 
                                            fk_streamts_hlsts = {Convert.ToInt32(dr["fk_streamts_hlsts"])},
                                            fk_streamts_resolution = {Convert.ToInt32(dr["fk_streamts_resolution"])},
                                            streamts_increment = {Convert.ToInt32(dr["streamts_increment"])},
                                            streamts_stream = @blob
                                        WHERE 
                                            streamts_id = {streamts_id}";
                var updateFlag = UpdateBlobRecordsInTable(updateQueryString, dr["streamts_stream"] as byte[]);
                Console.WriteLine($@"cbv_streamts table update status for id - {streamts_id} result - {updateFlag}");
            }
        }
        
        public static void UpdateRowsToNotes(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var modifyDate = Convert.ToString(dr["notes_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var notes_id = Convert.ToInt32(dr["notes_id"]);
                var updateQueryString = $@" UPDATE cbv_notes
                                        SET 
                                            notes_index = {Convert.ToInt32(dr["notes_index"])},
                                            notes_line = '{Convert.ToString(dr["notes_line"])}',
                                            notes_wordcount = {Convert.ToInt32(dr["notes_wordcount"])},
                                            notes_modifydate = '{modifyDate}'
                                        WHERE 
                                            notes_id = {notes_id}";
                var updateFlag = UpdateRecordsInTable(updateQueryString);
                Console.WriteLine($@"cbv_notes table update status for id - {notes_id} result - {updateFlag}");
            }
        }

        #endregion


        #region == Other existing Methods - Not in use for now  ==
        /*
        /// <summary>
        /// A Warapper. 
        /// Equal to archiveProject(projectId, false);
        /// </summary>
        /// <param name="projectId">Project Id</param>
        public static void UnarchiveProject(int projectId)
        {
            ArchiveProject(projectId, false);
        }

        

        /// <summary>
        /// Get the project list for given ID consisting Name and ID
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"> </exception>
        public static List<CBVProject> GetProject(int projectId)
        {
            List<CBVProject> projects = new List<CBVProject>();

            // Check if database is created
            if (false == IsDbCreated())
            {
                throw new Exception("Database is not present.");
            }

            string sqlQueryString = $@"SELECT * FROM cbv_project WHERE project_id={projectId}";

            SQLiteConnection sqlCon = null;
            try
            {
                string fileName = GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                var sqlReader = sqlQuery.ExecuteReader();


                while (sqlReader.Read())
                {
                    var project = new CBVProject
                    {
                        project_id = sqlReader.GetInt32(0),
                        project_name = sqlReader.GetString(1)
                    };
                    projects.Add(project);
                }

                // Close database
                sqlCon.Close();
            }
            catch (Exception e)
            {
                if (null != sqlCon)
                {
                    sqlCon.Close();
                }
                throw e;
            }

            return projects;
        }

        public static List<CBVForm> GetForms(int videoEvent)
        {
            List<CBVForm> forms = new List<CBVForm>();

            // Check if database is created
            if (false == IsDbCreated())
            {
                throw new Exception("Database is not present.");
            }

            string sqlQueryString = $@"SELECT * FROM cbv_form WHERE fk_form_videoevent = {videoEvent}";

            SQLiteConnection sqlCon = null;
            try
            {
                string fileName = GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                var sqlReader = sqlQuery.ExecuteReader();


                while (sqlReader.Read())
                {
                    var form = new CBVForm
                    {
                        id = sqlReader.GetInt32(0),
                        line = sqlReader.GetString(2)
                    };
                    forms.Add(form);
                }

                // Close database
                sqlCon.Close();
            }
            catch (Exception e)
            {
                sqlCon?.Close();
                throw e;
            }

            return forms;
        }

        /// <summary>
        /// Returns event list from database for the given project if
        /// </summary>
        /// <param name="projectId"> Priject Id </param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static List<CBVEvent> GetEvents(int projectId)
        {
            List<CBVEvent> events = new List<CBVEvent>();

            // Check if database is created
            if (false == IsDbCreated())
            {
                throw new Exception("Database is not present.");
            }

            string sqlQueryString = $@"SELECT * FROM cbv_videoevent WHERE fk_videoevent_project={projectId}";

            SQLiteConnection sqlCon = null;

            try
            {
                string fileName = GetFileName();

                // Open Database connection 
                sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlCon.Open();

                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                var sqlReader = sqlQuery.ExecuteReader();


                while (sqlReader.Read())
                {
                    var cbvEvent = new CBVEvent
                    {
                        id = sqlReader.GetInt32(0),
                        projectId = sqlReader.GetInt32(1),
                        trakeNo = sqlReader.GetInt32(2),
                        screenId = sqlReader.GetInt32(3),
                        mediaId = sqlReader.GetInt32(4),
                        name = sqlReader.GetString(5),
                        start = sqlReader.GetDouble(6),
                        duration = sqlReader.GetInt32(7)
                    };

                    events.Add(cbvEvent);
                }

                // Close database
                sqlCon.Close();
                return events;
            }
            catch (Exception e)
            {
                sqlCon?.Close();
                throw e;
            }

        }
        */

        #endregion
    }
}
