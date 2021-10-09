using GLC_Structs;

namespace GLC
{
    class CLibraryPage : CPage
    {
        public delegate void OnFocusEventHandler(object sender, GenericEventArgs<int> e);
        public event OnFocusEventHandler OnFocus;

        private enum PanelIndex
        {
            cPlatform = 0,
            cGameList = 1,
            cGameInfo = 2,
        }

        public CLibraryPage(CWindow parent) : base(parent, "Library")
        {
            m_panels = new CPanel[3];
            m_activePanel = (int)PanelIndex.cPlatform;
        }

        public override void Initialise(PanelType[] panelTypes)
        {
            for(int i = 0; i < panelTypes.Length && i < m_panels.Length; i++)
            {
                PanelData data = CConstants.PANEL_DATA[(int)panelTypes[i]];
                switch(panelTypes[i])
                {
                    case PanelType.cPlatforms:
                    {
                        m_panels[i] = new CPlatformPanel(data.percentWidth, data.percentHeight, this);
                    }
                    break;

                    case PanelType.cGames:
                    {
                        m_panels[i] = new CGamesPanel(data.percentWidth, data.percentHeight, this);
                    }
                    break;

                    case PanelType.cGameInfo:
                    {
                        //m_panels[i] = new CGameInfoPanel(data.percentWidth, data.percentHeight);
                    }
                    break;
                }

                if(m_panels[i] != null)
                {
                    OnFocus += m_panels[i].OnSetFocus;
                }
            }

            m_panels[(int)PanelIndex.cPlatform].OnDirty += m_panels[(int)PanelIndex.cGameList].OnUpdateData;
            //m_panels[(int)PanelIndex.cGameList].OnDirty += m_panels[(int)PanelIndex.cGameInfo].OnUpdateData;

            OnFocus.Invoke(this, new GenericEventArgs<int>((int)PanelIndex.cPlatform));
            CalculatePanelLayout();
            Redraw(true);
        }

        public override void OnTab()
        {
            m_activePanel = (m_activePanel == (int)PanelIndex.cPlatform) ? (int)PanelIndex.cGameList : (int)PanelIndex.cPlatform;
            OnFocus.Invoke(this, new GenericEventArgs<int>(m_activePanel));
        }
    }
}
