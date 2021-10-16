using ConsoleUI.Structs;

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
        public static readonly PanelData[] PANEL_DATA =
        {
            new PanelData(PanelType.PLATFORM_PANEL,  "Platforms", 50, 100),
            new PanelData(PanelType.GAME_LIST_PANEL, "Games",     50, 70),
            new PanelData(PanelType.GAME_INFO_PANEL, "GameInfo",  50, 30),
        };

        private static readonly System.ConsoleColor[] DEFAULT_COLOUR_ARRAY =
        {
            // Default background/foreground
            System.ConsoleColor.Black,
            System.ConsoleColor.White,
            // Status background/foreground
            System.ConsoleColor.White,
            System.ConsoleColor.Black,
            // Panel border background/foreground
            System.ConsoleColor.DarkGreen,
            System.ConsoleColor.White,
            // Panel mian background/foreground
            System.ConsoleColor.Black,
            System.ConsoleColor.White,
            // Panel highlight background/foreground
            System.ConsoleColor.DarkGray,
            System.ConsoleColor.Red,
            // Focused panel highlight background/foreground
            System.ConsoleColor.Blue,
            System.ConsoleColor.Red,
        };

        public static readonly ColourTheme DEFAULT_THEME = new ColourTheme(DEFAULT_COLOUR_ARRAY);
    }
}
