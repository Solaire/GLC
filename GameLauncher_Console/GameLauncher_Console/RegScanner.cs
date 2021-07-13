using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static System.Environment;
using Microsoft.Win32;
using Logger;

namespace GameLauncher_Console
{
	/// <summary>
	/// Class used to scan the registry and retrieve the game data.
	/// </summary>
	public static class CRegScanner
	{
		// CONSTANTS

		// Steam (Valve)
		public const string STEAM_NAME				= "Steam";
		public const string STEAM_NAME_LONG			= "Steam";
		//private const string STEAM_UNREG			= "Steam"; // HKLM32 Uninstall

		// GOG Galaxy
		public const string GOG_NAME				= "GOG";
		public const string GOG_NAME_LONG			= "GOG Galaxy";
		public const string GOG_REG_GAMES			= @"SOFTWARE\WOW6432Node\GOG.com\Games";
		public const string GOG_REG_CLIENT			= @"SOFTWARE\WOW6432Node\GOG.com\GalaxyClient\paths";
		private const string GOG_CLIENT				= "client";
		private const string GOG_GAME_ID			= "GameID";
		private const string GOG_GAME_PATH			= "PATH";
		private const string GOG_GAME_NAME			= "GAMENAME";
		private const string GOG_GAME_LAUNCH		= "LAUNCHCOMMAND";
		private const string GOG_LAUNCH				= " /command=runGame /gameId=";
		private const string GOG_PATH				= " /path=";
		private const string GOG_GALAXY_EXE			= "GalaxyClient.exe";
		//private const string GOG_GALAXY_UNREG		= "{7258BA11-600C-430E-A759-27E2C691A335}_is1"; // HKLM32 Uninstall

		// Ubisoft Connect (formerly Uplay)
		public const string UPLAY_NAME				= "Ubisoft";
		public const string UPLAY_NAME_LONG			= "Ubisoft Connect";
		private const string UPLAY_INSTALL			= "Uplay Install ";
		private const string UPLAY_LAUNCH			= "uplay://launch/";
		//private const string UPLAY_UNREG				= "Uplay" // HKLM32 Uninstall
		//private const string UPLAY_REG				= @"SOFTWARE\WOW6432Node\Ubisoft\Launcher"; // HKLM32

		// Origin [soon to be EA Desktop]
		public const string ORIGIN_NAME				= "Origin"; //"EA"
		public const string ORIGIN_NAME_LONG		= "Origin"; //"EA Desktop";
		private const string ORIGIN_CONTENT			= @"\Origin\LocalContent";
		private const string ORIGIN_PATH			= "dipinstallpath=";
		/*
		private const string ORIGIN_GAMES			= "Origin Games";
		private const string EA_GAMES				= "EA Games";
		private const string ORIGIN_UNREG			= "Origin"; // HKLM32 Uninstall
		private const string ORIGIN_REG				= @"SOFTWARE\WOW6432Node\Origin"; // HKLM32
		*/

		// Bethesda.net Launcher
		public const string BETHESDA_NAME			= "Bethesda";
		public const string BETHESDA_NAME_LONG		= "Bethesda.net Launcher";
		private const string BETHESDA_NET			= "bethesda.net";
		private const string BETHESDA_PATH			= "Path";
		private const string BETHESDA_CREATION_KIT	= "Creation Kit";
		private const string BETHESDA_LAUNCH		= "bethesda://run/";
		private const string BETHESDA_PRODUCT_ID	= "ProductID";
		//private const string BETHESDA_UNREG			= "{3448917E-E4FE-4E30-9502-9FD52EABB6F5}_is1"; // HKLM32 Uninstall
		//private const string BETHESDA_REG			= @"SOFTWARE\WOW6432Node\Bethesda Softworks\Bethesda.net"; // HKLM32

		// Battle.net (Blizzard)
		public const string BATTLENET_NAME			= "Battlenet";
		public const string BATTLENET_NAME_LONG		= "Battle.net";
		//private const string BATTLE_NET_UNREG		= "Battle.net"; // HKLM32 Uninstall
		private const string BATTLE_NET_REG			= @"SOFTWARE\WOW6432Node\Blizzard Entertainment\Battle.net"; // HKLM32

		// Amazon Games
		//public const string AMAZON_NAME			= "Amazon";
		public const string AMAZON_NAME_LONG		= "Amazon";
		//private const string AMAZON_UNREG			= @"{4DD10B06-78A4-4E6F-AA39-25E9C38FA568}"; // HKCU64 Uninstall

		// Big Fish Games
		public const string BIGFISH_NAME			= "BigFish";
		public const string BIGFISH_NAME_LONG		= "Big Fish";
		private const string BIGFISH_GAME_FOLDER	= "BFG-";
		private const string BIGFISH_LAUNCH			= "LaunchGame.bfg";
		public const string BIGFISH_REG				= @"SOFTWARE\WOW6432Node\Big Fish Games\Client"; // HKLM32

		// Epic Games Launcher
		//public const string EPIC_NAME				= "Epic";
		public const string EPIC_NAME_LONG			= "Epic Games Launcher";
		//private const string EPIC_GAMES_UNREG			= "{A2FB1E1A-55D9-4511-A0BF-DEAD0493FBBC}"; // HKLM32 Uninstall
		//private const string EPIC_GAMES_UNREG			= "{A7BBC0A6-3DB0-41CC-BCED-DDFC5D4F3060}"; // HKLM32 Uninstall

		// Arc
		/*
		private const string ARC_NAME				= "Arc";
		private const string ARC_NAME_LONG			= "Arc";
		private const string ARC_UNREG				= "{CED8E25B-122A-4E80-B612-7F99B93284B3}"; // HKLM32 Uninstall
		*/

		// itch
		//public const string ITCH_NAME				= "itch";
		public const string ITCH_NAME_LONG			= "itch";
		/*
		private const string ITCH_DB				= @"\itch\db\butler.db";
		private const string ITCH_GAME_FOLDER		= "apps";
		private const string ITCH_METADATA			= ".itch\\receipt.json.gz";
		private const string ITCH_UNREG				= "itch"; // HKCU64 Uninstall
		*/

		// Paradox Launcher
		public const string PARADOX_NAME			= "Paradox";
		public const string PARADOX_NAME_LONG		= "Paradox Launcher";
		public const string PARADOX_REG				= @"SOFTWARE\WOW6432Node\Paradox Interactive\Paradox Launcher\LauncherPath"; // HKLM32
		public const string PARADOX_PATH			= "Path";

		// Plarium Play
		/*
		public const string PLARIUM_NAME			= "Plarium";
		public const string PLARIUM_NAME_LONG		= "Plarium Play";
		private const string PLARIUM_UNREG			= "{970D6975-3C2A-4AF9-B190-12AF8837331F}"; // HKLM32 Uninstall
		*/

		// Rockstar Games Launcher
		/*
		public const string ROCKSTAR_NAME			= "Rockstar";
		public const string ROCKSTAR_NAME_LONG		= "Rockstar Games Launcher";
		private const string ROCKSTAR_REG			= @"SOFTWARE\WOW6432Node\Rockstar Games\Launcher"; // HKLM32
		*/

		// Twitch [deprecated, now Amazon Games]
		/*
		public const string TWITCH_NAME				= "Twitch";
		public const string TWITCH_NAME_LONG		= "Twitch";
		private const string TWITCH_UNREG			= "{DEE70742-F4E9-44CA-B2B9-EE95DCF37295}"; // HKCU64 Uninstall
		*/

		// Wargaming.net Game Center
		/*
		public const string WARGAMING_NAME			= "Wargaming";
		public const string WARGAMING_NAME_LONG		= "Wargaming.net Game Center";
		private const string WARGAMING_UNREG		= "Wargaming.net Game Center"; // HKCU64 Uninstall
		*/

		// Indiegala
		public const string IG_NAME					= "Indiegala"; //"IGClient";
		public const string IG_NAME_LONG			= "Indiegala Client";
		//private const string IG_JSON_FILE			= @"\IGClient\storage\installed.json";
		//private const string IG_UNREG				= "6f4f090a-db12-53b6-ac44-9ecdb7703b4a"; // HKLM64 Uninstall
		public const string IG_REG					= @"SOFTWARE\6f4f090a-db12-53b6-ac44-9ecdb7703b4a"; // HKLM64

		// Xbox (Microsoft Store)
		/*
		public const string XBOX_NAME				= "Xbox";
		public const string XBOX_NAME_LONG			= "Xbox Game Pass";
		private const string XBOX_LAUNCH_SUFFIX		= @":\\";
		*/

		// Custom games
		//public const string CUSTOM_NAME				= "Custom";
		public const string CUSTOM_NAME_LONG		= "Custom";

		// generic constants
		public const string NODE64_REG				= @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
		public const string NODE32_REG				= @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
		public const string GAME_DISPLAY_NAME		= "DisplayName";
		public const string GAME_DISPLAY_ICON		= "DisplayIcon";
		public const string GAME_INSTALL_PATH		= "InstallPath";
		public const string GAME_INSTALL_LOCATION	= "InstallLocation";
		public const string GAME_UNINSTALL_STRING	= "UninstallString";
		public const string INSTALLSHIELD			= "_is1";

		/// <summary>
		/// Collect data from the registry
		/// </summary>
		public struct RegistryGameData
		{
			public string m_strID;
			public string m_strTitle;
			public string m_strLaunch;
			public string m_strIcon;
			public string m_strUninstall;
			public string m_strAlias;
			public bool m_bInstalled;
			public string m_strPlatform;

			public RegistryGameData(string strID, string strTitle, string strLaunch, string strIconPath, string strUninstall, string strAlias, bool bInstalled, string strPlatform)
			{
				m_strID			= strID;
				m_strTitle		= strTitle;
				m_strLaunch		= strLaunch;
				m_strIcon		= strIconPath;
				m_strUninstall	= strUninstall;
				m_strAlias		= strAlias;
				m_bInstalled	= bInstalled;
				m_strPlatform	= strPlatform;
			}
		}

		/// <summary>
		/// Scan the registry for games, add new games to memory and export into JSON document
		/// </summary>
		public static void ScanGames(bool bOnlyCustom, bool bExpensiveIcons, bool bFirstScan)
		{
			CGameData.CTempGameSet tempGameSet = new CGameData.CTempGameSet();
			CLogger.LogDebug("-----------------------");
			List<RegistryGameData> gameDataList = GetGames(bOnlyCustom, bExpensiveIcons);
			foreach (RegistryGameData data in gameDataList)
			{
				tempGameSet.InsertGame(data.m_strID, data.m_strTitle, data.m_strLaunch, data.m_strIcon, data.m_strUninstall, data.m_bInstalled, false, true, false, data.m_strAlias, data.m_strPlatform, 0f);
			}
			Console.Write(".");
			CLogger.LogInfo("Looking for {0} games...", CUSTOM_NAME_LONG.ToUpper());
			CGameFinder.ImportFromFolder(ref tempGameSet);
			CGameData.MergeGameSets(tempGameSet);
			if (bFirstScan)
				CGameData.SortGames(true, false, (bool)CConfig.GetConfigBool(CConfig.CFG_USEINST), true);
			CLogger.LogDebug("-----------------------");
			Console.WriteLine();
			CJsonWrapper.ExportGames(CGameData.GetPlatformGameList(CGameData.GamePlatform.All).ToList());
		}

		/// <summary>
		/// Scan the directory and try to find all installed games
		/// </summary>
		/// <returns>List of game data objects</returns>
		public static List<RegistryGameData> GetGames(bool bOnlyCustom, bool bExpensiveIcons)
		{
			List<RegistryGameData> gameDataList = new List<RegistryGameData>();

			if (!bOnlyCustom)
			{
				Console.Write(".");
				CLogger.LogInfo("Looking for {0} games...", AMAZON_NAME_LONG.ToUpper());
				CJsonWrapper.GetAmazonGames(gameDataList, bExpensiveIcons);
				Console.Write(".");
				CLogger.LogInfo("Looking for {0} games...", BATTLENET_NAME_LONG.ToUpper());
				GetBattlenetGames(gameDataList);
				Console.Write(".");
				CLogger.LogInfo("Looking for {0} games...", BETHESDA_NAME_LONG.ToUpper());
				GetBethesdaGames(gameDataList);
				Console.Write(".");
				CLogger.LogInfo("Looking for {0} games...", BIGFISH_NAME_LONG.ToUpper());
				GetBigFishGames(gameDataList);
				Console.Write(".");
				CLogger.LogInfo("Looking for {0} games...", EPIC_NAME_LONG.ToUpper());
				CJsonWrapper.GetEpicGames(gameDataList);
				Console.Write(".");
				CLogger.LogInfo("Looking for {0} games...", GOG_NAME_LONG.ToUpper());
				GetGogGames(gameDataList);
				Console.Write(".");
				CLogger.LogInfo("Looking for {0} games...", IG_NAME_LONG.ToUpper());
				CJsonWrapper.GetIGGames(gameDataList);
				Console.Write(".");
				CLogger.LogInfo("Looking for {0} games...", ITCH_NAME_LONG.ToUpper());
				CJsonWrapper.GetItchGames(gameDataList);
				Console.Write(".");
				CLogger.LogInfo("Looking for {0} games...", ORIGIN_NAME_LONG.ToUpper());
				GetOriginGames(gameDataList);
				Console.Write(".");
				CLogger.LogInfo("Looking for {0} games...", PARADOX_NAME_LONG.ToUpper());
				CJsonWrapper.GetParadoxGames(gameDataList);
				Console.Write(".");
				CLogger.LogInfo("Looking for {0} games...", STEAM_NAME_LONG.ToUpper());
				CJsonWrapper.GetSteamGames(gameDataList, bExpensiveIcons);
				Console.Write(".");
				CLogger.LogInfo("Looking for {0} games...", UPLAY_NAME_LONG.ToUpper());
				GetUplayGames(gameDataList, bExpensiveIcons);
				/*
				Console.Write(".");
				CLogger.LogInfo("Looking for {0} games...", XBOX_NAME_LONG.ToUpper());
				CStoreScanner.GetXboxGames(gameDataList);
				*/
			}

			return gameDataList;
		}

		/// <summary>
		/// Find installed GOG games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		private static void GetGogGames(List<RegistryGameData> gameDataList)
		{
			string strClientPath = "";

			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(GOG_REG_CLIENT, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM
			{
				if (key == null)
				{
					CLogger.LogInfo("{0} client not found in the registry.", GOG_NAME.ToUpper());
					return;
				}

				strClientPath = GetRegStrVal(key, GOG_CLIENT) + "\\" + GOG_GALAXY_EXE;
			}

			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(GOG_REG_GAMES, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM
			{
				if(key == null)
				{
					CLogger.LogInfo("{0} folder not found in the registry.", GOG_NAME.ToUpper());
					return;
				}

				CLogger.LogInfo("{0} {1} games found", key.GetSubKeyNames().Length, GOG_NAME.ToUpper());
				foreach(string strSubkeyName in key.GetSubKeyNames())
				{
					using (RegistryKey subkey = key.OpenSubKey(strSubkeyName, RegistryKeyPermissionCheck.ReadSubTree))
					{
						string strID = "";
						string strTitle = "";
						string strLaunch = "";
						string strIconPath = "";
						string strUninstall = "";
						string strAlias = "";
						string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.GOG);
						try
						{
							string strGameID = GetRegStrVal(subkey, GOG_GAME_ID);
							string strGamePath = GetRegStrVal(subkey, GOG_GAME_PATH);
							string strUninstKeyName = NODE32_REG + "\\" + strGameID + INSTALLSHIELD;

							strID = Path.GetFileName(subkey.Name);
							strTitle = GetRegStrVal(subkey, GOG_GAME_NAME);
							CLogger.LogDebug($"- {strTitle}");
							strLaunch = strClientPath + GOG_LAUNCH + strGameID + GOG_PATH + strGamePath;
							strIconPath = GetRegStrVal(subkey, GOG_GAME_LAUNCH).Trim(new char[] { ' ', '"' });
							using (RegistryKey uninstKey = Registry.LocalMachine.OpenSubKey(strUninstKeyName, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
							{
								if (uninstKey != null)
								{
									strUninstall = GetRegStrVal(uninstKey, GAME_UNINSTALL_STRING).Trim(new char[] { ' ', '"' });
									strAlias = GetAlias(Path.GetFileNameWithoutExtension(GetRegStrVal(uninstKey, GAME_INSTALL_LOCATION).Trim(new char[] { ' ', '\'', '"', '\\', '/' })));
								}
							}
							if (strAlias.Length > strTitle.Length)
								strAlias = GetAlias(strTitle);
							if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
								strAlias = "";
						}
						catch (Exception e)
                        {
							CLogger.LogError(e);
                        }
						if (!string.IsNullOrEmpty(strLaunch)) gameDataList.Add(
							new RegistryGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
					}
				}
				CLogger.LogDebug("-------------------");
			}
		}

		/// <summary>
		/// Find installed Ubisoft (formerly Uplay) games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		private static void GetUplayGames(List<RegistryGameData> gameDataList, bool expensiveIcons)
		{
			List<RegistryKey> keyList; //= new List<RegistryKey>();

			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE32_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if (key == null)
				{
					CLogger.LogInfo("{0} client not found in the registry.", UPLAY_NAME.ToUpper());
					return;
				}

				keyList = FindGameFolders(key, UPLAY_INSTALL);

				CLogger.LogInfo("{0} {1} games found", keyList.Count, UPLAY_NAME.ToUpper());
				foreach(var data in keyList)
				{
					string loc = GetRegStrVal(data, GAME_INSTALL_LOCATION);

					string strID = "";
					string strTitle = "";
					string strLaunch = "";
					string strIconPath = "";
					string strUninstall = "";
					string strAlias = "";
					string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.Uplay);
					try
					{
						strID = Path.GetFileName(data.Name);
						strTitle = GetRegStrVal(data, GAME_DISPLAY_NAME);
						CLogger.LogDebug($"- {strTitle}");
						strLaunch = UPLAY_LAUNCH + GetUplayGameID(Path.GetFileNameWithoutExtension(data.Name));
						strIconPath = GetRegStrVal(data, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
						if (string.IsNullOrEmpty(strIconPath) && expensiveIcons)
							strIconPath = CGameFinder.FindGameBinaryFile(loc, strTitle);
						strUninstall = GetRegStrVal(data, GAME_UNINSTALL_STRING); //.Trim(new char[] { ' ', '"' });
						strAlias = GetAlias(Path.GetFileNameWithoutExtension(loc.Trim(new char[] { ' ', '\'', '"', '\\', '/' })));
						if (strAlias.Length > strTitle.Length)
							strAlias = GetAlias(strTitle);
						if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
							strAlias = "";
					}
					catch (Exception e)
					{
						CLogger.LogError(e);
					}
					if (!string.IsNullOrEmpty(strLaunch)) gameDataList.Add(
						new RegistryGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
				}
				CLogger.LogDebug("-----------------------");
			}
		}

		/// <summary>
		/// Find installed EA Desktop/Origin games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		private static void GetOriginGames(List<RegistryGameData> gameDataList)
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
				CLogger.LogError(e, string.Format("ERROR: {0} directory read error: ", ORIGIN_NAME.ToUpper(), path));
			}

			CLogger.LogInfo("{0} {1} games found", dirs.Count, ORIGIN_NAME.ToUpper());
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
						CLogger.LogError(e, string.Format("ERROR: Malformed {0} file: {1}", ORIGIN_NAME.ToUpper(), file));
					}
				}

				if (!string.IsNullOrEmpty(install))
				{
					install = Uri.UnescapeDataString(install);

					using (RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE32_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
					{
						if (key != null)
						{
							keyList = FindGameKeys(key, install, GAME_INSTALL_LOCATION, new string[] { ORIGIN_NAME });
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

					if (!string.IsNullOrEmpty(strLaunch)) gameDataList.Add(
						new RegistryGameData(strID, strTitle, strLaunch, strLaunch, strUninstall, strAlias, true, strPlatform));
				}
			}
			CLogger.LogDebug("----------------------");
		}

		/// <summary>
		/// Find installed Bethesda.net games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		private static void GetBethesdaGames(List<RegistryGameData> gameDataList)
		{
			List<RegistryKey> keyList; //= new List<RegistryKey>();

			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE32_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if (key == null)
				{
					CLogger.LogInfo("{0} client not found in the registry.", BETHESDA_NAME.ToUpper());
					return;
				}

				keyList = FindGameKeys(key, BETHESDA_NET, BETHESDA_PATH, new string[] { BETHESDA_CREATION_KIT });

				CLogger.LogInfo("{0} {1} games found", keyList.Count, BETHESDA_NAME.ToUpper());
				foreach(var data in keyList)
				{
					string loc = GetRegStrVal(data, BETHESDA_PATH);

					string strID = "";
					string strTitle = "";
					string strLaunch = "";
					string strIconPath = "";
					string strUninstall = "";
					string strAlias = "";
					string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.Bethesda);
					try
					{
						strID = Path.GetFileName(data.Name);
						strTitle = GetRegStrVal(data, GAME_DISPLAY_NAME);
						CLogger.LogDebug($"- {strTitle}");
						strLaunch = BETHESDA_LAUNCH + GetRegStrVal(data, BETHESDA_PRODUCT_ID) + ".exe";
						strIconPath = GetRegStrVal(data, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
						if (string.IsNullOrEmpty(strIconPath))
							strIconPath = loc.Trim(new char[] { ' ', '"' }) + "\\" + strTitle + ".exe";
						strUninstall = GetRegStrVal(data, GAME_UNINSTALL_STRING); //.Trim(new char[] { ' ', '"' });
						strAlias = GetAlias(Path.GetFileNameWithoutExtension(loc.Trim(new char[] { ' ', '\'', '"', '\\', '/' })));
						if (strAlias.Length > strTitle.Length)
							strAlias = GetAlias(strTitle);
						if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
							strAlias = "";
					}
					catch (Exception e)
					{
						CLogger.LogError(e);
					}
					if (!string.IsNullOrEmpty(strLaunch)) gameDataList.Add(
						new RegistryGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
				}
				CLogger.LogDebug("------------------------");
			}
		}

		/// <summary>
		/// Find installed Battle.net games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		private static void GetBattlenetGames(List<RegistryGameData> gameDataList)
		{
			List<RegistryKey> keyList; //= new List<RegistryKey>();

			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE32_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if (key == null)
				{
					CLogger.LogInfo("{0} client not found in the registry.", BATTLENET_NAME.ToUpper());
					return;
				}

				keyList = FindGameKeys(key, BATTLE_NET_REG, GAME_UNINSTALL_STRING, new string[] { BATTLE_NET_REG });

				CLogger.LogInfo("{0} {1} games found", keyList.Count, BATTLENET_NAME.ToUpper());
				foreach(var data in keyList)
				{
					string strID = "";
					string strTitle = "";
					string strLaunch = "";
					//string strIconPath = "";
					string strUninstall = "";
					string strAlias = "";
					string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.Battlenet);
					try
					{
						strID = Path.GetFileName(data.Name);
						strTitle = GetRegStrVal(data, GAME_DISPLAY_NAME);
						CLogger.LogDebug($"- {strTitle}");
						strLaunch = GetRegStrVal(data, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
						strUninstall = GetRegStrVal(data, GAME_UNINSTALL_STRING); //.Trim(new char[] { ' ', '"' });
						strAlias = GetAlias(Path.GetFileNameWithoutExtension(GetRegStrVal(data, GAME_INSTALL_LOCATION).Trim(new char[] { ' ', '\'', '"', '\\', '/' })));
						if (strAlias.Length > strTitle.Length)
							strAlias = GetAlias(strTitle);
						if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
							strAlias = "";
					}
					catch (Exception e)
					{
						CLogger.LogError(e);
					}
					if (!string.IsNullOrEmpty(strLaunch)) gameDataList.Add(
						new RegistryGameData(strID, strTitle, strLaunch, strLaunch, strUninstall, strAlias, true, strPlatform));
				}
				CLogger.LogDebug("--------------------------");
			}
		}

		/// <summary>
		/// Find installed Big Fish games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		private static void GetBigFishGames(List<RegistryGameData> gameDataList)
		{
			List<RegistryKey> keyList; //= new List<RegistryKey>();

			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE32_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if (key == null)
				{
					CLogger.LogInfo("{0} client not found in the registry.", BIGFISH_NAME.ToUpper());
					return;
				}

				keyList = FindGameFolders(key, BIGFISH_GAME_FOLDER);

				CLogger.LogInfo("{0} {1} games found", keyList.Count, BIGFISH_NAME.ToUpper());
				foreach (var data in keyList)
				{
					string loc = GetRegStrVal(data, GAME_INSTALL_PATH);

					string strID = "";
					string strTitle = "";
					string strLaunch = "";
					string strIconPath = "";
					string strUninstall = "";
					string strAlias = "";
					string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.BigFish);
					try
					{
						strID = Path.GetFileName(data.Name);
						strTitle = GetRegStrVal(data, GAME_DISPLAY_NAME);
						CLogger.LogDebug($"- {strTitle}");
						strLaunch = loc + "\\" + BIGFISH_LAUNCH;
						strIconPath = GetRegStrVal(data, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
						strUninstall = GetRegStrVal(data, GAME_UNINSTALL_STRING).Trim(new char[] { ' ', '"' });
						strAlias = GetAlias(Path.GetFileNameWithoutExtension(loc.Trim(new char[] { ' ', '\'', '"', '\\', '/' })));
						if (strAlias.Length > strTitle.Length)
							strAlias = GetAlias(strTitle);
						if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
							strAlias = "";
					}
					catch (Exception e)
					{
						CLogger.LogError(e);
					}
					if (!string.IsNullOrEmpty(strLaunch)) gameDataList.Add(
						new RegistryGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
				}
				CLogger.LogDebug("------------------------");
			}
		}

		/*
		/// <summary>
		/// Find installed Amazon games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		private static void GetAmazonGames(List<RegistryGameData> gameDataList, bool expensiveIcons)
		{
			// moved to JsonWrapper.cs
		}
		*/

		/*
		/// <summary>
		/// Find installed Epic store games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		private static void GetEpicGames(List<RegistryGameData> gameDataList)
		{
			// moved to JsonWrapper.cs
		}
		*/

		/*
		/// <summary>
		/// Find installed Itch games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		private static void GetItchGames(List<RegistryGameData> gameDataList)
		{
			// moved to JsonWrapper.cs
		}
		*/

		/*
		/// <summary>
		/// Find installed Steam games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		private static void GetSteamGames(List<RegistryGameData> gameDataList)
		{
			// moved to JsonWrapper.cs
		}
		*/

		/// <summary>
		/// Find game keys in specified root
		/// Looks for a key-value pair inside the specified root.
		/// </summary>
		/// <param name="root">Root folder that will be scanned</param>
		/// <param name="strKey">The target key that should contain the target value</param>
		/// <param name="strValue">The target value in the key</param>
		/// <param name="ignore">Function will ignore these folders (used to ignore things like launchers)</param>
		/// <returns>List of game registry keys</returns>
		private static List<RegistryKey> FindGameKeys(RegistryKey root, string strKey, string strValue, string[] ignore)
		{
			LinkedList<RegistryKey> toCheck = new LinkedList<RegistryKey>();
			List<RegistryKey> gameKeys = new List<RegistryKey>();

			toCheck.AddLast(root);

			while(toCheck.Count > 0)
			{
				root = toCheck.First.Value;
				toCheck.RemoveFirst();

				if(root != null)
				{
					foreach(var name in root.GetValueNames())
					{
						if(root.GetValueKind(name) == RegistryValueKind.String && name == strValue)
						{
							if(((string)root.GetValue(name)).Contains(strKey))
							{
								gameKeys.Add(root);
								break;
							}
						}
					}

					foreach(var sub in root.GetSubKeyNames()) // Add subkeys to search list
					{
						if(!(sub.Equals("Microsoft"))) // Microsoft folder only contains system stuff and it doesn't need searching
						{
							foreach(var entry in ignore)
							{
								if(!(sub.Equals(entry.ToString())))
								{
									try
									{
										toCheck.AddLast(root.OpenSubKey(sub, RegistryKeyPermissionCheck.ReadSubTree));
									}
									catch (Exception e)
									{
										CLogger.LogError(e);
									}
								}
							}
						}
					}
				}
			}
			return gameKeys;
		}

		/// <summary>
		/// Find game folders in the registry.
		/// This method will look for and return folders that match the input string.
		/// </summary>
		/// <param name="root">Root directory that will be scanned</param>
		/// <param name="strFolder">Target game folder</param>
		/// <returns>List of Reg keys with game folders</returns>
		private static List<RegistryKey> FindGameFolders(RegistryKey root, string strFolder)
		{
			LinkedList<RegistryKey> toCheck = new LinkedList<RegistryKey>();
			List<RegistryKey> gameKeys = new List<RegistryKey>();

			toCheck.AddLast(root);

			while(toCheck.Count > 0)
			{
				root = toCheck.First.Value;
				toCheck.RemoveFirst();

				if(root != null)
				{
					foreach(var sub in root.GetSubKeyNames())
					{
						if(!(sub.Equals("Microsoft")) && sub.IndexOf(strFolder, StringComparison.OrdinalIgnoreCase) >= 0)
						{
							gameKeys.Add(root.OpenSubKey(sub, RegistryKeyPermissionCheck.ReadSubTree));
						}
					}
				}
			}
			return gameKeys;
		}

		/// <summary>
		/// Get a value from the registry if it exists
		/// </summary>
		/// <param name="key">The registry key</param>
		/// <param name="valName">The registry value name</param>
		/// <returns>the value's string data</returns>
		public static string GetRegStrVal(RegistryKey key, string valName)
		{
			try
			{
				object valData = key.GetValue(valName);
				if (valData != null)
					return valData.ToString();
			}
			catch (Exception e)
            {
				CLogger.LogError(e);
            }
			return String.Empty;
		}

		/// <summary>
		/// Simplify a string for use as a default alias
		/// </summary>
		/// <param name="title">The game's title</param>
		/// <returns>simplified string</returns>
		public static string GetAlias(string title)
		{
			string alias = title.ToLower();
			/*
			foreach (string prep in new List<string> { "for", "of", "to" })
			{
				if (alias.StartsWith(prep + " "))
					alias = alias.Substring(prep.Length + 1);
			}
			*/
			foreach (string art in CGameData.articles)
			{
				if (alias.StartsWith(art + " "))
					alias = alias.Substring(art.Length + 1);
			}
			alias = new string(alias.Where(c => !char.IsWhiteSpace(c) && !char.IsPunctuation(c) && !char.IsSymbol(c)).ToArray());
			return alias;
		}

		/// <summary>
		/// Scan the key name and extract the Steam game id
		/// </summary>
		/// <param name="key">The game string</param>
		/// <returns>Steam game ID as string</returns>
		public static string GetSteamGameID(string key)
		{
			return Path.GetFileNameWithoutExtension(key.Substring(key.LastIndexOf("_") + 1));
		}

		/// <summary>
		/// Scan the key name and extract the Uplay game id
		/// </summary>
		/// <param name="key">The game string</param>
		/// <returns>Uplay game ID as string</returns>
		private static string GetUplayGameID(string key)
		{
			int index = 0;
			for(int i = key.Length - 1; i > -1; i--)
			{
				if(char.IsDigit(key[i]))
				{
					index = i;
					continue;
				}
				break;
			}

			return key.Substring(index);
		}

		/*
		/// <summary>
		/// Scan the key name and extract the Amazon game id [no longer necessary after moving to SQLite method]
		/// </summary>
		/// <param name="key">The game string</param>
		/// <returns>Amazon game ID as string</returns>
		private static string GetAmazonGameID(string key)
		{
			return key.Substring(key.LastIndexOf(" -p ") + 4);
		}
		*/

		/// <summary>
		/// Scan the key name and extract the Itch game id
		/// </summary>
		/// <param name="key">The game string</param>
		/// <returns>itch game ID as string</returns>
		public static string GetItchGameID(string key)
		{
			return key.Substring(5);
		}
	}
}