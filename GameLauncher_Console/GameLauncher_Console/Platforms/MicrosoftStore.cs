using HtmlAgilityPack;
using Logger;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
//using System.Management.Automation;		// PowerShell Reference Assemblies
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
//using Windows.Management.Deployment;	// PackageManager [won't work unless this is a UWP or MSIX-packaged app?]
using static GameLauncher_Console.CJsonWrapper;
using static GameLauncher_Console.CRegScanner;

namespace GameLauncher_Console
{
    public class PlatformMicrosoftStore : IPlatform
	{
		// Microsoft Store/Xbox Game Pass
		// [experimental]
		public const CGameData.GamePlatform ENUM = CGameData.GamePlatform.MicrosoftStore;
		public const string NAME				= "Microsoft";
		public const string DESCRIPTION			= "Microsoft Store";
		public const string PROTOCOL			= "";
		//private const string LAUNCH_SUFFIX	= @":\\";

		CGameData.GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => NAME;

        string IPlatform.Description => DESCRIPTION;

        public static void Launch() => Process.Start("explorer.exe shell:AppsFolder\\Microsoft.WindowsStore_8wekyb3d8bbwe!App");
        //public static void Launch() => Process.Start("explorer.exe shell:AppsFolder\\Microsoft.XboxApp_8wekyb3d8bbwe!Microsoft.XboxApp");

        public void GetGames(List<RegistryGameData> gameDataList)
		{
			// TODO?: PowerShell Reference Assemblies method
			/*
			PowerShell ps = PowerShell.Create();
			ps.AddCommand("Find-Package");
			foreach (PSObject result in ps.Invoke())
			{
				CLogger.LogDebug("{0}", result);
				string strID = "";
				string strTitle = "";
				string strLaunch = "";
				string strIconPath = "";
				string strUninstall = "";
				string strAlias = "";
				string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.MicrosoftStore);
				try
				{
					strID = ""; //package.Id.Name;
					strTitle = strID;
					CLogger.LogDebug("- {0}", strTitle);
					strLaunch = strID;
					strIconPath = strID;
					strUninstall = strID;
					strAlias = strID;
					if (strAlias.Equals(strTitle))
						strAlias = "";
				}
				catch (Exception e)
				{
					CLogger.LogError(e);
				}
				if (!(string.IsNullOrEmpty(strLaunch)))
					gameDataList.Add(
						new RegistryGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
			}
			*/

			// TODO?: Internet method
			/*
			string xboxUsername = "";
			string msUsername = "";
			string msPassword = "";
			string url = $"https://account.xbox.com/en-us/profile?gamertag={xboxUsername}&activetab=main:mainTab2";
			HtmlWeb web = new HtmlWeb();
			web.UseCookies = true;
			web.PreRequest += (request) => {
				request.Credentials = new NetworkCredential(msUsername, msPassword);
				return true;
			};
#if DEBUG
			string tmpfile = string.Format("tmp_{0}.html", NAME);

			try
			{
				if (!File.Exists(tmpfile))
				{
					using (var client = new WebClient())
					{
						client.DownloadFile(url, tmpfile);
					}
				}
				HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument
				{
					OptionUseIdAttribute = true
				};
				doc.Load(tmpfile);
#else
				HtmlAgilityPack.HtmlDocument doc = web.Load(url);
				doc.OptionUseIdAttribute = true;
#endif
				// div#gamesList.All
				// <ul class="recentProgressList"><li style><xbox-title-item><div class="recentProgressInfoWrapper"><a class="recentProgressLinkWrapper" ...
				// data-m="{\"cN\":\"Microsoft Minesweeper\",\"bhvr\":281,\"tags\":{\"titleName\":\"Microsoft Minesweeper\",\"titleType\":\"XboxOne\"}}"

				//HtmlNode gameList = doc.DocumentNode.SelectNodes("recentProgressList");

				/*
#if DEBUG
				File.Delete(tmpfile);
#endif
				*/
			/*
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
			}
			*/

			// TODO?: PackageManager method
			/*
			if (System.Environment.OSVersion.Version.Major < 10 || System.Environment.OSVersion.Version.Build < 18917)
			{
				CLogger.LogInfo("Microsoft Store support requires Windows 10, build 18917 or newer.");
				return;
			}

			PackageManager packageManager = new PackageManager();

			try
			{
				var packages = packageManager.FindProvisionedPackages();

				if (packages.Count == 0)
				{
					Console.WriteLine("No packages were found.");
				}
				else
				{
					foreach (var package in packages)
					{
						DisplayPackageInfo(package);
						Console.WriteLine();
					}
				}

			}
			catch (UnauthorizedAccessException)
			{
				Console.WriteLine("packageManager.FindProvisionedPackages() failed because access was denied. This program must be run from an elevated command prompt.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("packageManager.FindProvisionedPackages() failed, error message: {0}", ex.Message);
				Console.WriteLine("Full Stacktrace: {0}", ex.ToString());
			}
			*/

			CLogger.LogDebug("----------------------");
		}

		public void GetGames(List<RegistryGameData> gameDataList, bool expensiveIcons)
		{
			GetGames(gameDataList);
		}

		/*
		private static void DisplayPackageInfo(Windows.ApplicationModel.Package package)
		{
			Console.WriteLine("Name: {0}", package.Id.Name);

			Console.WriteLine("FullName: {0}", package.Id.FullName);

			Console.WriteLine("Version: {0}.{1}.{2}.{3}", package.Id.Version.Major, package.Id.Version.Minor,
				package.Id.Version.Build, package.Id.Version.Revision);

			Console.WriteLine("Publisher: {0}", package.Id.Publisher);

			Console.WriteLine("PublisherId: {0}", package.Id.PublisherId);
		}
		*/
	}

	/*
	static class NativeMethods
	{
		[DllImport("shlwapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
		private static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);

		static internal string ExtractStringFromPRIFile(string pathToPRI, string resourceKey)
		{
			string sWin8ManifestString = string.Format("@{{{0}? {1}}}", pathToPRI, resourceKey);
			var outBuff = new StringBuilder(1024);
			int result = SHLoadIndirectString(sWin8ManifestString, outBuff, outBuff.Capacity, IntPtr.Zero);
			return outBuff.ToString();
		}

		public enum ActivateOptions
		{
			None = 0x00000000,  // No flags set
			DesignMode = 0x00000001,  // The application is being activated for design mode, and thus will not be able to
			// to create an immersive window. Window creation must be done by design tools which
			// load the necessary components by communicating with a designer-specified service on
			// the site chain established on the activation manager.  The splash screen normally
			// shown when an application is activated will also not appear.  Most activations
			// will not use this flag.
			NoErrorUI = 0x00000002,  // Do not show an error dialog if the app fails to activate.
			NoSplashScreen = 0x00000004,  // Do not show the splash screen when activating the app.
		}

		[ComImport, Guid("2e941141-7f97-4756-ba1d-9decde894a3d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IApplicationActivationManager
		{
			// Activates the specified immersive application for the "Launch" contract, passing the provided arguments
			// string into the application.  Callers can obtain the process Id of the application instance fulfilling this contract.
			IntPtr ActivateApplication([In] String appUserModelId, [In] String arguments, [In] ActivateOptions options, [Out] out UInt32 processId);
	*/
	//IntPtr ActivateForFile([In] String appUserModelId, [In] IntPtr /*IShellItemArray* */ itemArray, [In] String verb, [Out] out UInt32 processId);
	//IntPtr ActivateForProtocol([In] String appUserModelId, [In] IntPtr /* IShellItemArray* */itemArray, [Out] out UInt32 processId);
	/*
		}

		[ComImport, Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C")]//Application Activation Manager
		public class ApplicationActivationManager : IApplicationActivationManager
		{
	*/
	//[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)/*, PreserveSig*/]
	//public extern IntPtr ActivateApplication([In] String appUserModelId, [In] String arguments, [In] ActivateOptions options, [Out] out UInt32 processId);
	//[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	//public extern IntPtr ActivateForFile([In] String appUserModelId, [In] IntPtr /*IShellItemArray* */ itemArray, [In] String verb, [Out] out UInt32 processId);
	//[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	//public extern IntPtr ActivateForProtocol([In] String appUserModelId, [In] IntPtr /* IShellItemArray* */itemArray, [Out] out UInt32 processId);
	/*
		}
	}
	*/
}