using System;
using System.Collections;
using System.Collections.Generic;

using Terminal.Gui;

using glc.ColourScheme;
using glc.UI;

namespace glc.Settings
{
    public class CColourSettingsContainer : CGenericSettingContainer<ColourNode>
    {
        public CColourSettingsContainer()
        {
            DataList = CColourSchemeSQL.GetColours();
            for(int i = 0; i < DataList.Count; i++)
            {
                if(DataList[i].IsSystem)
                {
                    ColourNode temp = DataList[i];
                    temp.scheme = Colors.ColorSchemes[DataList[i].Name];
                    DataList[i] = temp;
                }
            }

            DataSource = new CColourDataSource(DataList);
        }

        public override void EditNode(int selectionIndex)
        {
            SelectNode(selectionIndex);

            // TODO: Implement custom colours
        }

        public override void SelectNode(int selectionIndex)
        {
            Application.Top.ColorScheme = DataList[selectionIndex].scheme;
            CColourSchemeSQL.SetActive(DataList[selectionIndex].PrimaryKey);
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
                if(ItemList[i].Name.Length > m_maxTitleLength)
                {
                    m_maxTitleLength = ItemList[i].Name.Length;
                }
                SetMark(i, ItemList[i].IsEnabled);
            }
        }

        protected override string ConstructString(int itemIndex)
        {
            string title        = ItemList[itemIndex].Name;
            string description  = ItemList[itemIndex].Description;
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
}
