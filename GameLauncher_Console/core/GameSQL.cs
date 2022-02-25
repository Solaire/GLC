using System.Collections.Generic;
using SqlDB;
using static SqlDB.CSqlField;

namespace core
{
    public static class CGameSQL
    {
        /// <summary>
        /// Game title articles.
        /// Removed from titles when creating default aliases
        /// Ignored during title searches
        /// </summary>
        public static readonly string[] ARTICLES =
        {
            "The ",								// English definite
			"A ", "An ",						// English indefinite
			/*
			"El ", "La ", "Los ", "Las ",		// Spanish definite
			"Un ", "Una ", "Unos ", "Unas ",	// Spanish indefinite
			"Le ", "Les ", "L\'",				//, "La" [Spanish] // French definite
			"Une ", "De ", "Des ",				//, "Un" [Spanish] // French indefinite [many French sort with indefinite article]
			"Der", "Das",						//, "Die" [English word] // German definite
			"Ein", "Eine"						// German indefinite
			*/
        };

        // Due to C#'s limitations, every query will come with a lot of bloated boilerplate code
        // Best thing to do for now is to hide them away in a region and just use them. I will have to return to them one day
        #region Query definitions

        /// <summary>
        /// Main game query, handling all reading and writing
        /// TODO: Once the game class is done, it's probably worth seeing if this can be split into special-purpose queries to improve performance and avoid potential problems
        /// </summary>
        public class CQryGame : CSqlQry
        {
            public CQryGame() : base("Game", "", "")
            {
                m_sqlRow["GameID"] = new CSqlFieldInteger("GameID", QryFlag.cSelRead | QryFlag.cWhere);
                m_sqlRow["PlatformFK"] = new CSqlFieldInteger("PlatformFK", QryFlag.cInsWrite | QryFlag.cSelRead | QryFlag.cWhere);
                m_sqlRow["Identifier"] = new CSqlFieldString("Identifier", QryFlag.cInsWrite | QryFlag.cSelRead);
                m_sqlRow["Title"] = new CSqlFieldString("Title", QryFlag.cInsWrite | QryFlag.cSelRead | QryFlag.cSelWhere);
                m_sqlRow["Alias"] = new CSqlFieldString("Alias", QryFlag.cInsWrite | QryFlag.cSelRead | QryFlag.cSelWhere);
                m_sqlRow["Launch"] = new CSqlFieldString("Launch", QryFlag.cInsWrite | QryFlag.cSelRead);
                m_sqlRow["Uninstall"] = new CSqlFieldString("Uninstall", QryFlag.cInsWrite | QryFlag.cSelRead);
                m_sqlRow["IsInstalled"] = new CSqlFieldBoolean("IsInstalled", QryFlag.cInsWrite | QryFlag.cSelRead | QryFlag.cSelWhere);
                m_sqlRow["IsFavourite"] = new CSqlFieldBoolean("IsFavourite", QryFlag.cInsWrite | QryFlag.cSelRead | QryFlag.cSelWhere);
                m_sqlRow["IsNew"] = new CSqlFieldBoolean("IsNew", QryFlag.cInsWrite | QryFlag.cSelRead | QryFlag.cSelWhere);
                m_sqlRow["IsHidden"] = new CSqlFieldBoolean("IsHidden", QryFlag.cInsWrite | QryFlag.cSelRead | QryFlag.cSelWhere);
                m_sqlRow["Frequency"] = new CSqlFieldDouble("Frequency", QryFlag.cInsWrite | QryFlag.cSelRead);
                m_sqlRow["Rating"] = new CSqlFieldInteger("Rating", QryFlag.cInsWrite | QryFlag.cSelRead);
                m_sqlRow["Icon"] = new CSqlFieldString("Icon", QryFlag.cInsWrite | QryFlag.cSelRead);
                m_sqlRow["Description"] = new CSqlFieldString("Description", QryFlag.cInsWrite | QryFlag.cSelRead);
                m_sqlRow["IsMultiPlatform"] = new CSqlFieldBoolean("IsMultiPlatform", QryFlag.cInsWrite | QryFlag.cSelRead | QryFlag.cSelWhere);
                m_sqlRow["Group"] = new CSqlFieldString("Group", QryFlag.cInsWrite | QryFlag.cSelRead | QryFlag.cSelWhere);
            }
            public int GameID
            {
                get { return m_sqlRow["GameID"].Integer; }
                set { m_sqlRow["GameID"].Integer = value; }
            }
            public int PlatformFK
            {
                get { return m_sqlRow["PlatformFK"].Integer; }
                set { m_sqlRow["PlatformFK"].Integer = value; }
            }
            public string Identifier
            {
                get { return m_sqlRow["Identifier"].String; }
                set { m_sqlRow["Identifier"].String = value; }
            }
            public string Title
            {
                get { return m_sqlRow["Title"].String; }
                set { m_sqlRow["Title"].String = value; }
            }
            public string Alias
            {
                get { return m_sqlRow["Alias"].String; }
                set { m_sqlRow["Alias"].String = value; }
            }
            public string Launch
            {
                get { return m_sqlRow["Launch"].String; }
                set { m_sqlRow["Launch"].String = value; }
            }
            public string Uninstall
            {
                get { return m_sqlRow["Uninstall"].String; }
                set { m_sqlRow["Uninstall"].String = value; }
            }
            public bool IsInstalled
            {
                get { return m_sqlRow["IsInstalled"].Bool; }
                set { m_sqlRow["IsInstalled"].Bool = value; }
            }
            public bool IsFavourite
            {
                get { return m_sqlRow["IsFavourite"].Bool; }
                set { m_sqlRow["IsFavourite"].Bool = value; }
            }
            public bool IsNew
            {
                get { return m_sqlRow["IsNew"].Bool; }
                set { m_sqlRow["IsNew"].Bool = value; }
            }
            public bool IsHidden
            {
                get { return m_sqlRow["IsHidden"].Bool; }
                set { m_sqlRow["IsHidden"].Bool = value; }
            }
            public double Frequency
            {
                get { return m_sqlRow["Frequency"].Double; }
                set { m_sqlRow["Frequency"].Double = value; }
            }
            public int Rating
            {
                get { return m_sqlRow["Rating"].Integer; }
                set { m_sqlRow["Rating"].Integer = value; }
            }
            public string Icon
            {
                get { return m_sqlRow["Icon"].String; }
                set { m_sqlRow["Icon"].String = value; }
            }
            public string Description
            {
                get { return m_sqlRow["Description"].String; }
                set { m_sqlRow["Description"].String = value; }
            }
            public bool IsMultiPlatform
            {
                get { return m_sqlRow["IsMultiPlatform"].Bool; }
                set { m_sqlRow["IsMultiPlatform"].Bool = value; }
            }
            public string Group
            {
                get { return m_sqlRow["Group"].String; }
                set { m_sqlRow["Group"].String = value; }
            }
        }

        /// <summary>
        /// Query for game sorting
        /// </summary>
        private class CQryGameSort : CSqlQry
        {
            public CQryGameSort() : base("Game", "", "")
            {
                m_sqlRow["GameID"] = new CSqlFieldInteger("GameID", QryFlag.cSelRead);
                m_sqlRow["PlatformFK"] = new CSqlFieldInteger("PlatformFK", QryFlag.cSelWhere);
            }

            public int GameID
            {
                get { return m_sqlRow["GameID"].Integer; }
                set { m_sqlRow["GameID"].Integer = value; }
            }
            public int PlatformFK
            {
                get { return m_sqlRow["PlatformFK"].Integer; }
                set { m_sqlRow["PlatformFK"].Integer = value; }
            }
        }

        /// <summary>
        /// Class for performing a fuzzy search
        /// </summary>
        private class CQryGameFuzzySearch : CSqlQry
        {
            public CQryGameFuzzySearch() : base("Game", "(& = ?) AND (& LIKE '%?%')", "")
            {
                m_sqlRow["PlatformFK"] = new CSqlFieldInteger("PlatformFK", QryFlag.cSelRead | QryFlag.cSelWhere);
                m_sqlRow["Title"] = new CSqlFieldString("Title", QryFlag.cSelRead | QryFlag.cSelWhere);
                m_sqlRow["GameID"] = new CSqlFieldInteger("GameID", QryFlag.cSelRead);
            }
            public int GameID
            {
                get { return m_sqlRow["GameID"].Integer; }
                set { m_sqlRow["GameID"].Integer = value; }
            }
            public int PlatformFK
            {
                get { return m_sqlRow["PlatformFK"].Integer; }
                set { m_sqlRow["PlatformFK"].Integer = value; }
            }
            public string Title
            {
                get { return m_sqlRow["Title"].String; }
                set { m_sqlRow["Title"].String = value; }
            }
        }

        public class CQryGameCount : CSqlQry
        {
            public CQryGameCount()
                : base("Game", "", "")
            {
                m_sqlRow["GameCount"] = new CSqlFieldInteger("COUNT(*) as GameCount", CSqlField.QryFlag.cSelRead);
                m_sqlRow["PlatformFK"] = new CSqlFieldInteger("PlatformFK", CSqlField.QryFlag.cSelWhere);
            }

            public int GameCount
            {
                get { return m_sqlRow["GameCount"].Integer; }
                set { m_sqlRow["GameCount"].Integer = value; }
            }

            public int PlatformFK
            {
                get { return m_sqlRow["PlatformFK"].Integer; }
                set { m_sqlRow["PlatformFK"].Integer = value; }
            }
        }

        #endregion // Query definitions

        private static CQryGame m_qryGame           = new CQryGame();
        private static CQryGameCount m_qryGameCount = new CQryGameCount();
        private static CDbAttribute m_gameAttribute = new CDbAttribute("Game");

        /// <summary>
        /// Load all games from the database
        /// </summary>
        /// <returns>HashSet of game objects</returns>
        public static HashSet<GameObject> LoadAllGames()
        {
            HashSet<GameObject> databaseGames = new HashSet<GameObject>();

            m_qryGame.MakeFieldsNull();
            if(m_qryGame.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                do
                {
                    databaseGames.Add(new GameObject(m_qryGame));
                } while(m_qryGame.Fetch());
            }

            return databaseGames;
        }

        /// <summary>
        /// Load games from the database, matching the platform
        /// </summary>
        /// <param name="platformFK">The platform foreign key</param>
        /// <returns>HashSet of platform-specific games</returns>
        public static HashSet<GameObject> LoadPlatformGames(int platformFK)
        {
            HashSet<GameObject> databaseGames = new HashSet<GameObject>();

            m_qryGame.MakeFieldsNull();
            m_qryGame.PlatformFK = platformFK;
            if(m_qryGame.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                do
                {
                    databaseGames.Add(new GameObject(m_qryGame));
                } while(m_qryGame.Fetch());
            }

            return databaseGames;
        }

        /// <summary>
        /// Update the frequency value of all games
        /// Increment specified game's value by 5
        /// Reduce all other frequencies by 10%
        /// </summary>
        /// <param name="incrementGameID">Frequency increment game ID</param>
        public static void NormaliseFrequencies(int incrementGameID)
        {
            CSqlDB.Instance.Conn.Execute("UPDATE Game SET Frequency = (CASE GameID WHEN " + incrementGameID + " THEN Frequency + 5 ELSE Frequency * 0.9 END)");
        }

        /// <summary>
        /// Update game's Favourite flag
        /// </summary>
        /// <param name="gameID">The gameID</param>
        /// <param name="isFavourite">The new Favourite value</param>
        public static void ToggleFavourite(int gameID, bool isFavourite)
        {
            m_qryGame.MakeFieldsNull();
            m_qryGame.GameID = gameID;
            m_qryGame.IsFavourite = isFavourite;
            m_qryGame.Update();
        }

        /// <summary>
        /// Update game's Hidden flag
        /// </summary>
        /// <param name="gameID">The gameID</param>
        /// <param name="isHidden">The new Hidden value</param>
        public static void ToggleHidden(int gameID, bool isHidden)
        {
            m_qryGame.MakeFieldsNull();
            m_qryGame.GameID = gameID;
            m_qryGame.IsHidden = isHidden;
            m_qryGame.Update();
        }

        /// <summary>
        /// Insert game into the database
        /// </summary>
        /// <param name="game">The new game</param>
        /// <returns>True if insert was successful</returns>
        public static bool InsertGame(GameObject game)
        {
            m_qryGame.MakeFieldsNull();
            m_qryGame.PlatformFK    = game.PlatformFK;
            m_qryGame.Identifier    = game.Identifier;
            m_qryGame.Title         = game.Title;
            m_qryGame.Alias         = game.Alias;
            m_qryGame.Launch        = game.Launch;

            return m_qryGame.Insert() == System.Data.SQLite.SQLiteErrorCode.Ok;
        }

        /// <summary>
        /// Remove game from the database
        /// </summary>
        /// <param name="gameID">The gameID</param>
        /// <returns>True if delete was successful</returns>
        public static bool DeleteGame(int gameID)
        {
            m_qryGame.MakeFieldsNull();
            m_qryGame.GameID = gameID;
            return m_qryGame.Delete() == System.Data.SQLite.SQLiteErrorCode.Ok;
        }
    }
}
