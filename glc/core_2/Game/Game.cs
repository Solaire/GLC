using static core_2.DataAccess.GameSQL;

namespace core_2.Game
{
    public class Game : IData
    {
        #region IData

        public int ID
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public bool IsEnabled
        {
            get;
            private set;
        }

        #endregion IData

        #region Properties

        /// <summary>
        /// PlatformFK field
        /// </summary>
        public int PlatformFK { get; private set; }

        /// <summary>
        /// Unique game identifier used by the platform
        /// eg. GOG_[game_number]
        /// </summary>
        public string Identifier { get; private set; }

        /// <summary>
        /// Game alias (acronym, partial name, etc)
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Launch command and/or parameter
        /// eg. steam://rungameid/[game_id]
        /// </summary>
        public string Launch { get; private set; }

        /// <summary>
        /// Frequency at which the game is launched.
        /// Implemented as a "leaky-bucket" which decreases by 10%
        /// when other games are selected instead of this one
        /// </summary>
        public double Frequency { get; private set; }

        /// <summary>
        /// Favourite flag
        /// </summary>
        public bool IsFavourite { get; private set; }

        /// <summary>
        /// Custom property which allows games to be grouped
        /// eg. installed and non-installed
        /// </summary>
        public string Tag { get; private set; }

        /// <summary>
        /// Game icon bytearray
        /// </summary>
        public string Icon { get; private set; } //TODO: Image/bytearray type

        #endregion Properties

        private Game()
        {

        }

        /// <summary>
        /// Create new game entry with minimal data.
        /// </summary>
        /// <param name="name">The game name</param>
        /// <param name="platformFK">The platform ID</param>
        /// <param name="identifier">The game identifier</param>
        /// <param name="alias">The game alias</param>
        /// <param name="launch">The launch command</param>
        /// <param name="tag">Tag associated with the game</param>
        /// <returns>A game object with minimal data</returns>
        public static Game CreateNew(string name, int platformFK, string identifier, string alias, string launch, string tag)
        {
            return new Game()
            {
                Name = name,
                PlatformFK = platformFK,
                Identifier = identifier,
                Alias = alias,
                Launch = launch,
                Tag = tag,

                // Unused
                ID = 0,
                Frequency = 0.0,
                IsFavourite = false,
                IsEnabled = true,
                Icon = "" // TODO
            };
        }

        /// <summary>
        /// Create a game object from database row.
        /// </summary>
        /// <param name="row">A database row</param>
        /// <returns>Game object with database data</returns>
        internal static Game CreateFromDB(QryBaseReadGame row)
        {
            return new Game()
            {
                ID = row.GameID,
                Name = row.Title,
                PlatformFK = row.PlatformFK,
                Identifier = row.Identifier,
                Alias = row.Alias,
                Launch = row.Launch,
                Tag = row.Tag,
                Frequency = row.Frequency,
                IsFavourite = row.IsFavourite,
                IsEnabled = row.IsEnabled,
                Icon = row.Icon // TODO
            };
        }

        public void ToggleFavourite()
        {
            IsFavourite = !IsFavourite;
            // CGameSql.ToggleFavourite(ID, IsFavourite);
        }

        public void UpdateRating(int rating)
        {
            // Rating = rating;
            // CGameSql.UpdateRating(ID, rating);
        }

        public void UpdateFrequency(bool isDecimate)
        {
            Frequency = isDecimate ? Frequency * 0.90 : Frequency + 1;
        }
    }

    public class GameComparer : IEqualityComparer<Game>
    {
        public bool Equals(Game l, Game r)
        {
            return l.Identifier == r.Identifier;
        }

        public int GetHashCode(Game game)
        {
            return game.Identifier.GetHashCode();
        }
    }
}
