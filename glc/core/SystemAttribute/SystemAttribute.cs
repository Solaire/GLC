using core.DataAccess;

namespace core.SystemAttribute
{
    /// <summary>
    /// Structure representing a system attribute node
    /// </summary>
    public struct SystemAttributeNode
    {
        public readonly string AttributeName { get; }
        public readonly string AttributeDescription { get; }
        public string AttributeValue { get; set; }
        public CSystemAttributeSQL.AttributeType AttributeType { get; set; }

        public SystemAttributeNode(string name, string description, string value, CSystemAttributeSQL.AttributeType type)
        {
            AttributeName = name;
            AttributeDescription = description;
            AttributeValue = value;
            AttributeType = type;
        }

        public SystemAttributeNode(CSystemAttributeSQL.CQryAttribute qryAttribute)
        {
            AttributeName = qryAttribute.AttributeName;
            AttributeDescription = qryAttribute.AttributeDesc;
            AttributeValue = qryAttribute.AttributeValue;
            AttributeType = (CSystemAttributeSQL.AttributeType)qryAttribute.AttributeType;
        }

        public bool IsTrue()
        {
            return AttributeValue.ToLower() == CSystemAttributeSQL.BOOL_VALUE_TRUE;
        }

        public void SetBool(bool isTrue)
        {
            AttributeValue = (isTrue) ? CSystemAttributeSQL.BOOL_VALUE_TRUE : CSystemAttributeSQL.BOOL_VALUE_FALSE;
        }
    }
}
