using GLC_Structs;

namespace GLC
{
    /// <summary>
    /// Static class for storing constant variables and/or structures
    /// </summary>
    public static class CConstants
    {
        public static int DEFAULT_WINDOW_WIDTH  = 60;
        public static int DEFAULT_WINDOW_HEIGHT = 100;

        public static int TEXT_PADDING_LEFT  = 1;
        public static int TEXT_PADDING_RIGHT = 1;

        // Initialise the panel array and each individual panel
        public static readonly PanelData[] PANEL_DATA =
        {
            new PanelData(PanelType.cPlatforms, "Platforms",    50, 100),
            new PanelData(PanelType.cGames,     "Games",        50, 100),
            new PanelData(PanelType.cGameInfo,  "GameInfo",     50, 100),
            new PanelData(PanelType.cKeyConfig, "Key Bindings", 50, 100),
        };

        private static readonly System.ConsoleColor[] DEFAULT_COLOUR_ARRAY =
        {
            System.ConsoleColor.Black, // Default background
            System.ConsoleColor.White, // Default foreground

            System.ConsoleColor.White, // Status background
            System.ConsoleColor.Black, // Status foreground
            
            System.ConsoleColor.DarkGreen, // Panel border background
            System.ConsoleColor.White,     // Panel border foreground

            System.ConsoleColor.Black, // Panel main background
            System.ConsoleColor.White, // Panel main foreground

            System.ConsoleColor.DarkBlue, // Panel highlight background
            System.ConsoleColor.Red,      // Panel highlight foreground

            System.ConsoleColor.Blue, // Panel highlight focus background
            System.ConsoleColor.Red, // Panel highlight focus foreground
        };

        public static readonly ColourTheme DEFAULT_THEME = new ColourTheme(DEFAULT_COLOUR_ARRAY);
    }
}
