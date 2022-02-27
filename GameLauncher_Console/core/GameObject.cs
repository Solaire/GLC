namespace core
{
    /// <summary>
    /// Structure containing information about a game
    /// </summary>
    public struct GameObject : System.IEquatable<GameObject>
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
        public string Group         { get; private set; }

        /// <summary>
        /// Game icon bytearray
        /// </summary>
        public string Icon          { get; } //TODO: Image/bytearray type

        #endregion Properties

        #region Comparison functions

        /// <summary>
        /// Compare the instance with another GameObject using the ID property
        /// </summary>
        /// <param name="other">The GameObject instance to compare</param>
        /// <returns>True if this.ID > other.ID</returns>
        public bool CompareByID(GameObject other)
        {
            return this.ID > other.ID;
        }

        /// <summary>
        /// Compare the instance with another GameObject using the IsFavourite property
        /// </summary>
        /// <param name="other">The GameObject instance to compare</param>
        /// <returns>True if instance is favourite and other is not favourite</returns>
        public bool CompareByFavourite(GameObject other)
        {
            return this.IsFavourite && !other.IsFavourite;
        }

        /// <summary>
        /// Compare the instance with another GameObject using the Frequency property
        /// </summary>
        /// <param name="other">The GameObject instance to compare</param>
        /// <returns>True if this.Frequency > other.Frequency</returns>
        public bool CompareByFrequency(GameObject other)
        {
            return this.Frequency > other.Frequency;
        }

        /// <summary>
        /// Determine the equality of this instance and another GameObject
        /// using the Identifier property
        /// </summary>
        /// <param name="other">The GameObject instance to compare</param>
        /// <returns>True if this instance and other have the same Identifier field</returns>
        public bool Equals(GameObject other)
        {
            return this.Identifier == other.Identifier;
        }

        #endregion Comparison functions

        /// <summary>
        /// Constructor.
        /// Create a game object with minimal data.
        /// Should be used for new game entries found with the platform's scanner
        /// </summary>
        /// <param name="title">The title</param>
        /// <param name="platformFK">PlatformID</param>
        /// <param name="identifier">The unique identifier</param>
        /// <param name="alias">The alias</param>
        /// <param name="launch">The launch command</param>
        /// <param name="group">The game's group</param>
        public GameObject(string title, int platformFK, string identifier, string alias, string launch, string group)
        {
            this.PlatformFK = platformFK;
            this.Identifier = identifier;
            this.Title      = title;
            this.Alias      = alias;
            this.Launch     = launch;
            this.Group      = group;

            this.ID         = 0;
            this.Frequency  = 0.0;
            this.IsFavourite= false;
            this.IsHidden   = false;
            this.Icon       = ""; // TODO
        }

        /// <summary>
        /// Constructor.
        /// Create a full game object from the database entry.
        /// </summary>
        /// <param name="qry">The game query</param>
        public GameObject(CGameSQL.CQryReadGame qry)
        {
            this.ID         = qry.GameID;
            this.PlatformFK = qry.PlatformFK;
            this.Identifier = qry.Identifier;
            this.Title      = qry.Title;
            this.Alias      = qry.Alias;
            this.Launch     = qry.Launch;
            this.Frequency  = qry.Frequency;
            this.IsFavourite= qry.IsFavourite;
            this.IsHidden   = qry.IsHidden;
            this.Icon       = qry.Icon;
            this.Group      = qry.Group;
        }

        /// <summary>
        /// Set the game's favourite flag and update database row
        /// </summary>
        /// <param name="isFavourite">The new favourite flag</param>
        public void SetFavourite(bool isFavourite)
        {
            if(this.IsFavourite != isFavourite)
            {
                this.IsFavourite = isFavourite;
                CGameSQL.ToggleFavourite(this.ID, isFavourite);
            }
        }

        /// <summary>
        /// Set the game's hidden flag and update database row
        /// </summary>
        /// <param name="isHidden">The new hidden flag</param>
        public void SetHidden(bool isHidden)
        {
            if(this.IsHidden != isHidden)
            {
                this.IsHidden = isHidden;
                CGameSQL.ToggleHidden(this.ID, isHidden);
            }
        }

        /// <summary>
        /// Update the game's frequency incrementing the
        /// value by 1 or decreasing by 10% and update the database row
        /// </summary>
        /// <param name="isDecimate">If true, decrease value by 10%</param>
        public void UpdateFrequency(bool isDecimate)
        {
            this.Frequency = (isDecimate) ? this.Frequency * 0.90 : this.Frequency + 1;
        }
    }
}
