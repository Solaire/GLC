using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;
using static GameLauncher_Console.CJsonWrapper;
using static GameLauncher_Console.CRegScanner;
using static System.Environment;

namespace GameLauncher_Console
{
	// itch
	// [owned and installed games]
	public class PlatformItch : IPlatform
	{
		public const CGameData.GamePlatform ENUM = CGameData.GamePlatform.Itch;
		public const string NAME				= "itch";
		public const string DESCRIPTION			= "itch";
		public const string PROTOCOL			= "itch://";
		public const string LAUNCH				= PROTOCOL + "library";
		public const string INSTALL				= PROTOCOL + "games";
		private const string ITCH_DB			= @"\itch\db\butler.db";
		/*
		private const string ITCH_GAME_FOLDER	= "apps";
		private const string ITCH_METADATA		= ".itch\\receipt.json.gz";
		private const string ITCH_UNREG			= "itch"; // HKCU64 Uninstall
		*/

		CGameData.GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => NAME;

        string IPlatform.Description => DESCRIPTION;

		// Can't call PROTOCOL directly as itch is launched in command line mode, and StartInfo.Redirect* don't work when ShellExecute=True
		public static void Launch()
		{
			using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Classes\\itch\\shell\\open\\command", RegistryKeyPermissionCheck.ReadSubTree))
			{
				string value = key.GetValue(null).ToString();
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
				args.Replace("%1", LAUNCH);
				CLogger.LogDebug("command: " + command + ", parameters: " + args);
				CDock.StartAndRedirect(command, args);
			}
		}

		public static void InstallGame(CGameData.CGame game)
		{
			CDock.DeleteCustomImage(game.Title);
			try
			{
				using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("\\itch\\shell\\open\\command", RegistryKeyPermissionCheck.ReadSubTree))
				{
					string[] subs = ((string)key.GetValue(null)).Split(' ');
					string command = "";
					string parameters = "";
					for (int i = 0; i > subs.Length; i++)
					{
						if (i > 0)
							parameters += subs[i];
						else
							command = subs[0];
					}
					parameters.Replace("%1", INSTALL + "/" + GetGameID(game.ID));
					CDock.StartAndRedirect(command, parameters);
				}
			}
			catch (Exception e)
            {
				CLogger.LogError(e);
            }
		}

        public void GetGames(List<RegistryGameData> gameDataList)
		{
			// Get installed games
			string db = GetFolderPath(SpecialFolder.ApplicationData) + ITCH_DB;
			if (!File.Exists(db))
			{
				CLogger.LogInfo("{0} database not found.", NAME.ToUpper());
				return;
			}

			try
			{
				using (var con = new SQLiteConnection($"Data Source={db}"))
				{
					con.Open();

					// Get both installed and not-installed games

					using (var cmd = new SQLiteCommand(string.Format("SELECT id, title, classification, cover_url, still_cover_url FROM games;"), con))
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
										strAlias = GetAlias(strTitle);
										if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
											strAlias = "";

										using (JsonDocument document = JsonDocument.Parse(@verdict, jsonTrailingCommas))
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
												gameDataList.Add(new RegistryGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, true, strPlatform));
											}
										}
									}
								}
								// Add not-installed games
								if (string.IsNullOrEmpty(strLaunch) && !(bool)CConfig.GetConfigBool(CConfig.CFG_INSTONLY))
								{
									CLogger.LogDebug($"- *{strTitle}");
									gameDataList.Add(new RegistryGameData(strID, strTitle, "", "", "", "", false, strPlatform));

									// Use still_cover_url, or cover_url if it doesn't exist, to download not-installed icons
									if ((bool)(CConfig.GetConfigBool(CConfig.CFG_IMGDOWN)))
									{
										string iconUrl = rdr.GetString(4);
										if (string.IsNullOrEmpty(iconUrl))
											iconUrl = rdr.GetString(3);
										CDock.DownloadCustomImage(strTitle, iconUrl);
									}
								}
							}
						}
					}
					con.Close();
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e, string.Format("Malformed {0} database output!", NAME.ToUpper()));
			}
			CLogger.LogDebug("-------------------");
		}

		public void GetGames(List<RegistryGameData> gameDataList, bool expensiveIcons)
		{
			GetGames(gameDataList);
		}

		/// <summary>
		/// Scan the key name and extract the Itch game id
		/// </summary>
		/// <param name="key">The game string</param>
		/// <returns>itch game ID as string</returns>
		public static string GetGameID(string key)
		{
			return key.Substring(5);
		}
	}
}