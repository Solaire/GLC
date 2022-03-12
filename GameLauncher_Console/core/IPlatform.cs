using System.Collections.Generic;

namespace core
{
    /// <summary>
    /// Abstract base for managing a platform instance.
    /// Since platforms will be implemented as plug-in components the
    /// child class will have to implement the scanning logic
    /// </summary>
    public abstract class CPlatform
    {
        protected readonly string m_name;
        protected readonly string m_description;
        protected readonly string m_path;

        protected int    m_id;
        protected bool   m_isActive;
        protected Dictionary<string, HashSet<GameObject>> m_gameDictionary;

        #region Properties

        /// <summary>
        /// Platform unique name
        /// </summary>
        public string   Name        { get { return m_name; } }

        /// <summary>
        /// Platform description
        /// </summary>
        public string   Description { get { return m_description; } }

        /// <summary>
        /// Path to the platform root directory
        /// </summary>
        public string   Path        { get { return m_path; } }

        /// <summary>
        /// The PlatformID database primary key getter and setter
        /// </summary>
        public int      ID
        {
            get { return m_id; }
            set { m_id = value; }
        }

        /// <summary>
        /// IsActive flag getter and setter
        /// </summary>
        public bool IsActive
        {
            get { return m_isActive; }
            set { m_isActive = value; }
        }

        /// <summary>
        /// Game dictionary, grouped by Game's Group property
        /// </summary>
        public Dictionary<string, HashSet<GameObject>> Games { get { return m_gameDictionary; } }

        /// <summary>
        /// Retrieve specific group of games
        /// </summary>
        /// <param name="group">The group name</param>
        /// <returns>HashSet of GameObject types</returns>
        public HashSet<GameObject> this[string group]
        {
            get
            {
                if(!m_gameDictionary.ContainsKey(group))
                {
                    m_gameDictionary[group] = new HashSet<GameObject>();
                }
                return m_gameDictionary[group];
            }
        }

        #endregion Properties

        /// <summary>
        /// Base constructor.
        /// </summary>
        /// <param name="id">The platform ID</param>
        /// <param name="name">The platform name</param>
        /// <param name="description">The platform description</param>
        /// <param name="path">The path to platform directory</param>
        /// <param name="isActive">IsActive flag</param>
        public CPlatform(int id, string name, string description, string path, bool isActive)
        {
            m_id            = id;
            m_name          = name;
            m_description   = description;
            m_path          = path;
            m_isActive      = isActive;
            m_gameDictionary = new Dictionary<string, HashSet<GameObject>>();
        }

        /// <summary>
        /// Comparison function.
        /// Compare this platform with another, based on ID property
        /// </summary>
        /// <param name="other">The other platform object</param>
        /// <returns>True if this.ID > other.ID</returns>
        public bool SortByID(CPlatform other)
        {
            return this.ID > other.ID;
        }

        /// <summary>
        /// Comparison function.
        /// Compare this platform with another, based on number of games
        /// </summary>
        /// <param name="other">The other platform object</param>
        /// <returns>True if this.GameCount > other.GameCount</returns>
        public bool SortByGameCount(CPlatform other)
        {
            return this.Games.Count > other.Games.Count;
        }

        /// <summary>
        /// Insert the gamge object into the dictionary
        /// </summary>
        /// <param name="group">The group to store the game in</param>
        /// <param name="game">The GameObject instance</param>
        public void AddGame(string group, GameObject game)
        {
            this[group].Add(game);
        }

        /// <summary>
        /// Save newly found games into the database and remove uninstalled games
        /// </summary>
        /// <param name="newGames">The HashSet containing new games</param>
        protected virtual void SaveNewGames(HashSet<GameObject> newGames)
        {
            HashSet<GameObject> allGames = CGameSQL.LoadPlatformGames(this.ID);
            HashSet<GameObject> gamesToAdd = new HashSet<GameObject>(newGames);
            HashSet<GameObject> gamesToRemove = new HashSet<GameObject>(allGames);

            gamesToAdd.ExceptWith(allGames);
            gamesToRemove.ExceptWith(newGames);

            foreach(GameObject game in gamesToAdd)
            {
                m_gameDictionary[game.Group].Add(game);
                CGameSQL.InsertGame(game);
            }
            foreach(GameObject game in gamesToRemove)
            {
                m_gameDictionary[game.Group].Remove(game);
                CGameSQL.DeleteGame(game.ID);
            }
        }

        #region Abstract functions

        /// <summary>
        /// Search for platform games
        /// </summary>
        /// <returns>HashSet of GameObjects</returns>
        public abstract HashSet<GameObject> GameScanner();

        /// <summary>
        /// Launch or activate specified game
        /// </summary>
        /// <param name="game">The game to launch</param>
        /// <returns>True on success</returns>
        public abstract bool GameLaunch(GameObject game);

        #endregion Abstract functions
    }

    /// <summary>
    /// Platform factory interface for creating instances of T (derived classes)
    /// </summary>
    /// <typeparam name="T">Type that inherits from CPlatform abstract class</typeparam>
    public interface IPlatformFactory<T> where T : CPlatform
    {
        /// <summary>
        /// Create instance of child CPlatform with exisitng data
        /// </summary>
        /// <param name="id">The platform ID</param>
        /// <param name="name">The platform name</param>
        /// <param name="description">The platform description</param>
        /// <param name="path">The path to platform directory</param>
        /// <param name="isActive">IsActive flag</param>
        /// <returns>Instance of T (child of CPlatform)</returns>
        T CreateFromDatabase(int id, string name, string description, string path, bool isActive);

        /// <summary>
        /// Create default instance of child CPlatform
        /// </summary>
        /// <returns>Instance of T (child of CPlatform)</returns>
        T CreateDefault();

        /// <summary>
        /// Return the name of the platform.
        /// </summary>
        /// <returns>Platform name string</returns>
        string GetPlatformName();
    }
}
