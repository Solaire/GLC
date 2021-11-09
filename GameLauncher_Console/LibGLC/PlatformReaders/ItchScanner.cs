using System;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;
using Logger;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for Itch.io
	/// </summary>
    public sealed class CItchScanner : CBasePlatformScanner<CItchScanner>
    {
		private const string ITCH_NAME = "itch";
		private const string ITCH_DB = @"\itch\db\butler.db";

        protected override bool GetInstalledGames(bool expensiveIcons)
        {
			int gameCount = 0;

			// Get installed games
			string db = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + ITCH_DB;
			if(!File.Exists(db))
			{
				CLogger.LogInfo("{0} database not found.", ITCH_NAME.ToUpper());
				return false;
			}

			try
			{
				using(var con = new SQLiteConnection($"Data Source={db}"))
				{
					con.Open();

					// Get both installed and not-installed games

					// TODO: Use still_cover_url, or cover_url if it doesn't exist, to download not-installed icons
					using(var cmd = new SQLiteCommand(string.Format("SELECT id, title, classification, cover_url, still_cover_url FROM games;"), con))
					using(SQLiteDataReader rdr = cmd.ExecuteReader())
					{
						while(rdr.Read())
						{
							if(!rdr.GetString(2).Equals("assets"))  // i.e., just "game" or "tool"
							{
								int id = rdr.GetInt32(0);
								string strID = $"itch_{id}";
								string strTitle = rdr.GetString(1);
								string strAlias = "";
								string strLaunch = "";
								string strPlatform = "Itch";// CGameData.GetPlatformString(CGameData.GamePlatform.Itch);

								// SELECT path FROM install_locations;
								// SELECT install_folder FROM downloads;
								// SELECT verdict FROM caves;
								using(var cmd2 = new SQLiteCommand($"SELECT verdict, install_folder_name FROM caves WHERE game_id = {id};", con))
								using(SQLiteDataReader rdr2 = cmd2.ExecuteReader())
								{
									while(rdr2.Read())
									{
										string verdict = rdr2.GetString(0);
										strAlias = CRegHelper.GetAlias(strTitle);
										if(strAlias.Equals(strTitle, StringComparison.CurrentCultureIgnoreCase))
											strAlias = "";

										var options = new JsonDocumentOptions
										{
											AllowTrailingCommas = true
										};

										using(JsonDocument document = JsonDocument.Parse(@verdict, options))
										{
											string basePath = CJsonHelper.GetStringProperty(document.RootElement, "basePath");
											if(document.RootElement.TryGetProperty("candidates", out JsonElement candidates)) // 'candidates' object exists
											{
												if(!string.IsNullOrEmpty(candidates.ToString()))
												{
													foreach(JsonElement jElement in candidates.EnumerateArray())
													{
														strLaunch = string.Format("{0}\\{1}", basePath, CJsonHelper.GetStringProperty(jElement, "path"));
													}
												}
											}
											// Add installed games
											if(!string.IsNullOrEmpty(strLaunch))
											{
												CLogger.LogDebug($"- {strTitle}");
												//gameList.Add(new GameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, true, strPlatform));
												CEventDispatcher.NewGameFound(new RawGameData(strID, strTitle, strLaunch, strLaunch, "", strAlias, true, strPlatform));
												gameCount++;
											}
										}
									}
								}
								// Add not-installed games
								/*
								if(string.IsNullOrEmpty(strLaunch) && !(bool)CConfig.GetConfigBool(CConfig.CFG_INSTONLY))
								{
									CLogger.LogDebug($"- *{strTitle}");
									gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, "", "", "", "", false, strPlatform));
								}
								*/
							}
						}
					}
					con.Close();
				}
			}
			catch(Exception e)
			{
				CLogger.LogError(e, string.Format("Malformed {0} database output!", ITCH_NAME.ToUpper()));
			}
			return gameCount > 0;
		}

        protected override bool GetNonInstalledGames(bool expensiveIcons)
        {
			return false;
        }
    }
}
