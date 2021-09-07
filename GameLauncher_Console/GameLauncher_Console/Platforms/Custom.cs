using Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static GameLauncher_Console.CRegScanner;

namespace GameLauncher_Console
{
	// .lnk and .exe files in .\CustomGames
	// [maybe this shouldn't derive from IPlatform interface?]
	public class PlatformCustom : IPlatform
    {
		public const CGameData.GamePlatform ENUM = CGameData.GamePlatform.Custom;
		public const string NAME				= "Custom";
		public const string DESCRIPTION			= "Custom";
		public const string PROTOCOL			= "";
		private const string CUSTOM_PLATFORM	= "Custom";
		private const string GAME_FOLDER_NAME	= "CustomGames";

		CGameData.GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => NAME;

        string IPlatform.Description => DESCRIPTION;

        public static void Launch() => throw new NotImplementedException();

		public static void InstallGame(CGameData.CGame game) => throw new NotImplementedException();

		public void GetGames(List<RegistryGameData> gameDataList) => throw new NotImplementedException();

		public void GetGames(List<RegistryGameData> gameDataList, bool expensiveIcons) => throw new NotImplementedException();

		public void GetGames(ref CGameData.CTempGameSet tempGameSet)
        {
			FindCustomLinkFiles(ref tempGameSet);
			FindCustomBinaries(ref tempGameSet);
		}

		/// <summary>
		/// Search the "CustomGames" folder for file shortcuts (.lnk) to import.
		/// </summary>
		/// http://www.saunalahti.fi/janij/blog/2006-12.html#d6d9c7ee-82f9-4781-8594-152efecddae2
		private static void FindCustomLinkFiles(ref CGameData.CTempGameSet tempGameSet)
		{
			List<string> fileList = Directory.EnumerateFiles(Path.Combine(CDock.currentPath, GAME_FOLDER_NAME), "*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".lnk")).ToList();

			foreach (string file in fileList)
			{
				string strPathOnly = Path.GetDirectoryName(file);
				strPathOnly = Path.GetFullPath(strPathOnly);
				string strFilenameOnly = Path.GetFileName(file);

				Shell32.Shell shell = new Shell32.Shell();
				Shell32.Folder folder = shell.NameSpace(strPathOnly);
				Shell32.FolderItem folderItem = folder.ParseName(strFilenameOnly);
				if (folderItem != null)
				{
					Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
					string strID = Path.GetFileNameWithoutExtension(file);
					string strTitle = strID;
					CLogger.LogDebug($"- {strTitle}");
					string strLaunch = link.Path;
					string strUninstall = "";  // N/A
					string strAlias = GetAlias(strTitle);
					if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
						strAlias = "";
					tempGameSet.InsertGame(strID, strTitle, strLaunch, strLaunch, strUninstall, true, false, true, false, strAlias, CUSTOM_PLATFORM, 0f);
				}
			}
		}

		/// <summary>
		/// Search the "CustomGames" folder for binaries (.exe) files to import
		/// </summary>
		private static void FindCustomBinaries(ref CGameData.CTempGameSet tempGameSet)
		{
			List<string> fileList = Directory.EnumerateFiles(Path.Combine(CDock.currentPath, GAME_FOLDER_NAME), "*", SearchOption.AllDirectories).Where(s => s.EndsWith(".exe")).ToList();

			// Big Fish Games may use .bfg for executables
			//fileList.AddRange(Directory.EnumerateFiles(Path.Combine(CDock.currentPath, GAME_FOLDER_NAME), "*", SearchOption.AllDirectories).Where(s => s.EndsWith(".bfg")).ToList());

			foreach (string file in fileList)
			{
				string strID = Path.GetFileNameWithoutExtension(file);
				string strTitle = strID;
				CLogger.LogDebug($"- {strTitle}");
				string strLaunch = Path.GetFullPath(file);
				string strUninstall = ""; // N/A
				string strAlias = GetAlias(strTitle);
				if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
					strAlias = "";
				tempGameSet.InsertGame(strID, strTitle, strLaunch, strLaunch, strUninstall, true, false, true, false, strAlias, CUSTOM_PLATFORM, 0f);
			}
		}
	}
}