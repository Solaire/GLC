using System;
using System.Data.SQLite;
using System.IO;
using Microsoft.Win32;
using Logger;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for Amazon games (previously Twitch game store)
	/// </summary>
    public sealed class CAmazonScanner : CBasePlatformScanner<CAmazonScanner>
    {
		private const string AMAZON_NAME = "Amazon";
		private const string AMAZON_LAUNCH = "amazon-games://play/";
		private const string AMAZON_DB = @"\Amazon Games\Data\Games\Sql\GameInstallInfo.sqlite";
		private const string AMAZON_OWN_DB = @"\Amazon Games\Data\Games\Sql\GameProductInfo.sqlite";
		//private const string AMAZON_UNINST_EXE = @"\__InstallData__\Amazon Game Remover.exe";
		//private const string AMAZON_UNINST_SUFFIX = "-m Game -p";

        protected override bool GetInstalledGames(bool expensiveIcons)
        {
			// Get installed games
			string db = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + AMAZON_DB;
			if(!File.Exists(db))
			{
				CLogger.LogInfo("{0} installed game database not found.", AMAZON_NAME.ToUpper());
				return false;
			}

			int gameCount = 0;
			try
			{
				using(var con = new SQLiteConnection($"Data Source={db}"))
				{
					con.Open();

					using(var cmd = new SQLiteCommand("SELECT Id, InstallDirectory, ProductTitle FROM DbSet;", con))
					{
						using(SQLiteDataReader rdr = cmd.ExecuteReader())
						{
							while(rdr.Read())
							{
								string strID = rdr.GetString(0);
								string strTitle = rdr.GetString(2);
								CLogger.LogDebug($"- {strTitle}");
								string strLaunch = AMAZON_LAUNCH + strID;
								string strIconPath = "";
								string strUninstall = "";

								using(RegistryKey key = Registry.CurrentUser.OpenSubKey(NODE64_REG + "\\AmazonGames/" + strTitle, RegistryKeyPermissionCheck.ReadSubTree))
								{
									if(key != null)
									{
										strIconPath = CRegHelper.GetRegStrVal(key, "DisplayIcon");
										strUninstall = CRegHelper.GetRegStrVal(key, "UninstallString");
									}
								}
								if(string.IsNullOrEmpty(strIconPath))
								{
									if(expensiveIcons)
                                    {
										strIconPath = CDirectoryHelper.FindGameBinaryFile(rdr.GetString(1), strTitle);
									}
								}
								string strAlias = CRegHelper.GetAlias(strTitle);
								string strPlatform = "Amazon";//CGameData.GetPlatformString(CGameData.GamePlatform.Amazon);

								if(!string.IsNullOrEmpty(strLaunch))
								{
									if(strAlias.Equals(strTitle, StringComparison.CurrentCultureIgnoreCase))
									{
										strAlias = "";
									}
									//gameList.Add(new GameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
									CEventDispatcher.NewGameFound(new RawGameData(strID, strTitle, strLaunch, strIconPath, strUninstall, strAlias, true, strPlatform));
									gameCount++;
								}
							}
						}
					}
				}
			}
			catch(Exception e)
			{
				CLogger.LogError(e, string.Format("Malformed {0} database output!", AMAZON_NAME.ToUpper()));
			}
			return gameCount > 0;
		}

        protected override bool GetNonInstalledGames(bool expensiveIcons)
        {
			string db = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + AMAZON_OWN_DB;
			if(!File.Exists(db))
			{
				CLogger.LogInfo("{0} not-installed game database not found.", AMAZON_NAME.ToUpper());
				return false;
			}

			int found = 0;
			CLogger.LogDebug("{0} not-installed games:", AMAZON_NAME.ToUpper());
			try
			{
				using(var con = new SQLiteConnection($"Data Source={db}"))
				{
					con.Open();

					using(var cmd = new SQLiteCommand("SELECT Id, ProductIconUrl, ProductIdStr, ProductTitle FROM DbSet;", con))  // TODO: Use ProductIconUrl to download icon
					{
						using(SQLiteDataReader rdr = cmd.ExecuteReader())
						{
							while(rdr.Read())
							{
								string strID = rdr.GetString(2); // TODO: Should I use Id or ProductIdStr?
								string strTitle = rdr.GetString(3);
								CLogger.LogDebug($"- *{strTitle}");
								string strPlatform = "Amazon";//CGameData.GetPlatformString(CGameData.GamePlatform.Amazon);
															  //gameDataList.Add(new CRegScanner.RegistryGameData(strID, strTitle, "", "", "", "", false, strPlatform));
								CEventDispatcher.NewGameFound(new RawGameData(strID, strTitle, "", "", "", "", false, strPlatform));
								found++;
							}
						}
					}
				}
			}
			catch(Exception e)
			{
				CLogger.LogError(e, string.Format("Malformed {0} database output!", AMAZON_NAME.ToUpper()));
			}
			return found > 0;
		}
    }
}
