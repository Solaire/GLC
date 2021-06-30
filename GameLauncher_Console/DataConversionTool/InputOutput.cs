using GameLauncher_Console;
using Logger;
using System;
using System.Collections.Generic;

namespace DataConversionTool
{
    public static class CInputOutput
    {
        // Command-line option const
        private const string FLAG_APPLY = "-apply";
        private const string FLAG_VERIFY = "-verify";

        private static readonly string[] HELP =
        {
        //  0|-------|---------|---------|---------|---------|---------|---------|---------|80
            "This conversion tool will migrate the game data from JSON to SQLite database",
            "Command line parameters:",
            "-verify: Load, verify and display game data. No migration will happen.",
            "-apply: Perform the same checks as 'verify' and also migrate data to the DB",
        };

        /// <summary>
        /// Determine tool mode from input string
        /// </summary>
        /// <param name="mode">Input string</param>
        /// <returns>ConvertMode enum</returns>
        public static CConverter.ConvertMode DetermineMode(string mode)
        {
            switch(mode)
            {
                case FLAG_VERIFY:
                    Log("Running in VERIFY mode");
                    return CConverter.ConvertMode.cModeVerify;
                        
                case FLAG_APPLY:
                    Log("Running in APPLY mode");
                    return CConverter.ConvertMode.cModeApply;

                default:
                    Log("WARN: Could not determine mode");
                    return CConverter.ConvertMode.cModeUnknown;
            }
        }

        /// <summary>
        /// Write help array to donsole
        /// </summary>
        public static void ShowHelp()
        {
            Console.Clear();
            foreach(string s in HELP)
            {
                Console.WriteLine(s);
            }
        }

        /// <summary>
        /// Write message to console and log file
        /// </summary>
        /// <param name="message">The message</param>
        public static void Log(string message)
        {
            Console.WriteLine(message);
            CLogger.LogInfo(message);
        }

        /// <summary>
        /// Log game and platform statistics
        /// </summary>
        /// <param name="games"></param>
        public static void LogGameData(Dictionary<string, int> platforms, HashSet<CGameData.CGame> games)
        {
            // Log the platforms
            Log(string.Format("Found {0} platforms and {1} games:", platforms.Keys.Count, platforms.Values.Count));
            foreach(KeyValuePair<string, int> platform in platforms)
            {
                Log(string.Format("Platform: {0} -> {1} games", platform.Key, platform.Value));
            }
            Log("");
            // Log the actual games
            foreach(CGameData.CGame game in games)
            {
                Log(string.Format("ID: {0}, Title: {1}, Platform: {2}", game.ID, game.Title, game.PlatformString));
                Log(string.Format("Alias: {0}, Frequency: {1}", game.Alias, game.Frequency));
                Log(string.Format("Launch: {0}, Uninstall: {1}, Icon {2}", game.Launch, game.Uninstaller, game.Icon));
                Log(string.Format("Installed: {0}, Favourite: {1}, Hidden: {2}, New: {3}", game.IsInstalled, game.IsFavourite, game.IsHidden, game.IsNew));
                Log("");
            }
        }
    }
}
