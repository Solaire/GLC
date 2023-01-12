using Logger;
using SqlDB;
using System.Data.SQLite;
using System.Reflection;

namespace core_2
{
    public abstract class CCore
    {
        private const string DATA_SCHEMA_PATH = "CreateDB.sql";

        public virtual bool Initialise()
        {
            if(!InitialiseLogger() || !InitialiseDatabase())
            {
                return false;
            }

            return DataManager.Initialise();
        }

        /// <summary>
        /// Initialise the logger
        /// </summary>
        /// <returns>True on initialise success</returns>
        private bool InitialiseLogger()
        {
#if DEBUG
            // Log unhandled exceptions in debug only
            System.AppDomain.CurrentDomain.UnhandledException += new System.UnhandledExceptionEventHandler(CLogger.ExceptionHandleEvent);
#endif // DEBUG

            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            CLogger.Configure(Path.Combine(currentPath,
                Path.GetFileNameWithoutExtension(System.Environment.GetCommandLineArgs()[0]) + ".log")); // Create a log file

            CLogger.LogInfo("{0} version {1}",
                Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title,
                Assembly.GetEntryAssembly().GetName().Version.ToString());
            CLogger.LogInfo("*************************");

            return true;
        }

        /// <summary>
        /// Initialise the database connection
        /// If database does not exist, create new and apply data schema
        /// </summary>
        /// <returns>True on initialise success</returns>
        private bool InitialiseDatabase()
        {
            SQLiteErrorCode err = CSqlDB.Instance.Open(true);
            if(err == SQLiteErrorCode.Ok)
            {
                // Success - no op.
            }
            else if(err == SQLiteErrorCode.Schema)
            {
                // New database, apply schema
                string script = "";
                try
                {
                    script = File.ReadAllText(DATA_SCHEMA_PATH);
                }
                catch(IOException e)
                {
                    CLogger.LogError(e);
                    err = SQLiteErrorCode.Unknown; // Use unknown for non-sql issues
                }
                if(script.Length > 0)
                {
                    err = CSqlDB.Instance.Conn.Execute(script);
                }
            }
            return err == SQLiteErrorCode.Ok;
        }
    }
}
