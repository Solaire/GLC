using System;
using ConsoleUI;
using ConsoleUI.Structs;
using ConsoleUI.Event;

namespace GLC
{
    /// <summary>
    /// Manage groups of elements and controls
    /// Handle events and dispatch them to active controls
    /// </summary>
    public class CAppWindow : CWindow
    {
        public string Command { private get; set; }

        private CMinibuffer m_minibuffer;
        private bool        m_isMinibufferFocused;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">Window title</param>
        /// <param name="rect">Size of the window/buffer</param>
        /// <param name="defaultColours">Default background and foreground colour pair</param>
        public CAppWindow(string title, ConsoleRect rect, ColourTheme colours, int pageCount) : base(title, rect, colours, pageCount)
        {
            ConsoleRect minibufferRect = new ConsoleRect(0, rect.height - 2, rect.width, 2);
            m_minibuffer          = new CMinibuffer(this, minibufferRect);
            m_isMinibufferFocused = true;
        }

        /// <summary>
        /// Initialise console window and buffer
        /// </summary>
        public override void Initialise()
        {
            m_minibuffer.Initialise();

            // Initialise all pages
            m_pages[0] = new CLibraryPage(this, m_area, "Library", 3);
            m_pages[0].Initialise(new PanelTypeCode[] { PanelType.PLATFORM_PANEL, PanelType.GAME_LIST_PANEL, PanelType.GAME_INFO_PANEL });
        }

        /// <summary>
        /// Main event loop for the window class
        /// Get input, despatch to relevant control element and redraw the element
        /// </summary>
        public void WindowMain()
        {
            while(true)
            {
                Console.CursorVisible  = m_isMinibufferFocused;
                CElement focused       = (m_isMinibufferFocused) ? m_minibuffer : m_pages[m_activePageIndex];
                if(m_isMinibufferFocused && Console.CursorVisible)
                {
                    m_minibuffer.SetCursor();
                }
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                switch(keyInfo.Key)
                {
                    case ConsoleKey.Escape:
                    {
                        // Switch focus from microbuffer into the active page
                        m_isMinibufferFocused = false;
                        m_minibuffer.ClearBuffer();
                    }
                    break;

                    case ConsoleKey.Oem1: // Shift + semicolon
                    {
                        // Switch focus from active page into the microbuffer
                        m_isMinibufferFocused = true;
                        m_minibuffer.KeyPressed(keyInfo);
                    }
                    break;

                    default:
                    {
                        if(m_isMinibufferFocused)
                        {
                            m_minibuffer.KeyPressed(keyInfo);
                            m_minibuffer.Redraw();
                        }
                        else
                        {
                            m_pages[m_activePageIndex].KeyPressed(keyInfo);
                            m_pages[m_activePageIndex].Redraw(false);
                        }
                    }
                    break;
                }
            }
        }

        public override void OnCommand(object sender, GenericEventArgs<string> e)
        {
            throw new System.NotImplementedException();
        }

        public override void OnResize(object sender, ResizeEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
