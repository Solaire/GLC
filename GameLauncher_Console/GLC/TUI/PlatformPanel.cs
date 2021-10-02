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
            m_currentItem = 0;
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
                int currentItemY = m_rect.y + m_currentItem + 1;
                CConsoleEx.WriteText(m_platforms[m_currentItem], 0, currentItemY, CConstants.TEXT_PADDING_LEFT, m_rect.width - 1, m_parentPage.GetColour(ColourThemeIndex.cPanelSelectFocusBg), m_parentPage.GetColour(ColourThemeIndex.cPanelSelectFocusFg));

                if(m_currentItem > 0)
                {
                    int adjecentItemY = m_rect.y + m_currentItem;
                    CConsoleEx.WriteText(m_platforms[m_currentItem - 1], 0, adjecentItemY, CConstants.TEXT_PADDING_LEFT, m_rect.width - 1, m_parentPage.GetColour(ColourThemeIndex.cPanelMainBg), m_parentPage.GetColour(ColourThemeIndex.cPanelMainFg));
                }

                if(m_currentItem < m_platforms.Length - 1)
                {
                    int adjecentItemY = m_rect.y + m_currentItem + 2;
                    CConsoleEx.WriteText(m_platforms[m_currentItem + 1], 0, adjecentItemY, CConstants.TEXT_PADDING_LEFT, m_rect.width - 1, m_parentPage.GetColour(ColourThemeIndex.cPanelMainBg), m_parentPage.GetColour(ColourThemeIndex.cPanelMainFg));
                }

                return;
            }

            CConsoleEx.DrawColourRect(m_rect, ConsoleColor.Black);
            if(m_bottomBorder)
            {
                CConsoleEx.DrawHorizontalLine(m_rect.x, m_rect.height - 1, m_rect.width, m_parentPage.GetColour(ColourThemeIndex.cPanelBorderBg), m_parentPage.GetColour(ColourThemeIndex.cPanelBorderFg));
            }
            if(m_rightBorder)
            {
                CConsoleEx.DrawVerticalLine(m_rect.width - 1, m_rect.y, m_rect.height, m_parentPage.GetColour(ColourThemeIndex.cPanelBorderBg), m_parentPage.GetColour(ColourThemeIndex.cPanelBorderFg));
            }

            for(int row = m_rect.x + 1, i = 0; row < m_rect.x + m_rect.width && i < m_platforms.Length; row++, i++)
            {
                ColourThemeIndex background = (m_currentItem == i) ? ColourThemeIndex.cPanelSelectFocusBg : ColourThemeIndex.cPanelMainBg;
                ColourThemeIndex foreground = (m_currentItem == i) ? ColourThemeIndex.cPanelSelectFocusFg : ColourThemeIndex.cPanelMainFg;
                CConsoleEx.WriteText(m_platforms[i], 0, row, CConstants.TEXT_PADDING_LEFT, m_rect.width - 1, m_parentPage.GetColour(background), m_parentPage.GetColour(foreground));
            }

        }

        public override void OnEnter()
        {
            throw new NotImplementedException();
        }

        public override void OnUpArrow()
        {
            m_currentItem = Math.Max(m_currentItem - 1, 0);
        }

        public override void OnDownArrow()
        {
            m_currentItem = Math.Min(m_currentItem + 1, m_platforms.Length - 1);
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
#endregion // CPanel overrides
    }
}
