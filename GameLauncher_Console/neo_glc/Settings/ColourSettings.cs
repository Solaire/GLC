using SqlDB;
using System;
using System.Collections;
using System.Collections.Generic;

using Terminal.Gui;

namespace glc.Settings
{
    public class CColourContainer : CGenericSettingContainer<ColourNode>
    {
        public CColourContainer()
        {
            DataList = CColourSchemeSQL.GetColours();
            for(int i = 0; i < DataList.Count; i++)
            {
                if(DataList[i].isSystem)
                {
                    ColourNode temp = DataList[i];
                    temp.scheme = Colors.ColorSchemes[DataList[i].name];
                    DataList[i] = temp;
                }
            }

            DataSource = new CColourDataSource(DataList);
        }

        public override void EditNode(int selectionIndex)
        {
            SelectNode(selectionIndex);

            // TODO: implement after adding custom colours
            /*
            ColorScheme selected = DataList[selectionIndex];
            CEditColourDlg dlg   = new CEditColourDlg(selected.ToString(), selected);

            SystemAttributeNode temp = new SystemAttributeNode("temp", "temp", "temp", AttributeType.cTypeBool);

            if(dlg.Run(ref temp))
            {
                DataList[selectionIndex] = selected;
                DataSource.ToList()[selectionIndex] = selected;
            }
            */
        }

        public override void SelectNode(int selectionIndex)
        {
            Application.Top.ColorScheme = DataList[selectionIndex].scheme;
            CColourSchemeSQL.SetActive(DataList[selectionIndex].colourSchemeID);
        }
    }

    internal class CColourDataSource : CGenericDataSource<ColourNode>
    {
        private readonly long m_maxTitleLength;
        private BitArray marks;

        public CColourDataSource(List<ColourNode> itemList)
            : base(itemList)
        {
            marks = new BitArray(Count);

            for(int i = 0; i < itemList.Count; i++)
            {
                if(ItemList[i].name.Length > m_maxTitleLength)
                {
                    m_maxTitleLength = ItemList[i].name.Length;
                }
                SetMark(i, ItemList[i].isActive);
            }
        }

        protected override string ConstructString(int itemIndex)
        {
            string title        = ItemList[itemIndex].name;
            string description  = ItemList[itemIndex].description;
            String s1 = String.Format(String.Format("{{0,{0}}}", -m_maxTitleLength), title);
            return $"{s1}  {description}";
        }

        protected override string GetString(int itemIndex)
        {
            return ItemList[itemIndex].ToString();
        }

        public override bool IsMarked(int item)
        {
            if(item >= 0 && item < Count)
            {
                return marks[item];
            }
            return false;
        }

        public override void SetMark(int item, bool value)
        {
            if(item >= 0 && item < Count)
            {
                marks[item] = value;
            }
        }
    }

    public static class CColourSchemeSQL
    {
        public const string FIELD_COLOUR_SCHEME_ID          = "ColourSchemeID";
        public const string FIELD_COLOUR_SCHEME_NAME        = "Name";
        public const string FIELD_COLOUR_SCHEME_DESC        = "Description";
        public const string FIELD_COLOUR_SCHEME_IS_ACTIVE   = "IsActive";
        public const string FIELD_COLOUR_SCHEME_IS_SYSTEM   = "IsSystem";

        public const string FIELD_COLOUR_CONTENT_FK          = "ColourSchemeFK";
        public const string FIELD_COLOUR_CONTENT_NORMAL      = "Normal";
        public const string FIELD_COLOUR_CONTENT_FOCUS       = "Focus";
        public const string FIELD_COLOUR_CONTENT_HOT_NORMAL  = "HotNormal";
        public const string FIELD_COLOUR_CONTENT_HOT_FOCUS   = "HotFocus";
        public const string FIELD_COLOUR_CONTENT_DISABLED    = "Disabled";

        /// <summary>
        /// Query for reading system attribute
        /// </summary>
        public class CQryColourScheme : CSqlQry
        {
            public CQryColourScheme()
                : base("ColourScheme", "", "")
            {
                m_sqlRow[FIELD_COLOUR_SCHEME_ID]        = new CSqlFieldInteger(FIELD_COLOUR_SCHEME_ID,          CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cUpdWhere);
                m_sqlRow[FIELD_COLOUR_SCHEME_NAME]      = new CSqlFieldString(FIELD_COLOUR_SCHEME_NAME,         CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_COLOUR_SCHEME_DESC]      = new CSqlFieldString(FIELD_COLOUR_SCHEME_DESC,         CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_COLOUR_SCHEME_IS_ACTIVE] = new CSqlFieldBoolean(FIELD_COLOUR_SCHEME_IS_ACTIVE,   CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_COLOUR_SCHEME_IS_SYSTEM] = new CSqlFieldBoolean(FIELD_COLOUR_SCHEME_IS_SYSTEM,   CSqlField.QryFlag.cSelRead);
            }
            public int ColourSchemeID
            {
                get { return m_sqlRow[FIELD_COLOUR_SCHEME_ID].Integer; }
                set { m_sqlRow[FIELD_COLOUR_SCHEME_ID].Integer = value; }
            }
            public string Name
            {
                get { return m_sqlRow[FIELD_COLOUR_SCHEME_NAME].String; }
                set { m_sqlRow[FIELD_COLOUR_SCHEME_NAME].String = value; }
            }
            public string Description
            {
                get { return m_sqlRow[FIELD_COLOUR_SCHEME_DESC].String; }
                set { m_sqlRow[FIELD_COLOUR_SCHEME_DESC].String = value; }
            }
            public bool IsActive
            {
                get { return m_sqlRow[FIELD_COLOUR_SCHEME_IS_ACTIVE].Bool; }
                set { m_sqlRow[FIELD_COLOUR_SCHEME_IS_ACTIVE].Bool = value; }
            }
            public bool IsSystem
            {
                get { return m_sqlRow[FIELD_COLOUR_SCHEME_IS_SYSTEM].Bool; }
                set { m_sqlRow[FIELD_COLOUR_SCHEME_IS_SYSTEM].Bool = value; }
            }
        }

        // Query objects
        private static CQryColourScheme m_qryColourScheme = new CQryColourScheme();

        public static void SetActive(int colourSchemeID)
        {
            CSqlDB.Instance.Conn.Execute("UPDATE ColourScheme SET IsActive = (CASE ColourSchemeID WHEN " + colourSchemeID + " THEN 1 ELSE 0 END)");
        }

        public static List<ColourNode> GetColours()
        {
            List<ColourNode> nodes = new List<ColourNode>();

            m_qryColourScheme.MakeFieldsNull();
            m_qryColourScheme.SelectExtraCondition = "";
            if(m_qryColourScheme.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                do
                {
                    nodes.Add(new ColourNode(m_qryColourScheme));
                } while(m_qryColourScheme.Fetch());
            }
            return nodes;
        }

        public static bool GetActiveColour(out ColourNode node)
        {
            m_qryColourScheme.MakeFieldsNull();
            m_qryColourScheme.SelectExtraCondition = "WHERE IsActive = 1";
            if(m_qryColourScheme.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                node = new ColourNode(m_qryColourScheme);
                return true;
            }
            node = new ColourNode();
            return false;
        }
    }

    public struct ColourNode
    {
        public readonly int    colourSchemeID;
        public readonly string name;
        public readonly string description;
        public bool   isActive;
        public bool   isSystem;

        public ColorScheme scheme;

        public ColourNode(CColourSchemeSQL.CQryColourScheme qry)
        {
            this.colourSchemeID = qry.ColourSchemeID;
            this.name           = qry.Name;
            this.description    = qry.Description;
            this.isActive       = qry.IsActive;
            this.isSystem       = qry.IsSystem;

            this.scheme         = null;
        }
    }
}
