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
            CConsoleEx.DrawColourRect(m_area, m_parentPage.GetColour(ColourThemeIndex.cPanelMainBG));
            if(m_bottomBorder)
            {
                CConsoleEx.DrawHorizontalLine(m_area.x, m_area.height - 1, m_area.width - 1, m_parentPage.GetColour(ColourThemeIndex.cPanelBorderBG), m_parentPage.GetColour(ColourThemeIndex.cPanelBorderFG));
            }
            if(m_rightBorder)
            {
                CConsoleEx.DrawVerticalLine(m_area.width - 1, m_area.y, m_area.height - 1, m_parentPage.GetColour(ColourThemeIndex.cPanelBorderBG), m_parentPage.GetColour(ColourThemeIndex.cPanelBorderFG));
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

        public override bool Update()
        {
            throw new NotImplementedException();
        }
        #endregion // CPanel overrides
    }
}
