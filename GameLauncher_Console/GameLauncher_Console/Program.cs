using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Logger;

namespace GameLauncher_Console
{
	/// <summary>
	/// Entry point class.
	/// Configure logging and go further into the app
	/// </summary>
	class Program
	{
		[STAThread] // Requirement for Shell32.Shell COM object
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

			// Allow for unicode characters, such as trademark symbol
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			CDock gameDock = new CDock();
			gameDock.MainLoop(args);
		}
	}
}
