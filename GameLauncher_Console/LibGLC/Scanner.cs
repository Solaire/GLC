using System;
using System.Collections.Generic;
using LibGLC.PlatformReaders;

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

			// The CBasePlatformScanner events are static, so it doesn't matter which instance we use to link
			// Can't use the base class since it's a generic
			CSteamScanner.PlatformStarted += OnPlatformStarted;
			CSteamScanner.GameFound		  += OnGameFound;
			ScannerFinished				  += OnScanFinish;
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

			// TEST code
			/*
            string[] test = new string[]
            {
                "Scanning, Steam",
                "Found - Dark souls 3",
                "Found - Divinity 2",
                "Found - Tekken 7",
                "Found - Civ V",
                "Found - MK 11",
                "Found - Overcooked 2",
                "Found - Quake",

                "Scanning, GOG",
                "Found - Gothic 2 NK",
                "Found - Heroes 3",
                "Found - Witcher 3",
                "Found - Baldur's gate",
                "Found - Project warlock",

                "Scanning, Uplay",
                "Found - Assassin's Creed",
                "Found - Far Cry 4",
                "Found - Ghost Recon - wildlands",
                "Found - Settlers",
                "Found - Heroes of might and magic",
            };

            int platformsFound = 0;
            int gamesFound = 0;

            for(int i = 0; i < test.Length; i++)
            {
                if(test[i].Contains("Scanning, "))
                {
                    OnPlatformStart(++platformsFound, test[i]);
                }
                else if(test[i].Contains("Found - "))
                {
                    OnNewGameFound(++gamesFound, new CGame.GameObject(0, "test", test[i], "test", "test", "test"));
                }
                Thread.Sleep(150);
            }
            */
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
				case CPlatform.GamePlatform.Steam:		return CSteamScanner.		Instance.GetGames(false, false); // FindSteamGames();
				case CPlatform.GamePlatform.GOG:		return CGogScanner.			Instance.GetGames(false, false); // FindGogGames();
				case CPlatform.GamePlatform.Uplay:		return CUbisoftScanner.		Instance.GetGames(false, false); // FindUbisoftGames();
				case CPlatform.GamePlatform.Origin:		return COriginScanner.		Instance.GetGames(false, false); // FindOriginGames();	
				case CPlatform.GamePlatform.Epic:		return CEpicGamesScanner.	Instance.GetGames(false, false); // FindEpicGames();		
				case CPlatform.GamePlatform.Bethesda:	return CBethesdaScanner.	Instance.GetGames(false, false); // FindBethesdaGames();	
				case CPlatform.GamePlatform.Battlenet:	return CBattlenetScanner.	Instance.GetGames(false, false); // FindBattlenetGames();	
				case CPlatform.GamePlatform.Rockstar:	return CRockstarScanner.	Instance.GetGames(false, false); // FindRockstarGames();	
				case CPlatform.GamePlatform.Amazon:		return CAmazonScanner.		Instance.GetGames(false, false); // FindAmazonGames();	
				case CPlatform.GamePlatform.BigFish:	return CBigFishScanner.		Instance.GetGames(false, false); // FindBigFishGames();	
				case CPlatform.GamePlatform.Arc:		return CArcScanner.			Instance.GetGames(false, false); // FindArcGames();		
				case CPlatform.GamePlatform.Itch:		return CItchScanner.		Instance.GetGames(false, false); // FindItchGames();		
				case CPlatform.GamePlatform.Paradox:	return CParadoxScanner.		Instance.GetGames(false, false); // FindParadoxGames();	
				case CPlatform.GamePlatform.Plarium:	return CPlariumScanner.		Instance.GetGames(false, false); // FindPlariumGames();	
				//case CPlatform.GamePlatform.Twitch:	return CTwitchParser.		Instance.GetGames(false, false); // FindTwitchGames();	
				case CPlatform.GamePlatform.Wargaming:	return CWargamingScanner.	Instance.GetGames(false, false); // FindWargamingGames();	
				case CPlatform.GamePlatform.IGClient:	return CIndiegalaScanner.	Instance.GetGames(false, false); // FindIndiegalaGames();	

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
			if(!ScanSinglePlatform(m_currentPlatform++))
			{
				m_currentPlatform = CPlatform.GamePlatform.Unknown;
				return false;
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
				ScannerFinished.Invoke(new EventArgs());
			}
			m_currentPlatform = 0;
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
}