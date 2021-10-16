using System;
using ConsoleUI;
using ConsoleUI.Structs;
using ConsoleUI.Event;

namespace GLC
{
    /// <summary>
    /// Panel implementation class for the platform list on the library page
    /// </summary>
    public sealed class CPlatformPanel : CPanel
    {
        public CPlatformPanel(string title, PanelTypeCode type, int percentWidth, int percentHeight, CPage parent) : base(title, type, percentWidth, percentHeight, parent)
        {
            m_panelElements = new string[]
            {
                "Platform 1",
                "Platform 2",
                "Platform 3",
                "Platform 4",
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
            throw new NotImplementedException();
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
