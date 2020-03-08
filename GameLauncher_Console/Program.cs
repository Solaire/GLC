using System;

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
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Logger.CLogger.ExceptionHandleEvent);
#endif
			Logger.CLogger.Configure("GameLauncherConsole.log"); // Create a log file

			CDock gameDock = new CDock();
			gameDock.MainLoop();			
		}
	}
}
