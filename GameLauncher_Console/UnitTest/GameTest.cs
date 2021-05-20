using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlDB;

namespace UnitTest
{
    /// <summary>
    /// Test class for any game-related logic
    /// </summary>
    [TestClass]
    public class CGameTest
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
    }
}