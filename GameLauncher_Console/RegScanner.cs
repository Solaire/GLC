using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GameLauncher_Console
{
	/// <summary>
	/// Class used to scan the registry and retrieve the game data.
	/// </summary>
	public static class CRegScanner
	{
		// Steam constants
		private const string STEAM_NAME			= "STEAM";
		private const string STEAM_REG			= @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
		private const string STEAM_GAME_FOLDER	= "Steam App";
		private const string STEAM_LAUNCH		= "steam://rungameid/";

		// GOG constants
		private const string GOG_NAME			= "GOG";
		private const string GOG_REG_GAMES		= @"SOFTWARE\Wow6432Node\GOG.com\Games";
		private const string GOG_REG_CLIENT		= @"SOFTWARE\WOW6432Node\GOG.com\GalaxyClient\paths";
		private const string GOG_CLIENT			= "client";
		private const string GOG_GAME_ID		= "GameID";
		private const string GOG_GAME_PATH		= "PATH";
		private const string GOG_GAME_NAME		= "GAMENAME";
		private const string GOG_GAME_LAUNCH	= "LAUNCHCOMMAND";
		private const string GOG_LAUNCH			= " /command=runGame /gameId=";
		private const string GOG_PATH			= " /path=";
		private const string GOG_GALAXY_EXE		= "\\GalaxyClient.exe";

		// Uplay constants
		private const string UPLAY_NAME			= "UPLAY";
		private const string UPLAY_INSTALL		= "Uplay Install";
		private const string UPLAY_LAUNCH		= "uplay://launch/";

		// Origin constants
		private const string ORIGIN_NAME		= "Origin";
		private const string ORIGIN_GAMES		= "Origin Games";

		// Bethesda constants
		private const string BETHESDA_NAME			= "bethesda";
		private const string BETHESDA_NET			= "bethesda.net";
		private const string BETHESDA_PATH			= "Path";
		private const string BETHESDA_CREATION_KIT	= "Creation Kit";
		private const string BETHESTA_LAUNCH		= "bethesda://run/";
		private const string BETHESTA_PRODUCT_ID	= "ProductID";

		// Battlenet constants
		private const string BATTLENET_NAME				= "Battlenet";
		private const string BATTLE_NET					= "Battle.net";
		private const string BATTLENET_UNINSTALL_STRING = "UninstallString";

		// Epic store constants
		private const string EPIC_NAME				= "Epic";
		private const string EPIC_GAMES_LAUNCHER	= "Epic Games Launcher";
		private const string EPIC_UNREAL_ENGINE		= "Unreal Engine";
		private const string EPIC_LAUNCHER			= "Launcher";
		private const string EPIC_DIRECT_X_REDIST	= "DirectXRedist";

		// Generic constants
		private const string NODE64_REG				= @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
		private const string GAME_DISPLAY_NAME		= "DisplayName";
		private const string GAME_DISPLAY_ICON		= "DisplayIcon";
		private const string GAME_INSTALL_LOCATION	= "InstallLocation";

		/// <summary>
		/// Collect data from the registry
		/// </summary>
		public struct RegistryGameData
		{
			public string m_strTitle;
			public string m_strLaunch;
			public string m_strIcon;
			public string m_strPlatform;

			public RegistryGameData(string strTitle, string strLaunch, string strIcon, string strPlatform)
			{
				m_strTitle		= strTitle;
				m_strLaunch		= strLaunch;
				m_strIcon		= strIcon;
				m_strPlatform	= strPlatform;
			}
		}

		/// <summary>
		/// Scan the registry for games, add new games to memory and export into JSON document
		/// </summary>
		public static void ScanGames()
		{
			CGameData.CTempGameSet tempGameSet = new CGameData.CTempGameSet();
			List<CRegScanner.RegistryGameData> gameDataList = CRegScanner.GetGames();
			foreach(CRegScanner.RegistryGameData data in gameDataList)
			{
				tempGameSet.InsertGame(data.m_strTitle, data.m_strLaunch, false, data.m_strPlatform);
			}
			CGameFinder.ImportFromFolder(ref tempGameSet);
			CGameData.MergeGameSets(tempGameSet);
			CJsonWrapper.Export(CGameData.GetPlatformGameList(CGameData.GamePlatform.All).ToList());
		}

		/// <summary>
		/// Scan the directory and try to find all installed games
		/// </summary>
		/// <returns>List of game data objects</returns>
		public static List<RegistryGameData> GetGames()
		{
			List<RegistryGameData> gameDataList = new List<RegistryGameData>();

			GetSteamGames(gameDataList);
			GetGogGames(gameDataList);
			GetUplayGames(gameDataList);
			GetEpicGames(gameDataList);
			GetBethesdaGames(gameDataList);
			GetBattlenetGames(gameDataList);
			GetOriginGames(gameDataList);

			return gameDataList;
		}

		/// <summary>
		/// Find installed Steam games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		private static void GetSteamGames(List<RegistryGameData> gameDataList)
		{
			List<RegistryKey> keyList = new List<RegistryKey>();

			RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
			using(RegistryKey key = baseKey.OpenSubKey(STEAM_REG, RegistryKeyPermissionCheck.ReadSubTree))
			{
				keyList = FindGameFolders(key, STEAM_GAME_FOLDER);

				foreach(var data in keyList)
				{
					string strTitle  = data.GetValue(GAME_DISPLAY_NAME).ToString();
					string strLaunch = STEAM_LAUNCH + Path.GetFileNameWithoutExtension(data.Name).Substring(10);
					string strIcon	 = CGameFinder.FindGameBinaryFile(data.GetValue(GAME_INSTALL_LOCATION).ToString(), data.GetValue(GAME_DISPLAY_NAME).ToString());
					gameDataList.Add(new RegistryGameData(strTitle, strLaunch, strIcon, "Steam"));
				}
			}
		}

		/// <summary>
		/// Find installed GOG games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		private static void GetGogGames(List<RegistryGameData> gameDataList)
		{
			string strClientPath = "";

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(GOG_REG_CLIENT))
			{
				if(key == null) // GOG not found
					return;

				strClientPath = key.GetValue(GOG_CLIENT).ToString() + GOG_GALAXY_EXE;
			}

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(GOG_REG_GAMES))
			{
				foreach(string strSubkeyName in key.GetSubKeyNames())
				{
					using(RegistryKey subkey = key.OpenSubKey(strSubkeyName))
					{
						string strGameID	= subkey.GetValue(GOG_GAME_ID).ToString();
						string strGamePath	= subkey.GetValue(GOG_GAME_PATH).ToString();

						string strTitle		= subkey.GetValue(GOG_GAME_NAME).ToString();
						string strLaunch	= strClientPath + GOG_LAUNCH + strGameID + GOG_PATH + strGamePath;
						string strIcon		= subkey.GetValue(GOG_GAME_LAUNCH).ToString().Trim(new char[] { ' ', '\'', '"'});

						gameDataList.Add(new RegistryGameData(strTitle, strLaunch, strIcon, GOG_NAME));
					}
				}
			}
		}

		/// <summary>
		/// Find installed Uplay games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		private static void GetUplayGames(List<RegistryGameData> gameDataList)
		{
			List<RegistryKey> keyList = new List<RegistryKey>();

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE64_REG))
			{
				keyList = FindGameFolders(key, UPLAY_INSTALL);

				foreach(var data in keyList)
				{
					string strTitle		= data.GetValue(GAME_DISPLAY_NAME).ToString();
					string strLaunch	= UPLAY_LAUNCH + Path.GetFileNameWithoutExtension(data.Name).Substring(10) + "/0";
					string strIcon		= CGameFinder.FindGameBinaryFile(data.GetValue(GAME_INSTALL_LOCATION).ToString(), data.GetValue(GAME_DISPLAY_NAME).ToString());

					gameDataList.Add(new RegistryGameData(strTitle, strLaunch, strIcon, UPLAY_NAME));
				}
			}
		}

		/// <summary>
		/// Find installed Origin games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		private static void GetOriginGames(List<RegistryGameData> gameDataList)
		{
			List<RegistryKey> keyList = new List<RegistryKey>();

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE64_REG))
			{
				keyList = FindGameKeys(key, ORIGIN_GAMES, GAME_INSTALL_LOCATION, ORIGIN_NAME);

				foreach(var data in keyList)
				{
					string strTitle  = data.GetValue(GAME_DISPLAY_NAME).ToString();
					string strLaunch = data.GetValue(GAME_DISPLAY_ICON).ToString().Trim(new char[] { ' ', '\'', '"' });

					gameDataList.Add(new RegistryGameData(strTitle, strLaunch, strLaunch, ORIGIN_NAME));
				}
			}
		}

		/// <summary>
		/// Find installed Bethesda.net games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		private static void GetBethesdaGames(List<RegistryGameData> gameDataList)
		{
			List<RegistryKey> keyList = new List<RegistryKey>();

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE64_REG))
			{
				keyList = FindGameKeys(key, BETHESDA_NET, BETHESDA_PATH, BETHESDA_CREATION_KIT);

				foreach(var data in keyList)
				{
					string strTitle  = data.GetValue(GAME_DISPLAY_NAME).ToString();
					string strLaunch = BETHESTA_LAUNCH + data.GetValue(BETHESTA_PRODUCT_ID).ToString() + ".exe";
					string strIcon	 = data.GetValue(BETHESDA_PATH).ToString().Trim(new char[] { ' ', '\'', '"'}) + "\\" + data.GetValue(GAME_DISPLAY_NAME).ToString() + ".exe";

					gameDataList.Add(new RegistryGameData(strTitle, strLaunch, strIcon, BETHESDA_NAME));
				}
			}
		}

		/// <summary>
		/// Find installed Battle.net games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		private static void GetBattlenetGames(List<RegistryGameData> gameDataList)
		{
			List<RegistryKey> keyList = new List<RegistryKey>();

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE64_REG))
			{
				keyList = FindGameKeys(key, BATTLE_NET, BATTLENET_UNINSTALL_STRING, BATTLE_NET);

				foreach(var data in keyList)
				{
					string strTitle  = data.GetValue(GAME_DISPLAY_NAME).ToString();
					string strLaunch = data.GetValue(GAME_DISPLAY_ICON).ToString();

					gameDataList.Add(new RegistryGameData(strTitle, strLaunch, strLaunch, BATTLENET_NAME));
				}
			}
		}

		/// <summary>
		/// Find installed Epic store games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		private static void GetEpicGames(List<RegistryGameData> gameDataList)
		{
			string strStorePath = "";
			List<RegistryKey> keyList = new List<RegistryKey>();

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE64_REG))
			{
				keyList = FindGameKeys(key, EPIC_GAMES_LAUNCHER, GAME_DISPLAY_NAME, EPIC_UNREAL_ENGINE);

				if(keyList.Count == 0) // Epic Game Store not found
					return;

				strStorePath = keyList[0].GetValue(GAME_INSTALL_LOCATION).ToString();
			}
			string[] folders = Directory.GetDirectories(strStorePath, "*", SearchOption.TopDirectoryOnly);

			foreach(string folder in folders)
			{
				if(!(folder.Contains(EPIC_GAMES_LAUNCHER) || folder.Contains(EPIC_DIRECT_X_REDIST)))
				{
					string strTitle  = folder.Substring(folder.LastIndexOf('\\') + 1);
					string strLaunch = folder + "\\" + strTitle + ".exe";

					gameDataList.Add(new RegistryGameData(strTitle, strLaunch, strLaunch, EPIC_NAME));
				}
			}
		}

		/// <summary>
		/// Find game keys in specified root
		/// Looks for a key-value pair inside the specified root.
		/// </summary>
		/// <param name="root">Root folder that will be scanned</param>
		/// <param name="strKey">The target key that should contain the target value</param>
		/// <param name="strValue">The target value in the key</param>
		/// <param name="ignore">Function will ignore this folder (used to ignore things like launchers)</param>
		/// <returns>List of game registry keys</returns>
		private static List<RegistryKey> FindGameKeys(RegistryKey root, string strKey, string strValue, string ignore)
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
						if(sub != "Microsoft" && sub != ignore) // Microsoft folder only contains system stuff and it doesn't need searching
						{
							try
							{
								toCheck.AddLast(root.OpenSubKey(sub));
							}
							catch(System.Security.SecurityException)
							{

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
						if(sub != "Microsoft" && sub.Contains(strFolder))
						{
							gameKeys.Add(root.OpenSubKey(sub));
						}
					}
				}
			}
			return gameKeys;
		}
	}
}