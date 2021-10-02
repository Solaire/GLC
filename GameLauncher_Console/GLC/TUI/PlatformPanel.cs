using GLC_Structs;
using System;

namespace GLC
{
    /// <summary>
    /// Panel implementation class for the platform list on the library page
    /// </summary>
    public sealed class CPlatformPanel : CPanel
    {
        string[] m_platforms;

        public CPlatformPanel(int percentWidth, int percentHeight, CPage parentPage) : base("Library", PanelType.cPlatforms, percentWidth, percentHeight, parentPage)
        {
            m_hoveredItemIndex = 0;
            m_platforms = new string[]
            {
                "Platform 1",
                "Platform 2",
                "Platform 3",
                "Platform 4",
            };
        }

        public void Initalise()
        {

        }

#region CControl overrides
        public override void Redraw(bool fullRedraw)
        {
            if(!fullRedraw)
            {
                int currentItemY = m_area.y + m_hoveredItemIndex + 1;
                CConsoleEx.WriteText(m_platforms[m_hoveredItemIndex], 0, currentItemY, CConstants.TEXT_PADDING_LEFT, m_area.width - 1, m_parentPage.GetColour(ColourThemeIndex.cPanelSelectFocusBG), m_parentPage.GetColour(ColourThemeIndex.cPanelSelectFocusFG));

                if(m_hoveredItemIndex > 0)
                {
                    int adjecentItemY = m_area.y + m_hoveredItemIndex;
                    CConsoleEx.WriteText(m_platforms[m_hoveredItemIndex - 1], 0, adjecentItemY, CConstants.TEXT_PADDING_LEFT, m_area.width - 1, m_parentPage.GetColour(ColourThemeIndex.cPanelMainBG), m_parentPage.GetColour(ColourThemeIndex.cPanelMainFG));
                }

                if(m_hoveredItemIndex < m_platforms.Length - 1)
                {
                    int adjecentItemY = m_area.y + m_hoveredItemIndex + 2;
                    CConsoleEx.WriteText(m_platforms[m_hoveredItemIndex + 1], 0, adjecentItemY, CConstants.TEXT_PADDING_LEFT, m_area.width - 1, m_parentPage.GetColour(ColourThemeIndex.cPanelMainBG), m_parentPage.GetColour(ColourThemeIndex.cPanelMainFG));
                }

                return;
            }

            CConsoleEx.DrawColourRect(m_area, ConsoleColor.Black);
            if(m_bottomBorder)
            {
                CConsoleEx.DrawHorizontalLine(m_area.x, m_area.height - 1, m_area.width, m_parentPage.GetColour(ColourThemeIndex.cPanelBorderBG), m_parentPage.GetColour(ColourThemeIndex.cPanelBorderFG));
            }
            if(m_rightBorder)
            {
                CConsoleEx.DrawVerticalLine(m_area.width - 1, m_area.y, m_area.height, m_parentPage.GetColour(ColourThemeIndex.cPanelBorderBG), m_parentPage.GetColour(ColourThemeIndex.cPanelBorderFG));
            }

            for(int row = m_area.x + 1, i = 0; row < m_area.x + m_area.width && i < m_platforms.Length; row++, i++)
            {
                ColourThemeIndex background = (m_hoveredItemIndex == i) ? ColourThemeIndex.cPanelSelectFocusBG : ColourThemeIndex.cPanelMainBG;
                ColourThemeIndex foreground = (m_hoveredItemIndex == i) ? ColourThemeIndex.cPanelSelectFocusFG : ColourThemeIndex.cPanelMainFG;
                CConsoleEx.WriteText(m_platforms[i], 0, row, CConstants.TEXT_PADDING_LEFT, m_area.width - 1, m_parentPage.GetColour(background), m_parentPage.GetColour(foreground));
            }

        }

        public override void OnEnter()
        {
            throw new NotImplementedException();
        }

        public override void OnUpArrow()
        {
            m_hoveredItemIndex = Math.Max(m_hoveredItemIndex - 1, 0);
        }

        public override void OnDownArrow()
        {
            m_hoveredItemIndex = Math.Min(m_hoveredItemIndex + 1, m_platforms.Length - 1);
        }

        public override void OnLeftArrow()
        {
            throw new NotImplementedException();
        }

        public override void OnRightArrow()
        {
            throw new NotImplementedException();
        }
#endregion // CControl overrides

#region CPanel overrides
        protected override bool LoadContent()
        {
            throw new NotImplementedException();
        }

        protected override void ReloadContent()
        {
            throw new NotImplementedException();
        }

        protected override void DrawHighlighted(bool isFocused)
        {
            throw new NotImplementedException();
        }

        public override void OnTab()
        {
            throw new NotImplementedException();
        }

        public override void OnKeyInfo(ConsoleKeyInfo keyInfo)
        {
            throw new NotImplementedException();
        }

        public override bool Update()
        {
            throw new NotImplementedException();
        }
        #endregion // CPanel overrides
    }
}
