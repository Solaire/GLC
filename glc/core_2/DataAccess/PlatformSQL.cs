using core_2.Platform;
using SqlDB;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace core_2.DataAccess
{
    internal class CPlatformSQL
    {
        #region Query definitions

        // Query parameter names
        private const string FIELD_PLATFORM_ID = "PlatformID";
        private const string FIELD_NAME = "Name";
        private const string FIELD_DESCRIPTION = "Description";
        private const string FIELD_PATH = "Path";
        private const string FIELD_IS_ACTIVE = "IsActive";
        private const string FIELD_GAME_COUNT = "GameCount";

        /// <summary>
        /// Query for inserting new platforms into the database
        /// </summary>
        public class CQryNewPlatform : CSqlQry
        {
            public CQryNewPlatform()
                : base(
                "Platform ", "", "")
            {
                m_sqlRow[FIELD_NAME] = new CSqlFieldString(FIELD_NAME, CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_DESCRIPTION] = new CSqlFieldString(FIELD_DESCRIPTION, CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_PATH] = new CSqlFieldString(FIELD_PATH, CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_IS_ACTIVE] = new CSqlFieldBoolean(FIELD_IS_ACTIVE, CSqlField.QryFlag.cInsWrite);
            }
            public string Name
            {
                get { return m_sqlRow[FIELD_NAME].String; }
                set { m_sqlRow[FIELD_NAME].String = value; }
            }
            public string Description
            {
                get { return m_sqlRow[FIELD_DESCRIPTION].String; }
                set { m_sqlRow[FIELD_DESCRIPTION].String = value; }
            }
            public string Path
            {
                get { return m_sqlRow[FIELD_PATH].String; }
                set { m_sqlRow[FIELD_PATH].String = value; }
            }
            public bool IsActive
            {
                get { return m_sqlRow[FIELD_IS_ACTIVE].Bool; }
                set { m_sqlRow[FIELD_IS_ACTIVE].Bool = value; }
            }
        }

        /// <summary>
        /// Query for updating and removing a platform from database
        /// </summary>
        public class CQryUpdatePlatform : CSqlQry
        {
            public CQryUpdatePlatform()
                : base(
                "Platform ", "", "")
            {
                m_sqlRow[FIELD_PLATFORM_ID] = new CSqlFieldInteger(FIELD_PLATFORM_ID, CSqlField.QryFlag.cUpdWhere | CSqlField.QryFlag.cDelWhere);
                m_sqlRow[FIELD_NAME] = new CSqlFieldString(FIELD_NAME, CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_DESCRIPTION] = new CSqlFieldString(FIELD_DESCRIPTION, CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_PATH] = new CSqlFieldString(FIELD_PATH, CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_IS_ACTIVE] = new CSqlFieldBoolean(FIELD_IS_ACTIVE, CSqlField.QryFlag.cUpdWrite);
            }
            public int PlatformID
            {
                get { return m_sqlRow[FIELD_PLATFORM_ID].Integer; }
                set { m_sqlRow[FIELD_PLATFORM_ID].Integer = value; }
            }
            public string Name
            {
                get { return m_sqlRow[FIELD_NAME].String; }
                set { m_sqlRow[FIELD_NAME].String = value; }
            }
            public string Description
            {
                get { return m_sqlRow[FIELD_DESCRIPTION].String; }
                set { m_sqlRow[FIELD_DESCRIPTION].String = value; }
            }
            public string Path
            {
                get { return m_sqlRow[FIELD_PATH].String; }
                set { m_sqlRow[FIELD_PATH].String = value; }
            }
            public bool IsActive
            {
                get { return m_sqlRow[FIELD_IS_ACTIVE].Bool; }
                set { m_sqlRow[FIELD_IS_ACTIVE].Bool = value; }
            }
        }

        /// <summary>
        /// Query for retrieving platform data
        /// </summary>
        public class CQryReadPlatform : CSqlQry
        {
            public CQryReadPlatform()
                : base(
                "Platform " +
                "LEFT JOIN Game G on PlatformID = G.PlatformFK",
                "", "GROUP BY PlatformID")
            {
                m_sqlRow[FIELD_PLATFORM_ID] = new CSqlFieldInteger(FIELD_PLATFORM_ID, CSqlField.QryFlag.cSelWhere | CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_NAME] = new CSqlFieldString(FIELD_NAME, CSqlField.QryFlag.cSelWhere | CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_DESCRIPTION] = new CSqlFieldString(FIELD_DESCRIPTION, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_PATH] = new CSqlFieldString(FIELD_PATH, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_IS_ACTIVE] = new CSqlFieldBoolean(FIELD_IS_ACTIVE, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_GAME_COUNT] = new CSqlFieldInteger("COUNT(G.GameID) as GameCount", CSqlField.QryFlag.cSelRead);
            }
            public int PlatformID
            {
                get { return m_sqlRow[FIELD_PLATFORM_ID].Integer; }
                set { m_sqlRow[FIELD_PLATFORM_ID].Integer = value; }
            }
            public string Name
            {
                get { return m_sqlRow[FIELD_NAME].String; }
                set { m_sqlRow[FIELD_NAME].String = value; }
            }
            public string Description
            {
                get { return m_sqlRow[FIELD_DESCRIPTION].String; }
                set { m_sqlRow[FIELD_DESCRIPTION].String = value; }
            }
            public string Path
            {
                get { return m_sqlRow[FIELD_PATH].String; }
                set { m_sqlRow[FIELD_PATH].String = value; }
            }
            public bool IsActive
            {
                get { return m_sqlRow[FIELD_IS_ACTIVE].Bool; }
                set { m_sqlRow[FIELD_IS_ACTIVE].Bool = value; }
            }
            public int GameCount
            {
                get { return m_sqlRow[FIELD_GAME_COUNT].Integer; }
                set { m_sqlRow[FIELD_GAME_COUNT].Integer = value; }
            }
        }

        #endregion Query definitions

        private static CQryNewPlatform m_qryNewPlatform = new CQryNewPlatform();
        private static CQryUpdatePlatform m_qryUpdatePlatform = new CQryUpdatePlatform();
        private static CQryReadPlatform m_qryReadPlatform = new CQryReadPlatform();
        private static CDbAttribute m_platformAttribute = new CDbAttribute("Platform");

        /// <summary>
        /// Load the platform by name lookup
        /// </summary>
        /// <typeparam name="T">Type T which must derive from the abstract CPlatform class</typeparam>
        /// <param name="factory">Implementation of the Platform factory interface which will create the instance of T</param>
        /// <param name="platform">Out parameter of type T which is the loaded CPlatform instance</param>
        /// <returns>True if load is successful</returns>
        public static bool LoadPlatform<T>(CPlatformFactory<T> factory, out T? platform) where T : CPlatform
        {
            m_qryReadPlatform.MakeFieldsNull();
            m_qryReadPlatform.Name = factory.GetPlatformName();
            m_qryReadPlatform.Select();
            if(m_qryReadPlatform.PlatformID <= 0)
            {
                platform = null;
                return false;
            }
            platform = factory.CreateFromDatabase(m_qryReadPlatform.PlatformID, m_qryReadPlatform.Name, "", "", m_qryReadPlatform.IsActive);
            return true;
        }

        public static List<CPlatform> ListPlatforms()
        {
            List<CPlatform> nodes = new List<CPlatform>();
            m_qryReadPlatform.MakeFieldsNull();
            if(m_qryReadPlatform.Select() == SQLiteErrorCode.Ok)
            {
                do
                {
                    nodes.Add(new CPlatform(m_qryReadPlatform));
                } while(m_qryReadPlatform.Fetch());
            }
            return nodes;
        }

        /// <summary>
        /// Insert platform into the database
        /// </summary>
        /// <param name="platform">Instance of CPlatform</param>
        /// <returns>True if insert was successful</returns>
        public static bool InsertPlatform(CPlatform platform)
        {
            m_qryNewPlatform.MakeFieldsNull();
            m_qryNewPlatform.Name = platform.Name;
            m_qryNewPlatform.Description = platform.Description;
            m_qryNewPlatform.Path = platform.Path;
            m_qryNewPlatform.IsActive = platform.IsEnabled;
            if(m_qryNewPlatform.Insert() != SQLiteErrorCode.Ok)
            {
                return false;
            }
            platform.PrimaryKey = (int)CSqlDB.Instance.Conn.SqlConn.LastInsertRowId;
            return true;
        }

        /// <summary>
        /// Delete platform from the database
        /// </summary>
        /// <param name="platformID">The platformID</param>
        /// <returns>True if delete success</returns>
        public static bool DeletePlatform(int platformID)
        {
            if(platformID <= 0)
            {
                return true;
            }
            m_qryUpdatePlatform.MakeFieldsNull();
            m_qryUpdatePlatform.PlatformID = platformID;
            return m_qryUpdatePlatform.Delete() == SQLiteErrorCode.Ok;
        }

        /// <summary>
        /// Toggle the Active flag for the platform
        /// </summary>
        /// <param name="platformID">The platformID</param>
        /// <param name="isActive">New Active value</param>
        /// <returns>True on update success</returns>
        public static bool ToggleActive(int platformID, bool isActive)
        {
            if(platformID <= 0)
            {
                return true;
            }
            m_qryUpdatePlatform.MakeFieldsNull();
            m_qryUpdatePlatform.PlatformID = platformID;
            m_qryUpdatePlatform.IsActive = isActive;
            return m_qryUpdatePlatform.Update() == SQLiteErrorCode.Ok;
        }

        public static void SetTags(int platformID, List<int> enabledTags)
        {
            m_platformAttribute.MasterID = platformID;
            m_platformAttribute.DeleteAttribute("A_PLATFORM_TAG");

            for(int i = 0; i < enabledTags.Count; i++)
            {
                m_platformAttribute.SetStringValue("A_PLATFORM_TAG", enabledTags[i].ToString(), i);
            }
        }

        public static void SetTag(int platformID, int tagID, bool isEnabled)
        {
            m_platformAttribute.MasterID = platformID;
            List<string> existing = m_platformAttribute.GetStringValues("A_PLATFORM_TAG").ToList();
            int index = existing.FindIndex(tag => tag == tagID.ToString());

            if(index == -1 && isEnabled)
            {
                m_platformAttribute.SetStringValue("A_PLATFORM_TAG", tagID.ToString(), existing.Count);
            }
            else if(index != -1 && !isEnabled)
            {
                // Delete and reinsert all tags to preserve the indexing (TODO: optimise)
                existing.RemoveAt(index);
                m_platformAttribute.DeleteAttribute("A_PLATFORM_TAG");
                for(int i = 0; i < existing.Count; i++)
                {
                    m_platformAttribute.SetStringValue("A_PLATFORM_TAG", existing[i].ToString(), i);
                }
            }
        }
    }
}
