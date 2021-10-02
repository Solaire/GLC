using GLC_Structs;
using System;

namespace GLC
{
    /// <summary>
    /// Panel implementation class for the game list on the library page
    /// </summary>
    public sealed class CGamesPanel : CPanel
    {
        public CGamesPanel(int percentWidth, int percentHeight, CPage parentPage) : base("Games", PanelType.cGames, percentWidth, percentHeight, parentPage)
        {

        }

        public void Initalise()
        {

        }

#region CControl overrides
        public override void Redraw(bool fullRedraw)
        {
            CConsoleEx.DrawColourRect(m_rect, m_parentPage.GetColour(ColourThemeIndex.cPanelMainBg));
            if(m_bottomBorder)
            {
                CConsoleEx.DrawHorizontalLine(m_rect.x, m_rect.height - 1, m_rect.width - 1, m_parentPage.GetColour(ColourThemeIndex.cPanelBorderBg), m_parentPage.GetColour(ColourThemeIndex.cPanelBorderFg));
            }
            if(m_rightBorder)
            {
                CConsoleEx.DrawVerticalLine(m_rect.width - 1, m_rect.y, m_rect.height - 1, m_parentPage.GetColour(ColourThemeIndex.cPanelBorderBg), m_parentPage.GetColour(ColourThemeIndex.cPanelBorderFg));
            }
        }

        public override void OnEnter()
        {
            throw new NotImplementedException();
        }

        public override void OnUpArrow()
        {
            throw new NotImplementedException();
        }

        public override void OnDownArrow()
        {
            throw new NotImplementedException();
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
