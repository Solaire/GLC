using core.Game;
using core.Platform;
using System;
using System.Collections.Generic;

namespace BasePlatformExtension
{
	public abstract class CBasePlatformScanner
	{
		protected const string NODE64_REG              = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
		protected const string NODE32_REG              = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
		protected const string GAME_DISPLAY_NAME       = "DisplayName";
		protected const string GAME_DISPLAY_ICON       = "DisplayIcon";
		protected const string GAME_INSTALL_PATH       = "InstallPath";
		protected const string GAME_INSTALL_LOCATION   = "InstallLocation";
		protected const string GAME_UNINSTALL_STRING   = "UninstallString";
		protected const string INSTALLSHIELD           = "_is1";

        protected readonly int m_platformID;

        public CBasePlatformScanner(int platformID)
        {
            m_platformID = platformID;
        }

        public abstract HashSet<GameObject> GetInstalledGames(bool expensiveIcons);
		public abstract HashSet<GameObject> GetNonInstalledGames(bool expensiveIcons);
	}

	public abstract class CBasePlatformExtension<T> : CPlatform where T : CBasePlatformScanner//, new()
    {
        protected readonly CBasePlatformScanner m_scanner;

        public CBasePlatformExtension(int id, string name, string description, string path, bool isActive)
            : base(id, name, description, path, isActive)
        {
            m_scanner = (T)Activator.CreateInstance(typeof(T), new object[] { id });
            //m_scanner = new T();
        }

        public override HashSet<GameObject> GetInstalledGames()
        {
            HashSet<GameObject> result = new HashSet<GameObject>();
            m_scanner.GetInstalledGames(false);
            return result;
        }

        public override HashSet<GameObject> GetNonInstalledGames()
        {
            HashSet<GameObject> result = new HashSet<GameObject>();
            m_scanner.GetNonInstalledGames(false);
            return result;
        }
    }
}
