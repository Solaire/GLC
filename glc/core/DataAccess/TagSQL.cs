using System.Collections.Generic;

using core.Platform;
using core.Tag;
using SqlDB;

namespace core.DataAccess
{
    public static class CTagSQL
    {
        public const string FIELD_TAG_ID = "TagID";
        public const string FIELD_TAG_NAME = "Name";
        public const string FIELD_TAG_DESC = "Description";
        public const string FIELD_TAG_IS_ACTIVE = "IsActive";
        public const string FIELD_TAG_IS_INTERNAL = "IsInternal";

        public const string FIELD_TAG_PLATFORM_ID = "PlatformID";
        public const string FIELD_TAG_PLATFORM_ENABLED = "PlatformEnabled";

        /// <summary>
        /// Query for reading system attribute
        /// </summary>
        public class CQryTag : CSqlQry
        {
            public CQryTag()
                : base("Tag", "", "")
            {
                m_sqlRow[FIELD_TAG_ID] = new CSqlFieldInteger(FIELD_TAG_ID, CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cUpdWhere);
                m_sqlRow[FIELD_TAG_NAME] = new CSqlFieldString(FIELD_TAG_NAME, CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_TAG_DESC] = new CSqlFieldString(FIELD_TAG_DESC, CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_TAG_IS_ACTIVE] = new CSqlFieldBoolean(FIELD_TAG_IS_ACTIVE, CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_TAG_IS_INTERNAL] = new CSqlFieldBoolean(FIELD_TAG_IS_INTERNAL, CSqlField.QryFlag.cSelRead);
            }
            public int TagID
            {
                get { return m_sqlRow[FIELD_TAG_ID].Integer; }
                set { m_sqlRow[FIELD_TAG_ID].Integer = value; }
            }
            public string Name
            {
                get { return m_sqlRow[FIELD_TAG_NAME].String; }
                set { m_sqlRow[FIELD_TAG_NAME].String = value; }
            }
            public string Description
            {
                get { return m_sqlRow[FIELD_TAG_DESC].String; }
                set { m_sqlRow[FIELD_TAG_DESC].String = value; }
            }
            public bool IsActive
            {
                get { return m_sqlRow[FIELD_TAG_IS_ACTIVE].Bool; }
                set { m_sqlRow[FIELD_TAG_IS_ACTIVE].Bool = value; }
            }
            public bool IsInternal
            {
                get { return m_sqlRow[FIELD_TAG_IS_INTERNAL].Bool; }
                set { m_sqlRow[FIELD_TAG_IS_INTERNAL].Bool = value; }
            }
        }

        /// <summary>
        /// Query for reading system attribute
        /// </summary>
        public class CQryTagsForPlatform : CSqlQry
        {
            public CQryTagsForPlatform()
                : base("Tag t " +
                      "CROSS JOIN Platform ", "", "GROUP BY TagID")
            {
                m_sqlRow[FIELD_TAG_ID] = new CSqlFieldInteger(FIELD_TAG_ID, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TAG_NAME] = new CSqlFieldString("t.Name", CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TAG_DESC] = new CSqlFieldString("t.Description", CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TAG_IS_ACTIVE] = new CSqlFieldBoolean("t.IsActive", CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TAG_PLATFORM_ID] = new CSqlFieldInteger(FIELD_TAG_PLATFORM_ID, CSqlField.QryFlag.cSelWhere);
                m_sqlRow[FIELD_TAG_PLATFORM_ENABLED] = new CSqlFieldBoolean("CASE WHEN EXISTS (SELECT 1 FROM PlatformAttribute WHERE AttributeName = 'A_PLATFORM_TAG' AND AttributeValue = TagID AND PlatformFK = PlatformID) THEN 1 ELSE 0 END AS PlatformEnabled", CSqlField.QryFlag.cSelRead);
            }
            public int TagID
            {
                get { return m_sqlRow[FIELD_TAG_ID].Integer; }
                set { m_sqlRow[FIELD_TAG_ID].Integer = value; }
            }
            public string Name
            {
                get { return m_sqlRow[FIELD_TAG_NAME].String; }
                set { m_sqlRow[FIELD_TAG_NAME].String = value; }
            }
            public string Description
            {
                get { return m_sqlRow[FIELD_TAG_DESC].String; }
                set { m_sqlRow[FIELD_TAG_DESC].String = value; }
            }
            public bool IsActive
            {
                get { return m_sqlRow[FIELD_TAG_IS_ACTIVE].Bool; }
                set { m_sqlRow[FIELD_TAG_IS_ACTIVE].Bool = value; }
            }
            public int PlatformFK
            {
                get { return m_sqlRow[FIELD_TAG_PLATFORM_ID].Integer; }
                set { m_sqlRow[FIELD_TAG_PLATFORM_ID].Integer = value; }
            }
            public bool PlatformEnabled
            {
                get { return m_sqlRow[FIELD_TAG_PLATFORM_ENABLED].Bool; }
                set { m_sqlRow[FIELD_TAG_PLATFORM_ENABLED].Bool = value; }
            }
        }

        /// <summary>
        /// Query for reading system attribute
        /// </summary>
        public class CQryPlatformsForTag : CSqlQry
        {
            public CQryPlatformsForTag()
                : base("Platform p " +
                      "CROSS JOIN Tag ", "", "")
            {
                m_sqlRow[FIELD_TAG_ID] = new CSqlFieldInteger(FIELD_TAG_ID, CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cSelWhere);
                m_sqlRow[FIELD_TAG_NAME] = new CSqlFieldString("p.Name", CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TAG_DESC] = new CSqlFieldString("p.Description", CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TAG_PLATFORM_ID] = new CSqlFieldInteger(FIELD_TAG_PLATFORM_ID, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TAG_PLATFORM_ENABLED] = new CSqlFieldBoolean("CASE WHEN EXISTS (SELECT 1 FROM PlatformAttribute WHERE AttributeName = 'A_PLATFORM_TAG' AND PlatformFK = PlatformID AND AttributeValue = TagID) THEN 1 ELSE 0 END AS PlatformEnabled", CSqlField.QryFlag.cSelRead);
            }
            public int TagID
            {
                get { return m_sqlRow[FIELD_TAG_ID].Integer; }
                set { m_sqlRow[FIELD_TAG_ID].Integer = value; }
            }
            public string Name
            {
                get { return m_sqlRow[FIELD_TAG_NAME].String; }
                set { m_sqlRow[FIELD_TAG_NAME].String = value; }
            }
            public string Description
            {
                get { return m_sqlRow[FIELD_TAG_DESC].String; }
                set { m_sqlRow[FIELD_TAG_DESC].String = value; }
            }
            public int PlatformID
            {
                get { return m_sqlRow[FIELD_TAG_PLATFORM_ID].Integer; }
                set { m_sqlRow[FIELD_TAG_PLATFORM_ID].Integer = value; }
            }
            public bool PlatformEnabled
            {
                get { return m_sqlRow[FIELD_TAG_PLATFORM_ENABLED].Bool; }
                set { m_sqlRow[FIELD_TAG_PLATFORM_ENABLED].Bool = value; }
            }
        }

        // Query objects
        private static CQryTag m_qryTag = new CQryTag();
        private static CQryTagsForPlatform m_qryTagsForPlatform = new CQryTagsForPlatform();
        private static CQryPlatformsForTag m_qryPlatformsForTag = new CQryPlatformsForTag();

        public static void ToggleActive(int tagID, bool isActive)
        {
            m_qryTag.MakeFieldsNull();
            m_qryTag.TagID = tagID;
            m_qryTag.IsActive = isActive;
            m_qryTag.Update();
        }

        public static void UpdateNameDescription(TagObject tag)
        {
            m_qryTag.MakeFieldsNull();
            m_qryTag.TagID = tag.tagID;
            m_qryTag.Name = tag.name;
            m_qryTag.Description = tag.description;
            m_qryTag.Update();
        }

        public static List<TagObject> GetTags()
        {
            List<TagObject> tags = new List<TagObject>();
            m_qryTag.MakeFieldsNull();
            if (m_qryTag.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                do
                {
                    tags.Add(new TagObject(m_qryTag));
                } while (m_qryTag.Fetch());
            }
            return tags;
        }

        public static List<TagObject> GetTagsforPlatform(int platformID)
        {
            List<TagObject> tags = new List<TagObject>();
            m_qryTagsForPlatform.MakeFieldsNull();
            m_qryTagsForPlatform.PlatformFK = platformID;
            if (m_qryTagsForPlatform.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                do
                {
                    tags.Add(new TagObject(m_qryTagsForPlatform));
                } while (m_qryTagsForPlatform.Fetch());
            }
            return tags;
        }

        public static List<CBasicPlatform> GetPlatformsForTag(int tagID)
        {
            List<CBasicPlatform> platforms = new List<CBasicPlatform>();
            m_qryPlatformsForTag.MakeFieldsNull();
            m_qryPlatformsForTag.TagID = tagID;
            if (m_qryPlatformsForTag.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                do
                {
                    platforms.Add(new CBasicPlatform(m_qryPlatformsForTag.PlatformID, m_qryPlatformsForTag.Name, m_qryPlatformsForTag.Description, "", m_qryPlatformsForTag.PlatformEnabled));
                } while (m_qryPlatformsForTag.Fetch());
            }
            return platforms;
        }
    }
}
