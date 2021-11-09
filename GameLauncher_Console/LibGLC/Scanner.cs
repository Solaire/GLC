using System;
using LibGLC.PlatformReaders;
using Logger;

namespace LibGLC
{
	/// <summary>
	/// Main handler class for finding games
	/// </summary>
	public sealed class CScanner
	{
		private CPlatform.GamePlatform m_currentPlatform;
		public event Action<EventArgs> ScannerFinished;

		/// <summary>
		/// Contructor. Create event handler links between the scanners and the calling class
		/// </summary>
		/// <param name="OnPlatformStarted">Delegate to the method handling new platform scanning</param>
		/// <param name="OnGameFound">Delegate to the method handling new game found</param>
		/// <param name="OnScanFinish">Delegate to the method handling scan completion</param>
		public CScanner(Action<CNewPlatformEventArgs> OnPlatformStarted, Action<CNewGameFoundEventArgs> OnGameFound, Action<EventArgs> OnScanFinish)
		{
			m_currentPlatform = 0;

			CEventDispatcher.PlatformStarted += OnPlatformStarted;
			CEventDispatcher.GameFound		 += OnGameFound;
			ScannerFinished					 += OnScanFinish;
		}

		/// <summary>
		/// Look for games in the specified platform
		/// </summary>
		/// <param name="target">The target platform, if empty or "all", scan all available platforms</param>
		public void ScanForGames(string target)
		{
			if(target.Length == 0 || target.ToLower() == "all")
			{
				ScanAllPlatforms();
			}
			else
			{
				ScanSinglePlatform(target.ToLower());
			}
			ScannerFinish();
		}

		/// <summary>
		/// Find the platform enumerator from the string and attempt to scan
		/// </summary>
		/// <param name="platform">The platform string</param>
		/// <returns>True if scan was successful, otherwise false</returns>
		private bool ScanSinglePlatform(string platform)
		{
			CPlatform.GamePlatform search = CExtensions.GetValueFromDescription(platform, CPlatform.GamePlatform.Unknown);
			return ScanSinglePlatform(search);
		}

		/// <summary>
		/// Scan a platform based on the specified enumerator
		/// </summary>
		/// <param name="platform">The platform enumerator</param>
		/// <returns>True if scan was successful, otherwise false</returns>
		private bool ScanSinglePlatform(CPlatform.GamePlatform platform)
		{
			switch(platform)
			{
				case CPlatform.GamePlatform.Steam:		return CSteamScanner.		Instance.GetGames(false, false);
				case CPlatform.GamePlatform.GOG:		return CGogScanner.			Instance.GetGames(false, false);
				case CPlatform.GamePlatform.Uplay:		return CUbisoftScanner.		Instance.GetGames(false, false);
				case CPlatform.GamePlatform.Origin:		return COriginScanner.		Instance.GetGames(false, false);
				case CPlatform.GamePlatform.Epic:		return CEpicGamesScanner.	Instance.GetGames(false, false);
				case CPlatform.GamePlatform.Bethesda:	return CBethesdaScanner.	Instance.GetGames(false, false);
				case CPlatform.GamePlatform.Battlenet:	return CBattlenetScanner.	Instance.GetGames(false, false);
				case CPlatform.GamePlatform.Rockstar:	return CRockstarScanner.	Instance.GetGames(false, false);
				case CPlatform.GamePlatform.Amazon:		return CAmazonScanner.		Instance.GetGames(false, false);
				case CPlatform.GamePlatform.BigFish:	return CBigFishScanner.		Instance.GetGames(false, false);
				case CPlatform.GamePlatform.Arc:		return CArcScanner.			Instance.GetGames(false, false);
				case CPlatform.GamePlatform.Itch:		return CItchScanner.		Instance.GetGames(false, false);
				case CPlatform.GamePlatform.Paradox:	return CParadoxScanner.		Instance.GetGames(false, false);
				case CPlatform.GamePlatform.Plarium:	return CPlariumScanner.		Instance.GetGames(false, false);
				//case CPlatform.GamePlatform.Twitch:	return CTwitchParser.		Instance.GetGames(false, false);
				case CPlatform.GamePlatform.Wargaming:	return CWargamingScanner.	Instance.GetGames(false, false);
				case CPlatform.GamePlatform.IGClient:	return CIndiegalaScanner.	Instance.GetGames(false, false);

				case CPlatform.GamePlatform.Unknown:
				default:
					return false;
			}
		}

		/// <summary>
		/// Scan the current platform enum and, if successful, increment the enum for next iteration
		/// </summary>
		/// <returns>True if scan was successful, otherwise false</returns>
		private bool ScanAllPlatforms()
		{
			if(m_currentPlatform < 0)
			{
				return false;
			}

			int platformCount = Enum.GetNames(typeof(CPlatform.GamePlatform)).Length - 1;
			m_currentPlatform = CPlatform.GamePlatform.Unknown;
			while((int)m_currentPlatform++ < platformCount)
            {
				ScanSinglePlatform(m_currentPlatform);
			}

			return true;
		}

		/// <summary>
		/// Raise scanning finished event
		/// </summary>
		private void ScannerFinish()
		{
			if(ScannerFinished != null)
			{
				ScannerFinished.Invoke(EventArgs.Empty);
			}
			m_currentPlatform = 0;
			CEventDispatcher.Unsubscribe(); // Avoid memory leaks
		}
	}
	
	/// <summary>
	/// Struct containing raw game information retrieved by the scanners
	/// </summary>
	public struct RawGameData
	{
		public string m_strID;
		public string m_strTitle;
		public string m_strLaunch;
		public string m_strIcon;
		public string m_strUninstall;
		public string m_strAlias;
		public bool m_bInstalled;
		public string m_strPlatform;

		public RawGameData(string strID, string strTitle, string strLaunch, string strIconPath, string strUninstall, string strAlias, bool bInstalled, string strPlatform)
		{
			m_strID = strID;
			m_strTitle = strTitle;
			m_strLaunch = strLaunch;
			m_strIcon = strIconPath;
			m_strUninstall = strUninstall;
			m_strAlias = strAlias;
			m_bInstalled = bInstalled;
			m_strPlatform = strPlatform;
		}
	}

	/// <summary>
	/// Event argument class containing platform data as string
	/// </summary>
	public class CNewPlatformEventArgs : EventArgs
	{
		public string Value { get; }

		public CNewPlatformEventArgs(string value)
		{
			Value = value;
		}
	}

	/// <summary>
	/// Event argument class containing game data as a structure
	/// </summary>
	public class CNewGameFoundEventArgs : EventArgs
	{
		public RawGameData Value { get; }

		public CNewGameFoundEventArgs(RawGameData value)
		{
			Value = value;
		}
	}

	/// <summary>
	/// Static class for dispatching events created to omit requiring
	/// all singleton-children from subscribing their events
	/// </summary>
	internal static class CEventDispatcher
	{
		public static event Action<CNewPlatformEventArgs>  PlatformStarted;
		public static event Action<CNewGameFoundEventArgs> GameFound;

		/// <summary>
		/// Raise new platform event to subscriber
		/// </summary>
		/// <param name="platformName">Name of the platform</param>
		public static void OnPlatformStarted(string platformName)
		{
			if(PlatformStarted != null && platformName.Length > 0)
			{
				PlatformStarted.Invoke(new CNewPlatformEventArgs(platformName));
				CLogger.LogInfo("Scanning platform: {0}", platformName);
			}
		}

		/// <summary>
		/// Rase new game found event to subscriber
		/// </summary>
		/// <param name="gameData">The game data structure</param>
		public static void OnGameFound(RawGameData gameData)
		{
			if(GameFound != null)
			{
				GameFound.Invoke(new CNewGameFoundEventArgs(gameData));
				CLogger.LogInfo("Found game: {0}", gameData.m_strTitle);
			}
		}

		/// <summary>
		/// Unsubscribe from all event handlers
		/// </summary>
		public static void Unsubscribe()
        {
			if(PlatformStarted != null)
            {
				Delegate[] subArr = PlatformStarted.GetInvocationList();
				foreach(Delegate sub in subArr)
                {
					PlatformStarted -= (sub as Action<CNewPlatformEventArgs>);
                }
            }
			if(GameFound != null)
			{
				Delegate[] subArr = GameFound.GetInvocationList();
				foreach(Delegate sub in subArr)
				{
					GameFound -= (sub as Action<CNewGameFoundEventArgs>);
				}
			}
		}
	}
}