using System;
using System.Diagnostics;

namespace GameHub_CS
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
			Logger.CLogger.Configure("GameHub.log"); // Create a log file

			// Allow for unicode characters, such as trademark symbol
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			CDock gameDock = new CDock();
			gameDock.MainLoop();			
		}
	}
}
