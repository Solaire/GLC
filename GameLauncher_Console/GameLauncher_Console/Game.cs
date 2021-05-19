

namespace GameLauncher_Console
{
    /// <summary>
    /// Handles any game-relaed logic and data structures
    /// </summary>
    public static class CGame
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

        /// <summary>
        /// Main game query, handling all reading and writing
        /// TODO: Once the game class is done, it's probably worth seeing if this can be split into special-purpose queries to improve performance and avoid potential problems
        /// </summary>
        /*
        private class CQryGame : CSqlQry
        { 
            public CQryGame() : base("Game")
            {
                m_fields["GameID"]          = new CSqlFieldInteger("GameID",          CSqlField.QryFlag.cInsWrite | CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cWhere);
                m_fields["PlatformFK"]      = new CSqlFieldInteger("PlatformFK",      CSqlField.QryFlag.cInsWrite | CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cWhere);
                m_fields["Identifier"]      = new CSqlFieldString("Identifier",       CSqlField.QryFlag.cInsWrite | CSqlField.QryFlag.cSelRead);
                m_fields["Title"]           = new CSqlFieldString("Title",            CSqlField.QryFlag.cInsWrite | CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cSelWhere);
                m_fields["Alias"]           = new CSqlFieldString("Alias",            CSqlField.QryFlag.cInsWrite | CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cSelWhere);
                m_fields["Launch"]          = new CSqlFieldString("Launch",           CSqlField.QryFlag.cInsWrite | CSqlField.QryFlag.cSelRead);
                m_fields["Uninstall"]       = new CSqlFieldString("Uninstall",        CSqlField.QryFlag.cInsWrite | CSqlField.QryFlag.cSelRead);
                m_fields["IsFavourite"]     = new CSqlFieldBoolean("IsFavourite",     CSqlField.QryFlag.cInsWrite | CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cSelWhere);
                m_fields["IsHidden"]        = new CSqlFieldBoolean("IsHidden",        CSqlField.QryFlag.cInsWrite | CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cSelWhere);
                m_fields["Frequency"]       = new CSqlFieldDouble("Frequency",        CSqlField.QryFlag.cInsWrite | CSqlField.QryFlag.cSelRead);
                m_fields["Rating"]          = new CSqlFieldInteger("Rating",          CSqlField.QryFlag.cInsWrite | CSqlField.QryFlag.cSelRead);
                m_fields["Description"]     = new CSqlFieldString("Description",      CSqlField.QryFlag.cInsWrite | CSqlField.QryFlag.cSelRead);
                m_fields["IsMultiPlatform"] = new CSqlFieldBoolean("IsMultiPlatform", CSqlField.QryFlag.cInsWrite | CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cSelWhere);
            }

            public string this[string key]
            {
                get { return m_fields[key].String; }
                set { m_fields[key].String = value; }
            }
        }
        */

        /// <summary>
        /// Struct which actually holds the game information
        /// </summary>
        public struct GameObject
        {
            // TODOs:
            //  once platform enum is implemented, change platofmr type from int to Platform

            //private readonly Platform m_platformID;
            private readonly int m_platformID;

            public string ID { get; private set; }
            public string Title { get; private set; }
            public string Launch { get; private set; }
            public string Uninstall { get; private set; }
            public string Icon { get; private set; }
            public bool IsFavourite { get; private set; }
            public bool IsHidden { get; private set; }
            public double OccurCount { get; private set; }

            //public Platform PlatformID { get; }
            public int PlatformID { get; }
        }
    }
}
