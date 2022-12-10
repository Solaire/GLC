using Terminal.Gui;
using core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using glc.Settings;

namespace glc.UI.Settings
{
    public class CSettingsCategoriesPanel : CFramePanel<SettingCategory, ListView>
    {
        public CSettingsCategoriesPanel(string name, Pos x, Pos y, Dim width, Dim height, bool canFocus)
            : base(name, x, y, width, height, canFocus)
        {
            Initialise(name, x, y, width, height, canFocus);
        }

        public override void CreateContainerView()
        {
            m_containerView = new ListView(
                                new CSettingsDataSource(Enum.GetValues(typeof(SettingCategory))
                                .Cast<SettingCategory>()
                                .ToList()))
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
