using SqlDB;

namespace core_2.DataAccess
{
    /// <summary>
    /// Class for managing data in the "Game" database table.
    /// </summary>
    internal class GameSQL
    {
        // Query parameter names
        private const string FIELD_GAME_ID = "GameID";
        private const string FIELD_PLATFORM_FK = "PlatformFK";
        private const string FIELD_IDENTIFIER = "Identifier";
        private const string FIELD_NAME = "Name";
        private const string FIELD_ALIAS = "Alias";
        private const string FIELD_LAUNCH = "Launch";
        private const string FIELD_FREQUENCY = "Frequency";
        private const string FIELD_IS_FAVOURITE = "IsFavourite";
        private const string FIELD_IS_ENABLED = "IsEnabled";
        private const string FIELD_TAG = "Tag";
        private const string FIELD_ICON = "Icon";

        // Due to C#'s limitations, every query will come with a lot of bloated boilerplate code
        // Best thing to do for now is to hide them away in a region and just use them. I will have to return to them one day
        #region Query definitions

        /// <summary>
        /// Query for inserting new games into the database
        /// </summary>
        internal class QryNewGame : CSqlQry
        {
            internal QryNewGame()
                : base("Game", "", "")
            {
                m_sqlRow[FIELD_PLATFORM_FK] = new CSqlFieldInteger(FIELD_PLATFORM_FK, CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_IDENTIFIER] = new CSqlFieldString(FIELD_IDENTIFIER, CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_NAME] = new CSqlFieldString(FIELD_NAME, CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_ALIAS] = new CSqlFieldString(FIELD_ALIAS, CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_LAUNCH] = new CSqlFieldString(FIELD_LAUNCH, CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_IS_FAVOURITE] = new CSqlFieldBoolean(FIELD_IS_FAVOURITE, CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_IS_ENABLED] = new CSqlFieldBoolean(FIELD_IS_ENABLED, CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_FREQUENCY] = new CSqlFieldDouble(FIELD_FREQUENCY, CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_ICON] = new CSqlFieldString(FIELD_ICON, CSqlField.QryFlag.cInsWrite);
                m_sqlRow[FIELD_TAG] = new CSqlFieldString(FIELD_TAG, CSqlField.QryFlag.cInsWrite);

            }
            internal int PlatformFK
            {
                get { return m_sqlRow[FIELD_PLATFORM_FK].Integer; }
                set { m_sqlRow[FIELD_PLATFORM_FK].Integer = value; }
            }
            internal string Identifier
            {
                get { return m_sqlRow[FIELD_IDENTIFIER].String; }
                set { m_sqlRow[FIELD_IDENTIFIER].String = value; }
            }
            internal string Name
            {
                get { return m_sqlRow[FIELD_NAME].String; }
                set { m_sqlRow[FIELD_NAME].String = value; }
            }
            internal string Alias
            {
                get { return m_sqlRow[FIELD_ALIAS].String; }
                set { m_sqlRow[FIELD_ALIAS].String = value; }
            }
            internal string Launch
            {
                get { return m_sqlRow[FIELD_LAUNCH].String; }
                set { m_sqlRow[FIELD_LAUNCH].String = value; }
            }
            internal bool IsFavourite
            {
                get { return m_sqlRow[FIELD_IS_FAVOURITE].Bool; }
                set { m_sqlRow[FIELD_IS_FAVOURITE].Bool = value; }
            }
            internal bool IsEnabled
            {
                get { return m_sqlRow[FIELD_IS_ENABLED].Bool; }
                set { m_sqlRow[FIELD_IS_ENABLED].Bool = value; }
            }
            internal double Frequency
            {
                get { return m_sqlRow[FIELD_FREQUENCY].Double; }
                set { m_sqlRow[FIELD_FREQUENCY].Double = value; }
            }
            internal string Icon // TODO: maybe different type (image, bytearray?)
            {
                get { return m_sqlRow[FIELD_ICON].String; }
                set { m_sqlRow[FIELD_ICON].String = value; }
            }
            internal string Tag
            {
                get { return m_sqlRow[FIELD_TAG].String; }
                set { m_sqlRow[FIELD_TAG].String = value; }
            }
        }

        /// <summary>
        /// Query for updating or removing a database entry
        /// </summary>
        internal class QryUpdateGame : CSqlQry
        {
            internal QryUpdateGame()
                : base("Game", "", "")
            {
                m_sqlRow[FIELD_GAME_ID] = new CSqlFieldInteger(FIELD_GAME_ID, CSqlField.QryFlag.cUpdWhere | CSqlField.QryFlag.cDelWhere);
                m_sqlRow[FIELD_PLATFORM_FK] = new CSqlFieldInteger(FIELD_PLATFORM_FK, CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_IDENTIFIER] = new CSqlFieldString(FIELD_IDENTIFIER, CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_NAME] = new CSqlFieldString(FIELD_NAME, CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_ALIAS] = new CSqlFieldString(FIELD_ALIAS, CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_LAUNCH] = new CSqlFieldString(FIELD_LAUNCH, CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_IS_FAVOURITE] = new CSqlFieldBoolean(FIELD_IS_FAVOURITE, CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_IS_ENABLED] = new CSqlFieldBoolean(FIELD_IS_ENABLED, CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_FREQUENCY] = new CSqlFieldDouble(FIELD_FREQUENCY, CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_ICON] = new CSqlFieldString(FIELD_ICON, CSqlField.QryFlag.cUpdWrite);
                m_sqlRow[FIELD_TAG] = new CSqlFieldString(FIELD_TAG, CSqlField.QryFlag.cUpdWrite);
            }
            internal int GameID
            {
                get { return m_sqlRow[FIELD_GAME_ID].Integer; }
                set { m_sqlRow[FIELD_GAME_ID].Integer = value; }
            }
            internal int PlatformFK
            {
                get { return m_sqlRow[FIELD_PLATFORM_FK].Integer; }
                set { m_sqlRow[FIELD_PLATFORM_FK].Integer = value; }
            }
            internal string Identifier
            {
                get { return m_sqlRow[FIELD_IDENTIFIER].String; }
                set { m_sqlRow[FIELD_IDENTIFIER].String = value; }
            }
            internal string Name
            {
                get { return m_sqlRow[FIELD_NAME].String; }
                set { m_sqlRow[FIELD_NAME].String = value; }
            }
            internal string Alias
            {
                get { return m_sqlRow[FIELD_ALIAS].String; }
                set { m_sqlRow[FIELD_ALIAS].String = value; }
            }
            internal string Launch
            {
                get { return m_sqlRow[FIELD_LAUNCH].String; }
                set { m_sqlRow[FIELD_LAUNCH].String = value; }
            }
            internal bool IsFavourite
            {
                get { return m_sqlRow[FIELD_IS_FAVOURITE].Bool; }
                set { m_sqlRow[FIELD_IS_FAVOURITE].Bool = value; }
            }
            internal bool IsEnabled
            {
                get { return m_sqlRow[FIELD_IS_ENABLED].Bool; }
                set { m_sqlRow[FIELD_IS_ENABLED].Bool = value; }
            }
            internal double Frequency
            {
                get { return m_sqlRow[FIELD_FREQUENCY].Double; }
                set { m_sqlRow[FIELD_FREQUENCY].Double = value; }
            }
            internal string Icon // TODO: maybe different type (image, bytearray?)
            {
                get { return m_sqlRow[FIELD_ICON].String; }
                set { m_sqlRow[FIELD_ICON].String = value; }
            }
            internal string Tag
            {
                get { return m_sqlRow[FIELD_TAG].String; }
                set { m_sqlRow[FIELD_TAG].String = value; }
            }
        }

        /// <summary>
        /// Base class for read queries
        /// </summary>
        internal abstract class QryBaseReadGame : CSqlQry
        {
            internal QryBaseReadGame(string query)
                : base("Game", query, "")
            {
                m_sqlRow[FIELD_GAME_ID] = new CSqlFieldInteger(FIELD_GAME_ID, CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cSelWhere);
                m_sqlRow[FIELD_PLATFORM_FK] = new CSqlFieldInteger(FIELD_PLATFORM_FK, CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cSelWhere);
                m_sqlRow[FIELD_IDENTIFIER] = new CSqlFieldString(FIELD_IDENTIFIER, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_NAME] = new CSqlFieldString(FIELD_NAME, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_ALIAS] = new CSqlFieldString(FIELD_ALIAS, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_LAUNCH] = new CSqlFieldString(FIELD_LAUNCH, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_IS_FAVOURITE] = new CSqlFieldBoolean(FIELD_IS_FAVOURITE, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_IS_ENABLED] = new CSqlFieldBoolean(FIELD_IS_ENABLED, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_FREQUENCY] = new CSqlFieldDouble(FIELD_FREQUENCY, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_ICON] = new CSqlFieldString(FIELD_ICON, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TAG] = new CSqlFieldString(FIELD_TAG, CSqlField.QryFlag.cSelRead);
            }
            internal int GameID
            {
                get { return m_sqlRow[FIELD_GAME_ID].Integer; }
                set { m_sqlRow[FIELD_GAME_ID].Integer = value; }
            }
            internal int PlatformFK
            {
                get { return m_sqlRow[FIELD_PLATFORM_FK].Integer; }
                set { m_sqlRow[FIELD_PLATFORM_FK].Integer = value; }
            }
            internal string Identifier
            {
                get { return m_sqlRow[FIELD_IDENTIFIER].String; }
                set { m_sqlRow[FIELD_IDENTIFIER].String = value; }
            }
            internal string Name
            {
                get { return m_sqlRow[FIELD_NAME].String; }
                set { m_sqlRow[FIELD_NAME].String = value; }
            }
            internal string Alias
            {
                get { return m_sqlRow[FIELD_ALIAS].String; }
                set { m_sqlRow[FIELD_ALIAS].String = value; }
            }
            internal string Launch
            {
                get { return m_sqlRow[FIELD_LAUNCH].String; }
                set { m_sqlRow[FIELD_LAUNCH].String = value; }
            }
            internal bool IsFavourite
            {
                get { return m_sqlRow[FIELD_IS_FAVOURITE].Bool; }
                set { m_sqlRow[FIELD_IS_FAVOURITE].Bool = value; }
            }
            internal bool IsEnabled
            {
                get { return m_sqlRow[FIELD_IS_ENABLED].Bool; }
                set { m_sqlRow[FIELD_IS_ENABLED].Bool = value; }
            }
            internal double Frequency
            {
                get { return m_sqlRow[FIELD_FREQUENCY].Double; }
                set { m_sqlRow[FIELD_FREQUENCY].Double = value; }
            }
            internal string Icon // TODO: maybe different type (image, bytearray?)
            {
                get { return m_sqlRow[FIELD_ICON].String; }
                set { m_sqlRow[FIELD_ICON].String = value; }
            }
            internal string Tag
            {
                get { return m_sqlRow[FIELD_TAG].String; }
                set { m_sqlRow[FIELD_TAG].String = value; }
            }
        }

        /// <summary>
        /// Query for generic read
        /// </summary>
        internal class QryReadGame : QryBaseReadGame
        {
            internal QryReadGame()
                : base("((& = ?) OR (& = ?))")
            {
                m_sqlRow[FIELD_GAME_ID] = new CSqlFieldInteger(FIELD_GAME_ID, CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cSelWhere);
                m_sqlRow[FIELD_PLATFORM_FK] = new CSqlFieldInteger(FIELD_PLATFORM_FK, CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cSelWhere);
                m_sqlRow[FIELD_IDENTIFIER] = new CSqlFieldString(FIELD_IDENTIFIER, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_NAME] = new CSqlFieldString(FIELD_NAME, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_ALIAS] = new CSqlFieldString(FIELD_ALIAS, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_LAUNCH] = new CSqlFieldString(FIELD_LAUNCH, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_IS_FAVOURITE] = new CSqlFieldBoolean(FIELD_IS_FAVOURITE, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_IS_ENABLED] = new CSqlFieldBoolean(FIELD_IS_ENABLED, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_FREQUENCY] = new CSqlFieldDouble(FIELD_FREQUENCY, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_ICON] = new CSqlFieldString(FIELD_ICON, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TAG] = new CSqlFieldString(FIELD_TAG, CSqlField.QryFlag.cSelRead);
            }
        }

        /// <summary>
        /// Class for performing a fuzzy search
        /// </summary>
        internal class QryGameFuzzySearch : QryBaseReadGame
        {
            internal QryGameFuzzySearch()
                : base("(& LIKE '%?%')")
            {
                m_sqlRow[FIELD_NAME] = new CSqlFieldString(FIELD_NAME, CSqlField.QryFlag.cSelRead | CSqlField.QryFlag.cSelWhere);
                m_sqlRow[FIELD_GAME_ID] = new CSqlFieldInteger(FIELD_GAME_ID, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_PLATFORM_FK] = new CSqlFieldInteger(FIELD_PLATFORM_FK, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_IDENTIFIER] = new CSqlFieldString(FIELD_IDENTIFIER, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_ALIAS] = new CSqlFieldString(FIELD_ALIAS, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_LAUNCH] = new CSqlFieldString(FIELD_LAUNCH, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_IS_FAVOURITE] = new CSqlFieldBoolean(FIELD_IS_FAVOURITE, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_IS_ENABLED] = new CSqlFieldBoolean(FIELD_IS_ENABLED, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_FREQUENCY] = new CSqlFieldDouble(FIELD_FREQUENCY, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_ICON] = new CSqlFieldString(FIELD_ICON, CSqlField.QryFlag.cSelRead);
                m_sqlRow[FIELD_TAG] = new CSqlFieldString(FIELD_TAG, CSqlField.QryFlag.cSelRead);
            }
        }

        #endregion // Query definitions

        private static QryNewGame m_qryNewGame = new QryNewGame();
        private static QryUpdateGame m_qryUpdateGame = new QryUpdateGame();
        private static QryReadGame m_qryReadGame = new QryReadGame();
        private static QryGameFuzzySearch m_qryGameFuzzySearch = new QryGameFuzzySearch();
    }
}
