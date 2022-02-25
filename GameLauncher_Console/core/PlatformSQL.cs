using SqlDB;
using System.Collections.Generic;
using System.Data.SQLite;

namespace core
{
	/// <summary>
	/// Static class for platform read/write operations via the database
	/// </summary>
    public static class CPlatformSQL
    {
		#region Query definitions

		/// <summary>
		/// Retrieve the platform information from the database
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
				m_sqlRow["PlatformID"] = new CSqlFieldInteger("PlatformID", CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cSelWhere);
				m_sqlRow["Name"] = new CSqlFieldString("Name", CSqlField.QryFlag.cSelRead);
				m_sqlRow["GameCount"] = new CSqlFieldString("COUNT(G.GameID) as GameCount", CSqlField.QryFlag.cSelRead);
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
				m_sqlRow["PlatformID"] = new CSqlFieldInteger("PlatformID", CSqlField.QryFlag.cUpdWhere | CSqlField.QryFlag.cDelWhere);
				m_sqlRow["Name"] = new CSqlFieldString("Name", CSqlField.QryFlag.cUpdWrite | CSqlField.QryFlag.cInsWrite);
				m_sqlRow["Description"] = new CSqlFieldString("Description", CSqlField.QryFlag.cUpdWrite | CSqlField.QryFlag.cInsWrite);
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

		/// <summary>
		/// Query for counting the number of supported platforms
		/// </summary>
		public class CQryPlatformCount : CSqlQry
		{
			public CQryPlatformCount()
				: base("Platform", "", "")
			{
				m_sqlRow["PlatformCount"] = new CSqlFieldInteger("COUNT(*) as PlatformCount", CSqlField.QryFlag.cSelRead);
			}

			public int PlatformCount
			{
				get { return m_sqlRow["PlatformCount"].Integer; }
				set { m_sqlRow["PlatformCount"].Integer = value; }
			}
		}

		/// <summary>
		/// Query for retrieveing the platformID based on string
		/// </summary>
		public class CQryGetPlatform : CSqlQry
		{
			public CQryGetPlatform()
				: base("Platform", "", "")
			{
				m_sqlRow["PlatformID"] = new CSqlFieldInteger("PlatformID", CSqlField.QryFlag.cSelRead);
				m_sqlRow["Name"] = new CSqlFieldString("Name", CSqlField.QryFlag.cSelWhere);
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
		}

		#endregion // Query definitions

		private static CQryReadPlatforms m_qryRead          = new CQryReadPlatforms();
		private static CQryWritePlatforms m_qryWrite        = new CQryWritePlatforms();
		private static CQryPlatformCount m_qryPlatformCount = new CQryPlatformCount();
		private static CQryGetPlatform m_qryGetPlatform     = new CQryGetPlatform();

		/// <summary>
		/// Load the platform by name lookup
		/// </summary>
		/// <typeparam name="T">Type T which must derive from the abstract CPlatform class</typeparam>
		/// <param name="name">The platform name</param>
		/// <param name="factory">Implementation of the Platform factory interface which will create the instance of T</param>
		/// <param name="platform">Out parameter of type T which is the loaded CPlatform instance</param>
		/// <returns>True if load is successful</returns>
		public static bool LoadPlatform<T>(string name, IPlatformFactory<T> factory, out T? platform) where T : CPlatform
		{
			if(name.Length == 0)
			{
				platform = null;
				return false;
			}
			m_qryGetPlatform.MakeFieldsNull();
			m_qryGetPlatform.Name = name;
			m_qryGetPlatform.Select();
			if(m_qryGetPlatform.PlatformID <= 0)
			{
				platform = null;
				return false;
			}
			platform = factory.Create(m_qryGetPlatform.PlatformID, m_qryGetPlatform.Name, "", "");
			return true;
		}

		/// <summary>
		/// Insert platform into the database
		/// </summary>
		/// <param name="platform">Instance of CPlatform</param>
		/// <returns>True if insert was successful</returns>
		public static bool InsertPlatform(CPlatform platform)
        {
			m_qryWrite.MakeFieldsNull();
			m_qryWrite.Name			= platform.Name;
			m_qryWrite.Description	= platform.Description;
			return m_qryWrite.Insert() == SQLiteErrorCode.Ok;
		}

		/// <summary>
		/// Delete platform from the database
		/// </summary>
		/// <param name="platformID">The platformID</param>
		/// <returns>True if delete success</returns>
		public static bool DeletePlatform(int platformID)
        {
			m_qryWrite.MakeFieldsNull();
			m_qryWrite.PlatformID = platformID;
			return m_qryWrite.Delete() == SQLiteErrorCode.Ok;
		}

		/*
		/// <summary>
		/// Load platforms from database
		/// </summary>
		/// <param name="excludeEmptyPlatforms">If true, platforms with 0 games will be ignored</param>
		/// <returns>List of platform objects</returns>
		public static List<PlatformObject> LoadPlatforms(bool excludeEmptyPlatforms)
        {
			List<PlatformObject> platforms = new List<PlatformObject>();
			m_qryRead.MakeFieldsNull();
			if(m_qryRead.Select() == SQLiteErrorCode.Ok)
			{
				do
				{
					if(m_qryRead.GameCount == 0 && excludeEmptyPlatforms)
					{
						continue;
					}
					platforms.Add(new PlatformObject(m_qryRead));
				} while(m_qryRead.Fetch());
			}
			return platforms;
		}

		/// <summary>
		/// Load platform by name lookup
		/// </summary>
		/// <param name="name">The platform name</param>
		/// <param name="platform">nullable out parameter which will store retrieved platform</param>
		/// <returns>True if lookup success, otherwise false</returns>
		public static bool LoadPlatform(string name, out PlatformObject ? platform)
		{
			if(name.Length == 0)
			{
				platform = null;
				return false;
			}
			m_qryGetPlatform.MakeFieldsNull();
			m_qryGetPlatform.Name = name;
			m_qryGetPlatform.Select();
			if(m_qryGetPlatform.PlatformID <= 0)
            {
				platform = null;
				return false;
			}
			platform = new PlatformObject(m_qryGetPlatform.PlatformID, m_qryGetPlatform.Name, true);
			return true;
		}

		/// <summary>
		/// Load platform by ID lookup
		/// </summary>
		/// <param name="id">The platform ID</param>
		/// <param name="platform">nullable out parameter which will store retrieved platform</param>
		/// <returns>True if lookup success, otherwise false</returns>
		public static bool LoadPlatform(int id, out PlatformObject ? platform)
		{
			m_qryRead.MakeFieldsNull();
			m_qryRead.PlatformID = id;
			m_qryRead.Select();
			if(m_qryRead.PlatformID <= 0)
			{
				platform = null;
				return false;
			}
			platform = new PlatformObject(m_qryRead, true);
			return true;
		}

		/// <summary>
		/// Get number of platforms in the database
		/// </summary>
		/// <returns>Number of platforms in the database</returns>
		public static int GetPlatformCount()
		{
			m_qryPlatformCount.MakeFieldsNull();
			m_qryPlatformCount.Select();
			return m_qryPlatformCount.PlatformCount;
		}

		/// <summary>
		/// Insert specified platform into the database
		/// </summary>
		/// <param name="platform">The PlatformObject to insert</param>
		/// <returns>True on insert success, otherwise false</returns>
		public static bool InsertPlatform(PlatformObject platform)
		{
			m_qryWrite.MakeFieldsNull();
			m_qryWrite.Name = platform.Name;
			m_qryWrite.Description = "";
			return m_qryWrite.Insert() == SQLiteErrorCode.Ok;
		}

		/// <summary>
		/// Insert specified platform into the database
		/// </summary>
		/// <param name="title">Platform title</param>
		/// <returns>True on insert success, otherwise false</returns>
		public static bool InsertPlatform(string title)
		{
			m_qryWrite.MakeFieldsNull();
			m_qryWrite.Name = title;
			m_qryWrite.Description = "";
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
			m_qryWrite.PlatformID = platform.ID;
			m_qryWrite.Name = platform.Name;
			m_qryWrite.Description = "";
			return m_qryWrite.Update() == SQLiteErrorCode.Ok;
		}

		/// <summary>
		/// Remove platform with selected ID from the database
		/// </summary>
		/// <param name="platformID">The platformID</param>
		/// <returns>True on delete success, otherwise false</returns>
		public static bool RemovePlatform(int platformID)
		{
			m_qryWrite.MakeFieldsNull();
			m_qryWrite.PlatformID = platformID;
			return m_qryWrite.Delete() == SQLiteErrorCode.Ok;
		}
		*/
	}
}
