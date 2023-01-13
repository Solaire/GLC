using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using core_2.Game;

namespace core_2.Platform
{
    /// <summary>
    /// Base class for generic platform
    /// </summary>
    public abstract class BasePlatform : IData
    {
        internal Dictionary<string, List<Game.Game>> m_games = new Dictionary<string, List<Game.Game>>();

        #region IData

        public int ID
        {
            get;
            set;
        }

        public string Name
        {
            get;
            protected set;
        }

        public bool IsEnabled
        {
            get;
            protected set;
        }

        #endregion IData

        #region Properties

        /// <summary>
        /// Platform description
        /// </summary>
        public string Description
        {
            get;
            protected set;
        }

        /// <summary>
        /// Platform's install path
        /// </summary>
        public string Path
        {
            get;
            protected set;
        }

        /// <summary>
        /// Check if the platform has loaded the games
        /// </summary>
        public bool IsLoaded
        {
            get => m_games.Any();
        }

        /// <summary>
        /// Getter for all games
        /// </summary>
        public IEnumerable<Game.Game> AllGames
        {
            get
            {
                HashSet<Game.Game> allGames = new HashSet<Game.Game>();
                foreach (var set in m_games.Values)
                {
                    allGames.UnionWith(set);
                }
                return allGames.ToList();
            }
        }

        /// <summary>
        /// Getter for favourite games
        /// </summary>
        public IEnumerable<Game.Game> Favourites
        {
            get
            {
                HashSet<Game.Game> favourites = new HashSet<Game.Game>();
                foreach(var set in m_games.Values)
                {
                    favourites.UnionWith(set.Where(t => t.IsFavourite));
                }
                return favourites.ToList();
            }
        }

        /// <summary>
        /// Getter for games matching a tag
        /// </summary>
        /// <param name="tag">The tag name</param>
        /// <returns>List of games with specific tag, or empty list</returns>
        public IEnumerable<Game.Game> this[string tag]
        {
            get => (m_games.ContainsKey(tag)) ? m_games[tag].ToList() : new List<Game.Game>();
        }

        #endregion Properties

        #region Abstract methods

        /// <summary>
        /// Scan for installed games
        /// </summary>
        /// <returns>HashSet containing installed game objects</returns>
        public abstract HashSet<Game.Game> GetInstalledGames();

        /// <summary>
        /// Scan for non-installed games
        /// </summary>
        /// <returns>HashSet containing non-installed game objects</returns>
        public abstract HashSet<Game.Game> GetNonInstalledGames();

        /// <summary>
        /// Launch or activate specified game
        /// </summary>
        /// <param name="game">The game to launch</param>
        /// <returns>True on success</returns>
        public abstract bool GameLaunch(Game.Game game);

        #endregion Abstract methods

        protected virtual void SaveNewGames(HashSet<Game.Game> newGames)
        {

        }
    }
}
