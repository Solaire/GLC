using Logger;
using System;
using System.IO;
using System.Reflection;

namespace DataConversionTool
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            // Log unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CLogger.ExceptionHandleEvent);
#endif
            CLogger.Configure(Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]) + ".log"); // Create a log file
            CLogger.LogInfo("{0} version {1}",
                Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title,
                Assembly.GetEntryAssembly().GetName().Version.ToString());
            CLogger.LogInfo("*************************");
        }


    }
}
