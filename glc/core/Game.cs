using core.DataAccess;

namespace core
{
    /// <summary>
    /// The database game object.
    /// </summary>
    public class Game : IEquatable<Game>
    {
        #region Properties

        /// <summary>
        /// GameID field - primary key
        /// </summary>
        public int ID               { get; }

        /// <summary>
        /// PlatformFK field
        /// </summary>
        public int PlatformFK       { get; }

        /// <summary>
        /// Unique game identifier used by the platform
        /// eg. GOG_[game_number]
        /// </summary>
        public string Identifier    { get; }

        /// <summary>
        /// Full game Title
        /// </summary>
        public string Title         { get; }

        /// <summary>
        /// Game alias (acronym, partial name, etc)
        /// </summary>
        public string Alias         { get; }

        /// <summary>
        /// Launch command and/or parameter
        /// eg. steam://rungameid/[game_id]
        /// </summary>
        public string Launch        { get; }

        /// <summary>
        /// Frequency at which the game is launched.
        /// Implemented as a "leaky-bucket" which decreases by 10%
        /// when other games are selected instead of this one
        /// </summary>
        public double Frequency     { get; private set; }

        /// <summary>
        /// Favourite flag
        /// </summary>
        public bool IsFavourite     { get; private set; }

        /// <summary>
        /// Flag which will hide the game
        /// </summary>
        public bool IsHidden        { get; private set; }

        /// <summary>
        /// Custom property which allows games to be grouped
        /// eg. installed and non-installed
        /// </summary>
        public string Tag           { get; private set; }

        /// <summary>
        /// Game icon bytearray
        /// </summary>
        public string Icon          { get; } //TODO: Image/bytearray type

        #endregion Properties

        /// <summary>
        /// Determine the equality of this instance and another GameObject
        /// using the Identifier property
        /// </summary>
        /// <param name="other">The GameObject instance to compare</param>
        /// <returns>True if this instance and other have the same Identifier field</returns>
        public bool Equals(Game other)
        {
            return (other == null) ? false : Identifier == other.Identifier;
        }

        /// <summary>
        /// Constructor.
        /// Create a game object with minimal necessary data.
        /// Should be used for new game entries found with the platform's scanner
        /// </summary>
        /// <param name="title">The title</param>
        /// <param name="platformFK">PlatformID</param>
        /// <param name="identifier">The unique identifier</param>
        /// <param name="alias">The alias</param>
        /// <param name="launch">The launch command</param>
        /// <param name="tag">The game's tag</param>
        public Game(string title, int platformFK, string identifier, string alias, string launch, string tag)
        {
            PlatformFK = platformFK;
            Identifier = identifier;
            Title      = title;
            Alias      = alias;
            Launch     = launch;
            Tag        = tag;

            ID         = 0;
            Frequency  = 0.0;
            IsFavourite= false;
            IsHidden   = false;
            Icon       = ""; // TODO
        }

        /// <summary>
        /// Constructor.
        /// Create a full game object from the database entry.
        /// </summary>
        /// <param name="qry">The game query</param>
        public Game(CGameSQL.CQryReadGame qry)
        {
            ID         = qry.GameID;
            PlatformFK = qry.PlatformFK;
            Identifier = qry.Identifier;
            Title      = qry.Title;
            Alias      = qry.Alias;
            Launch     = qry.Launch;
            Frequency  = qry.Frequency;
            IsFavourite= qry.IsFavourite;
            IsHidden   = qry.IsHidden;
            Icon       = qry.Icon;
            Tag        = qry.Tag;
        }

        // TODO: Remove later
        public Game(CGameSQL.CQryGameFuzzySearch qry)
        {
            ID         = qry.GameID;
            PlatformFK = qry.PlatformFK;
            Identifier = qry.Identifier;
            Title      = qry.Title;
            Alias      = qry.Alias;
            Launch     = qry.Launch;
            Frequency  = qry.Frequency;
            IsFavourite= qry.IsFavourite;
            IsHidden   = qry.IsHidden;
            Icon       = qry.Icon;
            Tag        = qry.Tag;
        }

        /// <summary>
        /// Toggle the favourite flag
        /// </summary>
        public void ToggleFavourite()
        {
            SetFavourite(!IsFavourite);
        }

        /// <summary>
        /// Set the game's favourite flag and update database row
        /// </summary>
        /// <param name="isFavourite">The new favourite flag</param>
        private void SetFavourite(bool isFavourite)
        {
            if(IsFavourite != isFavourite)
            {
                IsFavourite = isFavourite;
                CGameSQL.ToggleFavourite(ID, isFavourite);
            }
        }

        /// <summary>
        /// Set the game's hidden flag and update database row
        /// </summary>
        /// <param name="isHidden">The new hidden flag</param>
        public void SetHidden(bool isHidden)
        {
            if(IsHidden != isHidden)
            {
                IsHidden = isHidden;
                CGameSQL.ToggleHidden(ID, isHidden);
            }
        }

        /// <summary>
        /// Update the game's frequency incrementing the
        /// value by 1 or decreasing by 10% and update the database row
        /// </summary>
        /// <param name="isDecimate">If true, decrease value by 10%</param>
        public void UpdateFrequency(bool isDecimate)
        {
            Frequency = (isDecimate) ? Frequency * 0.90 : Frequency + 1;
        }

        public void UpdateRating(int rating)
        {
            // Rating = rating;
            //CGameSQL.UpdateRating(ID, rating);
        }

        public void Update()
        {

        }
    }
}
