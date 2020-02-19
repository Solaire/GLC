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
		private const string STEAM_REG	= @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
		private const string GOG_REG	= @"SOFTWARE\Wow6432Node\GOG.com\Games";
		private const string GOG_CLIENT = @"SOFTWARE\WOW6432Node\GOG.com\GalaxyClient\paths";
		private const string NODE64_REG = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

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