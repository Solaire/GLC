using core.DataAccess;

namespace core.Platform
{
    /// <summary>
    /// Abstract base for managing a platform instance.
    /// </summary>
    public abstract class CPlatform : CBasicPlatform
    {
        /// <summary>
        /// Game dictionary, grouped by the Tag property
        /// </summary>
        public Dictionary<string, HashSet<Game>> Games
        {
            get;
            protected set;
        }

        /// <summary>
        /// Retrieve games with specific tag
        /// </summary>
        /// <param name="tag">The tag name</param>
        /// <returns>HashSet of GameObject types</returns>
        public HashSet<Game> this[string tag]
        {
            get
            {
                if(!Games.ContainsKey(tag))
                {
                    Games[tag] = new HashSet<Game>();
                }
                return Games[tag];
            }
        }

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
            Games = new Dictionary<string, HashSet<Game>>();
        }

        /// <summary>
        /// Comparison function.
        /// Compare this platform with another, based on number of games
        /// </summary>
        /// <param name="other">The other platform object</param>
        /// <returns>True if this.GameCount > other.GameCount</returns>
        public bool SortByGameCount(CPlatform other)
        {
            return Games.Count > other.Games.Count;
        }

        /// <summary>
        /// Insert the gamge object into the dictionary
        /// </summary>
        /// <param name="tag">The tag to store the game in</param>
        /// <param name="game">The GameObject instance</param>
        public void AddGame(string tag, Game game)
        {
            this[tag].Add(game);
        }

        /// <summary>
        /// Save newly found games into the database and remove uninstalled games
        /// </summary>
        /// <param name="newGames">The HashSet containing new games</param>
        protected virtual void SaveNewGames(HashSet<Game> newGames)
        {
            HashSet<Game> allGames = CGameSQL.LoadPlatformGames(PrimaryKey);
            HashSet<Game> gamesToAdd = new HashSet<Game>(newGames);
            HashSet<Game> gamesToRemove = new HashSet<Game>(allGames);

            gamesToAdd.ExceptWith(allGames);
            gamesToRemove.ExceptWith(newGames);

            foreach(Game game in gamesToAdd)
            {
                Games[game.Tag].Add(game);
                CGameSQL.InsertGame(game);
            }
            foreach(Game game in gamesToRemove)
            {
                Games[game.Tag].Remove(game);
                CGameSQL.DeleteGame(game.ID);
            }
        }

        #region Abstract functions

        /// <summary>
        /// Scan for installed games
        /// </summary>
        /// <returns>HashSet containing installed game objects</returns>
        public abstract HashSet<Game> GetInstalledGames();

        /// <summary>
        /// Scan for non-installed games
        /// </summary>
        /// <returns>HashSet containing non-installed game objects</returns>
        public abstract HashSet<Game> GetNonInstalledGames();

        /// <summary>
        /// Launch or activate specified game
        /// </summary>
        /// <param name="game">The game to launch</param>
        /// <returns>True on success</returns>
        public abstract bool GameLaunch(Game game);

        #endregion Abstract functions
    }
}
