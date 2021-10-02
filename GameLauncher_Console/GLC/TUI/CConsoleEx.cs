using System;
using GLC_Structs;

namespace GLC
{
    /// <summary>
    /// Manages the size of the window and buffer
    /// Contains shared helper functions for drawing
    /// </summary>
    public static class CConsoleEx
    {
        /// <summary>
        /// Initialise console window
        /// </summary>
        public static void InitialiseWindow(int width, int height, string title)
        {
            Console.Title = title;
            CConsoleEx.UpdateWindowSize(width, height);
        }

        /// <summary>
        /// Draw rectangle
        /// </summary>
        /// <param name="rect">The rectangle</param>
        /// <param name="colour">Backgound colour</param>
        public static void DrawColourRect(ConsoleRect rect, ConsoleColor colour)
        {
            Console.BackgroundColor = colour;

            // Draw row by row
            for(int row = rect.y; row < rect.y + rect.height; row++)
            {
                Console.CursorLeft = rect.x;
                Console.CursorTop  = row;

                Console.WriteLine("".PadLeft(rect.width));
            }
        }

        public static void DrawHorizontalLine(int x, int y, int width, ConsoleColor colourBg, ConsoleColor colourFg)
        {
            Console.BackgroundColor = colourBg;
            Console.ForegroundColor = colourFg;

            for(int col = x; col < x + width; col++)
            {
                Console.CursorLeft = col;
                Console.CursorTop  = y;

                Console.Write("-");
            }
        }

        public static void DrawVerticalLine(int x, int y, int height, ConsoleColor colourBg, ConsoleColor colourFg)
        {
            Console.BackgroundColor = colourBg;
            Console.ForegroundColor = colourFg;

            for(int row = y; row < y + height; row++)
            {
                Console.CursorLeft = x;
                Console.CursorTop  = row;

                Console.Write("|");
            }
        }
        
        /// <summary>
        /// Write text to console.
        /// </summary>
        /// <param name="text">The string</param>
        /// <param name="x">Console buffer column position</param>
        /// <param name="y">Console buffer row position</param>
        /// <param name="colourPair">Pair of backgound and foreground colours</param>
        public static void WriteText(string text, int x, int y, int padLeft, int padRight, ConsoleColor colourBg, ConsoleColor colourFg)
        {
            // TODO: check if x and y are within the buffer area
            if((x < 0 || x > Console.BufferWidth) || (y < 0 || y > Console.BufferHeight))
            {
                return;
            }

            Console.CursorVisible = false;

            Console.CursorLeft = x;
            Console.CursorTop  = y;

            Console.BackgroundColor = colourBg;
            Console.ForegroundColor = colourFg;

            text = text.PadLeft(text.Length + padLeft);
            text = text.PadRight(padRight);

            Console.Write(text);
        }

        /*
        private static void SetupWindow()
        {
            m_windowRect.height = Console.BufferHeight;

            int whereToMove = Console.CursorTop + 1; // One line below the visible window height
            if(whereToMove < Console.WindowHeight)
                whereToMove = Console.WindowHeight + 1;

            if(Console.BufferHeight < whereToMove + Console.WindowHeight) // Fix buffer size
                Console.BufferHeight = whereToMove + Console.WindowHeight;

            Console.MoveBufferArea(0, 0, Console.WindowWidth, Console.WindowHeight, 0, whereToMove);

            Console.CursorVisible = false;
            m_windowRect.x = Console.CursorTop;
            m_windowRect.y = Console.CursorLeft;
            m_defaultColours.foreground = Console.ForegroundColor;
            m_defaultColours.background = Console.BackgroundColor;

            Console.CursorTop  = 0;
            Console.CursorLeft = 0;
        }

        public static void EndWindow()
        {
            Console.ForegroundColor = m_defaultColours.foreground;
            Console.BackgroundColor = m_defaultColours.background;

            int whereToGet = m_windowRect.x; // One line below the visible window height
            if(whereToGet < Console.WindowHeight)
                whereToGet = Console.WindowHeight + 1;

            Console.MoveBufferArea(0, whereToGet, Console.WindowWidth, Console.WindowHeight, 0, 0);

            Console.CursorTop  = m_windowRect.x;
            Console.CursorLeft = m_windowRect.y;

            Console.CursorVisible = true;
            Console.BufferHeight  = m_windowRect.height;
        }
        */

        /// <summary>
        /// Update console window and buffer size and flush the buffer
        /// </summary>
        /// <param name="width">New window/buffer width</param>
        /// <param name="height">New window/buffer height</param>
        private static void UpdateWindowSize(int width, int height)
        {
            Console.CursorVisible = false;
            
            if(width > Console.BufferWidth)
            {
                Console.BufferWidth = width;
                Console.WindowWidth = width;
            }
            else
            {
                Console.WindowWidth = width;
                Console.BufferWidth = width;
            }

            if(height > Console.BufferHeight)
            {
                Console.BufferHeight = height;
                Console.WindowHeight = height;
            }
            else
            {
                Console.WindowHeight = height;
                Console.BufferHeight = height;
            }
            
            ConsoleRect rect;
            rect.x      = 0;
            rect.y      = 0;
            rect.width  = width;
            rect.height = height;
            Console.BackgroundColor = ConsoleColor.Gray;

            DrawColourRect(rect, Console.BackgroundColor); // Flush buffer
        }
    }
}
