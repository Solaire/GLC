using GLC_Structs;
using System;

namespace GLC
{
    /// <summary>
    /// Panel implementation class for the game list on the library page
    /// </summary>
    public sealed class CGamesPanel : CPanel
    {
        public CGamesPanel(int percentWidth, int percentHeight) : base("Games", PanelType.cPanel_Games, percentWidth, percentHeight)
        {

        }

        public void Initalise()
        {

        }

#region CPanel overrides
        public override void Redraw()
        {
            CConsoleEx.DrawColourRect(m_rect, ConsoleColor.Green);
            if(m_bottomBorder)
            {
                CConsoleEx.DrawHorizontalLine(m_rect.x, m_rect.height - 1, m_rect.width - 1);
            }
            if(m_rightBorder)
            {
                CConsoleEx.DrawVerticalLine(m_rect.width - 1, m_rect.y, m_rect.height - 1);
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
#endregion

#region CControl overrides
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
#endregion
    }
}
