namespace LibGLC.PlatformReaders
{
    /// <summary>
    /// Scanner for Wargaming.net (World of Tanks)
    /// </summary>
    public sealed class CWargamingScanner : CBasePlatformScanner<CWargamingScanner>
    {
        protected override bool GetInstalledGames(bool expensiveIcons)
        {
            return false;
        }

        protected override bool GetNonInstalledGames(bool expensiveIcons)
        {
            return false;
        }
    }
}
