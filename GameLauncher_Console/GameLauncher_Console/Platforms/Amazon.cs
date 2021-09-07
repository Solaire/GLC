using Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using static GameLauncher_Console.CRegScanner;
using static System.Environment;

namespace GameLauncher_Console
{
	// Amazon Games
	// [owned and installed games]
	public class PlatformAmazon : IPlatform
	{
		public const CGameData.GamePlatform ENUM = CGameData.GamePlatform.Amazon;
		public const string NAME				= "Amazon";
		public const string DESCRIPTION			= "Amazon";
		public const string PROTOCOL			= "amazon-games://";
		public const string START_GAME			= PROTOCOL + "play";
		//const string UNINST_EXE = @"\__InstallData__\Amazon Game Remover.exe";
		//const string UNINST_SUFFIX = "-m Game -p";
		const string AMAZON_DB = @"\Amazon Games\Data\Games\Sql\GameInstallInfo.sqlite";
		const string AMAZON_OWN_DB = @"\Amazon Games\Data\Games\Sql\GameProductInfo.sqlite";
		//private const string AMAZON_UNREG		= @"{4DD10B06-78A4-4E6F-AA39-25E9C38FA568}"; // HKCU64 Uninstall

		CGameData.GamePlatform IPlatform.Enum => ENUM;

		string IPlatform.Name => NAME;

        string IPlatform.Description => DESCRIPTION;

        public static void Launch() => Process.Start(PROTOCOL);

		public static void InstallGame(CGameData.CGame game)
		{
			CDock.DeleteCustomImage(game.Title);
			Process.Start(START_GAME + "/" + game.ID);
		}

        public void GetGames(List<RegistryGameData> gameDataList) => GetGames(gameDataList, false);

        public void GetGames(List<RegistryGameData> gameDataList, bool expensiveIcons)
        {
			// Get installed games
			string db = GetFolderPath(SpecialFolder.LocalApplicationData) + AMAZON_DB;
			if (!File.Exists(db))
			{
				CLogger.LogInfo("{0} installed game database not found.", NAME.ToUpper());
				//return;
			}

			try
			{
				using (var con = new SQLiteConnection($"Data Source={db}"))
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
								string strLaunch = START_GAME + "/" + strID;
								string strIconPath = "";
								string strUninstall = "";

								using (RegistryKey key = Registry.CurrentUser.OpenSubKey(NODE64_REG + "\\AmazonGames/" + strTitle, RegistryKeyPermissionCheck.ReadSubTree))
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
										strIconPath = CGameFinder.FindGameBinaryFile(rdr.GetString(1), strTitle);
								}
								string strAlias = GetAlias(strTitle);
								string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.Amazon);

								if (!string.IsNullOrEmpty(strLaunch))
								{
									if (strAlias.Equals(strTitle, CDock.IGNORE_CASE))
										strAlias = "";
									gameDataList.Add(new RegistryGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				CLogger.LogError(e, string.Format("Malformed {0} database output!", NAME.ToUpper()));
			}

			// Get not-installed games
			if (!(bool)CConfig.GetConfigBool(CConfig.CFG_INSTONLY))
			{
				db = GetFolderPath(SpecialFolder.LocalApplicationData) + AMAZON_OWN_DB;
				if (!File.Exists(db))
					CLogger.LogInfo("{0} not-installed game database not found.", NAME.ToUpper());
				else
				{
					CLogger.LogDebug("{0} not-installed games:", NAME.ToUpper());
					try
					{
						using (var con = new SQLiteConnection($"Data Source={db}"))
						{
							con.Open();

							using (var cmd = new SQLiteCommand("SELECT Id, ProductIconUrl, ProductIdStr, ProductTitle FROM DbSet;", con))
							{
								using (SQLiteDataReader rdr = cmd.ExecuteReader())
								{
									while (rdr.Read())
									{
										string strID = rdr.GetString(2); // TODO: Should I use Id or ProductIdStr?
										string strTitle = rdr.GetString(3);
										CLogger.LogDebug($"- *{strTitle}");
										string strPlatform = CGameData.GetPlatformString(CGameData.GamePlatform.Amazon);
										gameDataList.Add(new RegistryGameData(strID, strTitle, "", "", "", "", false, strPlatform));

										// Use ProductIconUrl to download not-installed icons
										if ((bool)(CConfig.GetConfigBool(CConfig.CFG_IMGDOWN)))
										{
											string iconUrl = rdr.GetString(1);
											CDock.DownloadCustomImage(strTitle, iconUrl);
										}
									}
								}
							}
						}
					}
					catch (Exception e)
					{
						CLogger.LogError(e, string.Format("Malformed {0} database output!", NAME.ToUpper()));
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
			return key.Substring(key.LastIndexOf(" -p ") + 4);
		}
		*/
	}
}