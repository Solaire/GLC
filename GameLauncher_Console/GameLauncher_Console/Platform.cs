using SqlDB;
using System.Collections.Generic;
using System.ComponentModel;
using static SqlDB.CSqlField;
using System.Data.SQLite;

namespace GameLauncher_Console
{
	/// <summary>
	/// Class to handle platform object and platform database table
	/// </summary>
    public static class CPlatform
    {
		/// <summary>
		/// Enumerator containing currently supported game platforms
		/// </summary>
		public enum GamePlatform
        {
			[Description("Unknown")]
			Unknown = -1,
			[Description("Favourites")]
			Favourites = 0,
			[Description("Custom games")]
			Custom = 1,
			[Description("All games")]
			All = 2,
			[Description("Steam")]
			Steam = 3,
			[Description("GOG Galaxy")]
			GOG = 4,
			[Description("Ubisoft Connect")]
			Uplay = 5,
			[Description("Origin")]
			Origin = 6,
			[Description("Epic")]
			Epic = 7,
			[Description("Betheda.net")]
			Bethesda = 8,
			[Description("Battle.net")]
			Battlenet = 9,
			[Description("Rockstar")]
			Rockstar = 10,
			[Description("Hidden games")]
			Hidden = 11,
			[Description("Search results")]
			Search = 12,
			[Description("Amazon")]
			Amazon = 13,
			[Description("Big Fish")]
			BigFish = 14,
			[Description("Arc")]
			Arc = 15,
			[Description("itch")]
			Itch = 16,
			[Description("Paradox")]
			Paradox = 17,
			[Description("Plarium Play")]
			Plarium = 18,
			[Description("Twitch")]
			Twitch = 19,
			[Description("Wargaming.net")]
			Wargaming = 20
		}

		#region Query definitions

		/// <summary>
		/// Retrieve the platform information fromthe database
		/// Also returns the game count for each platform
		/// </summary>
		public class CQryReadPlatforms : CSqlQry
        {
			public CQryReadPlatforms()
				: base(
				"Platform " + 
				"LEFT JOIN Game G on PlatformID = G.PlatformFK", 
				"", " GROUP BY PlatformID")
			{
				m_sqlRow["PlatformID"]	= new CSqlFieldInteger("PlatformID", QryFlag.cSelWhere);
				m_sqlRow["Name"]		= new CSqlFieldString("Name",		 QryFlag.cSelRead);
				m_sqlRow["GameCount"]	= new CSqlFieldString("COUNT(G.GameID) as GameCount", QryFlag.cSelRead);
			}
			public int PlatformID
			{
				get { return m_sqlRow["PlatformID"].Integer; }
				set { m_sqlRow["PlatformID"].Integer = value; }
			}
			public string Name
			{
				get { return m_sqlRow["Name"].String; }
				set { m_sqlRow["Name"].String = value; }
			}
			public int GameCount
			{
				get { return m_sqlRow["GameCount"].Integer; }
				set { m_sqlRow["GameCount"].Integer = value; }
			}
		}

		/// <summary>
		/// Query for writing to the platform table
		/// </summary>
		public class CQryWritePlatforms : CSqlQry
        {
			public CQryWritePlatforms()
				: base("Platform", "", "")
			{
				m_sqlRow["PlatformID"]	= new CSqlFieldInteger("PlatformID" , QryFlag.cUpdWhere | QryFlag.cDelWhere);
				m_sqlRow["Name"]		= new CSqlFieldString("Name"		, QryFlag.cUpdWrite | QryFlag.cInsWrite);
				m_sqlRow["Description"]	= new CSqlFieldString("Description" , QryFlag.cUpdWrite | QryFlag.cInsWrite);
			}
			public int PlatformID
			{
				get { return m_sqlRow["PlatformID"].Integer; }
				set { m_sqlRow["PlatformID"].Integer = value; }
			}
			public string Name
			{
				get { return m_sqlRow["Name"].String; }
				set { m_sqlRow["Name"].String = value; }
			}
			public string Description
			{
				get { return m_sqlRow["Description"].String; }
				set { m_sqlRow["Description"].String = value; }
			}
		}

		#endregion // Query definitions

		private static CQryReadPlatforms m_qryRead = new CQryReadPlatforms();
		private static CQryWritePlatforms m_qryWrite = new CQryWritePlatforms();

		/// <summary>
		/// Container for a single platform
		/// </summary>
		public struct PlatformObject
        {
			public PlatformObject(int platformID, string name, int gameCount, string description)
            {
				PlatformID	= platformID;
				Name		= name;
				GameCount	= gameCount;
				Description = description;
			}

			/// <summary>
			/// Constructor overload.
			/// Populate using query
			/// </summary>
			/// <param name="qryRead"></param>
			public PlatformObject(CQryReadPlatforms qryRead)
			{
				PlatformID	= qryRead.PlatformID;
				Name		= qryRead.Name;
				GameCount	= qryRead.GameCount;
				Description = "";
			}

			// Properties
			public int PlatformID { get; }
			public string Name { get; }
			public int GameCount { get; private set; }
			public string Description { get; }
        }

		/// <summary>
		/// Get list of platforms from the database
		/// </summary>
		/// <returns>List of PlatformObjects</returns>
		public static List<PlatformObject> GetPlatforms()
        {
			List<PlatformObject> platforms = new List<PlatformObject>();
			m_qryRead.MakeFieldsNull();
			if(m_qryRead.Select() == SQLiteErrorCode.Ok)
            {
				do
				{
					platforms.Add(new PlatformObject(m_qryRead));
				} while(m_qryRead.Fetch());
            }
			return platforms;
        }

		/// <summary>
		/// Insert specified platform into the database
		/// </summary>
		/// <param name="platform">The PlatformObject to insert</param>
		/// <returns>True on insert success, otherwise false</returns>
		public static bool InsertPlatform(PlatformObject platform)
        {
			m_qryWrite.MakeFieldsNull();
			m_qryWrite.Name			= platform.Name;
			m_qryWrite.Description	= platform.Description;
			return m_qryWrite.Insert() == SQLiteErrorCode.Ok;
        }

		/// <summary>
		/// Update specified platform
		/// </summary>
		/// <param name="platform">The PlatformObject to write</param>
		/// <returns>True on update success, otherwise false</returns>
		public static bool UpdatePlatform(PlatformObject platform)
        {
			m_qryWrite.MakeFieldsNull();
			m_qryWrite.PlatformID	= platform.PlatformID;
			m_qryWrite.Name			= platform.Name;
			m_qryWrite.Description	= platform.Description;
			return m_qryWrite.Update() == SQLiteErrorCode.Ok;
		}

		/// <summary>
		/// Remove platform with selected PlatformID from the database
		/// </summary>
		/// <param name="platformID">The platformID to delete</param>
		/// <returns>True on delete success, otherwise false</returns>
		public static bool RemovePlatform(int platformID)
        {
			m_qryWrite.MakeFieldsNull();
			m_qryWrite.PlatformID = platformID;
			return m_qryWrite.Delete() == SQLiteErrorCode.Ok;
		}
    }
}