﻿// This application entry point is based on ASP.NET Core new project templates and is included
// as a starting point for app host configuration.
// This file may need updated according to the specific scenario of the application being upgraded.
// For more information on ASP.NET Core hosting, see https://docs.microsoft.com/aspnet/core/fundamentals/host/web-host

using Logger;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
using System;
//using System.Collections.Generic;
//using System.Diagnostics;
using System.IO;
//using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
//using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameLauncher_Console
{
	/// <summary>
	/// Entry point class.
	/// Configure logging and go further into the app
	/// </summary>
	public class Program
    {
		private const string IMAGE_FOLDER_NAME = "CustomImages";
		private const string GAME_FOLDER_NAME = "CustomGames";
		private static readonly IntPtr dpiAwarenessContextHandle = GetThreadDpiAwarenessContext();

		[STAThread] // Requirement for Shell32.Shell [and Microsoft.Web.WebView2] COM objects
		public static void Main(string[] args)
        {
#if DEBUG
			// Log unhandled exceptions
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CLogger.ExceptionHandleEvent);
#endif
			SetProcessDpiAwarenessContext(GetAwarenessFromDpiAwarenessContext(dpiAwarenessContextHandle) + 
				(int)DpiAwarenessContext.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
			Application.EnableVisualStyles();
			//Application.SetCompatibleTextRenderingDefault(false);

			string currentPath = Path.GetDirectoryName(AppContext.BaseDirectory);
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

			if (Console.IsOutputRedirected)
				Console.WriteLine("redir");
				
			// Allow for unicode characters, such as trademark symbol
			Console.OutputEncoding = System.Text.Encoding.UTF8;  // 'The handle is invalid.'

			CDock gameDock = new();
			gameDock.MainLoop(args);
		}

		enum ProcessDpiAwareness
		{
			PROCESS_DPI_UNAWARE = 0,
			PROCESS_SYSTEM_DPI_AWARE = 1,
			PROCESS_PER_MONITOR_DPI_AWARE = 2
		}

		enum DpiAwareness
		{
			DPI_AWARENESS_INVALID = -1,
			DPI_AWARENESS_UNAWARE = 0,
			DPI_AWARENESS_SYSTEM_AWARE = 1,
			DPI_AWARENESS_PER_MONITOR_AWARE = 2
		}

		enum DpiAwarenessContext
		{
			DPI_AWARENESS_CONTEXT_UNAWARE = -1,
			DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = -2,
			DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = -3,
			DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4,
			DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED = -5
		}

		[DllImport("user32.dll")]
		static extern IntPtr SetProcessDPIAware();

		[DllImport("user32.dll")]
		static extern IntPtr GetWindowDpiAwarenessContext(IntPtr hWnd);

		[DllImport("shcore.dll")]
		static extern int GetProcessDpiAwareness(IntPtr hProcess, out ProcessDpiAwareness value);

		[DllImport("user32.dll")]
		static extern IntPtr GetThreadDpiAwarenessContext();

		[DllImport("user32.dll")]
		static extern int GetAwarenessFromDpiAwarenessContext(IntPtr dpiContext);

		[DllImport("user32.dll")]
		static extern bool SetProcessDpiAwarenessContext(int value);
	}
}
