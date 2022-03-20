using Terminal.Gui;
using core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace glc
{
    public enum SettingType
    {
        cSelect = 0,
        cMultiSelect = 1,
        cDialog = 2,
    }

    class SettingTypeAttribute : System.Attribute
    {
        public SettingType Type { get; private set; }

        public SettingTypeAttribute(SettingType type)
        {
            this.Type = type;
        }
    }

    public enum SettingCategory
    {
        [Category("General"),   Description("Main application settings"),   SettingTypeAttribute(SettingType.cSelect)]
        cGeneral  = 0,
        [Category("Theme"),     Description("Change and edit colour themes"), SettingTypeAttribute(SettingType.cSelect)]
        cTheme    = 1,
        [Category("Platform"),  Description("Manage platforms extensions"), SettingTypeAttribute(SettingType.cSelect)]
        cPlatform = 2,
        [Category("Tags"),      Description("Edit game tags"),              SettingTypeAttribute(SettingType.cSelect)]
        cTags     = 3,
    }

    public class CSettingsCategoriesPanel : CFramePanel<SettingCategory, ListView>
    {
        public CSettingsCategoriesPanel(string name, Pos x, Pos y, Dim width, Dim height, bool canFocus, Key focusShortCut)
            : base(name, x, y, width, height, canFocus, focusShortCut)
        {
            m_contentList = Enum.GetValues(typeof(SettingCategory))
                            .Cast<SettingCategory>()
                            .ToList();
            Initialise(name, x, y, width, height, canFocus, focusShortCut);
        }

        public override void CreateContainerView()
        {
            m_containerView = new ListView(new CSettingsDataSource(m_contentList))
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                AllowsMarking = false,
                CanFocus = true
            };

            m_frameView.Add(m_containerView);
        }
    }

    internal class CSettingsDataSource : CGenericDataSource<SettingCategory>
    {
        private readonly long m_maxCategoryLength;

        public CSettingsDataSource(List<SettingCategory> itemList)
            : base(itemList)
        {
            for(int i = 0; i < itemList.Count; i++)
            {
                string category = ItemList[i].GetDescription<CategoryAttribute>().Category;
                if(category.Length > m_maxCategoryLength)
                {
                    m_maxCategoryLength = category.Length;
                }
            }
        }

        protected override string ConstructString(int itemIndex)
        {
            string category     = ItemList[itemIndex].GetDescription<CategoryAttribute>().Category;
            string description  = ItemList[itemIndex].GetDescription<DescriptionAttribute>().Description;
            String s1 = String.Format(String.Format("{{0,{0}}}", -m_maxCategoryLength), category);
            return $"{s1}  {description}";
        }

        protected override string GetString(int itemIndex)
        {
            return ItemList[itemIndex].GetDescription<CategoryAttribute>().Category;
        }
    }
}
