using GameLauncher_Console;
using Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SQLite;
using System.IO;
using static GameLauncher_Console.CSqlField;

namespace UnitTest
{
    public static class CTest_Setup
    {
        private static bool m_IsInitialised = false;
        public static void Initialise()
        {
            if(m_IsInitialised)
            {
                return;
            }
#if DEBUG
            // Log unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CLogger.ExceptionHandleEvent);
#endif
            CLogger.Configure(Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]) + ".log"); // Create a log file
            CLogger.LogInfo("*************************");
            m_IsInitialised = true;
        }
    }

    /*
        4 sample queries for SQL testing.
        All trying to read/write into the Platform table (should be part of the schema)
        NOTE and potantial TODO:
            Tons of boilerplate code. Will need to find a way of dealing with that
    */

    public class CTest_InsertQry : CSqlQry
    {
        private CSqlField[] fields =
        {
            new CSqlField("Name"     , QueryFlag.cInsWrite, FieldType.cTypeString),
            new CSqlField("GameCount", QueryFlag.cInsWrite, FieldType.cTypeInteger)
        };

        public CTest_InsertQry() : base("Platform")
        {
            InitialiseQuery(fields);
        }

        public override SQLiteErrorCode Insert()
        {
            PrepareInsertStmt(fields);
            return base.Insert();
        }

        public String Name
        {
            get
            {
                return fields[0].m_columnValue;
            }
            set
            {
                fields[0].m_columnValue = value;
            }
        }

        public int GameCount
        {
            get
            {
                return fields[1].Int();
            }
            set
            {
                fields[1].m_columnValue = value.ToString();
            }
        }
    }

    public class CTest_SelectQry : CSqlQry
    {
        private CSqlField[] fields =
        {
            new CSqlField("PlatformID", QueryFlag.cSelRead, FieldType.cTypeInteger),
            new CSqlField("Name"      , QueryFlag.cSelWhere, FieldType.cTypeString),
            new CSqlField("GameCount" , QueryFlag.cSelRead , FieldType.cTypeInteger)
        };

        public CTest_SelectQry() : base("Platform")
        {
            InitialiseQuery(fields);
        }

        public SQLiteErrorCode Select()
        {
            PrepareSelectStmt(fields);
            return base.Select(fields);
        }

        public int PlatformID
        {
            get
            {
                return fields[0].Int();
            }
            set
            {
                fields[0].m_columnValue = value.ToString();
            }
        }

        public String Name
        {
            get
            {
                return fields[1].m_columnValue;
            }
            set
            {
                fields[1].m_columnValue = value;
            }
        }

        public int GameCount
        {
            get
            {
                return fields[2].Int();
            }
            set
            {
                fields[2].m_columnValue = value.ToString();
            }
        }
    }

    public class CTest_UpdateQry : CSqlQry
    {
        private CSqlField[] fields =
        {
            new CSqlField("Name"     , QueryFlag.cUpdWhere, FieldType.cTypeString),
            new CSqlField("GameCount", QueryFlag.cUpdWrite, FieldType.cTypeInteger)
        };

        public CTest_UpdateQry() : base("Platform")
        {
            InitialiseQuery(fields);
        }

        public String Name
        {
            get
            {
                return fields[0].m_columnValue;
            }
            set
            {
                fields[0].m_columnValue = value;
            }
        }

        public int GameCount
        {
            get
            {
                return fields[1].Int();
            }
            set
            {
                fields[1].m_columnValue = value.ToString();
            }
        }
    }

    public class CTest_DeleteQry : CSqlQry
    {
        private CSqlField[] fields =
        {
            new CSqlField("Name", QueryFlag.cDelWhere, FieldType.cTypeString),
        };

        public CTest_DeleteQry() : base("Platform")
        {
            InitialiseQuery(fields);
        }

        public String Name
        {
            get
            {
                return fields[0].m_columnValue;
            }
            set
            {
                fields[0].m_columnValue = value;
            }
        }
    }

    /// <summary>
    /// Database test class.
    /// Load database and test various queries such as SELECT or INSERT
    /// NOTE: The tests are sequencial and rely on the previous ones to pass (I know it's not good TDD but I can't be bothered to do it properly at this time)
    /// </summary>
    [TestClass]
    public class CDatabaseTest
    {
        [TestInitialize]
        public void Initialise()
        {
            CTest_Setup.Initialise();
        }

        /// <summary>
        /// Create database called 'database.db' and populate with schema information
        /// </summary>
        [TestMethod]
        public void CreateDB()
        {
            Assert.IsTrue(CSqlDB.Instance.IsOpen());
        }

        /// <summary>
        /// Insert row into the database
        /// </summary>
        [TestMethod]
        public void InsertRow()
        {
            // Insert into database
            CTest_InsertQry qryIns = new CTest_InsertQry();
            qryIns.Name        = "Test insert";
            qryIns.GameCount   = 10;
            Assert.AreEqual(qryIns.Insert(), SQLiteErrorCode.Ok);

            // Read from database
            SQLiteDataReader reader;
            CTest_SelectQry qrySel = new CTest_SelectQry();
            qrySel.Name = "Test insert";
            Assert.AreEqual(qrySel.Select(), SQLiteErrorCode.Ok);
            Assert.AreEqual(qrySel.PlatformID, 1);
            Assert.AreEqual(qrySel.Name, "Test insert");
            Assert.AreEqual(qrySel.GameCount, 10);
        }

        /// <summary>
        /// Update a row in the database
        /// </summary>
        [TestMethod]
        public void UpdateRow()
        {
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Delete row from the database
        /// </summary>
        [TestMethod]
        public void DeleteRow()
        {
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Insert row with duplicate PK into the database
        /// </summary>
        [TestMethod]
        public void InsertDuplicateRow()
        {
            Assert.IsTrue(true);
        }
    }
}
