using Logger;
using SqlDB;
using System;
using System.Data.SQLite;
using System.IO;

namespace UnitTest
{
    /// <summary>
    /// Helper functions to aid in environment setup
    /// </summary>
    public static class CTestHelper
    {
        private static bool LoggerInit { get; set; }
        private static bool RemovedDB { get; set; }
        private static bool OpenedDB { get; set; }

        private const string SQL_TEST_DATA_SOURCE = "test.db";

        /// <summary>
        /// Initialise log file
        /// </summary>
        public static void InitLogger()
        {
            if(LoggerInit)
            {
                return;
            }
#if DEBUG
            // Log unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CLogger.ExceptionHandleEvent);
#endif
            CLogger.Configure(Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]) + ".log"); // Create a log file
            CLogger.LogInfo("*************************");
            LoggerInit = true;
        }

        /// <summary>
        /// Setup test database tables and data
        /// </summary>
        public static void DatabaseSetup()
        {
            if(!OpenedDB && CSqlDB.Instance.Open(true, SQL_TEST_DATA_SOURCE) == SQLiteErrorCode.Ok)
            {
                OpenedDB = true;
            }
            CSqlDB.Instance.Execute("DELETE FROM Platform");
            CSqlDB.Instance.Execute("insert into Platform (PlatformID, Name, Description) VALUES (1, 'test', 'PlatformID 1')");
        }

        /// <summary>
        /// Delete database file
        /// </summary>
        public static void RemoveDatabase()
        {
            if(RemovedDB)
            {
                return;
            }
            if(File.Exists(SQL_TEST_DATA_SOURCE))
            {
                File.Delete(SQL_TEST_DATA_SOURCE);
            }
            RemovedDB = true;
        }
    }
}
