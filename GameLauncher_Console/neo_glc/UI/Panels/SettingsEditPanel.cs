#define TEST

using Terminal.Gui;
using core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using glc.Settings;
using static core.CSystemAttributeSQL;

#if TEST

namespace glc
{
    public class CSettingsEditPanel : CFramePanel<int, ListView> // Base class m_contentList is not used
    {
        private CSettingContainer[] m_settingContainers;
        private int m_selectedContainer;

        public CSettingsEditPanel(string name, Pos x, Pos y, Dim width, Dim height, bool canFocus, Key focusShortCut)
            : base(name, x, y, width, height, canFocus, focusShortCut)
        {
            m_settingContainers = new CSettingContainer[4];
            m_settingContainers[0] = new CSystemAttributeContainer();
            m_settingContainers[1] = new CColourContainer();
            m_settingContainers[2] = new CPlatformContainer();
            m_settingContainers[3] = new CTagContainer();

            m_selectedContainer = 0;

            //m_contentList = new List<SettingNode>();
            Initialise(name, x, y, width, height, canFocus, focusShortCut);
        }

        public override void CreateContainerView()
        {
            m_containerView = new ListView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                AllowsMarking = false,
                AllowsMultipleSelection = false,
                CanFocus = true
            };

            m_containerView.Source = m_settingContainers[m_selectedContainer].DataSource;
            m_frameView.Add(m_containerView);

            m_frameView.KeyDown += KeyDownHandler;
        }

        public void EditValue()
        {
            if(m_selectedContainer < m_settingContainers.Length)
            {
                m_settingContainers[m_selectedContainer].EditNode(ContainerView.SelectedItem);
            }
        }

        public void LoadCategory(SettingCategory category)
        {
            if((int)category >= m_settingContainers.Length)
            {
                return;
            }

            switch(category)
            {
                case SettingCategory.cTheme:
                    m_containerView.AllowsMarking = true;
                    break;

                case SettingCategory.cGeneral:
                case SettingCategory.cPlatform:
                case SettingCategory.cTags:
                    m_containerView.AllowsMarking = false;
                    break;

                default:
                    return;
            }

            m_containerView.Source = m_settingContainers[(int)category].DataSource;
            m_selectedContainer = (int)category;

            FrameView.SetNeedsDisplay();
        }

        private void KeyDownHandler(View.KeyEventEventArgs a)
        {
            //if (a.KeyEvent.Key == Key.Tab || a.KeyEvent.Key == Key.BackTab) {
            //	// BUGBUG: Work around Issue #434 by implementing our own TAB navigation
            //	if (_top.MostFocused == _categoryListView)
            //		_top.SetFocus (_rightPane);
            //	else
            //		_top.SetFocus (_leftPane);
            //}

            if(a.KeyEvent.Key == Key.Space)
            {
                m_settingContainers[m_selectedContainer].SelectNode(ContainerView.SelectedItem);
            }
        }
    }
}

#else

namespace glc
{
    public class CSettingsEditPanel : CFramePanel<SystemAttributeNode, ListView>
    {
        public CSettingsEditPanel(string name, Pos x, Pos y, Dim width, Dim height, bool canFocus, Key focusShortCut)
            : base(name, x, y, width, height, canFocus, focusShortCut)
        {
            m_contentList = CSystemAttributeSQL.GetAllNodes();
            Initialise(name, x, y, width, height, canFocus, focusShortCut);
        }

        public override void CreateContainerView()
        {
            m_containerView = new ListView(new CSettingsEditDataSource(m_contentList))
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

        public void EditValue()
        {
            SystemAttributeNode selected = m_contentList[ContainerView.SelectedItem];
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
                m_contentList[ContainerView.SelectedItem] = selected;
            }
        }
    }

    internal class CSettingsEditDataSource : CGenericDataSource<SystemAttributeNode>
    {
        private readonly long m_maxDescLength;

        public CSettingsEditDataSource(List<SystemAttributeNode> itemList)
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
            string value       = ResolveValue(itemIndex);
            String s1 = String.Format(String.Format("{{0,{0}}}", -m_maxDescLength), description);
            return $"{s1}  {value}";
        }

        protected override string GetString(int itemIndex)
        {
            return ItemList[itemIndex].AttributeDescription;
        }

        private string ResolveValue(int itemIndex)
        {
            if(ItemList[itemIndex].AttributeType == AttributeType.cTypeBool)
            {
                return ItemList[itemIndex].IsTrue() ? "true" : "false";
            }
            return ItemList[itemIndex].AttributeValue;
        }
    }
}

#endif // TEST