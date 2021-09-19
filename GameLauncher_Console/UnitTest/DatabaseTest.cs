using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SQLite;
using SqlDB;
using static SqlDB.CSqlField;

namespace UnitTest
{
    /// <summary>
    /// Abstract class for any Platform-specific table
    /// Contains the column property getters/setters
    /// </summary>
    public abstract class CTest_PlatformQry : CSqlQry
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="table">The table name. This string should also include any JOIN tables</param>
        /// <param name="selectCondition">Select condition string for more complex queries. Use the field and value masks as templates (eg. & <= ?), those will be populated when query is constructed based on the CSqlRow's field insert order</param>
        /// <param name="selectExtraCondition">Any additional select conditions such as ORDER BY or GROUP BY</param>
        public CTest_PlatformQry(string table, string selectCondition, string selectExtraCondition) 
            : base(table, selectCondition, selectExtraCondition)
        {

        }
        public int PlatformID
        {
            get { return m_sqlRow["PlatformID"].Integer; }
            set { m_sqlRow["PlatformID"].Integer = value; }
        }
        public string Name
        {
            get { return m_sqlRow["Name"].String; }
            set { m_sqlRow["Name"].String = value; }
        }
        public string Description
        {
            get { return m_sqlRow["Description"].String; }
            set { m_sqlRow["Description"].String = value; }
        }
    }

    /// <summary>
    /// Test query for INSERT INTO ... statements
    /// </summary>
    public class CTest_InsertQry : CTest_PlatformQry
    {
        public CTest_InsertQry() 
            : base("Platform", "", "")
        {
            m_sqlRow["PlatformID"]  = new CSqlFieldInteger("PlatformID", QryFlag.cInsWrite);
            m_sqlRow["Name"]        = new CSqlFieldString("Name", QryFlag.cInsWrite);
            m_sqlRow["Description"] = new CSqlFieldString("Description", QryFlag.cInsWrite);
        }
    }

    /// <summary>
    /// Test query for SELECT ... WHERE
    /// </summary>
    public class CTest_SelectQry : CTest_PlatformQry
    {
        public CTest_SelectQry()
            : base("Platform", "", "")
        {
            m_sqlRow["PlatformID"] = new CSqlFieldInteger("PlatformID", QryFlag.cSelWhere);
            m_sqlRow["Name"]       = new CSqlFieldString("Name", QryFlag.cSelRead);
            m_sqlRow["Description"] = new CSqlFieldString("Description", QryFlag.cSelRead);
        }
    }

    /// <summary>
    /// Test query for UPDATE ... WHERE
    /// </summary>
    public class CTest_UpdateQry : CTest_PlatformQry
    {
        public CTest_UpdateQry()
            : base("Platform", "", "")
        {
            m_sqlRow["PlatformID"] = new CSqlFieldInteger("PlatformID", QryFlag.cUpdWhere);
            m_sqlRow["Description"] = new CSqlFieldString("Description", QryFlag.cUpdWrite);
        }
    }

    /// <summary>
    /// Test query for DELETE ... WHERE
    /// </summary>
    public class CTest_DeleteQry : CTest_PlatformQry
    {
        public CTest_DeleteQry()
            : base("Platform", "", "")
        {
            m_sqlRow["PlatformID"] = new CSqlFieldInteger("PlatformID", QryFlag.cDelWhere);
        }
    }

    /// <summary>
    /// Test query for selecting multiple columns (no WHERE clause)
    /// </summary>
    public class CTest_SelectManyQry : CTest_PlatformQry
    {
        public CTest_SelectManyQry()
            : base("Platform", "", "")
        {
            m_sqlRow["PlatformID"] = new CSqlFieldInteger("PlatformID", QryFlag.cSelRead);
            m_sqlRow["Name"] = new CSqlFieldString("Name", QryFlag.cSelRead);
            m_sqlRow["Description"] = new CSqlFieldString("Description", QryFlag.cSelRead);
        }
    }

    /// <summary>
    /// Test query for SELECT ... WHERE (multiple conditions)
    /// </summary>
    public class CTest_SelectMultiParamQry : CTest_PlatformQry
    {
        public CTest_SelectMultiParamQry()
            : base("Platform", "", "")
        {
            m_sqlRow["PlatformID"] = new CSqlFieldInteger("PlatformID", QryFlag.cSelRead);
            m_sqlRow["Name"] = new CSqlFieldString("Name", QryFlag.cSelWhere);
            m_sqlRow["Description"] = new CSqlFieldString("Description", QryFlag.cSelWhere);
        }
    }

    /// <summary>
    /// Test query for multiple query types (INSERT, SELECT AND DELETE)
    /// </summary>
    public class CTest_MultiQry : CTest_PlatformQry
    {
        public CTest_MultiQry()
            : base("Platform", "", "")
        {
            m_sqlRow["PlatformID"]  = new CSqlFieldInteger("PlatformID",    QryFlag.cInsWrite | QryFlag.cSelWhere | QryFlag.cUpdWhere | QryFlag.cDelWhere);
            m_sqlRow["Name"]        = new CSqlFieldString("Name",           QryFlag.cSelRead  | QryFlag.cUpdWrite | QryFlag.cInsWrite);
            m_sqlRow["Description"] = new CSqlFieldString("Description",    QryFlag.cSelRead  | QryFlag.cUpdWrite | QryFlag.cInsWrite);
        }
    }

    /// <summary>
    /// Advanced query test class.
    /// Select from Platform table where PlatformID is greater than x
    /// </summary>
    public class CTest_AdvancedGreaterThanQry : CTest_PlatformQry
    {
        public CTest_AdvancedGreaterThanQry()
            : base("Platform", "(& > ?)", "ORDER BY PlatformID DESC")
        {
            m_sqlRow["PlatformID"]  = new CSqlFieldInteger("PlatformID" , QryFlag.cSelRead | QryFlag.cSelWhere);
            m_sqlRow["Name"]        = new CSqlFieldString("Name"        , QryFlag.cSelRead );
            m_sqlRow["Description"] = new CSqlFieldString("Description" , QryFlag.cSelRead );
        }
    }

    /// <summary>
    /// Advanced query test class.
    /// Select from Platform table where PlatformID like x
    /// </summary>
    public class CTest_AdvancedLikeQry : CTest_PlatformQry
    {
        public CTest_AdvancedLikeQry()
            : base("Platform", "(& LIKE '%?%')", "")
        {
            m_sqlRow["Name"]        = new CSqlFieldString("Name",         QryFlag.cSelRead | QryFlag.cSelWhere);
            m_sqlRow["PlatformID"]  = new CSqlFieldInteger("PlatformID" , QryFlag.cSelRead );
            m_sqlRow["Description"] = new CSqlFieldString("Description" , QryFlag.cSelRead );
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
            CTestHelper.InitLogger();
            CTestHelper.RemoveDatabase();
            CTestHelper.DatabaseSetup(); // This is pretty much our test for creating a database and populating it with data, if this fails all tests will fail
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

        /// <summary>
        /// Test the 'SELECT ... WHERE x > y' query
        /// </summary>
        [TestMethod]
        public void Test_SelectGreaterThanQuery()
        {
            // Add some platforms
            Assert.AreEqual(CSqlDB.Instance.Execute("DELETE FROM Platform"), SQLiteErrorCode.Ok);
            Assert.AreEqual(CSqlDB.Instance.Execute(
                "INSERT INTO Platform (PlatformID, Name, Description) VALUES " + 
                "(1,  'test 1',  'PlatformID 1'), " +
                "(2,  'test 2',  'PlatformID 2'), " +
                "(3,  'test 3',  'PlatformID 3'), " + 
                "(5,  'test 5',  'PlatformID 5'), " +
                "(10, 'test 10', 'PlatformID 10')"), SQLiteErrorCode.Ok);

            CTest_AdvancedGreaterThanQry qry = new CTest_AdvancedGreaterThanQry();
            qry.PlatformID = 3;
            Assert.AreEqual(qry.Select(), SQLiteErrorCode.Ok);
            Assert.AreEqual(qry.PlatformID, 10);
            Assert.AreEqual(qry.Name, "test 10");
            Assert.AreEqual(qry.Description, "PlatformID 10");
            Assert.IsTrue(qry.Fetch());
            Assert.AreEqual(qry.PlatformID, 5);
            Assert.AreEqual(qry.Name, "test 5");
            Assert.AreEqual(qry.Description, "PlatformID 5");
        }

        /// <summary>
        /// Test the 'SELECT ... WHERE x LIKE '%y%' query
        /// </summary>
        [TestMethod]
        public void Test_SelectLikeQuery()
        {
            // Add some platforms
            Assert.AreEqual(CSqlDB.Instance.Execute("DELETE FROM Platform"), SQLiteErrorCode.Ok);
            Assert.AreEqual(CSqlDB.Instance.Execute(
                "INSERT INTO Platform (PlatformID, Name, Description) VALUES " +
                "(1,  'platform A', 'PlatformID 1'), " +
                "(2,  'platform B', 'PlatformID 2'), " +
                "(3,  'STEAM',      'PlatformID 3'), " +
                "(5,  'gog',        'PlatformID 5'), " +
                "(10, 'PLATFORM C', 'PlatformID 10')"), SQLiteErrorCode.Ok);

            CTest_AdvancedLikeQry qry = new CTest_AdvancedLikeQry();
            qry.Name = "platform"; // Should be case-insensitive
            Assert.AreEqual(qry.Select(), SQLiteErrorCode.Ok);
            Assert.AreEqual(qry.PlatformID, 1);
            Assert.AreEqual(qry.Name, "platform A");
            Assert.AreEqual(qry.Description, "PlatformID 1");
            Assert.IsTrue(qry.Fetch());
            Assert.AreEqual(qry.PlatformID, 2);
            Assert.AreEqual(qry.Name, "platform B");
            Assert.AreEqual(qry.Description, "PlatformID 2");
            Assert.IsTrue(qry.Fetch());
            Assert.AreEqual(qry.PlatformID, 10);
            Assert.AreEqual(qry.Name, "PLATFORM C");
            Assert.AreEqual(qry.Description, "PlatformID 10");
        }
    }
}
