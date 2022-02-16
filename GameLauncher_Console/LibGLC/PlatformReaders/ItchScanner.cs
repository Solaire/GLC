using System;
using System.Data.SQLite;
using System.Text.Json;
using Logger;
using SqlDB;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for Itch.io
	/// This scanner is using an SQLite database to get game information
	/// </summary>
	public sealed class CItchScanner : CBasePlatformScanner<CItchScanner>
    {
		private const string ITCH_DB = @"\itch\db\butler.db";

		#region SQL Queries

		/// <summary>
		/// SQL query for getting installed games
		/// </summary>
		private class CQryGetInstalledGames : CSqlQry
		{
			public CQryGetInstalledGames(CSqlConn sqlConn)
				: base("games G" +
					  " LEFT JOIN caves C ON C.game_id = G.id",
					  " G.Classification IN ('game', 'tool') AND C.game_id IS NOT NULL",
					  "",
					  sqlConn)
			{
				m_sqlRow["id"]					= new CSqlFieldString("G.id"				 , CSqlField.QryFlag.cSelRead);
				m_sqlRow["title"]				= new CSqlFieldString("G.title"				 , CSqlField.QryFlag.cSelRead);
				m_sqlRow["cover_url"]			= new CSqlFieldString("G.cover_url"			 , CSqlField.QryFlag.cSelRead);
				m_sqlRow["still_cover_url"]		= new CSqlFieldString("G.still_cover_url"	 , CSqlField.QryFlag.cSelRead);
				m_sqlRow["verdict"]				= new CSqlFieldString("C.verdict"			 , CSqlField.QryFlag.cSelRead);
				m_sqlRow["install_folder_name"] = new CSqlFieldString("C.install_folder_name", CSqlField.QryFlag.cSelRead);
			}
			public string ID
			{
				get { return m_sqlRow["id"].String; }
				set { m_sqlRow["id"].String = value; }
			}
			public string Title
			{
				get { return m_sqlRow["title"].String; }
				set { m_sqlRow["title"].String = value; }
			}
			public string CoverUrl
			{
				get { return m_sqlRow["cover_url"].String; }
				set { m_sqlRow["cover_url"].String = value; }
			}
			public string StillCoverUrl
			{
				get { return m_sqlRow["still_cover_url"].String; }
				set { m_sqlRow["still_cover_url"].String = value; }
			}
			public string Verdict
			{
				get { return m_sqlRow["verdict"].String; }
				set { m_sqlRow["verdict"].String = value; }
			}
			public string InstallFolderName
			{
				get { return m_sqlRow["install_folder_name"].String; }
				set { m_sqlRow["install_folder_name"].String = value; }
			}
		}

		/// <summary>
		/// SQL query for getting non-installed games
		/// </summary>
		private class CQryGetNonInstalledGames : CSqlQry
		{
			public CQryGetNonInstalledGames(CSqlConn sqlConn)
				: base("games G",
					  " G.Classification IN ('game', 'tool') AND NOT EXISTS (SELECT C.game_id FROM caves C WHERE C.game_id = G.id)",
					  "",
					  sqlConn)
			{
				m_sqlRow["id"]				= new CSqlFieldString("G.id"			 , CSqlField.QryFlag.cSelRead);
				m_sqlRow["title"]			= new CSqlFieldString("G.title"			 , CSqlField.QryFlag.cSelRead);
				m_sqlRow["cover_url"]		= new CSqlFieldString("G.cover_url"		 , CSqlField.QryFlag.cSelRead);
				m_sqlRow["still_cover_url"] = new CSqlFieldString("G.still_cover_url", CSqlField.QryFlag.cSelRead);
			}
			public string ID
			{
				get { return m_sqlRow["id"].String; }
				set { m_sqlRow["id"].String = value; }
			}
			public string Title
			{
				get { return m_sqlRow["title"].String; }
				set { m_sqlRow["title"].String = value; }
			}
			public string CoverUrl
			{
				get { return m_sqlRow["cover_url"].String; }
				set { m_sqlRow["cover_url"].String = value; }
			}
			public string StillCoverUrl
			{
				get { return m_sqlRow["still_cover_url"].String; }
				set { m_sqlRow["still_cover_url"].String = value; }
			}
		}

		#endregion SQL Queries

		private CItchScanner()
		{
			m_platformName = CExtensions.GetDescription(CPlatform.GamePlatform.Itch);
		}

		/// <summary>
		/// Override.
		/// Create an sql connection to the local database and retrieve games
		/// </summary>
		/// <param name="getNonInstalled">If true, try to get non-installed games</param>
		/// <param name="expensiveIcons">(not used) If true, try to get expensive icons</param>
		/// <returns>True if at least one game was found. False if no games found or if the data source is unavailable</returns>
		public override bool GetGames(bool getNonInstalled, bool expensiveIcons)
		{
			CEventDispatcher.OnPlatformStarted(m_platformName);

			CSqlConn conn = new CSqlConn(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + ITCH_DB);
			if(!conn.IsOpen())
			{
				CLogger.LogInfo("{0}: Could not open database.", m_platformName.ToUpper());
				return false;
			}

			bool success = GetInstalledGames(conn);
			if(getNonInstalled)
			{
				success = success && GetNonInstalledGames(conn);
			}
			conn.Close();
			return success;
		}


		/// <summary>
		/// Overload.
		/// Use the sql connection to get list of installed games
		/// </summary>
		/// <param name="conn">SQLite connection instance</param>
		/// <returns>True is found at least one game, false if no games found or data connection is closed</returns>
		private bool GetInstalledGames(CSqlConn conn)
        {
			if(!conn.IsOpen())
            {
				return false;
            }

			CQryGetInstalledGames qry = new CQryGetInstalledGames(conn);
			int  gameCount  = 0;
			bool isOk       = qry.Select() == SQLiteErrorCode.Ok;

			while(isOk)
            {
				string launch = "";
				var options = new JsonDocumentOptions
				{
					AllowTrailingCommas = true
				};
				using(JsonDocument document = JsonDocument.Parse(qry.Verdict, options))
				{
					string basePath = CJsonHelper.GetStringProperty(document.RootElement, "basePath");
					if( document.RootElement.TryGetProperty("candidates", out JsonElement candidates) && !string.IsNullOrEmpty(candidates.ToString()) ) // 'candidates' object exists
					{
						foreach(JsonElement jElement in candidates.EnumerateArray())
						{
							launch = string.Format("{0}\\{1}", basePath, CJsonHelper.GetStringProperty(jElement, "path"));
						}
					}
				}

				if(launch.Length > 0)
                {
					string strAlias = CRegHelper.GetAlias(qry.Title);
					if(strAlias.Equals(qry.Title, StringComparison.CurrentCultureIgnoreCase))
                    {
						strAlias = "";
					}

					CEventDispatcher.OnGameFound(new RawGameData($"itch_{qry.ID}", qry.Title, launch, launch, "", strAlias, true, m_platformName));
					gameCount++;
				}
				isOk = qry.Fetch();
            }
			return gameCount > 0;
        }

		/// <summary>
		/// Overload.
		/// Use the sql connection to get list of non-installed games
		/// </summary>
		/// <param name="conn">SQLite connection instance</param>
		/// <returns>True is found at least one game, false if no games found or data connection is closed</returns>
		private bool GetNonInstalledGames(CSqlConn conn)
		{
			if(!conn.IsOpen())
			{
				return false;
			}

			CQryGetNonInstalledGames qry = new CQryGetNonInstalledGames(conn);
			int  gameCount  = 0;
			bool isOk       = qry.Select() == SQLiteErrorCode.Ok;

			while(isOk)
			{
				CEventDispatcher.OnGameFound(new RawGameData($"itch_{qry.ID}", qry.Title, "", "", "", "", false, m_platformName));
				gameCount++;
				isOk = qry.Fetch();
			}

			return gameCount > 0;
		}

		protected override bool GetInstalledGames(bool expensiveIcons)
		{
			throw new NotSupportedException("Use the overloaded function");
		}

		protected override bool GetNonInstalledGames(bool expensiveIcons)
		{
			throw new NotSupportedException("Use the overloaded function");
		}
	}
}
