using System;
using System.Collections.Generic;

using core;
using static core.CSystemAttributeSQL;

namespace glc.Settings
{
    public class CSystemAttributeContainer : CGenericSettingContainer<SystemAttributeNode>
    {
        public CSystemAttributeContainer()
        {
            DataList    = CSystemAttributeSQL.GetAllNodes();
            DataSource  = new CSystemAttributeDataSource(DataList);
        }

        public override void EditNode(int selectionIndex)
        {
            SystemAttributeNode selected = DataList[selectionIndex];
            CEditDlg dlg;

            switch(selected.AttributeType)
            {
                case AttributeType.cTypeInteger:
                    dlg = new CEditIntDlg(selected.AttributeDescription, selected.AttributeValue);
                    break;

                case AttributeType.cTypeBool:
                    dlg = new CEditBoolDlg(selected.AttributeDescription, selected.IsTrue());
                    break;

                case AttributeType.cTypeString:
                default:
                    dlg = new CEditStringDlg(selected.AttributeDescription, selected.AttributeValue);
                    break;
            }

            if(dlg.Run(ref selected))
            {
                CSystemAttributeSQL.UpdateNode(selected);
                DataList[selectionIndex] = selected;
                DataSource.ToList()[selectionIndex] = selected;
            }
        }

        public override void SelectNode(int selectionIndex)
        {
            EditNode(selectionIndex);
        }
    }

    internal class CSystemAttributeDataSource : CGenericDataSource<SystemAttributeNode>
    {
        private readonly long m_maxDescLength;

        public CSystemAttributeDataSource(List<SystemAttributeNode> itemList)
            : base(itemList)
        {
            for(int i = 0; i < itemList.Count; i++)
            {
                string description = ItemList[i].AttributeDescription;
                if(description.Length > m_maxDescLength)
                {
                    m_maxDescLength = description.Length;
                }
            }
        }

        protected override string ConstructString(int itemIndex)
        {
            string description = ItemList[itemIndex].AttributeDescription;
            string value       = ItemList[itemIndex].AttributeValue;
            String s1 = String.Format(String.Format("{{0,{0}}}", -m_maxDescLength), description);
            return $"{s1}  {value}";
        }

        protected override string GetString(int itemIndex)
        {
            return ItemList[itemIndex].AttributeDescription;
        }
    }
}
