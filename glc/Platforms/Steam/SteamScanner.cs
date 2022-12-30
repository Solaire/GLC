using BasePlatformExtension;
using core;

namespace Steam
{
    /// <summary>
    /// Steam scanner implementation
    /// </summary>
    public sealed class CSteamScanner : CBasePlatformScanner
    {
        private const string STEAM_NAME = "Steam Platform";

        public CSteamScanner(int platformID)
            : base(platformID)
        {
        }

        /// <summary>
        /// Generate 10 installed game objects
        /// </summary>
        /// <param name="expensiveIcons">TODO: unused</param>
        /// <returns>HashSet with 10 generated game objects</returns>
        public override HashSet<Game> GetInstalledGames(bool expensiveIcons)
        {
            HashSet<Game> games = new HashSet<Game>();
            for(int i = 0; i < 10; i++)
            {
                string title  = $"Installed ref game {i}";
                string id     = $"ref_game_{i}";
                string alias  = $"IREF{i}";
                string launch = $"START_REF_{i}";

                games.Add(new Game(title, m_platformID, id, alias, launch, ""));
            }
            return games;
        }

        /// <summary>
        /// Generate 10 non-installed game objects
        /// </summary>
        /// <param name="expensiveIcons">TODO: unused</param>
        /// <returns>HashSet with 10 generated game objects</returns>
        public override HashSet<Game> GetNonInstalledGames(bool expensiveIcons)
        {
            HashSet<Game> games = new HashSet<Game>();
            for(int i = 0; i < 10; i++)
            {
                string title  = $"Non-installed ref game {i}";
                string id     = $"ref_game_{i}";

                games.Add(new Game(title, m_platformID, id, "", "", ""));
            }
            return games;
        }
    }
}
