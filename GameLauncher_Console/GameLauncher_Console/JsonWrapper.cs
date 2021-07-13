using HtmlAgilityPack;
using IniParser;
using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
//using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using static System.Environment;

namespace GameLauncher_Console
{
	/// <summary>
	/// Class for serializing and deserializing JSON data. 
	/// JSON data is stored and handled in a dynamically sized dictionary structure.
	/// </summary>
	public static class CJsonWrapper
	{
		// minimum version numbers
		private static readonly Version MIN_CFG_VERSION		= System.Version.Parse("1.2.0");
		private static readonly Version MIN_LAST_VERSION	= System.Version.Parse("1.1.0");
		private static readonly Version MIN_GAME_VERSION	= System.Version.Parse("1.1.0");

		// .json filenames (they should be in the same directory as the executable)
		private static readonly string CFG_INI_FILE			= CDock.FILENAME + ".ini";
		private static readonly string CFGOLD_JSON_FILE		= CDock.FILENAME + "-cfg.json";
		private static readonly string LAST_JSON_FILE		= CDock.FILENAME + "-last.json";
		private static readonly string GAME_JSON_FILE		= CDock.FILENAME + "-games.json";
		private const string GAMEOLD_JSON_FILE				= "games.json";

		// INI sections
		private const string VERSION_SECTION				= "version";
		private const string CONFIG_SECTION					= "settings";

		// JSON field names

		private const string INI_VERSION					= "ini_version";
		private const string JSON_VERSION					= "json_version";
		
		// games json
		private const string GAMES_ARRAY					= "games";
		private const string GAMES_ARRAY_ID					= "id";
		private const string GAMES_ARRAY_TITLE				= "title";
		private const string GAMES_ARRAY_LAUNCH				= "launch";
		private const string GAMES_ARRAY_ICON				= "icon";
		private const string GAMES_ARRAY_UNINSTALLER		= "uninstaller";
		private const string GAMES_ARRAY_PLATFORM			= "platform";
		private const string GAMES_ARRAY_INSTALLED			= "installed";
		private const string GAMES_ARRAY_FAVOURITE			= "favourite";
		private const string GAMES_ARRAY_NEW				= "new";
		private const string GAMES_ARRAY_HIDDEN				= "hidden";
		private const string GAMES_ARRAY_ALIAS				= "alias";
		private const string GAMES_ARRAY_FREQUENCY			= "frequency";

		// search json (for command line non-interactive interactions)
		private const string LAST_ARRAY						= "matches";
		private const string LAST_ARRAY_INDEX				= "index";
		private const string LAST_ARRAY_TITLE				= "title";
		private const string LAST_ARRAY_PERCENT				= "percent";

		private static readonly string currentPath	= Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
		private static readonly string configPath	= currentPath + "\\" + CFG_INI_FILE;
		private static readonly string configOldPath = currentPath + "\\" + CFGOLD_JSON_FILE;
		private static readonly string searchPath	= currentPath + "\\" + LAST_JSON_FILE;
		private static readonly string gamesPath	= currentPath + "\\" + GAME_JSON_FILE;
		private static readonly string gamesOldPath	= currentPath + "\\" + GAMEOLD_JSON_FILE;
		private static readonly string version		= Assembly.GetEntryAssembly().GetName().Version.ToString();

		// Configuration data
		public static bool ImportFromINI(out CConfig.ConfigVolatile cfgv, out CConfig.Hotkeys keys, out CConfig.Colours cols)
        {
			CConfig.config = new Dictionary<string, string>();
			cfgv = new CConfig.ConfigVolatile();
			keys = new CConfig.Hotkeys();
			cols = new CConfig.Colours();
			bool parseError = false;

			// Set up default configuration
			IOrderedEnumerable<SettingsProperty> settings = Properties.Settings.Default.Properties.OfType<SettingsProperty>().OrderBy(s => s.Name);
			foreach (SettingsProperty setting in settings)
			{
				CConfig.config.Add(setting.Name, setting.DefaultValue.ToString());
			}

			try
			{
				if (!File.Exists(configPath))
				{
					if (File.Exists(configOldPath))
					{
						File.Delete(configOldPath);
						CLogger.LogWarn("{0} is outdated. Creating new {1}...", CFGOLD_JSON_FILE, CFG_INI_FILE);
					}
					else
						CLogger.LogInfo("{0} is missing. Writing new defaults...", CFG_INI_FILE);

					CreateNewConfigFile(configPath);
					SetConfigVolatile(ref cfgv);
					TranslateConfig(ref keys, ref cols);
				}
				else if (!ImportConfig(configPath, ref cfgv, ref keys, ref cols))
					parseError = true;
			}
			catch (Exception e)
            {
				CLogger.LogError(e);
            }

			return !parseError;
		}

		/// <summary>
		/// Import games from the games json config file
		/// </summary>
		/// <returns>True if successful, otherwise false</returns>
		public static bool ImportFromJSON(out List<CGameData.CMatch> matches)
		{
			bool parseError = false;
			matches = new List<CGameData.CMatch>();

			// Previous search matches
			Version verSearch = System.Version.Parse("0.0");
			try
			{
				if (File.Exists(searchPath))
				{
					CLogger.LogInfo("{0} was found.", LAST_JSON_FILE);
					ImportSearch(searchPath, ref verSearch, ref matches);
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
			}

			if (verSearch < MIN_LAST_VERSION)
				matches = new List<CGameData.CMatch>();

			// Game data
			int nGameCount = 0;
			Version verGames = System.Version.Parse("0.0");

			try
			{
				if (!File.Exists(gamesPath))
				{
					if (File.Exists(gamesOldPath))
					{
						File.Delete(gamesOldPath);
						CLogger.LogWarn("{0} is outdated. Creating new {1}...", GAMEOLD_JSON_FILE, GAME_JSON_FILE);
					}
					else
						CLogger.LogInfo("{0} is missing. Creating new file...", GAME_JSON_FILE);
					CreateNewGamesFile(gamesPath);
				}
				else if (!(bool)CConfig.GetConfigBool(CConfig.CFG_USESCAN))
				{
					if (!ImportGames(gamesPath, ref nGameCount, ref verGames, (bool)CConfig.GetConfigBool(CConfig.CFG_USEALPH), (bool)CConfig.GetConfigBool(CConfig.CFG_USEFAVE), (bool)CConfig.GetConfigBool(CConfig.CFG_USEINST), true))
						parseError = true;
				}
			}
			catch (Exception e)
            {
				CLogger.LogError(e);
            }

			if (nGameCount < 1)
			{
				CLogger.LogInfo("{0} is empty, corrupt, or outdated. Scanning for games...", GAME_JSON_FILE);
				Console.Write("Scanning for games");  // ScanGames() will add dots for each platform
				CRegScanner.ScanGames((bool)CConfig.GetConfigBool(CConfig.CFG_USECUST), !(bool)CConfig.GetConfigBool(CConfig.CFG_IMGSCAN), true);
			}

			return !parseError;
		}

		/// <summary>
		/// Export game data from memory to the games json file
		/// NOTE: At the moment, the program will pretty pretty much create a brand new JSON file and override all of the content...
		/// ... I need to find a nice workaround as JsonDocument class is read-only.
		/// </summary>
		/// <returns>True is successful, otherwise false</returns>
		public static bool ExportGames(List<CGameData.CGame> gameList)
		{
			CLogger.LogInfo("Saving {0} games to JSON...", gameList.Count);
			var options = new JsonWriterOptions
			{
				Indented = true
			};

			try
			{
				using(var stream = new MemoryStream())
				{
					using(var writer = new Utf8JsonWriter(stream, options))
					{
						writer.WriteStartObject();
						writer.WriteString(JSON_VERSION, version);
						writer.WriteStartArray(GAMES_ARRAY);
						for(int i = 0; i < gameList.Count; i++)
						{
							WriteGame(writer, gameList[i]); //, stream, options,
						}
						writer.WriteEndArray();
						writer.WriteEndObject();
					}

					string strJsonData = Encoding.UTF8.GetString(stream.ToArray());
					byte[] bytes = new UTF8Encoding(true).GetBytes(strJsonData);

					using(FileStream fs = File.Create(gamesPath))
					{
						fs.Write(bytes, 0, bytes.Length);
					}
				}
			}
			catch(Exception e)
			{
				CLogger.LogError(e);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Export search matches from memory to the search json file
		/// </summary>
		/// <returns>True is successful, otherwise false</returns>
		public static bool ExportSearch(List<CGameData.CMatch> matchList)
		{
			CLogger.LogInfo("Saving search data to JSON...");
			var options = new JsonWriterOptions
			{
				Indented = true
			};

			try
			{
				using (var stream = new MemoryStream())
				{
					using (var writer = new Utf8JsonWriter(stream, options))
					{
						writer.WriteStartObject();
						writer.WriteString(JSON_VERSION, version);
						writer.WriteStartArray(LAST_ARRAY);
						for (int i = 0; i < matchList.Count; i++)
						{
							WriteSearch(writer, matchList[i]); //, stream, options,
						}
						writer.WriteEndArray();
						writer.WriteEndObject();
					}

					string strJsonData = Encoding.UTF8.GetString(stream.ToArray());
					byte[] bytes = new UTF8Encoding(true).GetBytes(strJsonData);

					using (FileStream fs = File.Create(searchPath))
					{
						fs.Write(bytes, 0, bytes.Length);
					}
				}

			}
			catch (Exception e)
			{
				CLogger.LogError(e);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Export configuration data from memory to the config json file
		/// </summary>
		/// <returns>True is successful, otherwise false</returns>
		public static bool ExportConfig()
		{
			CLogger.LogInfo("Saving configuration data to INI...");

			try
			{
				CIniParser ini = new CIniParser(configPath);

				ini.Write(INI_VERSION, version, VERSION_SECTION);
				foreach (KeyValuePair<string, string> setting in CConfig.config)
				{
					ini.Write(setting.Key, setting.Value, CONFIG_SECTION);
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Create empty games json file with the empty array
		/// </summary>
		/// <returns>True if file created, otherwise false</returns>
		private static bool CreateNewGamesFile(string file)
		{
			var options = new JsonWriterOptions
			{
				Indented = true
			};

			try
			{
				using(var stream = new MemoryStream())
				{
					using(var writer = new Utf8JsonWriter(stream, options))
					{
						writer.WriteStartObject();
						writer.WriteString(JSON_VERSION, version);
						writer.WriteStartArray(GAMES_ARRAY);
						writer.WriteEndArray();
						writer.WriteEndObject();
					}

					string strJsonData = Encoding.UTF8.GetString(stream.ToArray());
					byte[] bytes = new UTF8Encoding(true).GetBytes(strJsonData);

					using (FileStream fs = File.Create(file))
					{
						fs.Write(bytes, 0, bytes.Length);
					}
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Create empty config json file
		/// </summary>
		/// <returns>True if file created, otherwise false</returns>
		private static bool CreateNewConfigFile(string file)
		{
			try
			{
				if (File.Exists(file))
					File.Delete(file);
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
			}

			try
			{
				CIniParser ini = new CIniParser(file);

				ini.Write(INI_VERSION, version, VERSION_SECTION);
				ini.WriteComment(" <https://docs.microsoft.com/dotnet/api/system.consolekey>", " Valid key_* entries :");
				ini.WriteComment(" <https://docs.microsoft.com/dotnet/api/system.consolecolor>", " Valid colour_* entries :");
				foreach (KeyValuePair<string, string> setting in CConfig.config)
				{
					ini.Write(setting.Key, setting.Value, CONFIG_SECTION);
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Import games from the json file and add them to the global game dictionary.
		/// <returns>True if successful, otherwise false</returns>
		/// </summary>
		private static bool ImportGames(string file, ref int nGameCount, ref Version version, bool alphaSort, bool faveSort, bool instSort, bool ignoreArticle)
		{
			CLogger.LogInfo("Importing games from JSON...");
			var options = new JsonDocumentOptions
			{
				AllowTrailingCommas = true
			};

			string strDocumentData = File.ReadAllText(file);

			if (string.IsNullOrEmpty(strDocumentData))
				return false;

			try
			{
				using (JsonDocument document = JsonDocument.Parse(@strDocumentData, options))
				{
					System.Version.TryParse(GetStringProperty(document.RootElement, JSON_VERSION), out version);
					if (version < MIN_GAME_VERSION) return true;

					if (!document.RootElement.TryGetProperty(GAMES_ARRAY, out JsonElement jArrGames)) // 'games' array does not exist
						return false;

					foreach (JsonElement jElement in jArrGames.EnumerateArray())
					{
						string strID = GetStringProperty(jElement, GAMES_ARRAY_ID);
						string strTitle = GetStringProperty(jElement, GAMES_ARRAY_TITLE);
						if (string.IsNullOrEmpty(strTitle))
							continue;

						string strLaunch = GetStringProperty(jElement, GAMES_ARRAY_LAUNCH);
						string strIconPath = GetStringProperty(jElement, GAMES_ARRAY_ICON);
						string strUninstall = GetStringProperty(jElement, GAMES_ARRAY_UNINSTALLER);
						bool   bIsInstalled = GetBoolProperty(jElement, GAMES_ARRAY_INSTALLED);
						bool   bIsFavourite = GetBoolProperty(jElement, GAMES_ARRAY_FAVOURITE);
						bool   bIsNew = GetBoolProperty(jElement, GAMES_ARRAY_NEW);
						bool   bIsHidden = GetBoolProperty(jElement, GAMES_ARRAY_HIDDEN);
						string strAlias = GetStringProperty(jElement, GAMES_ARRAY_ALIAS);
						string strPlatform = GetStringProperty(jElement, GAMES_ARRAY_PLATFORM);
						double fOccurCount = GetDoubleProperty(jElement, GAMES_ARRAY_FREQUENCY);

						CGameData.AddGame(strID, strTitle, strLaunch, strIconPath, strUninstall, bIsInstalled, bIsFavourite, bIsNew, bIsHidden, strAlias, strPlatform, fOccurCount);
						nGameCount++;
					}
					CGameData.SortGames(alphaSort, faveSort, instSort, ignoreArticle);
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e, string.Format($"Malformed {file} file!"));
				Console.WriteLine($"ERROR: Malformed {file} file!");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Find installed and not-installed Steam games (latter from an html file; requires public profile)
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		public static void GetSteamGames(List<CRegScanner.RegistryGameData> gameDataList, bool expensiveIcons)
		{
			const string NODE64_REG			= @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
			const string NODE32_REG			= @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
			const string GAME_DISPLAY_ICON	= "DisplayIcon";
			const string GAME_INSTALL_PATH	= "InstallPath";
			const string STEAM_NAME			= "Steam";
			//const string STEAM_NAME_LONG	= "Steam";
			const int STEAM_MAX_LIBS		= 64;
			const string STEAM_GAME_FOLDER	= "Steam App ";
			const string STEAM_LAUNCH		= "steam://rungameid/";
			const string STEAM_UNINST		= "steam://uninstall/";
			const string STEAM_PATH			= "steamapps";
			const string STEAM_LIBFILE		= "libraryfolders.vdf";
			const string STEAM_APPFILE		= "SteamAppData.vdf";
			const string STEAM_USRFILE		= "loginusers.vdf";
			const string STEAM_LIBARR		= "LibraryFolders";
			const string STEAM_APPDATA		= "SteamAppData";
			const string STEAM_APPARR		= "AppState";
			const string STEAM_USRARR		= "users";
			const string STEAM_REG			= @"SOFTWARE\WOW6432Node\Valve\Steam"; // HKLM32
			
			string strInstallPath = "";
			string strClientPath = "";

			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(STEAM_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if (key == null)
				{
					CLogger.LogInfo("{0} client not found in the registry.", STEAM_NAME.ToUpper());
					return;
				}

				strInstallPath = CRegScanner.GetRegStrVal(key, GAME_INSTALL_PATH);
				strClientPath = strInstallPath + "\\" + STEAM_PATH;
			}

			if (!Directory.Exists(strClientPath))
			{
				CLogger.LogInfo("{0} library not found: {1}", STEAM_NAME.ToUpper(), strClientPath);
				return;
			}

			string libFile = strClientPath + "\\" + STEAM_LIBFILE;
			List<string> libs = new List<string>
			{
				strClientPath
			};
			int nLibs = 1;

			try
			{
				if (File.Exists(libFile))
				{
					SteamWrapper document = new SteamWrapper(libFile);
					ACF_Struct documentData = document.ACFFileToStruct();
					ACF_Struct folders = documentData.SubACF[STEAM_LIBARR];

					for (; nLibs <= STEAM_MAX_LIBS; ++nLibs)
					{
						folders.SubItems.TryGetValue(nLibs.ToString(), out string library);
						if (string.IsNullOrEmpty(library))
						{
							nLibs--;
							break;
						}
						library += "\\" + STEAM_PATH;
						if (Directory.Exists(library))
							libs.Add(library);
					}
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e, string.Format("ERROR: Malformed {0} file: {1}", STEAM_NAME.ToUpper(), libFile));
				nLibs--;
			}

			int i = 0;
			List<string> allFiles = new List<string>();
			foreach (string lib in libs)
			{
				List<string> libFiles = new List<string>();
				try
				{
					libFiles = Directory.GetFiles(lib, "appmanifest_*.acf", SearchOption.TopDirectoryOnly).ToList();
					allFiles.AddRange(libFiles);
					CLogger.LogInfo("{0} {1} games found in library {2}", libFiles.Count, STEAM_NAME.ToUpper(), lib);
				}
				catch (Exception e)
				{
					CLogger.LogError(e, string.Format("ERROR: {0} directory read error: ", STEAM_NAME.ToUpper(), lib));
					continue;
				}

				foreach (string file in libFiles)
				{
					try
					{
						SteamWrapper document = new SteamWrapper(file);
						ACF_Struct documentData = document.ACFFileToStruct();
						ACF_Struct app = documentData.SubACF[STEAM_APPARR];

						string id = app.SubItems["appid"];
						if (id.Equals("228980"))  // Steamworks Common Redistributables
							continue;

						string strID = Path.GetFileName(file);
						string strTitle = app.SubItems["name"];
						CLogger.LogDebug($"- {strTitle}");
						string strLaunch = STEAM_LAUNCH + id;
						string strIconPath = "";
						string strUninstall = "";
						string strAlias = "";
						string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.Steam);

						if (!string.IsNullOrEmpty(strLaunch))
						{
							using (RegistryKey key = Registry.LocalMachine.OpenSubKey(NODE64_REG + "\\" + STEAM_GAME_FOLDER + id, RegistryKeyPermissionCheck.ReadSubTree),  // HKLM64
											   key2 = Registry.LocalMachine.OpenSubKey(NODE32_REG + "\\" + STEAM_GAME_FOLDER + id, RegistryKeyPermissionCheck.ReadSubTree))  // HKLM32
							{
								if (key != null)
									strIconPath = CRegScanner.GetRegStrVal(key, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
								else if (key2 != null)
									strIconPath = CRegScanner.GetRegStrVal(key2, GAME_DISPLAY_ICON).Trim(new char[] { ' ', '"' });
							}
							if (string.IsNullOrEmpty(strIconPath) && expensiveIcons)
							{
								strIconPath = CGameFinder.FindGameBinaryFile(lib + "\\common\\" + app.SubItems["installdir"], strTitle);
								strAlias = CRegScanner.GetAlias(Path.GetFileNameWithoutExtension(strIconPath));
							}
							else
								strAlias = CRegScanner.GetAlias(strTitle);
							if (strAlias.Length > strTitle.Length)
								strAlias = CRegScanner.GetAlias(strTitle);
							if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
								strAlias = "";
							strUninstall = STEAM_UNINST + id;
							gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
						}
					}
					catch (Exception e)
					{
						CLogger.LogError(e, string.Format("ERROR: Malformed {0} file: {1}", STEAM_NAME.ToUpper(), file));
					}
				}
				i++;
				if (i > nLibs)
					CLogger.LogDebug("---------------------");
			}

			// Get not-installed games
			if (!(bool)CConfig.GetConfigBool(CConfig.CFG_INSTONLY))
			{
				// First get Steam user ID
				ulong userId = (ulong)CConfig.GetConfigULong(CConfig.CFG_STEAMID);

				if (userId < 1)
				{
					try
					{
						ulong userIdTmp = 0;
						string userName = "";
						string userNameTmp = "";
						string appFile = strInstallPath + "\\config\\" + STEAM_APPFILE;

						if (File.Exists(appFile))
						{
							SteamWrapper appDoc = new SteamWrapper(appFile);
							ACF_Struct appDocData = appDoc.ACFFileToStruct();
							ACF_Struct appData = appDocData.SubACF[STEAM_APPDATA];

							appData.SubItems.TryGetValue("AutoLoginUser", out userName);

							SteamWrapper usrDoc = new SteamWrapper(strInstallPath + "\\config\\" + STEAM_USRFILE);
							ACF_Struct usrDocData = usrDoc.ACFFileToStruct();
							ACF_Struct usrData = usrDocData.SubACF[STEAM_USRARR];

							foreach (KeyValuePair<string, ACF_Struct> user in usrData.SubACF)
							{
								ulong.TryParse(user.Key, out userIdTmp);

								foreach (KeyValuePair<string, string> userVal in user.Value.SubItems)
								{
									if (userVal.Key.Equals("AccountName"))
									{
										userNameTmp = userVal.Value;
										if (userNameTmp.Equals(userName))
                                            ulong.TryParse(user.Key, out userId);
									}
									if (userVal.Key.Equals("MostRecent") && userVal.Value.Equals("1") && string.IsNullOrEmpty(userName))
									{
										userId = userIdTmp;
										userName = userNameTmp;
										break;
									}
								}
							}
							if (userId < 1)
							{
								userId = userIdTmp;
								userName = userNameTmp;
							}
						}
						if (userId > 0)
						{
							CConfig.SetConfigValue(CConfig.CFG_STEAMID, userId);
							ExportConfig();
						}
					}
					catch (Exception e)
					{
						CLogger.LogError(e, string.Format("ERROR: Malformed {0} file: {1} or {2}", STEAM_NAME.ToUpper(), STEAM_APPFILE, STEAM_USRFILE));
					}
				}

				if (userId > 0)
				{
					// Download game list from public user profile
					try
					{
						string url = string.Format("https://steamcommunity.com/profiles/{0}/games/?tab=all", CConfig.GetConfigULong(CConfig.CFG_STEAMID));
/*
#if DEBUG
						string tmpfile = $"tmp_{STEAM_NAME}.html";
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
*/
						HtmlWeb web = new HtmlWeb();
						web.UseCookies = true;
						HtmlAgilityPack.HtmlDocument doc = web.Load(url);
						doc.OptionUseIdAttribute = true;
//#endif
						HtmlNode gameList = doc.DocumentNode.SelectSingleNode("//script[@language='javascript']");
						if (gameList != null)
						{
							CLogger.LogInfo("{0} not-installed games:", STEAM_NAME.ToUpper());

							var options = new JsonDocumentOptions
							{
								AllowTrailingCommas = true
							};
							string rgGames = gameList.InnerText.Remove(0, gameList.InnerText.IndexOf('['));
							rgGames = rgGames.Remove(rgGames.IndexOf(';'));

							using (JsonDocument document = JsonDocument.Parse(@rgGames, options))
							{
								foreach (JsonElement game in document.RootElement.EnumerateArray())
								{
									ulong id = GetULongProperty(game, "appid");
									if (id > 0)
									{
										// Check if game is already installed
										string strID = $"appmanifest_{id}.acf";
										bool found = false;
										foreach (string file in allFiles)
										{
											if (file.EndsWith(strID))
												found = true;
										}
										if (!found)
										{
											string strTitle = GetStringProperty(game, "name");
											//string strIconPath = GetStringProperty(game, "logo");  // TODO: Use logo to download icon
											string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.Steam);

											// Add not-installed games
											CLogger.LogDebug($"- *{strTitle}");
											gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, "", "", "", "", false, strPlatform));
										}
									}
								}
							}
						}
						else
                        {
							CLogger.LogInfo("Can't get not-installed {0} games. Profile may not be public.\n" +
											"To change this, go to <https://steamcommunity.com/my/edit/settings>.",
								STEAM_NAME.ToUpper());
						}
/*
#if DEBUG
						File.Delete(tmpfile);
#endif
*/
					}
					catch (Exception e)
					{
						CLogger.LogError(e);
					}
					CLogger.LogDebug("---------------------");
				}
			}
		}

		/// <summary>
		/// Find installed Epic store games (from json files)
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		public static void GetEpicGames(List<CRegScanner.RegistryGameData> gameDataList)
		{
			const string EPIC_NAME = "Epic";
			const string EPIC_ITEMS_FOLDER = @"\Epic\EpicGamesLauncher\Data\Manifests";

			string dir = GetFolderPath(SpecialFolder.CommonApplicationData) + EPIC_ITEMS_FOLDER;
			if (!Directory.Exists(dir))
			{
				CLogger.LogInfo("{0} games not found in ProgramData.", EPIC_NAME.ToUpper());
				return;
			}
			string[] files = Directory.GetFiles(dir, "*.item", SearchOption.TopDirectoryOnly);
			CLogger.LogInfo("{0} {1} games found", files.Count(), EPIC_NAME.ToUpper());
			
			var options = new JsonDocumentOptions
			{
				AllowTrailingCommas = true
			};

			foreach (string file in files)
			{
				string strDocumentData = File.ReadAllText(file);

				if (string.IsNullOrEmpty(strDocumentData))
					continue;

				try
				{
					using (JsonDocument document = JsonDocument.Parse(@strDocumentData, options))
					{
						string strID = Path.GetFileName(file);
						string strTitle = GetStringProperty(document.RootElement, "DisplayName");
						CLogger.LogDebug($"- {strTitle}");
						string strLaunch = GetStringProperty(document.RootElement, "LaunchExecutable"); // DLCs won't have this set
						string strAlias = "";
						string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.Epic);

						if (!string.IsNullOrEmpty(strLaunch))
						{
							strLaunch = GetStringProperty(document.RootElement, "InstallLocation") + "\\" + strLaunch;
							strAlias = CRegScanner.GetAlias(GetStringProperty(document.RootElement, "MandatoryAppFolderName"));
							if (strAlias.Length > strTitle.Length)
								strAlias = CRegScanner.GetAlias(strTitle);
							if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
								strAlias = "";
							gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, true, strPlatform));
						}
					}
				}
				catch (Exception e)
				{
					CLogger.LogError(e, string.Format("ERROR: Malformed {0} file: {1}", EPIC_NAME.ToUpper(), file));
				}
			}
			CLogger.LogDebug("--------------------");
		}

		/// <summary>
		/// Find installed and owned Paradox games (latter from a json file)
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		public static void GetParadoxGames(List<CRegScanner.RegistryGameData> gameDataList)
		{
			List<string> dirs = new List<string>();
			const string PARADOX_NAME = "Paradox";
			const string PARADOX_REG = @"SOFTWARE\WOW6432Node\Paradox Interactive\Paradox Launcher\LauncherPath"; //HKLM32
			const string PARADOX_PATH = "Path";
			//const string PARADOX_UNREG = "{ED2CDA1D-39E4-4CBB-992C-5C1D08672128}"; //HKLM32
			const string PARADOX_JSON_FOLDER = @"\Paradox Interactive\launcher";

			// Get installed games
			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(PARADOX_REG, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				if (key == null)
					CLogger.LogInfo("{0} client not found in the registry.", PARADOX_NAME.ToUpper());
				else
				{
					string path = key.GetValue(PARADOX_PATH).ToString();

					try
					{
						if (!path.Equals(null) && Directory.Exists(path))
						{
							dirs.AddRange(Directory.GetDirectories(Directory.GetParent(Directory.GetParent(path).ToString()) + "\\games", "*.*", SearchOption.TopDirectoryOnly));
							foreach (string dir in dirs)
							{
								CultureInfo ci = new CultureInfo("en-GB");
								TextInfo ti = ci.TextInfo;

								string strID = Path.GetFileName(dir);
								string strTitle = "";
								string strLaunch = "";
								string strAlias = "";
								string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.Paradox);

								strTitle = ti.ToTitleCase(strID.Replace('_', ' '));
								CLogger.LogDebug($"- {strTitle}");
								strLaunch = CGameFinder.FindGameBinaryFile(dir, strTitle);
								strAlias = CRegScanner.GetAlias(strLaunch);
								if (strAlias.Length > strTitle.Length)
									strAlias = CRegScanner.GetAlias(strTitle);
								if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
									strAlias = "";
								if (!string.IsNullOrEmpty(strLaunch)) gameDataList.Add(
									new CRegScanner.RegistryGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, true, strPlatform));
							}

						}
					}
					catch (Exception e)
					{
						CLogger.LogError(e);
					}
				}
			}

			// Get not-installed games
			if (!(bool)CConfig.GetConfigBool(CConfig.CFG_INSTONLY))
			{
				string folder = GetFolderPath(SpecialFolder.LocalApplicationData) + PARADOX_JSON_FOLDER;
				if (!Directory.Exists(folder))
				{
					CLogger.LogInfo("{0} games not found in Local AppData.", PARADOX_NAME.ToUpper());
				}
				else
				{
					string[] files = Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly);

					var options = new JsonDocumentOptions
					{
						AllowTrailingCommas = true
					};

					foreach (string file in files)
					{
						if (file.EndsWith("_installableGames.json") && !(file.StartsWith("_noUser")))
						{
							string strDocumentData = File.ReadAllText(file);

							if (string.IsNullOrEmpty(strDocumentData))
								continue;

							CLogger.LogDebug("{0} not-installed games:", PARADOX_NAME.ToUpper());
							
							try
							{
								using (JsonDocument document = JsonDocument.Parse(@strDocumentData, options))
								{
									document.RootElement.TryGetProperty("content", out JsonElement content);
									if (!content.Equals(null))
									{
										foreach (JsonElement game in content.EnumerateArray())
										{
											game.TryGetProperty("_name", out JsonElement id);

											// Check if game is already installed
											bool found = false;
											foreach (string dir in dirs)
											{
												if (id.ToString().Equals(Path.GetFileName(dir)))
													found = true;
											}
											if (!found)
											{
												game.TryGetProperty("_displayName", out JsonElement title);
												game.TryGetProperty("_owned", out JsonElement owned);
												if (!id.Equals(null) && !title.Equals(null) && owned.ToString().ToLower().Equals("true"))
												{
													string strID = id.ToString();
													string strTitle = title.ToString();
													CLogger.LogDebug($"- *{strTitle}");
													string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.Paradox);
													gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, "", "", "", "", false, strPlatform));
												}
											}
										}
									}
								}
							}
							catch (Exception e)
							{
								CLogger.LogError(e, string.Format("ERROR: Malformed {0} file: {1}", PARADOX_NAME.ToUpper(), file));
							}
						}
					}
				}
			}
			CLogger.LogDebug("--------------------");
		}

		/// <summary>
		/// Find installed and owned Indiegala games (from json files)
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		public static void GetIGGames(List<CRegScanner.RegistryGameData> gameDataList)
		{
			const string IG_NAME			= "IGClient";
			const string IG_JSON_FILE		= @"\IGClient\storage\installed.json";
			const string IG_OWN_JSON_FILE	= @"\IGClient\config.json";

			// Get installed games
			string file = GetFolderPath(SpecialFolder.ApplicationData) + IG_JSON_FILE;
			if (!File.Exists(file))
			{
				CLogger.LogInfo("{0} installed games not found in AppData", IG_NAME.ToUpper());
				return;
			}
			else
			{
				var options = new JsonDocumentOptions
				{
					AllowTrailingCommas = true
				};

				string strDocumentData = File.ReadAllText(file);

				if (string.IsNullOrEmpty(strDocumentData))
					CLogger.LogWarn(string.Format("ERROR: Malformed {0} file: {1}", IG_NAME.ToUpper(), file));
				else
				{
					try
					{
						using (JsonDocument document = JsonDocument.Parse(@strDocumentData, options))
						{
							foreach (JsonElement element in document.RootElement.EnumerateArray())
							{
								string strID = "";
								string strTitle = "";
								string strLaunch = "";
								string strAlias = "";
								string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.IGClient);

								element.TryGetProperty("target", out JsonElement target);
								if (!target.Equals(null))
								{
									target.TryGetProperty("item_data", out JsonElement item);
									if (!item.Equals(null))
									{
										strID = GetStringProperty(item, "id_key_name");
										strTitle = GetStringProperty(item, "name");
									}
								}
								element.TryGetProperty("path", out JsonElement paths);
								if (!paths.Equals(null))
								{
									foreach (JsonElement path in paths.EnumerateArray())
										strLaunch = CGameFinder.FindGameBinaryFile(path.ToString(), strTitle);
								}

								CLogger.LogDebug($"- {strTitle}");

								if (!string.IsNullOrEmpty(strLaunch))
								{
									strAlias = CRegScanner.GetAlias(Path.GetFileNameWithoutExtension(strLaunch));
									if (strAlias.Length > strTitle.Length)
										strAlias = CRegScanner.GetAlias(strTitle);
									if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
										strAlias = "";
									gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, true, strPlatform));
								}
							}
						}
					}
					catch (Exception e)
					{
						CLogger.LogError(e, string.Format("ERROR: Malformed {0} file: {1}", IG_NAME.ToUpper(), file));
					}
				}
			}

			// Get not-installed games
			if (!(bool)CConfig.GetConfigBool(CConfig.CFG_INSTONLY))
			{
				file = GetFolderPath(SpecialFolder.ApplicationData) + IG_OWN_JSON_FILE;
				if (!File.Exists(file))
					CLogger.LogInfo("{0} not-installed games not found in AppData", IG_NAME.ToUpper());
				else
				{
					CLogger.LogDebug("{0} not-installed games:", IG_NAME.ToUpper());
					var options = new JsonDocumentOptions
					{
						AllowTrailingCommas = true
					};

					string strDocumentData = File.ReadAllText(file);

					if (string.IsNullOrEmpty(strDocumentData))
						CLogger.LogWarn(string.Format("ERROR: Malformed {0} file: {1}", IG_NAME.ToUpper(), file));
					else
					{
						try
						{
							using (JsonDocument document = JsonDocument.Parse(@strDocumentData, options))
							{
								bool found = false;
								JsonElement coll = new JsonElement();
								document.RootElement.TryGetProperty("gala_data", out JsonElement gData);
								if (!gData.Equals(null))
								{
									gData.TryGetProperty("data", out JsonElement data);
									if (!data.Equals(null))
									{
										data.TryGetProperty("showcase_content", out JsonElement sContent);
										if (!sContent.Equals(null))
										{
											sContent.TryGetProperty("content", out JsonElement content);
											if (!content.Equals(null))
											{
												content.TryGetProperty("user_collection", out coll);
												if (!coll.Equals(null))
													found = true;
											}
										}
									}
								}

								if (found)
								{
									foreach (JsonElement prod in coll.EnumerateArray())
									{
										string strID = GetStringProperty(prod, "prod_id_key_name");
										if (!string.IsNullOrEmpty(strID))
										{
											string strTitle = GetStringProperty(prod, "prod_name");
											//string strIconPath = GetStringProperty(prod, "prod_dev_image");  // TODO: Use prod_dev_image to download icon 
											CLogger.LogDebug($"- *{strTitle}");
											string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.IGClient);
											gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, "", "", "", "", false, strPlatform));
										}
									}
								}
							}
						}
						catch (Exception e)
						{
							CLogger.LogError(e, string.Format("ERROR: Malformed {0} file: {1}", IG_NAME.ToUpper(), file));
						}
					}
				}
			}
			CLogger.LogDebug("--------------------");
		}

		/// <summary>
		/// Find installed and owned Amazon games (from SQLite database)
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		public static void GetAmazonGames(List<CRegScanner.RegistryGameData> gameDataList, bool expensiveIcons)
		{
			const string NODE64_REG = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

			const string AMAZON_NAME = "Amazon";
			const string AMAZON_LAUNCH = "amazon-games://play/";
			const string AMAZON_DB = @"\Amazon Games\Data\Games\Sql\GameInstallInfo.sqlite";
			const string AMAZON_OWN_DB = @"\Amazon Games\Data\Games\Sql\GameProductInfo.sqlite";
			//const string AMAZON_UNINST_EXE = @"\__InstallData__\Amazon Game Remover.exe";
			//const string AMAZON_UNINST_SUFFIX = "-m Game -p";

			// Get installed games
			string db = GetFolderPath(SpecialFolder.LocalApplicationData) + AMAZON_DB;
			if (!File.Exists(db))
			{
				CLogger.LogInfo("{0} installed game database not found.", AMAZON_NAME.ToUpper());
				//return;
			}

			try
			{
				using (var con = new SQLiteConnection(string.Format($"Data Source={db}")))
				{
					con.Open();

					using (var cmd = new SQLiteCommand("SELECT Id, InstallDirectory, ProductTitle FROM DbSet;", con))
					{
						using (SQLiteDataReader rdr = cmd.ExecuteReader())
						{
							while (rdr.Read())
							{
								string strID = rdr.GetString(0);
								string strTitle = rdr.GetString(2);
								CLogger.LogDebug($"- {strTitle}");
								string strLaunch = AMAZON_LAUNCH + strID;
								string strIconPath = "";
								string strUninstall = "";

								using (RegistryKey key = Registry.CurrentUser.OpenSubKey(NODE64_REG + "\\AmazonGames/" + strTitle, RegistryKeyPermissionCheck.ReadSubTree))
								{
									if (key != null)
									{
										strIconPath = CRegScanner.GetRegStrVal(key, "DisplayIcon");
										strUninstall = CRegScanner.GetRegStrVal(key, "UninstallString");
									}
								}
								if (string.IsNullOrEmpty(strIconPath))
								{
									if (expensiveIcons)
										strIconPath = CGameFinder.FindGameBinaryFile(rdr.GetString(1), strTitle);
								}
								string strAlias = CRegScanner.GetAlias(strTitle);
								string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.Amazon);

								if (!string.IsNullOrEmpty(strLaunch))
								{
									if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
										strAlias = "";
									gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e, string.Format($"ERROR: Malformed {0} database output!", AMAZON_NAME.ToUpper()));
			}

			// Get not-installed games
			if (!(bool)CConfig.GetConfigBool(CConfig.CFG_INSTONLY))
			{
				db = GetFolderPath(SpecialFolder.LocalApplicationData) + AMAZON_OWN_DB;
				if (!File.Exists(db))
					CLogger.LogInfo("{0} not-installed game database not found.", AMAZON_NAME.ToUpper());
				else
				{
					CLogger.LogDebug("{0} not-installed games:", AMAZON_NAME.ToUpper());
					try
					{
						using (var con = new SQLiteConnection(string.Format($"Data Source={db}")))
						{
							con.Open();

							using (var cmd = new SQLiteCommand("SELECT Id, ProductIconUrl, ProductIdStr, ProductTitle FROM DbSet;", con))  // TODO: Use ProductIconUrl to download icon
							{
								using (SQLiteDataReader rdr = cmd.ExecuteReader())
								{
									while (rdr.Read())
									{
										string strID = rdr.GetString(2); // TODO: Should I use Id or ProductIdStr?
										string strTitle = rdr.GetString(3);
										CLogger.LogDebug($"- *{strTitle}");
										string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.Amazon);
										gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, "", "", "", "", false, strPlatform));
									}
								}
							}
						}
					}
					catch (Exception e)
					{
						CLogger.LogError(e, string.Format($"ERROR: Malformed {0} database output!", AMAZON_NAME.ToUpper()));
					}
				}
			}
			CLogger.LogDebug("-------------------");
		}

		/// <summary>
		/// Find installed and owned itch games (from SQLite database)
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		public static void GetItchGames(List<CRegScanner.RegistryGameData> gameDataList)
		{
			const string ITCH_NAME = "itch";
			const string ITCH_DB = @"\itch\db\butler.db";

			// Get installed games
			string db = GetFolderPath(SpecialFolder.ApplicationData) + ITCH_DB;
			if (!File.Exists(db))
			{
				CLogger.LogInfo("{0} database not found.", ITCH_NAME.ToUpper());
				return;
			}

			try
			{
				using (var con = new SQLiteConnection(string.Format($"Data Source={db}")))
				{
					con.Open();

					// Get both installed and not-installed games

					// TODO: Use still_cover_url, or cover_url if it doesn't exist, to download not-installed icons
					using (var cmd = new SQLiteCommand(string.Format("SELECT id, title, classification, cover_url, still_cover_url FROM games;"), con))
					using (SQLiteDataReader rdr = cmd.ExecuteReader())
					{
						while (rdr.Read())
						{
							if (!rdr.GetString(2).Equals("assets"))  // i.e., just "game" or "tool"
							{
								int id = rdr.GetInt32(0);
								string strID = string.Format($"itch_{id}");
								string strTitle = rdr.GetString(1);
								string strAlias = "";
								string strLaunch = "";
								string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.Itch);

								// SELECT path FROM install_locations;
								// SELECT install_folder FROM downloads;
								// SELECT verdict FROM caves;
								using (var cmd2 = new SQLiteCommand($"SELECT verdict, install_folder_name FROM caves WHERE game_id = {id};", con))
								using (SQLiteDataReader rdr2 = cmd2.ExecuteReader())
								{
									while (rdr2.Read())
									{
										string verdict = rdr2.GetString(0);
										strAlias = CRegScanner.GetAlias(strTitle);
										if (strAlias.Length > strTitle.Length)
											strAlias = CRegScanner.GetAlias(strTitle);
										if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
											strAlias = "";

										var options = new JsonDocumentOptions
										{
											AllowTrailingCommas = true
										};

										using (JsonDocument document = JsonDocument.Parse(@verdict, options))
										{
											string basePath = GetStringProperty(document.RootElement, "basePath");
											if (document.RootElement.TryGetProperty("candidates", out JsonElement candidates)) // 'candidates' object exists
											{
												if (!string.IsNullOrEmpty(candidates.ToString()))
												{
													foreach (JsonElement jElement in candidates.EnumerateArray())
													{
														strLaunch = string.Format("{0}\\{1}", basePath, GetStringProperty(jElement, "path"));
													}
												}
											}
											// Add installed games
											if (!string.IsNullOrEmpty(strLaunch))
											{
												CLogger.LogDebug($"- {strTitle}");
												gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, true, strPlatform));
											}
										}
									}
								}
								// Add not-installed games
								if (string.IsNullOrEmpty(strLaunch) && !(bool)CConfig.GetConfigBool(CConfig.CFG_INSTONLY))
								{
									CLogger.LogDebug($"- *{strTitle}");
									gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, "", "", "", "", false, strPlatform));
								}
							}
						}
					}
					con.Close();
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e, string.Format($"ERROR: Malformed {0} database output!", ITCH_NAME.ToUpper()));
			}
			CLogger.LogDebug("-------------------");
		}

		/// <summary>
		/// Import games from the json file and add them to the global game dictionary.
		/// <returns>True if successful, otherwise false</returns>
		/// </summary>
		private static bool ImportSearch(string file, ref Version version, ref List<CGameData.CMatch> matches)
		{
			CLogger.LogInfo("Importing search results from JSON...");
			var options = new JsonDocumentOptions
			{
				AllowTrailingCommas = true
			};

			string strDocumentData = File.ReadAllText(file);

			if (string.IsNullOrEmpty(strDocumentData))
				return false;

			try
			{
				using (JsonDocument document = JsonDocument.Parse(@strDocumentData, options))
				{
					System.Version.TryParse(GetStringProperty(document.RootElement, JSON_VERSION), out version);

					if (!document.RootElement.TryGetProperty(LAST_ARRAY, out JsonElement jArrSearch)) // 'matches' array does not exist
						return false;

					foreach (JsonElement jElement in jArrSearch.EnumerateArray())
					{
						string strTitle = GetStringProperty(jElement, LAST_ARRAY_TITLE);
						if (string.IsNullOrEmpty(strTitle))
							continue;

						int nIndex = GetIntProperty(jElement, LAST_ARRAY_INDEX);
						int nPercent = GetUShortProperty(jElement, LAST_ARRAY_PERCENT);
						matches.Add(new CGameData.CMatch(strTitle, nIndex, nPercent));
					}
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e, string.Format($"ERROR: Malformed {file} file!"));
				return false;
			}
			return true;
		}

		/// <summary>
		/// Import configuration items from the ini file and add them to the global configuration dictionary.
		/// <returns>True if successful, otherwise false</returns>
		/// </summary>
		private static bool ImportConfig(string file, ref CConfig.ConfigVolatile configv, ref CConfig.Hotkeys hotkeys, ref CConfig.Colours colours)
		{
			bool importError = false;
			bool translateError = false;
			Dictionary<string, string> configNew = new Dictionary<string, string>();
			//Version version = System.Version.Parse("0.0");

			CLogger.LogInfo("Importing configuration data from INI...");

			try
			{
				CIniParser ini = new CIniParser(file);

				System.Version.TryParse(ini.Read(INI_VERSION, VERSION_SECTION), out Version version);
				if (version >= MIN_CFG_VERSION)
				{
					foreach (KeyValuePair<string, string> setting in CConfig.config)
					{
						string strVal = ini.Read(setting.Key, CONFIG_SECTION);
						//CLogger.LogDebug("{0}={1}", setting.Key, strVal);
						if (string.IsNullOrEmpty(strVal))
							configNew.Add(setting.Key, CConfig.GetConfigDefault(setting.Key));
						else
							configNew.Add(setting.Key, strVal);
					}
					CConfig.config = new Dictionary<string, string>(configNew);
				}
				else
				{
					CLogger.LogWarn("ERROR: Outdated configuration file. Resetting defaults...");
					Console.WriteLine($"ERROR: Outdated {file} file. Resetting defaults...");
					CreateNewConfigFile(file);
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
				Console.WriteLine($"ERROR: Bad data in {file} file. Resetting defaults...");
				importError = true;
				SetConfigDefaults(true);
			}

			SetConfigVolatile(ref configv);
			translateError = !(TranslateConfig(ref hotkeys, ref colours));
			return !(importError || translateError);
		}

		private static bool SetConfigVolatile(ref CConfig.ConfigVolatile configv)
        {
			configv.dontSaveChanges = (bool)CConfig.GetConfigBool(CConfig.CFG_USEFILE);
			configv.typingInput = (bool)CConfig.GetConfigBool(CConfig.CFG_USETYPE);
			configv.listView = (bool)CConfig.GetConfigBool(CConfig.CFG_USELIST);
			configv.imageBorder = (bool)CConfig.GetConfigBool(CConfig.CFG_IMGBORD);
			configv.imageSize = (ushort)CConfig.GetConfigNum(CConfig.CFG_IMGSIZE);
			configv.iconSize = (ushort)CConfig.GetConfigNum(CConfig.CFG_ICONSIZE);
			return true;
        }

		/// <summary>
		/// Translate configuration strings into ConsoleKey and ConsoleColor enums
		/// <returns>True if successful, otherwise false</returns>
		/// </summary>
		private static bool TranslateConfig(ref CConfig.Hotkeys hotkeys, ref CConfig.Colours colours)
		{
			bool parseError = false;

			try
			{
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLBG1), true, out colours.bgCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLBG2), true, out colours.bgLtCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLTITLE1), true, out colours.titleCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLTITLE2), true, out colours.titleLtCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLSUB1), true, out colours.subCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLSUB2), true, out colours.subLtCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLENTRY1), true, out colours.entryCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLENTRY2), true, out colours.entryLtCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLUNIN1), true, out colours.uninstCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLUNIN2), true, out colours.uninstLtCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLHIBG1), true, out colours.highbgCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLHIBG2), true, out colours.highbgLtCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLHILITE1), true, out colours.highlightCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLHILITE2), true, out colours.highlightLtCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLINPBG1), true, out colours.inputbgCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLINPBG2), true, out colours.inputbgLtCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLINPUT1), true, out colours.inputCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLINPUT2), true, out colours.inputLtCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLERRBG1), true, out colours.errorCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLERRBG2), true, out colours.errorLtCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLERROR1), true, out colours.errorCC);
				Enum.TryParse<ConsoleColor>(CConfig.GetConfigString(CConfig.CFG_COLERROR2), true, out colours.errorLtCC);
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
				Console.WriteLine($"ERROR: Bad colour value. Resetting defaults...");
				parseError = true;
				SetConfigDefaults(false, false, false, false, true, false, false);
			}
			try
			{
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYLT1), true, out hotkeys.leftCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYLT2), true, out hotkeys.leftCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYUP1), true, out hotkeys.upCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYUP2), true, out hotkeys.upCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYRT1), true, out hotkeys.rightCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYRT2), true, out hotkeys.rightCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYDN1), true, out hotkeys.downCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYDN2), true, out hotkeys.downCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYSEL1), true, out hotkeys.selectCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYSEL2), true, out hotkeys.selectCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYQUIT1), true, out hotkeys.quitCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYQUIT2), true, out hotkeys.quitCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYSCAN1), true, out hotkeys.scanCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYSCAN2), true, out hotkeys.scanCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYHELP1), true, out hotkeys.helpCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYHELP2), true, out hotkeys.helpCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYBACK1), true, out hotkeys.backCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYBACK2), true, out hotkeys.backCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYPGUP1), true, out hotkeys.pageUpCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYPGUP2), true, out hotkeys.pageUpCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYPGDN1), true, out hotkeys.pageDownCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYPGDN2), true, out hotkeys.pageDownCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYHOME1), true, out hotkeys.firstCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYHOME2), true, out hotkeys.firstCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYEND1), true, out hotkeys.lastCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYEND2), true, out hotkeys.lastCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYPLAT1), true, out hotkeys.launcherCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYPLAT2), true, out hotkeys.launcherCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYCFG1), true, out hotkeys.settingsCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYCFG2), true, out hotkeys.settingsCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYFIND1), true, out hotkeys.searchCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYFIND2), true, out hotkeys.searchCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYTAB1), true, out hotkeys.completeCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYTAB2), true, out hotkeys.completeCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYESC1), true, out hotkeys.cancelCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYESC2), true, out hotkeys.cancelCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYNEW1), true, out hotkeys.newCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYNEW2), true, out hotkeys.newCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYDEL1), true, out hotkeys.deleteCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYDEL2), true, out hotkeys.deleteCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYUNIN1), true, out hotkeys.uninstCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYUNIN2), true, out hotkeys.uninstCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYCUT1), true, out hotkeys.shortcutCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYCUT2), true, out hotkeys.shortcutCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYHIDE1), true, out hotkeys.hideCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYHIDE2), true, out hotkeys.hideCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYFAVE1), true, out hotkeys.faveCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYFAVE2), true, out hotkeys.faveCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYALIAS1), true, out hotkeys.aliasCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYALIAS2), true, out hotkeys.aliasCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYTYPE1), true, out hotkeys.typeCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYTYPE2), true, out hotkeys.typeCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYVIEW1), true, out hotkeys.viewCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYVIEW2), true, out hotkeys.viewCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYMODE1), true, out hotkeys.modeCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYMODE2), true, out hotkeys.modeCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYIMG1), true, out hotkeys.imageCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYIMG2), true, out hotkeys.imageCK2);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYSORT1), true, out hotkeys.sortCK1);
				Enum.TryParse<ConsoleKey>(CConfig.GetConfigString(CConfig.CFG_KEYSORT2), true, out hotkeys.sortCK2);
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
				Console.WriteLine($"ERROR: Bad hotkey value. Resetting defaults...");
				parseError = true;
				SetConfigDefaults(false, false, false, false, false, true, false);
			}
			return !parseError;
		}

		/// <summary>
		/// Create a game JSON object and add write it with JsonWriter
		/// </summary>
		/// <param name="writer">JsonWriter object</param>
		/// <param name="stream">MemoryStream object</param>
		/// <param name="options">JsonWriter options struct</param>
		/// <param name="data">Game data</param>
		private static void WriteGame(Utf8JsonWriter writer, CGameData.CGame data) //, MemoryStream stream, JsonWriterOptions options, 
		{
			writer.WriteStartObject();
			writer.WriteString(GAMES_ARRAY_ID			, data.ID);
			writer.WriteString(GAMES_ARRAY_TITLE		, data.Title);
			writer.WriteString(GAMES_ARRAY_LAUNCH		, data.Launch);
			writer.WriteString(GAMES_ARRAY_ICON			, data.Icon);
			writer.WriteString(GAMES_ARRAY_UNINSTALLER	, data.Uninstaller);
			writer.WriteString(GAMES_ARRAY_PLATFORM		, data.PlatformString);
			writer.WriteBoolean(GAMES_ARRAY_INSTALLED	, data.IsInstalled);
			writer.WriteBoolean(GAMES_ARRAY_FAVOURITE	, data.IsFavourite);
			writer.WriteBoolean(GAMES_ARRAY_NEW			, data.IsNew);
			writer.WriteBoolean(GAMES_ARRAY_HIDDEN		, data.IsHidden);
			writer.WriteString(GAMES_ARRAY_ALIAS		, data.Alias);
			writer.WriteNumber(GAMES_ARRAY_FREQUENCY	, data.Frequency);
			writer.WriteEndObject();
		}

		/// <summary>
		/// Create a search JSON object and add write it with JsonWriter
		/// </summary>
		/// <param name="writer">JsonWriter object</param>
		/// <param name="stream">MemoryStream object</param>
		/// <param name="options">JsonWriter options struct</param>
		/// <param name="data">Match data</param>
		private static void WriteSearch(Utf8JsonWriter writer, CGameData.CMatch data) //, MemoryStream stream, JsonWriterOptions options, 
		{
			writer.WriteStartObject();
			writer.WriteString(LAST_ARRAY_TITLE		, data.m_strTitle);
			writer.WriteNumber(LAST_ARRAY_INDEX		, data.m_nIndex);
			writer.WriteNumber(LAST_ARRAY_PERCENT	, data.m_nPercent);
			writer.WriteEndObject();
		}

		/// <summary>
		/// Retrieve string representing a ConsoleColor value from the JSON element
		/// </summary>
		/// <param name="strPropertyName">Name of the property</param>
		/// <param name="jElement">Source JSON element</param>
		/// <returns>Value of the property as a string</returns>
		private static string GetColourProperty(JsonElement jElement, string strPropertyName)
		{
			string strElement = "";
			try
			{
				if (jElement.TryGetProperty(strPropertyName, out JsonElement jValue))
					strElement = CConfig.ToConsoleColorFormat(jValue.GetString());
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
			}
			return strElement;
		}

		/// <summary>
		/// Retrieve string representing a ConsoleKey value from the JSON element
		/// </summary>
		/// <param name="strPropertyName">Name of the property</param>
		/// <param name="jElement">Source JSON element</param>
		/// <returns>Value of the property as a string</returns>
		private static string GetHotkeyProperty(JsonElement jElement, string strPropertyName)
		{
			string strElement = "";
			try
			{
				if (jElement.TryGetProperty(strPropertyName, out JsonElement jValue))
					strElement = CConfig.ToConsoleKeyFormat(jValue.GetString());
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
			}
			return strElement;
		}

		/// <summary>
		/// Retrieve string value from the JSON element
		/// </summary>
		/// <param name="strPropertyName">Name of the property</param>
		/// <param name="jElement">Source JSON element</param>
		/// <returns>Value of the property as a string or empty string if not found</returns>
		private static string GetStringProperty(JsonElement jElement, string strPropertyName)
		{
			try
			{
				if (jElement.TryGetProperty(strPropertyName, out JsonElement jValue))
					return jValue.GetString();
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
			}
			return "";
		}

		/// <summary>
		/// Retrieve boolean value from the JSON element
		/// </summary>
		/// <param name="strPropertyName">Name of the property</param>
		/// <param name="jElement">Source JSON element</param>
		/// <returns>Value of the property as a boolean or false if not found</returns>
		private static bool GetBoolProperty(JsonElement jElement, string strPropertyName)
		{
			try
			{
				if (jElement.TryGetProperty(strPropertyName, out JsonElement jValue))
				{
					return jValue.GetBoolean();
					/*
					if (jValue.GetString() == "1" ||
						jValue.GetString()[0].ToString().Equals("t", StringComparison.OrdinalIgnoreCase) ||
						jValue.GetString()[0].ToString().Equals("y", StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
					*/
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
			}
			return false;
		}

		/// <summary>
		/// Retrieve integer value from the JSON element
		/// </summary>
		/// <param name="strPropertyName">Name of the property</param>
		/// <param name="jElement">Source JSON element</param>
		/// <returns>Value of the property as an int or 0 if not found</returns>
		private static int GetIntProperty(JsonElement jElement, string strPropertyName)
		{
			try
			{
				if (jElement.TryGetProperty(strPropertyName, out JsonElement jValue))
				{
					if (jValue.TryGetInt32(out int nOut)) return (int)nOut;
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
			}
			return 0;
		}

		/// <summary>
		/// Retrieve unsigned long value from the JSON element
		/// </summary>
		/// <param name="strPropertyName">Name of the property</param>
		/// <param name="jElement">Source JSON element</param>
		/// <returns>Value of the property as a ulong or 0 if not found</returns>
		private static ulong GetULongProperty(JsonElement jElement, string strPropertyName)
		{
			try
			{
				if (jElement.TryGetProperty(strPropertyName, out JsonElement jValue))
				{
					if (jValue.TryGetUInt64(out ulong nOut)) return (ulong)nOut;
				}
			}
			catch (Exception e)
            {
				CLogger.LogError(e);
            }
			return 0;
		}

		/// <summary>
		/// Retrieve unsigned short value from the JSON element
		/// </summary>
		/// <param name="strPropertyName">Name of the property</param>
		/// <param name="jElement">Source JSON element</param>
		/// <returns>Value of the property as an unsigned short or 0 if not found</returns>
		private static ushort GetUShortProperty(JsonElement jElement, string strPropertyName)
		{
			try
			{
				if (jElement.TryGetProperty(strPropertyName, out JsonElement jValue))
				{
					if (jValue.TryGetUInt16(out ushort nOut)) return (ushort)nOut;
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
			}
			return 0;
		}

		/// <summary>
		/// Retrieve double value from the JSON element
		/// </summary>
		/// <param name="strPropertyName">Name of the property</param>
		/// <param name="jElement">Source JSON element</param>
		/// <returns>Value of the property as a double or 0f if not found</returns>
		private static double GetDoubleProperty(JsonElement jElement, string strPropertyName)
		{
			try
			{
				if (jElement.TryGetProperty(strPropertyName, out JsonElement jValue))
				{
					if (jValue.TryGetDouble(out double fOut)) return (double)fOut;
				}
			}
			catch (Exception e)
            {
				CLogger.LogError(e);
            }
			return 0f;
		}

		/// <summary>
		/// Set the default configuration values
		/// </summary>
		/// <param name="boolOnly">Only affects boolean values, and override with defaults</param>
		/// <param name="numOnly">Only affects number values, and override with defaults</param>
		/// <param name="colourOnly">Only affects colour values, and override with defaults</param>
		/// <param name="keyOnly">Only affects hotkey values, and override with defaults</param>
		/// <param name="textOnly">Only affects text values, and override with defaults</param>
		private static void SetConfigDefaults(bool forceAll, bool boolOnly, bool numOnly, bool longOnly, bool colourOnly, bool keyOnly, bool textOnly)
		{
			if (forceAll)
            {
				SetBoolDefaults(true);
				SetNumberDefaults(true);
				SetLongDefaults(true);
				SetColourDefaults(true);
				SetKeyDefaults(true);
				SetTextDefaults(true);
			}
			else if (boolOnly)
				SetBoolDefaults(true);
			else if (numOnly)
				SetNumberDefaults(true);
			else if (longOnly)
				SetLongDefaults(true);
			else if (colourOnly)
				SetColourDefaults(true);
			else if (keyOnly)
				SetKeyDefaults(true);
			else if (textOnly)
				SetTextDefaults(true);
			else
            {
				SetBoolDefaults(false);
				SetNumberDefaults(false);
				SetLongDefaults(false);
				SetColourDefaults(false);
				SetKeyDefaults(false);
				SetTextDefaults(false);
			}
		}

		/// <summary>
		/// Set the default configuration values
		/// </summary>
		private static void SetConfigDefaults(bool forceAll)
		{
			SetConfigDefaults(forceAll, false, false, false, false, false, false);
        }

		/// <summary>
		/// Set the default boolean configuration values
		/// </summary>
		private static void SetBoolDefaults(bool force)
        {
			SetDefaultVal(CConfig.CFG_NOQUIT, force);
			SetDefaultVal(CConfig.CFG_USEFILE, force);
			SetDefaultVal(CConfig.CFG_USESCAN, force);
			SetDefaultVal(CConfig.CFG_USECMD, force);
			SetDefaultVal(CConfig.CFG_USETYPE, force);
			SetDefaultVal(CConfig.CFG_USELIST, force);
			SetDefaultVal(CConfig.CFG_NOPAGE, force);
			SetDefaultVal(CConfig.CFG_USESIZE, force);
			SetDefaultVal(CConfig.CFG_USEALPH, force);
			SetDefaultVal(CConfig.CFG_USEFAVE, force);
			SetDefaultVal(CConfig.CFG_USEINST, force);
			SetDefaultVal(CConfig.CFG_USELITE, force);
			SetDefaultVal(CConfig.CFG_USEALL, force);
			SetDefaultVal(CConfig.CFG_NOCFG, force);
			SetDefaultVal(CConfig.CFG_INSTONLY, force);
			SetDefaultVal(CConfig.CFG_USECUST, force);
			SetDefaultVal(CConfig.CFG_USETEXT, force);
			SetDefaultVal(CConfig.CFG_IMGBORD, force);
			SetDefaultVal(CConfig.CFG_IMGCUST, force);
			SetDefaultVal(CConfig.CFG_IMGRTIO, force);
			SetDefaultVal(CConfig.CFG_IMGBGLEG, force);
			SetDefaultVal(CConfig.CFG_IMGSCAN, force);
		}
		
		/// <summary>
		/// Set the default number configuration values
		/// </summary>
		private static void SetNumberDefaults(bool force)
		{
			SetDefaultVal(CConfig.CFG_ICONSIZE, force);
			SetDefaultVal(CConfig.CFG_ICONRES, force);
			SetDefaultVal(CConfig.CFG_IMGSIZE, force);
			SetDefaultVal(CConfig.CFG_IMGRES, force);
			SetDefaultVal(CConfig.CFG_IMGPOS, force);
			SetDefaultVal(CConfig.CFG_COLSIZE, force);
		}

		/// <summary>
		/// Set the default unsigned long integer configuration values
		/// </summary>
		private static void SetLongDefaults(bool force)
		{
			SetDefaultVal(CConfig.CFG_STEAMID, force);
		}

		/// <summary>
		/// Set the default colour configuration values
		/// </summary>
		private static void SetColourDefaults(bool force)
		{
			SetDefaultVal(CConfig.CFG_COLBG1, force);
			SetDefaultVal(CConfig.CFG_COLBG2, force);
			SetDefaultVal(CConfig.CFG_COLTITLE1, force);
			SetDefaultVal(CConfig.CFG_COLTITLE2, force);
			SetDefaultVal(CConfig.CFG_COLSUB1, force);
			SetDefaultVal(CConfig.CFG_COLSUB2, force);
			SetDefaultVal(CConfig.CFG_COLENTRY1, force);
			SetDefaultVal(CConfig.CFG_COLENTRY2, force);
			SetDefaultVal(CConfig.CFG_COLUNIN1, force);
			SetDefaultVal(CConfig.CFG_COLUNIN2, force);
			SetDefaultVal(CConfig.CFG_COLHIBG1, force);
			SetDefaultVal(CConfig.CFG_COLHIBG2, force);
			SetDefaultVal(CConfig.CFG_COLHILITE1, force);
			SetDefaultVal(CConfig.CFG_COLHILITE2, force);
			SetDefaultVal(CConfig.CFG_COLINPBG1, force);
			SetDefaultVal(CConfig.CFG_COLINPBG2, force);
			SetDefaultVal(CConfig.CFG_COLINPUT1, force);
			SetDefaultVal(CConfig.CFG_COLINPUT2, force);
			SetDefaultVal(CConfig.CFG_COLERRBG1, force);
			SetDefaultVal(CConfig.CFG_COLERRBG2, force);
			SetDefaultVal(CConfig.CFG_COLERROR1, force);
			SetDefaultVal(CConfig.CFG_COLERROR2, force);
		}

		/// <summary>
		/// Set the default colour configuration values
		/// </summary>
		private static void SetKeyDefaults(bool force)
		{
			SetDefaultVal(CConfig.CFG_KEYLT1, force);
			SetDefaultVal(CConfig.CFG_KEYLT2, force);
			SetDefaultVal(CConfig.CFG_KEYUP1, force);
			SetDefaultVal(CConfig.CFG_KEYUP2, force);
			SetDefaultVal(CConfig.CFG_KEYRT1, force);
			SetDefaultVal(CConfig.CFG_KEYRT2, force);
			SetDefaultVal(CConfig.CFG_KEYDN1, force);
			SetDefaultVal(CConfig.CFG_KEYDN2, force);
			SetDefaultVal(CConfig.CFG_KEYSEL1, force);
			SetDefaultVal(CConfig.CFG_KEYSEL2, force);
			SetDefaultVal(CConfig.CFG_KEYQUIT1, force);
			SetDefaultVal(CConfig.CFG_KEYQUIT2, force);
			SetDefaultVal(CConfig.CFG_KEYSCAN1, force);
			SetDefaultVal(CConfig.CFG_KEYSCAN2, force);
			SetDefaultVal(CConfig.CFG_KEYHELP1, force);
			SetDefaultVal(CConfig.CFG_KEYHELP2, force);
			SetDefaultVal(CConfig.CFG_KEYBACK1, force);
			SetDefaultVal(CConfig.CFG_KEYBACK2, force);
			SetDefaultVal(CConfig.CFG_KEYPGUP1, force);
			SetDefaultVal(CConfig.CFG_KEYPGUP2, force);
			SetDefaultVal(CConfig.CFG_KEYPGDN1, force);
			SetDefaultVal(CConfig.CFG_KEYPGDN2, force);
			SetDefaultVal(CConfig.CFG_KEYHOME1, force);
			SetDefaultVal(CConfig.CFG_KEYHOME2, force);
			SetDefaultVal(CConfig.CFG_KEYEND1, force);
			SetDefaultVal(CConfig.CFG_KEYEND2, force);
			SetDefaultVal(CConfig.CFG_KEYCFG1, force);
			SetDefaultVal(CConfig.CFG_KEYCFG2, force);
			SetDefaultVal(CConfig.CFG_KEYFIND1, force);
			SetDefaultVal(CConfig.CFG_KEYFIND2, force);
			SetDefaultVal(CConfig.CFG_KEYTAB1, force);
			SetDefaultVal(CConfig.CFG_KEYTAB2, force);
			SetDefaultVal(CConfig.CFG_KEYESC1, force);
			SetDefaultVal(CConfig.CFG_KEYESC2, force);
			SetDefaultVal(CConfig.CFG_KEYNEW1, force);
			SetDefaultVal(CConfig.CFG_KEYNEW2, force);
			SetDefaultVal(CConfig.CFG_KEYDEL1, force);
			SetDefaultVal(CConfig.CFG_KEYDEL2, force);
			SetDefaultVal(CConfig.CFG_KEYUNIN1, force);
			SetDefaultVal(CConfig.CFG_KEYUNIN2, force);
			SetDefaultVal(CConfig.CFG_KEYCUT1, force);
			SetDefaultVal(CConfig.CFG_KEYCUT2, force);
			SetDefaultVal(CConfig.CFG_KEYHIDE1, force);
			SetDefaultVal(CConfig.CFG_KEYHIDE2, force);
			SetDefaultVal(CConfig.CFG_KEYFAVE1, force);
			SetDefaultVal(CConfig.CFG_KEYFAVE2, force);
			SetDefaultVal(CConfig.CFG_KEYALIAS1, force);
			SetDefaultVal(CConfig.CFG_KEYALIAS2, force);
			SetDefaultVal(CConfig.CFG_KEYTYPE1, force);
			SetDefaultVal(CConfig.CFG_KEYTYPE2, force);
			SetDefaultVal(CConfig.CFG_KEYVIEW1, force);
			SetDefaultVal(CConfig.CFG_KEYVIEW2, force);
			SetDefaultVal(CConfig.CFG_KEYMODE1, force);
			SetDefaultVal(CConfig.CFG_KEYMODE2, force);
			SetDefaultVal(CConfig.CFG_KEYIMG1, force);
			SetDefaultVal(CConfig.CFG_KEYIMG2, force);
			SetDefaultVal(CConfig.CFG_KEYSORT1, force);
			SetDefaultVal(CConfig.CFG_KEYSORT2, force);
		}

		/// <summary>
		/// Set the default text configuration values
		/// </summary>
		private static void SetTextDefaults(bool force)
		{
			SetDefaultVal(CConfig.CFG_TXTMAINT, force);
			SetDefaultVal(CConfig.CFG_TXTCFGT, force);
			SetDefaultVal(CConfig.CFG_TXTFILET, force);
			SetDefaultVal(CConfig.CFG_TXTROOT, force);
			SetDefaultVal(CConfig.CFG_TXTSELECT, force);
			SetDefaultVal(CConfig.CFG_TXTCREATE, force);
			SetDefaultVal(CConfig.CFG_TXTNMAIN1, force);
			SetDefaultVal(CConfig.CFG_TXTNMAIN2, force);
			SetDefaultVal(CConfig.CFG_TXTNSUB1, force);
			SetDefaultVal(CConfig.CFG_TXTNSUB2, force);
			SetDefaultVal(CConfig.CFG_TXTIMAIN1, force);
			SetDefaultVal(CConfig.CFG_TXTIMAIN2, force);
			SetDefaultVal(CConfig.CFG_TXTISUB1, force);
			SetDefaultVal(CConfig.CFG_TXTISUB2, force);
		}

		/// <summary>
		/// Set the default configuration value if a value doesn't already exist
		/// </summary>
		/// <param name="value">The current value</param>
		/// <param name="default">The default</param>
		private static void SetDefaultVal(string property, bool force)
        {
            if (force)
                CConfig.SetConfigDefault(property);
            else if (CConfig.config.TryGetValue(property, out string value))
            {
                if (string.IsNullOrEmpty(value))
                    CConfig.SetConfigDefault(property);
            }
            return;
		}

		/*
		/// <summary>
		/// Decompress gzip file (for itch) [no longer necessary after moving to SQLite method]
		/// </summary>
		/// <param name="fileToDecompress"></param>
		public static void Decompress(FileInfo fileToDecompress)
		{
			using (FileStream originalFileStream = fileToDecompress.OpenRead())
			{
				string currentFileName = fileToDecompress.FullName;
				string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

				using (FileStream decompressedFileStream = File.Create(newFileName))
				using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
					decompressionStream.CopyTo(decompressedFileStream);
			}
		}
		*/
	}
}
