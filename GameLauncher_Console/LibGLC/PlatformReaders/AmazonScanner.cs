using System;
using System.Data.SQLite;
using System.IO;
using Microsoft.Win32;
using Logger;
using SqlDB;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for Amazon games (previously Twitch game store)
	/// This scanner is using an SQLite database and Registry to get game information
	/// </summary>
	public sealed class CAmazonScanner : CBasePlatformScanner<CAmazonScanner>
    {
		private const string AMAZON_LAUNCH = "amazon-games://play/";
		private const string AMAZON_DB = @"\Amazon Games\Data\Games\Sql\GameInstallInfo.sqlite";
		private const string AMAZON_OWN_DB = @"\Amazon Games\Data\Games\Sql\GameProductInfo.sqlite";

		#region SQL Queries

		/// <summary>
		/// SQL query for getting installed games
		/// </summary>
		private class CQryGetInstalledGames : CSqlQry
		{
			public CQryGetInstalledGames(CSqlConn sqlConn)
				: base("DbSet", "LD.Title is not NULL", "",sqlConn)
			{
				m_sqlRow["id"]				 = new CSqlFieldString("id"				 , CSqlField.QryFlag.cSelRead);
				m_sqlRow["InstallDirectory"] = new CSqlFieldString("InstallDirectory", CSqlField.QryFlag.cSelRead);
				m_sqlRow["ProductTitle"]	 = new CSqlFieldString("ProductTitle"	 , CSqlField.QryFlag.cSelRead);
			}
			public string ID
			{
				get { return m_sqlRow["id"].String; }
				set { m_sqlRow["id"].String = value; }
			}
			public string InstallDirectory
			{
				get { return m_sqlRow["InstallDirectory"].String; }
				set { m_sqlRow["InstallDirectory"].String = value; }
			}
			public string ProductTitle
			{
				get { return m_sqlRow["ProductTitle"].String; }
				set { m_sqlRow["ProductTitle"].String = value; }
			}
		}

		/// <summary>
		/// SQL query for getting non-installled games
		/// </summary>
		private class CQryGetNonInstalledGames : CSqlQry
		{
			public CQryGetNonInstalledGames(CSqlConn sqlConn)
				: base("DbSet", "LD.Title is not NULL", "", sqlConn)
			{
				m_sqlRow["id"]				= new CSqlFieldString("id"			  , CSqlField.QryFlag.cSelRead);
				m_sqlRow["ProductIconUrl"]	= new CSqlFieldString("ProductIconUrl", CSqlField.QryFlag.cSelRead);
				m_sqlRow["ProductIdStr"]	= new CSqlFieldString("ProductIdStr"  , CSqlField.QryFlag.cSelRead);
				m_sqlRow["ProductTitle"]	= new CSqlFieldString("ProductTitle"  , CSqlField.QryFlag.cSelRead);
			}
			public string ID
			{
				get { return m_sqlRow["id"].String; }
				set { m_sqlRow["id"].String = value; }
			}
			public string ProductIconUrl
			{
				get { return m_sqlRow["ProductIconUrl"].String; }
				set { m_sqlRow["ProductIconUrl"].String = value; }
			}
			public string ProductIdStr
			{
				get { return m_sqlRow["ProductIdStr"].String; }
				set { m_sqlRow["ProductIdStr"].String = value; }
			}
			public string ProductTitle
			{
				get { return m_sqlRow["ProductTitle"].String; }
				set { m_sqlRow["ProductTitle"].String = value; }
			}
		}

		#endregion SQL Queries

		private CAmazonScanner()
        {
			m_platformName = CExtensions.GetDescription(CPlatform.GamePlatform.Amazon);
		}

        protected override bool GetInstalledGames(bool expensiveIcons)
        {
			CSqlConn conn = new CSqlConn(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + AMAZON_DB);
			if(!conn.IsOpen())
			{
				CLogger.LogInfo("{0}: Could not open database.", m_platformName.ToUpper());
				return false;
			}

			CQryGetInstalledGames qry = new CQryGetInstalledGames(conn);
			int  gameCount  = 0;
			bool isOk       = qry.Select() == SQLiteErrorCode.Ok;

			while(isOk)
            {
				string id        = qry.ID;
				string title     = qry.ProductTitle;
				string launch	 = AMAZON_LAUNCH + id;
				string iconPath  = "";
				string uninstall = "";
				string alias     = "";

				using(RegistryKey key = Registry.CurrentUser.OpenSubKey(NODE64_REG + "\\AmazonGames/" + title, RegistryKeyPermissionCheck.ReadSubTree))
				{
					if(key != null)
					{
						iconPath = CRegHelper.GetRegStrVal(key, "DisplayIcon");
						uninstall = CRegHelper.GetRegStrVal(key, "UninstallString");
					}
				}
				if(string.IsNullOrEmpty(iconPath))
				{
					if(expensiveIcons)
					{
						iconPath = CDirectoryHelper.FindGameBinaryFile(qry.InstallDirectory, title);
					}
				}
				alias = CRegHelper.GetAlias(title);

				if(!string.IsNullOrEmpty(launch))
				{
					if(alias.Equals(title, StringComparison.CurrentCultureIgnoreCase))
					{
						alias = "";
					}
					CEventDispatcher.OnGameFound(new RawGameData(id, title, launch, iconPath, uninstall, alias, true, m_platformName));
					gameCount++;
				}

				isOk = qry.Fetch();
            }
			conn.Close();
			return gameCount > 0;
		}

        protected override bool GetNonInstalledGames(bool expensiveIcons)
        {
			CSqlConn conn = new CSqlConn(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + AMAZON_OWN_DB);
			if(!conn.IsOpen())
			{
				CLogger.LogInfo("{0}: non-installed game database not found.", m_platformName.ToUpper());
				return false;
			}

			CQryGetNonInstalledGames qry = new CQryGetNonInstalledGames(conn);
			int  gameCount  = 0;
			bool isOk       = qry.Select() == SQLiteErrorCode.Ok;

			while(isOk)
            {
				// TODO: Should I use Id or ProductIdStr?
				// TODO: Use ProductIconUrl to download icon
				CEventDispatcher.OnGameFound(new RawGameData(qry.ProductIdStr, qry.ProductTitle, "", "", "", "", false, m_platformName)); 
				gameCount++;

				isOk = qry.Fetch();
            }
			conn.Close();
			return gameCount > 0;
		}
    }
}
