using System;
using ConsoleUI;
using ConsoleUI.Structs;
using ConsoleUI.Event;

namespace GLC
{
    /// <summary>
    /// Panel implementation class for the game list on the library page
    /// </summary>
    public sealed class CGamesPanel : CPanel
    {
        public CGamesPanel(string title, PanelTypeCode type, int percentWidth, int percentHeight, CPage parent) : base(title, type, percentWidth, percentHeight, parent)
        {
            m_panelElements = new string[]
            {
                "Game 1",
                "Game 2",
                "Game 3",
                "Game 4",
            };
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
            for(int i = 0; i < m_panelElements.Length; i++)
            {
                m_panelElements[i] = e.Data + " - Game " + i.ToString();
            }
            Redraw(true);
            FireDataChangedEvent(e.Data); // Will update the item info panel
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
    }
}
