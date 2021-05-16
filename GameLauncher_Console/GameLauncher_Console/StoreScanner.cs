using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
//using System.Management.Automation;
//using Windows.Management.Deployment;
using Logger;

/* THIS FILE HAS BEEN REMOVED FROM THE .CSPROJ, AS IT'S NOT LIKELY TO BE USEFUL.
 * Not sure how to determine whether a given UWP app is a *game* or not
 */

namespace GameLauncher_Console
{
	/// <summary>
	/// Class used to scan Microsoft Store apps and retrieve the game data.
	/// </summary>
	public static class CStoreScanner
	{
		/// <summary>
		/// Find installed Amazon games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		public static void GetXboxGames(List<CRegScanner.RegistryGameData> gameDataList)
		{
            // PackageManager isn't going to work unless this is a UWP or at least an MSIX-packaged app
            //var packages = new PackageManager().FindPackagesForUser("");

            /*
            PowerShell ps = PowerShell.Create();
            ps.AddCommand("Find-Package");
            //...
            */

            //foreach (var package in packages)
            //{
                string strID = "";
				string strTitle = "";
				string strLaunch = "";
				string strIconPath = "";
				string strUninstall = "";
				string strAlias = "";
                string strPlatform = ""; //CGameData.GetPlatformString(CGameData.GamePlatform.Xbox);
				try
				{
                    strID = ""; //package.Id.Name;
					strTitle = strID;
					CLogger.LogDebug("* {0}", strTitle);
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
				if (!string.IsNullOrEmpty(strLaunch)) gameDataList.Add(
					new CRegScanner.RegistryGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, strPlatform));
			//}
			CLogger.LogDebug("----------------------");
		}
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