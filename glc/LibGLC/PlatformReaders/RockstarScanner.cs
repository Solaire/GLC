namespace LibGLC.PlatformReaders
{
    /// <summary>
    /// Scanner for Rockstar games
    /// </summary>
    public sealed class CRockstarScanner : CBasePlatformScanner<CRockstarScanner>
    {
        private CRockstarScanner()
        {
            m_platformName = CExtensions.GetDescription(CPlatform.GamePlatform.Rockstar);
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
