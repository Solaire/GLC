using System;
using ConsoleUI;
using ConsoleUI.Event;
using ConsoleUI.Structs;

namespace GLC
{
    class CLibraryPage : CPage
    {
        private enum PanelIndex
        {
            cPlatform = 0,
            cGameList = 1,
            cGameInfo = 2,
        }

        public CLibraryPage(CWindow parent, ConsoleRect area, string title, int panelCount) : base(parent, area, title, panelCount)
        {

        }

        public override void Initialise(PanelTypeCode[] panelTypes)
        {
            for(int i = 0; i < panelTypes.Length && i < m_panels.Length; i++)
            {
                PanelData data = CConstants.PANEL_DATA[panelTypes[i].Code];

                if(panelTypes[i] == PanelType.PLATFORM_PANEL)
                {
                    m_panels[i] = new CPlatformPanel(data.title, panelTypes[i], data.percentWidth, data.percentHeight, this);
                }
                else if(panelTypes[i] == PanelType.GAME_LIST_PANEL)
                {
                    m_panels[i] = new CGamesPanel(data.title, panelTypes[i], data.percentWidth, data.percentHeight, this);
                }
                else if(panelTypes[i] == PanelType.GAME_INFO_PANEL)
                {
                    m_panels[i] = new CGameInfoPanel(data.title, panelTypes[i], data.percentWidth, data.percentHeight, this);
                }

                if(m_panels[i] != null)
                {
                    FocusChange += m_panels[i].OnSetFocus;
                }
            }

            m_panels[PanelType.PLATFORM_PANEL.Code].OnDataChangedString  += m_panels[PanelType.GAME_LIST_PANEL.Code].OnUpdateData;
            m_panels[PanelType.GAME_LIST_PANEL.Code].OnDataChangedString += m_panels[PanelType.GAME_INFO_PANEL.Code].OnUpdateData;

            FireFocusChangeEvent(PanelType.PLATFORM_PANEL);
            CalculatePanelLayout();
            Redraw(true);
        }

        public override void Initialise()
        {
            throw new NotImplementedException();
        }

        public override void KeyPressed(ConsoleKeyInfo keyInfo)
        {
            if(keyInfo.Key == ConsoleKey.Tab)
            {
                m_focusedPanelIndex = (m_focusedPanelIndex == (int)PanelIndex.cPlatform) ? (int)PanelIndex.cGameList : (int)PanelIndex.cPlatform;
                FireFocusChangeEvent(m_panels[m_focusedPanelIndex].PanelType);
            }
            else
            {
                m_panels[m_focusedPanelIndex].KeyPressed(keyInfo);
            }
        }

        public override void OnResize(object sender, ResizeEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
