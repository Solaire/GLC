using System;
using System.Collections.Generic;

using core;
using static core.CSystemAttributeSQL;

namespace glc.Settings
{
    public class CPlatformContainer : CGenericSettingContainer<CBasicPlatform>
    {
        public CPlatformContainer()
        {
            DataList = CPlatformSQL.ListPlatforms();
            DataSource = new CPlatformDataSource(DataList);
        }

        public override void EditNode(int selectionIndex)
        {
            CBasicPlatform selected = DataList[selectionIndex];
            CEditPlatformDlg dlg = new CEditPlatformDlg(selected);

            SystemAttributeNode temp = new SystemAttributeNode("temp", "temp", "temp", AttributeType.cTypeBool);

            if(dlg.Run(ref temp))
            {
                CPlatformSQL.ToggleActive(selected.ID, temp.IsTrue());

                List<int> enabledTags = new List<int>();
                foreach(TagObject tag in dlg.Tags)
                {
                    if(tag.isActive)
                    {
                        enabledTags.Add(tag.tagID);
                    }
                }

                CPlatformSQL.SetTags(selected.ID, enabledTags);
                selected.IsActive = temp.IsTrue();
                DataList[selectionIndex] = selected;
                DataSource.ToList()[selectionIndex] = selected;
            }
        }

        public override void SelectNode(int selectionIndex)
        {
            EditNode(selectionIndex);
        }
    }

    internal class CPlatformDataSource : CGenericDataSource<CBasicPlatform>
    {
        private readonly long m_maxNameLength;
        private readonly long m_maxDescLength;

        public CPlatformDataSource(List<CBasicPlatform> itemList)
            : base(itemList)
        {
            for(int i = 0; i < itemList.Count; i++)
            {
                if(ItemList[i].Name.Length > m_maxNameLength)
                {
                    m_maxNameLength = ItemList[i].Name.Length;
                }
                if(ItemList[i].Description.Length > m_maxDescLength)
                {
                    m_maxDescLength = ItemList[i].Description.Length;
                }
            }
        }

        protected override string ConstructString(int itemIndex)
        {
            String s1 = String.Format(String.Format("{{0,{0}}}", -m_maxNameLength), ItemList[itemIndex].Name);
            String s2 = String.Format(String.Format("{{0,{0}}}", -m_maxDescLength), ItemList[itemIndex].Description);
            string enabled = (ItemList[itemIndex].IsActive) ? "Enabled" : "Disabled";

            return $"{s1}  {s2}  {enabled}";
        }

        protected override string GetString(int itemIndex)
        {
            return ItemList[itemIndex].Name;
        }
    }
}
