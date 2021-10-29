using System;
using ConsoleUI.Type;
using ConsoleUI.Helper;

namespace GLC
{
    /// <summary>
    /// A control containing a status bar and a user input line.
    /// </summary>
    public class CMinibuffer
    {
        private ConsoleRect m_rect;

        private string  m_statusBuffer;
        private string  m_inputBuffer;
        private int     m_inputPosition;

        public CMinibuffer(ConsoleRect rect)
        {
            m_rect          = rect;
            m_statusBuffer  = "";
            m_inputBuffer   = "";
            m_inputPosition = 0;
        }

        public void Initialise()
        {
            DrawBuffer();
            DrawStatus();
        }

        public void SetStatus(string status)
        {
            m_statusBuffer = status;
            DrawStatus();
        }

        public void ClearBuffer()
        {
            m_inputBuffer   = "";
            m_inputPosition = 0;
            DrawBuffer();
        }

        public void SetCursor()
        {
            Console.CursorLeft = m_inputPosition + CConstants.TEXT_PADDING_LEFT;
            Console.CursorTop  = m_rect.y + 1;
        }

        private void DrawStatus()
        {
            Console.CursorVisible = false;
            string statusCpy      = m_statusBuffer;

            if(statusCpy.Length > m_rect.width - 2) // 2 char padding
            {
                statusCpy = statusCpy.Remove(statusCpy.Length - m_rect.width - 5); // 2 char padding + space for an Ellipsis
                statusCpy += "...";
            }
            CConsoleDraw.WriteText(statusCpy.PadRight(m_rect.width), m_rect.x, m_rect.y, ConsoleColor.Black, ConsoleColor.White);
        }

        private void DrawBuffer()
        {
            Console.CursorVisible = false;
            string bufferCpy      = m_inputBuffer.ToString();

            if (bufferCpy.Length > m_rect.width - 1) // 1 char padding
            {
                bufferCpy = bufferCpy.Remove(bufferCpy.Length - m_rect.width - 1); // 1 char padding
            }

            //Console.Write(bufferCpy.PadRight(m_rect.width));

            CConsoleDraw.WriteText(bufferCpy.PadRight(m_rect.width), m_rect.x, m_rect.y + 1, ConsoleColor.White, ConsoleColor.Black);

            Console.CursorVisible = true;
            Console.CursorLeft    = m_inputPosition;
        }

        private void AddInput(ConsoleKeyInfo key)
        {
            if(key.Key == ConsoleKey.Delete ||
                key.Key == ConsoleKey.Backspace)
            {
                // Allow
            }
            else if(char.IsControl(key.KeyChar) || m_inputBuffer.Length == m_rect.width - 2)
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
            if(delta != 0)
            {
                m_inputPosition = Math.Min(m_inputBuffer.Length, Math.Max(0, m_inputPosition + delta));
            }
        }
    }
}
