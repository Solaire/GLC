using System;
using GLC_Structs;

namespace GLC
{
    /// <summary>
    /// Manage groups of elements and controls
    /// Handle events and dispatch them to active controls
    /// </summary>
    public class CWindow
    {
        /*
            A window does not contain controls by itself. Instead it will delegate anything to pages
            The only real control that is present in the window is the microbuffer
        */
        private string      m_title;
        public  ConsoleRect m_rect { get; private set; }

        public  ColourPair  m_defaultColours { get; private set; }

        public string Command { private get; set; }

        private int     m_currentPage;
        private CPage[] m_pages;
        private CMicrobuffer m_microbuffer;
        private bool    m_microbufferFocus;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">Window title</param>
        /// <param name="rect">Size of the window/buffer</param>
        /// <param name="defaultColours">Default background and foreground colour pair</param>
        public CWindow(string title, ConsoleRect rect, ColourPair defaultColours)
        {
            m_title          = title;
            m_rect           = rect;
            m_defaultColours = defaultColours;

            m_currentPage    = 0;
            m_pages = new CPage[2];

            m_microbuffer = new CMicrobuffer(this);
            m_microbufferFocus = true;

            Command = "";
        }

        /// <summary>
        /// Initialise console window and buffer
        /// </summary>
        public void Initialise()
        {
            // Initialise window and the microbuffer
            CConsoleEx.InitialiseWindow(m_rect.width, m_rect.height, m_title);
            m_microbuffer.Initialise();

            // Initialise all pages
            m_pages[0] = new CPage(this, "Library");
            m_pages[0].Initialise(new PanelType[] { PanelType.cPanel_Platforms, PanelType.cPanel_Games });
        }

        /// <summary>
        /// Main event loop for the window class
        /// Get input, despatch to relevant control element and redraw the element
        /// </summary>
        public void WindowMain()
        {
            while(true)
            {
                Console.CursorVisible  = m_microbufferFocus;
                CControl focused       = (m_microbufferFocus) ? m_microbuffer : m_pages[m_currentPage];
                if(m_microbufferFocus && Console.CursorVisible)
                {
                    m_microbuffer.SetCursor();
                }
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                switch(keyInfo.Key)
                {
                    case ConsoleKey.Escape:
                    {
                        // Switch focus from microbuffer into the active page
                        m_microbufferFocus = false;
                    }
                    break;

                    case ConsoleKey.Oem1: // Shift + semicolon
                    {
                        // Switch focus from active page into the microbuffer
                        m_microbufferFocus = true;
                        m_microbuffer.AddInput(keyInfo.KeyChar);
                    }
                    break;

                    case ConsoleKey.Enter:
                    {
                        // Submit the microbuffer command or whatever TUI function
                        focused.OnEnter();
                    }
                    break;

                    case ConsoleKey.UpArrow:
                    {
                        // Previous command or a TUI function
                        focused.OnUpArrow();
                    }
                    break;

                    case ConsoleKey.DownArrow:
                    {
                        // Next command or a TUI function
                        focused.OnDownArrow();
                    }
                    break;

                    case ConsoleKey.LeftArrow:
                    {
                        // Move cartet left or a TUI function
                        focused.OnLeftArrow();
                    }
                    break;

                    case ConsoleKey.RightArrow:
                    {
                        // Move cartet right or a TUI function
                        focused.OnRightArrow();
                    }
                    break;

                    default:
                    {
                        if(m_microbufferFocus)
                        {
                            m_microbuffer.AddInput(keyInfo.KeyChar);
                        }
                    }
                    break;
                }
                if(Command.Length != 0) // Parse and execute command
                {
                    HandleCommand();
                }
                focused.Redraw(false);
            }
        }

        private void HandleCommand()
        {
            // TODO: implement

            Command = "";
        }

        /*
        public void MicrobufferTest()
        {
            while(true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(false);
                switch(keyInfo.Key)
                {
                    case ConsoleKey.Escape: // Exit
                        m_currentPage = 0;
                        return;

                    case ConsoleKey.Enter:
                        m_microbuffer.StatusFromInput();
                        break;

                    case ConsoleKey.LeftArrow:
                        m_microbuffer.MoveLeft();
                        break;

                    case ConsoleKey.RightArrow:
                        m_microbuffer.MoveRight();
                        break;

                    default:
                        m_microbuffer.AddInput(keyInfo.KeyChar);
                        break;
                }
                m_microbuffer.Redraw();
            }
        }
        */
    }
}
