using System;
using System.Collections.Generic;

using core.Game;

namespace core.Platform
{
    public class CBasicPlatform : IDataNode
    {
        protected readonly string m_name;
        protected readonly string m_description;
        protected readonly string m_path;

        protected int    m_id;
        protected bool   m_isEnabled;

        /// <summary>
        /// Platform unique name
        /// </summary>
        public string Name { get { return m_name; } }

        /// <summary>
        /// Platform description
        /// </summary>
        public string Description { get { return m_description; } }

        /// <summary>
        /// Path to the platform root directory
        /// </summary>
        public string Path { get { return m_path; } }

        /// <summary>
        /// Check if this is a special platform
        /// </summary>
        public bool   IsSpecialPlatform { get { return m_id < 0; } }

        /// <summary>
        /// The PlatformID database primary key getter and setter
        /// </summary>
        public int PrimaryKey
        {
            get { return m_id; }
            set { m_id = value; }
        }

        /// <summary>
        /// IsActive flag getter and setter
        /// </summary>
        public bool IsEnabled
        {
            get { return m_isEnabled; }
            set { m_isEnabled = value; }
        }

        public CBasicPlatform(int id, string name, string description, string path, bool isEnabled)
        {
            m_id = id;
            m_name = name;
            m_description = description;
            m_path = path;
            m_isEnabled = isEnabled;
        }

        public CBasicPlatform(CPlatformSQL.CQryReadPlatform qry)
        {
            m_id            = qry.PlatformID;
            m_name          = qry.Name;
            m_description   = qry.Description;
            m_path          = qry.Path;
            m_isEnabled     = qry.IsActive;
        }

        /// <summary>
        /// Comparison function.
        /// Compare this platform with another, based on ID property
        /// </summary>
        /// <param name="other">The other platform object</param>
        /// <returns>True if this.ID > other.ID</returns>
        public bool SortByID(CPlatform other)
        {
            return this.PrimaryKey > other.PrimaryKey;
        }
    }

    /// <summary>
    /// Abstract base for managing a platform instance.
    /// Since platforms will be implemented as plug-in components the
    /// child class will have to implement the scanning logic
    /// </summary>
    public abstract class CPlatform : CBasicPlatform
    {
        protected Dictionary<string, HashSet<GameObject>> m_gameDictionary;

        #region Properties

        /// <summary>
        /// Game dictionary, grouped by Game's Tag property
        /// </summary>
        public Dictionary<string, HashSet<GameObject>> Games { get { return m_gameDictionary; } }

        /// <summary>
        /// Retrieve games with specific tag
        /// </summary>
        /// <param name="tag">The tag name</param>
        /// <returns>HashSet of GameObject types</returns>
        public HashSet<GameObject> this[string tag]
        {
            get
            {
                if(!m_gameDictionary.ContainsKey(tag))
                {
                    m_gameDictionary[tag] = new HashSet<GameObject>();
                }
                return m_gameDictionary[tag];
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
            : base(id, name, description, path, isActive)
        {
            m_gameDictionary = new Dictionary<string, HashSet<GameObject>>();
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
        /// <param name="tag">The tag to store the game in</param>
        /// <param name="game">The GameObject instance</param>
        public void AddGame(string tag, GameObject game)
        {
            this[tag].Add(game);
        }

        /// <summary>
        /// Save newly found games into the database and remove uninstalled games
        /// </summary>
        /// <param name="newGames">The HashSet containing new games</param>
        protected virtual void SaveNewGames(HashSet<GameObject> newGames)
        {
            HashSet<GameObject> allGames = CGameSQL.LoadPlatformGames(this.PrimaryKey);
            HashSet<GameObject> gamesToAdd = new HashSet<GameObject>(newGames);
            HashSet<GameObject> gamesToRemove = new HashSet<GameObject>(allGames);

            gamesToAdd.ExceptWith(allGames);
            gamesToRemove.ExceptWith(newGames);

            foreach(GameObject game in gamesToAdd)
            {
                m_gameDictionary[game.Tag].Add(game);
                CGameSQL.InsertGame(game);
            }
            foreach(GameObject game in gamesToRemove)
            {
                m_gameDictionary[game.Tag].Remove(game);
                CGameSQL.DeleteGame(game.ID);
            }
        }

        #region Abstract functions

        /// <summary>
        /// Scan for installed games
        /// </summary>
        /// <returns>HashSet containing installed game objects</returns>
        public abstract HashSet<GameObject> GetInstalledGames();

        /// <summary>
        /// Scan for non-installed games
        /// </summary>
        /// <returns>HashSet containing non-installed game objects</returns>
        public abstract HashSet<GameObject> GetNonInstalledGames();

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
    public abstract class CPlatformFactory<T> where T : CPlatform
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
        public abstract T CreateFromDatabase(int id, string name, string description, string path, bool isActive);

        /// <summary>
        /// Create default instance of child CPlatform
        /// </summary>
        /// <returns>Instance of T (child of CPlatform)</returns>
        public abstract T CreateDefault();

        /// <summary>
        /// Return the name of the platform.
        /// </summary>
        /// <returns>Platform name string</returns>
        public abstract string GetPlatformName();
    }
}
