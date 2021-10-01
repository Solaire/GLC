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
        public CPlatformPanel(int percentWidth, int percentHeight) : base("Library", PanelType.cPanel_Platforms, percentWidth, percentHeight)
        {
            m_platforms = new string[]
            {
                "Platform 1",
                "Platform 1",
                "Platform 1",
                "Platform 1",
            };
        }

        public void Initalise()
        {

        }

#region CPanel overrides
        public override void Redraw()
        {
            CConsoleEx.DrawColourRect(m_rect, ConsoleColor.Red);
            if(m_bottomBorder)
            {
                CConsoleEx.DrawHorizontalLine(m_rect.x, m_rect.height - 1, m_rect.width);
            }
            if(m_rightBorder)
            {
                CConsoleEx.DrawVerticalLine(m_rect.width - 1, m_rect.y, m_rect.height);
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
