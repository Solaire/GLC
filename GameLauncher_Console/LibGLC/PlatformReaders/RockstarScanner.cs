namespace LibGLC.PlatformReaders
{
    /// <summary>
    /// Scanner for Rockstar games
    /// </summary>
    public sealed class CRockstarScanner : CBasePlatformScanner<CRockstarScanner>
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
