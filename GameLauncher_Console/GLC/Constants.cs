using ConsoleUI.Base;

namespace GLC
{
    /// <summary>
    /// Static class for storing constant variables and/or structures
    /// </summary>
    public static class CConstants
    {
        public static int DEFAULT_WINDOW_WIDTH  = 100;
        public static int DEFAULT_WINDOW_HEIGHT = 48;

        public static int TEXT_PADDING_LEFT  = 1;
        public static int TEXT_PADDING_RIGHT = 1;

        // Initialise the panel array and each individual panel
        public static readonly ViewInitData[] PANEL_DATA =
        {
            new ViewInitData("Platforms", 50, 100),
            new ViewInitData("Games",     50, 70),
            new ViewInitData("GameInfo",  50, 30),
        };
    }
}
