namespace LibGLC.PlatformReaders
{
    /// <summary>
    /// Scanner for Plarium Play
    /// </summary>
    public sealed class CPlariumScanner : CBasePlatformScanner<CPlariumScanner>
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