using System.Collections.Generic;

namespace core.Game
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

        // Query parameter names
        private const string FIELD_GAME_ID       = "GameID";
        private const string FIELD_PLATFORM_FK   = "PlatformFK";
        private const string FIELD_IDENTIFIER    = "Identifier";
        private const string FIELD_TITLE         = "Title";
        private const string FIELD_ALIAS         = "Alias";
        private const string FIELD_LAUNCH        = "Launch";
        private const string FIELD_FREQUENCY     = "Frequency";
        private const string FIELD_IS_FAVOURITE  = "IsFavourite";
        private const string FIELD_IS_HIDDEN     = "IsHidden";
        private const string FIELD_TAG           = "Tag";
        private const string FIELD_ICON          = "Icon";

        // Due to C#'s limitations, every query will come with a lot of bloated boilerplate code
        // Best thing to do for now is to hide them away in a region and just use them. I will have to return to them one day
        #region Query definitions

        /// <summary>
        /// Query for inserting new games into the database
        /// </summary>
        public class CQryNewGame : CSqlQry
        {
            public CQryNewGame()
                : base("Game", "", "")
            {
                m_sqlRow[FIELD_PLATFORM_FK]  = new CSqlFieldInteger(FIELD_PLATFORM_FK  , CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_IDENTIFIER]   = new CSqlFieldString(FIELD_IDENTIFIER    , CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_TITLE]        = new CSqlFieldString(FIELD_TITLE         , CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_ALIAS]        = new CSqlFieldString(FIELD_ALIAS         , CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_LAUNCH]       = new CSqlFieldString(FIELD_LAUNCH        , CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_IS_FAVOURITE] = new CSqlFieldBoolean(FIELD_IS_FAVOURITE , CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_IS_HIDDEN]    = new CSqlFieldBoolean(FIELD_IS_HIDDEN    , CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_FREQUENCY]    = new CSqlFieldDouble(FIELD_FREQUENCY     , CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_ICON]         = new CSqlFieldString(FIELD_ICON          , CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_TAG]          = new CSqlFieldString(FIELD_TAG           , CSqlField.QryFlag.cInsWrite);

            }
            public int PlatformFK
            {
                get { return m_sqlRow[FIELD_PLATFORM_FK].Integer; }
                set { m_sqlRow[FIELD_PLATFORM_FK].Integer = value; }
            }
            public string Identifier
            {
                get { return m_sqlRow[FIELD_IDENTIFIER].String; }
                set { m_sqlRow[FIELD_IDENTIFIER].String = value; }
            }
            public string Title
            {
                get { return m_sqlRow[FIELD_TITLE].String; }
                set { m_sqlRow[FIELD_TITLE].String = value; }
            }
            public string Alias
            {
                get { return m_sqlRow[FIELD_ALIAS].String; }
                set { m_sqlRow[FIELD_ALIAS].String = value; }
            }
            public string Launch
            {
                get { return m_sqlRow[FIELD_LAUNCH].String; }
                set { m_sqlRow[FIELD_LAUNCH].String = value; }
            }
            public bool IsFavourite
            {
                get { return m_sqlRow[FIELD_IS_FAVOURITE].Bool; }
                set { m_sqlRow[FIELD_IS_FAVOURITE].Bool = value; }
            }
            public bool IsHidden
            {
                get { return m_sqlRow[FIELD_IS_HIDDEN].Bool; }
                set { m_sqlRow[FIELD_IS_HIDDEN].Bool = value; }
            }
            public double Frequency
            {
                get { return m_sqlRow[FIELD_FREQUENCY].Double; }
                set { m_sqlRow[FIELD_FREQUENCY].Double = value; }
            }
            public string Icon // TODO: maybe different type (image, bytearray?)
            {
                get { return m_sqlRow[FIELD_ICON].String; }
                set { m_sqlRow[FIELD_ICON].String = value; }
            }
            public string Tag
            {
                get { return m_sqlRow[FIELD_TAG].String; }
                set { m_sqlRow[FIELD_TAG].String = value; }
            }
        }

        /// <summary>
        /// Query for updating or removing a database entry
        /// </summary>
        public class CQryUpdateGame : CSqlQry
        {
            public CQryUpdateGame()
                : base("Game", "", "")
            {
                m_sqlRow[FIELD_GAME_ID]      = new CSqlFieldInteger(FIELD_GAME_ID      , CSqlField.QryFlag.cUpdWhere | CSqlField.QryFlag.cDelWhere);
                m_sqlRow[FIELD_PLATFORM_FK]  = new CSqlFieldInteger(FIELD_PLATFORM_FK  , CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_IDENTIFIER]   = new CSqlFieldString(FIELD_IDENTIFIER    , CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_TITLE]        = new CSqlFieldString(FIELD_TITLE         , CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_ALIAS]        = new CSqlFieldString(FIELD_ALIAS         , CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_LAUNCH]       = new CSqlFieldString(FIELD_LAUNCH        , CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_IS_FAVOURITE] = new CSqlFieldBoolean(FIELD_IS_FAVOURITE , CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_IS_HIDDEN]    = new CSqlFieldBoolean(FIELD_IS_HIDDEN    , CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_FREQUENCY]    = new CSqlFieldDouble(FIELD_FREQUENCY     , CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_ICON]         = new CSqlFieldString(FIELD_ICON          , CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_TAG]          = new CSqlFieldString(FIELD_TAG           , CSqlField.QryFlag.cUpdWrite);
            }
            public int GameID
            {
                get { return m_sqlRow[FIELD_GAME_ID].Integer; }
                set { m_sqlRow[FIELD_GAME_ID].Integer = value; }
            }
            public int PlatformFK
            {
                get { return m_sqlRow[FIELD_PLATFORM_FK].Integer; }
                set { m_sqlRow[FIELD_PLATFORM_FK].Integer = value; }
            }
            public string Identifier
            {
                get { return m_sqlRow[FIELD_IDENTIFIER].String; }
                set { m_sqlRow[FIELD_IDENTIFIER].String = value; }
            }
            public string Title
            {
                get { return m_sqlRow[FIELD_TITLE].String; }
                set { m_sqlRow[FIELD_TITLE].String = value; }
            }
            public string Alias
            {
                get { return m_sqlRow[FIELD_ALIAS].String; }
                set { m_sqlRow[FIELD_ALIAS].String = value; }
            }
            public string Launch
            {
                get { return m_sqlRow[FIELD_LAUNCH].String; }
                set { m_sqlRow[FIELD_LAUNCH].String = value; }
            }
            public bool IsFavourite
            {
                get { return m_sqlRow[FIELD_IS_FAVOURITE].Bool; }
                set { m_sqlRow[FIELD_IS_FAVOURITE].Bool = value; }
            }
            public bool IsHidden
            {
                get { return m_sqlRow[FIELD_IS_HIDDEN].Bool; }
                set { m_sqlRow[FIELD_IS_HIDDEN].Bool = value; }
            }
            public double Frequency
            {
                get { return m_sqlRow[FIELD_FREQUENCY].Double; }
                set { m_sqlRow[FIELD_FREQUENCY].Double = value; }
            }
            public string Icon // TODO: maybe different type (image, bytearray?)
            {
                get { return m_sqlRow[FIELD_ICON].String; }
                set { m_sqlRow[FIELD_ICON].String = value; }
            }
            public string Tag
            {
                get { return m_sqlRow[FIELD_TAG].String; }
                set { m_sqlRow[FIELD_TAG].String = value; }
            }
        }

        /// <summary>
        /// Query for updating or removing a database entry
        /// </summary>
        public class CQryReadGame : CSqlQry
        {
            public CQryReadGame()
                : base("Game", "((& = ?) OR (& = ?))", "")
            {
                m_sqlRow[FIELD_GAME_ID]      = new CSqlFieldInteger(FIELD_GAME_ID      , CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cSelWhere);
                m_sqlRow[FIELD_PLATFORM_FK]  = new CSqlFieldInteger(FIELD_PLATFORM_FK  , CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cSelWhere);
                m_sqlRow[FIELD_IDENTIFIER]   = new CSqlFieldString(FIELD_IDENTIFIER    , CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TITLE]        = new CSqlFieldString(FIELD_TITLE         , CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_ALIAS]        = new CSqlFieldString(FIELD_ALIAS         , CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_LAUNCH]       = new CSqlFieldString(FIELD_LAUNCH        , CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_IS_FAVOURITE] = new CSqlFieldBoolean(FIELD_IS_FAVOURITE , CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_IS_HIDDEN]    = new CSqlFieldBoolean(FIELD_IS_HIDDEN    , CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_FREQUENCY]    = new CSqlFieldDouble(FIELD_FREQUENCY     , CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_ICON]         = new CSqlFieldString(FIELD_ICON          , CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TAG]          = new CSqlFieldString(FIELD_TAG           , CSqlField.QryFlag.cSelRead);
            }
            public int GameID
            {
                get { return m_sqlRow[FIELD_GAME_ID].Integer; }
                set { m_sqlRow[FIELD_GAME_ID].Integer = value; }
            }
            public int PlatformFK
            {
                get { return m_sqlRow[FIELD_PLATFORM_FK].Integer; }
                set { m_sqlRow[FIELD_PLATFORM_FK].Integer = value; }
            }
            public string Identifier
            {
                get { return m_sqlRow[FIELD_IDENTIFIER].String; }
                set { m_sqlRow[FIELD_IDENTIFIER].String = value; }
            }
            public string Title
            {
                get { return m_sqlRow[FIELD_TITLE].String; }
                set { m_sqlRow[FIELD_TITLE].String = value; }
            }
            public string Alias
            {
                get { return m_sqlRow[FIELD_ALIAS].String; }
                set { m_sqlRow[FIELD_ALIAS].String = value; }
            }
            public string Launch
            {
                get { return m_sqlRow[FIELD_LAUNCH].String; }
                set { m_sqlRow[FIELD_LAUNCH].String = value; }
            }
            public bool IsFavourite
            {
                get { return m_sqlRow[FIELD_IS_FAVOURITE].Bool; }
                set { m_sqlRow[FIELD_IS_FAVOURITE].Bool = value; }
            }
            public bool IsHidden
            {
                get { return m_sqlRow[FIELD_IS_HIDDEN].Bool; }
                set { m_sqlRow[FIELD_IS_HIDDEN].Bool = value; }
            }
            public double Frequency
            {
                get { return m_sqlRow[FIELD_FREQUENCY].Double; }
                set { m_sqlRow[FIELD_FREQUENCY].Double = value; }
            }
            public string Icon // TODO: maybe different type (image, bytearray?)
            {
                get { return m_sqlRow[FIELD_ICON].String; }
                set { m_sqlRow[FIELD_ICON].String = value; }
            }
            public string Tag
            {
                get { return m_sqlRow[FIELD_TAG].String; }
                set { m_sqlRow[FIELD_TAG].String = value; }
            }
        }

        /// <summary>
        /// Class for performing a fuzzy search
        /// </summary>
        public class CQryGameFuzzySearch : CSqlQry
        {
            public CQryGameFuzzySearch()
                : base("Game", "(& LIKE '%?%')", "")
            {
                m_sqlRow[FIELD_TITLE]       = new CSqlFieldString(FIELD_TITLE           , CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cSelWhere);
                m_sqlRow[FIELD_GAME_ID]     = new CSqlFieldInteger(FIELD_GAME_ID        , CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_PLATFORM_FK] = new CSqlFieldInteger(FIELD_PLATFORM_FK    , CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_IDENTIFIER]  = new CSqlFieldString(FIELD_IDENTIFIER      , CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_ALIAS]       = new CSqlFieldString(FIELD_ALIAS           , CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_LAUNCH]      = new CSqlFieldString(FIELD_LAUNCH          , CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_IS_FAVOURITE]= new CSqlFieldBoolean(FIELD_IS_FAVOURITE   , CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_IS_HIDDEN]   = new CSqlFieldBoolean(FIELD_IS_HIDDEN      , CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_FREQUENCY]   = new CSqlFieldDouble(FIELD_FREQUENCY       , CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_ICON]        = new CSqlFieldString(FIELD_ICON            , CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TAG]         = new CSqlFieldString(FIELD_TAG             , CSqlField.QryFlag.cSelRead);
            }
            public int GameID
            {
                get { return m_sqlRow[FIELD_GAME_ID].Integer; }
                set { m_sqlRow[FIELD_GAME_ID].Integer = value; }
            }
            public int PlatformFK
            {
                get { return m_sqlRow[FIELD_PLATFORM_FK].Integer; }
                set { m_sqlRow[FIELD_PLATFORM_FK].Integer = value; }
            }
            public string Identifier
            {
                get { return m_sqlRow[FIELD_IDENTIFIER].String; }
                set { m_sqlRow[FIELD_IDENTIFIER].String = value; }
            }
            public string Title
            {
                get { return m_sqlRow[FIELD_TITLE].String; }
                set { m_sqlRow[FIELD_TITLE].String = value; }
            }
            public string Alias
            {
                get { return m_sqlRow[FIELD_ALIAS].String; }
                set { m_sqlRow[FIELD_ALIAS].String = value; }
            }
            public string Launch
            {
                get { return m_sqlRow[FIELD_LAUNCH].String; }
                set { m_sqlRow[FIELD_LAUNCH].String = value; }
            }
            public bool IsFavourite
            {
                get { return m_sqlRow[FIELD_IS_FAVOURITE].Bool; }
                set { m_sqlRow[FIELD_IS_FAVOURITE].Bool = value; }
            }
            public bool IsHidden
            {
                get { return m_sqlRow[FIELD_IS_HIDDEN].Bool; }
                set { m_sqlRow[FIELD_IS_HIDDEN].Bool = value; }
            }
            public double Frequency
            {
                get { return m_sqlRow[FIELD_FREQUENCY].Double; }
                set { m_sqlRow[FIELD_FREQUENCY].Double = value; }
            }
            public string Icon // TODO: maybe different type (image, bytearray?)
            {
                get { return m_sqlRow[FIELD_ICON].String; }
                set { m_sqlRow[FIELD_ICON].String = value; }
            }
            public string Tag
            {
                get { return m_sqlRow[FIELD_TAG].String; }
                set { m_sqlRow[FIELD_TAG].String = value; }
            }
        }

        #endregion // Query definitions

        private static CQryNewGame          m_qryNewGame         = new CQryNewGame();
        private static CQryUpdateGame       m_qryUpdateGame      = new CQryUpdateGame();
        private static CQryReadGame         m_qryReadGame        = new CQryReadGame();
        private static CQryGameFuzzySearch  m_qryGameFuzzySearch = new CQryGameFuzzySearch();

        /// <summary>
        /// Load all games from the database
        /// </summary>
        /// <returns>HashSet of game objects</returns>
        public static HashSet<GameObject> LoadAllGames()
        {
            HashSet<GameObject> databaseGames = new HashSet<GameObject>();

            m_qryReadGame.MakeFieldsNull();
            m_qryReadGame.SelectExtraCondition = "";
            if(m_qryReadGame.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                do
                {
                    databaseGames.Add(new GameObject(m_qryReadGame));
                } while(m_qryReadGame.Fetch());
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

            m_qryReadGame.MakeFieldsNull();
            m_qryReadGame.SelectExtraCondition = "";
            m_qryReadGame.PlatformFK = platformFK;
            if(m_qryReadGame.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                do
                {
                    databaseGames.Add(new GameObject(m_qryReadGame));
                } while(m_qryReadGame.Fetch());
            }

            return databaseGames;
        }

        public static HashSet<GameObject> LoadPlatformGames(int platformFK, bool favourites)
        {
            HashSet<GameObject> databaseGames = new HashSet<GameObject>();

            m_qryReadGame.MakeFieldsNull();
            m_qryReadGame.PlatformFK = platformFK;
            if(favourites)
            {
                m_qryReadGame.SelectExtraCondition = " AND (IsFavourite = 1)";
            }
            if(m_qryReadGame.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                do
                {
                    databaseGames.Add(new GameObject(m_qryReadGame));
                } while(m_qryReadGame.Fetch());
            }

            return databaseGames;
        }

        public static HashSet<GameObject> LoadPlatformGames(int platformFK, string groupName)
        {
            HashSet<GameObject> databaseGames = new HashSet<GameObject>();

            m_qryReadGame.MakeFieldsNull();
            m_qryReadGame.PlatformFK = platformFK;
            m_qryReadGame.SelectExtraCondition = $" AND (Tag = '{groupName}')";
            if(m_qryReadGame.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                do
                {
                    databaseGames.Add(new GameObject(m_qryReadGame));
                } while(m_qryReadGame.Fetch());
            }

            return databaseGames;
        }


        public static HashSet<GameObject> GameSearch(string searchTerm)
        {
            HashSet<GameObject> databaseGames = new HashSet<GameObject>();
            m_qryGameFuzzySearch.MakeFieldsNull();
            m_qryGameFuzzySearch.Title = searchTerm;
            if(m_qryGameFuzzySearch.Select() == System.Data.SQLite.SQLiteErrorCode.Ok)
            {
                do
                {
                    databaseGames.Add(new GameObject(m_qryGameFuzzySearch));
                } while(m_qryGameFuzzySearch.Fetch());
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
            m_qryUpdateGame.MakeFieldsNull();
            m_qryUpdateGame.GameID = gameID;
            m_qryUpdateGame.IsFavourite = isFavourite;
            m_qryUpdateGame.Update();
        }

        /// <summary>
        /// Update game's Hidden flag
        /// </summary>
        /// <param name="gameID">The gameID</param>
        /// <param name="isHidden">The new Hidden value</param>
        public static void ToggleHidden(int gameID, bool isHidden)
        {
            m_qryUpdateGame.MakeFieldsNull();
            m_qryUpdateGame.GameID = gameID;
            m_qryUpdateGame.IsHidden = isHidden;
            m_qryUpdateGame.Update();
        }

        /// <summary>
        /// Insert game into the database
        /// </summary>
        /// <param name="game">The new game</param>
        /// <returns>True if insert was successful</returns>
        public static bool InsertGame(GameObject game)
        {
            m_qryNewGame.MakeFieldsNull();
            m_qryNewGame.PlatformFK    = game.PlatformFK;
            m_qryNewGame.Identifier    = game.Identifier;
            m_qryNewGame.Title         = game.Title;
            m_qryNewGame.Alias         = game.Alias;
            m_qryNewGame.Launch        = game.Launch;
            m_qryNewGame.Tag           = game.Tag;

            return m_qryNewGame.Insert() == System.Data.SQLite.SQLiteErrorCode.Ok;
        }

        /// <summary>
        /// Remove game from the database
        /// </summary>
        /// <param name="gameID">The gameID</param>
        /// <returns>True if delete was successful</returns>
        public static bool DeleteGame(int gameID)
        {
            m_qryUpdateGame.MakeFieldsNull();
            m_qryUpdateGame.GameID = gameID;
            return m_qryUpdateGame.Delete() == System.Data.SQLite.SQLiteErrorCode.Ok;
        }
    }
}
