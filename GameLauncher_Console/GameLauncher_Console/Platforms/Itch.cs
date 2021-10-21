using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Runtime.Versioning;
using System.Text.Json;
using static GameLauncher_Console.CGameData;
using static GameLauncher_Console.CJsonWrapper;
using static GameLauncher_Console.CRegScanner;
using static System.Environment;

namespace GameLauncher_Console
{
	// itch
	// [owned and installed games]
	public class PlatformItch : IPlatform
	{
		public const GamePlatform ENUM			= GamePlatform.Itch;
		public const string PROTOCOL			= "itch://";
		public const string LAUNCH				= PROTOCOL + "library";
		public const string INSTALL_GAME		= PROTOCOL + "games";
		private const string ITCH_DB			= @"itch\db\butler.db"; // AppData\Roaming
		/*
		private const string ITCH_GAME_FOLDER	= "apps";
		private const string ITCH_METADATA		= ".itch\\receipt.json.gz";
		private const string ITCH_UNREG			= "itch"; // HKCU64 Uninstall
		*/

		private static readonly string _name = Enum.GetName(typeof(GamePlatform), ENUM);

		GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => _name;

        string IPlatform.Description => GetPlatformString(ENUM);

		// Can't call PROTOCOL directly as itch is launched in command line mode, and StartInfo.Redirect* don't work when ShellExecute=True
		public static void Launch()
		{
			if (OperatingSystem.IsWindows())
			{
                using RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"itch\shell\open\command", RegistryKeyPermissionCheck.ReadSubTree);
                string value = GetRegStrVal(key, null);
                string[] subs = value.Split();
                string command = "";
                string args = "";
                for (int i = 0; i < subs.Length; i++)
                {
                    if (i > 0)
                        args += subs[i];
                    else
                        command = subs[0];
                }
                CDock.StartAndRedirect(command, args.Replace("%1", LAUNCH));
            }
		}

		public static void InstallGame(CGame game)
		{
			CDock.DeleteCustomImage(game.Title);
			if (OperatingSystem.IsWindows())
			{
				try
				{
                    using RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"itch\shell\open\command", RegistryKeyPermissionCheck.ReadSubTree);
                    string[] subs = GetRegStrVal(key, null).Split(' ');
                    string command = "";
                    string args = "";
                    for (int i = 0; i > subs.Length; i++)
                    {
                        if (i > 0)
                            args += subs[i];
                        else
                            command = subs[0];
                    }
                    CDock.StartAndRedirect(command, args.Replace("%1", INSTALL_GAME + "/" + GetGameID(game.ID)));
                }
				catch (Exception e)
				{
					CLogger.LogError(e);
				}
			}
		}

        public void GetGames(List<ImportGameData> gameDataList, bool expensiveIcons = false)
		{
			// Get installed games
			string db = Path.Combine(GetFolderPath(SpecialFolder.ApplicationData), ITCH_DB);
			if (!File.Exists(db))
			{
				CLogger.LogInfo("{0} database not found.", _name.ToUpper());
				return;
			}

			try
			{
                using var con = new SQLiteConnection($"Data Source={db}");
                con.Open();

                // Get both installed and not-installed games

                using (var cmd = new SQLiteCommand("SELECT id, title, classification, cover_url, still_cover_url FROM games;", con))
                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        if (!rdr.GetString(2).Equals("assets"))  // i.e., just "game" or "tool"
                        {
                            int id = rdr.GetInt32(0);
                            string strID = $"itch_{id}";
                            string strTitle = rdr.GetString(1);
                            string strAlias = "";
                            string strLaunch = "";
                            string strPlatform = GetPlatformString(ENUM);
                            DateTime lastRun = DateTime.MinValue;

                            string iconUrl = rdr.GetString(4);
                            if (string.IsNullOrEmpty(iconUrl))
                                iconUrl = rdr.GetString(3);

                            // SELECT path FROM install_locations;
                            // SELECT install_folder FROM downloads;
                            using (var cmd2 = new SQLiteCommand($"SELECT installed_at, last_touched_at, verdict, install_folder_name FROM caves WHERE game_id = {id};", con))
                            using (SQLiteDataReader rdr2 = cmd2.ExecuteReader())
                            {
                                while (rdr2.Read())
                                {
                                    if (!rdr2.IsDBNull(1))
                                    {
                                        lastRun = rdr2.GetDateTime(1);
                                        //CLogger.LogDebug("    last_touched_at: " + rdr2.GetString(1) + " -> " + lastRun.ToShortDateString());
                                    }
                                    //else if (!rdr2.IsDBNull(0))
                                    //    lastRun = rdr2.GetDateTime(0);
                                    string verdict = rdr2.GetString(2);
                                    //strAlias = rdr2.GetString(3);
                                    strAlias = GetAlias(strTitle);
                                    if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
                                        strAlias = "";

                                    using JsonDocument document = JsonDocument.Parse(@verdict, jsonTrailingCommas);
                                    string basePath = GetStringProperty(document.RootElement, "basePath");
                                    if (document.RootElement.TryGetProperty("candidates", out JsonElement candidates) && !string.IsNullOrEmpty(candidates.ToString()))
                                    {
                                        foreach (JsonElement jElement in candidates.EnumerateArray())
                                        {
                                            strLaunch = string.Format("{0}\\{1}", basePath, GetStringProperty(jElement, "path"));
                                        }
                                    }
                                    // Add installed games
                                    if (!string.IsNullOrEmpty(strLaunch))
                                    {
                                        CLogger.LogDebug($"- {strTitle}");
                                        gameDataList.Add(new ImportGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, true, strPlatform, dateLastRun:lastRun));

                                        // Use still_cover_url, or cover_url if it doesn't exist, to download missing icons
                                        if (!(bool)(CConfig.GetConfigBool(CConfig.CFG_IMGDOWN)) &&
                                            !Path.GetExtension(strLaunch).Equals(".exe", CDock.IGNORE_CASE))
                                        {
                                            CDock.DownloadCustomImage(strTitle, iconUrl);
                                        }
                                    }
                                }
                            }
                            // Add not-installed games
                            if (string.IsNullOrEmpty(strLaunch) && !(bool)CConfig.GetConfigBool(CConfig.CFG_INSTONLY))
                            {
                                CLogger.LogDebug($"- *{strTitle}");
                                gameDataList.Add(new ImportGameData(strID, strTitle, "", "", "", "", false, strPlatform));

                                // Use still_cover_url, or cover_url if it doesn't exist, to download not-installed icons
                                if (!(bool)(CConfig.GetConfigBool(CConfig.CFG_IMGDOWN)))
                                    CDock.DownloadCustomImage(strTitle, iconUrl);
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
		/// Scan the key name and extract the Itch game id
		/// </summary>
		/// <param name="key">The game string</param>
		/// <returns>itch game ID as string</returns>
		public static string GetGameID(string key)
		{
			return key[5..];
		}

        /*
		/// <summary>
		/// Decompress gzip file [no longer necessary after moving to SQLite method]
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