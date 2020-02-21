using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GameLauncher_Console
{
	/// <summary>
	/// Class used to scan the registry and retrieve the game data.
	/// TODO: Review the code and try to optimise/reduce the clutter
	/// TODO: Instead of using dictionaries and string lists, create a struct and use that to collect and return the data.
	/// </summary>
	public static class CRegScanner
	{
		// Registry locations for the game clients / stores
		private const string NODE64_REG = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

		// Steam constant strings
		private const string STEAM_NAME			= "STEAM";
		private const string STEAM_REG			= @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
		private const string STEAM_GAME_FOLDER	= "Steam App";
		private const string STEAM_LAUNCH		= "steam://rungameid/";

		// GOG constant strings
		private const string GOG_NAME			= "GOG";
		private const string GOG_REG_GAMES		= @"SOFTWARE\Wow6432Node\GOG.com\Games";
		private const string GOG_REG_CLIENT		= @"SOFTWARE\WOW6432Node\GOG.com\GalaxyClient\paths";
		private const string GOG_CLIENT			= "client";
		private const string GOG_GAME_ID		= "GameID";
		private const string GOG_GAME_PATH		= "PATH";
		private const string GOG_GAME_NAME		= "GAMENAME";
		private const string GOG_GAME_LAUNCH	= "LAUNCHCOMMAND";
		private const string GOG_LAUNCH			= " /command=runGame /gameId";
		private const string GOG_PATH			= "/path=";
		private const string GOG_GALAXY_EXE		= "\\GalaxyClient.exe";

		// Uplay constant strings
		private const string UPLAY_NAME			= "UPLAY";
		private const string UPLAY_INSTALL		= "Uplay Install";
		private const string UPLAY_LAUNCH		= "uplay://launch/";

		// Origin constant strings
		private const string ORIGIN_NAME		= "Origin";
		private const string ORIGIN_GAMES		= "Origin Games";

		// Bethesda constant strings
		private const string BETHESDA_NAME		 = "bethesda.net";
		private const string BETHESDA_PATH		 = "Path";
		private const string BETHESDA_CREATION_KIT	= "Creation Kit";
		private const string BETHESTA_LAUNCH	 = "bethesda://run/";
		private const string BETHESTA_PRODUCT_ID = "ProductID";

		// Battlenet constant strings
		private const string BATTLENET_NAME		= "Battle.net";
		private const string BATTLENET_UNINSTALL_STRING = "UninstallString";

		// Epic store constant strings
		private const string EPIC_NAME				= "Epic";
		private const string EPIC_GAMES_LAUNCHER	= "Epic Games Launcher";
		private const string EPIC_UNREAL_ENGINE		= "Unreal Engine";
		private const string EPIC_LAUNCHER			= "Launcher";
		private const string EPIC_DIRECT_X_REDIST	= "DirectXRedist";

		//
		private const string GAME_DISPLAY_NAME		= "DisplayName";
		private const string GAME_DISPLAY_ICON		= "DisplayIcon";
		private const string GAME_INSTALL_LOCATION	= "InstallLocation";

		/// <summary>
		/// Collect data from the registry
		/// </summary>
		public struct RegistryGameData
		{
			string m_strTitle;
			string m_strLaunch;
			string m_strIcon;
			string m_strPlatform;

			public RegistryGameData(string strTitle, string strLaunch, string strIcon, string strPlatform)
			{
				m_strTitle		= strTitle;
				m_strLaunch		= strLaunch;
				m_strIcon		= strIcon;
				m_strPlatform	= strPlatform;
			}
		}

		public static List<RegistryGameData> GetGames()
		{
			List<RegistryGameData> gameDataList = new List<RegistryGameData>();

			GetSteamGames(gameDataList);
			GetGogGames(gameDataList);
			GetUplayGames(gameDataList);
			GetEpicStoreGames(gameDataList);
			GetBethesdaGames(gameDataList);
			GetBattlenetGames(gameDataList);
			GetOriginGames(gameDataList);

			return gameDataList;
		}

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
					string strLaunch = STEAM_LAUNCH + Path.GetFileNameWithoutExtension(strTitle).Substring(10);
					string strIcon	 = FindGameBinaryFile(data.GetValue(GAME_INSTALL_LOCATION).ToString(), data.GetValue(GAME_DISPLAY_NAME).ToString());
					gameDataList.Add(new RegistryGameData(strTitle, strLaunch, strIcon, "Steam"));
				}
			}
		}

		private static void GetGogGames(List<RegistryGameData> gameDataList)
		{
			string strClientPath = "";

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(GOG_REG_CLIENT))
			{
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

		private static void GetUplayGames(List<RegistryGameData> gameDataList)
		{
			List<RegistryKey> keyList = new List<RegistryKey>();

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE64_REG))
			{
				keyList = FindGameFolders(key, "Uplay Install");

				foreach(var data in keyList)
				{
					string strTitle		= data.GetValue(UPLAY_INSTALL).ToString();
					string strLaunch	= UPLAY_LAUNCH + Path.GetFileNameWithoutExtension(data.Name).Substring(10) + "/0";
					string strIcon		= FindGameBinaryFile(data.GetValue(GAME_INSTALL_LOCATION).ToString(), data.GetValue(GAME_DISPLAY_NAME).ToString());

					gameDataList.Add(new RegistryGameData(strTitle, strLaunch, strIcon, UPLAY_NAME));
				}
			}
		}

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

		private static void GetBethesdaGames(List<RegistryGameData> gameDataList)
		{
			List<RegistryKey> keyList = new List<RegistryKey>();

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE64_REG))
			{
				keyList = FindGameKeys(key, BETHESDA_NAME, BETHESDA_PATH, BETHESDA_CREATION_KIT);

				foreach(var data in keyList)
				{
					string strTitle  = data.GetValue(GAME_DISPLAY_NAME).ToString();
					string strLaunch = BETHESTA_LAUNCH + data.GetValue(BETHESTA_PRODUCT_ID).ToString() + ".exe";
					string strIcon	 = data.GetValue(BETHESDA_PATH).ToString().Trim(new char[] { ' ', '\'', '"'}) + "\\" + data.GetValue(GAME_DISPLAY_NAME).ToString() + ".exe";

					gameDataList.Add(new RegistryGameData(strTitle, strLaunch, strIcon, BETHESDA_NAME));
				}
			}
		}

		private static void GetBattlenetGames(List<RegistryGameData> gameDataList)
		{
			List<RegistryKey> keyList = new List<RegistryKey>();

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE64_REG))
			{
				keyList = FindGameKeys(key, BATTLENET_NAME, BATTLENET_UNINSTALL_STRING, BATTLENET_NAME);

				foreach(var data in keyList)
				{
					string strTitle  = data.GetValue(GAME_DISPLAY_NAME).ToString();
					string strLaunch = data.GetValue(GAME_DISPLAY_ICON).ToString();

					gameDataList.Add(new RegistryGameData(strTitle, strLaunch, strLaunch, BATTLENET_NAME));
				}
			}
		}

		private static void GetEpicGames(List<RegistryGameData> gameDataList)
		{
			string strStorePath = "";
			List<RegistryKey> keyList = new List<RegistryKey>();

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE64_REG))
			{
				keyList = FindGameKeys(key, EPIC_GAMES_LAUNCHER, GAME_DISPLAY_NAME, EPIC_UNREAL_ENGINE);
				strStorePath = keyList[0].GetValue(GAME_INSTALL_LOCATION).ToString();
			}
			string[] folders = Directory.GetDirectories(strStorePath, "*", SearchOption.TopDirectoryOnly);

			foreach(string folder in folders)
			{
				if(!(folder.Contains(EPIC_GAMES_LAUNCHER) || folder.Contains(EPIC_DIRECT_X_REDIST)))
				{
					string strTitle  = folder.Substring(folder.LastIndexOf('\\') + 1);
					string strLaunch = folder + "\\" + strTitle + ".exe");

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
		/*
		public static void GetGames(Dictionary<string, List<string>> games, List<string> platforms)
		{
			foreach(string s in platforms)
			{
				string[] _temp = s.Split('=');

				if(_temp[1] == "false")
					continue;

				try
				{
					switch(_temp[0])
					{
						case "STEAM":
							GetSteamGames(games);
							break;

						case "GOG":
							GetGogGames(games);
							break;

						case "UPLAY":
							GetUplayGames(games);
							break;

						case "ORIGIN":
							GetOriginGames(games);
							break;

						case "BETHESDA.NET":
							GetBethesdaGames(games);
							break;

						case "EPICGAMES":
							GetEpicStoreGames(games);
							break;

						case "BATTLENET":
							GetBattlenetGames(games);
							break;

						default:
							break;
					}
				}
				catch
				{
					//Log
				}

			}

		}
		*/

		/// <summary>
		/// Find the game's binary executable file
		/// </summary>
		/// <param name="strPath">Path to search</param>
		/// <param name="strTitle">Title of the game</param>
		/// <returns></returns>
		private static string FindGameBinaryFile(string strPath, string strTitle)
		{
			/* function flow
			 * Find all exe files in the directory. (Exclude files with words: "redist", "x86", "x64")
			 * Compare exe files with gameTitle
			 * If failed, compare with name of root directory
			 * Return "" if failed
			 */

			// Get our list of exe files in the game folder + all subfolders
			List<string> exeFiles = Directory.EnumerateFiles(strPath, "*", SearchOption.AllDirectories).Where(s => s.EndsWith(".exe")).ToList();

			// If only 1 file has been detected return it.
			if(exeFiles.Count == 1)
			{
				return exeFiles[0];
			}

			// First, compare the files against the game title
			{
				string[] words = strTitle.Split(new char[] { ' ', ':', '-', '_' });
				string letters = "";

				foreach(string word in words)
				{
					if(word != "")
						letters += word[0];
				}

				foreach(string file in exeFiles)
				{
					if(file.ToLower().Contains("redist")) // A lot of steam games contain C++ redistributables. Ignore them
						continue;


					string description = FileVersionInfo.GetVersionInfo(file).FileDescription ?? "";
					description.ToLower();

					FileInfo info = new FileInfo(file);
					string name = info.Name.Substring(0, info.Name.IndexOf('.')).ToLower();

					// Perform a check againt the acronym
					if(letters.Length > 2)
					{
						if(letters.ToLower().Contains(name) || name.Contains(letters.ToLower()))
							return file;

						else if((description != null && description != "") &&
						(letters.ToLower().Contains(description) || description.Contains(letters.ToLower())))
							return file;
					}

					foreach(string word in words)
					{
						if(word == "" && word.Length < 3)
							continue;

						if(name.Contains(word.ToLower()))
							return file;

						if((description != null && description != "") && description.Contains(word.ToLower()))
							return file;
					}
				}
			}

			// If search failed, we need to compare the exe files against the name of root directory
			{
				string[] words = strPath.Substring(strPath.LastIndexOf('/')).Split(new char[] { ' ', '-', '_', ':' });
				string letters = "";

				foreach(string word in words)
				{
					if(word != "")
						letters += word[0];
				}

				foreach(string file in exeFiles)
				{
					if(file.ToLower().Contains("redist")) // A lot of steam games contain C++ redistributables. Ignore them
						continue;

					string description = FileVersionInfo.GetVersionInfo(file).FileDescription ?? "";
					description.ToLower();

					FileInfo info = new FileInfo(file);
					string name = info.Name.Substring(0, info.Name.IndexOf('.')).ToLower();

					// Perform a check againt the acronym
					if(letters.Length > 2)
					{
						if(letters.ToLower().Contains(name) || name.Contains(letters.ToLower()))
							return file;

						else if((description != null && description != "") &&
						(letters.ToLower().Contains(description) || description.Contains(letters.ToLower())))
							return file;
					}

					foreach(string word in words)
					{
						if(word == "" && word.Length < 3)
							continue;

						if(name.Contains(word.ToLower()))
							return file;

						if((description != null && description != "") && description.Contains(word.ToLower()))
							return file;
					}
				}
			}
			return "";
		}

		/// <summary>
		/// Find installed STEAM games in the registry and add them to the xml file.
		/// </summary>
		public static void GetSteamGames(Dictionary<string, List<string>> games)
		{
			List<RegistryKey> gameKeys = new List<RegistryKey>();

			RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
			using(RegistryKey key = baseKey.OpenSubKey(STEAM_REG, RegistryKeyPermissionCheck.ReadSubTree))
			{
				gameKeys = FindGameFolders(key, "Steam App");

				foreach(var gameData in gameKeys)
				{
					games["Name"].Add(gameData.GetValue("DisplayName").ToString());
					games["LaunchCommand"].Add("steam://rungameid/" + System.IO.Path.GetFileNameWithoutExtension(gameData.Name).Substring(10));
					games["Platform"].Add("STEAM");
					games["Icon"].Add(FindGameBinaryFile(gameData.GetValue("InstallLocation").ToString(), gameData.GetValue("DisplayName").ToString()));
					games["Flag"].Add("0");
				}
			}
		}

		/// <summary>
		/// Find installed GOG games in the registry and add them to the xml file.
		/// </summary>
		public static void GetGogGames(Dictionary<string, List<string>> games)
		{
			string gameID = "";
			string gamePath = "";
			string clientPath = "";

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(GOG_CLIENT))
			{
				clientPath = key.GetValue("client").ToString() + "\\GalaxyClient.exe";
			}

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(GOG_REG))
			{
				foreach(string subKey_name in key.GetSubKeyNames())
				{
					using(RegistryKey subKey = key.OpenSubKey(subKey_name))
					{
						gameID = subKey.GetValue("gameID").ToString();
						gamePath = subKey.GetValue("PATH").ToString();

						games["Name"].Add(subKey.GetValue("GAMENAME").ToString());
						games["LaunchCommand"].Add(clientPath + " /command=runGame /gameId=" + gameID + "/path=" + gamePath);
						games["Platform"].Add("GOG");
						games["Icon"].Add(subKey.GetValue("LAUNCHCOMMAND").ToString().Trim(new char[] { ' ', '\'', '"' }));
						games["Flag"].Add("0");
					}
				}
			}
		}

		/// <summary>
		/// Find installed UPLAY games in the registry and add them to the xml file.
		/// </summary>
		public static void GetUplayGames(Dictionary<string, List<string>> games)
		{
			List<RegistryKey> gameKeys = new List<RegistryKey>();

			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE64_REG))
			{
				gameKeys = FindGameFolders(key, "Uplay Install");

				foreach(var gameData in gameKeys)
				{
					games["Name"].Add(gameData.GetValue("DisplayName").ToString());
					games["LaunchCommand"].Add("uplay://launch/" + System.IO.Path.GetFileNameWithoutExtension(gameData.Name).Substring(10) + "/0");
					games["Platform"].Add("UPLAY");
					games["Icon"].Add(FindGameBinaryFile(gameData.GetValue("InstallLocation").ToString(), gameData.GetValue("DisplayName").ToString()));
					games["Flag"].Add("0");
				}
			}
		}

		/// <summary>
		/// Find installed ORIGIN games in the registry and add them to xml file
		/// </summary>
		/// Finding and extracting Origin games is more tricky as the games are not in the same place as origin (Game keys are under their developer entry)
		/// Extracting origin games: Use ORIGIN reg key to get "DisplayName" value in the game ID -> use first word from the "DisplayName" to find key in the same parent node -> Use "Product GUID" to locate game data in the uninstall node
		public static void GetOriginGames(Dictionary<string, List<string>> games)
		{
			List<RegistryKey> gameKeys = new List<RegistryKey>();
			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE64_REG))
			{
				gameKeys = FindGameKeys(key, "Origin Games", "InstallLocation", "Origin");
				foreach(var gameData in gameKeys)
				{
					games["Name"].Add(gameData.GetValue("DisplayName").ToString());
					games["LaunchCommand"].Add(gameData.GetValue("DisplayIcon").ToString().Trim(new char[] { ' ', '\'', '"' }));
					games["Platform"].Add("ORIGIN");
					games["Icon"].Add(gameData.GetValue("DisplayIcon").ToString().Trim(new char[] { ' ', '\'', '"' }));
					games["Flag"].Add("0");
				}
			}
		}

		/// <summary>
		/// Find installed BETHESDA games in the registry and add them to xml file
		/// </summary>
		/// Will need to find exe files for each
		public static void GetBethesdaGames(Dictionary<string, List<string>> games)
		{
			List<RegistryKey> gameKeys = new List<RegistryKey>();
			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE64_REG))
			{
				gameKeys = FindGameKeys(key, "bethesda.net", "Path", "Creation Kit");
				foreach(var gameData in gameKeys)
				{
					games["Name"].Add(gameData.GetValue("DisplayName").ToString());
					games["LaunchCommand"].Add("bethesdanet://run/" + gameData.GetValue("ProductID").ToString());
					games["Platform"].Add("BETHESDA.NET");
					games["Icon"].Add(gameData.GetValue("Path").ToString().Trim(new char[] { ' ', '\'', '"' }) + "\\" + gameData.GetValue("DisplayName").ToString() + ".exe");
					games["Flag"].Add("0");
				}
			}
		}

		/// <summary>
		/// Find installed BATTLENET games in the registry and add them to xml file
		/// </summary>
		public static void GetBattlenetGames(Dictionary<string, List<string>> games)
		{
			List<RegistryKey> gameKeys = new List<RegistryKey>();
			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE64_REG))
			{
				gameKeys = FindGameKeys(key, "Battle.net", "UninstallString", "Battle.net");
				foreach(var gameData in gameKeys)
				{
					games["Name"].Add(gameData.GetValue("DisplayName").ToString());
					games["LaunchCommand"].Add(gameData.GetValue("DisplayIcon").ToString());
					games["Platform"].Add("BATTLENET");
					games["Icon"].Add(gameData.GetValue("DisplayIcon").ToString());
					games["Flag"].Add("0");
				}
			}
		}

		/// <summary>
		/// Find installed EPIC games in the registry and add them to xml file
		/// </summary>
		public static void GetEpicStoreGames(Dictionary<string, List<string>> games)
		{
			string storePath = "";
			List<RegistryKey> gameKeys = new List<RegistryKey>();
			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE64_REG))
			{
				gameKeys = FindGameKeys(key, "Epic Games Launcher", "DisplayName", "Unreal Engine");
				storePath = gameKeys[0].GetValue("InstallLocation").ToString();
			}
			string[] folders = System.IO.Directory.GetDirectories(storePath, "*", System.IO.SearchOption.TopDirectoryOnly);

			foreach(string folder in folders)
			{
				if(!(folder.Contains("Launcher") || folder.Contains("DirectXRedist")))
				{
					games["Name"].Add(folder.Substring(folder.LastIndexOf('\\') + 1));
					games["LaunchCommand"].Add(folder + "\\" + folder.Substring(folder.LastIndexOf('\\') + 1) + ".exe");
					games["Platform"].Add("EPICSTORE");
					games["Icon"].Add(folder + "\\" + folder.Substring(folder.LastIndexOf('\\') + 1) + ".exe");
					games["Flag"].Add("0");
				}
			}
		}
	}
}