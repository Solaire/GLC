using System;
using System.Data.SQLite;
using System.IO;
using Microsoft.Win32;
using Logger;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for GOG (formely Good Old Games)
	/// </summary>
	public sealed class CGogScanner : CBasePlatformScanner<CGogScanner>
    {
		private const string GOG_NAME = "GOG";
		private const string GOG_DB = @"\GOG.com\Galaxy\storage\galaxy-2.0.db";
		private const string GOG_LAUNCH = " /command=runGame /gameId=";
		private const string GOG_PATH = " /path=";
		private const string GOG_GALAXY_EXE = "\\GalaxyClient.exe";
		private const string GOG_REG_CLIENT = @"SOFTWARE\WOW6432Node\GOG.com\GalaxyClient\paths";

		private CGogScanner()
		{
			m_platformName = CExtensions.GetDescription(CPlatform.GamePlatform.GOG);
		}

		protected override bool GetInstalledGames(bool expensiveIcons)
        {
			/*
			productId from ProductAuthorizations
			productId, installationPath from InstalledBaseProducts
			productId, images, title from LimitedDetails
			//images = icon from json
			id, gameReleaseKey from PlayTasks
			playTaskId, executablePath, commandLineArgs from PlayTaskLaunchParameters
			*/

			int gameCount = 0;

			// Get installed games
			string db = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + GOG_DB;
			if(!File.Exists(db))
			{
				CLogger.LogInfo("{0} database not found.", m_platformName.ToUpper());
				return false;
			}
			string launcherPath = "";
			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(GOG_REG_CLIENT, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				launcherPath = key.GetValue("client") + GOG_GALAXY_EXE;
				if(!File.Exists(launcherPath))
				{
					launcherPath = "";
				}
			}

			try
			{
				using(var con = new SQLiteConnection($"Data Source={db}"))
				{
					con.Open();

					// Get both installed and not-installed games

					using(var cmd = new SQLiteCommand(string.Format("SELECT productId from ProductAuthorizations"), con))
					using(SQLiteDataReader rdr = cmd.ExecuteReader())
					{
						while(rdr.Read())
						{
							int id = rdr.GetInt32(0);

							using(var cmd2 = new SQLiteCommand($"SELECT images, title from LimitedDetails WHERE productId = {id};", con))
							using(SQLiteDataReader rdr2 = cmd2.ExecuteReader())
							{
								while(rdr2.Read())
								{
									// To be safe, we should probably confirm "gog_{id}" is correct here with
									// "SELECT releaseKey FROM ProductsToReleaseKeys WHERE gogId = {id};"
									string strID = $"gog_{id}";
									string strTitle = rdr2.GetString(1);
									string strAlias = "";
									string strLaunch = "";
									string strIconPath = "";
									string strPlatform = m_platformName;

									strAlias = CRegHelper.GetAlias(strTitle);
									if(strAlias.Equals(strTitle, StringComparison.CurrentCultureIgnoreCase))
									{
										strAlias = "";
									}

									using(var cmd3 = new SQLiteCommand($"SELECT installationPath FROM InstalledBaseProducts WHERE productId = {id};", con))
									using(SQLiteDataReader rdr3 = cmd3.ExecuteReader())
									{
										while(rdr3.Read())
										{
											using(var cmd4 = new SQLiteCommand($"SELECT id FROM PlayTasks WHERE gameReleaseKey = '{strID}';", con))
											using(SQLiteDataReader rdr4 = cmd4.ExecuteReader())
											{
												while(rdr4.Read())
												{
													int task = rdr4.GetInt32(0);

													using(var cmd5 = new SQLiteCommand($"SELECT executablePath, commandLineArgs FROM PlayTaskLaunchParameters WHERE playTaskId = {task};", con))
													using(SQLiteDataReader rdr5 = cmd5.ExecuteReader())
													{
														while(rdr5.Read())
														{
															// Add installed games
															strIconPath = rdr5.GetString(0);
															if(string.IsNullOrEmpty(launcherPath))
															{
																string args = rdr5.GetString(1);
																if(!string.IsNullOrEmpty(strLaunch))
																{
																	if(!string.IsNullOrEmpty(args))
																	{
																		strLaunch += " " + args;
																	}
																}
															}
															else
															{
																strLaunch = launcherPath + GOG_LAUNCH + id + GOG_PATH + "\"" + Path.GetDirectoryName(strIconPath) + "\"";
																if(strLaunch.Length > 8191)
																{
																	strLaunch = launcherPath + GOG_LAUNCH + id;
																}
															}
															CLogger.LogDebug($"- {strTitle}");
															CEventDispatcher.OnGameFound(new RawGameData(strID, strTitle, strLaunch, strIconPath, "", strAlias, true, strPlatform));
															gameCount++;
														}
													}
												}
											}
										}
									}

									// Add not-installed games
									// TODO
									/*
									if(string.IsNullOrEmpty(strLaunch) && !(bool)CConfig.GetConfigBool(CConfig.CFG_INSTONLY))
									{
										// TODO: Use icon from images (json) to download icons
										/*
										string images = rdr2.GetString(0);
										string iconUrl = "";

										var options = new JsonDocumentOptions
										{
											AllowTrailingCommas = true
										};

										using (JsonDocument document = JsonDocument.Parse(@images, options))
										{
											iconUrl = GetStringProperty(document.RootElement, "icon");
										}

										if (!string.IsNullOrEmpty(iconUrl))
										{
											string iconFile = string.Format("{0}.{1}", strTitle, Path.GetExtension(iconUrl))
											using (var client = new WebClient())
											{
												client.DownloadFile(iconUrl, $"customImages\\{iconFile}");
											}
										}
										* /
										CLogger.LogDebug($"- *{strTitle}");
										gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, "", "", "", "", false, strPlatform));
									}
									*/
								}
							}
						}
					}
					con.Close();
				}
			}
			catch(Exception e)
			{
				CLogger.LogError(e, string.Format("Malformed {0} database output!", m_platformName.ToUpper()));
			}
			return gameCount > 0;
		}

        protected override bool GetNonInstalledGames(bool expensiveIcons)
        {
			return false;
        }
    }
}
