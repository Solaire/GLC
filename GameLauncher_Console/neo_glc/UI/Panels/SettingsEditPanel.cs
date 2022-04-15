using Terminal.Gui;

using glc.Settings;

namespace glc
{
    public class CSettingsEditPanel : CFramePanel<int, ListView> // Base class m_contentList is not used
    {
        private CSettingContainer[] m_settingContainers;
        private int m_selectedContainer;

        public CSettingsEditPanel(string name, Pos x, Pos y, Dim width, Dim height, bool canFocus)
            : base(name, x, y, width, height, canFocus)
        {
            m_settingContainers = new CSettingContainer[4];
            m_settingContainers[0] = new CSystemSettingsContainer();
            m_settingContainers[1] = new CColourContainer();
            m_settingContainers[2] = new CPlatformSettingsContainer();
            m_settingContainers[3] = new CTagsSettingsContainer();

            m_selectedContainer = 0;

            //m_contentList = new List<SettingNode>();
            Initialise(name, x, y, width, height, canFocus);
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
