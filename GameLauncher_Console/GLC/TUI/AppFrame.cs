using System;
using ConsoleUI.Base;
using ConsoleUI.Type;

namespace GLC
{
    class CAppFrame : CFrame
    {
        public CAppFrame(string title, ConsoleRect area, int pageCount) : base(title, area, pageCount)
        {

        }

        public override void Initialise()
        {
            base.Initialise();


            ConsoleRect pageRect = new ConsoleRect(m_rect.x, m_rect.y + 1, m_rect.width, m_rect.height - 1);

            m_pages[0] = new CLibraryPage("Library", pageRect, 3, this);
            m_pages[0].Initialise();
            Draw(true);
        }

        public override void OnCommand(object sender, GenericEventArgs<string> e)
        {
            throw new System.NotImplementedException();
        }

        public override void KeyPress(ConsoleKeyInfo keyInfo)
        {
            throw new NotImplementedException();
        }
    }
}
