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

        // Initialise the panel array and each individual panel
        public static readonly PanelData[] PANEL_DATA =
        {
            new PanelData(PanelType.cPanel_Platforms, "Platforms",    50, 50),
            new PanelData(PanelType.cPanel_Games,     "Games",        50, 100),
            new PanelData(PanelType.cPanel_GameInfo,  "GameInfo",     50, 100),
            new PanelData(PanelType.cPanel_KeyConfig, "Key Bindings", 50, 100),
        };
    }
}
