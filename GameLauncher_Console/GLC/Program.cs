/*
using System;
using System.IO;
using System.Reflection;
using Logger;
*/
using GLC_Structs;

namespace GLC
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
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

            // Allow for unicode characters, such as trademark symbol
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            */


            /*
                TODOs:
                    Figure out window resize problem (adjust the buffer size and redraw every second?)
                    Handle keyboard input
                    Implement pages and panels
            */

            // Main program start
            ConsoleRect rect;
            rect.x = 0;
            rect.y = 0;
            rect.width = 100;
            rect.height = 48;

            ColourPair pair;
            pair.background = System.ConsoleColor.Black;
            pair.foreground = System.ConsoleColor.White;

            CWindow window = new CWindow("GLC 2.0 test", rect, pair);
            window.Initialise();
            window.WindowMain();

            // TODO: Remove
            // Wait for user input (keep console alive)
            System.Console.ReadLine();
        }
    }
}
