using System;

namespace GameLauncher_Console
{
	/// <summary>
	/// Entry point class.
	/// Configure logging and go further into the app
	/// </summary>
	class Program
	{
		static void Main(string[] args)
		{
			// Log unhandled exceptions
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Logger.CLogger.ExceptionHandleEvent);
			Logger.CLogger.Configure("GameLauncherConsole.log"); // Create a log file

			//CConsole console = new CConsole(3, 15, CConsole.ConsoleState.cState_Navigate);
			//console.ConsoleStart();
			CJsonWrapper json = new CJsonWrapper();
		}
	}
}
