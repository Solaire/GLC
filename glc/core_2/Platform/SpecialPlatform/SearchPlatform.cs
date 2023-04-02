namespace core_2.Platform.SpecialPlatform
{
    internal class SearchPlatform : InternalPlatform
    {
        /// <summary>
        /// Map containing lists of <see cref="Game.Game"/>, grouped
        /// by the search term
        /// </summary>
        internal Dictionary<string, List<Game.Game>> GameMap
        {
            get;
            private set;
        }

        /// <summary>
        /// Index accessor.
        /// <param name="searchTerm">The search term</param>
        /// <returns>List of games matching the search term, or <c>null</c> if not present</returns>
        internal List<Game.Game> ? this[string searchTerm]
        {
            get => (GameMap.ContainsKey(searchTerm)) ? GameMap[searchTerm] : null;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        internal SearchPlatform()
        {
            GameMap = new Dictionary<string, List<Game.Game>>();
        }

        /// <summary>
        /// Add games to the search map, replacing existing list if exists
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <param name="games">List of games matching the search</param>
        internal void AddSearchTerm(string searchTerm, List<Game.Game> games)
        {
            GameMap[searchTerm] = games;
        }
    }
}
