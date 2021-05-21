using GameLauncher_Console;
using SqlDB;
using System.Collections.Generic;
using static CGame_Test.CGame;

namespace DataConversionTool
{
    public class CConverter
    {
        /// <summary>
        /// Enum for convertion mode
        /// </summary>
        public enum ConvertMode
        {
            cModeUnknown = 0x00,
            cModeVerify  = 0x01,
            cModeApply   = 0x02,
        }

        private readonly ConvertMode Mode;

        public CConverter(ConvertMode mode)
        {
            Mode = mode;
        }

        /// <summary>
        /// Begin the data conversion
        /// </summary>
        /// <returns>True if in 'verify' mode or on conversion success</returns>
        public bool ConvertData()
        {
            // TODO: Open database connection
            // TODO: Load games and put into a GameSets (platform to game objects)
            // TODO: Display platform count, games per platform and each game data
            // TODO: Log the data as well
            // TODO: Save the games to database (show the counter too while doing it)
            // TODO: Load the games from database (to a new collection) and compare against the original
            // TODO: Is success, print success - otherwise remove data from DB table (rollback?) and print failure
            bool success = true;
            if (Mode == ConvertMode.cModeApply)
            {
                success = CSqlDB.Instance.Open(true) == System.Data.SQLite.SQLiteErrorCode.Ok;
                if (!success)
                {
                    CInputOutput.Log("ERROR: Could not open or create the database.");
                    return success;
                }
            }
            // Load games from JSON
            success = CJsonWrapper.ImportFromJSON(out CConfig.Configuration config, out CConfig.Hotkeys keys, out CConfig.Colours cols, out List<CGameData.CMatch> matches);
            if(!success)
            {
                CInputOutput.Log("ERROR: Could not load games from the JSON file");
                return success;
            }
            Dictionary<string, int> platforms = CGameData.GetPlatforms();
            HashSet<CGameData.CGame> allGames = CGameData.GetPlatformGameList(CGameData.GamePlatform.All);
            CInputOutput.LogGameData(platforms, allGames);

            if(Mode == ConvertMode.cModeApply)
            {
                // Add platforms to the database


                // Add the games to the database
                List<GameObject> gamesDB = new List<GameObject>();
                foreach(CGameData.CGame game in allGames)
                {
                    gamesDB.Add(new GameObject());
                }
            }
            else
            {
                CInputOutput.Log("Mode is 'verify' - no changes have been made");
                CInputOutput.Log("In order to perform conversion, run the program in 'apply' mode");
                CInputOutput.Log("**** END ****");
            }

            return success;
        }

        private Dictionary<string, int> AddPlatformsToDB(List<string> platforms)
        {
            Dictionary<string, int> platformsDB = new Dictionary<string, int>();

            // TODO;

            return platformsDB;
        }
    }
}
