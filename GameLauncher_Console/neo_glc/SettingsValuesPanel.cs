using Terminal.Gui;
using core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace glc
{
    public struct SettingNode
    {
        public string name;
        public string dbAttributeName;
        public string value;

        public SettingNode(string name, string dbAttributeName, string value)
        {
            this.name = name;
            this.dbAttributeName = dbAttributeName;
            this.value = value;
        }
    }

    public class CSettingsValuesPanel : CFramePanel<SettingNode, ListView>
    {
        private SettingType m_settingType;

        public CSettingsValuesPanel(SettingCategory category, Pos x, Pos y, Dim width, Dim height, bool canFocus, Key focusShortCut)
            : base("", x, y, width, height, canFocus, focusShortCut)
        {
            m_contentList = new List<SettingNode>();

            switch(category)
            {
                case SettingCategory.cGeneral:
                    m_contentList.Add(new SettingNode("General 1", "DB_GENERAL_1", "test"));
                    m_contentList.Add(new SettingNode("General 2", "DB_GENERAL_2", "Hello world"));
                    m_contentList.Add(new SettingNode("General 3", "DB_GENERAL_3", "Y"));
                    m_contentList.Add(new SettingNode("General 4", "DB_GENERAL_4", "12"));
                    break;

                case SettingCategory.cTheme:
                    m_contentList.Add(new SettingNode("Theme 1", "DB_THEME_1", "Solarized"));
                    m_contentList.Add(new SettingNode("Theme 2", "DB_THEME_2", "Midnight blue"));
                    m_contentList.Add(new SettingNode("Theme 3", "DB_THEME_3", "Crimson"));
                    m_contentList.Add(new SettingNode("Theme 4", "DB_THEME_4", "Dracula"));
                    break;

                case SettingCategory.cPlatform:
                    m_contentList.Add(new SettingNode("Steam",  "DB_PLATFORM_1", "Enabled"));
                    m_contentList.Add(new SettingNode("Gog",    "DB_PLATFORM_2", "Disabled"));
                    m_contentList.Add(new SettingNode("Test",   "DB_PLATFORM_3", "Enabled"));
                    break;

                case SettingCategory.cTags:
                    m_contentList.Add(new SettingNode("Installed",  "DB_TAG_1", "Enabled"));
                    m_contentList.Add(new SettingNode("RPG",        "DB_TAG_2", "Disabled"));
                    m_contentList.Add(new SettingNode("Fighting",   "DB_TAG_3", "Enabled"));
                    m_contentList.Add(new SettingNode("Modded",     "DB_TAG_4", "Diabled"));
                    break;

                default:
                    break;
            }

            string s = category.GetDescription<CategoryAttribute>().Category;
            Initialise(s, x, y, width, height, canFocus, focusShortCut);
        }

        public override void CreateContainerView()
        {
            m_containerView = new ListView(new CSettingsValueSource(m_contentList))
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                CanFocus = true,
                AllowsMarking = (m_settingType != SettingType.cDialog),
                AllowsMultipleSelection = (m_settingType == SettingType.cMultiSelect),
            };

            m_frameView.Add(m_containerView);
        }
    }

    internal class CSettingsValueSource : CGenericDataSource<SettingNode>
    {
        private readonly long m_maxCategoryLength;

        public CSettingsValueSource(List<SettingNode> itemList)
            : base(itemList)
        {
            for(int i = 0; i < itemList.Count; i++)
            {
                string name = ItemList[i].name;
                if(name.Length > m_maxCategoryLength)
                {
                    m_maxCategoryLength = name.Length;
                }
            }
        }

        protected override string ConstructString(int itemIndex)
        {
            String s1 = String.Format(String.Format("{{0,{0}}}", -m_maxCategoryLength), ItemList[itemIndex].name);
            return $"{s1}  {ItemList[itemIndex].value}";
        }

        protected override string GetString(int itemIndex)
        {
            return ItemList[itemIndex].name;
        }
    }
}
