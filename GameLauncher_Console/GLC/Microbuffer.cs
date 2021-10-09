using System;
using GLC_Structs;

namespace GLC
{
    /// <summary>
    /// A control containing a status bar and a user input line.
    /// </summary>
    public class CMicrobuffer : CControl
    {
        private CWindow m_parent;
        private string m_status;
        private string m_inputBuffer;
        private int m_inputPosition;

        public CMicrobuffer(CWindow parent)
        {
            m_parent        = parent;
            m_status        = "";
            m_inputBuffer   = "";
            m_inputPosition = 0;

            // Buffer will cover bottom 2 lines
            m_area.x = 0;
            m_area.y = parent.m_rect.height - 2;
            m_area.width = parent.m_rect.width;
            m_area.height = 2;
        }

        public void Initialise()
        {
            Redraw(true);
        }

        public void SetStatus(string status)
        {
            m_status = status;
            DrawStatus();
        }

        public void DrawStatus()
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

        public void DrawBuffer()
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

        public void SetCursor()
        {
            Console.CursorLeft = m_inputPosition + CConstants.TEXT_PADDING_LEFT;
            Console.CursorTop = m_area.y + 1;
        }

        public void AddInput(ConsoleKeyInfo key)
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
                m_inputBuffer   = before + input + after;
                m_inputPosition = (m_inputPosition + 1 > m_inputBuffer.Length) ? m_inputBuffer.Length : m_inputPosition + 1;
            }

            DrawBuffer();
        }

        /// <summary>
        /// Redraw the microbuffer and status texts
        /// </summary>
        public override void Redraw(bool fullRedraw)
        {
            DrawStatus();
            DrawBuffer();
        }

        /// <summary>
        /// Submit the command from the microbuffer
        /// </summary>
        public override void OnEnter()
        {
            // Despatch the command back to the parent to deal with
            if(m_parent != null)
            {
                m_parent.Command = m_inputBuffer;
            }
            
            // TODO: Temporary
            SetStatus(m_inputBuffer);
            m_inputBuffer = "";
            m_inputPosition = 0;
        }

        /// <summary>
        /// Select previous command
        /// </summary>
        public override void OnUpArrow()
        {
            throw new NotImplementedException("CMicrobuffer.OnUpArrow() not yet implemented");
        }

        /// <summary>
        /// Select next command
        /// </summary>
        public override void OnDownArrow()
        {
            throw new NotImplementedException("CMicrobuffer.OnDownArrow() not yet implemented");
        }

        /// <summary>
        /// Move cartet left by 1 space
        /// </summary>
        public override void OnLeftArrow()
        {
            m_inputPosition = (m_inputPosition - 1 < 0) ? 0 : m_inputPosition - 1;
        }

        /// <summary>
        /// Move the cartet right by 1 space
        /// </summary>
        public override void OnRightArrow()
        {
            m_inputPosition = (m_inputPosition + 1 > m_inputBuffer.Length) ? m_inputBuffer.Length : m_inputPosition + 1;
        }

        public override void OnTab()
        {
            throw new NotImplementedException("CMicrobuffer.OnTab() not yet implemented");
        }
    }
}
