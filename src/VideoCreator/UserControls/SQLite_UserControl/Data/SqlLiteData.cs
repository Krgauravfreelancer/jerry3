﻿using Sqllite_Library.Helpers;
using Sqllite_Library.Models;
using Sqllite_Library.Models.Planning;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;

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

            //Added so that DB tables are created all or none
            using (var transaction = sqlCon.BeginTransaction())
            {
                try
                {
                    CreateAllTables(sqlCon);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
                sqlCon?.Close();
            }


            //if (encryptFlag) sqlCon.SetPassword("mypassword");
            // else sqlCon.SetPassword(); // We need to decrypt only if the DB is encrypted.
            // Close connection

        }

        public static SQLiteConnection GetOpenedConnection()
        {
            // Open Database connection 
            var completeFileName = RegisteryHelper.GetFileName();
            if (completeFileName != null)
            {
                string connectionString = string.Format("Data Source={0};Version=3;", completeFileName);
                var sqlCon = new SQLiteConnection(connectionString);
                sqlCon.Open();
                return sqlCon;
            }
            return null;
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
        private static void ExecuteNonQueryForTransaction(string sqlQueryString, SQLiteConnection sqlCon)
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

            CreatePlanningTable(sqlCon);
            CreatePlanningMediaTable(sqlCon);
            CreatePlanningDescTable(sqlCon);
            CreatePlanningBulletTable(sqlCon);

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
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
        }

        private static void CreateBackgroundTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_background' (
                'background_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                'fk_background_company' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_company' ('company_id'),
                'background_media' BLOB NOT NULL  DEFAULT NULL,
                'background_active' INTEGER(1) NOT NULL  DEFAULT 1
                );";
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
        }

        private static void CreateVoiceTimerTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_voicetimer' (
                'voicetimer_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                'voicetimer_line' TEXT NOT NULL  DEFAULT 'NULL',
                'voicetimer_wordcount' INTEGER NOT NULL  DEFAULT 0,
                'voicetimer_index' INTEGER NOT NULL  DEFAULT 0  
                );";
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
        }

        private static void CreateVoiceAverageTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_voiceaverage' (
                'voiceaverage_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                'voiceaverage_average' TEXT NOT NULL  DEFAULT '1'
                );";
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
        }

        private static void CreateAppTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_app' (
                'app_id' INTEGER NOT NULL  DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'app_name' TEXT(5) NOT NULL DEFAULT 'Write',
                'app_active' INTEGER(1) NOT NULL  DEFAULT 0,
                UNIQUE (app_name)
                );";
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
        }

        private static void CreateMediaTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_media' (
                'media_id' INTEGER NOT NULL DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                'media_name' TEXT(20) NOT NULL  DEFAULT 'NULL', 
                'media_color' TEXT(15) NOT NULL  DEFAULT 'NULL', 
                UNIQUE (media_name)
                );";
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
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
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
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
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
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
                    'projdet_submitted' INTEGER(1) NOT NULL  DEFAULT 0,
                    'projdet_createdate' TEXT NOT NULL  DEFAULT 'NULL',
                    'projdet_modifydate' TEXT NOT NULL  DEFAULT 'NULL'
                    );";
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
        }

        private static void CreatePlanningTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE 'cbv_planning' (
                            'planning_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                            'fk_planning_project' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_project' ('project_id'),
                            'fk_planning_screen' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_screen' ('screen_id'),
                            'planning_customname' TEXT(50) DEFAULT NULL,
                            'planning_notesline' TEXT DEFAULT NULL,
                            'planning_medialibid' TEXT(12) DEFAULT NULL,
                            'planning_sort' TEXT NOT NULL  DEFAULT '1',
                            'planning_suggestnotesline' TEXT DEFAULT NULL,
                            'planning_createdate' TEXT NOT NULL  DEFAULT NULL,
                            'planning_modifydate' TEXT NOT NULL  DEFAULT 'NULL',
                            'planning_serverid' INTEGER NOT NULL  DEFAULT 1,
                            'planning_issynced' INTEGER(1) NOT NULL  DEFAULT 0,
                            'planning_syncerror' TEXT(50) NOT NULL  DEFAULT NULL,
                            'planning_isedited' INTEGER(1) NOT NULL  DEFAULT 0,
                            'planning_audioduration' TEXT(20) DEFAULT NULL
                            ,UNIQUE (fk_planning_project, fk_planning_screen, planning_customname)
                        );";
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
        }

        private static void CreatePlanningMediaTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE 'cbv_planningmedia' (
                            'planningmedia_id' TEXT NOT NULL  DEFAULT 'NULL' PRIMARY KEY REFERENCES 'cbv_planning' ('planning_id'),
                            'planningmedia_mediathumb' NONE NOT NULL  DEFAULT NULL,
                            'planningmedia_mediafull' NONE NOT NULL  DEFAULT NULL
                        );";
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
        }

        private static void CreatePlanningDescTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE 'cbv_planningdesc' (
                            'planningdesc_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY REFERENCES 'cbv_planning' ('planning_id'),
                            'planningdesc_line' TEXT(100) NOT NULL  DEFAULT NULL
                        );";
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
        }

        private static void CreatePlanningBulletTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE 'cbv_planningbullet' (
                            'planningbullet_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                            'fk_planningbullet_desc' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_planningdesc' ('planningdesc_id'),
                            'planningbullet_line' TEXT(50) NOT NULL  DEFAULT NULL
                        );";
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
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
                    'review_isedited' INTEGER(1) NOT NULL  DEFAULT 0
                    );";
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
        }

        private static void CreateReviewImageTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE 'cbv_reviewimg' (
                    'reviewimg_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                    'fk_reviewimg_review' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_review' ('review_id'),
                    'reviewimg_media' NONE NOT NULL  DEFAULT NULL,
                    'reviewimg_createdate' TEXT NOT NULL  DEFAULT 'NULL'
                    );";
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
        }

        private static void CreateReviewFtfTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE 'cbv_reviewftf' (
                    'reviewftf_id' INTEGER NOT NULL  DEFAULT 1 PRIMARY KEY AUTOINCREMENT,
                    'fk_reviewftf_review' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_review' ('review_id'),
                    'reviewrtf_rtfnote' TEXT NOT NULL  DEFAULT 'NULL',
                    'reviewrtf_createdate' TEXT NOT NULL  DEFAULT 'NULL'
                    );";
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
        }

        private static void CreateVideoEventTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_videoevent'(
                'videoevent_id' INTEGER NOT NULL DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'fk_videoevent_projdet' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_projdet' ('projdet_id'),
                'fk_videoevent_media' INTEGER NOT NULL DEFAULT 1 REFERENCES 'cbv_media'('media_id'),
                'videoevent_track' INTEGER NOT NULL  DEFAULT 1,
                'videoevent_start' TEXT(12) NOT NULL DEFAULT '00:00:00.000',
                'videoevent_end' TEXT(12) NOT NULL DEFAULT '00:00:00.000',
                'videoevent_duration' TEXT(12) NOT NULL DEFAULT 'NULL',
                'videoevent_origduration' TEXT(12) DEFAULT NULL,
                'videoevent_planning' INTEGER DEFAULT 0,
                'videoevent_createdate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'videoevent_modifydate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'videoevent_isdeleted' INTEGER(1) NOT NULL  DEFAULT 0,
                'videoevent_issynced' INTEGER(1) NOT NULL  DEFAULT 0,
                'videoevent_serverid' INTEGER NOT NULL  DEFAULT 1,
                'videoevent_syncerror' TEXT(50) DEFAULT NULL
                );";
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
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
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
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
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
        }
        */

        private static void CreateNotesTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_notes' (
                'notes_id' INTEGER NOT NULL DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'fk_notes_videoevent' INTEGER NOT NULL  DEFAULT NULL REFERENCES 'cbv_videoevent' ('videoevent_id'),
                'notes_line' TEXT NOT NULL  DEFAULT 'NULL',
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
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
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
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
        }

        private static void CreateLivAudioTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_livaudio' (
                'livaudio_id' INTEGER NOT NULL DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'fk_livaudio_videoevent' INTEGER NOT NULL DEFAULT NULL REFERENCES 'cbv_videoevent' ('videoevent_id')
                );";
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
        }

        private static void CreateDesignTable(SQLiteConnection sqlCon)
        {
            string sqlQueryString = @"CREATE TABLE IF NOT EXISTS 'cbv_design' (
                'design_id' INTEGER NOT NULL DEFAULT NULL PRIMARY KEY AUTOINCREMENT,
                'fk_design_videoevent' INTEGER NOT NULL  DEFAULT NULL REFERENCES 'cbv_videoevent' ('videoevent_id'),
                'fk_design_background' INTEGER NOT NULL  DEFAULT 1 REFERENCES 'cbv_background' ('background_id'),
                'fk_design_screen' INTEGER NOT NULL DEFAULT 1 REFERENCES 'cbv_screen'('screen_id'),
                'design_xml' TEXT NOT NULL  DEFAULT 'NULL',
                'design_createdate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'design_modifydate' TEXT(25) NOT NULL DEFAULT '1999-01-01 00:00:00',
                'design_isdeleted' INTEGER(1) NOT NULL  DEFAULT 0,
                'design_issynced' INTEGER(1) NOT NULL  DEFAULT 0,
                'design_serverid' INTEGER NOT NULL  DEFAULT 1,
                'design_syncerror' TEXT(50) DEFAULT NULL
                );";
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
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
            ExecuteNonQueryForTransaction(sqlQueryString, sqlCon);
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
            var sqlCon = GetOpenedConnection();
            using (var transaction = sqlCon.BeginTransaction())
            {
                try
                {
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

                        insertedId = InsertBlobRecordsInTable("cbv_background", sqlQueryString, dr["background_media"] as byte[], sqlCon);
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
                sqlCon?.Close();
                return insertedId;
            }



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

        public static string GetNextStartForTransaction(int fk_videoevent_media, int projdetId, SQLiteConnection sqlCon)
        {
            string result;
            string sqlQueryString = $@"Select max(videoevent_end) from cbv_videoevent ";
            var whereClause = string.Empty;

            if (fk_videoevent_media <= 4) whereClause = " where (fk_videoevent_media = 1 or fk_videoevent_media = 2 or fk_videoevent_media = 4) ";
            else whereClause = $" where fk_videoevent_media = {fk_videoevent_media}";


            whereClause += $" and fk_videoevent_projdet = {projdetId} and videoevent_isdeleted = 0 and videoevent_track = 2";

            var sqlQuery = new SQLiteCommand(sqlQueryString + whereClause, sqlCon);
            result = Convert.ToString(sqlQuery.ExecuteScalar());
            sqlQuery.Dispose();
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

        public static string GetTimespanFromSeconds(int seconds)
        {
            int s = seconds % 60;
            seconds /= 60;
            int mins = seconds % 60;
            int hours = seconds / 60;
            return string.Format("{0:D2}:{1:D2}:{2:D2}.000", hours, mins, s);
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
            if (endTime < 0)
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

            int planningId = 0;
            if (dr.Table.Columns.Contains("videoevent_planning") && dr["videoevent_planning"] != DBNull.Value)
                planningId = Convert.ToInt32(dr["videoevent_planning"]);

            var start = Convert.ToString(dr["videoevent_start"]);
            //if (string.IsNullOrEmpty(start) || start == "00:00:00.000" || start == "00:00:00")
            //    start = GetNextStart(Convert.ToInt32(dr["fk_videoevent_media"]), Convert.ToInt32(dr["fk_videoevent_projdet"]));

            var end = ShiftRight(start, Convert.ToString(dr["videoevent_duration"]));

            var issynced = Convert.ToInt16(dr["videoevent_issynced"]);
            var serverid = Convert.ToInt64(dr["videoevent_serverid"]);

            var syncErrorString = Convert.ToString(dr["videoevent_syncerror"]);
            var syncerror = syncErrorString?.Length > 50 ? syncErrorString.Substring(0, 50) : syncErrorString;

            values.Add($"({dr["fk_videoevent_projdet"]}, {dr["fk_videoevent_media"]}, {dr["videoevent_track"]}, " +
                $"'{start}', '{dr["videoevent_duration"]}', '{dr["videoevent_origduration"]}', '{end}', {planningId}, " +
                $"'{createDate}', '{modifyDate}', {dr["videoevent_isdeleted"]}, {issynced}, {serverid}, '{syncerror}')");

            var valuesString = string.Join(",", values.ToArray());
            string sqlQueryString =
                $@"INSERT INTO cbv_videoevent 
                    (fk_videoevent_projdet, fk_videoevent_media, videoevent_track, 
                        videoevent_start, videoevent_duration, videoevent_origduration, videoevent_end, videoevent_planning, 
                        videoevent_createdate, videoevent_modifydate, videoevent_isdeleted, videoevent_issynced, videoevent_serverid, videoevent_syncerror) 
                VALUES 
                    {valuesString}";

            var insertedId = InsertRecordsInTable("cbv_videoevent", sqlQueryString);
            return insertedId;
        }

        public static int InsertRowsToVideoEventForTransaction(DataRow dr, SQLiteConnection sqlCon)
        {
            var values = new List<string>();

            var createDate = Convert.ToString(dr["videoevent_createdate"]);
            if (string.IsNullOrEmpty(createDate))
                createDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var modifyDate = Convert.ToString(dr["videoevent_modifydate"]);
            if (string.IsNullOrEmpty(modifyDate))
                modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            int planningId = 0;
            if (dr.Table.Columns.Contains("videoevent_planning") && dr["videoevent_planning"] != DBNull.Value)
                planningId = Convert.ToInt32(dr["videoevent_planning"]);

            var start = Convert.ToString(dr["videoevent_start"]);
            //if (string.IsNullOrEmpty(start) || start == "00:00:00.000" || start == "00:00:00")
            //    start = GetNextStart(Convert.ToInt32(dr["fk_videoevent_media"]), Convert.ToInt32(dr["fk_videoevent_projdet"]));

            var end = ShiftRight(start, Convert.ToString(dr["videoevent_duration"]));

            var issynced = Convert.ToInt16(dr["videoevent_issynced"]);
            var serverid = Convert.ToInt64(dr["videoevent_serverid"]);

            var syncErrorString = Convert.ToString(dr["videoevent_syncerror"]);
            var syncerror = syncErrorString?.Length > 50 ? syncErrorString.Substring(0, 50) : syncErrorString;

            values.Add($"({dr["fk_videoevent_projdet"]}, {dr["fk_videoevent_media"]}, {dr["videoevent_track"]}, " +
                $"'{start}', '{dr["videoevent_duration"]}', '{dr["videoevent_origduration"]}', '{end}', {planningId}, " +
                $"'{createDate}', '{modifyDate}', {dr["videoevent_isdeleted"]}, {issynced}, {serverid}, '{syncerror}')");

            var valuesString = string.Join(",", values.ToArray());
            string sqlQueryString =
                $@"INSERT INTO cbv_videoevent 
                    (fk_videoevent_projdet, fk_videoevent_media, videoevent_track, 
                        videoevent_start, videoevent_duration, videoevent_origduration, videoevent_end, videoevent_planning, 
                        videoevent_createdate, videoevent_modifydate, videoevent_isdeleted, videoevent_issynced, videoevent_serverid, videoevent_syncerror) 
                VALUES 
                    {valuesString}";

            var insertedId = InsertRecordsInTableForTransaction("cbv_videoevent", sqlQueryString, sqlCon);
            return insertedId;
        }

        public static int InsertRowsToVideoSegment(DataTable dataTable)
        {
            var values = new List<string>();
            var sqlCon = GetOpenedConnection();
            using (var transaction = sqlCon.BeginTransaction())
            {
                try
                {
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

                    var insertedId = InsertBlobRecordsInTable("cbv_videosegment", sqlQueryString, dataTable.Rows[0]["videosegment_media"] as byte[], sqlCon); // This does not have seq
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
                sqlCon?.Close();
                return 1;
            }
        }

        public static int InsertRowsToVideoSegmentForTransaction(DataTable dataTable, SQLiteConnection sqlCon)
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

            var insertedId = InsertBlobRecordsInTable("cbv_videosegment", sqlQueryString, dataTable.Rows[0]["videosegment_media"] as byte[], sqlCon); // This does not have seq
            return insertedId;
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


        public static int InsertRowsToDesignForTransaction(DataTable dataTable, SQLiteConnection sqlCon)
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

            var insertedId = InsertRecordsInTableForTransaction("cbv_design", sqlQueryString, sqlCon);
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


        public static int InsertRowsToNotesForTransaction(DataTable dataTable, SQLiteConnection sqlCon)
        {
            var insertedId = 0;
            foreach (DataRow dr in dataTable.Rows)
            {
                //(Select Max(notes_index) + 1 from cbv_notes where fk_notes_videoevent = 1) as index
                var max_index = GetMaxIndexForNotesForTransaction(Convert.ToInt32(dr["fk_notes_videoevent"]), sqlCon);
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

                insertedId = InsertRecordsInTableForTransaction("cbv_notes", sqlQueryString, sqlCon);
            }

            return insertedId;
        }



        #endregion

        public static List<int> InsertRowsToLocAudio(DataTable dataTable)
        {
            var insertedIds = new List<int>();
            var sqlCon = GetOpenedConnection();
            using (var transaction = sqlCon.BeginTransaction())
            {
                try
                {
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

                        var insertedId = InsertBlobRecordsInTable("cbv_locaudio", sqlQueryString, dataTable.Rows[0]["locaudio_media"] as byte[], sqlCon);
                        insertedIds.Add(insertedId);
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
                sqlCon?.Close();
            }
            return insertedIds;
        }


        #region == FinalMp4 Tables ==

        public static int InsertRowsToFinalMp4(DataTable dataTable)
        {
            var values = new List<string>();
            var insertedId = -1;
            var sqlCon = GetOpenedConnection();
            using (var transaction = sqlCon.BeginTransaction())
            {
                try
                {
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

                        insertedId = InsertBlobRecordsInTable("cbv_finalmp4", sqlQueryString, dr["finalmp4_media"] as byte[], sqlCon);
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
                sqlCon?.Close();
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

        private static int InsertBlobRecordsInTable(string tableName, string InsertQuery, byte[] blob, SQLiteConnection sqlCon)
        {
            var id = -1;
            try
            {
                using (var command = new SQLiteCommand(InsertQuery, sqlCon))
                {
                    var dataParameter = new SQLiteParameter("blob", DbType.Binary) { Value = blob };
                    command.Parameters.Add(dataParameter);
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
                throw;
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

        public static int GetMaxIndexForNotesForTransaction(int fkVideoEventId, SQLiteConnection sqlCon)
        {
            int count;
            string sqlQueryString = $@"Select IFNULL(Max(notes_index),0) + 1 from cbv_notes where fk_notes_videoevent = {fkVideoEventId} and notes_isdeleted = 0";
            var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
            count = Convert.ToInt32(sqlQuery.ExecuteScalar());
            sqlQuery.Dispose();
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

        public static List<CBVBackground> GetBackgroundForTransaction(int company_id, SQLiteConnection sqlCon)
        {
            var data = new List<CBVBackground>();
            string sqlQueryString = $@"SELECT * FROM cbv_background ";
            if (company_id > -1)
                sqlQueryString += $@" where fk_background_company = {company_id} ";

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

        public static int IsProjectPlanningAvailableForTransaction(int projectId, SQLiteConnection sqlCon)
        {
            int planning_id = -1;
            // Check if database is created
            string sqlQueryString = $@"SELECT planning_id FROM cbv_planning Where fk_planning_project = {projectId} Limit 1";


            var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
            using (var sqlReader = sqlQuery.ExecuteReader())
            {
                while (sqlReader.Read())
                {
                    planning_id = Convert.ToInt32(sqlReader["planning_id"]);
                    return planning_id;
                }
            }
            sqlQuery.Dispose();
            return planning_id;
        }

        public static int IsProjectAvailableForTransaction(int projectServerId, SQLiteConnection sqlCon)
        {
            int project_id = -1;
            string sqlQueryString = $@"SELECT project_id FROM cbv_project Where project_serverid = {projectServerId}";
            var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
            using (var sqlReader = sqlQuery.ExecuteReader())
            {
                while (sqlReader.Read())
                {
                    project_id = Convert.ToInt32(sqlReader["project_id"]);
                    return project_id;
                }
            }
            sqlQuery.Dispose();

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

        public static List<CBVProjectForJoin> GetDownloadedProjectListForTransaction(SQLiteConnection sqlCon)
        {
            var projects = new List<CBVProjectForJoin>();
            string sqlQueryString = $@"SELECT 
                                                P.project_id, P.project_videotitle, P.project_date, P.project_serverid, PD.projdet_version
                                            FROM cbv_project P
                                                JOIN cbv_projdet PD on PD.fk_projdet_project = P.project_id";
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
            sqlQuery.Dispose();
            return projects;
        }

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


        public static List<CBVProject> GetProjectByIdForTransaction(int projectId, bool projdetFlag, SQLiteConnection sqlCon)
        {
            var data = new List<CBVProject>();
            string sqlQueryString = $@"SELECT * FROM cbv_project where project_id = {projectId}";

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
            sqlQuery.Dispose();
            return data;
        }



        #region == Planning GET ==

        public static List<CBVPlanning> GetPlanning(int projectId, bool dependentData = true)
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
                        var planningId = Convert.ToInt32(sqlReader["planning_id"]);
                        var obj = new CBVPlanning
                        {
                            planning_id = planningId,
                            fk_planning_project = Convert.ToInt32(sqlReader["fk_planning_project"]),
                            fk_planning_screen = Convert.ToInt32(sqlReader["fk_planning_screen"]),
                            planning_customname = Convert.ToString(sqlReader["planning_customname"]),
                            planning_notesline = Convert.ToString(sqlReader["planning_notesline"]),
                            planning_medialibid = Convert.ToInt32(sqlReader["planning_medialibid"]),
                            planning_sort = Convert.ToInt32(sqlReader["planning_sort"]),
                            planning_suggestnotesline = Convert.ToString(sqlReader["planning_suggestnotesline"]),
                            planning_createdate = Convert.ToDateTime(sqlReader["planning_createdate"]),
                            planning_modifydate = Convert.ToDateTime(sqlReader["planning_modifydate"]),
                            planning_serverid = Convert.ToInt64(sqlReader["planning_serverid"]),
                            planning_issynced = Convert.ToBoolean(sqlReader["planning_issynced"]),
                            planning_syncerror = Convert.ToString(sqlReader["planning_syncerror"]),
                            planning_isedited = Convert.ToBoolean(sqlReader["planning_isedited"]),
                            planning_audioduration = Convert.ToString(sqlReader["planning_audioduration"])
                        };

                        if (dependentData)
                        {
                            obj.planning_desc = GetPlanningDescriptions(planningId);
                            obj.planning_media = GetPlanningMedia(planningId);
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
                if (null != sqlCon)
                    sqlCon.Close();
                throw;
            }

            return data;
        }

        public static List<CBVPlanning> GetPlanningForTransaction(int projectId, SQLiteConnection sqlCon, bool dependentData = true)
        {
            var data = new List<CBVPlanning>();
            string sqlQueryString = $@"SELECT * FROM cbv_planning where fk_planning_project = {projectId}";
            var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
            using (var sqlReader = sqlQuery.ExecuteReader())
            {
                while (sqlReader.Read())
                {
                    var planningId = Convert.ToInt32(sqlReader["planning_id"]);
                    var obj = new CBVPlanning
                    {
                        planning_id = planningId,
                        fk_planning_project = Convert.ToInt32(sqlReader["fk_planning_project"]),
                        fk_planning_screen = Convert.ToInt32(sqlReader["fk_planning_screen"]),
                        planning_customname = Convert.ToString(sqlReader["planning_customname"]),
                        planning_notesline = Convert.ToString(sqlReader["planning_notesline"]),
                        planning_medialibid = Convert.ToInt32(sqlReader["planning_medialibid"]),
                        planning_sort = Convert.ToInt32(sqlReader["planning_sort"]),
                        planning_suggestnotesline = Convert.ToString(sqlReader["planning_suggestnotesline"]),
                        planning_createdate = Convert.ToDateTime(sqlReader["planning_createdate"]),
                        planning_modifydate = Convert.ToDateTime(sqlReader["planning_modifydate"]),
                        planning_serverid = Convert.ToInt64(sqlReader["planning_serverid"]),
                        planning_issynced = Convert.ToBoolean(sqlReader["planning_issynced"]),
                        planning_syncerror = Convert.ToString(sqlReader["planning_syncerror"]),
                        planning_isedited = Convert.ToBoolean(sqlReader["planning_isedited"]),
                        planning_audioduration = Convert.ToString(sqlReader["planning_audioduration"])
                    };

                    if (dependentData)
                    {
                        obj.planning_desc = GetPlanningDescriptionsForTransaction(planningId, sqlCon);
                        obj.planning_media = GetPlanningMediaForTransaction(planningId, sqlCon);
                    }
                    data.Add(obj);
                }
            }
            sqlQuery.Dispose();
            return data;
        }



        public static CBVPlanning GetPlanningById(int planningid = 0, Int64 planningServerId = 0)
        {
            var obj = new CBVPlanning();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_planning where planning_id = {planningid}";
            if (planningid == 0 && planningServerId > 0)
                sqlQueryString = $@"SELECT * FROM cbv_planning where planning_serverid = {planningServerId}";
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
                        var planningId = Convert.ToInt32(sqlReader["planning_id"]);
                        obj = new CBVPlanning
                        {
                            planning_id = planningId,
                            fk_planning_project = Convert.ToInt32(sqlReader["fk_planning_project"]),
                            fk_planning_screen = Convert.ToInt32(sqlReader["fk_planning_screen"]),
                            planning_customname = Convert.ToString(sqlReader["planning_customname"]),
                            planning_notesline = Convert.ToString(sqlReader["planning_notesline"]),
                            planning_medialibid = Convert.ToInt32(sqlReader["planning_medialibid"]),
                            planning_sort = Convert.ToInt32(sqlReader["planning_sort"]),
                            planning_suggestnotesline = Convert.ToString(sqlReader["planning_suggestnotesline"]),
                            planning_createdate = Convert.ToDateTime(sqlReader["planning_createdate"]),
                            planning_modifydate = Convert.ToDateTime(sqlReader["planning_modifydate"]),
                            planning_serverid = Convert.ToInt64(sqlReader["planning_serverid"]),
                            planning_issynced = Convert.ToBoolean(sqlReader["planning_issynced"]),
                            planning_syncerror = Convert.ToString(sqlReader["planning_syncerror"]),
                            planning_isedited = Convert.ToBoolean(sqlReader["planning_isedited"]),
                            planning_audioduration = Convert.ToString(sqlReader["planning_audioduration"])
                        };
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

            return obj;
        }

        private static List<CBVPlanningDesc> GetPlanningDescriptions(int planning_id)
        {
            var data = new List<CBVPlanningDesc>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_planningdesc where planningdesc_id = {planning_id}";

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
                        var obj = new CBVPlanningDesc
                        {
                            planningdesc_id = Convert.ToInt32(sqlReader["planningdesc_id"]),
                            planningdesc_line = Convert.ToString(sqlReader["planningdesc_line"]),
                            planningdesc_bullets = GetPlanningBullets(Convert.ToInt32(sqlReader["planningdesc_id"]))
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

        private static List<CBVPlanningDesc> GetPlanningDescriptionsForTransaction(int planning_id, SQLiteConnection sqlCon)
        {
            var data = new List<CBVPlanningDesc>();
            string sqlQueryString = $@"SELECT * FROM cbv_planningdesc where planningdesc_id = {planning_id}";
            var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
            using (var sqlReader = sqlQuery.ExecuteReader())
            {
                while (sqlReader.Read())
                {
                    var obj = new CBVPlanningDesc
                    {
                        planningdesc_id = Convert.ToInt32(sqlReader["planningdesc_id"]),
                        planningdesc_line = Convert.ToString(sqlReader["planningdesc_line"]),
                        planningdesc_bullets = GetPlanningBulletsForTransaction(Convert.ToInt32(sqlReader["planningdesc_id"]), sqlCon)
                    };
                    data.Add(obj);
                }
            }
            sqlQuery.Dispose();
            return data;
        }

        private static List<CBVPlanningBullet> GetPlanningBullets(int planningdesc_id)
        {
            var data = new List<CBVPlanningBullet>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_planningbullet where fk_planningbullet_desc = {planningdesc_id}";

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
                        var obj = new CBVPlanningBullet
                        {
                            planningbullet_id = Convert.ToInt32(sqlReader["planningbullet_id"]),
                            fk_planningbullet_desc = Convert.ToInt32(sqlReader["fk_planningbullet_desc"]),
                            planningbullet_line = Convert.ToString(sqlReader["planningbullet_line"]),
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

        private static List<CBVPlanningBullet> GetPlanningBulletsForTransaction(int planningdesc_id, SQLiteConnection sqlCon)
        {
            var data = new List<CBVPlanningBullet>();
            string sqlQueryString = $@"SELECT * FROM cbv_planningbullet where fk_planningbullet_desc = {planningdesc_id}";
            var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
            using (var sqlReader = sqlQuery.ExecuteReader())
            {
                while (sqlReader.Read())
                {
                    var obj = new CBVPlanningBullet
                    {
                        planningbullet_id = Convert.ToInt32(sqlReader["planningbullet_id"]),
                        fk_planningbullet_desc = Convert.ToInt32(sqlReader["fk_planningbullet_desc"]),
                        planningbullet_line = Convert.ToString(sqlReader["planningbullet_line"]),
                    };
                    data.Add(obj);
                }
            }
            // Close database
            sqlQuery.Dispose();
            return data;
        }

        private static List<CBVPlanningMedia> GetPlanningMedia(int planning_id)
        {
            var data = new List<CBVPlanningMedia>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_planningmedia where planningmedia_id = {planning_id}";

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
                        var obj = new CBVPlanningMedia
                        {
                            planningmedia_id = Convert.ToInt32(sqlReader["planningmedia_id"]),
                            planningmedia_mediafull = GetBlobMedia(sqlReader, "planningmedia_mediafull"),
                            planningmedia_mediathumb = GetBlobMedia(sqlReader, "planningmedia_mediathumb")
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

        private static List<CBVPlanningMedia> GetPlanningMediaForTransaction(int planning_id, SQLiteConnection sqlCon)
        {
            var data = new List<CBVPlanningMedia>();
            string sqlQueryString = $@"SELECT * FROM cbv_planningmedia where planningmedia_id = {planning_id}";
            var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
            using (var sqlReader = sqlQuery.ExecuteReader())
            {
                while (sqlReader.Read())
                {
                    var obj = new CBVPlanningMedia
                    {
                        planningmedia_id = Convert.ToInt32(sqlReader["planningmedia_id"]),
                        planningmedia_mediafull = GetBlobMedia(sqlReader, "planningmedia_mediafull"),
                        planningmedia_mediathumb = GetBlobMedia(sqlReader, "planningmedia_mediathumb")
                    };
                    data.Add(obj);
                }
            }
            sqlQuery.Dispose();
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

            if (designFlag)
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
                            videoevent_id = Convert.ToInt32(sqlReader["videoevent_id"]),
                            fk_videoevent_projdet = Convert.ToInt32(sqlReader["fk_videoevent_projdet"]),
                            fk_videoevent_media = Convert.ToInt32(sqlReader["fk_videoevent_media"]),
                            videoevent_track = Convert.ToInt32(sqlReader["videoevent_track"]),
                            videoevent_start = Convert.ToString(sqlReader["videoevent_start"]),
                            videoevent_end = Convert.ToString(sqlReader["videoevent_end"]),
                            videoevent_duration = Convert.ToString(sqlReader["videoevent_duration"]),
                            videoevent_origduration = Convert.ToString(sqlReader["videoevent_origduration"]),
                            videoevent_planning = Convert.ToInt32(sqlReader["videoevent_planning"]),
                            videoevent_createdate = Convert.ToDateTime(sqlReader["videoevent_createdate"]),
                            videoevent_modifydate = Convert.ToDateTime(sqlReader["videoevent_modifydate"]),
                            videoevent_isdeleted = Convert.ToBoolean(sqlReader["videoevent_isdeleted"]),
                            videoevent_issynced = Convert.ToBoolean(sqlReader["videoevent_issynced"]),
                            videoevent_serverid = Convert.ToInt64(sqlReader["videoevent_serverid"]),
                            videoevent_syncerror = Convert.ToString(sqlReader["videoevent_syncerror"])
                        };

                        if (designFlag)
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

        public static List<CBVVideoEvent> GetVideoEventbyServerId(Int64 videoeventServerId)
        {
            var data = new List<CBVVideoEvent>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT * FROM cbv_videoevent where videoevent_serverid = {videoeventServerId}";


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
                            videoevent_id = Convert.ToInt32(sqlReader["videoevent_id"]),
                            fk_videoevent_projdet = Convert.ToInt32(sqlReader["fk_videoevent_projdet"]),
                            fk_videoevent_media = Convert.ToInt32(sqlReader["fk_videoevent_media"]),
                            videoevent_track = Convert.ToInt32(sqlReader["videoevent_track"]),
                            videoevent_start = Convert.ToString(sqlReader["videoevent_start"]),
                            videoevent_end = Convert.ToString(sqlReader["videoevent_end"]),
                            videoevent_duration = Convert.ToString(sqlReader["videoevent_duration"]),
                            videoevent_origduration = Convert.ToString(sqlReader["videoevent_origduration"]),
                            videoevent_planning = Convert.ToInt32(sqlReader["videoevent_planning"]),
                            videoevent_createdate = Convert.ToDateTime(sqlReader["videoevent_createdate"]),
                            videoevent_modifydate = Convert.ToDateTime(sqlReader["videoevent_modifydate"]),
                            videoevent_isdeleted = Convert.ToBoolean(sqlReader["videoevent_isdeleted"]),
                            videoevent_issynced = Convert.ToBoolean(sqlReader["videoevent_issynced"]),
                            videoevent_serverid = Convert.ToInt64(sqlReader["videoevent_serverid"]),
                            videoevent_syncerror = Convert.ToString(sqlReader["videoevent_syncerror"])
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

        public static List<CBVShiftVideoEvent> GetShiftVideoEventsbyTime(int fk_videoevent_projdet, string endTime, EnumTrack track = EnumTrack.IMAGEORVIDEO)
        {
            var data = new List<CBVShiftVideoEvent>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string whereTrackClause = "";
            if (track == EnumTrack.IMAGEORVIDEO)
                whereTrackClause = $@"videoevent_track = {(int)track}";
            else
                whereTrackClause = $@"videoevent_track > {(int)EnumTrack.IMAGEORVIDEO}"; // For callout1 and callout 2

            string sqlQueryString = $@" Select 
	                                        videoevent_id,
	                                        videoevent_start, 
	                                        videoevent_duration,
                                            videoevent_origduration,
	                                        videoevent_end, 
	                                        videoevent_serverid
                                        FROM
	                                        cbv_videoevent
                                        where 
	                                        videoevent_start >= '{endTime}'
	                                        And fk_videoevent_projdet = {fk_videoevent_projdet}
	                                        And {whereTrackClause}
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
                    obj.videoevent_end = Convert.ToString(sqlObject.SQLiteDataReader["videoevent_end"]);
                    obj.videoevent_duration = Convert.ToString(sqlObject.SQLiteDataReader["videoevent_duration"]);
                    obj.videoevent_origduration = Convert.ToString(sqlObject.SQLiteDataReader["videoevent_duration"]);
                    obj.videoevent_serverid = Convert.ToInt64(sqlObject.SQLiteDataReader["videoevent_serverid"]);
                    data.Add(obj);
                }
                sqlObject.CloseConnections();
            }
            return data;


        }

        public static List<CBVVideoEvent> GetOverlappingCalloutsByTime(int fk_videoevent_projdet, string startTime, string endtime)
        {
            var data = new List<CBVVideoEvent>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@" Select 
	                                        * 
                                        from 
	                                        cbv_videoevent 
                                        where 
	                                        videoevent_track > 2
	                                        AND videoevent_isdeleted = 0
	                                        AND fk_videoevent_projdet = {fk_videoevent_projdet}
	                                        AND (
		                                        videoevent_start BETWEEN '{startTime}' AND '{endtime}'
		                                        OR 
		                                        videoevent_end BETWEEN '{startTime}' AND '{endtime}'
		                                        )";

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
                            videoevent_id = Convert.ToInt32(sqlReader["videoevent_id"]),
                            fk_videoevent_projdet = Convert.ToInt32(sqlReader["fk_videoevent_projdet"]),
                            fk_videoevent_media = Convert.ToInt32(sqlReader["fk_videoevent_media"]),
                            videoevent_track = Convert.ToInt32(sqlReader["videoevent_track"]),
                            videoevent_start = Convert.ToString(sqlReader["videoevent_start"]),
                            videoevent_end = Convert.ToString(sqlReader["videoevent_end"]),
                            videoevent_duration = Convert.ToString(sqlReader["videoevent_duration"]),
                            videoevent_origduration = Convert.ToString(sqlReader["videoevent_origduration"]),
                            videoevent_createdate = Convert.ToDateTime(sqlReader["videoevent_createdate"]),
                            videoevent_modifydate = Convert.ToDateTime(sqlReader["videoevent_modifydate"]),
                            videoevent_isdeleted = Convert.ToBoolean(sqlReader["videoevent_isdeleted"]),
                            videoevent_issynced = Convert.ToBoolean(sqlReader["videoevent_issynced"]),
                            videoevent_serverid = Convert.ToInt64(sqlReader["videoevent_serverid"]),
                            videoevent_syncerror = Convert.ToString(sqlReader["videoevent_syncerror"])
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
                            videoevent_id = Convert.ToInt32(sqlReader["videoevent_id"]),
                            fk_videoevent_projdet = Convert.ToInt32(sqlReader["fk_videoevent_projdet"]),
                            fk_videoevent_media = Convert.ToInt32(sqlReader["fk_videoevent_media"]),
                            videoevent_track = Convert.ToInt32(sqlReader["videoevent_track"]),
                            videoevent_start = Convert.ToString(sqlReader["videoevent_start"]),
                            videoevent_end = Convert.ToString(sqlReader["videoevent_end"]),
                            videoevent_duration = Convert.ToString(sqlReader["videoevent_duration"]),
                            videoevent_origduration = Convert.ToString(sqlReader["videoevent_origduration"]),
                            videoevent_planning = Convert.ToInt32(sqlReader["videoevent_planning"]),
                            videoevent_createdate = Convert.ToDateTime(sqlReader["videoevent_createdate"]),
                            videoevent_modifydate = Convert.ToDateTime(sqlReader["videoevent_modifydate"]),
                            videoevent_isdeleted = Convert.ToBoolean(sqlReader["videoevent_isdeleted"]),
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
                sqlCon?.Close();
            }
            catch (Exception)
            {
                sqlCon?.Close();
                return new List<CBVVideoEvent>();
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

            if (designFlag)
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
                            videoevent_id = Convert.ToInt32(sqlReader["videoevent_id"]),
                            fk_videoevent_projdet = Convert.ToInt32(sqlReader["fk_videoevent_projdet"]),
                            fk_videoevent_media = Convert.ToInt32(sqlReader["fk_videoevent_media"]),
                            videoevent_track = Convert.ToInt32(sqlReader["videoevent_track"]),
                            videoevent_start = Convert.ToString(sqlReader["videoevent_start"]),
                            videoevent_end = Convert.ToString(sqlReader["videoevent_end"]),
                            videoevent_duration = Convert.ToString(sqlReader["videoevent_duration"]),
                            videoevent_origduration = Convert.ToString(sqlReader["videoevent_origduration"]),
                            videoevent_planning = Convert.ToInt32(sqlReader["videoevent_planning"]),
                            videoevent_createdate = Convert.ToDateTime(sqlReader["videoevent_createdate"]),
                            videoevent_modifydate = Convert.ToDateTime(sqlReader["videoevent_modifydate"]),
                            videoevent_isdeleted = Convert.ToBoolean(sqlReader["videoevent_isdeleted"]),
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

            return data.GroupBy(x => x.videoevent_id).Select(p => p.First()).Distinct().ToList();
        }


        public static List<CBVVideoEvent> GetPlaceholderVideoEvents(int projdetId)
        {
            var data = new List<CBVVideoEvent>();

            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT 
                                            VE.* 
                                        FROM 
                                            cbv_videoevent VE
                                            INNER JOIN cbv_planning P on VE.videoevent_planning = P.planning_serverid AND (P.fk_planning_screen = 4 or P.fk_planning_screen = 7) ";

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
                            videoevent_id = Convert.ToInt32(sqlReader["videoevent_id"]),
                            fk_videoevent_projdet = Convert.ToInt32(sqlReader["fk_videoevent_projdet"]),
                            fk_videoevent_media = Convert.ToInt32(sqlReader["fk_videoevent_media"]),
                            videoevent_track = Convert.ToInt32(sqlReader["videoevent_track"]),
                            videoevent_start = Convert.ToString(sqlReader["videoevent_start"]),
                            videoevent_end = Convert.ToString(sqlReader["videoevent_end"]),
                            videoevent_duration = Convert.ToString(sqlReader["videoevent_duration"]),
                            videoevent_origduration = Convert.ToString(sqlReader["videoevent_origduration"]),
                            videoevent_planning = Convert.ToInt32(sqlReader["videoevent_planning"]),
                            videoevent_createdate = Convert.ToDateTime(sqlReader["videoevent_createdate"]),
                            videoevent_modifydate = Convert.ToDateTime(sqlReader["videoevent_modifydate"]),
                            videoevent_isdeleted = Convert.ToBoolean(sqlReader["videoevent_isdeleted"]),
                            videoevent_issynced = Convert.ToBoolean(sqlReader["videoevent_issynced"]),
                            videoevent_serverid = Convert.ToInt64(sqlReader["videoevent_serverid"]),
                            videoevent_syncerror = Convert.ToString(sqlReader["videoevent_syncerror"])
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

            return data.GroupBy(x => x.videoevent_id).Select(p => p.First()).Distinct().ToList();
        }

        public static bool IsPlaceHolderEvent(int videoeventId)
        {
            var data = new List<CBVVideoEvent>();
            // Check if database is created
            if (false == IsDbCreated())
                throw new Exception("Database is not present.");

            string sqlQueryString = $@"SELECT 
                                            VE.* 
                                        FROM 
                                            cbv_videoevent VE
                                            INNER JOIN cbv_planning P on VE.videoevent_planning = P.planning_serverid AND (P.fk_planning_screen = 4 or P.fk_planning_screen = 7) 
                                        where VE.videoevent_id = {videoeventId}";

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
                            videoevent_id = Convert.ToInt32(sqlReader["videoevent_id"]),
                            fk_videoevent_projdet = Convert.ToInt32(sqlReader["fk_videoevent_projdet"]),
                            fk_videoevent_media = Convert.ToInt32(sqlReader["fk_videoevent_media"]),
                            videoevent_track = Convert.ToInt32(sqlReader["videoevent_track"]),
                            videoevent_start = Convert.ToString(sqlReader["videoevent_start"]),
                            videoevent_end = Convert.ToString(sqlReader["videoevent_end"]),
                            videoevent_duration = Convert.ToString(sqlReader["videoevent_duration"]),
                            videoevent_origduration = Convert.ToString(sqlReader["videoevent_origduration"]),
                            videoevent_planning = Convert.ToInt32(sqlReader["videoevent_planning"]),
                            videoevent_createdate = Convert.ToDateTime(sqlReader["videoevent_createdate"]),
                            videoevent_modifydate = Convert.ToDateTime(sqlReader["videoevent_modifydate"]),
                            videoevent_isdeleted = Convert.ToBoolean(sqlReader["videoevent_isdeleted"]),
                            videoevent_issynced = Convert.ToBoolean(sqlReader["videoevent_issynced"]),
                            videoevent_serverid = Convert.ToInt64(sqlReader["videoevent_serverid"]),
                            videoevent_syncerror = Convert.ToString(sqlReader["videoevent_syncerror"])
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

            return data.Any();
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

        public static bool ReferentialKeyPresentForTransaction(SQLiteConnection sqlCon, string tablename, string IdColumnName, int id = -1)
        {
            var flag = false;
            string sqlQueryString = $@"SELECT * FROM {tablename} WHERE {IdColumnName} = {id}";
            var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
            using (var sqlReader = sqlQuery.ExecuteReader())
            {
                flag = sqlReader.HasRows;
            }
            // Close database
            sqlQuery.Dispose();


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
        private static bool ExecuteNonQueryInTable(string UpdateQuery, SQLiteConnection sqlCon)
        {
            // Execute the SQLite query
            var sqlQuery = new SQLiteCommand(UpdateQuery, sqlCon);
            try
            {
                sqlQuery.ExecuteNonQuery();
                sqlQuery.Dispose();
                return true;
            }
            catch (Exception)
            {
                sqlQuery?.Dispose();
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
            var sqlCon = GetOpenedConnection();
            using (var transaction = sqlCon.BeginTransaction())
            {
                try
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
                        var updateFlag = ExecuteNonQueryInTable(updateQueryString, sqlCon);
                        Console.WriteLine($@"cbv_project table update status for id - {project_id} result - {updateFlag}");
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
                sqlCon?.Close();
            }
        }

        public static void ShiftVideoEvents(DataTable dataTable)
        {
            var sqlCon = GetOpenedConnection();
            using (var transaction = sqlCon.BeginTransaction())
            {
                try
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {

                        var modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                        var videoevent_serverid = Convert.ToInt32(dr["videoevent_serverid"]);
                        var updateQueryString = $@" UPDATE cbv_videoevent 
                                        SET 
                                            videoevent_start = '{Convert.ToString(dr["videoevent_start"])}',
                                            videoevent_duration = '{Convert.ToString(dr["videoevent_duration"])}',
                                            videoevent_origduration = '{Convert.ToString(dr["videoevent_origduration"])}',
                                            videoevent_end = '{Convert.ToString(dr["videoevent_end"])}',
                                            videoevent_modifydate = '{modifyDate}'
                                        WHERE 
                                            videoevent_serverid = {videoevent_serverid}";
                        var updateFlag = ExecuteNonQueryInTable(updateQueryString, sqlCon);
                        Console.WriteLine($@"cbv_videoevent table update status for serverid - {videoevent_serverid} result - {updateFlag}");
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
                sqlCon?.Close();
            }
        }

        public static void UpdateRowsToVideoEvent(DataTable dataTable)
        {
            var sqlCon = GetOpenedConnection();
            using (var transaction = sqlCon.BeginTransaction())
            {
                try
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
                                            videoevent_origduration = '{Convert.ToString(dr["videoevent_origduration"])}',
                                            videoevent_end = '{Convert.ToString(dr["videoevent_end"])}',
                                            videoevent_isdeleted = {Convert.ToBoolean(dr["videoevent_isdeleted"])},
                                            videoevent_issynced = {Convert.ToBoolean(dr["videoevent_issynced"])},
                                            videoevent_syncerror = '{Convert.ToString(dr["videoevent_syncerror"])}',
                                            videoevent_modifydate = '{modifyDate}'
                                        WHERE 
                                            videoevent_id = {videoevent_id}";
                        var updateFlag = ExecuteNonQueryInTable(updateQueryString, sqlCon);
                        Console.WriteLine($@"cbv_videoevent table update status for id - {videoevent_id} result - {updateFlag}");
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
                sqlCon?.Close();
            }
        }

        public static void UpdateServerId(string table, int localId, Int64 serverId, string ErrorMessage)
        {
            var modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            var sqlCon = GetOpenedConnection();
            using (var transaction = sqlCon.BeginTransaction())
            {
                try
                {

                    var updateQueryString = $@" UPDATE cbv_{table} 
                                        SET 
                                            {table}_issynced = {string.IsNullOrEmpty(ErrorMessage)},
                                            {table}_serverid = {serverId},
                                            {table}_syncerror = '{ErrorMessage}',
                                            {table}_modifydate = '{modifyDate}'
                                        WHERE 
                                            {table}_id = {localId}";
                    var updateFlag = ExecuteNonQueryInTable(updateQueryString, sqlCon);
                    Console.WriteLine($@"cbv_{table} table update status for id - {localId} result - {updateFlag}");
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
                sqlCon?.Close();
            }
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
            var sqlCon = GetOpenedConnection();
            using (var transaction = sqlCon.BeginTransaction())
            {
                try
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
                                            design_syncerror = '{Convert.ToString(dr["design_syncerror"])}',
                                            design_modifydate = '{modifyDate}'
                                        WHERE 
                                            design_id = {design_id}";
                        var updateFlag = ExecuteNonQueryInTable(updateQueryString, sqlCon);
                        Console.WriteLine($@"cbv_design table update status for id - {design_id} result - {updateFlag}");
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
                sqlCon?.Close();
            }
        }

        public static void UpdateRowsToNotes(DataTable dataTable, bool isNotesServerId = false)
        {
            var sqlCon = GetOpenedConnection();
            using (var transaction = sqlCon.BeginTransaction())
            {
                try
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        var modifyDate = Convert.ToString(dr["notes_modifydate"]);
                        if (string.IsNullOrEmpty(modifyDate))
                            modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                        var whereClause = string.Empty;
                        var notes_id = Convert.ToInt32(dr["notes_id"]);
                        if (!isNotesServerId)
                        {
                            whereClause = $"WHERE notes_id = {notes_id}";
                        }
                        else
                        {
                            notes_id = Convert.ToInt32(dr["notes_serverid"]);
                            whereClause = $"WHERE notes_serverid = {notes_id}";
                        }
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
                                       {whereClause}";
                        var updateFlag = ExecuteNonQueryInTable(updateQueryString, sqlCon);
                        Console.WriteLine($@"cbv_notes table update status for id - {notes_id} result - {updateFlag}");
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
                sqlCon?.Close();
            }
        }

        public static void UpdateRowsToNotes(DataRow dr)
        {
            var modifyDate = Convert.ToString(dr["notes_modifydate"]);
            if (string.IsNullOrEmpty(modifyDate))
                modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var notes_id = Convert.ToInt32(dr["notes_id"]);
            var sqlCon = GetOpenedConnection();
            using (var transaction = sqlCon.BeginTransaction())
            {
                try
                {
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
                    var updateFlag = ExecuteNonQueryInTable(updateQueryString, sqlCon);
                    Console.WriteLine($@"cbv_notes table update status for id - {notes_id} result - {updateFlag}");
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
                sqlCon?.Close();
            }
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
            var sqlCon = GetOpenedConnection();
            using (var transaction = sqlCon.BeginTransaction())
            {
                try
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
                        var updateFlag = ExecuteNonQueryInTable(updateQueryString, sqlCon);
                        Console.WriteLine($@"cbv_voicetimer table update status for id - {voicetimer_id} result - {updateFlag}");
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
                sqlCon?.Close();
            }
        }

        public static void UpdateRowsToVoiceAverage(DataTable dataTable)
        {
            var sqlCon = GetOpenedConnection();
            using (var transaction = sqlCon.BeginTransaction())
            {
                try
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        var voiceaverage_id = Convert.ToInt32(dr["voiceaverage_id"]);
                        var updateQueryString = $@" UPDATE cbv_voiceaverage 
                                        SET 
                                            voiceaverage_average = '{Convert.ToString(dr["voiceaverage_average"])}'
                                        WHERE 
                                            voiceaverage_id = {voiceaverage_id}";
                        var updateFlag = ExecuteNonQueryInTable(updateQueryString, sqlCon);
                        Console.WriteLine($@"cbv_voiceaverage table update status for id - {voiceaverage_id} result - {updateFlag}");
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
                sqlCon?.Close();
            }
        }


        public static void SetProjectDetailSubmitted(int projdet_id)
        {
            var sqlCon = GetOpenedConnection();
            using (var transaction = sqlCon.BeginTransaction())
            {
                try
                {
                    var modifyDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                    var updateQueryString = $@" UPDATE cbv_projdet 
                                    SET 
                                        projdet_submitted = 1,
                                        projdet_modifydate = '{modifyDate}'
                                    WHERE 
                                        projdet_id = {projdet_id}";
                    var updateFlag = ExecuteNonQueryInTable(updateQueryString, sqlCon);
                    Console.WriteLine($@"cbv_projdet table update status for id - {projdet_id} result - {updateFlag}");
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
                sqlCon?.Close();
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


        public static void HardDeletePlanningsByProjectIdForTransaction(int projectId, SQLiteConnection sqlCon)
        {
            var deleteQueryString = $@" 
                                        Delete From cbv_planning 
                                        WHERE 
                                            fk_planning_project = {projectId};
                                        ";
            ExecuteNonQueryForTransaction(deleteQueryString, sqlCon);
            Console.WriteLine($@"cbv_planning table delete status for project Id - {projectId} | result - {true}");
        }

        public static void HardDeleteVideoEventsByIdForTransaction(int videoeventId, bool cascadeDelete, SQLiteConnection sqlCon)
        {
            var deleteQueryString = $@" 
                                        Delete From cbv_videoevent 
                                        WHERE 
                                            videoevent_id = {videoeventId};
                                        ";

            if (cascadeDelete == true)
                deleteQueryString = $@" Delete From  cbv_design
                                        WHERE 
                                            fk_design_videoevent = {videoeventId};

                                        Delete From cbv_videosegment
                                        WHERE 
                                            videosegment_id = {videoeventId};

                                        Delete From  cbv_locaudio
                                        WHERE 
                                        fk_locaudio_notes in (Select notes_id from cbv_notes WHERE fk_notes_videoevent = {videoeventId});

                                        Delete From cbv_notes
                                        WHERE 
                                            fk_notes_videoevent = {videoeventId};
                                        " + deleteQueryString;
            ExecuteNonQueryForTransaction(deleteQueryString, sqlCon);
            Console.WriteLine($@"cbv_videoevent table delete status for id - {videoeventId} | result - {true}");
        }

        public static void HardDeleteVideoEventsByServerIdForTransaction(int serverVideoeventId, bool cascadeDelete, SQLiteConnection sqlCon)
        {
            int videoEventId = -1;
            string sqlQueryString = $@"SELECT videoevent_id FROM cbv_videoevent where videoevent_serverId = {serverVideoeventId}";
            var sqlQuery = new SQLiteCommand(sqlQueryString, sqlCon);
            using (var sqlReader = sqlQuery.ExecuteReader())
            {
                while (sqlReader.Read())
                {
                    videoEventId = sqlReader.GetInt32(0);
                    break;
                }
            }
            sqlQuery.Dispose();
            if (videoEventId > 0)
                HardDeleteVideoEventsByIdForTransaction(videoEventId, cascadeDelete, sqlCon);
            Console.WriteLine($@"cbv_videoevent table delete status for videoevent serverid - {serverVideoeventId} completed");
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
        public static void UpsertRowsToAppForTransaction(DataTable dataTable, SQLiteConnection sqlCon)
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

                var upsertFlag = ExecuteNonQueryInTable(upsertQueryString, sqlCon);
                Console.WriteLine($@"cbv_app table upsert status for id - {app_id} result - {upsertFlag}");
            }
        }

        public static void UpsertRowsToMediaForTransaction(DataTable dataTable, SQLiteConnection sqlCon)
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

                var upsertFlag = ExecuteNonQueryInTable(upsertQueryString, sqlCon);
                Console.WriteLine($@"cbv_media table upsert status for id - {media_id} result - {upsertFlag}");
            }
        }

        public static void UpsertRowsToScreenForTransaction(DataTable dataTable, SQLiteConnection sqlCon)
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

                var upsertFlag = ExecuteNonQueryInTable(upsertQueryString, sqlCon);
                Console.WriteLine($@"cbv_screen table upsert status for id - {screen_id} result - {upsertFlag}");
            }
        }

        public static void UpsertRowsToCompanyForTransaction(DataTable dataTable, SQLiteConnection sqlCon)
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

                var upsertFlag = ExecuteNonQueryInTable(upsertQueryString, sqlCon);
                Console.WriteLine($@"cbv_company table upsert status for id - {company_id} result - {company_name}");
            }
        }

        public static void UpsertRowsToBackgroundForTransaction(DataTable dataTable, SQLiteConnection sqlCon)
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
                var upsertFlag = InsertBlobRecordsInTable("cbv_background", upsertQueryString, dr["background_media"] as byte[], sqlCon);
                Console.WriteLine($@"cbv_background table upsert status for id - {background_id} result - {upsertFlag}");
            }
        }

        public static int UpsertRowsToProjectForTransaction(DataTable dataTable, SQLiteConnection sqlCon)
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

            var insertedId = InsertRecordsInTableForTransaction("cbv_project", sqlQueryString, sqlCon);
            return insertedId;
        }

        public static int InsertRowsToProjectDetail(DataTable dataTable, int Project_Id, SQLiteConnection sqlCon)
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

                var insertedId = InsertRecordsInTableForTransaction("cbv_projdet", sqlQueryString, sqlCon);
                return insertedId;
            }

            return -1;
        }


        public static List<int> InsertRowsToPlanningForTransaction(DataTable dataTable, SQLiteConnection sqlCon)
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

                var audioDuration = Convert.ToString(dr["planning_audioduration"]);

                var planning_medialibid = Convert.ToInt32(Convert.ToString(dr["planning_medialibid"]) == "" ? 0 : dr["planning_medialibid"]);
                var planning_sort = Convert.ToInt32(dr["planning_sort"]);

                var serverid = Convert.ToInt64(dr["planning_serverid"]);
                var issynced = Convert.ToInt16(dr["planning_issynced"]);
                var isedited = Convert.ToInt16(dr["planning_isedited"]);

                var syncErrorString = Convert.ToString(dr["planning_syncerror"]);
                var syncerror = syncErrorString?.Length > 50 ? syncErrorString.Substring(0, 50) : syncErrorString;

                values.Add($"({dr["fk_planning_project"]},  {dr["fk_planning_screen"]}, '{dr["planning_customname"]}', '{dr["planning_notesline"]}', {planning_medialibid}, {planning_sort}, '{dr["planning_suggestnotesline"]}', " +
                    $"'{createDate}', '{modifyDate}', {serverid}, {issynced}, '{syncerror}', {isedited}, '{audioDuration}')");

                var valuesString = string.Join(",", values.ToArray());
                string sqlQueryString =
                    $@"INSERT INTO  cbv_planning 
                    (fk_planning_project, fk_planning_screen, planning_customname, planning_notesline, planning_medialibid, planning_sort, planning_suggestnotesline, 
                        planning_createdate, planning_modifydate, planning_serverid, planning_issynced, planning_syncerror, planning_isedited, planning_audioduration) 
                VALUES 
                    {valuesString}";

                var insertedId = InsertRecordsInTableForTransaction("cbv_planning", sqlQueryString, sqlCon);

                //Logic for PlanningDesc && Bullets
                var dtDesc = dr["planning_desc"] as DataTable;
                if (dtDesc != null && dtDesc.Rows?.Count > 0)
                    InsertRowsToPlanningDescForTransaction(dtDesc, insertedId, sqlCon);

                //Logic for PlanningMedia
                var dtMedia = dr["planning_media"] as DataTable;
                if (dtMedia != null && dtMedia.Rows?.Count > 0)
                    InsertRowsToPlanningMediaForTransaction(dtMedia, insertedId, sqlCon);

                ids.Add(insertedId);
            }
            return ids;
        }

        private static List<int> InsertRowsToPlanningDescForTransaction(DataTable dataTable, int planning_id, SQLiteConnection sqlCon)
        {
            var ids = new List<int>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var valuesString = $"({planning_id}, '{dr["planningdesc_line"]}')";
                string sqlQueryString =
                    $@" INSERT INTO  cbv_planningdesc 
                            (planningdesc_id, planningdesc_line) 
                        VALUES 
                            {valuesString}";

                var insertedId = InsertRecordsInTableForTransaction("cbv_planningdesc", sqlQueryString, sqlCon); // return -1 always
                                                                                                                 //Logic for Bullets
                var dtbullet = dr["planningdesc_bullet"] as DataTable;
                if (dtbullet != null && dtbullet.Rows?.Count > 0)
                    InsertRowsToPlanningBulletForTransaction(dtbullet, planning_id, sqlCon);
                ids.Add(planning_id);
            }
            return ids;
        }

        private static List<int> InsertRowsToPlanningBulletForTransaction(DataTable dataTable, int planningdesc_id, SQLiteConnection sqlCon)
        {
            var ids = new List<int>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var valuesString = $"({planningdesc_id}, '{dr["planningbullet_line"]}')";
                string sqlQueryString =
                    $@" INSERT INTO  cbv_planningbullet 
                            (fk_planningbullet_desc, planningbullet_line) 
                        VALUES 
                            {valuesString}";

                var insertedId = InsertRecordsInTableForTransaction("cbv_planningbullet", sqlQueryString, sqlCon);
                ids.Add(insertedId);
            }
            return ids;
        }

        private static List<int> InsertRowsToPlanningMediaForTransaction(DataTable dataTable, int planning_id, SQLiteConnection sqlCon)
        {
            var ids = new List<int>();
            foreach (DataRow dr in dataTable.Rows)
            {
                var valuesString = $"({planning_id}, @blob1, @blob2)";
                string sqlQueryString =
                    $@" INSERT INTO  cbv_planningmedia 
                            (planningmedia_id, planningmedia_mediathumb, planningmedia_mediafull) 
                        VALUES 
                            {valuesString}";

                var insertedId = Insert2BlobRecordsInTable("cbv_planningmedia", sqlQueryString,
                            dr["planningmedia_mediathumb"] != DBNull.Value ? dr["planningmedia_mediathumb"] as byte[] : null,
                            dr["planningmedia_mediafull"] != DBNull.Value ? dr["planningmedia_mediafull"] as byte[] : null,
                            sqlCon);
                ids.Add(insertedId);
            }
            return ids;
        }

        #endregion

        private static int Insert2BlobRecordsInTable(string tableName, string InsertQuery, byte[] blob1, byte[] blob2, SQLiteConnection sqlCon)
        {
            var id = -1;
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
            return id;
        }

        private static int InsertRecordsInTableForTransaction(string tableName, string InsertQuery, SQLiteConnection sqlCon)
        {

            // Execute the SQLite query
            var sqlQuery = new SQLiteCommand(InsertQuery, sqlCon);
            sqlQuery.ExecuteNonQuery();
            sqlQuery.Dispose();

            var id = -1;
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
            return id;
        }




    }
}
