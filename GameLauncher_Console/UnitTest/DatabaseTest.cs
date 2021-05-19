using GameLauncher_Console;
using Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SQLite;
using System.IO;
using static GameLauncher_Console.CSqlField;

namespace UnitTest
{
    /// <summary>
    /// Class for setting up tests
    /// </summary>
    public static class CTest_Setup
    {
        private static bool LoggerInit { get; set; }
        private static bool RemovedDB  { get; set; }
        private static bool OpenedDB   { get; set; }

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


    /// <summary>
    /// Test query for INSERT INTO ... statements
    /// </summary>
    public class CTest_InsertQry : CSqlQry
    {
        public CTest_InsertQry() : base("Platform")
        {
            m_fields["PlatformID"]  = new CSqlFieldInteger("PlatformID", QryFlag.cInsWrite);
            m_fields["Name"]        = new CSqlFieldString("Name", QryFlag.cInsWrite);
            m_fields["Description"] = new CSqlFieldString("Description", QryFlag.cInsWrite);
        }
        public int PlatformID
        {
            get { return m_fields["PlatformID"].Integer; }
            set { m_fields["PlatformID"].Integer = value; }
        }
        public string Name
        {
            get { return m_fields["Name"].String;   }
            set { m_fields["Name"].String = value;  }
        }
        public string Description
        {
            get { return m_fields["Description"].String; }
            set { m_fields["Description"].String = value; }
        }
    }

    /// <summary>
    /// Test query for SELECT ... WHERE
    /// </summary>
    public class CTest_SelectQry : CSqlQry
    {
        public CTest_SelectQry() : base("Platform")
        {
            m_fields["PlatformID"] = new CSqlFieldInteger("PlatformID", QryFlag.cSelWhere);
            m_fields["Name"]       = new CSqlFieldString("Name", QryFlag.cSelRead);
            m_fields["Description"] = new CSqlFieldString("Description", QryFlag.cSelRead);
        }
        public int PlatformID
        {
            get { return m_fields["PlatformID"].Integer; }
            set { m_fields["PlatformID"].Integer = value; }
        }
        public string Name
        {
            get { return m_fields["Name"].String; }
            set { m_fields["Name"].String = value; }
        }
        public string Description
        {
            get { return m_fields["Description"].String; }
            set { m_fields["Description"].String = value; }
        }
    }

    /// <summary>
    /// Test query for UPDATE ... WHERE
    /// </summary>
    public class CTest_UpdateQry : CSqlQry
    {
        public CTest_UpdateQry() : base("Platform")
        {
            m_fields["PlatformID"] = new CSqlFieldInteger("PlatformID", QryFlag.cUpdWhere);
            m_fields["Description"] = new CSqlFieldString("Description", QryFlag.cUpdWrite);
        }
        public int PlatformID
        {
            get { return m_fields["PlatformID"].Integer; }
            set { m_fields["PlatformID"].Integer = value; }
        }
        public string Description
        {
            get { return m_fields["Description"].String; }
            set { m_fields["Description"].String = value; }
        }
    }

    /// <summary>
    /// Test query for DELETE ... WHERE
    /// </summary>
    public class CTest_DeleteQry : CSqlQry
    {
        public CTest_DeleteQry() : base("Platform")
        {
            m_fields["PlatformID"] = new CSqlFieldInteger("PlatformID", QryFlag.cDelWhere);
        }
        public int PlatformID
        {
            get { return m_fields["PlatformID"].Integer; }
            set { m_fields["PlatformID"].Integer = value; }
        }
    }

    /// <summary>
    /// Test query for selecting multiple columns (no WHERE clause)
    /// </summary>
    public class CTest_SelectManyQry : CSqlQry
    {
        public CTest_SelectManyQry() : base("Platform")
        {
            m_fields["PlatformID"] = new CSqlFieldInteger("PlatformID", QryFlag.cSelRead);
            m_fields["Name"] = new CSqlFieldString("Name", QryFlag.cSelRead);
            m_fields["Description"] = new CSqlFieldString("Description", QryFlag.cSelRead);
        }
        public int PlatformID
        {
            get { return m_fields["PlatformID"].Integer; }
            set { m_fields["PlatformID"].Integer = value; }
        }
        public string Name
        {
            get { return m_fields["Name"].String; }
            set { m_fields["Name"].String = value; }
        }
        public string Description
        {
            get { return m_fields["Description"].String; }
            set { m_fields["Description"].String = value; }
        }
    }

    /// <summary>
    /// Test query for SELECT ... WHERE (multiple conditions)
    /// </summary>
    public class CTest_SelectMultiParamQry : CSqlQry
    {
        public CTest_SelectMultiParamQry() : base("Platform")
        {
            m_fields["PlatformID"] = new CSqlFieldInteger("PlatformID", QryFlag.cSelRead);
            m_fields["Name"] = new CSqlFieldString("Name", QryFlag.cSelWhere);
            m_fields["Description"] = new CSqlFieldString("Description", QryFlag.cSelWhere);
        }
        public int PlatformID
        {
            get { return m_fields["PlatformID"].Integer; }
            set { m_fields["PlatformID"].Integer = value; }
        }
        public string Name
        {
            get { return m_fields["Name"].String; }
            set { m_fields["Name"].String = value; }
        }
        public string Description
        {
            get { return m_fields["Description"].String; }
            set { m_fields["Description"].String = value; }
        }
    }

    /// <summary>
    /// Test query for multiple query types (INSERT, SELECT AND DELETE)
    /// </summary>
    public class CTest_MultiQry : CSqlQry
    {
        public CTest_MultiQry() : base("Platform")
        {
            m_fields["PlatformID"]  = new CSqlFieldInteger("PlatformID",    QryFlag.cInsWrite | QryFlag.cSelWhere | QryFlag.cUpdWhere | QryFlag.cDelWhere);
            m_fields["Name"]        = new CSqlFieldString("Name",           QryFlag.cSelRead  | QryFlag.cUpdWrite | QryFlag.cInsWrite);
            m_fields["Description"] = new CSqlFieldString("Description",    QryFlag.cSelRead  | QryFlag.cUpdWrite | QryFlag.cInsWrite);
        }
        public int PlatformID
        {
            get { return m_fields["PlatformID"].Integer; }
            set { m_fields["PlatformID"].Integer = value; }
        }
        public string Name
        {
            get { return m_fields["Name"].String; }
            set { m_fields["Name"].String = value; }
        }
        public string Description
        {
            get { return m_fields["Description"].String; }
            set { m_fields["Description"].String = value; }
        }
    }

    /// <summary>
    /// Database test class.
    /// Load database and test various queries such as SELECT or INSERT
    /// </summary>
    [TestClass]
    public class CDatabaseTest
    {
        [TestInitialize]
        public void Initialise()
        {
            CTest_Setup.InitLogger();
            CTest_Setup.RemoveDatabase();
            CTest_Setup.DatabaseSetup(); // This is pretty much our test for creating a database and populating it with data, if this fails all tests will fail
        }

        /// <summary>
        /// Open database
        /// </summary>
        [TestMethod]
        public void Test_CreateDB()
        {
            Assert.IsTrue(CSqlDB.Instance.IsOpen());
        }

        /// <summary>
        /// Insert row into the database
        /// </summary>
        [TestMethod]
        public void Test_InsertRow()
        {
            // Clear Platform table
            string qry = "DELETE FROM Platform";
            CSqlDB.Instance.Execute(qry);

            // Read from database (should fail)
            CTest_SelectQry qrySel = new CTest_SelectQry();
            qrySel.PlatformID = 1;
            Assert.AreEqual(qrySel.Select(), SQLiteErrorCode.NotFound);

            // Insert into database
            CTest_InsertQry qryIns = new CTest_InsertQry();
            qryIns.PlatformID  = 1;
            qryIns.Name        = "test";
            qryIns.Description = "PlatformID 1";
            Assert.AreEqual(qryIns.Insert(), SQLiteErrorCode.Ok);

            // Try again
            qrySel.PlatformID = 1;
            Assert.AreEqual(qrySel.Select(), SQLiteErrorCode.Ok);
            Assert.AreEqual(qrySel.PlatformID, 1);
            Assert.AreEqual(qrySel.Name, "test");
            Assert.AreEqual(qrySel.Description, "PlatformID 1");
            Assert.IsFalse(qrySel.Fetch()); // Only one row found
        }

        /// <summary>
        /// Update a row in the database
        /// </summary>
        [TestMethod]
        public void Test_UpdateRow()
        {
            // Get initial row
            CTest_SelectQry qrySel = new CTest_SelectQry();
            qrySel.PlatformID = 1;
            Assert.AreEqual(qrySel.Select(), SQLiteErrorCode.Ok);
            Assert.AreEqual(qrySel.PlatformID, 1);
            Assert.AreEqual(qrySel.Name, "test");
            Assert.AreEqual(qrySel.Description, "PlatformID 1");

            // Update database row
            CTest_UpdateQry qryUpd = new CTest_UpdateQry();
            qryUpd.PlatformID = 1;
            qryUpd.Description = "Updated PlatformID 1";
            Assert.AreEqual(qryUpd.Update(), SQLiteErrorCode.Ok);

            // Read again
            Assert.AreEqual(qrySel.Select(), SQLiteErrorCode.Ok);
            Assert.AreEqual(qrySel.PlatformID, 1);
            Assert.AreEqual(qrySel.Name, "test");
            Assert.AreEqual(qrySel.Description, "Updated PlatformID 1");
            Assert.IsFalse(qrySel.Fetch());
        }

        /// <summary>
        /// Delete row from the database
        /// </summary>
        [TestMethod]
        public void Test_DeleteRow()
        {
            // Get initial row
            CTest_SelectQry qrySel = new CTest_SelectQry();
            qrySel.PlatformID = 1;
            Assert.AreEqual(qrySel.Select(), SQLiteErrorCode.Ok);
            Assert.AreEqual(qrySel.PlatformID, 1);
            Assert.AreEqual(qrySel.Name, "test");
            Assert.AreEqual(qrySel.Description, "PlatformID 1");

            // Delete database row
            CTest_DeleteQry qryDel = new CTest_DeleteQry();
            qryDel.PlatformID = 1;
            Assert.AreEqual(qryDel.Delete(), SQLiteErrorCode.Ok);

            // Read again
            Assert.AreEqual(qrySel.Select(), SQLiteErrorCode.NotFound);
        }

        /// <summary>
        /// Select multiple columnns from the database
        /// </summary>
        [TestMethod]
        public void Test_ReadManyColumns()
        {
            CSqlDB.Instance.Execute("insert into Platform (PlatformID, Name, Description) VALUES (2, 'test 2', 'PlatformID 2')");
            CTest_SelectManyQry qry = new CTest_SelectManyQry();
            Assert.AreEqual(qry.Select(), SQLiteErrorCode.Ok);
            Assert.AreEqual(qry.PlatformID, 1);
            Assert.AreEqual(qry.Name, "test");
            Assert.AreEqual(qry.Description, "PlatformID 1");
            Assert.IsTrue(qry.Fetch());
            Assert.AreEqual(qry.PlatformID, 2);
            Assert.AreEqual(qry.Name, "test 2");
            Assert.AreEqual(qry.Description, "PlatformID 2");
        }

        /// <summary>
        /// Insert a row with a duplicate PK into the DB
        /// </summary>
        [TestMethod]
        public void Test_InsertDuplicateValue()
        {
            // Read from database
            CTest_SelectQry qrySel = new CTest_SelectQry();
            qrySel.PlatformID = 1;
            Assert.AreEqual(qrySel.Select(), SQLiteErrorCode.Ok);
            Assert.AreEqual(qrySel.PlatformID, 1);
            Assert.AreEqual(qrySel.Name, "test");
            Assert.AreEqual(qrySel.Description, "PlatformID 1");

            // Insert into database
            CTest_InsertQry qryIns = new CTest_InsertQry();
            qryIns.PlatformID = 1;
            qryIns.Name = "test";
            qryIns.Description = "PlatformID 1";
            Assert.AreNotEqual(qryIns.Insert(), SQLiteErrorCode.Ok);
        }

        /// <summary>
        /// Select row from database with a multi-param WHERE clause
        /// </summary>
        [TestMethod]
        public void Test_MultiSelectQuery_TwoParams()
        {
            CTest_SelectMultiParamQry qry = new CTest_SelectMultiParamQry();

            // Find data that does not exist (should fail)
            qry.Name = "test";
            qry.Description = "PlatformID 2";
            Assert.AreEqual(qry.Select(), SQLiteErrorCode.NotFound);

            // Find data that does exist
            qry.Name = "test";
            qry.Description = "PlatformID 1";
            Assert.AreEqual(qry.Select(), SQLiteErrorCode.Ok);
            Assert.AreEqual(qry.PlatformID, 1);
        }

        /// <summary>
        /// Select row from database with a multi-param WHERE clause
        /// Test when only one of the fields is used for SELECT
        /// </summary>
        [TestMethod]
        public void Test_MultiSelectQuery_OneParam()
        {
            CTest_SelectMultiParamQry qry = new CTest_SelectMultiParamQry();

            // Find data that does not exist (should fail)
            qry.Name = "test";
            qry.Description = "PlatformID 2";
            Assert.AreEqual(qry.Select(), SQLiteErrorCode.NotFound);

            // Find data that does exist
            qry.Description = "PlatformID 1";
            Assert.AreEqual(qry.Select(), SQLiteErrorCode.Ok);
            Assert.AreEqual(qry.PlatformID, 1);
        }

        /// <summary>
        /// Test a query class designed to insert, select and delete data
        /// </summary>
        [TestMethod]
        public void Test_MultiPurposeQuery()
        {
            CTest_MultiQry qry = new CTest_MultiQry();

            // Try to read (should fail)
            qry.PlatformID = 10;
            Assert.AreEqual(qry.Select(), SQLiteErrorCode.NotFound);
            qry.MakeFieldsNull();

            // Insert new row
            qry.PlatformID = 10;
            qry.Name = "test 10";
            qry.Description = "PlatformID 10";
            Assert.AreEqual(qry.Insert(), SQLiteErrorCode.Ok);
            qry.MakeFieldsNull();

            // Select new row
            qry.PlatformID = 10;
            Assert.AreEqual(qry.Select(), SQLiteErrorCode.Ok);
            Assert.AreEqual(qry.PlatformID, 10);
            Assert.AreEqual(qry.Name, "test 10");
            Assert.AreEqual(qry.Description, "PlatformID 10");
            qry.MakeFieldsNull();

            // Delete new row
            qry.PlatformID = 10;
            Assert.AreEqual(qry.Delete(), SQLiteErrorCode.Ok);
            qry.MakeFieldsNull();

            // Try to read again (should fail)
            qry.PlatformID = 10;
            Assert.AreEqual(qry.Select(), SQLiteErrorCode.NotFound);
        }

        /// <summary>
        /// Test the CDbAttribute class
        /// </summary>
        [TestMethod]
        public void Test_AttributeTable()
        {
            // Add a game

            Assert.AreEqual(CSqlDB.Instance.Execute("INSERT INTO Game (GameID, Identifier, Title, Alias, Launch) VALUES (1, 'testID', 'Test Game', 'test', 'test.exe')"), SQLiteErrorCode.Ok);

            CDbAttribute gameAttribute = new CDbAttribute("Game");
            gameAttribute.MasterID = 1;

            Assert.AreEqual(gameAttribute.SetStringValue("TEST_ATTRIBUTE", "TEST_VALUE", 0), SQLiteErrorCode.Ok); // Insert first attribute
            Assert.AreEqual(gameAttribute.GetStringValue("TEST_ATTRIBUTE", 0), "TEST_VALUE");  // Read
            Assert.AreEqual(gameAttribute.GetStringValue("TEST_ATTRIBUTE", 1), "");            // Read with different index
            Assert.AreEqual(gameAttribute.GetStringValue("NOT_EXIST"     , 0), "");            // Read different attribute name
            string[] multiple = gameAttribute.GetStringValues("TEST_ATTRIBUTE");               // Read all matching attribute name
            Assert.AreEqual(multiple.Length, 1);
            Assert.AreEqual(multiple[0], "TEST_VALUE");

            Assert.AreEqual(gameAttribute.SetStringValue("TEST_ATTRIBUTE", "TEST_VALUE_2", 1), SQLiteErrorCode.Ok); // Insert second attribute
            Assert.AreEqual(gameAttribute.GetStringValue("TEST_ATTRIBUTE", 0), "TEST_VALUE");   // Read
            Assert.AreEqual(gameAttribute.GetStringValue("TEST_ATTRIBUTE", 1), "TEST_VALUE_2"); // Read with different index
            multiple = gameAttribute.GetStringValues("TEST_ATTRIBUTE");                         // Read all matching attribute name
            Assert.AreEqual(multiple.Length, 2);
            Assert.AreEqual(multiple[0], "TEST_VALUE");
            Assert.AreEqual(multiple[1], "TEST_VALUE_2");
        }
    }
}
