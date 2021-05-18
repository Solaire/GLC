using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
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
		private static readonly Version MIN_CFG_VERSION		= System.Version.Parse("1.1.0");
		private static readonly Version MIN_LAST_VERSION	= System.Version.Parse("1.1.0");
		private static readonly Version MIN_GAME_VERSION	= System.Version.Parse("1.1.0");

		// .json filenames (they should be in the same directory as the executable)
		private static readonly string CFG_JSON_FILE		= CDock.FILENAME + "-cfg.json";
		private static readonly string LAST_JSON_FILE		= CDock.FILENAME + "-last.json";
		private static readonly string GAME_JSON_FILE		= CDock.FILENAME + "-games.json";
		private const string GAMEOLD_JSON_FILE				= "games.json";

		// JSON field names

		private const string JSON_VERSION	= "json_version";
		
		// games json
		private const string GAMES_ARRAY				= "games";
		private const string GAMES_ARRAY_ID				= "id";
		private const string GAMES_ARRAY_TITLE			= "title";
		private const string GAMES_ARRAY_LAUNCH			= "launch";
		private const string GAMES_ARRAY_ICON			= "icon";
		private const string GAMES_ARRAY_UNINSTALLER	= "uninstaller";
		private const string GAMES_ARRAY_PLATFORM		= "platform";
		private const string GAMES_ARRAY_FAVOURITE		= "favourite";
		private const string GAMES_ARRAY_HIDDEN			= "hidden";
		private const string GAMES_ARRAY_ALIAS			= "alias";
		private const string GAMES_ARRAY_FREQUENCY		= "frequency";

		// search json (for command line non-interactive interactions)
		private const string LAST_ARRAY			= "matches";
		private const string LAST_ARRAY_INDEX	= "index";
		private const string LAST_ARRAY_TITLE	= "title";
		private const string LAST_ARRAY_PERCENT	= "percent";

		private static readonly string currentPath	= Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
		private static readonly string configPath	= currentPath + "\\" + CFG_JSON_FILE;
		private static readonly string searchPath	= currentPath + "\\" + LAST_JSON_FILE;
		private static readonly string gamesPath	= currentPath + "\\" + GAME_JSON_FILE;
		private static readonly string gamesOldPath	= currentPath + "\\" + GAMEOLD_JSON_FILE;
		private static readonly string version		= Assembly.GetEntryAssembly().GetName().Version.ToString();

		/// <summary>
		/// Import games from the games json config file
		/// </summary>
		/// <returns>True if successful, otherwise false</returns>
		public static bool ImportFromJSON(out CConfig.Configuration config, out CConfig.Hotkeys keys, out CConfig.Colours cols, out List<CGameData.CMatch> matches)
		{
			Version verConfig = System.Version.Parse("0.0");
			config = new CConfig.Configuration();
			keys = new CConfig.Hotkeys();
			cols = new CConfig.Colours();
			matches = new List<CGameData.CMatch>();
			bool parseError = false;

			// Configuration data
			if (!File.Exists(configPath))
			{
				CLogger.LogInfo("{0} is missing. Creating new defaults...", CFG_JSON_FILE);
				CreateNewConfigFile(configPath);
			}
			if (!ImportConfig(configPath, ref verConfig, ref config, ref keys, ref cols, out bool bCfgReset))
				parseError = true;

			if (bCfgReset)
			{
				CLogger.LogWarn("{0} is empty or corrupt. Creating new defaults...", CFG_JSON_FILE);
				CreateNewConfigFile(configPath);
			}
			else if (verConfig < MIN_CFG_VERSION)
            {
				CLogger.LogWarn("{0} is outdated. Creating new defaults...", CFG_JSON_FILE);
				CreateNewConfigFile(configPath);
			}

			// Previous search matches
			Version verSearch = System.Version.Parse("0.0");
			if (File.Exists(searchPath))
			{
				CLogger.LogInfo("{0} was found.", LAST_JSON_FILE);
				ImportSearch(searchPath, ref verSearch, ref matches);
			}
			if (verSearch < MIN_LAST_VERSION)
				matches = new List<CGameData.CMatch>();

			// Game data
			int nGameCount = 0;
			Version verGames = System.Version.Parse("0.0");

			if (!File.Exists(gamesPath))
			{
				if (File.Exists(gamesOldPath))
				{
					File.Delete(gamesOldPath);
					CLogger.LogWarn("{0} is outdated. Creating new {1}...", GAMEOLD_JSON_FILE, GAME_JSON_FILE);
				}
				CreateNewGamesFile(gamesPath);
			}
			else if (!(bool)config.alwaysScan)
			{
				if (!ImportGames(gamesPath, ref nGameCount, ref verGames, (bool)config.alphaSort, (bool)config.faveSort, true))
					parseError = true;
			}

			if (nGameCount < 1)
			{
				CLogger.LogInfo("{0} is empty, corrupt, or outdated. Scanning for games...", GAME_JSON_FILE);
				Console.Write("Scanning for games");  // ScanGames() will add dots for each platform
				CRegScanner.ScanGames((bool)config.onlyCustom, config.imageSize > 0 || config.iconSize > 0);
			}

			if (parseError)
			{
				Console.Write("Press any key to continue...");
				Console.ReadKey();
				Console.WriteLine();
			}
			return true;
		}

		/// <summary>
		/// Export game data from memory to the games json file
		/// NOTE: At the moment, the program will pretty pretty much create a brand new JSON file and override all of the content...
		/// ... I need to find a nice workaround as JsonDocument class is read-only.
		/// </summary>
		/// <returns>True is successful, otherwise false</returns>
		public static bool ExportGames(List<CGameData.CGame> gameList)
		{
			CLogger.LogInfo("Save game data to JSON...");
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
			CLogger.LogInfo("Save search data to JSON...");
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
		public static bool ExportConfig(CConfig.Configuration config)
		{
			CLogger.LogInfo("Save configuration data to JSON...");
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
						writer.WriteBoolean(CConfig.CFG_NOQUIT, (bool)config.preventQuit);
						writer.WriteBoolean(CConfig.CFG_USEFILE, (bool)config.ignoreChanges);
						writer.WriteBoolean(CConfig.CFG_USESCAN, (bool)config.alwaysScan);
						writer.WriteBoolean(CConfig.CFG_USECMD, (bool)config.onlyCmdLine);
						writer.WriteBoolean(CConfig.CFG_USETYPE, (bool)config.typeInput);
						writer.WriteBoolean(CConfig.CFG_USELIST, (bool)config.listOutput);
						writer.WriteBoolean(CConfig.CFG_NOPAGE, (bool)config.noPageSplit);
						writer.WriteBoolean(CConfig.CFG_USESIZE, (bool)config.sizeToFit);
						writer.WriteBoolean(CConfig.CFG_USEALPH, (bool)config.alphaSort);
						writer.WriteBoolean(CConfig.CFG_USEFAVE, (bool)config.faveSort);
						writer.WriteBoolean(CConfig.CFG_USELITE, (bool)config.lightMode);
						writer.WriteBoolean(CConfig.CFG_USEALL, (bool)config.goToAll);
						writer.WriteBoolean(CConfig.CFG_NOCFG, (bool)config.hideSettings);
						writer.WriteBoolean(CConfig.CFG_USECUST, (bool)config.onlyCustom);
						writer.WriteBoolean(CConfig.CFG_USETEXT, (bool)config.msgCustom);
						writer.WriteBoolean(CConfig.CFG_IMGBORD, (bool)config.imageBorder);
						writer.WriteBoolean(CConfig.CFG_IMGCUST, (bool)config.noImageCustom);
						writer.WriteBoolean(CConfig.CFG_IMGRTIO, (bool)config.imageIgnoreRatio);
						writer.WriteNumber(CConfig.CFG_ICONSIZE, (int)config.iconSize);
						writer.WriteNumber(CConfig.CFG_ICONRES, (int)config.iconRes);
						writer.WriteNumber(CConfig.CFG_IMGSIZE, (int)config.imageSize);
						writer.WriteNumber(CConfig.CFG_IMGRES, (int)config.imageRes);
						writer.WriteNumber(CConfig.CFG_IMGPOS, (int)config.imagePosition);
						writer.WriteNumber(CConfig.CFG_COLSIZE, (int)config.columnSize);
						writer.WriteString(CConfig.CFG_COLBG1, config.bgCol);
						writer.WriteString(CConfig.CFG_COLBG2, config.bgLtCol);
						writer.WriteString(CConfig.CFG_COLTITLE1, config.titleCol);
						writer.WriteString(CConfig.CFG_COLTITLE2, config.titleLtCol);
						writer.WriteString(CConfig.CFG_COLSUB1, config.subCol);
						writer.WriteString(CConfig.CFG_COLSUB2, config.subLtCol);
						writer.WriteString(CConfig.CFG_COLENTRY1, config.entryCol);
						writer.WriteString(CConfig.CFG_COLENTRY2, config.entryLtCol);
						writer.WriteString(CConfig.CFG_COLHIBG1, config.highbgCol);
						writer.WriteString(CConfig.CFG_COLHIBG2, config.highbgLtCol);
						writer.WriteString(CConfig.CFG_COLHIFG1, config.highlightCol);
						writer.WriteString(CConfig.CFG_COLHIFG2, config.highlightLtCol);
						writer.WriteString(CConfig.CFG_COLERRBG1, config.errbgCol);
						writer.WriteString(CConfig.CFG_COLERRBG2, config.errbgLtCol);
						writer.WriteString(CConfig.CFG_COLERRFG1, config.errorCol);
						writer.WriteString(CConfig.CFG_COLERRFG2, config.errorLtCol);
						writer.WriteString(CConfig.CFG_KEYLT1, config.leftKey1);
						writer.WriteString(CConfig.CFG_KEYLT2, config.leftKey2);
						writer.WriteString(CConfig.CFG_KEYUP1, config.upKey1);
						writer.WriteString(CConfig.CFG_KEYUP2, config.upKey2);
						writer.WriteString(CConfig.CFG_KEYRT1, config.rightKey1);
						writer.WriteString(CConfig.CFG_KEYRT2, config.rightKey2);
						writer.WriteString(CConfig.CFG_KEYDN1, config.downKey1);
						writer.WriteString(CConfig.CFG_KEYDN2, config.downKey2);
						writer.WriteString(CConfig.CFG_KEYSEL1, config.selectKey1);
						writer.WriteString(CConfig.CFG_KEYSEL2, config.selectKey2);
						writer.WriteString(CConfig.CFG_KEYQUIT1, config.quitKey1);
						writer.WriteString(CConfig.CFG_KEYQUIT2, config.quitKey2);
						writer.WriteString(CConfig.CFG_KEYSCAN1, config.scanKey1);
						writer.WriteString(CConfig.CFG_KEYSCAN2, config.scanKey2);
						writer.WriteString(CConfig.CFG_KEYHELP1, config.helpKey1);
						writer.WriteString(CConfig.CFG_KEYHELP2, config.helpKey2);
						writer.WriteString(CConfig.CFG_KEYBACK1, config.backKey1);
						writer.WriteString(CConfig.CFG_KEYBACK2, config.backKey2);
						writer.WriteString(CConfig.CFG_KEYPGUP1, config.pageUpKey1);
						writer.WriteString(CConfig.CFG_KEYPGUP2, config.pageUpKey2);
						writer.WriteString(CConfig.CFG_KEYPGDN1, config.pageDownKey1);
						writer.WriteString(CConfig.CFG_KEYPGDN2, config.pageDownKey2);
						writer.WriteString(CConfig.CFG_KEYHOME1, config.firstKey1);
						writer.WriteString(CConfig.CFG_KEYHOME2, config.firstKey2);
						writer.WriteString(CConfig.CFG_KEYEND1, config.lastKey1);
						writer.WriteString(CConfig.CFG_KEYEND2, config.lastKey2);
						writer.WriteString(CConfig.CFG_KEYFIND1, config.searchKey1);
						writer.WriteString(CConfig.CFG_KEYFIND2, config.searchKey2);
						writer.WriteString(CConfig.CFG_KEYTAB1, config.completeKey1);
						writer.WriteString(CConfig.CFG_KEYTAB2, config.completeKey2);
						writer.WriteString(CConfig.CFG_KEYESC1, config.cancelKey1);
						writer.WriteString(CConfig.CFG_KEYESC2, config.cancelKey2);
						writer.WriteString(CConfig.CFG_KEYUNIN1, config.uninstKey1);
						writer.WriteString(CConfig.CFG_KEYUNIN2, config.uninstKey2);
						writer.WriteString(CConfig.CFG_KEYDESK1, config.desktopKey1);
						writer.WriteString(CConfig.CFG_KEYDESK2, config.desktopKey2);
						writer.WriteString(CConfig.CFG_KEYHIDE1, config.hideKey1);
						writer.WriteString(CConfig.CFG_KEYHIDE2, config.hideKey2);
						writer.WriteString(CConfig.CFG_KEYFAVE1, config.faveKey1);
						writer.WriteString(CConfig.CFG_KEYFAVE2, config.faveKey2);
						writer.WriteString(CConfig.CFG_KEYALIAS1, config.aliasKey1);
						writer.WriteString(CConfig.CFG_KEYALIAS2, config.aliasKey2);
						writer.WriteString(CConfig.CFG_KEYTYPE1, config.typeKey1);
						writer.WriteString(CConfig.CFG_KEYTYPE2, config.typeKey2);
						writer.WriteString(CConfig.CFG_KEYVIEW1, config.viewKey1);
						writer.WriteString(CConfig.CFG_KEYVIEW2, config.viewKey2);
						writer.WriteString(CConfig.CFG_KEYMODE1, config.modeKey1);
						writer.WriteString(CConfig.CFG_KEYMODE2, config.modeKey2);
						writer.WriteString(CConfig.CFG_KEYIMG1, config.imageKey1);
						writer.WriteString(CConfig.CFG_KEYIMG2, config.imageKey2);
						writer.WriteString(CConfig.CFG_KEYSORT1, config.sortKey1);
						writer.WriteString(CConfig.CFG_KEYSORT2, config.sortKey2);
						writer.WriteString(CConfig.CFG_TXTMAINT, config.mainTitle);
						writer.WriteString(CConfig.CFG_TXTCFGT, config.settingsTitle);
						writer.WriteString(CConfig.CFG_TXTNMAIN1, config.mainTextNav1);
						writer.WriteString(CConfig.CFG_TXTNMAIN2, config.mainTextNav2);
						writer.WriteString(CConfig.CFG_TXTNSUB1, config.subTextNav1);
						writer.WriteString(CConfig.CFG_TXTNSUB2, config.subTextNav2);
						writer.WriteString(CConfig.CFG_TXTIMAIN1, config.mainTextIns1);
						writer.WriteString(CConfig.CFG_TXTIMAIN2, config.mainTextIns2);
						writer.WriteString(CConfig.CFG_TXTISUB1, config.subTextIns1);
						writer.WriteString(CConfig.CFG_TXTISUB2, config.subTextIns2);
						writer.WriteEndObject();
					}

					string strJsonData = Encoding.UTF8.GetString(stream.ToArray());
					byte[] bytes = new UTF8Encoding(true).GetBytes(strJsonData);

					using (FileStream fs = File.Create(configPath))
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
						writer.WriteBoolean(CConfig.CFG_NOQUIT, CConfig.DEF_NOQUIT);
						writer.WriteBoolean(CConfig.CFG_USEFILE, CConfig.DEF_USEFILE);
						writer.WriteBoolean(CConfig.CFG_USESCAN, CConfig.DEF_USESCAN);
						writer.WriteBoolean(CConfig.CFG_USECMD, CConfig.DEF_USECMD);
						writer.WriteBoolean(CConfig.CFG_USETYPE, CConfig.DEF_USETYPE);
						writer.WriteBoolean(CConfig.CFG_USELIST, CConfig.DEF_USELIST);
						writer.WriteBoolean(CConfig.CFG_NOPAGE, CConfig.DEF_NOPAGE);
						writer.WriteBoolean(CConfig.CFG_USESIZE, CConfig.DEF_USESIZE);
						writer.WriteBoolean(CConfig.CFG_USEALPH, CConfig.DEF_USEALPH);
						writer.WriteBoolean(CConfig.CFG_USEFAVE, CConfig.DEF_USEFAVE);
						writer.WriteBoolean(CConfig.CFG_USELITE, CConfig.DEF_USELITE);
						writer.WriteBoolean(CConfig.CFG_USEALL, CConfig.DEF_USEALL);
						writer.WriteBoolean(CConfig.CFG_NOCFG, CConfig.DEF_NOCFG);
						writer.WriteBoolean(CConfig.CFG_USECUST, CConfig.DEF_USECUST);
						writer.WriteBoolean(CConfig.CFG_USETEXT, CConfig.DEF_USETEXT);
						writer.WriteBoolean(CConfig.CFG_IMGBORD, CConfig.DEF_IMGBORD);
						writer.WriteBoolean(CConfig.CFG_IMGCUST, CConfig.DEF_IMGCUST);
						writer.WriteBoolean(CConfig.CFG_IMGRTIO, CConfig.DEF_IMGRTIO);
						writer.WriteNumber(CConfig.CFG_ICONSIZE, CConfig.DEF_ICONSIZE);
						writer.WriteNumber(CConfig.CFG_ICONRES, CConfig.DEF_ICONRES);
						writer.WriteNumber(CConfig.CFG_IMGSIZE, CConfig.DEF_IMGSIZE);
						writer.WriteNumber(CConfig.CFG_IMGRES, CConfig.DEF_IMGRES);
						writer.WriteNumber(CConfig.CFG_IMGPOS, CConfig.DEF_IMGPOS);
						writer.WriteNumber(CConfig.CFG_COLSIZE, CConfig.DEF_COLSIZE);
						writer.WriteString(CConfig.CFG_COLBG1, CConfig.DEF_COLBG1);
						writer.WriteString(CConfig.CFG_COLBG2, CConfig.DEF_COLBG2);
						writer.WriteString(CConfig.CFG_COLTITLE1, CConfig.DEF_COLTITLE1);
						writer.WriteString(CConfig.CFG_COLTITLE2, CConfig.DEF_COLTITLE2);
						writer.WriteString(CConfig.CFG_COLSUB1, CConfig.DEF_COLSUB1);
						writer.WriteString(CConfig.CFG_COLSUB2, CConfig.DEF_COLSUB2);
						writer.WriteString(CConfig.CFG_COLENTRY1, CConfig.DEF_COLENTRY1);
						writer.WriteString(CConfig.CFG_COLENTRY2, CConfig.DEF_COLENTRY2);
						writer.WriteString(CConfig.CFG_COLHIBG1, CConfig.DEF_COLHIBG1);
						writer.WriteString(CConfig.CFG_COLHIBG2, CConfig.DEF_COLHIBG2);
						writer.WriteString(CConfig.CFG_COLHIFG1, CConfig.DEF_COLHIFG1);
						writer.WriteString(CConfig.CFG_COLHIFG2, CConfig.DEF_COLHIFG2);
						writer.WriteString(CConfig.CFG_COLERRBG1, CConfig.DEF_COLERRBG1);
						writer.WriteString(CConfig.CFG_COLERRBG2, CConfig.DEF_COLERRBG2);
						writer.WriteString(CConfig.CFG_COLERRFG1, CConfig.DEF_COLERRFG1);
						writer.WriteString(CConfig.CFG_COLERRFG2, CConfig.DEF_COLERRFG2);
						writer.WriteString(CConfig.CFG_KEYLT1, CConfig.DEF_KEYLT1);
						writer.WriteString(CConfig.CFG_KEYLT2, CConfig.DEF_KEYLT2);
						writer.WriteString(CConfig.CFG_KEYUP1, CConfig.DEF_KEYUP1);
						writer.WriteString(CConfig.CFG_KEYUP2, CConfig.DEF_KEYUP2);
						writer.WriteString(CConfig.CFG_KEYRT1, CConfig.DEF_KEYRT1);
						writer.WriteString(CConfig.CFG_KEYRT2, CConfig.DEF_KEYRT2);
						writer.WriteString(CConfig.CFG_KEYDN1, CConfig.DEF_KEYDN1);
						writer.WriteString(CConfig.CFG_KEYDN2, CConfig.DEF_KEYDN2);
						writer.WriteString(CConfig.CFG_KEYSEL1, CConfig.DEF_KEYSEL1);
						writer.WriteString(CConfig.CFG_KEYSEL2, CConfig.DEF_KEYSEL2);
						writer.WriteString(CConfig.CFG_KEYQUIT1, CConfig.DEF_KEYQUIT1);
						writer.WriteString(CConfig.CFG_KEYQUIT2, CConfig.DEF_KEYQUIT2);
						writer.WriteString(CConfig.CFG_KEYSCAN1, CConfig.DEF_KEYSCAN1);
						writer.WriteString(CConfig.CFG_KEYSCAN2, CConfig.DEF_KEYSCAN2);
						writer.WriteString(CConfig.CFG_KEYHELP1, CConfig.DEF_KEYHELP1);
						writer.WriteString(CConfig.CFG_KEYHELP2, CConfig.DEF_KEYHELP2);
						writer.WriteString(CConfig.CFG_KEYBACK1, CConfig.DEF_KEYBACK1);
						writer.WriteString(CConfig.CFG_KEYBACK2, CConfig.DEF_KEYBACK2);
						writer.WriteString(CConfig.CFG_KEYPGUP1, CConfig.DEF_KEYPGUP1);
						writer.WriteString(CConfig.CFG_KEYPGUP2, CConfig.DEF_KEYPGUP2);
						writer.WriteString(CConfig.CFG_KEYPGDN1, CConfig.DEF_KEYPGDN1);
						writer.WriteString(CConfig.CFG_KEYPGDN2, CConfig.DEF_KEYPGDN2);
						writer.WriteString(CConfig.CFG_KEYHOME1, CConfig.DEF_KEYHOME1);
						writer.WriteString(CConfig.CFG_KEYHOME2, CConfig.DEF_KEYHOME2);
						writer.WriteString(CConfig.CFG_KEYEND1, CConfig.DEF_KEYEND1);
						writer.WriteString(CConfig.CFG_KEYEND2, CConfig.DEF_KEYEND2);
						writer.WriteString(CConfig.CFG_KEYFIND1, CConfig.DEF_KEYFIND1);
						writer.WriteString(CConfig.CFG_KEYFIND2, CConfig.DEF_KEYFIND2);
						writer.WriteString(CConfig.CFG_KEYTAB1, CConfig.DEF_KEYTAB1);
						writer.WriteString(CConfig.CFG_KEYTAB2, CConfig.DEF_KEYTAB2);
						writer.WriteString(CConfig.CFG_KEYESC1, CConfig.DEF_KEYESC1);
						writer.WriteString(CConfig.CFG_KEYESC2, CConfig.DEF_KEYESC2);
						writer.WriteString(CConfig.CFG_KEYUNIN1, CConfig.DEF_KEYUNIN1);
						writer.WriteString(CConfig.CFG_KEYUNIN2, CConfig.DEF_KEYUNIN2);
						writer.WriteString(CConfig.CFG_KEYDESK1, CConfig.DEF_KEYDESK1);
						writer.WriteString(CConfig.CFG_KEYDESK2, CConfig.DEF_KEYDESK2);
						writer.WriteString(CConfig.CFG_KEYHIDE1, CConfig.DEF_KEYHIDE1);
						writer.WriteString(CConfig.CFG_KEYHIDE2, CConfig.DEF_KEYHIDE2);
						writer.WriteString(CConfig.CFG_KEYFAVE1, CConfig.DEF_KEYFAVE1);
						writer.WriteString(CConfig.CFG_KEYFAVE2, CConfig.DEF_KEYFAVE2);
						writer.WriteString(CConfig.CFG_KEYALIAS1, CConfig.DEF_KEYALIAS1);
						writer.WriteString(CConfig.CFG_KEYALIAS2, CConfig.DEF_KEYALIAS2);
						writer.WriteString(CConfig.CFG_KEYTYPE1, CConfig.DEF_KEYTYPE1);
						writer.WriteString(CConfig.CFG_KEYTYPE2, CConfig.DEF_KEYTYPE2);
						writer.WriteString(CConfig.CFG_KEYVIEW1, CConfig.DEF_KEYVIEW1);
						writer.WriteString(CConfig.CFG_KEYVIEW2, CConfig.DEF_KEYVIEW2);
						writer.WriteString(CConfig.CFG_KEYMODE1, CConfig.DEF_KEYMODE1);
						writer.WriteString(CConfig.CFG_KEYMODE2, CConfig.DEF_KEYMODE2);
						writer.WriteString(CConfig.CFG_KEYIMG1, CConfig.DEF_KEYIMG1);
						writer.WriteString(CConfig.CFG_KEYIMG2, CConfig.DEF_KEYIMG2);
						writer.WriteString(CConfig.CFG_KEYSORT1, CConfig.DEF_KEYSORT1);
						writer.WriteString(CConfig.CFG_KEYSORT2, CConfig.DEF_KEYSORT2);
						writer.WriteString(CConfig.CFG_TXTMAINT, CConfig.DEF_TXTMAINT);
						writer.WriteString(CConfig.CFG_TXTCFGT, CConfig.DEF_TXTCFGT);
						writer.WriteString(CConfig.CFG_TXTNMAIN1, CConfig.DEF_TXTNMAIN1);
						writer.WriteString(CConfig.CFG_TXTNMAIN2, CConfig.DEF_TXTNMAIN2);
						writer.WriteString(CConfig.CFG_TXTNSUB1, CConfig.DEF_TXTNSUB1);
						writer.WriteString(CConfig.CFG_TXTNSUB2, CConfig.DEF_TXTNSUB2);
						writer.WriteString(CConfig.CFG_TXTIMAIN1, CConfig.DEF_TXTIMAIN1);
						writer.WriteString(CConfig.CFG_TXTIMAIN2, CConfig.DEF_TXTIMAIN2);
						writer.WriteString(CConfig.CFG_TXTISUB1, CConfig.DEF_TXTISUB1);
						writer.WriteString(CConfig.CFG_TXTISUB2, CConfig.DEF_TXTISUB2);
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
		/// Import games from the json file and add them to the global game dictionary.
		/// <returns>True if successful, otherwise false</returns>
		/// </summary>
		private static bool ImportGames(string file, ref int nGameCount, ref Version version, bool alphaSort, bool faveSort, bool ignoreArticle)
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
						string strUninstaller = GetStringProperty(jElement, GAMES_ARRAY_UNINSTALLER);
						bool   bIsFavourite = GetBoolProperty(jElement, GAMES_ARRAY_FAVOURITE);
						bool   bIsHidden = GetBoolProperty(jElement, GAMES_ARRAY_HIDDEN);
						string strAlias = GetStringProperty(jElement, GAMES_ARRAY_ALIAS);
						string strPlatform = GetStringProperty(jElement, GAMES_ARRAY_PLATFORM);
						double fOccurCount = GetDoubleProperty(jElement, GAMES_ARRAY_FREQUENCY);

						CGameData.AddGame(strID, strTitle, strLaunch, strIconPath, strUninstaller, bIsFavourite, bIsHidden, strAlias, strPlatform, fOccurCount);
						nGameCount++;
					}
					CGameData.SortGames(alphaSort, faveSort, ignoreArticle);
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
				Console.WriteLine($"ERROR: Malformed {file} file!");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Find installed Epic store games (unlike the other platforms, this is scraped from json files instead of the registry)
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
						CLogger.LogDebug($"* {strTitle}");
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
							gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, strPlatform));
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
		/// Find installed Indiegala games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		public static void GetIGGames(List<CRegScanner.RegistryGameData> gameDataList)
		{
			const string IG_NAME		= "IGClient";
			const string IG_JSON_FILE	= @"\IGClient\storage\installed.json";
			string file = GetFolderPath(SpecialFolder.ApplicationData) + IG_JSON_FILE;

			if (!File.Exists(file))
			{
				CLogger.LogInfo("{0} games not found in AppData", IG_NAME.ToUpper());
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
				{
					CLogger.LogWarn(string.Format("ERROR: Malformed {0} file: {1}", IG_NAME.ToUpper(), file));
					return;
				}

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
									strID = string.Format("ig_{0}", GetStringProperty(item, "slugged_name"));
									strTitle = GetStringProperty(item, "name");
								}
							}
							element.TryGetProperty("path", out JsonElement paths);
							if (!paths.Equals(null))
							{
								foreach (JsonElement path in paths.EnumerateArray())
									strLaunch = CGameFinder.FindGameBinaryFile(path.ToString(), strTitle);
							}

							CLogger.LogDebug($"* {strTitle}");

							if (!string.IsNullOrEmpty(strLaunch))
							{
								strAlias = CRegScanner.GetAlias(Path.GetFileNameWithoutExtension(strLaunch));
								if (strAlias.Length > strTitle.Length)
									strAlias = CRegScanner.GetAlias(strTitle);
								if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
									strAlias = "";
								gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, strPlatform));
							}
						}
					}
				}
				catch (Exception e)
				{
					CLogger.LogError(e, string.Format("ERROR: Malformed {0} file: {1}", IG_NAME.ToUpper(), file));
				}
				CLogger.LogDebug("--------------------");
			}
		}

		/// <summary>
		/// Find installed Itch games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		public static void GetAmazonGames(List<CRegScanner.RegistryGameData> gameDataList, bool expensiveIcons)
		{
			const string NODE64_REG = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

			const string AMAZON_NAME = "Amazon";
			const string AMAZON_LAUNCH = "amazon-games://play/";
			const string AMAZON_DB = @"\Amazon Games\Data\Games\Sql\GameInstallInfo.sqlite";
			string db = GetFolderPath(SpecialFolder.LocalApplicationData) + AMAZON_DB;
			if (!File.Exists(db))
			{
				CLogger.LogInfo("{0} database not found.", AMAZON_NAME.ToUpper());
				return;
			}

			try
			{
				using (var con = new SQLiteConnection(string.Format($"Data Source={db}")))
				{
					con.Open();

					// SELECT path FROM install_locations
					// SELECT install_folder FROM downloads
					// SELECT verdict FROM caves
					using (var cmd = new SQLiteCommand("SELECT Id, InstallDirectory, ProductTitle FROM DbSet", con))
					{
						using (SQLiteDataReader rdr = cmd.ExecuteReader())
						{
							while (rdr.Read())
							{
								string strID = rdr.GetString(0);
								string strTitle = rdr.GetString(2);
								CLogger.LogDebug($"* {strTitle}");
								string strLaunch = AMAZON_LAUNCH + strID;
								string strIconPath = "";
								string strUninstall = "";

								using (RegistryKey key = Registry.CurrentUser.OpenSubKey(NODE64_REG + "\\AmazonGames/" + strTitle, RegistryKeyPermissionCheck.ReadSubTree))
								{
									strIconPath = CRegScanner.GetRegStrVal(key, "DisplayIcon");
									strUninstall = CRegScanner.GetRegStrVal(key, "UninstallString");
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
									gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, strPlatform));
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e, string.Format($"ERROR: Malformed {0} database output!", AMAZON_NAME.ToUpper()));
				return;
			}
			CLogger.LogDebug("-------------------");
		}

		/// <summary>
		/// Find installed Itch games
		/// </summary>
		/// <param name="gameDataList">List of game data objects</param>
		public static void GetItchGames(List<CRegScanner.RegistryGameData> gameDataList)
		{
			const string ITCH_NAME = "itch";
			const string ITCH_DB = @"\itch\db\butler.db";
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

					// SELECT path FROM install_locations
					// SELECT install_folder FROM downloads
					// SELECT verdict FROM caves
					using (var cmd = new SQLiteCommand("SELECT game_id, verdict, install_folder_name FROM caves", con))
					{
						using (SQLiteDataReader rdr = cmd.ExecuteReader())
						{
							while (rdr.Read())
							{
								int id = rdr.GetInt32(0);
								string strID = string.Format($"itch_{id}");
								string verdict = rdr.GetString(1);
								string strTitle = rdr.GetString(2);
								string strAlias = CRegScanner.GetAlias(strTitle);
								string strLaunch = "";
								string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.Itch);

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
								}
								if (string.IsNullOrEmpty(strLaunch))
									continue;

								using (var cmd2 = new SQLiteCommand(string.Format($"SELECT title FROM games WHERE id={id};"), con))
								using (SQLiteDataReader rdr2 = cmd2.ExecuteReader())
								{
									while (rdr2.Read())
									{
										strTitle = rdr2.GetString(0);
										if (strAlias.Length > strTitle.Length)
											strAlias = CRegScanner.GetAlias(strTitle);
										break;
									}
								}
								CLogger.LogDebug($"* {strTitle}");

								if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
									strAlias = "";
								if (!string.IsNullOrEmpty(strLaunch))
									gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, strPlatform));
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e, string.Format($"ERROR: Malformed {0} database output!", ITCH_NAME.ToUpper()));
				return;
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
						int nPercent = GetIntProperty(jElement, LAST_ARRAY_PERCENT);
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
		/// Import games from the json file and add them to the global game dictionary.
		/// <returns>True if successful, otherwise false</returns>
		/// </summary>
		private static bool ImportConfig(string file, ref Version version, ref CConfig.Configuration config, ref CConfig.Hotkeys hotkeys, ref CConfig.Colours colours, out bool bCfgReset)
		{
			bool parseError = false;
			bCfgReset = false;

			SetConfigDefaults(ref config, true);

			CLogger.LogInfo("Importing configuration data from JSON...");
			var options = new JsonDocumentOptions
			{
				AllowTrailingCommas = true
			};

			string strDocumentData = File.ReadAllText(file);

			SetConfigDefaults(ref config, false, false, false, false, false, false);  // always set defaults for null values in case some have been added after a version update

			if (string.IsNullOrEmpty(strDocumentData))
			{
				//SetConfigDefaults(ref config, false, false, false, false, false, false);
				bCfgReset = true;
				return true;
			}
			try
			{
				using (JsonDocument document = JsonDocument.Parse(@strDocumentData, options))
				{
					System.Version.TryParse(GetStringProperty(document.RootElement, JSON_VERSION), out version);
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
				Console.WriteLine($"ERROR: Bad version data in {file} file.");
				Console.Write("Would you like to reset to defaults? (y|n) : ");
				string strInput = Console.ReadLine();
				if (strInput.ToUpper().Equals("Y"))
				{
					bCfgReset = true;
					return false;
				}
			}
			try
			{
				using (JsonDocument document = JsonDocument.Parse(@strDocumentData, options))
				{
					config.preventQuit = GetBoolProperty(document.RootElement, CConfig.CFG_NOQUIT);
					config.ignoreChanges = GetBoolProperty(document.RootElement, CConfig.CFG_USEFILE);
					config.alwaysScan = GetBoolProperty(document.RootElement, CConfig.CFG_USESCAN);
					config.onlyCmdLine = GetBoolProperty(document.RootElement, CConfig.CFG_USECMD);
					config.typeInput = GetBoolProperty(document.RootElement, CConfig.CFG_USETYPE);
					config.listOutput = GetBoolProperty(document.RootElement, CConfig.CFG_USELIST);
					config.noPageSplit = GetBoolProperty(document.RootElement, CConfig.CFG_NOPAGE);
					config.sizeToFit = GetBoolProperty(document.RootElement, CConfig.CFG_USESIZE);
					config.alphaSort = GetBoolProperty(document.RootElement, CConfig.CFG_USEALPH);
					config.faveSort = GetBoolProperty(document.RootElement, CConfig.CFG_USEFAVE);
					config.lightMode = GetBoolProperty(document.RootElement, CConfig.CFG_USELITE);
					config.goToAll = GetBoolProperty(document.RootElement, CConfig.CFG_USEALL);
					config.hideSettings = GetBoolProperty(document.RootElement, CConfig.CFG_NOCFG);
					config.onlyCustom = GetBoolProperty(document.RootElement, CConfig.CFG_USECUST);
					config.msgCustom = GetBoolProperty(document.RootElement, CConfig.CFG_USETEXT);
					config.imageBorder = GetBoolProperty(document.RootElement, CConfig.CFG_IMGBORD);
					config.noImageCustom = GetBoolProperty(document.RootElement, CConfig.CFG_IMGCUST);
					config.imageIgnoreRatio = GetBoolProperty(document.RootElement, CConfig.CFG_IMGRTIO);
				}
			}
			catch (Exception e)
            {
				CLogger.LogError(e);
				Console.WriteLine($"ERROR: Bad boolean data in {file} file. Resetting defaults...");
				parseError = true;
				SetConfigDefaults(ref config, false, true, false, false, false, false);
			}
			try
			{
				using (JsonDocument document = JsonDocument.Parse(@strDocumentData, options))
				{
					config.iconSize = GetIntProperty(document.RootElement, CConfig.CFG_ICONSIZE);
					config.iconRes = GetIntProperty(document.RootElement, CConfig.CFG_ICONRES);
					config.imageSize = GetIntProperty(document.RootElement, CConfig.CFG_IMGSIZE);
					config.imageRes = GetIntProperty(document.RootElement, CConfig.CFG_IMGRES);
					config.imagePosition = GetIntProperty(document.RootElement, CConfig.CFG_IMGPOS);
					config.columnSize = GetIntProperty(document.RootElement, CConfig.CFG_COLSIZE);
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
				Console.WriteLine($"ERROR: Bad number data in {file} file. Resetting defaults...");
				parseError = true;
				SetConfigDefaults(ref config, false, false, true, false, false, false);
			}
			try
			{
				using (JsonDocument document = JsonDocument.Parse(@strDocumentData, options))
				{
					config.bgCol = GetColourProperty(document.RootElement, CConfig.CFG_COLBG1);
					config.bgLtCol = GetColourProperty(document.RootElement, CConfig.CFG_COLBG2);
					config.titleCol = GetColourProperty(document.RootElement, CConfig.CFG_COLTITLE1);
					config.titleLtCol = GetColourProperty(document.RootElement, CConfig.CFG_COLTITLE2);
					config.subCol = GetColourProperty(document.RootElement, CConfig.CFG_COLSUB1);
					config.subLtCol = GetColourProperty(document.RootElement, CConfig.CFG_COLSUB2);
					config.entryCol = GetColourProperty(document.RootElement, CConfig.CFG_COLENTRY1);
					config.entryLtCol = GetColourProperty(document.RootElement, CConfig.CFG_COLENTRY2);
					config.highbgCol = GetColourProperty(document.RootElement, CConfig.CFG_COLHIBG1);
					config.highbgLtCol = GetColourProperty(document.RootElement, CConfig.CFG_COLHIBG2);
					config.highlightCol = GetColourProperty(document.RootElement, CConfig.CFG_COLHIFG1);
					config.highlightLtCol = GetColourProperty(document.RootElement, CConfig.CFG_COLHIFG2);
					config.errbgCol = GetColourProperty(document.RootElement, CConfig.CFG_COLERRBG1);
					config.errbgLtCol = GetColourProperty(document.RootElement, CConfig.CFG_COLERRBG2);
					config.errorCol = GetColourProperty(document.RootElement, CConfig.CFG_COLERRFG1);
					config.errorLtCol = GetColourProperty(document.RootElement, CConfig.CFG_COLERRFG2);
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
				Console.WriteLine($"ERROR: Bad colour string in {file} file. Resetting defaults...");
				parseError = true;
				SetConfigDefaults(ref config, false, false, false, true, false, false);
			}
			try
			{
				Enum.TryParse<ConsoleColor>(config.bgCol.ToString(), true, out colours.bgCC);
				Enum.TryParse<ConsoleColor>(config.bgLtCol.ToString(), true, out colours.bgLtCC);
				Enum.TryParse<ConsoleColor>(config.titleCol.ToString(), true, out colours.titleCC);
				Enum.TryParse<ConsoleColor>(config.titleLtCol.ToString(), true, out colours.titleLtCC);
				Enum.TryParse<ConsoleColor>(config.subCol.ToString(), true, out colours.subCC);
				Enum.TryParse<ConsoleColor>(config.subLtCol.ToString(), true, out colours.subLtCC);
				Enum.TryParse<ConsoleColor>(config.entryCol.ToString(), true, out colours.entryCC);
				Enum.TryParse<ConsoleColor>(config.entryLtCol.ToString(), true, out colours.entryLtCC);
				Enum.TryParse<ConsoleColor>(config.highbgCol.ToString(), true, out colours.highbgCC);
				Enum.TryParse<ConsoleColor>(config.highbgLtCol.ToString(), true, out colours.highbgLtCC);
				Enum.TryParse<ConsoleColor>(config.highlightCol.ToString(), true, out colours.highlightCC);
				Enum.TryParse<ConsoleColor>(config.highlightLtCol.ToString(), true, out colours.highlightLtCC);
				Enum.TryParse<ConsoleColor>(config.errbgCol.ToString(), true, out colours.errorCC);
				Enum.TryParse<ConsoleColor>(config.errbgLtCol.ToString(), true, out colours.errorLtCC);
				Enum.TryParse<ConsoleColor>(config.errorCol.ToString(), true, out colours.errorCC);
				Enum.TryParse<ConsoleColor>(config.errorLtCol.ToString(), true, out colours.errorLtCC);
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
				Console.WriteLine($"ERROR: Bad colour value in {file} file. Resetting defaults...");
				parseError = true;
				SetConfigDefaults(ref config, false, false, false, true, false, false);
			}
			try
			{
				using (JsonDocument document = JsonDocument.Parse(@strDocumentData, options))
				{
					config.leftKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYLT1);
					config.leftKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYLT2);
					config.upKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYUP1);
					config.upKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYUP2);
					config.rightKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYRT1);
					config.rightKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYRT2);
					config.downKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYDN1);
					config.downKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYDN2);
					config.selectKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYSEL1);
					config.selectKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYSEL2);
					config.quitKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYQUIT1);
					config.quitKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYQUIT2);
					config.scanKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYSCAN1);
					config.scanKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYSCAN2);
					config.helpKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYHELP1);
					config.helpKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYHELP2);
					config.backKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYBACK1);
					config.backKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYBACK2);
					config.pageUpKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYPGUP1);
					config.pageUpKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYPGUP2);
					config.pageDownKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYPGDN1);
					config.pageDownKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYPGDN2);
					config.firstKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYHOME1);
					config.firstKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYHOME2);
					config.lastKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYEND1);
					config.lastKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYEND2);
					config.searchKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYFIND1);
					config.searchKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYFIND2);
					config.completeKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYTAB1);
					config.completeKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYTAB2);
					config.cancelKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYESC1);
					config.cancelKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYESC2);
					config.uninstKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYUNIN1);
					config.uninstKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYUNIN2);
					config.desktopKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYDESK1);
					config.desktopKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYDESK2);
					config.hideKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYHIDE1);
					config.hideKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYHIDE2);
					config.faveKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYFAVE1);
					config.faveKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYFAVE2);
					config.aliasKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYALIAS1);
					config.aliasKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYALIAS2);
					config.typeKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYTYPE1);
					config.typeKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYTYPE2);
					config.viewKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYVIEW1);
					config.viewKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYVIEW2);
					config.modeKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYMODE1);
					config.modeKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYMODE2);
					config.imageKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYIMG1);
					config.imageKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYIMG2);
					config.sortKey1 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYSORT1);
					config.sortKey2 = GetHotkeyProperty(document.RootElement, CConfig.CFG_KEYSORT2);
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
				Console.WriteLine($"ERROR: Bad hotkey string in {file} file. Resetting defaults...");
				parseError = true;
				SetConfigDefaults(ref config, false, false, false, false, true, false);
			}
			try
			{
				Enum.TryParse<ConsoleKey>(config.leftKey1.ToString(), true, out hotkeys.leftCK1);
				Enum.TryParse<ConsoleKey>(config.leftKey2.ToString(), true, out hotkeys.leftCK2);
				Enum.TryParse<ConsoleKey>(config.upKey1.ToString(), true, out hotkeys.upCK1);
				Enum.TryParse<ConsoleKey>(config.upKey2.ToString(), true, out hotkeys.upCK2);
				Enum.TryParse<ConsoleKey>(config.rightKey1.ToString(), true, out hotkeys.rightCK1);
				Enum.TryParse<ConsoleKey>(config.rightKey2.ToString(), true, out hotkeys.rightCK2);
				Enum.TryParse<ConsoleKey>(config.downKey1.ToString(), true, out hotkeys.downCK1);
				Enum.TryParse<ConsoleKey>(config.downKey2.ToString(), true, out hotkeys.downCK2);
				Enum.TryParse<ConsoleKey>(config.selectKey1.ToString(), true, out hotkeys.selectCK1);
				Enum.TryParse<ConsoleKey>(config.selectKey2.ToString(), true, out hotkeys.selectCK2);
				Enum.TryParse<ConsoleKey>(config.quitKey1.ToString(), true, out hotkeys.quitCK1);
				Enum.TryParse<ConsoleKey>(config.quitKey2.ToString(), true, out hotkeys.quitCK2);
				Enum.TryParse<ConsoleKey>(config.scanKey1.ToString(), true, out hotkeys.scanCK1);
				Enum.TryParse<ConsoleKey>(config.scanKey2.ToString(), true, out hotkeys.scanCK2);
				Enum.TryParse<ConsoleKey>(config.helpKey1.ToString(), true, out hotkeys.helpCK1);
				Enum.TryParse<ConsoleKey>(config.helpKey2.ToString(), true, out hotkeys.helpCK2);
				Enum.TryParse<ConsoleKey>(config.backKey1.ToString(), true, out hotkeys.backCK1);
				Enum.TryParse<ConsoleKey>(config.backKey2.ToString(), true, out hotkeys.backCK2);
				Enum.TryParse<ConsoleKey>(config.pageUpKey1.ToString(), true, out hotkeys.pageUpCK1);
				Enum.TryParse<ConsoleKey>(config.pageUpKey2.ToString(), true, out hotkeys.pageUpCK2);
				Enum.TryParse<ConsoleKey>(config.pageDownKey1.ToString(), true, out hotkeys.pageDownCK1);
				Enum.TryParse<ConsoleKey>(config.pageDownKey2.ToString(), true, out hotkeys.pageDownCK2);
				Enum.TryParse<ConsoleKey>(config.firstKey1.ToString(), true, out hotkeys.firstCK1);
				Enum.TryParse<ConsoleKey>(config.firstKey2.ToString(), true, out hotkeys.firstCK2);
				Enum.TryParse<ConsoleKey>(config.lastKey1.ToString(), true, out hotkeys.lastCK1);
				Enum.TryParse<ConsoleKey>(config.lastKey2.ToString(), true, out hotkeys.lastCK2);
				Enum.TryParse<ConsoleKey>(config.searchKey1.ToString(), true, out hotkeys.searchCK1);
				Enum.TryParse<ConsoleKey>(config.searchKey2.ToString(), true, out hotkeys.searchCK2);
				Enum.TryParse<ConsoleKey>(config.completeKey1.ToString(), true, out hotkeys.completeCK1);
				Enum.TryParse<ConsoleKey>(config.completeKey2.ToString(), true, out hotkeys.completeCK2);
				Enum.TryParse<ConsoleKey>(config.cancelKey1.ToString(), true, out hotkeys.cancelCK1);
				Enum.TryParse<ConsoleKey>(config.cancelKey2.ToString(), true, out hotkeys.cancelCK2);
				Enum.TryParse<ConsoleKey>(config.uninstKey1.ToString(), true, out hotkeys.uninstCK1);
				Enum.TryParse<ConsoleKey>(config.uninstKey2.ToString(), true, out hotkeys.uninstCK2);
				Enum.TryParse<ConsoleKey>(config.desktopKey1.ToString(), true, out hotkeys.desktopCK1);
				Enum.TryParse<ConsoleKey>(config.desktopKey2.ToString(), true, out hotkeys.desktopCK2);
				Enum.TryParse<ConsoleKey>(config.hideKey1.ToString(), true, out hotkeys.hideCK1);
				Enum.TryParse<ConsoleKey>(config.hideKey2.ToString(), true, out hotkeys.hideCK2);
				Enum.TryParse<ConsoleKey>(config.faveKey1.ToString(), true, out hotkeys.faveCK1);
				Enum.TryParse<ConsoleKey>(config.faveKey2.ToString(), true, out hotkeys.faveCK2);
				Enum.TryParse<ConsoleKey>(config.aliasKey1.ToString(), true, out hotkeys.aliasCK1);
				Enum.TryParse<ConsoleKey>(config.aliasKey2.ToString(), true, out hotkeys.aliasCK2);
				Enum.TryParse<ConsoleKey>(config.typeKey1.ToString(), true, out hotkeys.typeCK1);
				Enum.TryParse<ConsoleKey>(config.typeKey2.ToString(), true, out hotkeys.typeCK2);
				Enum.TryParse<ConsoleKey>(config.viewKey1.ToString(), true, out hotkeys.viewCK1);
				Enum.TryParse<ConsoleKey>(config.viewKey2.ToString(), true, out hotkeys.viewCK2);
				Enum.TryParse<ConsoleKey>(config.modeKey1.ToString(), true, out hotkeys.modeCK1);
				Enum.TryParse<ConsoleKey>(config.modeKey2.ToString(), true, out hotkeys.modeCK2);
				Enum.TryParse<ConsoleKey>(config.imageKey1.ToString(), true, out hotkeys.imageCK1);
				Enum.TryParse<ConsoleKey>(config.imageKey2.ToString(), true, out hotkeys.imageCK2);
				Enum.TryParse<ConsoleKey>(config.sortKey1.ToString(), true, out hotkeys.sortCK1);
				Enum.TryParse<ConsoleKey>(config.sortKey2.ToString(), true, out hotkeys.sortCK2);
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
				Console.WriteLine($"ERROR: Bad hotkey value in {file} file. Resetting defaults...");
				parseError = true;
				SetConfigDefaults(ref config, false, false, false, false, true, false);
			}
			try
			{
				using (JsonDocument document = JsonDocument.Parse(@strDocumentData, options))
				{
					config.mainTitle = GetStringProperty(document.RootElement, CConfig.CFG_TXTMAINT);
					config.settingsTitle = GetStringProperty(document.RootElement, CConfig.CFG_TXTCFGT);
					config.mainTextNav1 = GetStringProperty(document.RootElement, CConfig.CFG_TXTNMAIN1);
					config.mainTextNav2 = GetStringProperty(document.RootElement, CConfig.CFG_TXTNMAIN2);
					config.subTextNav1 = GetStringProperty(document.RootElement, CConfig.CFG_TXTNSUB1);
					config.subTextNav2 = GetStringProperty(document.RootElement, CConfig.CFG_TXTNSUB2);
					config.mainTextIns1 = GetStringProperty(document.RootElement, CConfig.CFG_TXTIMAIN1);
					config.mainTextIns2 = GetStringProperty(document.RootElement, CConfig.CFG_TXTIMAIN2);
					config.subTextIns1 = GetStringProperty(document.RootElement, CConfig.CFG_TXTISUB1);
					config.subTextIns2 = GetStringProperty(document.RootElement, CConfig.CFG_TXTISUB2);
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e);
				Console.WriteLine($"ERROR: Bad text data in {file} file. Resetting defaults...");
				parseError = true;
				SetConfigDefaults(ref config, false, false, false, false, false, true);
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
			writer.WriteBoolean(GAMES_ARRAY_FAVOURITE	, data.IsFavourite);
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
		/// Retrieve int value from the JSON element
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
		private static void SetConfigDefaults(ref CConfig.Configuration config, bool forceAll, bool boolOnly, bool numOnly, bool colourOnly, bool keyOnly, bool textOnly)
		{
			if (forceAll)
            {
				SetBoolDefaults(ref config, true);
				SetNumberDefaults(ref config, true);
				SetColourDefaults(ref config, true);
				SetKeyDefaults(ref config, true);
				SetTextDefaults(ref config, true);
			}
			else if (boolOnly)
				SetBoolDefaults(ref config, true);
			else if (numOnly)
				SetNumberDefaults(ref config, true);
			else if (colourOnly)
				SetColourDefaults(ref config, true);
			else if (keyOnly)
				SetKeyDefaults(ref config, true);
			else if (textOnly)
				SetTextDefaults(ref config, true);
			else
            {
				SetBoolDefaults(ref config, false);
				SetNumberDefaults(ref config, false);
				SetColourDefaults(ref config, false);
				SetKeyDefaults(ref config, false);
				SetTextDefaults(ref config, false);
			}
		}

		/// <summary>
		/// Set the default configuration values
		/// </summary>
		private static void SetConfigDefaults(ref CConfig.Configuration config, bool forceAll)
		{
			SetConfigDefaults(ref config, forceAll, false, false, false, false, false);
        }

		/// <summary>
		/// Set the default boolean configuration values
		/// </summary>
		private static void SetBoolDefaults(ref CConfig.Configuration config, bool force)
        {
			SetDefaultVal(ref config.preventQuit, CConfig.DEF_NOQUIT, force);
			SetDefaultVal(ref config.ignoreChanges, CConfig.DEF_USEFILE, force);
			SetDefaultVal(ref config.alwaysScan, CConfig.DEF_USESCAN, force);
			SetDefaultVal(ref config.onlyCmdLine, CConfig.DEF_USECMD, force);
			SetDefaultVal(ref config.typeInput, CConfig.DEF_USETYPE, force);
			SetDefaultVal(ref config.listOutput, CConfig.DEF_USELIST, force);
			SetDefaultVal(ref config.noPageSplit, CConfig.DEF_NOPAGE, force);
			SetDefaultVal(ref config.sizeToFit, CConfig.DEF_USESIZE, force);
			SetDefaultVal(ref config.alphaSort, CConfig.DEF_USEALPH, force);
			SetDefaultVal(ref config.faveSort, CConfig.DEF_USEFAVE, force);
			SetDefaultVal(ref config.lightMode, CConfig.DEF_USELITE, force);
			SetDefaultVal(ref config.goToAll, CConfig.DEF_USEALL, force);
			SetDefaultVal(ref config.hideSettings, CConfig.DEF_NOCFG, force);
			SetDefaultVal(ref config.onlyCustom, CConfig.DEF_USECUST, force);
			SetDefaultVal(ref config.msgCustom, CConfig.DEF_USETEXT, force);
			SetDefaultVal(ref config.imageBorder, CConfig.DEF_IMGBORD, force);
			SetDefaultVal(ref config.noImageCustom, CConfig.DEF_IMGCUST, force);
			SetDefaultVal(ref config.imageIgnoreRatio, CConfig.DEF_IMGRTIO, force);
		}
		
		/// <summary>
		/// Set the default number configuration values
		/// </summary>
		private static void SetNumberDefaults(ref CConfig.Configuration config, bool force)
		{
			SetDefaultVal(ref config.iconSize, CConfig.DEF_ICONSIZE, force);
			SetDefaultVal(ref config.iconRes, CConfig.DEF_ICONRES, force);
			SetDefaultVal(ref config.imageSize, CConfig.DEF_IMGSIZE, force);
			SetDefaultVal(ref config.imageRes, CConfig.DEF_IMGRES, force);
			SetDefaultVal(ref config.imagePosition, CConfig.DEF_IMGPOS, force);
			SetDefaultVal(ref config.columnSize, CConfig.DEF_COLSIZE, force);
		}

		/// <summary>
		/// Set the default colour configuration values
		/// </summary>
		private static void SetColourDefaults(ref CConfig.Configuration config, bool force)
		{
			SetDefaultVal(ref config.bgCol, CConfig.COLBG_UNK1, force);
			SetDefaultVal(ref config.bgLtCol, CConfig.COLBG_UNK2, force);
			SetDefaultVal(ref config.titleCol, CConfig.COLFG_UNK1, force);
			SetDefaultVal(ref config.titleLtCol, CConfig.COLFG_UNK2, force);
			SetDefaultVal(ref config.subCol, CConfig.COLFG_UNK1, force);
			SetDefaultVal(ref config.subLtCol, CConfig.COLFG_UNK2, force);
			SetDefaultVal(ref config.entryCol, CConfig.COLFG_UNK1, force);
			SetDefaultVal(ref config.entryLtCol, CConfig.COLFG_UNK2, force);
			SetDefaultVal(ref config.highbgCol, CConfig.COLFG_UNK1, force);
			SetDefaultVal(ref config.highbgLtCol, CConfig.COLFG_UNK2, force);
			SetDefaultVal(ref config.highlightCol, CConfig.COLBG_UNK1, force);
			SetDefaultVal(ref config.highlightLtCol, CConfig.COLBG_UNK2, force);
			SetDefaultVal(ref config.errbgCol, CConfig.COLBG_UNK1, force);
			SetDefaultVal(ref config.errbgLtCol, CConfig.COLBG_UNK2, force);
			SetDefaultVal(ref config.errorCol, CConfig.COLFG_UNK1, force);
			SetDefaultVal(ref config.errorLtCol, CConfig.COLFG_UNK2, force);
		}

		/// <summary>
		/// Set the default colour configuration values
		/// </summary>
		private static void SetKeyDefaults(ref CConfig.Configuration config, bool force)
		{
			SetDefaultVal(ref config.leftKey1, CConfig.DEF_KEYLT1, force);
			SetDefaultVal(ref config.leftKey2, CConfig.DEF_KEYLT2, force);
			SetDefaultVal(ref config.upKey1, CConfig.DEF_KEYUP1, force);
			SetDefaultVal(ref config.upKey2, CConfig.DEF_KEYUP2, force);
			SetDefaultVal(ref config.rightKey1, CConfig.DEF_KEYRT1, force);
			SetDefaultVal(ref config.rightKey2, CConfig.DEF_KEYRT2, force);
			SetDefaultVal(ref config.downKey1, CConfig.DEF_KEYDN1, force);
			SetDefaultVal(ref config.downKey2, CConfig.DEF_KEYDN2, force);
			SetDefaultVal(ref config.selectKey1, CConfig.DEF_KEYSEL1, force);
			SetDefaultVal(ref config.selectKey2, CConfig.DEF_KEYSEL2, force);
			SetDefaultVal(ref config.quitKey1, CConfig.DEF_KEYQUIT1, force);
			SetDefaultVal(ref config.quitKey2, CConfig.DEF_KEYQUIT2, force);
			SetDefaultVal(ref config.scanKey1, CConfig.DEF_KEYSCAN1, force);
			SetDefaultVal(ref config.scanKey2, CConfig.DEF_KEYSCAN2, force);
			SetDefaultVal(ref config.helpKey1, CConfig.DEF_KEYHELP1, force);
			SetDefaultVal(ref config.helpKey2, CConfig.DEF_KEYHELP2, force);
			SetDefaultVal(ref config.backKey1, CConfig.DEF_KEYBACK1, force);
			SetDefaultVal(ref config.backKey2, CConfig.DEF_KEYBACK2, force);
			SetDefaultVal(ref config.pageUpKey1, CConfig.DEF_KEYPGUP1, force);
			SetDefaultVal(ref config.pageUpKey2, CConfig.DEF_KEYPGUP2, force);
			SetDefaultVal(ref config.pageDownKey1, CConfig.DEF_KEYPGDN1, force);
			SetDefaultVal(ref config.pageDownKey2, CConfig.DEF_KEYPGDN2, force);
			SetDefaultVal(ref config.firstKey1, CConfig.DEF_KEYHOME1, force);
			SetDefaultVal(ref config.firstKey2, CConfig.DEF_KEYHOME2, force);
			SetDefaultVal(ref config.lastKey1, CConfig.DEF_KEYEND1, force);
			SetDefaultVal(ref config.lastKey2, CConfig.DEF_KEYEND2, force);
			SetDefaultVal(ref config.searchKey1, CConfig.DEF_KEYFIND1, force);
			SetDefaultVal(ref config.searchKey2, CConfig.DEF_KEYFIND2, force);
			SetDefaultVal(ref config.completeKey1, CConfig.DEF_KEYTAB1, force);
			SetDefaultVal(ref config.completeKey2, CConfig.DEF_KEYTAB2, force);
			SetDefaultVal(ref config.cancelKey1, CConfig.DEF_KEYESC1, force);
			SetDefaultVal(ref config.cancelKey2, CConfig.DEF_KEYESC2, force);
			SetDefaultVal(ref config.uninstKey1, CConfig.DEF_KEYUNIN1, force);
			SetDefaultVal(ref config.uninstKey2, CConfig.DEF_KEYUNIN2, force);
			SetDefaultVal(ref config.desktopKey1, CConfig.DEF_KEYDESK1, force);
			SetDefaultVal(ref config.desktopKey2, CConfig.DEF_KEYDESK2, force);
			SetDefaultVal(ref config.hideKey1, CConfig.DEF_KEYHIDE1, force);
			SetDefaultVal(ref config.hideKey2, CConfig.DEF_KEYHIDE2, force);
			SetDefaultVal(ref config.faveKey1, CConfig.DEF_KEYFAVE1, force);
			SetDefaultVal(ref config.faveKey2, CConfig.DEF_KEYFAVE2, force);
			SetDefaultVal(ref config.aliasKey1, CConfig.DEF_KEYALIAS1, force);
			SetDefaultVal(ref config.aliasKey2, CConfig.DEF_KEYALIAS2, force);
			SetDefaultVal(ref config.typeKey1, CConfig.DEF_KEYTYPE1, force);
			SetDefaultVal(ref config.typeKey2, CConfig.DEF_KEYTYPE2, force);
			SetDefaultVal(ref config.viewKey1, CConfig.DEF_KEYVIEW1, force);
			SetDefaultVal(ref config.viewKey2, CConfig.DEF_KEYVIEW2, force);
			SetDefaultVal(ref config.modeKey1, CConfig.DEF_KEYMODE1, force);
			SetDefaultVal(ref config.modeKey2, CConfig.DEF_KEYMODE2, force);
			SetDefaultVal(ref config.imageKey1, CConfig.DEF_KEYIMG1, force);
			SetDefaultVal(ref config.imageKey2, CConfig.DEF_KEYIMG2, force);
			SetDefaultVal(ref config.sortKey1, CConfig.DEF_KEYSORT1, force);
			SetDefaultVal(ref config.sortKey2, CConfig.DEF_KEYSORT2, force);
		}

		/// <summary>
		/// Set the default text configuration values
		/// </summary>
		private static void SetTextDefaults(ref CConfig.Configuration config, bool force)
		{
			SetDefaultVal(ref config.mainTitle, CConfig.DEF_TXTMAINT, force);
			SetDefaultVal(ref config.settingsTitle, CConfig.DEF_TXTCFGT, force);
			SetDefaultVal(ref config.mainTextNav1, CConfig.DEF_TXTNMAIN1, force);
			SetDefaultVal(ref config.mainTextNav2, CConfig.DEF_TXTNMAIN2, force);
			SetDefaultVal(ref config.subTextNav1, CConfig.DEF_TXTNSUB1, force);
			SetDefaultVal(ref config.subTextNav2, CConfig.DEF_TXTNSUB2, force);
			SetDefaultVal(ref config.mainTextIns1, CConfig.DEF_TXTIMAIN1, force);
			SetDefaultVal(ref config.mainTextIns2, CConfig.DEF_TXTIMAIN2, force);
			SetDefaultVal(ref config.subTextIns1, CConfig.DEF_TXTISUB1, force);
			SetDefaultVal(ref config.subTextIns2, CConfig.DEF_TXTISUB2, force);
		}

		/// <summary>
		/// Set the default boolean configuration value if a value doesn't already exist
		/// </summary>
		/// <param name="bValue">The current value</param>
		/// <param name="default">The default</param>
		private static void SetDefaultVal(ref bool? bValue, bool bDefault, bool force)
		{
			if (force)
				bValue = bDefault;
			else if (bValue == null)
				bValue = bDefault;
			return;
		}

		/// <summary>
		/// Set the default integer configuration value if a value doesn't already exist
		/// </summary>
		/// <param name="nValue">The current value</param>
		/// <param name="default">The default</param>
		private static void SetDefaultVal(ref int? nValue, int nDefault, bool force)
		{
			if (force)
				nValue = nDefault;
			else if (nValue == null)
				nValue = nDefault;
			return;
		}

		/// <summary>
		/// Set the default configuration value if a value doesn't already exist
		/// </summary>
		/// <param name="value">The current value</param>
		/// <param name="default">The default</param>
		private static void SetDefaultVal(ref string strValue, string strDefault, bool force)
        {
			if (force)
				strValue = strDefault;
			else if (string.IsNullOrEmpty(strValue))
				strValue = strDefault;
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
