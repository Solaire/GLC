using System;
using System.Collections.Generic;

namespace core.SystemAttribute
{
    /// <summary>
    /// Class for accessing and modyding the "SystemAttribute" table.
    /// Also contains all system attribute names
    /// </summary>
    public static class CSystemAttributeSQL
    {
        // List of supported core system attributes
        public const string A_HIDE_EMPTY_PLATFORMS  = "HIDE_EMPTY_PLATFORMS";
        public const string A_HIDE_EMPTY_GROUPS     = "HIDE_EMPTY_GROUPS";
        public const string A_CLOSE_AFTER_LAUNCHING = "CLOSE_AFTER_LAUNCHING";
        public const string A_SHOW_SEARCH_IN_DIALOG = "SHOW_SEARCH_IN_DIALOG";
        public const string A_SHOW_GAME_INFO_PANEL  = "SHOW_GAME_INFO_PANEL";

        // Query parameter names
        private const string FIELD_ATTRIBUTE_NAME  = "AttributeName";
        private const string FIELD_ATTRIBUTE_VALUE = "AttributeValue";
        private const string FIELD_ATTRIBUTE_DESC  = "AttributeDesc";
        private const string FIELD_ATTRIBUTE_TYPE  = "AttributeType";

        public const string BOOL_VALUE_TRUE  = "true";
        public const string BOOL_VALUE_FALSE = "false";

        public enum AttributeType
        {
            cTypeInteger = 0,
            cTypeBool    = 1,
            cTypeString  = 2,
        }

        /// <summary>
        /// Query for reading system attribute
        /// </summary>
        public class CQryAttribute : CSqlQry
        {
            public CQryAttribute()
                : base("SystemAttribute", "", "")
            {
                m_sqlRow[FIELD_ATTRIBUTE_NAME]  = new CSqlFieldString(FIELD_ATTRIBUTE_NAME,  CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cSelWhere | CSqlField.QryFlag.cUpdWhere);
                m_sqlRow[FIELD_ATTRIBUTE_VALUE] = new CSqlFieldString(FIELD_ATTRIBUTE_VALUE, CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_ATTRIBUTE_DESC]  = new CSqlFieldString(FIELD_ATTRIBUTE_DESC,  CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_ATTRIBUTE_TYPE]  = new CSqlFieldInteger(FIELD_ATTRIBUTE_TYPE, CSqlField.QryFlag.cSelRead);
            }
            public string AttributeName
            {
                get { return m_sqlRow[FIELD_ATTRIBUTE_NAME].String; }
                set { m_sqlRow[FIELD_ATTRIBUTE_NAME].String = value; }
            }
            public string AttributeValue
            {
                get { return m_sqlRow[FIELD_ATTRIBUTE_VALUE].String; }
                set { m_sqlRow[FIELD_ATTRIBUTE_VALUE].String = value; }
            }
            public string AttributeDesc
            {
                get { return m_sqlRow[FIELD_ATTRIBUTE_DESC].String; }
                set { m_sqlRow[FIELD_ATTRIBUTE_DESC].String = value; }
            }
            public int AttributeType
            {
                get { return m_sqlRow[FIELD_ATTRIBUTE_TYPE].Integer; }
                set { m_sqlRow[FIELD_ATTRIBUTE_TYPE].Integer = value; }
            }
        }

        // Query objects
        private static CQryAttribute m_qryAttribute = new CQryAttribute();

        /// <summary>
        /// Return all system attributes in the database
        /// </summary>
        /// <returns>List of SystemAttributeNode objects</returns>
        public static List<SystemAttributeNode> GetAllNodes()
        {
            List<SystemAttributeNode> nodes = new List<SystemAttributeNode>();

            m_qryAttribute.MakeFieldsNull();
            if(m_qryAttribute.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                do
                {
                    nodes.Add(new SystemAttributeNode(m_qryAttribute));
                } while(m_qryAttribute.Fetch());
            }
            return nodes;
        }

        /// <summary>
        /// Read specified system attribute and return a SystemAttributeNode obejct
        /// </summary>
        /// <param name="attributeName">The attribute name</param>
        /// <returns>Nullable SystemAttributeNode instance, null if lookup failed</returns>
        public static SystemAttributeNode ? GetNode(string attributeName)
        {
            m_qryAttribute.MakeFieldsNull();
            m_qryAttribute.AttributeName = attributeName;
            if(m_qryAttribute.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                return new SystemAttributeNode(m_qryAttribute);
            }
            return null;
        }

        /// <summary>
        /// Read specified system attribute and return the value as string
        /// </summary>
        /// <param name="attributeName">The attribute name</param>
        /// <returns>AttributeValue as string, or empty string if lookup failed</returns>
        public static string GetStringValue(string attributeName)
        {
            m_qryAttribute.MakeFieldsNull();
            m_qryAttribute.AttributeName = attributeName;
            if(m_qryAttribute.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                return m_qryAttribute.AttributeValue;
            }
            return "";
        }

        /// <summary>
        /// Read specified system attribute and return the value as int
        /// </summary>
        /// <param name="attributeName">The attribute name</param>
        /// <returns>AttributeValue as int, or 0 if lookup failed</returns>
        public static int GetIntValue(string attributeName)
        {
            m_qryAttribute.MakeFieldsNull();
            m_qryAttribute.AttributeName = attributeName;
            if(m_qryAttribute.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                Int32.TryParse(m_qryAttribute.AttributeValue, out int intValue);
                return intValue;
            }
            return 0;
        }

        /// <summary>
        /// Read specified system attribute and return the value as boolean
        /// </summary>
        /// <param name="attributeName">The attribute name</param>
        /// <returns>True if AttributeValue is true, otherwise false</returns>
        public static bool GetBoolValue(string attributeName)
        {
            m_qryAttribute.MakeFieldsNull();
            m_qryAttribute.AttributeName = attributeName;
            if(m_qryAttribute.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                return IsTrue(m_qryAttribute.AttributeValue);
            }
            return false;
        }

        /// <summary>
        /// Check if specified attribute is in database
        /// </summary>
        /// <param name="attributeName">The attribute name</param>
        /// <returns>True lookup successful, otherwise false</returns>
        public static bool IsAttributeInDB(string attributeName)
        {
            m_qryAttribute.MakeFieldsNull();
            m_qryAttribute.AttributeName = attributeName;
            return (m_qryAttribute.Select() == System.Data.SQLite.SQLiteErrorCode.Ok);
        }

        /// <summary>
        /// Check if attribute value is true
        /// </summary>
        /// <param name="attributeValue">The attribute value</param>
        /// <returns>True if value is "y", otherwise false</returns>
        private static bool IsTrue(string attributeValue)
        {
            return attributeValue.ToLower() == BOOL_VALUE_TRUE;
        }

        /// <summary>
        /// Update system attribute node
        /// </summary>
        /// <param name="node">The SystemAttributeNode to save</param>
        /// <returns>True on update success, otherwise false</returns>
        public static bool UpdateNode(SystemAttributeNode node)
        {
            m_qryAttribute.MakeFieldsNull();
            m_qryAttribute.AttributeName  = node.AttributeName;
            m_qryAttribute.AttributeValue = node.AttributeValue;
            return (m_qryAttribute.Update() == System.Data.SQLite.SQLiteErrorCode.Ok);
        }
    }
}
