using System;
using System.IO;
using System.Reflection;
using ConsoleUI.Type;
using Logger;

namespace GLC
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            // Log unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CLogger.ExceptionHandleEvent);
#endif
            CLogger.Configure(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]) + ".log")); // Create a log file
            CLogger.LogInfo("{0} version {1}",
                Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title,
                Assembly.GetEntryAssembly().GetName().Version.ToString());
            CLogger.LogInfo("*************************");

            // Main program start
            ConsoleRect rect = new ConsoleRect(0, 0, 100, 48);
            const int PAGE_COUNT = 1;

            CAppFrame window = new CAppFrame("GLC", rect, PAGE_COUNT);
            window.Initialise();
            window.WindowMain();
        }
    }
}
