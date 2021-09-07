using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static GameLauncher_Console.CRegScanner;
using static System.Environment;

namespace GameLauncher_Console
{
	// Origin [soon to be EA Desktop]
	// [installed games only]
	public class PlatformOrigin : IPlatform
	{
		public const CGameData.GamePlatform ENUM = CGameData.GamePlatform.Origin;
		public const string NAME				= "Origin";		//"EA"
		public const string DESCRIPTION			= "Origin";		//"EA Desktop";
		public const string PROTOCOL			= "origin://";	//"eadm://" was added by EA Desktop, but "origin://" and "origin2://" still work with it (for now)
		private const string ORIGIN_CONTENT		= @"\Origin\LocalContent";
		private const string ORIGIN_PATH		= "dipinstallpath=";
		/*
		private const string ORIGIN_GAMES		= "Origin Games";
		private const string EA_GAMES			= "EA Games";
		private const string ORIGIN_UNREG		= "Origin"; // HKLM32 Uninstall
		private const string ORIGIN_REG			= @"SOFTWARE\WOW6432Node\Origin"; // HKLM32
		*/

		CGameData.GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => NAME;

        string IPlatform.Description => DESCRIPTION;

        public static void Launch() => Process.Start(PROTOCOL);

        public void GetGames(List<RegistryGameData> gameDataList)
		{
			List<RegistryKey> keyList = new List<RegistryKey>();
			List<string> dirs = new List<string>();
			string path = "";
			try
			{
				path = GetFolderPath(SpecialFolder.CommonApplicationData) + ORIGIN_CONTENT;
				if (Directory.Exists(path))
				{
					dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly));
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e, string.Format("{0} directory read error: {1}", NAME.ToUpper(), path));
			}

			CLogger.LogInfo("{0} {1} games found", dirs.Count, NAME.ToUpper());
			foreach (string dir in dirs)
			{
				string[] files = { };
				string install = "";

				string strID = Path.GetFileName(dir);
				string strTitle = strID;
				string strLaunch = "";
				//string strIconPath = "";
				string strUninstall = "";
				string strAlias = "";
				string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.Origin);

				try
				{
					files = Directory.GetFiles(dir, "*.mfst", SearchOption.TopDirectoryOnly);
				}
				catch (Exception e)
				{
					CLogger.LogError(e);
				}

				foreach (string file in files)
				{
					try
					{
						string strDocumentData = File.ReadAllText(file);
						string[] subs = strDocumentData.Split('&');
						foreach (string sub in subs)
						{
							if (sub.StartsWith(ORIGIN_PATH))
								install = sub.Substring(15);
						}
					}
					catch (Exception e)
					{
						CLogger.LogError(e, string.Format("Malformed {0} file: {1}", NAME.ToUpper(), file));
					}
				}

				if (!string.IsNullOrEmpty(install))
				{
					install = Uri.UnescapeDataString(install);

					using (RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE32_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
					{
						if (key != null)
						{
							keyList = FindGameKeys(key, install, GAME_INSTALL_LOCATION, new string[] { NAME });
							foreach (var data in keyList)
							{
								strTitle = GetRegStrVal(data, GAME_DISPLAY_NAME);
								strLaunch = GetRegStrVal(data, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
								strUninstall = GetRegStrVal(data, GAME_UNINSTALL_STRING); //.Trim(new char[] { ' ', '"' });
							}
						}
					}

					CLogger.LogDebug($"- {strTitle}");
					if (string.IsNullOrEmpty(strLaunch))
						strLaunch = CGameFinder.FindGameBinaryFile(install, strTitle);
					strAlias = GetAlias(Path.GetFileNameWithoutExtension(install));
					if (strAlias.Length > strTitle.Length)
						strAlias = GetAlias(strTitle);
					if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
						strAlias = "";

					if (!(string.IsNullOrEmpty(strLaunch)))
						gameDataList.Add(
							new RegistryGameData(strID, strTitle, strLaunch, strLaunch, strUninstall, strAlias, true, strPlatform));
				}
			}
			CLogger.LogDebug("----------------------");
		}

		public void GetGames(List<RegistryGameData> gameDataList, bool expensiveIcons)
		{
			GetGames(gameDataList);
		}
	}
}