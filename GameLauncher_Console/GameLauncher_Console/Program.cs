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
		private const string IMAGE_FOLDER_NAME = "CustomImages";
		private const string GAME_FOLDER_NAME = "CustomGames";

		[STAThread] // Requirement for Shell32.Shell COM object
		static void Main(string[] args)
		{
#if DEBUG
			// Log unhandled exceptions
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CLogger.ExceptionHandleEvent);
#endif
			string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			string imgFolder = Path.Combine(currentPath, IMAGE_FOLDER_NAME);
			string gameFolder = Path.Combine(currentPath, GAME_FOLDER_NAME);

			CLogger.Configure(Path.Combine(currentPath,
				Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]) + ".log")); // Create a log file
			if (!Directory.Exists(imgFolder)) Directory.CreateDirectory(imgFolder);
			if (!Directory.Exists(gameFolder)) Directory.CreateDirectory(gameFolder);

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
