using CGame_Test;
using GameLauncher_Console;
using SqlDB;
using System.Collections.Generic;
using System.Data.SQLite;
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
        /// Convert platform and game data from JSON file into the database.
        /// NOTE: Any existing data in the Game and Platform tables will be deleted
        /// </summary>
        /// <returns>True if in 'verify' mode or on conversion success, false on any error during verification/conversion</returns>
        public bool ConvertData()
        {
            bool success = true;

            // Open/create database and clear the Game/Platform tables
            if (Mode == ConvertMode.cModeApply) 
            {
                success = CSqlDB.Instance.Open(true) == SQLiteErrorCode.Ok;
                if(!success)
                {
                    CInputOutput.Log("ERROR: Could not open or create the database.");
                    return success;
                }
                success = CSqlDB.Instance.Execute("DELETE FROM Game; DELETE FROM Platform") == SQLiteErrorCode.Ok; // Delete existing platforms and games
                if(!success)
                {
                    CInputOutput.Log("ERROR: Could not prepare the Platform and Game tables");
                    return success;
                }
            }

            // Load and log all platforms and games from JSON
            success = CJsonWrapper.ImportFromJSON(out List<CGameData.CMatch> matches);
            if(!success)
            {
                CInputOutput.Log("ERROR: Could not load games from the JSON file");
                return success;
            }
            Dictionary<string, int> jsonPlatforms = CGameData.GetPlatforms();
            HashSet<CGameData.CGame> jsonAllGames = CGameData.GetPlatformGameList(CGameData.GamePlatform.All);
            CInputOutput.LogGameData(jsonPlatforms, jsonAllGames);

            // Begin migration here
            if(Mode == ConvertMode.cModeApply)
            {
                // Add platforms to the database
                foreach(KeyValuePair<string, int> platform in jsonPlatforms)
                {
                    if(platform.Key != "All games" 
                        && platform.Key != "Search results" 
                        && platform.Key != "Favourites"
                        && platform.Key != "New games"
                        && platform.Key != "Hidden games"
                        && platform.Key != "Not installed")
                    {
                        CPlatform.InsertPlatform(platform.Key, "");
                    }
                }

                // Get the platforms from the DB (we need the PK)
                Dictionary<string, CPlatform.PlatformObject> dbPlatforms = CPlatform.GetPlatforms();

                // Add the games to the database
                foreach(CGameData.CGame game in jsonAllGames)
                {
                    int platformFK = (dbPlatforms.ContainsKey(game.PlatformString)) ? dbPlatforms[game.PlatformString].PlatformID : 0;
                    GameObject tmp = new GameObject(platformFK, game.ID, game.Title, game.Alias, game.Launch, game.Uninstaller);
                    CGame.InsertGame(tmp);
                }
                HashSet<CGame.GameObject> dbGames = CGame.GetAllGames();
                success = (success && jsonAllGames.Count == dbGames.Count);
                if (!success)
                {
                    CInputOutput.Log("ERROR: Not all games were migrated to the database");
                }

                CInputOutput.Log((success) ? "SUCCESS" : "FAILURE");
                CInputOutput.Log("Game data migration finished");
                CInputOutput.Log(dbPlatforms.Count + " Platforms added");
                CInputOutput.Log(dbGames.Count + " Games added");
                return success;
            }
            else
            {
                CInputOutput.Log("Mode is 'verify' - no changes have been made");
                CInputOutput.Log("In order to perform conversion, run the program in 'apply' mode");
            }
            CInputOutput.Log("**** END ****");
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
