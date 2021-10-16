using System;
using ConsoleUI;
using ConsoleUI.Event;
using ConsoleUI.Structs;

namespace GLC
{
    public class CGameInfoPanel : CPanel
    {
        internal struct GameInfo
        {
            public string itemName;
            public string itemClass;
            public int    itemCounter;
            public bool   isActive;

            public GameInfo(string itName, string itClass, int itCounter, bool isActive)
            {
                itemName = itName;
                itemClass = itClass;
                itemCounter = itCounter;
                this.isActive = isActive;
            }

            public void SetInfo(string itName, string itClass, int itCounter, bool isActive)
            {
                itemName = itName;
                itemClass = itClass;
                itemCounter = itCounter;
                this.isActive = isActive;
            }
        }

        private GameInfo m_currentItem;

        public CGameInfoPanel(string title, PanelTypeCode type, int percentWidth, int percentHeight, CPage parent) : base(title, type, percentWidth, percentHeight, parent)
        {
            m_panelElements = new string[0];
            m_currentItem = new GameInfo("", "", 0, false);
        }

        public override void Initialise()
        {
            throw new NotImplementedException();
        }

        public override void OnResize(object sender, ResizeEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void OnUpdateData(object sender, GenericEventArgs<string> e)
        {
            m_currentItem.itemName = "item name";
            m_currentItem.itemClass = e.Data;
            m_currentItem.itemCounter++;
            m_currentItem.isActive = (m_currentItem.itemCounter % 2 == 0);
            Redraw(true);
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }

        protected override void DrawHighlighted(bool isFocused)
        {
            throw new NotImplementedException();
        }

        protected override bool LoadContent()
        {
            throw new NotImplementedException();
        }

        protected override void ReloadContent()
        {
            throw new NotImplementedException();
        }

        public override void Redraw(bool fullRedraw)
        {
            // Background and borders
            CConsoleEx.DrawColourRect(m_area, m_parent.GetColour(ColourThemeIndex.cPanelMainBG));
            if(BottomBorder)
            {
                CConsoleEx.DrawHorizontalLine(m_area.x, m_area.Bottom - 1, m_area.width, m_parent.GetColour(ColourThemeIndex.cPanelBorderBG), m_parent.GetColour(ColourThemeIndex.cPanelBorderFG));
            }
            if(RightBorder)
            {
                CConsoleEx.DrawVerticalLine(m_area.Right - 1, m_area.y, m_area.height, m_parent.GetColour(ColourThemeIndex.cPanelBorderBG), m_parent.GetColour(ColourThemeIndex.cPanelBorderFG));
            }

            // Panel header
            CConsoleEx.WriteText(m_title, m_area.x, m_area.y, 1 /*CONSTANT*/, m_area.width, m_parent.GetColour(ColourThemeIndex.cPanelBorderBG), m_parent.GetColour(ColourThemeIndex.cPanelBorderFG));

            // Content
            int row = m_area.y + 1;
            CConsoleEx.WriteText(m_currentItem.itemClass, m_area.x + 1, row++, 1 /*CONSTANT*/, m_area.width - 1, m_parent.GetColour(ColourThemeIndex.cPanelMainBG), m_parent.GetColour(ColourThemeIndex.cPanelMainFG));
            CConsoleEx.WriteText(m_currentItem.itemName, m_area.x + 1, row++, 1 /*CONSTANT*/, m_area.width - 1, m_parent.GetColour(ColourThemeIndex.cPanelMainBG), m_parent.GetColour(ColourThemeIndex.cPanelMainFG));
            CConsoleEx.WriteText(m_currentItem.itemCounter.ToString(), m_area.x + 1, row++, 1 /*CONSTANT*/, m_area.width - 1, m_parent.GetColour(ColourThemeIndex.cPanelMainBG), m_parent.GetColour(ColourThemeIndex.cPanelMainFG));
            CConsoleEx.WriteText((m_currentItem.isActive) ? "active" : "inactive", m_area.x + 1, row++, 1 /*CONSTANT*/, m_area.width - 1, m_parent.GetColour(ColourThemeIndex.cPanelMainBG), m_parent.GetColour(ColourThemeIndex.cPanelMainFG));
        }
    }
}
