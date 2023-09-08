namespace LibGLC.PlatformReaders
{
    /// <summary>
    /// Scanner for Arc games
    /// </summary>
    public sealed class CArcScanner : CBasePlatformScanner<CArcScanner>
    {
        private CArcScanner()
        {
            m_platformName = CExtensions.GetDescription(CPlatform.GamePlatform.Arc);
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
