using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;
using Sqllite_Library.Models;
using System.Data;
using Sqllite_Library.Helpers;
using System.Windows;
using System.Linq;

namespace Sqllite_Library.Data
{
    public static class SqlLiteData
    {

        #region == Database Methods ==
        public static void CreateDatabaseIfNotExist(bool encryptFlag, bool canCreateRegistryIfNotExists)
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

        public static bool IsDbCreated()
        {
            bool fileExists;
            string fileName = RegisteryHelper.GetFileName();
            fileExists = File.Exists(fileName);
            return fileExists;
        }

        public static void DeleteDB()
        {
            // First delete DB recursively
            var filename = RegisteryHelper.GetFileName();
            File.Delete(filename);
            MessageBox.Show($"DB Deleted Succesfully !!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion


        #region == Create Table Region ==
        private static void CreateTableHelper(string sqlQueryString, SQLiteConnection sqlCon)
        {
            var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
            sqlQuery.ExecuteNonQuery();
            sqlQuery.Dispose();

        }

        private static void CreateAllTables(SQLiteConnection sqlCon)
        {
            CreateCompanyTable(sqlCon);
            CreateBackgroundTable(sqlCon);

            CreateVoiceTimerTable(sqlCon);
            CreateVoiceAverageTable(sqlCon);

            CreateAppTable(sqlCon);
            CreateMediaTable(sqlCon);
            CreateScreenTable(sqlCon);

            CreateProjectTable(sqlCon);
            CreateProjectDetTable(sqlCon);

            CreateRequireAutofillTable(sqlCon);
            CreateObjectiveAutofillTable(sqlCon);
            CreateNextAutofillTable(sqlCon);

            CreatePlanningHeadTable(sqlCon);
            CreatePlanningTable(sqlCon);

            CreateReviewTable(sqlCon);
            CreateReviewImageTable(sqlCon);
            CreateReviewFtfTable(sqlCon);

            CreateVideoEventTable(sqlCon);
            CreateVideoSegmentTable(sqlCon);
            //CreateAudioTable(sqlCon);
            CreateNotesTable(sqlCon);
            CreateDesignTable(sqlCon);

            CreateLivAudioTable(sqlCon);
            CreateLocAudioTable(sqlCon);

            CreateFinalMp4Table(sqlCon);
        }

        private static void CreateCompanyTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_company' (
                'company_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                'company_name' TEXT(30) NOT NULL  DEFAULT 'NULL',
                UNIQUE (company_name)
                );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateBackgroundTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_background' (
                'background_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                'fk_background_company' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_company' ('company_id'),
                'background_media' BLOB NOT NULL  DEFAULT NULL,
                'background_active' INTEGER(1) NOT NULL  DEFAULT 1
                );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateVoiceTimerTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_voicetimer' (
                'voicetimer_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                'voicetimer_line' TEXT NOT NULL  DEFAULT 'NULL',
                'voicetimer_wordcount' INTEGER NOT NULL  DEFAULT 0,
                'voicetimer_index' INTEGER NOT NULL  DEFAULT 0  
                );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateVoiceAverageTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_voiceaverage' (
                'voiceaverage_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                'voiceaverage_average' TEXT NOT NULL  DEFAULT '1'
                );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateAppTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_app' (
                'app_id' INTEGER NOT NULL  DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'app_name' TEXT(5) NOT NULL DEFAULT 'Write',
                'app_active' INTEGER(1) NOT NULL  DEFAULT 0,
                UNIQUE (app_name)
                );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateMediaTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_media' (
                'media_id' INTEGER NOT NULL DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                'media_name' TEXT(20) NOT NULL  DEFAULT 'NULL', 
                'media_color' TEXT(15) NOT NULL  DEFAULT 'NULL', 
                UNIQUE (media_name)
                );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateScreenTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_screen' (
                'screen_id' INTEGER NOT NULL DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'screen_name' TEXT(20) NOT NULL DEFAULT 'NULL',
                'screen_color' TEXT(15) NOT NULL DEFAULT 'NULL',
                'screen_hexcolor' TEXT(15) NOT NULL DEFAULT 'NULL',
                UNIQUE (screen_name)
                );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateProjectTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_project' (
                    'project_id' INTEGER NOT NULL  DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                    'project_videotitle' TEXT(50) UNIQUE NOT NULL  DEFAULT 'NULL',
                    'project_currwfstep' TEXT(20) NOT NULL  DEFAULT 'NULL',
                    'project_uploaded' INTEGER(1) NOT NULL  DEFAULT 0,
                    'fk_project_background' INTEGER NOT NULL DEFAULT 1 REFERENCES 'cbv_background' ('background_id'),
                    'project_date' TEXT(25) DEFAULT NULL, 
                    'project_archived' INTEGER(1) NOT NULL DEFAULT 0,
                    'project_createdate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                    'project_modifydate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                    'project_isdeleted' INTEGER(1) NOT NULL DEFAULT 0,
                    'project_issynced' INTEGER(1) NOT NULL  DEFAULT 0,
                    'project_serverid' INTEGER NOT NULL  DEFAULT 1,
                    'project_syncerror' TEXT(50) DEFAULT NULL
                    );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateProjectDetTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE 'cbv_projdet' (
                    'projdet_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                    'fk_projdet_project' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_project' ('project_id'),
                    'projdet_version' TEXT(20) NOT NULL  DEFAULT 'NULL',
                    'projdet_currver' INTEGER(1) NOT NULL  DEFAULT 0,
                    'projdet_comments' TEXT(100) DEFAULT NULL,
                    'projdet_serverid' INTEGER NOT NULL DEFAULT 0,
                    'projdet_createdate' TEXT NOT NULL  DEFAULT 'NULL',
                    'projdet_modifydate' TEXT NOT NULL  DEFAULT 'NULL'
                    );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateRequireAutofillTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE 'cbv_requireautofill' (
                    'requireautofill_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                    'fk_requireautofill_project' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_project' ('project_id'),
                    'requireautofill_name' TEXT(50) NOT NULL  DEFAULT 'NULL',
                    'requireautofill_importance' INTEGER NOT NULL  DEFAULT 1,
                    'requireautofill_active' INTEGER(1) NOT NULL  DEFAULT 1,
                    'requireautofill_createdate' TEXT NOT NULL  DEFAULT 'NULL',
                    'requireautofill_modifydate' TEXT NOT NULL  DEFAULT 'NULL',
                    'requireautofill_serverid' INTEGER NOT NULL  DEFAULT 1,
                    'requireautofill_issynced' INTEGER(1) NOT NULL  DEFAULT 0,
                    'requireautofill_syncerror' TEXT(50) NOT NULL  DEFAULT 'NULL',
                    'requireautofill_isedited' INTEGER(1) NOT NULL  DEFAULT 0,
                    UNIQUE (fk_requireautofill_project, requireautofill_name)
                    );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateObjectiveAutofillTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE 'cbv_objectiveautofill' (
                    'objectiveautofill_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                    'fk_objectiveautofill_project' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_project' ('project_id'),
                    'objectiveautofill_name' TEXT(50) NOT NULL  DEFAULT 'NULL',
                    'objectiveautofill_active' INTEGER(1) NOT NULL  DEFAULT 1,
                    'objectiveautofill_createdate' TEXT NOT NULL  DEFAULT 'NULL',
                    'objectiveautofill_modifydate' TEXT NOT NULL  DEFAULT 'NULL',
                    'objectiveautofill_serverid' INTEGER NOT NULL  DEFAULT 1,
                    'objectiveautofill_issynced' INTEGER(1) NOT NULL  DEFAULT 0,
                    'objectiveautofill_syncerror' TEXT(50) NOT NULL  DEFAULT 'NULL',
                    'objectiveautofill_isedited' INTEGER(1) NOT NULL  DEFAULT 0,
                    UNIQUE (fk_objectiveautofill_project, objectiveautofill_name)
                    );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateNextAutofillTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE 'cbv_nextautofill' (
                    'nextautofill_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                    'fk_nextautofill_project' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_project' ('project_id'),
                    'nextautofill_name' TEXT(50) NOT NULL  DEFAULT 'NULL',
                    'nextautofill_importance' INTEGER NOT NULL  DEFAULT 1,
                    'nextautofill_active' INTEGER(1) NOT NULL  DEFAULT 1,
                    'nextautofill_createdate' TEXT NOT NULL  DEFAULT 'NULL',
                    'nextautofill_modifydate' TEXT NOT NULL  DEFAULT 'NULL',
                    'nextautofill_serverid' INTEGER NOT NULL  DEFAULT 1,
                    'nextautofill_issynced' INTEGER(1) NOT NULL  DEFAULT 0,
                    'nextautofill_syncerror' TEXT(50) NOT NULL  DEFAULT 'NULL',
                    'nextautofill_isedited' INTEGER(1) NOT NULL  DEFAULT 0,
                    UNIQUE (fk_nextautofill_project, nextautofill_name)
                    );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }


        private static void CreatePlanningHeadTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE 'cbv_planninghead' (
                        'planninghead_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                        'planninghead_name' TEXT(30) NOT NULL  DEFAULT 'NULL',
                        'planninghead_sort' TEXT NOT NULL  DEFAULT '1',
                        UNIQUE (planninghead_name)
                        );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreatePlanningTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE 'cbv_planning' (
                        'planning_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                        'fk_planning_project' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_project' ('project_id'),
                        'fk_planning_head' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_planninghead' ('planninghead_id'),
                        'planning_customname' TEXT(50) DEFAULT NULL,
                        'planning_notesline' TEXT DEFAULT NULL,
                        'planning_medialibid' TEXT(12) DEFAULT NULL,
                        'planning_sort' TEXT NOT NULL  DEFAULT '1',
                        'planning_suggestnotesline' TEXT DEFAULT NULL,
                        'planning_mediathumb' NONE DEFAULT NULL,
                        'planning_mediafull' NONE DEFAULT NULL,
                        'planning_createdate' TEXT NOT NULL  DEFAULT 'NULL',
                        'planning_modifydate' TEXT NOT NULL  DEFAULT 'NULL',
                        'planning_serverid' INTEGER NOT NULL  DEFAULT 1,
                        'planning_issynced' INTEGER(1) NOT NULL  DEFAULT 0,
                        'planning_syncerror' TEXT(50) NOT NULL  DEFAULT 'NULL',
                        'planning_isEdited' INTEGER(1) NOT NULL  DEFAULT 0,
                        UNIQUE (fk_planning_project, fk_planning_head, planning_customname)
                        );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateReviewTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE 'cbv_review' (
                    'review_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                    'fk_review_project' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_planning' ('planning_id'),
                    'review_versionnote' TEXT(100) DEFAULT NULL,
                    'review_name' TEXT(30) NOT NULL  DEFAULT 'NULL',
                    'review_videoeventsid' INTEGER DEFAULT 1,
                    'review_createdate' TEXT NOT NULL  DEFAULT 'NULL',
                    'review_modifydate' TEXT NOT NULL  DEFAULT 'NULL',
                    'review_serverid' INTEGER NOT NULL  DEFAULT 1,
                    'review_issynced' INTEGER(1) NOT NULL  DEFAULT 0,
                    'review_syncerror' TEXT(50) NOT NULL  DEFAULT 'NULL',
                    'review_isEdited' INTEGER(1) NOT NULL  DEFAULT 0
                    );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateReviewImageTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE 'cbv_reviewimg' (
                    'reviewimg_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                    'fk_reviewimg_review' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_review' ('review_id'),
                    'reviewimg_media' NONE NOT NULL  DEFAULT NULL,
                    'reviewimg_createdate' TEXT NOT NULL  DEFAULT 'NULL'
                    );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateReviewFtfTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE 'cbv_reviewftf' (
                    'reviewftf_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                    'fk_reviewftf_review' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_review' ('review_id'),
                    'reviewrtf_rtfnote' TEXT NOT NULL  DEFAULT 'NULL',
                    'reviewrtf_createdate' TEXT NOT NULL  DEFAULT 'NULL'
                    );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateVideoEventTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_videoevent'(
                'videoevent_id' INTEGER NOT NULL DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'fk_videoevent_projdet' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_projdet' ('projdet_id'),
                'fk_videoevent_media' INTEGER NOT NULL DEFAULT 1 REFERENCES 'cbv_media'('media_id'),
                'videoevent_track' INTEGER NOT NULL  DEFAULT 1,
                'videoevent_start' TEXT(12) NOT NULL DEFAULT '00:00:00.000',
                'videoevent_duration' TEXT(12) NOT NULL DEFAULT 'NULL',
                'videoevent_end' TEXT(12) NOT NULL DEFAULT '00:00:00.000',
                'videoevent_createdate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'videoevent_modifydate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'videoevent_isdeleted' INTEGER(1) NOT NULL  DEFAULT 0,
                'videoevent_issynced' INTEGER(1) NOT NULL  DEFAULT 0,
                'videoevent_serverid' INTEGER NOT NULL  DEFAULT 1,
                'videoevent_syncerror' TEXT(50) DEFAULT NULL
                );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateVideoSegmentTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_videosegment' (
                'videosegment_id' INTEGER NOT NULL DEFAULT NULL PRIMARY KEY REFERENCES 'cbv_videoevent' ('videoevent_id'),
                'videosegment_media' BLOB NOT NULL DEFAULT NULL,
                'videosegment_createdate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'videosegment_modifydate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'videosegment_isdeleted' INTEGER(1) NOT NULL DEFAULT 0,
                'videosegment_issynced' INTEGER(1) NOT NULL  DEFAULT 0,
                'videosegment_serverid' INTEGER NOT NULL  DEFAULT 1,
                'videosegment_syncerror' TEXT(50) DEFAULT NULL
                );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        /*
        private static void CreateAudioTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_audio' (
                'audio_id' INTEGER NOT NULL DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'fk_audio_videoevent' INTEGER NOT NULL  DEFAULT NULL REFERENCES 'cbv_videoevent' ('videoevent_id'),
                'audio_media' BLOB NOT NULL DEFAULT NULL,
                'audio_createdate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'audio_modifydate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'audio_isdeleted' INTEGER(1) NOT NULL DEFAULT 0
                );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }
        */

        private static void CreateNotesTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_notes' (
                'notes_id' INTEGER NOT NULL DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'fk_notes_videoevent' INTEGER NOT NULL  DEFAULT NULL REFERENCES 'cbv_videoevent' ('videoevent_id'),
                'notes_line' TEXT(255) NOT NULL  DEFAULT 'NULL',
                'notes_wordcount' INTEGER NOT NULL  DEFAULT 0,
                'notes_index' INTEGER NOT NULL DEFAULT 0,
                'notes_start' TEXT(12) NOT NULL DEFAULT '00.00.00.000',
                'notes_duration' TEXT(12) NOT NULL DEFAULT '00:00:00.000',
                'notes_createdate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'notes_modifydate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'notes_isdeleted' INTEGER(1) NOT NULL  DEFAULT 0,
                'notes_issynced' INTEGER(1) NOT NULL  DEFAULT 0,
                'notes_serverid' INTEGER NOT NULL  DEFAULT 1,
                'notes_syncerror' TEXT(50) DEFAULT NULL
                );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateLocAudioTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_locaudio' (
                'locaudio_id' INTEGER NOT NULL DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'fk_locaudio_notes' INTEGER NOT NULL DEFAULT NULL REFERENCES 'cbv_notes' ('notes_id'),
                'locaudio_media' BLOB NOT NULL DEFAULT NULL,
                'locaudio_createdate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'locaudio_modifydate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'locaudio_isdeleted' INTEGER(1) NOT NULL DEFAULT 0
                );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateLivAudioTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_livaudio' (
                'livaudio_id' INTEGER NOT NULL DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'fk_livaudio_videoevent' INTEGER NOT NULL DEFAULT NULL REFERENCES 'cbv_videoevent' ('videoevent_id')
                );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateDesignTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_design' (
                'design_id' INTEGER NOT NULL DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'fk_design_videoevent' INTEGER NOT NULL  DEFAULT NULL REFERENCES 'cbv_videoevent' ('videoevent_id'),
                'fk_design_background' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_background' ('background_id'),
                'fk_design_screen' INTEGER NOT NULL DEFAULT 1 REFERENCES 'cbv_screen'('screen_id'),
                'design_xml' TEXT(255) NOT NULL  DEFAULT 'NULL',
                'design_createdate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'design_modifydate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'design_isdeleted' INTEGER(1) NOT NULL  DEFAULT 0,
                'design_issynced' INTEGER(1) NOT NULL  DEFAULT 0,
                'design_serverid' INTEGER NOT NULL  DEFAULT 1,
                'design_syncerror' TEXT(50) DEFAULT NULL
                );";
            CreateTableHelper(sqlQueryString, sqlCon);
        }

        private static void CreateFinalMp4Table(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE 'cbv_finalmp4' (
                'finalmp4_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                'fk_finalmp4_project' INTEGER NOT NULL  DEFAULT NULL REFERENCES 'cbv_project' ('project_id'),
                'finalmp4_version' INTEGER NOT NULL  DEFAULT 1,
                'finalmp4_comments' TEXT(30) DEFAULT NULL,
                'finalmp4_media' BLOB NOT NULL  DEFAULT NULL,
                'finalmp4_createdate' TEXT(25) NOT NULL  DEFAULT '1999-01-01 00:00:00',
                'finalmp4_modifydate' TEXT(25) NOT NULL  DEFAULT '1999-01-01 00:00:00',
                'finalmp4_isdeleted' INTEGER(1) NOT NULL  DEFAULT 0,
                'finalmp4_issynced' INTEGER(1) NOT NULL  DEFAULT 0,
                'finalmp4_serverid' INTEGER NOT NULL  DEFAULT 1,
                'finalmp4_syncerror' TEXT(50) DEFAULT NULL,
                UNIQUE (fk_finalmp4_project, finalmp4_version)
                );;";
            CreateTableHelper(sqlQueryString, sqlCon);
        }


        #endregion  == Create Table Region ==


        #region == Sync Methods  ==

        public static List<int> SyncCompany(DataTable dataTable)
        {
            var insertedIds = new List<int>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var values = $"'{dr["company_name"]}'";
                var whereClause = $" NOT EXISTS(SELECT 1 FROM cbv_company WHERE company_name = '{dr["company_name"]}');";
                string sqlQueryString =
                $@"INSERT INTO cbv_company 
                    (company_name) 
                    SELECT 
                        {values}
                    WHERE
                        {whereClause}";

                var insertedId = InsertRecordsInTable("cbv_company", sqlQueryString);
                insertedIds.Add(insertedId);
            }
            return insertedIds;
        }

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
                var values = $"'{dr["screen_name"]}', '{dr["screen_color"]}, '{dr["screen_hexcolor"]}'";
                var whereClause = $" NOT EXISTS(SELECT 1 FROM cbv_screen WHERE screen_name = '{dr["screen_name"]}');";
                string sqlQueryString =
                    $@"INSERT INTO cbv_screen 
                        (screen_name, screen_color, screen_hexcolor) 
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

        #endregion


        #region == Insert Methods ==

        public static int InsertRowsToCompany(DataTable dataTable)
        {
            var values = new List<string>();
            foreach (DataRow dr in dataTable.Rows)
            {
                values.Add($"('{dr["company_name"]}')");
            }
            var valuesString = string.Join(",", values.ToArray());

            string sqlQueryString =
                $@"INSERT INTO cbv_company 
                    (company_name) 
                VALUES 
                    {valuesString}";

            var insertedId = InsertRecordsInTable("cbv_company", sqlQueryString);
            return insertedId;
        }

        public static int InsertRowsToBackground(DataTable dataTable)
        {
            var insertedId = -1;
            foreach (DataRow dr in dataTable.Rows)
            {
                var values = new List<string>();
                values.Add($"({dr["fk_background_company"]}, {dr["background_active"]}, @blob)");
                var valuesString = string.Join(",", values.ToArray());

                string sqlQueryString =
                    $@"INSERT INTO cbv_background 
                    (fk_background_company, background_active, background_media) 
                VALUES 
                    {valuesString}";

                insertedId = InsertBlobRecordsInTable("cbv_background", sqlQueryString, dr["background_media"] as byte[]);
            }

            return insertedId;
        }

        public static int InsertRowsToProject(DataTable dataTable)
        {
            var values = new List<string>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var projectUploaded = Convert.ToBoolean(dr["project_uploaded"]);

                var projectDate = Convert.ToString(dr["project_date"]);
                if (string.IsNullOrEmpty(projectDate))
                    projectDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var projectArchived = Convert.ToBoolean(dr["project_archived"]);

                var createDate = Convert.ToString(dr["project_createdate"]);
                if (string.IsNullOrEmpty(createDate))
                    createDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["project_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var issynced = Convert.ToInt16(dr["project_issynced"]);
                var serverid = Convert.ToInt64(dr["project_serverid"]);

                var syncErrorString = Convert.ToString(dr["project_syncerror"]);
                var syncerror = syncErrorString?.Length > 50 ? syncErrorString.Substring(0, 50) : syncErrorString;

                var fkProjectBackground = Convert.ToBoolean(dr["fk_project_background"]);
                values.Add($"('{dr["project_videotitle"]}', {projectUploaded}, '{projectDate}', {projectArchived}, {fkProjectBackground}," +
                    $" '{createDate}', '{modifyDate}', 0, {issynced}, {serverid}, '{syncerror}')");
            }
            var valuesString = string.Join(",", values.ToArray());
            string sqlQueryString =
                $@"INSERT INTO  cbv_project 
                    (project_videotitle, project_uploaded, project_date, project_archived, fk_project_background, project_createdate, project_modifydate, 
                        project_isdeleted, project_issynced, project_serverid, project_syncerror) 
                VALUES 
                    {valuesString}";

            var insertedId = InsertRecordsInTable("cbv_project", sqlQueryString);
            return insertedId;
        }


        #region == Video Event and Depenedent Tables ==

        public static string GetNextStart(int fk_videoevent_media, int projdetId)
        {
            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            SQLiteConnection sqlCon = null;
            string result;

            string fileName = RegisteryHelper.GetFileName();

            // Open Database connection 
            sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
            sqlCon.Open();

            string sqlQueryString = $@"Select max(videoevent_end) from cbv_videoevent ";
            var whereClause = string.Empty;

            if (fk_videoevent_media <= 4) whereClause = " where (fk_videoevent_media = 1 or fk_videoevent_media = 2 or fk_videoevent_media = 4) ";
            else whereClause = $" where fk_videoevent_media = {fk_videoevent_media}";


            whereClause += $" and fk_videoevent_projdet = {projdetId} and videoevent_isdeleted = 0 and videoevent_track = 2";

            var sqlQuery = new SQLiteCommand(sqlQueryString + whereClause, sqlCon);
            result = Convert.ToString(sqlQuery.ExecuteScalar());
            sqlQuery.Dispose();
            sqlCon.Close();
            return string.IsNullOrEmpty(result) ? "00:00:00.000" : result;
        }

        public static int GetMillisecondsFromTimespan(string timespan)
        {
            var ms = timespan.Length > 8 ? timespan.Split('.')[1] : "000";
            var timeArray = timespan.Length > 8 ? timespan.Remove(8).Split(':') : timespan.Split(':');
            var timeinSecond = (Convert.ToInt32(timeArray[0]) * 3600) + (Convert.ToInt32(timeArray[1]) * 60) + (Convert.ToInt32(timeArray[2]));
            var timeInMillisecond = (timeinSecond * 1000) + Convert.ToInt32(ms);
            return timeInMillisecond;
        }

        public static string ShiftRight(string start, string duration)
        {
            if (string.IsNullOrEmpty(start))
                return "00:00:00.000";

            var ms = start.Length > 8 ? start.Split('.')[1] : "000";
            
            
            var array = start.Length > 8 ? start.Remove(8).Split(':') : start.Split(':');
            var startTimeinSecond = (Convert.ToInt32(array[0]) * 3600) + (Convert.ToInt32(array[1]) * 60) + (Convert.ToInt32(array[2]));
            //var time = startTimeinSecond * 1000 + ms;
            var starttime = GetMillisecondsFromTimespan(start);
            var durationtime = GetMillisecondsFromTimespan(duration);

            var endTime = starttime + durationtime;

            var endtimeInMs = endTime % 1000;
            endTime = endTime / 1000;
            int s = endTime % 60;
            endTime /= 60;
            int mins = endTime % 60;
            int hours = endTime / 60;
            return string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", hours, mins, s, endtimeInMs);
        }

        public static string ShiftLeft(string time, string durationToShift)
        {
            if (string.IsNullOrEmpty(time))
                return "00:00:00.000";

            var ms = time.Length > 8 ? time.Split('.')[1] : "000";


            var array = time.Length > 8 ? time.Remove(8).Split(':') : time.Split(':');
            var startTimeinSecond = (Convert.ToInt32(array[0]) * 3600) + (Convert.ToInt32(array[1]) * 60) + (Convert.ToInt32(array[2]));
            //var time = startTimeinSecond * 1000 + ms;
            var starttime = GetMillisecondsFromTimespan(time);
            var durationtime = GetMillisecondsFromTimespan(durationToShift);

            var endTime = starttime - durationtime;
            if(endTime < 0)
                return "00:00:00.000";
            var endtimeInMs = endTime % 1000;
            endTime = endTime / 1000;
            int s = endTime % 60;
            endTime /= 60;
            int mins = endTime % 60;
            int hours = endTime / 60;
            return string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", hours, mins, s, endtimeInMs);
        }

        public static int InsertRowsToVideoEvent(DataRow dr)
        {
            var values = new List<string>();

            var createDate = Convert.ToString(dr["videoevent_createdate"]);
            if (string.IsNullOrEmpty(createDate))
                createDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var modifyDate = Convert.ToString(dr["videoevent_modifydate"]);
            if (string.IsNullOrEmpty(modifyDate))
                modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var start = Convert.ToString(dr["videoevent_start"]);
            //if (string.IsNullOrEmpty(start) || start == "00:00:00.000" || start == "00:00:00")
            //    start = GetNextStart(Convert.ToInt32(dr["fk_videoevent_media"]), Convert.ToInt32(dr["fk_videoevent_projdet"]));

            var end = ShiftRight(start, Convert.ToString(dr["videoevent_duration"]));

            var issynced = Convert.ToInt16(dr["videoevent_issynced"]);
            var serverid = Convert.ToInt64(dr["videoevent_serverid"]);

            var syncErrorString = Convert.ToString(dr["videoevent_syncerror"]);
            var syncerror = syncErrorString?.Length > 50 ? syncErrorString.Substring(0, 50) : syncErrorString;

            values.Add($"({dr["fk_videoevent_projdet"]}, {dr["fk_videoevent_media"]}, {dr["videoevent_track"]}, " +
                $"'{start}', '{dr["videoevent_duration"]}', '{end}', '{createDate}', '{modifyDate}', 0, {issynced}, {serverid}, '{syncerror}')");

            var valuesString = string.Join(",", values.ToArray());
            string sqlQueryString =
                $@"INSERT INTO cbv_videoevent 
                    (fk_videoevent_projdet, fk_videoevent_media, videoevent_track, 
                        videoevent_start, videoevent_duration, videoevent_end, videoevent_createdate, videoevent_modifydate, 
                        videoevent_isdeleted, videoevent_issynced, videoevent_serverid, videoevent_syncerror) 
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
                    createDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["videosegment_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var issynced = Convert.ToInt16(dr["videosegment_issynced"]);
                var serverid = Convert.ToInt64(dr["videosegment_serverid"]);
                var syncErrorString = Convert.ToString(dr["videosegment_syncerror"]);
                var syncerror = syncErrorString?.Length > 50 ? syncErrorString.Substring(0, 50) : syncErrorString;

                values.Add($"({dr["videosegment_id"]}, @blob, '{createDate}', '{modifyDate}', 0, {issynced}, {serverid}, '{syncerror}')");
            }
            var valuesString = string.Join(",", values.ToArray());

            string sqlQueryString =
                $@"INSERT INTO cbv_videosegment 
                    (videosegment_id, videosegment_media, videosegment_createdate, videosegment_modifydate, 
                            videosegment_isdeleted, videosegment_issynced, videosegment_serverid, videosegment_syncerror) 
                VALUES 
                    {valuesString}";

            var insertedId = InsertBlobRecordsInTable("cbv_videosegment", sqlQueryString, dataTable.Rows[0]["videosegment_media"] as byte[]); // This does not have seq
            return 1;
        }

        /*
        public static int InsertRowsToAudio(DataTable dataTable)
        {
            var values = new List<string>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var createDate = Convert.ToString(dr["audio_createdate"]);
                if (string.IsNullOrEmpty(createDate))
                    createDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["audio_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                values.Add($"({dr["fk_audio_videoevent"]}, @blob, '{createDate}', '{modifyDate}', 0)");
            }
            var valuesString = string.Join(",", values.ToArray());

            string sqlQueryString =
                $@"INSERT INTO cbv_audio 
                    (fk_audio_videoevent, audio_media, audio_createdate, audio_modifydate, audio_isdeleted) 
                VALUES 
                    {valuesString}";

            var insertedId = InsertBlobRecordsInTable("cbv_audio", sqlQueryString, dataTable.Rows[0]["audio_media"] as byte[]);
            return insertedId;
        }
        */

        public static int InsertRowsToDesign(DataTable dataTable)
        {
            var values = new List<string>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var createDate = Convert.ToString(dr["design_createdate"]);
                if (string.IsNullOrEmpty(createDate))
                    createDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["design_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var issynced = Convert.ToInt16(dr["design_issynced"]);
                var serverid = Convert.ToInt64(dr["design_serverid"]);
                var syncErrorString = Convert.ToString(dr["design_syncerror"]);
                var syncerror = syncErrorString?.Length > 50 ? syncErrorString.Substring(0, 50) : syncErrorString;

                values.Add($"({dr["fk_design_videoevent"]}, {dr["fk_design_background"]}, {dr["fk_design_screen"]}, '{dr["design_xml"]}', '{createDate}', '{modifyDate}'" +
                            $", 0, {issynced}, {serverid}, '{syncerror}')");
            }
            var valuesString = string.Join(",", values.ToArray());

            string sqlQueryString =
                $@"INSERT INTO cbv_design 
                    (fk_design_videoevent, fk_design_background, fk_design_screen, design_xml, design_createdate, design_modifydate, 
                        design_isdeleted, design_issynced, design_serverid, design_syncerror) 
                VALUES 
                    {valuesString}";

            var insertedId = InsertRecordsInTable("cbv_design", sqlQueryString);
            return insertedId;
        }

        public static int InsertRowsToNotes(DataTable dataTable)
        {
            var insertedId = 0;
            foreach (DataRow dr in dataTable.Rows)
            {

                //(Select Max(notes_index) + 1 from cbv_notes where fk_notes_videoevent = 1) as index
                var max_index = GetMaxIndexForNotes(Convert.ToInt32(dr["fk_notes_videoevent"]));
                var createDate = Convert.ToString(dr["notes_createdate"]);
                if (string.IsNullOrEmpty(createDate))
                    createDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["notes_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var issynced = Convert.ToInt16(dr["notes_issynced"]);
                var serverid = Convert.ToInt64(dr["notes_serverid"]);
                var syncErrorString = Convert.ToString(dr["notes_syncerror"]);
                var syncerror = syncErrorString?.Length > 50 ? syncErrorString.Substring(0, 50) : syncErrorString;
                var notes_line = Convert.ToString(dr["notes_line"]).Trim('\'');

                var values = new List<string>();
                values.Add($"({dr["fk_notes_videoevent"]}, '{notes_line}', {dr["notes_wordcount"]}, {max_index}, '{dr["notes_start"]}', " +
                         $"'{dr["notes_duration"]}', '{createDate}', '{modifyDate}', 0, {issynced}, {serverid}, '{syncerror}')");
                var valuesString = string.Join(",", values.ToArray());

                string sqlQueryString =
                    $@"INSERT INTO cbv_notes 
                    (fk_notes_videoevent, notes_line, notes_wordcount, notes_index, notes_start, notes_duration, notes_createdate, notes_modifydate, 
                        notes_isdeleted, notes_issynced, notes_serverid, notes_syncerror) 
                VALUES 
                    {valuesString}";

                insertedId = InsertRecordsInTable("cbv_notes", sqlQueryString);
            }

            return insertedId;
        }

        #endregion

        public static List<int> InsertRowsToLocAudio(DataTable dataTable)
        {
            var insertedIds = new List<int>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var values = new List<string>();
                var createDate = Convert.ToString(dr["locaudio_createdate"]);
                if (string.IsNullOrEmpty(createDate))
                    createDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["locaudio_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                values.Add($"({dr["fk_locaudio_notes"]}, @blob, '{createDate}', '{modifyDate}', 0)");

                var valuesString = string.Join(",", values.ToArray());

                string sqlQueryString =
                    $@"INSERT INTO cbv_locaudio 
                    (fk_locaudio_notes, locaudio_media, locaudio_createdate, locaudio_modifydate, locaudio_isdeleted) 
                VALUES 
                    {valuesString}";

                var insertedId = InsertBlobRecordsInTable("cbv_locaudio", sqlQueryString, dataTable.Rows[0]["locaudio_media"] as byte[]);
                insertedIds.Add(insertedId);
            }
            return insertedIds;
        }


        #region == FinalMp4 Tables ==

        public static int InsertRowsToFinalMp4(DataTable dataTable)
        {
            var values = new List<string>();
            var insertedId = -1;
            foreach (DataRow dr in dataTable.Rows)
            {
                var createDate = Convert.ToString(dr["finalmp4_createdate"]);
                if (string.IsNullOrEmpty(createDate))
                    createDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["finalmp4_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var issynced = Convert.ToInt16(dr["finalmp4_issynced"]);
                var serverid = Convert.ToInt64(dr["finalmp4_serverid"]);
                var syncErrorString = Convert.ToString(dr["finalmp4_syncerror"]);
                var syncerror = syncErrorString?.Length > 50 ? syncErrorString.Substring(0, 50) : syncErrorString;

                values.Add($"({dr["fk_finalmp4_project"]}, {dr["finalmp4_version"]}, '{dr["finalmp4_comments"]}', @blob, '{createDate}', '{modifyDate}', " +
                    $"0, {issynced}, {serverid}, '{syncerror}')");
                var valuesString = string.Join(",", values.ToArray());

                string sqlQueryString =
                    $@"INSERT INTO cbv_finalmp4 
                    (fk_finalmp4_project, finalmp4_version, finalmp4_comments, finalmp4_media, finalmp4_createdate, finalmp4_modifydate, 
                        finalmp4_isdeleted, finalmp4_issynced, finalmp4_serverid, finalmp4_syncerror) 
                VALUES 
                    {valuesString}";

                insertedId = InsertBlobRecordsInTable("cbv_finalmp4", sqlQueryString, dr["finalmp4_media"] as byte[]);
            }

            return insertedId;
        }

        #endregion

        #region == Voice tables ==
        public static List<int> InsertRowsToVoiceTimer(DataTable dataTable)
        {
            var insertedIds = new List<int>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var values = new List<string>
                {
                    $"('{dr["voicetimer_line"]}', {dr["voicetimer_wordcount"]}, {dr["voicetimer_index"]})"
                };
                var valuesString = string.Join(",", values.ToArray());

                string sqlQueryString =
                    $@"INSERT INTO cbv_voicetimer 
                        (voicetimer_line, voicetimer_wordcount, voicetimer_index) 
                    VALUES 
                        {valuesString}";

                var id = InsertRecordsInTable("cbv_voicetimer", sqlQueryString);
                insertedIds.Add(id);
            }

            return insertedIds;
        }

        public static List<int> InsertRowsToVoiceAverage(DataTable dataTable)
        {
            var insertedIds = new List<int>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var values = new List<string>
                {
                    $"('{dr["voiceaverage_average"]}')"
                };
                var valuesString = string.Join(",", values.ToArray());

                string sqlQueryString =
                    $@"INSERT INTO cbv_voiceaverage 
                        (voiceaverage_average) 
                    VALUES 
                        {valuesString}";

                var id = InsertRecordsInTable("cbv_voiceaverage", sqlQueryString);
                insertedIds.Add(id);
            }

            return insertedIds;
        }

        #endregion

        #region == Insert Helper ==

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
            catch (Exception)
            {
                if (null != sqlCon)
                    sqlCon.Close();
                throw;
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

        private static int Insert2BlobRecordsInTable(string tableName, string InsertQuery, byte[] blob1, byte[] blob2)
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
                    command.Parameters.Add(new SQLiteParameter("blob1", DbType.Binary) { Value = blob1 });
                    command.Parameters.Add(new SQLiteParameter("blob2", DbType.Binary) { Value = blob2 });
                    command.ExecuteNonQuery();
                }

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
            catch (Exception)
            {
                if (null != sqlCon)
                    sqlCon.Close();
                throw;
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
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }
            return id;
        }

        #endregion

        #endregion


        #region == GET Methods ==

        private static void CloseConnections(this SqlConnectionModel sqlObject)
        {
            sqlObject?.SQLiteDataReader?.Close();
            sqlObject?.SQLiteCommand?.Dispose();
            sqlObject?.SQLiteConnection?.Close();
        }

        private static SqlConnectionModel RunSelectQuery(this string sqlQueryString)
        {
            var sqlObject = new SqlConnectionModel();
            try
            {
                // Check if database is created
                if (false == IsDbCreated())
                    throw new Exception("Database is not present.");

                string fileName = RegisteryHelper.GetFileName();

                // Open Database connection 
                sqlObject.SQLiteConnection = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
                sqlObject.SQLiteConnection.Open();

                sqlObject.SQLiteCommand = new SQLiteCommand(sqlQueryString, sqlObject.SQLiteConnection);
                sqlObject.SQLiteDataReader = sqlObject.SQLiteCommand.ExecuteReader();
                return sqlObject;
            }
            catch
            {
                CloseConnections(sqlObject);
            }
            return null;
        }

        public static int GetMaxIndexForNotes(int fkVideoEventId)
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

                string sqlQueryString = $@"Select IFNULL(Max(notes_index),0) + 1 from cbv_notes where fk_notes_videoevent = {fkVideoEventId} and notes_isdeleted = 0";
                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                count = Convert.ToInt32(sqlQuery.ExecuteScalar());
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
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
                sqlCon.Close();
            }
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }

            return count;
        }

        public static int GetVideoEventCountProjectAndDetailId(int ProjectId, int ProjectdetailId)
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

                string sqlQueryString = $@"SELECT 
	                                            count(*) 
                                            FROM 
	                                            cbv_videoevent V
	                                            Left join cbv_projdet pd on pd.projdet_id = v.fk_videoevent_projdet
                                            where 
	                                            v.fk_videoevent_projdet = {ProjectdetailId}
	                                            And pd.fk_projdet_project = {ProjectId}";
                var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
                count = Convert.ToInt32(sqlQuery.ExecuteScalar());
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }

            return count;
        }

        public static List<CBVCompany> GetCompany()
        {
            var data = new List<CBVCompany>();
            string sqlQueryString = $@"SELECT * FROM cbv_company";
            var sqlObject = sqlQueryString.RunSelectQuery();
            if (sqlObject != null)
            {
                while (sqlObject.SQLiteDataReader.Read())
                {
                    var obj = new CBVCompany
                    {
                        company_id = sqlObject.SQLiteDataReader.GetInt32(0),
                        company_name = sqlObject.SQLiteDataReader.GetString(1)
                    };
                    data.Add(obj);
                }
                sqlObject.CloseConnections();
            }
            return data;
        }

        public static List<CBVBackground> GetBackground(int company_id = -1)
        {
            var data = new List<CBVBackground>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_background ";
            if (company_id > -1)
                sqlQueryString += $@" where fk_background_company = {company_id} ";
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
                        var obj = new CBVBackground
                        {
                            background_id = Convert.ToInt32(sqlReader["background_id"]),
                            fk_background_company = Convert.ToInt32(sqlReader["fk_background_company"]),
                            background_active = Convert.ToBoolean(sqlReader["background_active"]),
                            background_media = GetBlobMedia(sqlReader, "background_media")
                        };
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }

            return data;
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
                            screen_hexcolor = sqlReader.GetString(3),
                        };
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                if (null != sqlCon)
                    sqlCon.Close();
                throw;
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
            catch (Exception)
            {
                if (null != sqlCon)
                    sqlCon.Close();
                throw;
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
            catch (Exception)
            {
                if (null != sqlCon)
                    sqlCon.Close();
                throw;
            }

            return data;
        }

        public static List<CBVPlanningHead> GetPlanningHead()
        {
            var data = new List<CBVPlanningHead>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_planninghead";

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
                        var obj = new CBVPlanningHead
                        {
                            planninghead_id = Convert.ToInt32(sqlReader["planninghead_id"]),
                            planninghead_name = Convert.ToString(sqlReader["planninghead_name"]),
                            planninghead_sort = Convert.ToInt32(sqlReader["planninghead_sort"]),
                        };
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                if (null != sqlCon)
                    sqlCon.Close();
                throw;
            }

            return data;
        }

        public static int IsProjectPlanningAvailable(int projectId)
        {
            int planning_id = -1;
            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");
            string sqlQueryString = $@"SELECT planning_id FROM cbv_planning Where fk_planning_project = {projectId} Limit 1";

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
                        planning_id = Convert.ToInt32(sqlReader["planning_id"]);
                        return planning_id;
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                if (null != sqlCon)
                    sqlCon.Close();
                throw;
            }
            return planning_id;
        }



        public static List<CBVPlanning> GetPlanning(int projectId)
        {
            var data = new List<CBVPlanning>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_planning where fk_planning_project = {projectId}";

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
                        var obj = new CBVPlanning
                        {
                            planning_id = Convert.ToInt32(sqlReader["planning_id"]),
                            fk_planning_project = Convert.ToInt32(sqlReader["fk_planning_project"]),
                            fk_planning_head = Convert.ToInt32(sqlReader["fk_planning_head"]),
                            planning_customname = Convert.ToString(sqlReader["planning_customname"]),
                            planning_notesline = Convert.ToString(sqlReader["planning_notesline"]),
                            planning_medialibid = Convert.ToInt32(sqlReader["planning_medialibid"]),
                            planning_sort = Convert.ToInt32(sqlReader["planning_sort"]),
                            planning_suggestnotesline = Convert.ToString(sqlReader["planning_suggestnotesline"]),
                            planning_mediafull = GetBlobMedia(sqlReader, "planning_mediafull"),
                            planning_mediathumb = GetBlobMedia(sqlReader, "planning_mediathumb"),
                            planning_createdate = Convert.ToDateTime(sqlReader["planning_createdate"]),
                            planning_modifydate = Convert.ToDateTime(sqlReader["planning_modifydate"]),

                            planning_serverid = Convert.ToInt64(sqlReader["planning_serverid"]),
                            planning_issynced = Convert.ToBoolean(sqlReader["planning_issynced"]),
                            planning_syncerror = Convert.ToString(sqlReader["planning_syncerror"]),
                            planning_isEdited = Convert.ToBoolean(sqlReader["planning_isEdited"]),

                        };
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                if (null != sqlCon)
                    sqlCon.Close();
                throw;
            }

            return data;
        }

        public static int IsProjectAvailable(int projectServerId)
        {
            int project_id = -1;
            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");
            string sqlQueryString = $@"SELECT project_id FROM cbv_project Where project_serverid = {projectServerId}";

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
                        project_id = Convert.ToInt32(sqlReader["project_id"]);
                        return project_id;
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                if (null != sqlCon)
                    sqlCon.Close();
                throw;
            }
            return project_id;
        }

        public static List<CBVProjectForJoin> GetDownloadedProjectList()
        {
            var projects = new List<CBVProjectForJoin>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");
            string sqlQueryString = $@"SELECT 
                                                P.project_id, P.project_videotitle, P.project_date, P.project_serverid, PD.projdet_version
                                            FROM cbv_project P
                                                JOIN cbv_projdet PD on PD.fk_projdet_project = P.project_id";

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
                        var project = new CBVProjectForJoin
                        {
                            project_id = Convert.ToInt32(sqlReader["project_id"]),
                            project_videotitle = Convert.ToString(sqlReader["project_videotitle"]),
                            project_version = Convert.ToString(sqlReader["projdet_version"]),
                            project_date = Convert.ToDateTime(sqlReader["project_date"]),
                            project_serverid = Convert.ToInt64(sqlReader["project_serverid"]),
                        };
                        projects.Add(project);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                if (null != sqlCon)
                    sqlCon.Close();
                throw;
            }

            return projects;



        }

        //   public static List<CBVWIPOrArchivedProjectList> GetWIPOrArchivedProjectList(bool archivedFlag, bool wipFlag = false)
        //   {
        //       var projects = new List<CBVWIPOrArchivedProjectList>();

        //       // Check if database is created
        //       if (false == IsDbCreated())
        //           throw new Exception("Database is not present.");
        //       string sqlQueryString = "SELECT project_id, project_serverid, project_videotitle, project_version, project_date FROM cbv_project ";

        //       if (wipFlag)
        //           sqlQueryString = $@"
        //               SELECT P.project_id, project_serverid, P.project_videotitle, P.project_version, P.project_date, 
        //                       CASE 
        //                  When VECount.VideoEventCount is NULL Then 0
        //                  When VECount.VideoEventCount = 0 Then 0
        //                  Else 1
        //                 End project_started,
        //                 CASE 
        //                     When VECount.VideoEventCount Is Null Then 0
        //                     Else VECount.VideoEventCount 
        //                 End project_videoeventcount
        //               FROM cbv_project P
        //            /* LEFT JOIN (
        //                           Select fk_videoevent_project, count(fk_videoevent_project) as VideoEventCount
        //                           from cbv_videoevent VE 
        //                  group by fk_videoevent_project
        //                           ) as VECount on VECount.fk_videoevent_project = P.project_id */
        //" ;

        //       sqlQueryString += $@" WHERE project_archived={archivedFlag} and project_isdeleted = 0";

        //       SQLiteConnection sqlCon = null;
        //       try
        //       {
        //           string fileName = RegisteryHelper.GetFileName();

        //           // Open Database connection 
        //           sqlCon = new SQLiteConnection("Data Source=" + fileName + ";Version=3;");
        //           sqlCon.Open();

        //           var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
        //           using (var sqlReader = sqlQuery.ExecuteReader())
        //           {
        //               while (sqlReader.Read())
        //               {
        //                   var project = new CBVWIPOrArchivedProjectList
        //                   {
        //                       project_id = Convert.ToInt32(sqlReader["project_id"]),
        //                       project_serverid = Convert.ToInt32(sqlReader["project_serverid"]),
        //                       project_videotitle = Convert.ToString(sqlReader["project_videotitle"]),
        //                       project_version = Convert.ToInt32(sqlReader["project_version"]),
        //                       project_date = Convert.ToDateTime(sqlReader["project_date"]),
        //                   };
        //                   if (wipFlag)
        //                   {
        //                       project.project_started = Convert.ToBoolean(sqlReader["project_started"]);
        //                       project.project_videoeventcount = Convert.ToInt32(sqlReader["project_videoeventcount"]);
        //                   }
        //                   projects.Add(project);
        //               }
        //           }
        //           // Close database
        //           sqlQuery.Dispose();
        //           sqlCon.Close();
        //       }
        //       catch (Exception)
        //       {
        //           if (null != sqlCon)
        //               sqlCon.Close();
        //           throw;
        //       }

        //       return projects;
        //   }

        public static List<CBVProjdet> GetProjectDetails(int projectId)
        {
            var data = new List<CBVProjdet>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_projdet where  fk_projdet_project = {projectId}";


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
                        var obj = new CBVProjdet
                        {
                            projdet_id = Convert.ToInt32(sqlReader["projdet_id"]),
                            fk_projdet_project = Convert.ToInt32(sqlReader["fk_projdet_project"]),
                            projdet_version = Convert.ToString(sqlReader["projdet_version"]),
                            projdet_currver = Convert.ToBoolean(sqlReader["projdet_currver"]),
                            projdet_comments = Convert.ToString(sqlReader["projdet_comments"]),
                            projdet_serverid = Convert.ToInt64(sqlReader["projdet_serverid"]),
                            projdet_createdate = Convert.ToDateTime(sqlReader["projdet_createdate"]),
                            projdet_modifydate = Convert.ToDateTime(sqlReader["projdet_modifydate"]),
                        };
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }

            return data;
        }

        public static List<CBVProject> GetProjectById(int projectId, bool projdetFlag)
        {
            var data = new List<CBVProject>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_project where project_id = {projectId}";
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
                        var obj = new CBVProject
                        {
                            project_id = Convert.ToInt32(sqlReader["project_id"]),
                            project_videotitle = Convert.ToString(sqlReader["project_videotitle"]),
                            project_currwfstep = Convert.ToString(sqlReader["project_currwfstep"]),
                            project_uploaded = Convert.ToBoolean(sqlReader["project_uploaded"]),
                            fk_project_background = Convert.ToInt32(sqlReader["fk_project_background"]),
                            project_date = Convert.ToDateTime(sqlReader["project_date"]),
                            project_archived = Convert.ToBoolean(sqlReader["project_archived"]),
                            project_createdate = Convert.ToDateTime(sqlReader["project_createdate"]),
                            project_modifydate = Convert.ToDateTime(sqlReader["project_modifydate"]),
                            project_isdeleted = Convert.ToBoolean(sqlReader["project_isdeleted"]),
                            project_issynced = Convert.ToBoolean(sqlReader["project_issynced"]),
                            project_serverid = Convert.ToInt64(sqlReader["project_serverid"]),
                            project_syncerror = Convert.ToString(sqlReader["project_syncerror"])

                        };
                        if (projdetFlag)
                        {
                            var projdetails = GetProjectDetails(projectId);
                            obj.projdet_data = projdetails;
                        }
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }

            return data;
        }

        #region == Autofill ==
        public static List<CBVNextAutofill> GetNextAutofillById(int NextAutofillId = -1, bool onlyActive = true)
        {
            var data = new List<CBVNextAutofill>();

            string selectClause = $@"SELECT * FROM cbv_nextautofill ";
            var whereClause = "";

            if (NextAutofillId > 0) whereClause += $@" where fk_nextautofill_project = {NextAutofillId}";
            if (onlyActive)
            {
                if (string.IsNullOrEmpty(whereClause))
                    whereClause += $@" where nextautofill_active = true";
                else
                    whereClause += $@" and nextautofill_active = true";
            }
            var sqlObject = (selectClause + whereClause).RunSelectQuery();
            if (sqlObject != null)
            {
                while (sqlObject.SQLiteDataReader.Read())
                {
                    var obj = new CBVNextAutofill();
                    obj.nextautofill_id = Convert.ToInt32(sqlObject.SQLiteDataReader["nextautofill_id"]);
                    obj.fk_nextautofill_project = Convert.ToInt32(sqlObject.SQLiteDataReader["fk_nextautofill_project"]);
                    obj.nextautofill_name = Convert.ToString(sqlObject.SQLiteDataReader["nextautofill_name"]);
                    obj.nextautofill_importance = Convert.ToInt32(sqlObject.SQLiteDataReader["nextautofill_importance"]);
                    obj.nextautofill_active = Convert.ToBoolean(sqlObject.SQLiteDataReader["nextautofill_active"]);
                    obj.nextautofill_createdate = Convert.ToDateTime(sqlObject.SQLiteDataReader["nextautofill_createdate"]);
                    obj.nextautofill_modifydate = Convert.ToDateTime(sqlObject.SQLiteDataReader["nextautofill_modifydate"]);
                    obj.nextautofill_serverid = Convert.ToInt64(sqlObject.SQLiteDataReader["nextautofill_serverid"]);
                    obj.nextautofill_issynced = Convert.ToBoolean(sqlObject.SQLiteDataReader["nextautofill_issynced"]);
                    obj.nextautofill_syncerror = Convert.ToString(sqlObject.SQLiteDataReader["nextautofill_syncerror"]);
                    obj.nextautofill_isedited = Convert.ToBoolean(sqlObject.SQLiteDataReader["nextautofill_isedited"]);
                    data.Add(obj);
                }
                sqlObject.CloseConnections();
            }
            return data;
        }

        public static List<CBVRequireAutofill> GetRequireAutofillById(int RequireAutofillId = -1, bool onlyActive = true)
        {
            var data = new List<CBVRequireAutofill>();

            string selectClause = $@"SELECT * FROM cbv_requireautofill ";
            var whereClause = "";

            if (RequireAutofillId > 0) whereClause += $@" where fk_requireautofill_project = {RequireAutofillId}";
            if (onlyActive)
            {
                if (string.IsNullOrEmpty(whereClause))
                    whereClause += $@" where requireautofill_active = true";
                else
                    whereClause += $@" and requireautofill_active = true";
            }
            var sqlObject = (selectClause + whereClause).RunSelectQuery();
            if (sqlObject != null)
            {
                while (sqlObject.SQLiteDataReader.Read())
                {
                    var obj = new CBVRequireAutofill();
                    obj.requireautofill_id = Convert.ToInt32(sqlObject.SQLiteDataReader["requireautofill_id"]);
                    obj.fk_requireautofill_project = Convert.ToInt32(sqlObject.SQLiteDataReader["fk_requireautofill_project"]);
                    obj.requireautofill_name = Convert.ToString(sqlObject.SQLiteDataReader["requireautofill_name"]);
                    obj.requireautofill_importance = Convert.ToInt32(sqlObject.SQLiteDataReader["requireautofill_importance"]);
                    obj.requireautofill_active = Convert.ToBoolean(sqlObject.SQLiteDataReader["requireautofill_active"]);
                    obj.requireautofill_createdate = Convert.ToDateTime(sqlObject.SQLiteDataReader["requireautofill_createdate"]);
                    obj.requireautofill_modifydate = Convert.ToDateTime(sqlObject.SQLiteDataReader["requireautofill_modifydate"]);
                    obj.requireautofill_serverid = Convert.ToInt64(sqlObject.SQLiteDataReader["requireautofill_serverid"]);
                    obj.requireautofill_issynced = Convert.ToBoolean(sqlObject.SQLiteDataReader["requireautofill_issynced"]);
                    obj.requireautofill_syncerror = Convert.ToString(sqlObject.SQLiteDataReader["requireautofill_syncerror"]);
                    obj.requireautofill_isedited = Convert.ToBoolean(sqlObject.SQLiteDataReader["requireautofill_isedited"]);
                    data.Add(obj);
                }
                sqlObject.CloseConnections();
            }
            return data;
        }

        public static List<CBVObjectiveAutofill> GetObjectiveAutofillById(int ObjectiveAutofillId = -1, bool onlyActive = true)
        {
            var data = new List<CBVObjectiveAutofill>();

            string selectClause = $@"SELECT * FROM cbv_objectiveautofill ";
            var whereClause = "";

            if (ObjectiveAutofillId > 0) whereClause += $@" where fk_objectiveautofill_project = {ObjectiveAutofillId}";
            if (onlyActive)
            {
                if (string.IsNullOrEmpty(whereClause))
                    whereClause += $@" where objectiveautofill_active = true";
                else
                    whereClause += $@" and objectiveautofill_active = true";
            }
            var sqlObject = (selectClause + whereClause).RunSelectQuery();
            if (sqlObject != null)
            {
                while (sqlObject.SQLiteDataReader.Read())
                {
                    var obj = new CBVObjectiveAutofill();
                    obj.objectiveautofill_id = Convert.ToInt32(sqlObject.SQLiteDataReader["objectiveautofill_id"]);
                    obj.fk_objectiveautofill_project = Convert.ToInt32(sqlObject.SQLiteDataReader["fk_objectiveautofill_project"]);
                    obj.objectiveautofill_name = Convert.ToString(sqlObject.SQLiteDataReader["objectiveautofill_name"]);
                    // obj.objectiveautofill_importance = Convert.ToInt32(sqlObject.SQLiteDataReader["objectiveautofill_importance"]);
                    obj.objectiveautofill_active = Convert.ToBoolean(sqlObject.SQLiteDataReader["objectiveautofill_active"]);
                    obj.objectiveautofill_createdate = Convert.ToDateTime(sqlObject.SQLiteDataReader["objectiveautofill_createdate"]);
                    obj.objectiveautofill_modifydate = Convert.ToDateTime(sqlObject.SQLiteDataReader["objectiveautofill_modifydate"]);
                    obj.objectiveautofill_serverid = Convert.ToInt64(sqlObject.SQLiteDataReader["objectiveautofill_serverid"]);
                    obj.objectiveautofill_issynced = Convert.ToBoolean(sqlObject.SQLiteDataReader["objectiveautofill_issynced"]);
                    obj.objectiveautofill_syncerror = Convert.ToString(sqlObject.SQLiteDataReader["objectiveautofill_syncerror"]);
                    obj.objectiveautofill_isedited = Convert.ToBoolean(sqlObject.SQLiteDataReader["objectiveautofill_isedited"]);
                    data.Add(obj);
                }
                sqlObject.CloseConnections();
            }
            return data;
        }

        public static CBVAutofill GetAutofillByProjectId(int ProjectId, bool requirmentFlag, bool objectiveFlag, bool nextFlag, bool onlyActive)
        {
            var data = new CBVAutofill();
            if (nextFlag) data.next_autofill = GetNextAutofillById(ProjectId, onlyActive);
            if (requirmentFlag) data.require_autofill = GetRequireAutofillById(ProjectId, onlyActive);
            if (objectiveFlag) data.objective_autofill = GetObjectiveAutofillById(ProjectId, onlyActive);
            return data;
        }

        #endregion

        public static List<CBVVideoEvent> GetVideoEventbyId(int videoeventId, bool dependentDataFlag = false, bool designFlag = false)
        {
            var data = new List<CBVVideoEvent>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_videoevent where videoevent_id = {videoeventId}";
            
            if(designFlag)
                sqlQueryString = $@"SELECT 
                                        VE.*, D.fk_design_screen, D.fk_design_background 
                                    FROM 
                                        cbv_videoevent VE 
                                        Join cbv_design D on D.fk_design_videoevent = VE.videoevent_id
                                    where 
                                        VE.videoevent_id = {videoeventId}
                                    LIMIT 1";
            
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
                            fk_videoevent_projdet = sqlReader.GetInt32(1),
                            fk_videoevent_media = sqlReader.GetInt32(2),
                            videoevent_track = sqlReader.GetInt32(3),
                            videoevent_start = sqlReader.GetString(4),
                            videoevent_duration = sqlReader.GetString(5),
                            videoevent_end = sqlReader.GetString(6),
                            videoevent_createdate = sqlReader.GetDateTime(7),
                            videoevent_modifydate = sqlReader.GetDateTime(8),
                            videoevent_isdeleted = sqlReader.GetBoolean(9),
                            videoevent_issynced = Convert.ToBoolean(sqlReader["videoevent_issynced"]),
                            videoevent_serverid = Convert.ToInt64(sqlReader["videoevent_serverid"]),
                            videoevent_syncerror = Convert.ToString(sqlReader["videoevent_syncerror"])
                        };

                        if(designFlag)
                        {
                            obj.fk_design_background = Convert.ToInt32(sqlReader["fk_design_background"]);
                            obj.fk_design_screen = Convert.ToInt32(sqlReader["fk_design_screen"]);
                        }

                            
                        if (dependentDataFlag)
                        {
                            var videoEventId = sqlReader.GetInt32(0);

                            //var audioData = GetAudio(videoEventId);
                            //obj.audio_data = audioData;

                            var videoSegmentData = GetVideoSegment(videoEventId);
                            obj.videosegment_data = videoSegmentData;

                            var designData = GetDesign(videoEventId);
                            obj.design_data = designData;

                            var notesData = GetNotes(videoEventId);
                            obj.notes_data = notesData;
                        }
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }

            return data;
        }

        public static List<CBVShiftVideoEvent> GetShiftVideoEventsbyTime(int fk_videoevent_projdet, string endTime)
        {
            var data = new List<CBVShiftVideoEvent>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@" Select 
	                                        videoevent_id,
	                                        videoevent_start, 
	                                        videoevent_duration,
	                                        videoevent_end, 
	                                        videoevent_serverid
                                        FROM
	                                        cbv_videoevent
                                        where 
	                                        videoevent_start >= '{endTime}'
	                                        And fk_videoevent_projdet = {fk_videoevent_projdet}
	                                        And videoevent_track = 2  
	                                        And videoevent_isdeleted = 0
                                        Order By
	                                        videoevent_start,
	                                        videoevent_end";


            var sqlObject = sqlQueryString.RunSelectQuery();
            if (sqlObject != null)
            {
                while (sqlObject.SQLiteDataReader.Read())
                {
                    var obj = new CBVShiftVideoEvent();
                    obj.videoevent_id = Convert.ToInt32(sqlObject.SQLiteDataReader["videoevent_id"]);
                    obj.videoevent_start = Convert.ToString(sqlObject.SQLiteDataReader["videoevent_start"]);
                    obj.videoevent_duration = Convert.ToString(sqlObject.SQLiteDataReader["videoevent_duration"]);
                    obj.videoevent_end = Convert.ToString(sqlObject.SQLiteDataReader["videoevent_end"]);
                    obj.videoevent_serverid = Convert.ToInt64(sqlObject.SQLiteDataReader["videoevent_serverid"]);
                    data.Add(obj);
                }
                sqlObject.CloseConnections();
            }
            return data;

            
        }

        public static List<CBVVideoEvent> GetNotSyncedVideoEvents(int projdetId, bool dependentDataFlag = true)
        {
            var data = new List<CBVVideoEvent>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_videoevent where fk_videoevent_projdet = {projdetId} and videoevent_issynced = 0 and videoevent_serverid > {DateTime.UtcNow.ToString("yyyyMMdd")}";
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
                            fk_videoevent_projdet = sqlReader.GetInt32(1),
                            fk_videoevent_media = sqlReader.GetInt32(2),
                            videoevent_track = sqlReader.GetInt32(3),
                            videoevent_start = sqlReader.GetString(4),
                            videoevent_duration = sqlReader.GetString(5),
                            videoevent_end = sqlReader.GetString(6),
                            videoevent_createdate = sqlReader.GetDateTime(7),
                            videoevent_modifydate = sqlReader.GetDateTime(8),
                            videoevent_isdeleted = sqlReader.GetBoolean(9),
                            videoevent_issynced = Convert.ToBoolean(sqlReader["videoevent_issynced"]),
                            videoevent_serverid = Convert.ToInt64(sqlReader["videoevent_serverid"]),
                            videoevent_syncerror = Convert.ToString(sqlReader["videoevent_syncerror"])
                        };
                        if (dependentDataFlag)
                        {
                            var videoEventId = sqlReader.GetInt32(0);

                            //var audioData = GetAudio(videoEventId);
                            //obj.audio_data = audioData;

                            var videoSegmentData = GetVideoSegment(videoEventId, false);
                            obj.videosegment_data = videoSegmentData;

                            var designData = GetDesign(videoEventId, false);
                            obj.design_data = designData;

                            var notesData = GetNotes(videoEventId, false);
                            obj.notes_data = notesData;
                        }
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }

            return data;
        }
        
        public static List<CBVVideoEvent> GetVideoEvents(int projdetId, bool dependentDataFlag = false, bool designFlag = false)
        {
            var data = new List<CBVVideoEvent>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT VE.* FROM cbv_videoevent VE";

            if(designFlag)
                sqlQueryString = $@"SELECT 
                                        VE.*, D.fk_design_screen, D.fk_design_background 
                                    FROM 
                                        cbv_videoevent VE 
                                        Left Join cbv_design D on D.fk_design_videoevent = VE.videoevent_id
                                    ";

            sqlQueryString += projdetId <= 0 ? " where VE.videoevent_isdeleted = 0 " : $@" where VE.fk_videoevent_projdet = {projdetId} and VE.videoevent_isdeleted = 0";
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
                            fk_videoevent_projdet = sqlReader.GetInt32(1),
                            fk_videoevent_media = sqlReader.GetInt32(2),
                            videoevent_track = sqlReader.GetInt32(3),
                            videoevent_start = sqlReader.GetString(4),
                            videoevent_duration = sqlReader.GetString(5),
                            videoevent_end = sqlReader.GetString(6),
                            videoevent_createdate = sqlReader.GetDateTime(7),
                            videoevent_modifydate = sqlReader.GetDateTime(8),
                            videoevent_isdeleted = sqlReader.GetBoolean(9),
                            videoevent_issynced = Convert.ToBoolean(sqlReader["videoevent_issynced"]),
                            videoevent_serverid = Convert.ToInt64(sqlReader["videoevent_serverid"]),
                            videoevent_syncerror = Convert.ToString(sqlReader["videoevent_syncerror"])
                        };
                        if (designFlag)
                        {
                            obj.fk_design_background = Convert.ToInt32(Convert.ToString(sqlReader["fk_design_background"]) == "" ? -1 : sqlReader["fk_design_background"]);
                            obj.fk_design_screen = Convert.ToInt32(Convert.ToString(sqlReader["fk_design_screen"]) == "" ? -1 : sqlReader["fk_design_screen"]);
                        }
                        if (dependentDataFlag)
                        {
                            var videoEventId = sqlReader.GetInt32(0);

                            //var audioData = GetAudio(videoEventId);
                            //obj.audio_data = audioData;

                            var videoSegmentData = GetVideoSegment(videoEventId);
                            obj.videosegment_data = videoSegmentData;

                            var designData = GetDesign(videoEventId);
                            obj.design_data = designData;

                            var notesData = GetNotes(videoEventId);
                            obj.notes_data = notesData;
                        }
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }
            
            return data.GroupBy(x => x.videoevent_id).Select(p=>p.First()).Distinct().ToList();
        }

        /*
        public static List<CBVAudio> GetAudio(int videoEventId)
        {
            var data = new List<CBVAudio>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_audio where audio_isdeleted = 0";
            if (videoEventId > -1)
                sqlQueryString += $@" and fk_audio_videoevent = {videoEventId}";
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
                            audio_isdeleted = Convert.ToBoolean(sqlReader["audio_isdeleted"]),
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
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }
            return data;
        }
        */

        public static List<CBVVideoSegment> GetVideoSegment(int videoEventId, bool issyncedFlag = true)
        {
            var data = new List<CBVVideoSegment>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_videosegment ";

            if (issyncedFlag == false)
                sqlQueryString += $@" where videosegment_issynced = 0 and videosegment_serverid > {DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";
            else
                sqlQueryString += $@" where videosegment_isdeleted = 0";

            if (videoEventId > -1)
                sqlQueryString += $@" and videosegment_id = {videoEventId};";
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
                            videosegment_createdate = Convert.ToDateTime(sqlReader["videosegment_createdate"]),
                            videosegment_modifydate = Convert.ToDateTime(sqlReader["videosegment_modifydate"]),
                            videosegment_isdeleted = Convert.ToBoolean(sqlReader["videosegment_isdeleted"]),
                            videosegment_issynced = Convert.ToBoolean(sqlReader["videosegment_issynced"]),
                            videosegment_serverid = Convert.ToInt64(sqlReader["videosegment_serverid"]),
                            videosegment_syncerror = Convert.ToString(sqlReader["videosegment_syncerror"])
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
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }

            return data;
        }

        public static List<CBVDesign> GetDesign(int videoEventId, bool issyncedFlag = true)
        {
            var data = new List<CBVDesign>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");


            string sqlQueryString = $@"SELECT * FROM cbv_design ";

            if (issyncedFlag == false)
                sqlQueryString += $@" where design_issynced = 0 and design_serverid > {DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";
            else
                sqlQueryString += $@" where design_isdeleted = 0";

            if (videoEventId > -1)
                sqlQueryString += $@" and fk_design_videoevent = {videoEventId} ";
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
                            fk_design_background = Convert.ToInt32(sqlReader["fk_design_background"]),
                            fk_design_videoevent = Convert.ToInt32(sqlReader["fk_design_videoevent"]),
                            design_xml = Convert.ToString(sqlReader["design_xml"]),
                            design_createdate = Convert.ToDateTime(sqlReader["design_createdate"]),
                            design_modifydate = Convert.ToDateTime(sqlReader["design_modifydate"]),
                            design_isdeleted = Convert.ToBoolean(sqlReader["design_isdeleted"]),
                            design_issynced = Convert.ToBoolean(sqlReader["design_issynced"]),
                            design_serverid = Convert.ToInt64(sqlReader["design_serverid"]),
                            design_syncerror = Convert.ToString(sqlReader["design_syncerror"])
                        };
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }

            return data;
        }

        public static List<CBVNotes> GetNotes(int videoEventId, bool issyncedFlag = true)
        {
            var data = new List<CBVNotes>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_notes ";

            if (issyncedFlag == false)
                sqlQueryString += $@" where notes_issynced = 0 and notes_serverid > {DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";
            else
                sqlQueryString += $@" where notes_isdeleted = 0";

            sqlQueryString += $@" and fk_notes_videoevent = {videoEventId} order by notes_index";
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
                            notes_line = Convert.ToString(sqlReader["notes_line"]).Trim('\''),
                            notes_wordcount = Convert.ToInt32(sqlReader["notes_wordcount"]),
                            notes_index = Convert.ToInt32(sqlReader["notes_index"]),
                            notes_start = Convert.ToString(sqlReader["notes_start"]),
                            notes_duration = Convert.ToString(sqlReader["notes_duration"]),
                            notes_createdate = Convert.ToDateTime(sqlReader["notes_createdate"]),
                            notes_modifydate = Convert.ToDateTime(sqlReader["notes_modifydate"]),
                            notes_isdeleted = Convert.ToBoolean(sqlReader["notes_isdeleted"]),
                            notes_issynced = Convert.ToBoolean(sqlReader["notes_issynced"]),
                            notes_serverid = Convert.ToInt64(sqlReader["notes_serverid"]),
                            notes_syncerror = Convert.ToString(sqlReader["notes_syncerror"])
                        };
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }

            return data;
        }

        public static List<CBVNotes> GetNotesbyId(int notesId)
        {
            var data = new List<CBVNotes>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_notes where notes_id = {notesId} and notes_isdeleted = 0 order by notes_index";

            sqlQueryString += $@"";
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
                            notes_line = Convert.ToString(sqlReader["notes_line"]).Trim('\''),
                            notes_wordcount = Convert.ToInt32(sqlReader["notes_wordcount"]),
                            notes_index = Convert.ToInt32(sqlReader["notes_index"]),
                            notes_start = Convert.ToString(sqlReader["notes_start"]),
                            notes_duration = Convert.ToString(sqlReader["notes_duration"]),
                            notes_createdate = Convert.ToDateTime(sqlReader["notes_createdate"]),
                            notes_modifydate = Convert.ToDateTime(sqlReader["notes_modifydate"]),
                            notes_isdeleted = Convert.ToBoolean(sqlReader["notes_isdeleted"]),
                            notes_issynced = Convert.ToBoolean(sqlReader["notes_issynced"]),
                            notes_serverid = Convert.ToInt64(sqlReader["notes_serverid"]),
                            notes_syncerror = Convert.ToString(sqlReader["notes_syncerror"])
                        };
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }

            return data;
        }

        public static List<CBVLocAudio> GetLocAudio(int notesId)
        {
            var data = new List<CBVLocAudio>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_locaudio where locaudio_isdeleted = 0";
            if (notesId > -1)
                sqlQueryString += $@" and fk_locaudio_notes = {notesId}";
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
                        var obj = new CBVLocAudio
                        {
                            locaudio_id = Convert.ToInt32(sqlReader["locaudio_id"]),
                            fk_locaudio_notes = Convert.ToInt32(sqlReader["fk_locaudio_notes"]),
                            locaudio_createdate = Convert.ToDateTime(sqlReader["locaudio_createdate"]),
                            locaudio_modifydate = Convert.ToDateTime(sqlReader["locaudio_modifydate"]),
                            locaudio_isdeleted = Convert.ToBoolean(sqlReader["locaudio_isdeleted"]),
                        };
                        var byteData = GetBlobMedia(sqlReader, "locaudio_media");
                        obj.locaudio_media = byteData;
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }
            return data;
        }

        public static List<CBVFinalMp4> GetFinalMp4(int projectId = -1, bool dependentDataFlag = false)
        {
            var data = new List<CBVFinalMp4>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_finalmp4 where finalm4_isdeleted = 0";
            if (projectId > -1)
                sqlQueryString += $@" and fk_finalmp4_project = {projectId} ";
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
                        var obj = new CBVFinalMp4
                        {
                            finalmp4_id = Convert.ToInt32(sqlReader["finalmp4_id"]),
                            fk_finalmp4_project = Convert.ToInt32(sqlReader["fk_finalmp4_project"]),
                            finalmp4_version = Convert.ToInt32(sqlReader["finalmp4_version"]),
                            finalmp4_comments = Convert.ToString(sqlReader["finalmp4_comments"]),
                            finalmp4_createdate = Convert.ToDateTime(sqlReader["finalmp4_createdate"]),
                            finalmp4_modifydate = Convert.ToDateTime(sqlReader["finalmp4_modifydate"]),
                            finalmp4_isdeleted = Convert.ToBoolean(sqlReader["finalmp4_isdeleted"]),
                            finalmp4_media = GetBlobMedia(sqlReader, "finalmp4_media"),
                            finalmp4_issynced = Convert.ToBoolean(sqlReader["finalmp4_issynced"]),
                            finalmp4_serverid = Convert.ToInt64(sqlReader["finalmp4_serverid"]),
                            finalmp4_syncerror = Convert.ToString(sqlReader["finalmp4_syncerror"])
                        };

                        if (dependentDataFlag)
                        {
                            //var hlstsId = sqlReader.GetInt32(0);
                            //var streamtsData = GetStreamts(hlstsId);
                            //obj.streamts_data = streamtsData;
                        }

                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }

            return data;
        }

        public static List<CBVVoiceTimer> GetVoiceTimers()
        {
            var data = new List<CBVVoiceTimer>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_voicetimer ";
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
                        var obj = new CBVVoiceTimer
                        {
                            voicetimer_id = Convert.ToInt32(sqlReader["voicetimer_id"]),
                            voicetimer_line = Convert.ToString(sqlReader["voicetimer_line"]),
                            voicetimer_wordcount = Convert.ToInt32(sqlReader["voicetimer_wordcount"]),
                            voicetimer_index = Convert.ToInt32(sqlReader["voicetimer_index"])
                        };
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }

            return data;
        }

        public static List<CBVVoiceAvergae> GetVoiceAverage()
        {
            var data = new List<CBVVoiceAvergae>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_voiceaverage ";
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
                        var obj = new CBVVoiceAvergae
                        {
                            voiceaverage_id = Convert.ToInt32(sqlReader["voiceaverage_id"]),
                            voiceaverage_average = Convert.ToString(sqlReader["voiceaverage_average"])
                        };
                        data.Add(obj);
                    }
                }
                // Close database
                sqlQuery.Dispose();
                sqlCon.Close();
            }
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
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
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
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
        private static bool ExecuteNonQueryInTable(string UpdateQuery)
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
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }
        }

        private static bool ExecuteNonQueryWithBlobInTable(string UpdateQuery, byte[] blob)
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
                    command.Dispose();
                }
                sqlCon?.Close();
                return true;
            }
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
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
            catch (Exception)
            {
                if (null != sqlCon)
                    sqlCon.Close();
                throw;
            }
        }

        #endregion


        public static void UpdateRowsToProject(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var modifyDate = Convert.ToString(dr["project_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var project_id = Convert.ToInt32(dr["project_id"]);

                var updateQueryString = $@" UPDATE cbv_project 
                                        SET 
                                            project_videotitle = '{Convert.ToString(dr["project_videotitle"])}',
                                            project_version = {Convert.ToInt32(dr["project_version"])},
                                            project_comments = '{Convert.ToString(dr["project_comments"])}',
                                            project_uploaded = {Convert.ToInt32(dr["project_uploaded"])},
                                            fk_project_background = {Convert.ToInt32(dr["fk_project_background"])},
                                            project_date = '{Convert.ToString(dr["project_date"])}',
                                            project_archived = {Convert.ToInt32(dr["project_archived"])},
                                            project_issynced = {Convert.ToBoolean(dr["project_issynced"])},
                                            project_serverid = {Convert.ToInt64(dr["project_serverid"])},
                                            project_syncerror = {Convert.ToString(dr["project_syncerror"])}
                                            project_modifydate = '{modifyDate}'
                                        WHERE 
                                            project_id = {project_id}";
                var updateFlag = ExecuteNonQueryInTable(updateQueryString);
                Console.WriteLine($@"cbv_project table update status for id - {project_id} result - {updateFlag}");
            }
        }

        public static void ShiftVideoEvents(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                
                var modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var videoevent_serverid = Convert.ToInt32(dr["videoevent_serverid"]);
                var updateQueryString = $@" UPDATE cbv_videoevent 
                                        SET 
                                            videoevent_start = '{Convert.ToString(dr["videoevent_start"])}',
                                            videoevent_duration = '{Convert.ToString(dr["videoevent_duration"])}',
                                            videoevent_end = '{Convert.ToString(dr["videoevent_end"])}',
                                            videoevent_modifydate = '{modifyDate}'
                                        WHERE 
                                            videoevent_serverid = {videoevent_serverid}";
                var updateFlag = ExecuteNonQueryInTable(updateQueryString);
                Console.WriteLine($@"cbv_videoevent table update status for serverid - {videoevent_serverid} result - {updateFlag}");
            }
        }

        public static void UpdateRowsToVideoEvent(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var modifyDate = Convert.ToString(dr["videoevent_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var videoevent_id = Convert.ToInt32(dr["videoevent_id"]);
                var updateQueryString = $@" UPDATE cbv_videoevent 
                                        SET 
                                            fk_videoevent_media = {Convert.ToInt32(dr["fk_videoevent_media"])},
                                            videoevent_track = {Convert.ToInt32(dr["videoevent_track"])},
                                            videoevent_start = '{Convert.ToString(dr["videoevent_start"])}',
                                            videoevent_duration = '{Convert.ToString(dr["videoevent_duration"])}',
                                            videoevent_end = '{Convert.ToString(dr["videoevent_end"])}',
                                            videoevent_isdeleted = {Convert.ToBoolean(dr["videoevent_isdeleted"])},
                                            videoevent_issynced = {Convert.ToBoolean(dr["videoevent_issynced"])},
                                            videoevent_syncerror = '{Convert.ToString(dr["videoevent_syncerror"])}',
                                            videoevent_modifydate = '{modifyDate}'
                                        WHERE 
                                            videoevent_id = {videoevent_id}";
                var updateFlag = ExecuteNonQueryInTable(updateQueryString);
                Console.WriteLine($@"cbv_videoevent table update status for id - {videoevent_id} result - {updateFlag}");
            }
        }

        public static void UpdateServerId(string table, int localId, Int64 serverId, string ErrorMessage)
        {
            var modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");


            var updateQueryString = $@" UPDATE cbv_{table} 
                                        SET 
                                            {table}_issynced = {string.IsNullOrEmpty(ErrorMessage)},
                                            {table}_serverid = {serverId},
                                            {table}_syncerror = '{ErrorMessage}',
                                            {table}_modifydate = '{modifyDate}'
                                        WHERE 
                                            {table}_id = {localId}";
            var updateFlag = ExecuteNonQueryInTable(updateQueryString);
            Console.WriteLine($@"cbv_{table} table update status for id - {localId} result - {updateFlag}");
        }

        public static void UpdateRowsToVideoSegment(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var modifyDate = Convert.ToString(dr["videosegment_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var videosegment_id = Convert.ToInt32(dr["videosegment_id"]);
                var updateQueryString = $@" UPDATE cbv_videosegment 
                                        SET 
                                            videosegment_media = @blob,
                                            videosegment_modifydate = '{modifyDate}'
                                        WHERE 
                                            videosegment_id = {videosegment_id}";
                var updateFlag = UpdateBlobRecordsInTable(updateQueryString, dr["videosegment_media"] as byte[]);
                Console.WriteLine($@"cbv_videosegment table update status for id - {videosegment_id} result - {updateFlag}");
            }
        }

        /*
        public static void UpdateRowsToAudio(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var modifyDate = Convert.ToString(dr["audio_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var audio_id = Convert.ToInt32(dr["audio_id"]);
                var updateQueryString = $@" UPDATE cbv_audio
                                        SET 
                                            audio_media = @blob,
                                            audio_modifydate = '{modifyDate}'
                                        WHERE 
                                            audio_id = {audio_id}";
                var updateFlag = UpdateBlobRecordsInTable(updateQueryString, dr["audio_media"] as byte[]);
                Console.WriteLine($@"cbv_audio table update status for id - {audio_id} result - {updateFlag}");
            }
        }
        */

        public static void UpdateRowsToDesign(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var modifyDate = Convert.ToString(dr["design_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var design_id = Convert.ToInt32(dr["design_id"]);
                var updateQueryString = $@" UPDATE cbv_design
                                        SET 
                                            fk_design_videoevent = {Convert.ToInt32(dr["fk_design_videoevent"])},
                                            fk_design_background = {Convert.ToInt32(dr["fk_design_background"])},
                                            fk_design_screen = {Convert.ToInt32(dr["fk_design_screen"])},
                                            design_xml = '{Convert.ToString(dr["design_xml"])}',
                                            design_issynced = {Convert.ToBoolean(dr["design_issynced"])},
                                            design_serverid = {Convert.ToInt64(dr["design_serverid"])},
                                            design_syncerror = {Convert.ToString(dr["design_syncerror"])},
                                            design_modifydate = '{modifyDate}'
                                        WHERE 
                                            design_id = {design_id}";
                var updateFlag = ExecuteNonQueryInTable(updateQueryString);
                Console.WriteLine($@"cbv_design table update status for id - {design_id} result - {updateFlag}");
            }
        }

        public static void UpdateRowsToNotes(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var modifyDate = Convert.ToString(dr["notes_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var notes_id = Convert.ToInt32(dr["notes_id"]);
                var updateQueryString = $@" UPDATE cbv_notes
                                        SET 
                                            notes_line = '{Convert.ToString(dr["notes_line"]).Trim('\'')}',
                                            notes_wordcount = {Convert.ToInt32(dr["notes_wordcount"])},
                                            notes_index = {Convert.ToInt32(dr["notes_index"])},
                                            notes_start = '{Convert.ToString(dr["notes_start"])}',
                                            notes_duration = '{Convert.ToString(dr["notes_duration"])}',
                                            notes_issynced = {Convert.ToBoolean(dr["notes_issynced"])},
                                            notes_serverid = {Convert.ToInt64(dr["notes_serverid"])},
                                            notes_syncerror = '{Convert.ToString(dr["notes_syncerror"])}',
                                            notes_modifydate = '{modifyDate}'
                                        WHERE 
                                            notes_id = {notes_id}";
                var updateFlag = ExecuteNonQueryInTable(updateQueryString);
                Console.WriteLine($@"cbv_notes table update status for id - {notes_id} result - {updateFlag}");
            }
        }

        public static void UpdateRowsToNotes(DataRow dr)
        {
            var modifyDate = Convert.ToString(dr["notes_modifydate"]);
            if (string.IsNullOrEmpty(modifyDate))
                modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var notes_id = Convert.ToInt32(dr["notes_id"]);
            var updateQueryString = $@" UPDATE cbv_notes
                                        SET 
                                            notes_line = '{Convert.ToString(dr["notes_line"]).Trim('\'')}',
                                            notes_wordcount = {Convert.ToInt32(dr["notes_wordcount"])},
                                            notes_index = {Convert.ToInt32(dr["notes_index"])},
                                            notes_start = '{Convert.ToString(dr["notes_start"])}',
                                            notes_duration = '{Convert.ToString(dr["notes_duration"])}',
                                            notes_issynced = {Convert.ToBoolean(dr["notes_issynced"])},
                                            notes_serverid = {Convert.ToInt64(dr["notes_serverid"])},
                                            notes_syncerror = '{Convert.ToString(dr["notes_syncerror"])}',
                                            notes_modifydate = '{modifyDate}'
                                        WHERE 
                                            notes_id = {notes_id}";
            var updateFlag = ExecuteNonQueryInTable(updateQueryString);
            Console.WriteLine($@"cbv_notes table update status for id - {notes_id} result - {updateFlag}");
        }

        public static void UpdateRowsToLocAudio(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var modifyDate = Convert.ToString(dr["locaudio_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var locaudio_id = Convert.ToInt32(dr["locaudio_id"]);
                var updateQueryString = $@" UPDATE cbv_locaudio
                                        SET 
                                            locaudio_media = @blob,
                                            locaudio_modifydate = '{modifyDate}'
                                        WHERE 
                                            locaudio_id = {locaudio_id}";
                var updateFlag = UpdateBlobRecordsInTable(updateQueryString, dr["locaudio_media"] as byte[]);
                Console.WriteLine($@"cbv_locaudio table update status for id - {locaudio_id} result - {updateFlag}");
            }
        }


        public static void UpdateRowsToFinalMp4(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var modifyDate = Convert.ToString(dr["finalmp4_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var finalmp4_id = Convert.ToInt32(dr["finalmp4_id"]);
                var updateQueryString = $@" UPDATE cbv_finalmp4 
                                        SET 
                                            fk_finalmp4_project = {Convert.ToInt32(dr["fk_finalmp4_project"])},
                                            finalmp4_version = {Convert.ToInt32(dr["finalmp4_version"])},
                                            finalmp4_comments = '{Convert.ToString(dr["finalmp4_comments"])}',
                                            finalmp4_media = @blob,
                                            finalmp4_issynced = {Convert.ToBoolean(dr["finalmp4_issynced"])},
                                            finalmp4_serverid = {Convert.ToInt64(dr["finalmp4_serverid"])},
                                            finalmp4_syncerror = {Convert.ToString(dr["finalmp4_syncerror"])},
                                            finalmp4_modifydate = '{modifyDate}'
                                        WHERE 
                                            finalmp4_id = {finalmp4_id}";
                var updateFlag = UpdateBlobRecordsInTable(updateQueryString, dr["finalmp4_media"] as byte[]);
                Console.WriteLine($@"cbv_finalmp4 table update status for id - {finalmp4_id} result - {updateFlag}");
            }
        }

        public static void UpdateRowsToVoiceTimer(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var voicetimer_id = Convert.ToInt32(dr["voicetimer_id"]);
                var updateQueryString = $@" UPDATE cbv_voicetimer 
                                        SET 
                                            voicetimer_line = '{Convert.ToString(dr["voicetimer_line"])}',
                                            voicetimer_wordcount = {Convert.ToInt32(dr["voicetimer_wordcount"])},
                                            voicetimer_index = {Convert.ToInt32(dr["voicetimer_index"])}
                                        WHERE 
                                            voicetimer_id = {voicetimer_id}";
                var updateFlag = ExecuteNonQueryInTable(updateQueryString);
                Console.WriteLine($@"cbv_voicetimer table update status for id - {voicetimer_id} result - {updateFlag}");
            }
        }

        public static void UpdateRowsToVoiceAverage(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var voiceaverage_id = Convert.ToInt32(dr["voiceaverage_id"]);
                var updateQueryString = $@" UPDATE cbv_voiceaverage 
                                        SET 
                                            voiceaverage_average = '{Convert.ToString(dr["voiceaverage_average"])}'
                                        WHERE 
                                            voiceaverage_id = {voiceaverage_id}";
                var updateFlag = ExecuteNonQueryInTable(updateQueryString);
                Console.WriteLine($@"cbv_voiceaverage table update status for id - {voiceaverage_id} result - {updateFlag}");
            }
        }

        #endregion


        #region == Delete Methods ==


        public static void DeleteNotesById(int notesId = -1)
        {
            var deleteQueryString = $@" update cbv_notes
                                        Set 
                                            notes_isdeleted = 1 
                                        WHERE 
                                            notes_id = {notesId};

                                        Delete from cbv_locaudio
                                        WHERE 
                                            fk_locaudio_notes = {notesId};

                                        ";
            var deleteFlag = DeleteRecordsInTable(deleteQueryString);
            Console.WriteLine($@"cbv_notes table delete status for id - {notesId} result - {deleteFlag}");
        }

        public static void DeleteVideoEventsById(int videoeventId, bool cascadeDelete, bool isDelelted = true)
        {
            var deleteValue = isDelelted ? 1 : 0;
            var deleteQueryString = $@" 
                                        update cbv_videoevent 
                                        Set 
                                           videoevent_isdeleted = {deleteValue}
                                        WHERE 
                                            videoevent_id = {videoeventId};
                                        ";

            if (cascadeDelete == true)
                deleteQueryString = $@" update cbv_design
                                        Set 
                                           design_isdeleted = {deleteValue} 
                                        WHERE 
                                            fk_design_videoevent = {videoeventId};

                                        update cbv_videosegment
                                        Set 
                                           videosegment_isdeleted = {deleteValue}
                                        WHERE 
                                            videosegment_id = {videoeventId};

                                        update cbv_locaudio
                                        Set 
                                           locaudio_isdeleted = {deleteValue}
                                        WHERE 
                                        fk_locaudio_notes in (Select notes_id from cbv_notes WHERE fk_notes_videoevent = {videoeventId});

                                        update cbv_notes
                                        Set 
                                           notes_isdeleted = {deleteValue}
                                        WHERE 
                                            fk_notes_videoevent = {videoeventId};
                                        " + deleteQueryString;
            var deleteFlag = DeleteRecordsInTable(deleteQueryString);
            Console.WriteLine($@"cbv_videoevent table delete status for id - {videoeventId} with deleteFlag = {isDelelted} result - {deleteFlag}");
        }


        public static void DeleteAllVideoEventsByProjdetId(int projdetId, bool cascadeDelete)
        {
            var deleteQueryString = $@" 
                                        Delete from cbv_videoevent 
                                        WHERE 
                                            fk_videoevent_projdet = {projdetId};
                                        ";

            if (cascadeDelete == true)
                deleteQueryString = $@" Delete from cbv_design 
                                        WHERE 
                                            fk_design_videoevent in (Select videoevent_id from cbv_videoevent where fk_videoevent_projdet = {projdetId});

                                        Delete from cbv_videosegment 
                                        WHERE 
                                            videosegment_id in (Select videoevent_id from cbv_videoevent where fk_videoevent_projdet = {projdetId});
                                        
                                        Delete from cbv_locaudio 
                                        WHERE 
                                            fk_locaudio_notes in (
                                                                    Select 
                                                                        notes_id 
                                                                    from 
                                                                        cbv_notes 
                                                                    WHERE 
                                                                        fk_notes_videoevent In (Select videoevent_id from cbv_videoevent where fk_videoevent_projdet = {projdetId})
                                                                );
                                
            
                                        Delete from cbv_notes 
                                        WHERE 
                                            fk_notes_videoevent in (Select videoevent_id from cbv_videoevent where fk_videoevent_projdet = {projdetId});

                                        " + deleteQueryString;
            var deleteFlag = DeleteRecordsInTable(deleteQueryString);
            Console.WriteLine($@"DeleteAllVideoEventsByProjectId Success for projdetId - {projdetId} with cascadeflag - {cascadeDelete}");
        }



        #region == Delete Helper Methods ==
        private static bool DeleteRecordsInTable(string deleteQuery)
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
                var sqlQuery = new SQLiteCommand(deleteQuery, sqlCon);
                sqlQuery.ExecuteNonQuery();
                sqlQuery.Dispose();
                sqlCon?.Close();
                return true;
            }
            catch (Exception)
            {
                sqlCon?.Close();
                throw;
            }
        }
        #endregion

        #endregion


        #region == Upsert Methods ==
        public static void UpsertRowsToApp(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var app_id = Convert.ToInt32(dr["app_id"]);
                var app_name = Convert.ToString(dr["app_name"]);
                var app_active = Convert.ToInt32(dr["app_active"]);

                var upsertQueryString = $@" INSERT INTO cbv_app(app_name, app_active)
                                                    VALUES('{app_name}',{app_active})
                                                ON CONFLICT(app_name) 
                                                DO UPDATE 
                                                SET
                                                    app_active = excluded.app_active;";

                var upsertFlag = ExecuteNonQueryInTable(upsertQueryString);
                Console.WriteLine($@"cbv_app table upsert status for id - {app_id} result - {upsertFlag}");
            }
        }

        public static void UpsertRowsToMedia(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var media_id = Convert.ToInt32(dr["media_id"]);
                var media_name = Convert.ToString(dr["media_name"]);
                var media_color = Convert.ToString(dr["media_color"]);

                var upsertQueryString = $@" INSERT INTO cbv_media(media_name, media_color)
                                                    VALUES('{media_name}','{media_color}')
                                                ON CONFLICT(media_name) 
                                                DO UPDATE 
                                                SET
                                                    media_color = excluded.media_color;";

                var upsertFlag = ExecuteNonQueryInTable(upsertQueryString);
                Console.WriteLine($@"cbv_media table upsert status for id - {media_id} result - {upsertFlag}");
            }
        }

        public static void UpsertRowsToScreen(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var screen_id = Convert.ToInt32(dr["screen_id"]);
                var screen_name = Convert.ToString(dr["screen_name"]);
                var screen_color = Convert.ToString(dr["screen_color"]);
                var screen_hexcolor = Convert.ToString(dr["screen_hexcolor"]);

                var upsertQueryString = $@" INSERT INTO cbv_screen(screen_name, screen_color, screen_hexcolor)
                                                    VALUES('{screen_name}','{screen_color}', '{screen_hexcolor}')
                                                ON CONFLICT(screen_name) 
                                                DO UPDATE 
                                                SET
                                                    screen_color = excluded.screen_color,
                                                    screen_hexcolor = excluded.screen_hexcolor;";

                var upsertFlag = ExecuteNonQueryInTable(upsertQueryString);
                Console.WriteLine($@"cbv_screen table upsert status for id - {screen_id} result - {upsertFlag}");
            }
        }

        public static void UpsertRowsToCompany(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var company_id = Convert.ToInt32(dr["company_id"]);
                var company_name = Convert.ToString(dr["company_name"]);

                var upsertQueryString = $@" INSERT INTO cbv_company(company_name)
                                                    VALUES('{company_name}')
                                                ON CONFLICT(company_name) 
                                                DO UPDATE 
                                                SET
                                                    company_id = {company_id};";

                var upsertFlag = ExecuteNonQueryInTable(upsertQueryString);
                Console.WriteLine($@"cbv_company table upsert status for id - {company_id} result - {company_name}");
            }
        }

        public static void UpsertRowsToBackground(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var background_id = Convert.ToInt32(dr["background_id"]);
                var fk_background_company = Convert.ToInt32(dr["fk_background_company"]);
                var background_active = Convert.ToInt32(dr["background_active"]);
                var whereClause = $" NOT EXISTS(SELECT 1 FROM cbv_background WHERE fk_background_company = {fk_background_company} and background_active = {background_active} );";


                var upsertQueryString = $@" INSERT INTO cbv_background(fk_background_company, background_active, background_media)
                                                   Select {fk_background_company}, {background_active}, @blob
                                                WHERE {whereClause};";
                var upsertFlag = InsertBlobRecordsInTable("cbv_background", upsertQueryString, dr["background_media"] as byte[]);
                Console.WriteLine($@"cbv_background table upsert status for id - {background_id} result - {upsertFlag}");
            }
        }

        public static void UpsertRowsToPlanningHead(DataTable dataTable)
        {
            foreach (DataRow dr in dataTable.Rows)
            {
                var planninghead_id = Convert.ToInt32(dr["planninghead_id"]);
                var planninghead_name = Convert.ToString(dr["planninghead_name"]);
                var planninghead_sort = Convert.ToInt32(dr["planninghead_sort"]);
                var whereClause = $" NOT EXISTS(SELECT 1 FROM cbv_planninghead WHERE planninghead_name = '{planninghead_name}' and planninghead_sort = {planninghead_sort} );";


                var upsertQueryString = $@" INSERT INTO cbv_planninghead (planninghead_name, planninghead_sort)
                                                   Select '{planninghead_name}', {planninghead_sort}
                                                WHERE {whereClause};";
                var upsertFlag = InsertRecordsInTable("cbv_planninghead", upsertQueryString);
                
                Console.WriteLine($@"cbv_planninghead table upsert status for id - {planninghead_id}, name -{planninghead_id} & sort - {planninghead_sort} |  result - {upsertFlag}");
            }
        }


        public static int UpsertRowsToProject(DataTable dataTable)
        {
            var values = new List<string>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var projectUploaded = Convert.ToBoolean(dr["project_uploaded"]);

                var projectDate = Convert.ToString(dr["project_date"]);
                if (string.IsNullOrEmpty(projectDate))
                    projectDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var projectArchived = Convert.ToBoolean(dr["project_archived"]);

                var createDate = Convert.ToString(dr["project_createdate"]);
                if (string.IsNullOrEmpty(createDate))
                    createDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["project_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                var isdeleted = Convert.ToInt16(dr["project_isdeleted"]);
                var issynced = Convert.ToInt16(dr["project_issynced"]);
                var serverid = Convert.ToInt64(dr["project_serverid"]);

                var syncErrorString = Convert.ToString(dr["project_syncerror"]);
                var syncerror = syncErrorString?.Length > 50 ? syncErrorString.Substring(0, 50) : syncErrorString;

                var fkProjectBackground = Convert.ToBoolean(dr["fk_project_background"]);
                values.Add($"('{dr["project_videotitle"]}',  '{dr["project_currwfstep"]}', {projectUploaded}, '{projectDate}', {projectArchived}, {fkProjectBackground}, '{createDate}', '{modifyDate}', " +
                    $" {isdeleted}, {issynced}, {serverid}, '{syncerror}')");
            }
            var valuesString = string.Join(",", values.ToArray());
            string sqlQueryString =
                $@"INSERT INTO  cbv_project 
                    (project_videotitle, project_currwfstep, project_uploaded, project_date, project_archived, fk_project_background, 
                        project_createdate, project_modifydate, project_isdeleted, project_issynced, project_serverid, project_syncerror) 
                VALUES 
                    {valuesString}";

            var insertedId = InsertRecordsInTable("cbv_project", sqlQueryString);
            return insertedId;
        }

        public static int InsertRowsToObjectiveAutofill(DataTable dataTable, int Project_Id)
        {
            var values = new List<string>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var createDate = Convert.ToString(dr["objectiveautofill_createdate"]);
                if (string.IsNullOrEmpty(createDate))
                    createDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["objectiveautofill_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                values.Add($"({Project_Id}, '{dr["objectiveautofill_name"]}', {dr["objectiveautofill_active"]}, '{createDate}', '{modifyDate}' ," +
                    $" {dr["objectiveautofill_serverid"]},  {dr["objectiveautofill_issynced"]},  '{dr["objectiveautofill_syncerror"]}',  {dr["objectiveautofill_isedited"]})");
            }
            var valuesString = string.Join(",", values.ToArray());
            string sqlQueryString =
                $@"INSERT INTO cbv_objectiveautofill
                    (fk_objectiveautofill_project, objectiveautofill_name,  objectiveautofill_active, objectiveautofill_createdate, objectiveautofill_modifydate,
                        objectiveautofill_serverid, objectiveautofill_issynced, objectiveautofill_syncerror, objectiveautofill_isedited) 
                VALUES 
                    {valuesString}";

            var insertedId = InsertRecordsInTable("cbv_objectiveautofill", sqlQueryString);
            return insertedId;
        }

        public static int InsertRowsToRequireAutofill(DataTable dataTable, int Project_Id)
        {
            var values = new List<string>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var createDate = Convert.ToString(dr["requireautofill_createdate"]);
                if (string.IsNullOrEmpty(createDate))
                    createDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["requireautofill_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                values.Add($"({Project_Id}, '{dr["requireautofill_name"]}', {dr["requireautofill_importance"]}, {dr["requireautofill_active"]}, '{createDate}', '{modifyDate}' ," +
                    $" {dr["requireautofill_serverid"]},  {dr["requireautofill_issynced"]},  '{dr["requireautofill_syncerror"]}',  {dr["requireautofill_isedited"]})");
            }
            var valuesString = string.Join(",", values.ToArray());
            string sqlQueryString =
                $@"INSERT INTO cbv_requireautofill
                    (fk_requireautofill_project, requireautofill_name,  requireautofill_importance, requireautofill_active, requireautofill_createdate, requireautofill_modifydate,
                        requireautofill_serverid, requireautofill_issynced, requireautofill_syncerror, requireautofill_isedited) 
                VALUES 
                    {valuesString}";

            var insertedId = InsertRecordsInTable("cbv_requireautofill", sqlQueryString);
            return insertedId;
        }

        public static int InsertRowsToNextAutofill(DataTable dataTable, int Project_Id)
        {
            var values = new List<string>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var createDate = Convert.ToString(dr["nextautofill_createdate"]);
                if (string.IsNullOrEmpty(createDate))
                    createDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["nextautofill_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                values.Add($"({Project_Id}, '{dr["nextautofill_name"]}', {dr["nextautofill_importance"]}, {dr["nextautofill_active"]}, '{createDate}', '{modifyDate}' ," +
                    $" {dr["nextautofill_serverid"]},  {dr["nextautofill_issynced"]},  '{dr["nextautofill_syncerror"]}',  {dr["nextautofill_isedited"]})");
            }
            var valuesString = string.Join(",", values.ToArray());
            string sqlQueryString =
                $@"INSERT INTO cbv_nextautofill
                    (fk_nextautofill_project, nextautofill_name,  nextautofill_importance, nextautofill_active, nextautofill_createdate, nextautofill_modifydate,
                        nextautofill_serverid, nextautofill_issynced, nextautofill_syncerror, nextautofill_isedited) 
                VALUES 
                    {valuesString}";

            var insertedId = InsertRecordsInTable("cbv_nextautofill", sqlQueryString);
            return insertedId;
        }

        public static int InsertRowsToProjectDetail(DataTable dataTable, int Project_Id)
        {
            var values = new List<string>();
            foreach (DataRow dr in dataTable.Rows)
            {

                var createDate = Convert.ToString(dr["projdet_createdate"]);
                if (string.IsNullOrEmpty(createDate))
                    createDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["projdet_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                values.Add($"({Project_Id}, '{dr["projdet_version"]}', {dr["projdet_currver"]}, '{dr["projdet_comments"]}', '{dr["projdet_serverid"]}', '{createDate}', '{modifyDate}')");

                var valuesString = string.Join(",", values.ToArray());
                string sqlQueryString =
                    $@"INSERT INTO cbv_projdet
                    (fk_projdet_project, projdet_version,  projdet_currver, projdet_comments, projdet_serverid, projdet_createdate, projdet_modifydate) 
                VALUES 
                    {valuesString}";

                var insertedId = InsertRecordsInTable("cbv_projdet", sqlQueryString);
                return insertedId;
            }

            return -1;
        }


        public static List<int> InsertRowsToPlanning(DataTable dataTable)
        {
            var ids = new List<int>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var values = new List<string>();
                var createDate = Convert.ToString(dr["planning_createdate"]);
                if (string.IsNullOrEmpty(createDate))
                    createDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var modifyDate = Convert.ToString(dr["planning_modifydate"]);
                if (string.IsNullOrEmpty(modifyDate))
                    modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");


                var planning_medialibid = Convert.ToInt32(Convert.ToString(dr["planning_medialibid"]) == "" ? 0 : dr["planning_medialibid"]);
                var planning_sort = Convert.ToInt32(dr["planning_sort"]);

                var serverid = Convert.ToInt64(dr["planning_serverid"]);
                var issynced = Convert.ToInt16(dr["planning_issynced"]);
                var isEdited = Convert.ToInt16(dr["planning_isEdited"]);

                var syncErrorString = Convert.ToString(dr["planning_syncerror"]);
                var syncerror = syncErrorString?.Length > 50 ? syncErrorString.Substring(0, 50) : syncErrorString;

                values.Add($"({dr["fk_planning_project"]},  {dr["fk_planning_head"]}, '{dr["planning_customname"]}', '{dr["planning_notesline"]}', {planning_medialibid}, {planning_sort}, '{dr["planning_suggestnotesline"]}', @blob1, @blob2, " +
                    $"'{createDate}', '{modifyDate}', {serverid}, {issynced}, '{syncerror}', {isEdited})");

                var valuesString = string.Join(",", values.ToArray());
                string sqlQueryString =
                    $@"INSERT INTO  cbv_planning 
                    (fk_planning_project, fk_planning_head, planning_customname, planning_notesline, planning_medialibid, planning_sort, planning_suggestnotesline, planning_mediathumb, planning_mediafull, 
                        planning_createdate, planning_modifydate, planning_serverid, planning_issynced, planning_syncerror, planning_isEdited) 
                VALUES 
                    {valuesString}";

                var insertedId = Insert2BlobRecordsInTable("cbv_planning", sqlQueryString, dr["planning_mediathumb"] != DBNull.Value ? dr["planning_mediathumb"] as byte[] : null, dr["planning_mediafull"] != DBNull.Value ? dr["planning_mediafull"] as byte[] : null);
                ids.Add(insertedId);
            }
            return ids;
        }

        #endregion

    }
}
