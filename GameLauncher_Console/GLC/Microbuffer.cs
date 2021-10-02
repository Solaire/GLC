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
            m_rect.x = 0;
            m_rect.y = parent.m_rect.height - 2;
            m_rect.width = parent.m_rect.width;
            m_rect.height = 2;
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

            if(statusCpy.Length > m_rect.width - 2) // 2 char padding
            {
                statusCpy = statusCpy.Remove(statusCpy.Length - m_rect.width - 5); // 2 char padding + space for an Ellipsis
                statusCpy += "...";
            }
            //Console.Write(statusCpy.PadRight(m_rect.width));
            CConsoleEx.WriteText(statusCpy, m_rect.x, m_rect.y, CConstants.TEXT_PADDING_LEFT, m_rect.width, m_parent.m_colours[ColourThemeIndex.cStatusBg], m_parent.m_colours[ColourThemeIndex.cStatusFg]);
        }

        public void DrawBuffer()
        {
            Console.CursorVisible = false;
            string bufferCpy      = m_inputBuffer.ToString();

            if (bufferCpy.Length > m_rect.width - 1) // 1 char padding
            {
                bufferCpy = bufferCpy.Remove(bufferCpy.Length - m_rect.width - 1); // 1 char padding
            }

            //Console.Write(bufferCpy.PadRight(m_rect.width));

            CConsoleEx.WriteText(bufferCpy, m_rect.x, m_rect.y + 1, CConstants.TEXT_PADDING_LEFT, m_rect.width, m_parent.m_colours[ColourThemeIndex.cDefaultBg], m_parent.m_colours[ColourThemeIndex.cDefaultFg]);

            Console.CursorVisible = true;
            Console.CursorLeft = m_inputPosition;
        }

        public void SetCursor()
        {
            Console.CursorLeft = m_inputPosition + CConstants.TEXT_PADDING_LEFT;
            Console.CursorTop = m_rect.y + 1;
        }

        public void AddInput(char input)
        {
            if(char.IsControl(input) || m_inputBuffer.Length == m_rect.width - 2)
            {
                return;
            }

            Console.CursorVisible = true;

            string before = m_inputBuffer.Substring(0, m_inputPosition);
            string after  = m_inputBuffer.Substring(m_inputPosition, m_inputBuffer.Length - m_inputPosition);
            m_inputBuffer = before + input + after;
            m_inputPosition++;
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
    }
}
