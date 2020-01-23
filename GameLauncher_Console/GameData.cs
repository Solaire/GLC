using System;
using System.Collections.Generic;

namespace GameLauncher_Console
{
	/// <summary>
	/// Contains the definition of the game data and its manipulation logic
	/// </summary>
	static public class CGameData
	{
		/// <summary>
		/// Enumerator containing currently supported game platforms
		/// </summary>
		public enum GamePlatform
		{
			Custom		= -1,
			Steam		= 0,
			GOG			= 1,
			Uplay		= 2, 
			Origin		= 3,
			Epic		= 4,
			Bethesda	= 5,
			Battlenet	= 6,
			Rockstar	= 7,
		}

		/// <summary>
		/// Contains information about a game
		/// </summary>
		public class CGame
		{
			private readonly string		  m_strTitle;
			private readonly string		  m_strLaunch;
			private readonly bool		  m_bIsFavourite;
			private readonly GamePlatform m_platfrom;
			//private readonly string		m_strIcon; // Currently not in use

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="strTitle">Title of the game</param>
			/// <param name="strLaunchCommand">Game's launch command</param>
			/// <param name="bIsFavourite">Flag indicating if the game is in the favourite tab</param>
			/// <param name="enumPlatform">Game's platform enumerator</param>
			protected CGame(string strTitle, string strLaunchCommand, bool bIsFavourite, GamePlatform enumPlatform)
			{
				m_strTitle		= strTitle;
				m_strLaunch		= strLaunchCommand;
				m_bIsFavourite	= bIsFavourite;
				m_platfrom		= enumPlatform; 
			}

			/// <summary>
			/// Title getter
			/// </summary>
			public string Title
			{
				get
				{
					return m_strTitle;
				}
			}

			/// <summary>
			/// Launch command getter
			/// </summary>
			public string Launch
			{
				get
				{
					return m_strLaunch;
				}
			}

			/// <summary>
			/// Favourite flag getter
			/// </summary>
			public bool IsFavourite
			{
				get
				{
					return m_bIsFavourite;
				}
			}

			/// <summary>
			/// Platform getter
			/// </summary>
			public GamePlatform Platform
			{
				get
				{
					return m_platfrom;
				}
			}
		}
		
		/// <summary>
		/// Wrapper class for the Game class
		/// The goal is to make the Game class visible to the rest of the client, but make it impossible to create new instances
		/// </summary>
		private class CGameInstance : CGame
		{
			/// <summary>
			/// Constructor.
			/// Call constructor of base class
			/// </summary>
			/// <param name="strTitle">Title of the game</param>
			/// <param name="strLaunchCommand">Game's launch command</param>
			/// <param name="bIsFavourite">Flag indicating if the game is in the favourite tab</param>
			/// <param name="enumPlatform">Game's platform enumerator</param>
			public CGameInstance(string strTitle, string strLaunchCommand, bool bIsFavourite, GamePlatform enumPlatform) 
				: base(strTitle, strLaunchCommand, bIsFavourite, enumPlatform)
			{

			}
		}

		/// <summary>
		/// Create a new instance of Game class
		/// </summary>
		/// <param name="strTitle">Title of the game</param>
		/// <param name="strLaunchCommand">Game's launch command</param>
		/// <param name="bIsFavourite">Flag indicating if the game is in the favourite tab</param>
		/// <param name="enumPlatform">Game's platform enumerator</param>
		/// <returns>Instance of Game</returns>
		private static CGame CreateGameInstance(string strTitle, string strLaunchCommand, bool bIsFavourite, GamePlatform enumPlatform)
		{
			return new CGameInstance(strTitle, strLaunchCommand, bIsFavourite, enumPlatform);
		}

		private static Dictionary<GamePlatform, List<CGame>> m_games = new Dictionary<GamePlatform, List<CGame>>();

		/// <summary>
		/// Return the list of Game objects with specified platform
		/// </summary>
		/// <param name="enumPlatform">Platform enumerator</param>
		/// <returns>List of Game objects</returns>
		public static List<CGame> GetGames(GamePlatform enumPlatform)
		{
			return m_games[enumPlatform];
		}

		/// <summary>
		/// Add game to the dictionary
		/// </summary>
		/// <param name="strTitle">Title of the game</param>
		/// <param name="strLaunchCommand">Game's launch command</param>
		/// <param name="bIsFavourite">Flag indicating if the game is in the favourite tab</param>
		/// <param name="strPlatform">Game's platform as a string value</param>
		public static void AddGame(string strTitle, string strLaunchCommand, bool bIsFavourite, string strPlatform)
		{
			// If platform is incorrect or unsupported, default to custom.
			GamePlatform enumPlatform;
			if(!Enum.TryParse(strPlatform, true, out enumPlatform))
			{
				enumPlatform = GamePlatform.Custom;
			}

			// If this is the first entry in the key, we need to initialise the list
			if(m_games[enumPlatform] == null)
			{
				m_games[enumPlatform] = new List<CGame>();
			}

			m_games[enumPlatform].Add(CreateGameInstance(strTitle, strLaunchCommand, bIsFavourite, enumPlatform));
		}
	}
}
