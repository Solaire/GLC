using Logger;
using SqlDB;
using System.IO;
using System.Reflection;

namespace core
{
    /// <summary>
    /// Main entry point into the core library
    /// Should be inherited by the implementing app class
    /// </summary>
    public abstract class CApplicationCore
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        protected CApplicationCore()
        {
            Initialise();
        }

        /// <summary>
        /// Initialise the library's core functions
        /// </summary>
        /// <returns>True on initialise success</returns>
        protected bool Initialise()
        {
            if(!InitialiseLogger())
            {
                return false;
            }
            if(!InitialiseDatabase())
            {
                CLogger.LogWarn("Error initialising the database: {0}", CSqlDB.Instance.Conn.LastError);
                return false;
            }
            if(InitialiseExtensions())
            {
                CLogger.LogWarn("Errors initialising plug-in extensions");
                return false;
            }
            if(InitialisePlatforms())
            {
                CLogger.LogWarn("Errors initialising platforms");
                return false;
            }
            return true;
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
        /// </summary>
        /// <returns>True on initialise success</returns>
        protected abstract bool InitialiseDatabase();

        /// <summary>
        /// Detect and load any plug-in extensions
        /// </summary>
        /// <returns>True on initialise success</returns>
        private bool InitialiseExtensions()
        {
            return true;
        }

        /// <summary>
        /// Initialise the platforms using the database and plug-ins
        /// If a platform exists in the database, but the plug-in is missing, deactivate the platform.
        /// If a platform does not exist and the plug-in is loaded, create the platform object and add to database
        /// </summary>
        /// <returns>True on initialise success</returns>
        private bool InitialisePlatforms()
        {
            return true;
        }
    }
}
