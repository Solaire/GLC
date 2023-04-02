namespace core_2.Platform.SpecialPlatform
{
    /// <summary>
    /// Enum representing the ID values of special platforms.
    /// <br></br>
    /// A special platform is a built-in implementation of <see cref="BasePlatform"/>
    /// which does not have a external data source, but instead aggregates <see cref="Game.Game"/>
    /// from platform extensions.
    /// <br></br>
    /// Since regular platforms are stored with positive non-zero IDs, special platforms
    /// MUST use negative IDs.
    /// </summary>
    internal enum SpecialPlatformType
    {
        /// <summary>
        /// Game search results.
        /// </summary
        Search = -1,

        /// <summary>
        /// <see cref="Game.Game"/> flagged as 'Favourite'
        /// </summary>
        Favourites = -2,
    }
}
