/// <summary>
/// Namespace containing various structures
/// </summary>
namespace GLC_Structs
{
    /// <summary>
    /// A rectangle structure used for drawing and creating areas
    /// </summary>
    public struct ConsoleRect
    {
        public int x;
        public int y;
        public int width;
        public int height;

        public int Right
        {
            get
            {
                return x + width;
            }
        }

        public int Bottom
        {
            get
            {
                return y + height;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ConsoleRect(int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            width  = w;
            height = h;
        }
    }

    /// <summary>
    /// Contains pair of colours for background and foreground
    /// </summary>
    public struct ColourPair
    {
        public System.ConsoleColor foreground;
        public System.ConsoleColor background;

        /// <summary>
        /// Constructor
        /// </summary>
        public ColourPair(System.ConsoleColor foreground, System.ConsoleColor backgound)
        {
            this.foreground = foreground;
            this.background = backgound;
        }
    }

    /// <summary>
    /// Struct holding information about given TUI panels
    /// </summary>
    public struct PanelData
    {
        public PanelType   type;
        public string      title;
        public int         percentWidth;
        public int         percentHeight;

        public PanelData(PanelType type, string title, int percentWidth, int percentHeight)
        {
            this.type = type;
            this.title = title;
            this.percentWidth = percentWidth;
            this.percentHeight = percentHeight;
        }
    }

    public enum PanelType
    {
        cPanel_Platforms = 0,
        cPanel_Games     = 1,
        cPanel_GameInfo  = 2,
        cPanel_KeyConfig = 3,

        // NOTE:
        // cPanel_END is the total count of panels
        // Ensure this enum is at the end and its value is correct
        cPanel_END, // = 4
    }
}
