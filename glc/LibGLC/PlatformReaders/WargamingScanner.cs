namespace LibGLC.PlatformReaders
{
    /// <summary>
    /// Scanner for Wargaming.net (World of Tanks)
    /// </summary>
    public sealed class CWargamingScanner : CBasePlatformScanner<CWargamingScanner>
    {
        private CWargamingScanner()
        {
            m_platformName = CExtensions.GetDescription(CPlatform.GamePlatform.Wargaming);
        }

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
