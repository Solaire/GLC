using System;
using System.Data.SQLite;
using System.IO;
using Microsoft.Win32;
using Logger;
using SqlDB;

namespace LibGLC.PlatformReaders
{
	/// <summary>
	/// Scanner for GOG (formely Good Old Games)
	/// This scanner is using an SQLite database to get game information
	/// NOTE/TODO: the galaxy-2.0.db also contains details of other platforms. If the user is linked, we could use this in the future
	/// </summary>
	public sealed class CGogScanner : CBasePlatformScanner<CGogScanner>
    {
		private const string GOG_DB = @"\GOG.com\Galaxy\storage\galaxy-2.0.db";
		private const string GOG_LAUNCH = " /command=runGame /gameId=";
		private const string GOG_PATH = " /path=";
		private const string GOG_GALAXY_EXE = "\\GalaxyClient.exe";
		private const string GOG_REG_CLIENT = @"SOFTWARE\WOW6432Node\GOG.com\GalaxyClient\paths";

        #region SQL queries

        /// <summary>
        /// SQL query for getting installed games
        /// </summary>
        private class CQryGetInstalledGames : CSqlQry
		{
			public CQryGetInstalledGames(CSqlConn sqlConn)
				: base("PlayTasks PT" +
					  " LEFT JOIN LimitedDetails LD ON LD.ProductID = replace(PT.GameReleaseKey, 'gog_', '')"+
					  " LEFT JOIN PlayTaskLaunchParameters PTLP ON PT.id = PTLP.PlayTaskId", 
					  "LD.Title is not NULL",
					  "",
					  sqlConn)
			{
				m_sqlRow["GameReleaseKey"]	= new CSqlFieldString("PT.GameReleaseKey"	, CSqlField.QryFlag.cSelRead);
				m_sqlRow["Title"]			= new CSqlFieldString("LD.Title"			, CSqlField.QryFlag.cSelRead);
				m_sqlRow["Images"]			= new CSqlFieldString("LD.Images"			, CSqlField.QryFlag.cSelRead);
				m_sqlRow["Label"]			= new CSqlFieldString("PTLP.Label"			, CSqlField.QryFlag.cSelRead);
				m_sqlRow["ExecutablePath"]  = new CSqlFieldString("PTLP.ExecutablePath" , CSqlField.QryFlag.cSelRead);
				m_sqlRow["CommandLineArgs"] = new CSqlFieldString("PTLP.CommandLineArgs", CSqlField.QryFlag.cSelRead);
			}
			public string GameReleaseKey
			{
				get { return m_sqlRow["GameReleaseKey"].String; }
				set { m_sqlRow["GameReleaseKey"].String = value; }
			}
			public string Title
			{
				get { return m_sqlRow["Title"].String; }
				set { m_sqlRow["Title"].String = value; }
			}
			public string Images
			{
				get { return m_sqlRow["Images"].String; }
				set { m_sqlRow["Images"].String = value; }
			}
			public string Label
			{
				get { return m_sqlRow["Label"].String; }
				set { m_sqlRow["Label"].String = value; }
			}
			public string ExecutablePath
			{
				get { return m_sqlRow["ExecutablePath"].String; }
				set { m_sqlRow["ExecutablePath"].String = value; }
			}
			public string CommandLineArgs
			{
				get { return m_sqlRow["CommandLineArgs"].String; }
				set { m_sqlRow["CommandLineArgs"].String = value; }
			}
		}

		/// <summary>
		/// SQL query for getting non-installed games
		/// </summary>
		private class CQryGetNonInstalledGames : CSqlQry
		{
			public CQryGetNonInstalledGames(CSqlConn sqlConn)
				: base("LibraryReleases LR" +
					  " LEFT JOIN LimitedDetails LD on LD.ProductID = replace(LR.ReleaseKey, 'gog_', '')",
					  " LR.ReleaseKey like '%gog_%' AND LD.Title IS NOT NULL AND NOT EXISTS (SELECT PT.id from PlayTasks PT where PT.GameReleaseKey = LR.ReleaseKey)", 
					  "", sqlConn)
			{
				m_sqlRow["ReleaseKey"]	= new CSqlFieldString("LR.ReleaseKey", CSqlField.QryFlag.cSelRead);
				m_sqlRow["Title"]		= new CSqlFieldString("LD.Title"	 , CSqlField.QryFlag.cSelRead);
				m_sqlRow["Images"]		= new CSqlFieldString("LD.Images"	 , CSqlField.QryFlag.cSelRead);
			}
			public string ReleaseKey
			{
				get { return m_sqlRow["ReleaseKey"].String; }
				set { m_sqlRow["ReleaseKey"].String = value; }
			}
			public string Title
			{
				get { return m_sqlRow["Title"].String; }
				set { m_sqlRow["Title"].String = value; }
			}
			public string Images
			{
				get { return m_sqlRow["Images"].String; }
				set { m_sqlRow["Images"].String = value; }
			}
		}

		#endregion SQL queries

		private CGogScanner()
		{
			m_platformName = CExtensions.GetDescription(CPlatform.GamePlatform.GOG);
		}

		/// <summary>
		/// Override.
		/// Create an sql connection to gog's local database and retrieve games
		/// </summary>
		/// <param name="getNonInstalled">If true, try to get non-installed games</param>
		/// <param name="expensiveIcons">(not used) If true, try to get expensive icons</param>
		/// <returns>True if at least one game was found. False if no games found or if the data source is unavailable</returns>
		public override bool GetGames(bool getNonInstalled, bool expensiveIcons)
        {
			CEventDispatcher.OnPlatformStarted(m_platformName);

			CSqlConn conn = new CSqlConn(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + GOG_DB);
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

			string launcherPath = "";
			using(RegistryKey key = Registry.LocalMachine.OpenSubKey(GOG_REG_CLIENT, RegistryKeyPermissionCheck.ReadSubTree)) // HKLM32
			{
				launcherPath = key.GetValue("client") + GOG_GALAXY_EXE;
				if(!File.Exists(launcherPath))
				{
					launcherPath = "";
				}
			}

			CQryGetInstalledGames qry = new CQryGetInstalledGames(conn);
			int  gameCount  = 0;
			bool isOk		= qry.Select() == SQLiteErrorCode.Ok;

			while(isOk)
			{
				string id       = qry.GameReleaseKey;
				string title    = qry.Title;
				string alias    = qry.Label;
				string iconPath = qry.ExecutablePath;
				string launch   = launcherPath + GOG_LAUNCH + id + GOG_PATH + "\"" + Path.GetDirectoryName(iconPath) + "\"";
				if(launch.Length > 8191)
				{
					launch = launcherPath + GOG_LAUNCH + id;
				}
				string platform = m_platformName;

				CEventDispatcher.OnGameFound(new RawGameData(id, title, launch, iconPath, "", alias, true, platform));
				gameCount++;
				isOk = qry.Fetch();
			}
			conn.Close();
			return gameCount > 0;
		}

		/// <summary>
		/// Overload.
		/// Use the sql connection to get list of non-installed games
		/// NOTE: The database does not have the details for all games (hence the table is called LimitedDetails)
		///		There is a chance that a game might be missing a title. The query will ignore all such cases, so the 
		///		actual list of non-installed games might be shorter than on the gog client
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
				string iconPath = ""; // TODO: download the images

				/*
				string images = qry.Images;
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
					string iconFile = string.Format("{0}.{1}", qry.Title, Path.GetExtension(iconUrl))
					using (var client = new WebClient())
					{
						client.DownloadFile(iconUrl, $"customImages\\{iconFile}");
					}
				}
				*/

				CEventDispatcher.OnGameFound(new RawGameData(qry.ReleaseKey, qry.Title, "", iconPath, "", "", false, m_platformName));
				gameCount++;
				isOk = qry.Fetch();
			}
			conn.Close();
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
