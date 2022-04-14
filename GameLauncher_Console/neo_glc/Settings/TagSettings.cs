using core;
using SqlDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static core.CSystemAttributeSQL;

namespace glc.Settings
{
    public struct TagObject
    {
        public int tagID;
        public string name;
        public string description;
        public bool isActive;
        public bool isInternal;

        public TagObject(int tagID, string name, string description, bool isActive, bool isInternal)
        {
            this.tagID = tagID;
            this.name = name;
            this.description = description;
            this.isActive = isActive;
            this.isInternal = isInternal;
        }

        public TagObject(CTagSQL.CQryTag qry)
        {
            this.tagID       = qry.TagID;
            this.name        = qry.Name;
            this.description = qry.Description;
            this.isActive    = qry.IsActive;
            this.isInternal = qry.IsInternal;
        }

        public TagObject(CTagSQL.CQryTagsForPlatform qry)
        {
            this.tagID          = qry.TagID;
            this.name           = qry.Name;
            this.description    = qry.Description;
            this.isActive       = qry.PlatformEnabled;
            this.isInternal     = false; // Unused
        }
    }

    public static class CTagSQL
    {
        public const string FIELD_TAG_ID        = "TagID";
        public const string FIELD_TAG_NAME      = "Name";
        public const string FIELD_TAG_DESC      = "Description";
        public const string FIELD_TAG_IS_ACTIVE = "IsActive";
        public const string FIELD_TAG_IS_INTERNAL = "IsInternal";

        public const string FIELD_TAG_PLATFORM_ID       = "PlatformID";
        public const string FIELD_TAG_PLATFORM_ENABLED  = "PlatformEnabled";

        /// <summary>
        /// Query for reading system attribute
        /// </summary>
        public class CQryTag : CSqlQry
        {
            public CQryTag()
                : base("Tag", "", "")
            {
                m_sqlRow[FIELD_TAG_ID]          = new CSqlFieldInteger(FIELD_TAG_ID,        CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cUpdWhere);
                m_sqlRow[FIELD_TAG_NAME]        = new CSqlFieldString(FIELD_TAG_NAME,       CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_TAG_DESC]        = new CSqlFieldString(FIELD_TAG_DESC,       CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_TAG_IS_ACTIVE]   = new CSqlFieldBoolean(FIELD_TAG_IS_ACTIVE, CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cUpdWrite);
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
                : base("Tag " +
                      "CROSS JOIN Platform", "", "GROUP BY TagID")
            {
                m_sqlRow[FIELD_TAG_ID]               = new CSqlFieldInteger(FIELD_TAG_ID,          CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TAG_NAME]             = new CSqlFieldString(FIELD_TAG_NAME,         CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TAG_DESC]             = new CSqlFieldString(FIELD_TAG_DESC,         CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TAG_IS_ACTIVE]        = new CSqlFieldBoolean(FIELD_TAG_IS_ACTIVE,   CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TAG_PLATFORM_ID]      = new CSqlFieldInteger(FIELD_TAG_PLATFORM_ID, CSqlField.QryFlag.cSelWhere);
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
                m_sqlRow[FIELD_TAG_ID]               = new CSqlFieldInteger(FIELD_TAG_ID,           CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cSelWhere);
                m_sqlRow[FIELD_TAG_NAME]             = new CSqlFieldString("p.Name",                CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TAG_DESC]             = new CSqlFieldString("p.Description",         CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TAG_PLATFORM_ID]      = new CSqlFieldInteger(FIELD_TAG_PLATFORM_ID,  CSqlField.QryFlag.cSelRead);
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
            if(m_qryTag.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                do
                {
                    tags.Add(new TagObject(m_qryTag));
                } while(m_qryTag.Fetch());
            }
            return tags;
        }

        public static List<TagObject> GetTagsforPlatform(int platformID)
        {
            List<TagObject> tags = new List<TagObject>();
            m_qryTagsForPlatform.MakeFieldsNull();
            m_qryTagsForPlatform.PlatformFK = platformID;
            if(m_qryTagsForPlatform.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                do
                {
                    tags.Add(new TagObject(m_qryTagsForPlatform));
                } while(m_qryTagsForPlatform.Fetch());
            }
            return tags;
        }

        public static List<CBasicPlatform> GetPlatformsForTag(int tagID)
        {
            List<CBasicPlatform> platforms = new List<CBasicPlatform>();
            m_qryPlatformsForTag.MakeFieldsNull();
            m_qryPlatformsForTag.TagID = tagID;
            if(m_qryPlatformsForTag.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                do
                {
                    platforms.Add(new CBasicPlatform(m_qryPlatformsForTag.PlatformID, m_qryPlatformsForTag.Name, m_qryPlatformsForTag.Description, "", m_qryPlatformsForTag.PlatformEnabled));
                } while(m_qryPlatformsForTag.Fetch());
            }
            return platforms;
        }
    }

    public class CTagContainer : CGenericSettingContainer<TagObject>
    {
        public CTagContainer()
        {
            DataList = CTagSQL.GetTags();
            DataSource = new CTagDataSource(DataList);
        }

        public override void EditNode(int selectionIndex)
        {
            TagObject selected = DataList[selectionIndex];
            CEditTagDlg dlg = new CEditTagDlg(selected);

            SystemAttributeNode temp = new SystemAttributeNode("temp", "temp", "temp", AttributeType.cTypeBool);

            if(dlg.Run(ref temp))
            {
                selected.isActive = temp.IsTrue();
                CTagSQL.ToggleActive(selected.tagID, selected.isActive);

                if(!selected.isInternal)
                {
                    selected.name = dlg.TagName;
                    selected.description = dlg.TagDescription;
                    CTagSQL.UpdateNameDescription(selected);
                }

                foreach(CBasicPlatform platform in dlg.Platforms)
                {
                    CPlatformSQL.SetTag(platform.ID, selected.tagID, platform.IsActive);
                }

                DataList[selectionIndex] = selected;
                DataSource.ToList()[selectionIndex] = selected;
            }
        }

        public override void SelectNode(int selectionIndex)
        {
            EditNode(selectionIndex);
        }
    }

    internal class CTagDataSource : CGenericDataSource<TagObject>
    {
        private readonly long m_maxNameLength;
        private readonly long m_maxDescLength;

        public CTagDataSource(List<TagObject> itemList)
            : base(itemList)
        {
            for(int i = 0; i < itemList.Count; i++)
            {
                if(ItemList[i].name.Length > m_maxNameLength)
                {
                    m_maxNameLength = ItemList[i].name.Length;
                }
                if(ItemList[i].description.Length > m_maxDescLength)
                {
                    m_maxDescLength = ItemList[i].description.Length;
                }
            }
        }

        protected override string ConstructString(int itemIndex)
        {
            String s1 = String.Format(String.Format("{{0,{0}}}", -m_maxNameLength), ItemList[itemIndex].name);
            String s2 = String.Format(String.Format("{{0,{0}}}", -m_maxDescLength), ItemList[itemIndex].description);
            string enabled = (ItemList[itemIndex].isActive) ? "Enabled" : "Disabled";

            return $"{s1}  {s2}  {enabled}";
        }

        protected override string GetString(int itemIndex)
        {
            return ItemList[itemIndex].name;
        }
    }
}
