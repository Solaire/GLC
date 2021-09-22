using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Threading;
using static GameLauncher_Console.CGameData;
using static GameLauncher_Console.CJsonWrapper;
//using static GameLauncher_Console.CRegScanner;
using static System.Environment;

namespace GameLauncher_Console
{
	// GOG Galaxy
	// [owned and installed games]
	public class PlatformGOG : IPlatform
	{
		public const GamePlatform ENUM = GamePlatform.GOG;
		public const string PROTOCOL			= "goggalaxy://";
		public const string INSTALL				= PROTOCOL + "openGameView";
		private const string START_SUFFIX		= "/command=runGame /gameId=";
		private const string START_PATH			= "/path=";
		public const string GOG_REG_GAMES		= @"SOFTWARE\WOW6432Node\GOG.com\Games";
		public const string GOG_REG_CLIENT		= @"SOFTWARE\WOW6432Node\GOG.com\GalaxyClient\paths";
		public const string GOG_GALAXY_EXE		= "\\GalaxyClient.exe";
		private const string GOG_DB				= @"\GOG.com\Galaxy\storage\galaxy-2.0.db";
		//private const string GOG_GALAXY_UNREG	= "{7258BA11-600C-430E-A759-27E2C691A335}_is1"; // HKLM32 Uninstall

		private static string _name = Enum.GetName(typeof(GamePlatform), ENUM);

		GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => _name;

        string IPlatform.Description => GetPlatformString(ENUM);

        public static void Launch() => Process.Start(PROTOCOL);

		/*
		public static void Launch()
        {
			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(GOG_REG_CLIENT, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				string launcherPath = key.GetValue("client") + GOG.GOG_GALAXY_EXE;
				if (File.Exists(launcherPath))
					Process.Start(launcherPath);
				else
				{
					//SetFgColour(cols.errorCC, cols.errorLtCC);
					CLogger.LogWarn("Cannot start {0} launcher.", _name.ToUpper());
					Console.WriteLine("ERROR: Launcher couldn't start. Is it installed properly?");
					//Console.ResetColor();
				}
			}
		}
		*/

		public static void InstallGame(CGame game)
		{
			CDock.DeleteCustomImage(game.Title);
			Process.Start(INSTALL + "/" + GetGameID(game.ID));
		}

		public static void StartGame(CGame game)
        {
			//CLogger.LogInfo("Setting up a {0} game...", GOG.NAME_LONG);
			ProcessStartInfo gogProcess = new();
			string gogClientPath = game.Launch.Contains(".") ? game.Launch.Substring(0, game.Launch.IndexOf('.') + 4) : game.Launch;
			string gogArguments = game.Launch.Contains(".") ? game.Launch[(game.Launch.IndexOf('.') + 4)..] : string.Empty;
			CLogger.LogInfo($"gogClientPath: {gogClientPath}");
			CLogger.LogInfo($"gogArguments: {gogArguments}");
			gogProcess.FileName = gogClientPath;
			gogProcess.Arguments = gogArguments;
			Process.Start(gogProcess);
			Thread.Sleep(4000);
			Process[] procs = Process.GetProcessesByName("GalaxyClient");
			foreach (Process proc in procs)
			{
                CDock.WindowMessage.ShowWindowAsync(procs[0].MainWindowHandle, CDock.WindowMessage.SW_FORCEMINIMIZE);
			}
		}

		[SupportedOSPlatform("windows")]
		public void GetGames(List<ImportGameData> gameDataList, bool expensiveIcons = false)
		{
			/*
			productId from ProductAuthorizations
			productId, installationPath from InstalledBaseProducts
			productId, images, title from LimitedDetails
			//images = icon from json
			id, gameReleaseKey from PlayTasks
			playTaskId, executablePath, commandLineArgs from PlayTaskLaunchParameters
			*/

			// Get installed games
			string db = GetFolderPath(SpecialFolder.CommonApplicationData) + GOG_DB;
			if (!File.Exists(db))
			{
				CLogger.LogInfo("{0} database not found.", _name.ToUpper());
				return;
			}
			string launcherPath = "";
			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(GOG_REG_CLIENT, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				launcherPath = key.GetValue("client") + GOG_GALAXY_EXE;
				if (!File.Exists(launcherPath))
					launcherPath = "";
			}

			try
			{
                using var con = new SQLiteConnection($"Data Source={db}");
                con.Open();

                // Get both installed and not-installed games

                using (var cmd = new SQLiteCommand(string.Format("SELECT productId from Builds"), con))
                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        int id = rdr.GetInt32(0);

                        using var cmd2 = new SQLiteCommand($"SELECT images, title from LimitedDetails WHERE productId = {id};", con);
                        using SQLiteDataReader rdr2 = cmd2.ExecuteReader();
                        while (rdr2.Read())
                        {
                            string images = rdr2.GetString(0);
                            // To be safe, we should probably confirm "gog_{id}" is correct here with
                            // "SELECT releaseKey FROM ProductsToReleaseKeys WHERE gogId = {id};"
                            string strID = $"gog_{id}";
                            string strTitle = rdr2.GetString(1);
                            string strAlias = "";
                            string strLaunch = "";
                            string strIconPath = "";
                            string strPlatform = GetPlatformString(GamePlatform.GOG);

                            strAlias = GetAlias(strTitle);
                            if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
                                strAlias = "";

                            using (var cmd3 = new SQLiteCommand($"SELECT installationPath FROM InstalledBaseProducts WHERE productId = {id};", con))
                            using (SQLiteDataReader rdr3 = cmd3.ExecuteReader())
                            {
                                while (rdr3.Read())
                                {
                                    using var cmd4 = new SQLiteCommand($"SELECT id FROM PlayTasks WHERE gameReleaseKey = '{strID}';", con);
                                    using SQLiteDataReader rdr4 = cmd4.ExecuteReader();
                                    while (rdr4.Read())
                                    {
                                        int task = rdr4.GetInt32(0);

                                        using var cmd5 = new SQLiteCommand($"SELECT executablePath, commandLineArgs FROM PlayTaskLaunchParameters WHERE playTaskId = {task};", con);
                                        using SQLiteDataReader rdr5 = cmd5.ExecuteReader();
                                        while (rdr5.Read())
                                        {
                                            // Add installed games
                                            strIconPath = rdr5.GetString(0);
                                            if (string.IsNullOrEmpty(launcherPath))
                                            {
                                                string args = rdr5.GetString(1);
                                                if (!string.IsNullOrEmpty(strLaunch))
                                                {
                                                    if (!string.IsNullOrEmpty(args))
                                                    {
                                                        strLaunch += " " + args;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                strLaunch = launcherPath + " " + START_SUFFIX + id + " " + START_PATH + "\"" + Path.GetDirectoryName(strIconPath) + "\"";
                                                if (strLaunch.Length > 8191)
                                                    strLaunch = launcherPath + " " + START_SUFFIX + id;
                                            }
                                            CLogger.LogDebug($"- {strTitle}");
                                            gameDataList.Add(new ImportGameData(strID, strTitle, strLaunch, strIconPath, "", strAlias, true, strPlatform));
                                            /*
                                            // grab last run date?
                                            using (var cmd6 = new SQLiteCommand($"SELECT lastPlayedDate FROM LastPlayedDates WHERE gameReleaseKey = {strID};", con))
                                            using (SQLiteDataReader rdr6 = cmd6.ExecuteReader())
                                            {
                                                while (rdr6.Read())
                                                {
                                                    DateTime dateLastRun = JsonSerializer.Deserialize<DateTime>("\"" + rdr5.GetString(0) + "\"");
                                                }
                                            }
                                            */
                                        }
                                    }
                                }
                            }

                            // Add not-installed games
                            if (string.IsNullOrEmpty(strLaunch) && !(bool)CConfig.GetConfigBool(CConfig.CFG_INSTONLY))
                            {
                                CLogger.LogDebug($"- *{strTitle}");
                                gameDataList.Add(new ImportGameData(strID, strTitle, "", "", "", "", false, strPlatform));

                                // Use icon from images (json) to download not-installed icons
                                if (!(bool)(CConfig.GetConfigBool(CConfig.CFG_IMGDOWN)))
                                {
                                    string iconUrl = "";
                                    using (JsonDocument document = JsonDocument.Parse(@images, jsonTrailingCommas))
                                    {
                                        iconUrl = GetStringProperty(document.RootElement, "logo2x"); // "icon" is 1:1 ratio, but in a circular frame
                                    }
                                    CDock.DownloadCustomImage(strTitle, iconUrl);
                                }
                            }
                        }
                    }
                }
                con.Close();
            }
			catch (Exception e)
			{
				CLogger.LogError(e, string.Format("Malformed {0} database output!", _name.ToUpper()));
			}
			CLogger.LogDebug("-------------------");
		}

		/// <summary>
		/// Scan the key name and extract the Steam game id
		/// </summary>
		/// <param name="key">The game string</param>
		/// <returns>Steam game ID as string</returns>
		public static string GetGameID(string key)
		{
			return key[4..];
		}
	}
}