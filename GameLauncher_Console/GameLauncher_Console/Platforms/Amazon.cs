using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Text.Json;
using static GameLauncher_Console.CGameData;
using static GameLauncher_Console.CJsonWrapper;
using static GameLauncher_Console.CRegScanner;
using static System.Environment;

namespace GameLauncher_Console
{
	// Amazon Games
	// [owned and installed games]
	public class PlatformAmazon : IPlatform
	{
		public const GamePlatform ENUM			= GamePlatform.Amazon;
		public const string PROTOCOL			= "amazon-games://";
		public const string START_GAME			= PROTOCOL + "play";
		public const string UNINST_GAME			= @"__InstallData__\Amazon Game Remover.exe";
        public const string UNINST_ARGS			= "-m Game -p";
        private const string AMAZON_DB			= @"Amazon Games\Data\Games\Sql\GameInstallInfo.sqlite"; // AppData\Local
		private const string AMAZON_OWN_DB		= @"Amazon Games\Data\Games\Sql\GameProductInfo.sqlite"; // AppData\Local
		//private const string AMAZON_UNREG		= @"{4DD10B06-78A4-4E6F-AA39-25E9C38FA568}"; // HKCU64 Uninstall

		private static readonly string _name = Enum.GetName(typeof(GamePlatform), ENUM);

		GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => _name;

        string IPlatform.Description => GetPlatformString(ENUM);

        public static void Launch()
        {
            if (OperatingSystem.IsWindows())
                CDock.StartShellExecute(PROTOCOL);
            else
                Process.Start(PROTOCOL);
        }

		public static void InstallGame(CGame game)
		{
			CDock.DeleteCustomImage(game.Title);
            if (OperatingSystem.IsWindows())
                CDock.StartShellExecute(START_GAME + "/" + game.ID);
            else
                Process.Start(START_GAME + "/" + game.ID);
		}

		[SupportedOSPlatform("windows")]
		public void GetGames(List<ImportGameData> gameDataList, bool expensiveIcons = false)
        {
			List<string> azIds = new();

			// Get installed games
			string db = Path.Combine(GetFolderPath(SpecialFolder.LocalApplicationData), AMAZON_DB);
			if (!File.Exists(db))
			{
				CLogger.LogInfo("{0} installed game database not found.", _name.ToUpper());
				//return;
			}

			try
			{
                using var con = new SQLiteConnection($"Data Source={db}");
                con.Open();

                using var cmd = new SQLiteCommand("SELECT Id, InstallDirectory, ProductTitle FROM DbSet;", con);
                using SQLiteDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    string dir = rdr.GetString(1);
                    string strID = rdr.GetString(0);
                    azIds.Add(strID);
                    string strTitle = rdr.GetString(2);
                    CLogger.LogDebug($"- {strTitle}");
                    string strLaunch = START_GAME + "/" + strID;
                    string strIconPath = "";
                    string strUninstall = "";

                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(Path.Combine(NODE64_REG, "AmazonGames", strTitle), RegistryKeyPermissionCheck.ReadSubTree))
                    {
                        if (key != null)
                        {
                            strIconPath = GetRegStrVal(key, "DisplayIcon");
                            strUninstall = GetRegStrVal(key, "UninstallString");
                        }
                    }
                    if (string.IsNullOrEmpty(strIconPath))
                    {
                        if (expensiveIcons)
                        {
                            bool success = false;
                            string fuelPath = Path.Combine(dir, "fuel.json");
                            if (File.Exists(fuelPath))
                            {
                                string strDocumentData = File.ReadAllText(fuelPath);

                                if (!string.IsNullOrEmpty(strDocumentData))
                                {
                                    using JsonDocument document = JsonDocument.Parse(@strDocumentData, jsonTrailingCommas);
                                    document.RootElement.TryGetProperty("Main", out JsonElement main);
                                    if (!main.Equals(null))
                                    {
                                        string iconFile = GetStringProperty(main, "Command");
                                        if (!string.IsNullOrEmpty(strIconPath))
                                        {
                                            strIconPath = Path.Combine(dir, iconFile);
                                            success = true;
                                        }
                                    }
                                }
                            }
                            if (!success)
                                strIconPath = CGameFinder.FindGameBinaryFile(dir, strTitle);
                        }
                    }
                    if (string.IsNullOrEmpty(strUninstall))
                    {
                        strUninstall = Path.Combine(Directory.GetParent(dir).FullName, UNINST_GAME) + " " + UNINST_ARGS + " " + strID;
                    }
                    string strAlias = GetAlias(strTitle);
                    string strPlatform = GetPlatformString(ENUM);

                    if (!string.IsNullOrEmpty(strLaunch))
                    {
                        if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
                            strAlias = "";
                        gameDataList.Add(new ImportGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
                    }
                }
                con.Close();
            }
			catch (Exception e)
			{
				CLogger.LogError(e, string.Format("Malformed {0} database output!", _name.ToUpper()));
			}

			// Get not-installed games
			if (!(bool)CConfig.GetConfigBool(CConfig.CFG_INSTONLY))
			{
				db = Path.Combine(GetFolderPath(SpecialFolder.LocalApplicationData), AMAZON_OWN_DB);
				if (!File.Exists(db))
					CLogger.LogInfo("{0} not-installed game database not found.", _name.ToUpper());
				else
				{
					CLogger.LogDebug("{0} not-installed games:", _name.ToUpper());
					try
					{
                        using var con = new SQLiteConnection($"Data Source={db}");
                        con.Open();

                        using var cmd = new SQLiteCommand("SELECT Id, ProductIconUrl, ProductIdStr, ProductTitle FROM DbSet;", con);
                        using SQLiteDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            bool found = false;
                            string strID = rdr.GetString(2); // TODO: Should I use Id or ProductIdStr?
                            foreach (string id in azIds)
                            {
                                if (id.Equals(strID))
                                    found = true;
                            }
                            if (!found)
                            {
                                string strTitle = rdr.GetString(3);
                                CLogger.LogDebug($"- *{strTitle}");
                                string strPlatform = GetPlatformString(ENUM);
                                gameDataList.Add(new ImportGameData(strID, strTitle, "", "", "", "", false, strPlatform));

                                // Use ProductIconUrl to download not-installed icons
                                if (!(bool)(CConfig.GetConfigBool(CConfig.CFG_IMGDOWN)))
                                {
                                    string iconUrl = rdr.GetString(1);
                                    CDock.DownloadCustomImage(strTitle, iconUrl);
                                }
                            }
                        }
                        con.Close();
                    }
					catch (Exception e)
					{
						CLogger.LogError(e, string.Format("Malformed {0} database output!", _name.ToUpper()));
					}
				}
			}
			CLogger.LogDebug("-------------------");
		}

        /*
		/// <summary>
		/// Scan the key name and extract the Amazon game id [no longer necessary after moving to SQLite method]
		/// </summary>
		/// <param name="key">The game string</param>
		/// <returns>Amazon game ID as string</returns>
		private static string GetGameID(string key)
		{
			return key[(key.LastIndexOf(" -p ") + 4)..];
		}
        */
	}
}