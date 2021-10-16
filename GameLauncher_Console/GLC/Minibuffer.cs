using System;
using ConsoleUI;
using ConsoleUI.Event;
using ConsoleUI.Structs;

namespace GLC
{
    /// <summary>
    /// A control containing a status bar and a user input line.
    /// </summary>
    public class CMinibuffer : CElement, IInteractive
    {
        private CWindow m_parent;
        private string  m_status;
        private string  m_inputBuffer;
        private int     m_inputPosition;

        public CMinibuffer(CWindow parent, ConsoleRect area) : base(area)
        {
            m_parent        = parent;
            m_status        = "";
            m_inputBuffer   = "";
            m_inputPosition = 0;

            /*
            // Buffer will cover bottom 2 lines
            m_area.x = 0;
            m_area.y = parent.m_rect.height - 2;
            m_area.width = parent.m_rect.width;
            m_area.height = 2;
            */
        }

        public override void Initialise()
        {
            Redraw();
        }

        public void SetStatus(string status)
        {
            m_status = status;
            DrawStatus();
        }

        public void ClearBuffer()
        {
            m_inputBuffer   = "";
            m_inputPosition = 0;
        }

        public void SetCursor()
        {
            Console.CursorLeft = m_inputPosition + CConstants.TEXT_PADDING_LEFT;
            Console.CursorTop = m_area.y + 1;
        }


        private void DrawStatus()
        {
            Console.CursorVisible = false;
            string statusCpy      = m_status;

            if(statusCpy.Length > m_area.width - 2) // 2 char padding
            {
                statusCpy = statusCpy.Remove(statusCpy.Length - m_area.width - 5); // 2 char padding + space for an Ellipsis
                statusCpy += "...";
            }
            //Console.Write(statusCpy.PadRight(m_rect.width));
            CConsoleEx.WriteText(statusCpy, m_area.x, m_area.y, CConstants.TEXT_PADDING_LEFT, m_area.width, m_parent.m_colours[ColourThemeIndex.cStatusBG], m_parent.m_colours[ColourThemeIndex.cStatusFG]);
        }

        private void DrawBuffer()
        {
            Console.CursorVisible = false;
            string bufferCpy      = m_inputBuffer.ToString();

            if (bufferCpy.Length > m_area.width - 1) // 1 char padding
            {
                bufferCpy = bufferCpy.Remove(bufferCpy.Length - m_area.width - 1); // 1 char padding
            }

            //Console.Write(bufferCpy.PadRight(m_rect.width));

            CConsoleEx.WriteText(bufferCpy, m_area.x, m_area.y + 1, CConstants.TEXT_PADDING_LEFT, m_area.width, m_parent.m_colours[ColourThemeIndex.cDefaultBG], m_parent.m_colours[ColourThemeIndex.cDefaultFG]);

            Console.CursorVisible = true;
            Console.CursorLeft = m_inputPosition;
        }

        private void AddInput(ConsoleKeyInfo key)
        {
            if(key.Key == ConsoleKey.Delete ||
                key.Key == ConsoleKey.Backspace)
            {
                // Allow
            }
            else if(char.IsControl(key.KeyChar) || m_inputBuffer.Length == m_area.width - 2)
            {
                return; // Cannot add anything
            }

            Console.CursorVisible = true;
            string before = m_inputBuffer.Substring(0, m_inputPosition);
            string after  = m_inputBuffer.Substring(m_inputPosition, m_inputBuffer.Length - m_inputPosition);

            if(key.Key == ConsoleKey.Delete)
            {
                // Delete first character from the buffer after the cursor
                int max = (after.Length == 0) ? 0 : 1;
                m_inputBuffer = before + after.Substring(max);
            }
            else if(key.Key == ConsoleKey.Backspace)
            {
                // Delete the last character from the buffer before the cursor and move cursor left
                int max = (before.Length == 0) ? 0 : before.Length - 1;
                m_inputBuffer = before.Substring(0, max) + after;
                m_inputPosition = (m_inputPosition - 1 < 0) ? 0 : m_inputPosition - 1;
            }
            else
            {
                // Default. Add input to string and move cursor to the right
                char input      = key.KeyChar;
                m_inputBuffer = before + input + after;
                m_inputPosition = (m_inputPosition + 1 > m_inputBuffer.Length) ? m_inputBuffer.Length : m_inputPosition + 1;
            }

            DrawBuffer();
        }

        /// <summary>
        /// Redraw the microbuffer and status texts
        /// </summary>
        public void Redraw()
        {
            DrawStatus();
            DrawBuffer();
        }

        public override void OnResize(object sender, ResizeEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void KeyPressed(ConsoleKeyInfo keyInfo)
        {
            switch(keyInfo.Key)
            {
                case ConsoleKey.LeftArrow:
                case ConsoleKey.RightArrow:
                {
                    MoveCursor((keyInfo.Key == ConsoleKey.LeftArrow) ? -1 : 1);
                }
                break;

                case ConsoleKey.Enter:
                {
                    OnEnter();
                }
                break;

                default:
                {
                    AddInput(keyInfo);
                }
                break;
            }
        }

        /// <summary>
        /// Submit the command from the microbuffer
        /// </summary>
        private void OnEnter()
        {
            // Despatch the command back to the parent to deal with
            if(m_parent != null)
            {
                // TODO:
                //m_parent.Command = m_inputBuffer;
            }

            // TODO: Temporary
            SetStatus(m_inputBuffer);
            m_inputBuffer = "";
            m_inputPosition = 0;
        }

        /// <summary>
        /// Move buffer carted by x spaces left or right
        /// </summary>
        /// <param name="delta">Movement delta, positive is right, negative is left</param>
        private void MoveCursor(int delta)
        {
            /*
            if(delta > 0) // Move x spaces right
            {
                m_inputPosition = (m_inputPosition + delta > m_inputBuffer.Length) ? m_inputBuffer.Length : m_inputPosition + delta;
            }
            else if(delta < 0) // Move x spaces left
            {
                m_inputPosition = (m_inputPosition - delta < 0) ? 0 : m_inputPosition - delta;
            }
            */

            if(delta != 0)
            {
                m_inputPosition = Math.Min(m_inputBuffer.Length, Math.Max(0, m_inputPosition + delta));
            }
        }
    }
}
